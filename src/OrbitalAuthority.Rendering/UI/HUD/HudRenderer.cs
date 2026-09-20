// src/OrbitalAuthority.Rendering/Renderers/HudRenderer.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitalAuthority.Gameplay.Utils;

namespace OrbitalAuthority.Rendering.UI.HUD;

public sealed class HudRenderer
{
    private readonly SpriteFont _font;
    private readonly SimulationClock _clock;
    private Texture2D _pixel;

    public HudRenderer(GraphicsDevice device, SpriteFont font, SimulationClock clock)
    {
        _font = font;
        _clock = clock;

        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight, int fps)
    {
        // Полупрозрачная подложка
        var panel = new Rectangle(10, 10, 280, 110);
        spriteBatch.Draw(_pixel, panel, new Color(0, 0, 0, 180));

        // Текст
        string line1 = $"Time: {_clock.Days:F2} days ({_clock.Years:F3} y)";
        string line2 = $"Speed: {_clock.TimeScale:F2}x  " +
                       $"({(_clock.IsPaused ? "PAUSED" : "RUNNING")})";
        string line3 = $"FPS: {fps}";
        string line4 = "[Space] pause  [[/]] speed  [R] reset";

        Color fpsColor = fps >= 55 ? Color.LightGreen : fps >= 30 ? Color.Yellow : Color.Red;

        spriteBatch.DrawString(_font, line1, new Vector2(20, 20), Color.White);
        spriteBatch.DrawString(_font, line2, new Vector2(20, 40), Color.Yellow);
        spriteBatch.DrawString(_font, line3, new Vector2(20, 60), fpsColor);
        spriteBatch.DrawString(_font, line4, new Vector2(20, 85), Color.Gray);
    }
}