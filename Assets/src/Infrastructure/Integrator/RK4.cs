using OrbitalAuthority.Domain.Core.Delegates;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;

namespace OrbitalAuthority.Infrastructure.Integrator
{
    public static class RK4Integrator
    {
        /// <summary>
        /// Выполняет один шаг интегрирования методом Рунге-Кутты 4-го порядка.
        /// </summary>
        /// <typeparam name="T">Тип состояния (должен реализовывать IState<T>)</typeparam>
        /// <param name="current">Текущее состояние</param>
        /// <param name="derivativeFunc">Функция вычисления производной</param>
        /// <param name="dt">Шаг по времени</param>
        /// <returns>Новое состояние после шага dt</returns>
        public static T Step<T>(T current, DerivativeFunction<T> derivativeFunc, double dt)
            where T : IState<T>
        {
            // k₁ = f(state)
            T k1 = derivativeFunc(current);
            
            // k₂ = f(state + (dt/2) * k₁)
            T state2 = current.Add(k1, dt * 0.5);
            T k2 = derivativeFunc(state2);
            
            // k₃ = f(state + (dt/2) * k₂)
            T state3 = current.Add(k2, dt * 0.5);
            T k3 = derivativeFunc(state3);
            
            // k₄ = f(state + dt * k₃)
            T state4 = current.Add(k3, dt);
            T k4 = derivativeFunc(state4);
            
            // newState = state + (dt/6) * (k₁ + 2*k₂ + 2*k₃ + k₄)
            T sum = k1
                .Add(k2, 2.0)
                .Add(k3, 2.0)
                .Add(k4, 1.0);
            
            return current.Add(sum, dt / 6.0);
        }
    }
}
