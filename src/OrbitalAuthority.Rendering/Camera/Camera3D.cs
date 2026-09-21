using System;
using Microsoft.Xna.Framework;

namespace OrbitalAuthority.Rendering.Camera;

/// <summary>
/// Орбитальная 3D-камера, всегда смотрящая на точку Target.
/// Мировые координаты (метры) хранятся в double — масштаб солнечной системы (~1e11 м)
/// не влезает в точность float. Матрицы и позиции для GPU вычисляются относительно
/// камеры (camera-relative rendering), чтобы не терять точность при кастовании в float.
/// </summary>
public sealed class Camera3D
{
    // Точка, вокруг которой вращается камера (мировые координаты, метры)
    public double TargetX { get; set; }
    public double TargetY { get; set; }
    public double TargetZ { get; set; }

    // Расстояние от камеры до цели (метры)
    public double Distance { get; set; } = 4e11;
    public double MinDistance { get; set; } = 1e6;
    public double MaxDistance { get; set; } = 5e12;

    // Углы орбиты вокруг цели (радианы). Yaw — вращение вокруг Z, Pitch — подъём над плоскостью XY.
    public double Yaw { get; set; }
    public double Pitch { get; set; } = 0.6; // ~34°, приятный обзорный ракурс по умолчанию

    private const double MinPitch = -1.5; // ~ -85.9°, не даём камере встать точно "над полюсом"
    private const double MaxPitch = 1.5;

    public int ViewportWidth { get; set; } = 1280;
    public int ViewportHeight { get; set; } = 720;

    public float FovRadians { get; set; } = MathHelper.ToRadians(60f);

    /// <summary>Мировая позиция камеры (double), вычисленная из Target/Distance/Yaw/Pitch.</summary>
    public (double X, double Y, double Z) WorldPosition
    {
        get
        {
            double cp = Math.Cos(Pitch);
            double ox = Distance * cp * Math.Cos(Yaw);
            double oy = Distance * cp * Math.Sin(Yaw);
            double oz = Distance * Math.Sin(Pitch);
            return (TargetX + ox, TargetY + oy, TargetZ + oz);
        }
    }

    public void ApplyZoom(double factor)
    {
        Distance = Math.Clamp(Distance * factor, MinDistance, MaxDistance);
    }

    public void Rotate(double deltaYaw, double deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch = Math.Clamp(Pitch + deltaPitch, MinPitch, MaxPitch);
    }

    /// <summary>
    /// Базисные векторы камеры (Right/Up/Forward) в мировой ориентации, для построения
    /// лучей в шейдере трассировки — выводятся из тех же Yaw/Pitch, что и WorldPosition,
    /// поэтому луч всегда совпадает с направлением взгляда растеризатора.
    /// </summary>
    public (Vector3 Right, Vector3 Up, Vector3 Forward) GetBasisVectors()
    {
        double cp = Math.Cos(Pitch);

        var forward = new Vector3(
            (float)(-cp * Math.Cos(Yaw)),
            (float)(-cp * Math.Sin(Yaw)),
            (float)(-Math.Sin(Pitch)));

        var right = new Vector3((float)(-Math.Sin(Yaw)), (float)Math.Cos(Yaw), 0f);

        var up = Vector3.Cross(right, forward);
        up.Normalize();

        return (right, up, forward);
    }

    /// <summary>Мировая (double) координата → координата в пространстве рендера относительно камеры (float).</summary>
    public Vector3 ToRelative(double worldX, double worldY, double worldZ)
    {
        var (cx, cy, cz) = WorldPosition;
        return new Vector3(
            (float)(worldX - cx),
            (float)(worldY - cy),
            (float)(worldZ - cz));
    }

    public Matrix GetViewMatrix()
    {
        var (cx, cy, cz) = WorldPosition;
        var targetRelative = new Vector3(
            (float)(TargetX - cx),
            (float)(TargetY - cy),
            (float)(TargetZ - cz));

        return Matrix.CreateLookAt(Vector3.Zero, targetRelative, Vector3.UnitZ);
    }

    public Matrix GetProjectionMatrix()
    {
        float aspect = ViewportHeight > 0 ? (float)ViewportWidth / ViewportHeight : 1f;

        // near/far масштабируются от текущей дистанции: на диапазоне 1e6..5e12 м
        // фиксированная пара near/far либо режет ближние объекты, либо убивает точность глубины.
        float near = Math.Max(0.01f, (float)Distance * 1e-5f);
        float far = Math.Max(near * 10f, (float)Distance * 50f);

        return Matrix.CreatePerspectiveFieldOfView(FovRadians, aspect, near, far);
    }
}
