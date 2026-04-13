using Vector2 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector2;
using Vector3 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector3;
using Zenject;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Data.Physics;
using OrbitalAuthority.Infrastructure.Integrator;

namespace OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory
{
    /// <summary>
    /// Решатель траекторий для двух тел в рамках ньютоновской гравитации.
    /// Работает с любыми двумя объектами, где один притягивает другой.
    /// </summary>
    public class TrajectorySolverInNewtonianPhysics : ITrajectorySolver<OrbitalState>
    {
        private readonly IPhysicableObject attractor;   // притягивающее тело (например, Земля)
        private readonly IPhysicableObject satellite;   // спутник (например, Луна)

        public TrajectorySolverInNewtonianPhysics(
            [Inject(Id = "Earth")] IPhysicableObject attractor,
            [Inject(Id = "Moon")] IPhysicableObject satellite
        )
        {
            this.attractor = attractor;
            this.satellite = satellite;
        }

        /// <summary>
        /// Вычисляет ускорение, действующее на спутник со стороны притягивающего тела.
        /// Позиция — это вектор от притягивающего тела до спутника.
        /// </summary>
        public Vector3 ComputeAcceleration(Vector3 position)
        {
            double r = position.Length();

            // Защита от деления на ноль
            if (r < 1.0)
                return new Vector3(0, 0, 0);

            double r3 = r * r * r;

            // a = -G * M_attractor / r³ * r_vec
            double factor = -PhysicsConstants.G * attractor.Mass / r3;

            return new Vector3(position.X * factor, position.Y * factor, position.Z * factor);
        }

        /// <summary>
        /// Вычисляет производную состояния.
        /// Производная позиции = скорость
        /// Производная скорости = ускорение
        /// </summary>
        public OrbitalState ComputeDerivative(OrbitalState state)
        {
            Vector3 acceleration = ComputeAcceleration(state.Position);
            
            return new OrbitalState(state.Velocity, acceleration);
        }

        /// <summary>
        /// Выполняет один шаг интегрирования методом Рунге-Кутты 4-го порядка.
        /// </summary>
        public OrbitalState Step(OrbitalState currentState, double dt)
        {
            return RK4Integrator.Step(currentState, ComputeDerivative, dt);
        }

        /// <summary>
        /// Создаёт начальное состояние для круговой орбиты вокруг притягивающего тела.
        /// </summary>
        public OrbitalState CreateOrbitState(double distance, double initialSpeed, double polarAngle, double azimuthalAngle)
        {
            double polarRad = polarAngle * System.Math.PI / 180.0;
            double azimuthalRad = azimuthalAngle * System.Math.PI / 180.0;

            double sinPolar = System.Math.Sin(polarRad);
            double cosPolar = System.Math.Cos(polarRad);
            double sinAzimuth = System.Math.Sin(azimuthalRad);
            double cosAzimuth = System.Math.Cos(azimuthalRad);

            // Позиция сразу в Unity-координатах:
            // X = distance * sin(polar) * cos(azimuth)
            // Y = distance * cos(polar)  — вертикаль (высота)
            // Z = distance * sin(polar) * sin(azimuth)
            Vector3 position = new Vector3(
                distance * sinPolar * cosAzimuth,
                distance * cosPolar,
                distance * sinPolar * sinAzimuth
            );

            // Скорость — касательная в горизонтальной плоскости XZ (если polar = 90°, то круговая орбита в XZ)
            // Вектор скорости: (-sinφ, 0, cosφ) * speed
            // Если нужны наклонные орбиты, добавьте компоненту Y: ( -sinφ * cosPolar, sinPolar, cosφ * cosPolar ) * speed
            Vector3 velocity = new Vector3(
                initialSpeed * (-sinAzimuth),
                0,
                initialSpeed * cosAzimuth
            );

            return new OrbitalState(position, velocity);
        }    
    }
}