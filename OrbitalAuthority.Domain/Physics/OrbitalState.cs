using Microsoft.Xna.Framework;
using OrbitalAuthority.Domain.Physics.Interfaces;

namespace OrbitalAuthority.Domain.Physics;

public readonly struct OrbitalState : IState<OrbitalState>
{
    public Vector3 Position { get; }
    public Vector3 Velocity { get; }

    public OrbitalState(Vector3 position, Vector3 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public static OrbitalState Create(Vector3 position, Vector3 velocity)
        => new OrbitalState(position, velocity);

    public OrbitalState Add(OrbitalState other, float scale)
        => new OrbitalState(
            Position + other.Position * scale,
            Velocity + other.Velocity * scale);

    public OrbitalState Scale(float scale)
        => new OrbitalState(
            Position * scale,
            Velocity * scale);
}