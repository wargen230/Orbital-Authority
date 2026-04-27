using System;
using System.Collections.Generic;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory;
using OrbitalAuthority.Domain.Data.Physics;
using UnityEngine;
using Zenject;
using Vector3 = OrbitalAuthority.Domain.Core.Math.Vectors.Vector3;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics
{
    public class NBodyOrbitController : MonoBehaviour
    {
        [Inject(Id = "Newtonian")] private ITrajectorySolver<OrbitalState> nBodySolver;
        [Inject(Id = "Bodies")] private IReadOnlyList<IPhysicableObject> bodies;        
        private List<OrbitalState> currentStates = new();
        private List<Vector3> currentVelocities = new();
        
        [SerializeField] private double dt = 60.0; // базовый шаг симуляции в секундах
        [SerializeField] private double simulationSpeedMultiplier = 1.0; // множитель скорости симуляции
        [SerializeField] private double distanceScale = 1;
        [SerializeField] private List<Transform> bodyTransforms;
        [SerializeField] private bool useCustomInitialVelocities = false;

        void Start()
        {
            InitializeStates();
            InitializeVelocities();
        }

        void InitializeStates()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                
                Vector3 startPosition = new Vector3(
                    bodyTransforms[i].position.x * distanceScale, 
                    bodyTransforms[i].position.y * distanceScale, 
                    bodyTransforms[i].position.z * distanceScale
                );

                double startVelocity;
                if (body.UseCustomSpeed)
                    startVelocity = body.InitialVelocity;
                else
                    startVelocity = 0;

                OrbitalState initialState = nBodySolver.CreateOrbitState(
                    startPosition, 
                    startVelocity, 
                    body.InitialPolarAngle, 
                    body.InitialAzimuthalAngle
                );
                
                currentStates.Add(initialState);
                
                // Обновляем позицию тела в соответствии с состоянием
                bodies[i].Position = startPosition;
            }
        }

        void InitializeVelocities()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                Vector3 initialVelocity;
                
                if (useCustomInitialVelocities && i < currentStates.Count)
                {
                    initialVelocity = currentStates[i].Velocity;
                }
                else
                {
                    // Инициализируем нулевой скоростью
                    initialVelocity = Vector3.Zero;
                }
                
                currentVelocities.Add(initialVelocity);
                nBodySolver.SetVelocity(i, initialVelocity);
            }
        }

        void Update()
        {
            // Выполняем шаг симуляции для всех тел одновременно
            double stepTime = dt * simulationSpeedMultiplier;
            nBodySolver.StepAll(stepTime);
            
            // Обновляем позиции всех тел в Unity
            for (int i = 0; i < bodies.Count; i++)
            {
                Vector3 newPosition = bodies[i].Position;
                UpdateBodyPosition(i, newPosition);
                
                // Обновляем сохранённые скорости (если нужно для отладки)
                currentVelocities[i] = nBodySolver.GetVelocity(i);
                
                // Обновляем состояние для возможного использования в других местах
                currentStates[i] = new OrbitalState(newPosition, currentVelocities[i]);
            }
        }

        private void UpdateBodyPosition(int index, Vector3 position)
        {
            if (index < bodyTransforms.Count && bodyTransforms[index] != null)
            {
                bodyTransforms[index].position = new UnityEngine.Vector3(
                    (float)(position.X / distanceScale), 
                    (float)(position.Y / distanceScale), 
                    (float)(position.Z / distanceScale)
                );
            }
        }

        void ResetSimulation()
        {
            InitializeStates();
            InitializeVelocities();
            
            // Принудительно обновляем позиции
            for (int i = 0; i < bodies.Count; i++)
            {
                UpdateBodyPosition(i, bodies[i].Position);
            }
        }

        // Метод для добавления возмущения к определённому телу
        public void AddImpulse(int bodyIndex, Vector3 impulse)
        {
            if (bodyIndex >= 0 && bodyIndex < bodies.Count)
            {
                Vector3 currentVelocity = nBodySolver.GetVelocity(bodyIndex);
                Vector3 newVelocity = currentVelocity + impulse;
                nBodySolver.SetVelocity(bodyIndex, newVelocity);
                currentVelocities[bodyIndex] = newVelocity;
            }
        }

        // Метод для получения текущей скорости тела (для отладки)
        public Vector3 GetBodyVelocity(int bodyIndex)
        {
            if (bodyIndex >= 0 && bodyIndex < bodies.Count)
            {
                return nBodySolver.GetVelocity(bodyIndex);
            }
            return Vector3.Zero;
        }

        // Метод для получения текущей кинетической энергии системы
        public double GetTotalKineticEnergy()
        {
            double totalEnergy = 0;
            for (int i = 0; i < bodies.Count; i++)
            {
                double speedSquared = nBodySolver.GetVelocity(i).LengthSquared();
                totalEnergy += 0.5 * bodies[i].Mass * speedSquared;
            }
            return totalEnergy;
        }

        // Метод для получения центра масс системы
        public UnityEngine.Vector3 GetCenterOfMass()
        {
            Vector3 centerOfMass = nBodySolver.ComputeCenterOfMass();
            return new UnityEngine.Vector3((float)centerOfMass.X, (float)centerOfMass.Y, (float)centerOfMass.Z);
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || bodies == null) return;
            
            // Рисуем центр масс
            UnityEngine.Vector3 centerOfMass = GetCenterOfMass();
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(centerOfMass, 0.5f);
            
            // Рисуем линии между телами для визуализации гравитационных связей
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            for (int i = 0; i < bodies.Count; i++)
            {
                for (int j = i + 1; j < bodies.Count; j++)
                {
                    if (i < bodyTransforms.Count && j < bodyTransforms.Count)
                    {
                        Gizmos.DrawLine(bodyTransforms[i].position, bodyTransforms[j].position);
                    }
                }
            }
        }
    }
}