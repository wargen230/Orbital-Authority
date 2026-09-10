using OrbitalAuthority.Domain.Math.Delegates;
using OrbitalAuthority.Domain.Physics.Interfaces;

namespace OrbitalAuthority.Domain.Math.Integrators.Interfaces;

public interface IIntegrator<TState>
    where TState : IState<TState>
{
    public TState Step(
        TState state,
        float time,
        float deltaT,
        OrbitalAccelerationFunction acceleration);
}