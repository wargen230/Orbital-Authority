using System;

namespace OrbitalAuthority.Domain.Core.Math.Vectors
{
    public struct Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero => new(0, 0, 0);

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator *(double scalar, Vector3 vector)
        {
            return new Vector3(scalar * vector.X, scalar * vector.Y, scalar * vector.Z);
        }

        public static Vector3 operator *(Vector3 vector, double scalar)
        {
            return new Vector3(scalar * vector.X, scalar * vector.Y, scalar * vector.Z);
        }

        public static Vector3 operator /(Vector3 vector, double scalar)
        {
            return new Vector3(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public double Length()
        {
            return System.Math.Sqrt(LengthSquared());
        }

        public Vector3 Normalize()
        {
            double length = Length();

            if (length < 1e-10)
            {
                return Zero;
            }

            return new Vector3(X / length, Y / length, Z / length);
        }

        public override string ToString()
        {
            return $"({X:F3}, {Y:F3}, {Z:F3})";
        }
    }
}
