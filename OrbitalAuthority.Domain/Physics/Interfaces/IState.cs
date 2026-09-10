using Microsoft.Xna.Framework;

namespace OrbitalAuthority.Domain.Physics.Interfaces;

public interface IState<TSelf>
    where TSelf : IState<TSelf>
{
    Vector3 Position { get; }
    Vector3 Velocity { get; }

    TSelf Add(TSelf other, float scale);

    TSelf Scale(float factor);

    static abstract TSelf Create(Vector3 position, Vector3 velocity);
}