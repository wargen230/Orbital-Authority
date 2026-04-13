namespace OrbitalAuthority.Domain.Core.Interfaces.Physics
{
    public interface ITrajectorySolver<T> where T : IState<T>
    {
        T Step(T state, double dt);
        T CreateOrbitState(double distance, double initialSpeed, double angle);
    }
}
