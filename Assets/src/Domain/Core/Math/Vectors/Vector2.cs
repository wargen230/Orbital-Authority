using System;

namespace OrbitalAuthority.Domain.Core.Math.Vectors
{
    public struct Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new(0, 0);

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(double scalar, Vector2 vector)
        {
            return new Vector2(scalar * vector.X, scalar * vector.Y);
        }

        public static Vector2 operator *(Vector2 vector, double scalar)
        {
            return new Vector2(scalar * vector.X, scalar * vector.Y);
        }

        public static Vector2 operator /(Vector2 vector, double scalar)
        {
            return new Vector2(vector.X / scalar, vector.Y / scalar);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y;
        }

        public double Length()
        {
            return System.Math.Sqrt(LengthSquared());
        }

        public Vector2 Normalize()
        {
            double length = Length();

            if (length < 1e-10)
            {
                return Zero;
            }

            return new Vector2(X / length, Y / length);
        }

        public override string ToString()
        {
            return $"({X:F3}, {Y:F3})";
        }
    }
}
