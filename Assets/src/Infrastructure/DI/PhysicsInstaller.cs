using System.Collections.Generic;
using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory;
using OrbitalAuthority.Presentation.MonoBehaviours.Physics;
using UnityEngine;
using Zenject;
using System.Linq;

public class PhysicsInstaller : MonoInstaller
{
    [SerializeField] private List<GameObject> _bodies; // Исправлено название

    public override void InstallBindings()
    {
        var bodiesPhysicable = new List<IPhysicableObject>();

        foreach (var body in _bodies)
        {
            if (body == null) continue;
            
            var physObject = body.GetComponent<PhysicableObject>();
            if (physObject == null)
            {
                Debug.LogError($"GameObject {body.name} does not have PhysicableObject component!");
                continue;
            }
            bodiesPhysicable.Add(physObject);
        }

        if (bodiesPhysicable.Count == 0)
        {
            Debug.LogError("No valid PhysicableObjects found!");
            return;
        }

        var readOnlyBodies = bodiesPhysicable.AsReadOnly();
        var solver = new TrajectorySolverInNewtonianPhysics(readOnlyBodies);
        
        // Регистрируем список тел как синглтон
        Container.Bind<IReadOnlyList<IPhysicableObject>>()
            .WithId("Bodies") // Исправлено название (Bodyes -> Bodies)
            .FromInstance(readOnlyBodies)
            .AsSingle();
        
        // Регистрируем решатель как синглтон
        Container.Bind<ITrajectorySolver<OrbitalState>>()
            .WithId("Newtonian")
            .FromInstance(solver)
            .AsSingle();
        
        // Опционально: регистрируем конкретный тип для прямого доступа
        Container.Bind<TrajectorySolverInNewtonianPhysics>()
            .FromInstance(solver)
            .AsSingle();
    }
}