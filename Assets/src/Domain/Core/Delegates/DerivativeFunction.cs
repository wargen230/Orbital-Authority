using OrbitalAuthority.Domain.Core.Interfaces.Physics;

namespace OrbitalAuthority.Domain.Core.Delegates
{
    /// <summary>
    /// Делегат для функции вычисления производной.
    /// Принимает состояние, возвращает производную (тоже состояние).
    /// </summary>
    public delegate T DerivativeFunction<T>(T state) where T : IState<T>;
}
