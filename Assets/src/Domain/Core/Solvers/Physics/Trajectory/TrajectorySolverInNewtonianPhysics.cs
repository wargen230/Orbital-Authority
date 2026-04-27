using Vector3 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector3;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Data.Physics;
using OrbitalAuthority.Infrastructure.Integrator;
using System;
using System.Collections.Generic;
using OrbitalAuthority.Domain.Core.Delegates;

namespace OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory
{
    /// <summary>
    /// Решатель траекторий для N тел в рамках ньютоновской гравитации.
    /// Работает с произвольным количеством гравитирующих объектов.
    /// </summary>
    public class TrajectorySolverInNewtonianPhysics : ITrajectorySolver<OrbitalState>
    {
        private readonly IReadOnlyList<IPhysicableObject> bodies;
        private readonly List<Vector3> velocities; // Хранение скоростей для каждого тела

        public TrajectorySolverInNewtonianPhysics(IReadOnlyList<IPhysicableObject> bodies)
        {
            this.bodies = bodies ?? throw new ArgumentNullException(nameof(bodies));
            this.velocities = new List<Vector3>(bodies.Count);
            
            // Инициализируем нулевыми скоростями
            for (int i = 0; i < bodies.Count; i++)
            {
                velocities.Add(Vector3.Zero);
            }
        }

        /// <summary>
        /// Устанавливает скорость для тела.
        /// </summary>
        public void SetVelocity(int bodyIndex, Vector3 velocity)
        {
            if (bodyIndex < 0 || bodyIndex >= velocities.Count)
                throw new ArgumentOutOfRangeException(nameof(bodyIndex));
            
            velocities[bodyIndex] = velocity;
        }

        /// <summary>
        /// Получает скорость тела.
        /// </summary>
        public Vector3 GetVelocity(int bodyIndex)
        {
            if (bodyIndex < 0 || bodyIndex >= velocities.Count)
                throw new ArgumentOutOfRangeException(nameof(bodyIndex));
            
            return velocities[bodyIndex];
        }

