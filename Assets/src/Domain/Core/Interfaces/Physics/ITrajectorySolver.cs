using OrbitalAuthority.Domain.Core.Math.Vectors;
using System.Collections.Generic;

namespace OrbitalAuthority.Domain.Core.Interfaces.Physics
{
    public interface ITrajectorySolver<T> where T : IState<T>
    {
        /// <summary>
        /// Выполняет один шаг интегрирования для всех тел.
        /// </summary>
        void StepAll(double dt);
        
        /// <summary>
        /// Создаёт начальное состояние для тела на орбите.
        /// </summary>
        T CreateOrbitState(Vector3 startPosition, double initialSpeed, double polarAngle, double azimuthalAngle);
        
        /// <summary>
        /// Устанавливает скорость для тела.
        /// </summary>
        void SetVelocity(int bodyIndex, Vector3 velocity);
        
        /// <summary>
        /// Получает скорость тела.
        /// </summary>
        Vector3 GetVelocity(int bodyIndex);
        
        /// <summary>
        /// Вычисляет ускорение для тела с указанным индексом.
        /// </summary>
        Vector3 ComputeAcceleration(int bodyIndex);
        
        /// <summary>
        /// Вычисляет полную энергию системы.
        /// </summary>
        double ComputeTotalEnergy();
        
        /// <summary>
        /// Вычисляет центр масс системы.
        /// </summary>
        Vector3 ComputeCenterOfMass();
    }
}