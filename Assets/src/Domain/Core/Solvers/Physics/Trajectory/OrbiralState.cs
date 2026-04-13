using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Core.Math.Vectors;

namespace OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory
{
    public class OrbitalState : IState<OrbitalState>
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public OrbitalState(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public OrbitalState Add(OrbitalState other, double scale = 1.0)
        {
            return new OrbitalState(
                Position + other.Position * scale,
                Velocity + other.Velocity * scale
            );
        }

        public OrbitalState Scale(double factor)
        {
            return new OrbitalState(
                Position * factor,
                Velocity * factor
            );
        }

        public OrbitalState Clone()
        {
            return new OrbitalState(Position, Velocity);
        }

        public override string ToString()
        {
            return $"Pos=({Position.X:F0}, {Position.Y:F0}) Vel=({Velocity.X:F1}, {Velocity.Y:F1})";
        }
    }
}
