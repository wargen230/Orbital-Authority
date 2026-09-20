// src/OrbitalAuthority.Physics/Integrators/VelocityVerletIntegrator.cs
using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;

namespace OrbitalAuthority.Physics.Integrators;

public sealed class VelocityVerletIntegrator : IIntegrator
{
    public void Step(World world, double dt)
    {
        int count = world.EntityCount;

        // --- Фаза 1: обновляем позиции, используя текущее ускорение ---
        // x += v*dt + 0.5*a*dt²
        for (int id = 0; id < count; id++)
        {
            if (world.Bodies[id] is not CelestialBodyComponent body) continue;
            if (body.IsStatic) continue;

            if (world.Transforms[id] is not Transform3D t) continue;
            if (world.Velocities[id] is not Velocity3D v) continue;
            if (world.Forces[id] is not ForceAccumulator f) continue;

            t.X += v.Vx * dt + 0.5 * f.Ax * dt * dt;
            t.Y += v.Vy * dt + 0.5 * f.Ay * dt * dt;
            t.Z += v.Vz * dt + 0.5 * f.Az * dt * dt;

            world.Transforms[id] = t;
        }
    }

    /// <summary>Вторая фаза: вызывается ПОСЛЕ пересчёта сил на новых позициях.</summary>
    public void CompleteStep(World world, double dt)
    {
        int count = world.EntityCount;

        for (int id = 0; id < count; id++)
        {
            if (world.Bodies[id] is not CelestialBodyComponent body) continue;
            if (body.IsStatic) continue;

            if (world.Velocities[id] is not Velocity3D v) continue;
            if (world.Forces[id] is not ForceAccumulator f) continue;

            // v += 0.5*(a_old + a_new)*dt
            v.Vx += 0.5 * (f.PrevAx + f.Ax) * dt;
            v.Vy += 0.5 * (f.PrevAy + f.Ay) * dt;
            v.Vz += 0.5 * (f.PrevAz + f.Az) * dt;

            world.Velocities[id] = v;
        }
    }
}