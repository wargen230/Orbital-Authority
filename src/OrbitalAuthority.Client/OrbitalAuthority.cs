using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitalAuthority.Data.Definitions;
using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Gameplay.Utils;
using OrbitalAuthority.Physics;
using OrbitalAuthority.Rendering.Camera;
using OrbitalAuthority.Rendering.Renderers;
using OrbitalAuthority.Rendering.UI.HUD;
using OrbitalAuthority.Rendering.Utils;

namespace OrbitalAuthority.Client;

public class OrbitalAuthorityGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly IServiceProvider _provider;

    private SpriteBatch _spriteBatch = null!;
    private SpriteFont _font = null!;
    private World _world = null!;
    private PhysicsPipeline _physics = null!;

    private Camera3D _camera = null!;
    private OrbitCameraController _cameraController = null!;
    private CelestialBodyRenderer3D _bodyRenderer = null!;
    private HudRenderer _hudRenderer = null!;
    private ObjectListRenderer _objectListRenderer = null!;

    private SimulationClock _clock = new();
    private SimulationClockController _clockController = null!;

    private double _lastLogTime;
    private int _fps;
    private int _frameCount;
    private double _fpsTimer;

    // Состояние ввода
    private bool _prevTab;
    private bool _prevEscape;
    private MouseState _prevMouseState;

    // Следование за объектом. -1 = свободная камера.
    private int _followEntityId = -1;

    // Замеры производительности
    private double _updateMs;
    private double _physicsMs;
    private double _drawMs;

    public OrbitalAuthorityGame(IServiceProvider provider)
    {
        _provider = provider;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1600,
            PreferredBackBufferHeight = 900,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void Initialize()
    {
        _world = _provider.GetRequiredService<World>();
        _physics = _provider.GetRequiredService<PhysicsPipeline>();

        _camera = new Camera3D
        {
            ViewportWidth = _graphics.PreferredBackBufferWidth,
            ViewportHeight = _graphics.PreferredBackBufferHeight,
        };

        _cameraController = new OrbitCameraController(_camera);
        _clockController = new SimulationClockController(_clock);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");

        _bodyRenderer = new CelestialBodyRenderer3D(GraphicsDevice, _world);
        _hudRenderer = new HudRenderer(GraphicsDevice, _font, _clock);
        _objectListRenderer = new ObjectListRenderer(GraphicsDevice, _font);

        // ✅ Сканирование ОДИН РАЗ при загрузке контента.
        var items = WorldScanner.Scan(_world);
        _objectListRenderer.SetItems(items);
    }

    protected override void Update(GameTime gameTime)
    {
        var swTotal = System.Diagnostics.Stopwatch.StartNew();

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        // --- Управление временем ---
        _clockController.Update(keyboard);

        // --- Навигация по списку (Tab) ---
        bool tab = keyboard.IsKeyDown(Keys.Tab);
        if (tab && !_prevTab)
        {
            _objectListRenderer.SelectNext();
            _followEntityId = _objectListRenderer.SelectedEntityId;
        }
        _prevTab = tab;

        // --- Отмена следования (Esc) ---
        bool escape = keyboard.IsKeyDown(Keys.Escape);
        if (escape && !_prevEscape)
        {
            _followEntityId = -1;
            _objectListRenderer.ClearSelection();
        }
        _prevEscape = escape;

        // --- Выпадающий список объектов: наведение/разворачивание/клик ---
        bool listSelectionChanged = _objectListRenderer.HandleInput(
            mouse, _prevMouseState,
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);
        if (listSelectionChanged)
        {
            _followEntityId = _objectListRenderer.SelectedEntityId;
        }
        _prevMouseState = mouse;

        // --- Камера: пан/зум всегда активны; ручной пан отменяет следование ---
        bool cameraPanned = _cameraController.Update(mouse, keyboard);
        if (cameraPanned)
        {
            _followEntityId = -1;
        }

        // --- Симуляция ---
        var swPhysics = System.Diagnostics.Stopwatch.StartNew();

        if (!_clock.IsPaused)
        {
            double effectiveDt = _clock.Dt * _clock.TimeScale;

            // ✅ Правильное разделение:
            // - MaxStableDt — максимальный безопасный dt для интегратора.
            // - MaxTicksPerFrame — жёсткий клэмп по количеству тиков.
            int ticks = Math.Max(1, (int)Math.Ceiling(effectiveDt / PhysicsConstants.MaxStableDt));
            ticks = Math.Min(ticks, PhysicsConstants.MaxTicksPerFrame);

            double dtPerTick = effectiveDt / ticks;

            for (int i = 0; i < ticks; i++)
            {
                _physics.Tick(dtPerTick);
            }

            _clock.Advance(ticks, dtPerTick);
        }

        swPhysics.Stop();
        _physicsMs = swPhysics.Elapsed.TotalMilliseconds;

        // --- Следование за выбранным объектом ---
        // Жёсткая привязка к центру, без сглаживания: иначе на быстром движении
        // (высокий TimeScale) камера отстаёт от объекта и он "уезжает" в сторону.
        if (_followEntityId >= 0 &&
            _world.Transforms[_followEntityId] is Transform3D t)
        {
            _camera.TargetX = t.X;
            _camera.TargetY = t.Y;
            _camera.TargetZ = t.Z;
        }

        // --- Лог раз в секунду ---
        if (gameTime.TotalGameTime.TotalSeconds - _lastLogTime > 1.0)
        {
            _lastLogTime = gameTime.TotalGameTime.TotalSeconds;
            Console.WriteLine(
                $"Update={_updateMs:F1}ms Physics={_physicsMs:F1}ms Draw={_drawMs:F1}ms " +
                $"Ticks={_clock.TimeScale:F1}x Days={_clock.Days:F2}");
        }

        swTotal.Stop();
        _updateMs = swTotal.Elapsed.TotalMilliseconds;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // --- FPS: считаем реальные вызовы Draw за секунду, а не мгновенный dt ---
        _frameCount++;
        _fpsTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_fpsTimer >= 1.0)
        {
            _fps = _frameCount;
            _frameCount = 0;
            _fpsTimer -= 1.0;
        }

        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(5, 5, 15), 1f, 0);

        _bodyRenderer.Draw(_camera);

        _spriteBatch.Begin();

        _hudRenderer.Draw(
            _spriteBatch,
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight,
            _fps);

        _objectListRenderer.Draw(
            _spriteBatch,
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight);

        _spriteBatch.End();

        sw.Stop();
        _drawMs = sw.Elapsed.TotalMilliseconds;

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _bodyRenderer?.Dispose();
        _objectListRenderer?.Dispose();
        base.UnloadContent();
    }
}