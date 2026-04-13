using System;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory;
using OrbitalAuthority.Domain.Data.Physics;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace OrbitalAuthority.Presentation.MonoBehaviours.Physics
{
    public class MoonOrbitController : MonoBehaviour
    {
        [Inject(Id = "Newtonian")] private ITrajectorySolver<OrbitalState> earthMoonSolver;
        [Inject(Id = "Moon")] private IPhysicableObject moon;
        [Inject(Id = "Earth")] private IPhysicableObject earth;
        
        private OrbitalState currentState;
        [SerializeField] private double dt = 60.0; // базовый шаг симуляции в секундах
        [SerializeField] private double simulationSpeedMultiplier = 1.0; // множитель скорости симуляции

        [SerializeField] private Transform moonTransform;

        void Start()
        {
            Vector3 startPosition = moonTransform.position;
            
            // Расстояние в 3D пространстве (а не только XZ)
            double distance = Math.Sqrt(
                startPosition.x * startPosition.x +
                startPosition.y * startPosition.y +
                startPosition.z * startPosition.z
            );

            double startVelocity;
            if (moon.UseCustomSpeed)
                startVelocity = moon.InitialVelocity;
            else
                startVelocity = Math.Sqrt(PhysicsConstants.G * earth.Mass / distance);

            currentState = earthMoonSolver.CreateOrbitState(
                distance, 
                startVelocity, 
                moon.InitialPolarAngle, 
                moon.InitialAzimuthalAngle
            );
        }

        void Update()
        {
            currentState = earthMoonSolver.Step(
                currentState,
                dt * simulationSpeedMultiplier * Time.deltaTime
            );
            
            UpdateMoonPosition(currentState.Position);
        }

       private void UpdateMoonPosition(Domain.Core.Math.Vectors.Vector3 position)
        {
            moonTransform.position = new Vector3((float)position.X, (float)position.Y, (float)position.Z);
        }
    }
}