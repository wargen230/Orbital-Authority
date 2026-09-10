using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using OrbitalAuthority.Domain.Math.Delegates;
using OrbitalAuthority.Domain.Math.Integrators.Interfaces;
using OrbitalAuthority.Domain.Physics.Interfaces;

namespace OrbitalAuthority.Domain.Math.Integrators;

public class VerleIntegrator<TState> : IIntegrator<TState>
    where TState : IState<TState>
{
    public TState Step(TState state, float time, float deltaT, OrbitalAccelerationFunction acceleration)
    {
        Vector3 a0 = acceleration(state.Position, time);

        Vector3 nextPosition = state.Position + state.Velocity * deltaT + 0.5f * a0 * deltaT * deltaT;

        Vector3 a1 = acceleration(state.Position, time + deltaT);

        Vector3 nextVelocity = state.Velocity + 0.5f * (a0 + a1) * deltaT;

        return TState.Create(nextPosition, nextVelocity);
    }
}