        /// <summary>
        /// Вычисляет полное ускорение, действующее на тело с указанным индексом
        /// со стороны всех остальных тел системы.
        /// </summary>
        public Vector3 ComputeAcceleration(int index)
        {
            if (index < 0 || index >= bodies.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            Vector3 totalAcceleration = Vector3.Zero;
            Vector3 currentPosition = bodies[index].Position;

            for (int j = 0; j < bodies.Count; j++)
            {
                if (j == index) continue;

                Vector3 relativePosition = bodies[j].Position - currentPosition;
                double distanceSquared = relativePosition.LengthSquared();
                double distance = System.Math.Sqrt(distanceSquared);
                
                // Избегаем деления на ноль (численное сглаживание)
                if (distance < 1e-10) continue;
                
                // Закон всемирного тяготения: a = G * M / r^2
                double accelerationMagnitude = PhysicsConstants.G * bodies[j].Mass / distanceSquared;
                
                // Направление ускорения (единичный вектор от текущего тела к притягивающему)
                Vector3 direction = relativePosition / distance;
                
                // Добавляем вклад в полное ускорение
                totalAcceleration += direction * accelerationMagnitude;
            }

            return totalAcceleration;
        }

        /// <summary>
        /// Вычисляет производную состояния для указанного тела.
        /// Производная позиции = скорость
        /// Производная скорости = ускорение
        /// </summary>
        public OrbitalState ComputeDerivative(OrbitalState state, int bodyIndex)
        {
            Vector3 acceleration = ComputeAcceleration(bodyIndex);
            return new OrbitalState(state.Velocity, acceleration);
        }

        /// <summary>
        /// Создаёт функцию производной для использования в интеграторе.
        /// </summary>
        private DerivativeFunction<OrbitalState> CreateDerivativeFunction(int bodyIndex)
        {
            return (OrbitalState state) => ComputeDerivative(state, bodyIndex);
        }

        /// <summary>
        /// Выполняет один шаг интегрирования для всех тел методом Рунге-Кутты 4-го порядка.
        /// </summary>
        public void StepAll(double dt)
        {
            // Сохраняем текущие позиции
            List<Vector3> originalPositions = new List<Vector3>(bodies.Count);
            for (int i = 0; i < bodies.Count; i++)
            {
                originalPositions.Add(bodies[i].Position);
            }

            // Создаём состояния для всех тел
            List<OrbitalState> states = new List<OrbitalState>(bodies.Count);
            for (int i = 0; i < bodies.Count; i++)
            {
                states.Add(new OrbitalState(originalPositions[i], velocities[i]));
            }

            // Временно сохраняем новые позиции и скорости
            List<Vector3> newPositions = new List<Vector3>(bodies.Count);
            List<Vector3> newVelocities = new List<Vector3>(bodies.Count);

            // Для каждого тела выполняем шаг интегратора
            for (int i = 0; i < bodies.Count; i++)
            {
                // Создаём локальную копию текущей позиции для вычислений
                Vector3 originalPosition = originalPositions[i];
                Vector3 currentVelocity = velocities[i];
                OrbitalState currentState = new OrbitalState(originalPosition, currentVelocity);
                
                // Создаём функцию производной, которая использует текущие позиции других тел
                DerivativeFunction<OrbitalState> derivativeFunc = (OrbitalState state) => {
                    // Временно обновляем позицию текущего тела для вычисления ускорения
                    Vector3 tempPosition = bodies[i].Position;
                    bodies[i].Position = state.Position;
                    
                    Vector3 acceleration = ComputeAcceleration(i);
                    
                    // Восстанавливаем позицию
                    bodies[i].Position = tempPosition;
                    
                    return new OrbitalState(state.Velocity, acceleration);
                };
                
                // Используем готовый интегратор
                OrbitalState newState = RK4Integrator.Step(currentState, derivativeFunc, dt);
                
                newPositions.Add(newState.Position);
                newVelocities.Add(newState.Velocity);
            }
            
            // Применяем новые позиции и скорости
            for (int i = 0; i < bodies.Count; i++)
            {
                bodies[i].Position = newPositions[i];
                velocities[i] = newVelocities[i];
            }
        }

        /// <summary>
        /// Выполняет один шаг интегрирования для одного тела (сохраняя позиции других неизменными).
        /// </summary>
        public OrbitalState Step(OrbitalState currentState, double dt, int bodyIndex)
        {
            DerivativeFunction<OrbitalState> derivativeFunc = (OrbitalState state) => {
                // Сохраняем оригинальную позицию
                Vector3 originalPosition = bodies[bodyIndex].Position;
                
                // Временно обновляем позицию для вычисления ускорения
                bodies[bodyIndex].Position = state.Position;
                Vector3 acceleration = ComputeAcceleration(bodyIndex);
                
                // Восстанавливаем позицию
                bodies[bodyIndex].Position = originalPosition;
                
                return new OrbitalState(state.Velocity, acceleration);
            };
            
            // Используем готовый интегратор
            return RK4Integrator.Step(currentState, derivativeFunc, dt);
        }

        /// <summary>
        /// Создаёт начальное состояние для тела на орбите.
        /// </summary>
        public OrbitalState CreateOrbitState(Vector3 position, double initialSpeed, double polarAngle, double azimuthalAngle)
        {
            double polarRad = polarAngle * System.Math.PI / 180.0;
            double azimuthalRad = azimuthalAngle * System.Math.PI / 180.0;
    
            Vector3 velocity = new Vector3(
                initialSpeed * System.Math.Cos(polarRad) * System.Math.Cos(azimuthalRad),
                initialSpeed * System.Math.Sin(azimuthalRad),
                initialSpeed * System.Math.Sin(polarRad) * System.Math.Cos(azimuthalRad)
            );

            return new OrbitalState(position, velocity);
        }

        /// <summary>
        /// Вычисляет полную энергию системы.
        /// </summary>
        public double ComputeTotalEnergy()
        {
            double kineticEnergy = 0;
            double potentialEnergy = 0;

            // Кинетическая энергия
            for (int i = 0; i < bodies.Count; i++)
            {
                double speedSquared = velocities[i].LengthSquared();
                kineticEnergy += 0.5 * bodies[i].Mass * speedSquared;
            }

            // Потенциальная энергия
            for (int i = 0; i < bodies.Count; i++)
            {
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    double distance = (bodies[j].Position - bodies[i].Position).Length();
                    if (distance > 1e-10)
                    {
                        potentialEnergy -= PhysicsConstants.G * bodies[i].Mass * bodies[j].Mass / distance;
                    }
                }
            }

            return kineticEnergy + potentialEnergy;
        }

        /// <summary>
        /// Вычисляет центр масс системы.
        /// </summary>
        public Vector3 ComputeCenterOfMass()
        {
            Vector3 centerOfMass = Vector3.Zero;
            double totalMass = 0;

            for (int i = 0; i < bodies.Count; i++)
            {
                centerOfMass += bodies[i].Position * bodies[i].Mass;
                totalMass += bodies[i].Mass;
            }

            return totalMass > 0 ? centerOfMass / totalMass : Vector3.Zero;
        }

        /// <summary>
        /// Вычисляет полный импульс системы.
        /// </summary>
        public Vector3 ComputeTotalMomentum()
        {
            Vector3 totalMomentum = Vector3.Zero;
            
            for (int i = 0; i < bodies.Count; i++)
            {
                totalMomentum += velocities[i] * bodies[i].Mass;
            }
            
            return totalMomentum;
        }
    }
}