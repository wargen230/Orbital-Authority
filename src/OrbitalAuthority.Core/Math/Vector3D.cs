// src/OrbitalAuthority.Core/Math/Vector2D.cs
namespace OrbitalAuthority.Core.Math;

public readonly struct Vector3D
{
    public readonly double X;
    public readonly double Y;
    public readonly double Z;

    public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3D operator *(Vector3D a, double s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3D operator /(Vector3D a, double s) => new(a.X / s, a.Y / s, a.Z / s);

    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3D Normalized()
    {
        var len = Length;
        return len > 1e-15 ? new Vector3D(X / len, Y / len, Z / len) : default;
    }

    public static double Distance(Vector3D a, Vector3D b) => (a - b).Length;
}