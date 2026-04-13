namespace OrbitalAuthority.Domain.Core.Interfaces.Physics
{
    public interface IState<T> where T : IState<T>
    {
        T Add(T other, double scale = 1.0);
        T Scale(double factor);
        T Clone();
    }
}
