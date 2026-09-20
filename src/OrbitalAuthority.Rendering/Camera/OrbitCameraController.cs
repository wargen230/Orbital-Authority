using Microsoft.Xna.Framework.Input;

namespace OrbitalAuthority.Rendering.Camera;

/// <summary>
/// Управление орбитальной камерой.
/// ЛКМ намеренно не используется — она нужна выпадающему списку объектов (UI).
/// Вращение — средняя кнопка мыши, зум — колесо, панорамирование цели — WASD/Home.
/// </summary>
public sealed class OrbitCameraController
{
    private readonly Camera3D _camera;

    private MouseState _prevMouse;
    private KeyboardState _prevKeyboard;

    /// <summary>Радиан поворота на пиксель движения мыши.</summary>
    public double RotateSpeed { get; set; } = 0.005;

    /// <summary>Доля текущей дистанции камеры, на которую сдвигается цель за кадр при зажатой клавише.</summary>
    public double PanSpeed { get; set; } = 0.02;

    public double ZoomStep { get; set; } = 1.15;

    public OrbitCameraController(Camera3D camera)
    {
        _camera = camera;
    }

    /// <summary>
    /// Обновляет камеру за кадр. Возвращает true, если было ручное панорамирование цели
    /// (WASD/Home) — это должно отменять следование за объектом. Вращение вокруг цели
    /// и зум следование не отменяют.
    /// </summary>
    public bool Update(MouseState mouse, KeyboardState keyboard)
    {
        bool panned = false;

        // --- Зум колёсиком ---
        int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            double factor = scrollDelta > 0 ? 1.0 / ZoomStep : ZoomStep;
            _camera.ApplyZoom(factor);
        }

        // --- Вращение вокруг цели средней кнопкой мыши ---
        if (mouse.MiddleButton == ButtonState.Pressed)
        {
            int dx = mouse.X - _prevMouse.X;
            int dy = mouse.Y - _prevMouse.Y;
            if (dx != 0 || dy != 0)
            {
                _camera.Rotate(-dx * RotateSpeed, dy * RotateSpeed);
            }
        }

        // --- Панорамирование цели WASD, в плоскости XY относительно направления камеры ---
        double panX = 0, panY = 0;
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) panY += 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) panY -= 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) panX -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) panX += 1;

        if (panX != 0 || panY != 0)
        {
            double speed = _camera.Distance * PanSpeed;

            double sinYaw = System.Math.Sin(_camera.Yaw);
            double cosYaw = System.Math.Cos(_camera.Yaw);

            // "Вперёд" — проекция направления взгляда камеры на плоскость XY; "вправо" — перпендикуляр к ней.
            double forwardX = -cosYaw;
            double forwardY = -sinYaw;
            double rightX = -sinYaw;
            double rightY = cosYaw;

            _camera.TargetX += (forwardX * panY + rightX * panX) * speed;
            _camera.TargetY += (forwardY * panY + rightY * panX) * speed;
            panned = true;
        }

        // --- Центр в (0,0,0) — Home ---
        if (keyboard.IsKeyDown(Keys.Home) && !_prevKeyboard.IsKeyDown(Keys.Home))
        {
            _camera.TargetX = 0;
            _camera.TargetY = 0;
            _camera.TargetZ = 0;
            panned = true;
        }

        _prevMouse = mouse;
        _prevKeyboard = keyboard;

        return panned;
    }
}
