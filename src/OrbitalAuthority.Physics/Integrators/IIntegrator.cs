// src/OrbitalAuthority.Physics/Integrators/IIntegrator.cs
using OrbitalAuthority.ECS.Core;

namespace OrbitalAuthority.Physics.Integrators;

public interface IIntegrator
{
    /// <summary>Фаза 1: обновление позиций.</summary>
    void Step(World world, double dt);

    /// <summary>Фаза 2: обновление скоростей (для Verlet). Для Euler — no-op.</summary>
    void CompleteStep(World world, double dt);
}