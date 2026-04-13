using OrbitalAuthority.Domain.Core.Interfaces.Physics;
using OrbitalAuthority.Domain.Core.Solvers.Physics.Trajectory;
using OrbitalAuthority.Presentation.MonoBehaviours.Physics;
using UnityEngine;
using Zenject;

public class PhysicsInstaller : MonoInstaller
{
    [SerializeField] private GameObject _earthObject;
    [SerializeField] private GameObject _moonObject;

    public override void InstallBindings()
    {
        var earthPhysicable = _earthObject.GetComponent<PhysicableObject>();
        var moonPhysicable = _moonObject.GetComponent<PhysicableObject>();

        Container.Bind<IPhysicableObject>()
            .WithId("Earth")
            .FromInstance(earthPhysicable)
            .AsTransient();
        
        Container.Bind<IPhysicableObject>()
            .WithId("Moon")
            .FromInstance(moonPhysicable)
            .AsTransient();

        Container.Bind<ITrajectorySolver<OrbitalState>>().
            WithId("Newtonian")
            .To<TrajectorySolverInNewtonianPhysics>()
            .AsSingle();
    }
}
