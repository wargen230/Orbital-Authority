// src/OrbitalAuthority.Physics/PhysicsPipeline.cs
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Data;
using OrbitalAuthority.Physics.Integrators;
using OrbitalAuthority.ECS.Components.PhysicsComponents;

namespace OrbitalAuthority.Physics;

public sealed class PhysicsPipeline
{
    private readonly World _world;
    private readonly IIntegrator _integrator;
    private readonly IGravityField _gravity;

    public PhysicsPipeline(World world, IIntegrator integrator, IGravityField gravity)
    {
        _world = world;
        _integrator = integrator;
        _gravity = gravity;
    }

    public void Tick(double dt)
    {
        // a(t) — ускорение на начало тика (осталось от конца предыдущего тика).
        SavePreviousAcceleration();

        ClearForces();
        AccumulateGravity();
        ComputeAccelerations();

        _integrator.Step(_world, dt);

        // a(t+dt) — ускорение уже на новых позициях, для второй половины Verlet.
        ClearForces();
        AccumulateGravity();
        ComputeAccelerations();

        _integrator.CompleteStep(_world, dt);
    }

    private void SavePreviousAcceleration()
    {
        for (int id = 0; id < _world.EntityCount; id++)
        {
            if (_world.Forces[id] is not ForceAccumulator f) continue;
            f.PrevAx = f.Ax;
            f.PrevAy = f.Ay;
            f.PrevAz = f.Az;
            _world.Forces[id] = f;
        }
    }

    private void ClearForces()
    {
        for (int id = 0; id < _world.Forces.Length; id++)
        {
            // Обнуляем силу/ускорение текущего шага, но PrevAx/Ay/Az нужно
            // сохранить нетронутым до конца тика — иначе Verlet теряет a(t).
            double prevAx = 0, prevAy = 0, prevAz = 0;
            if (_world.Forces[id] is ForceAccumulator existing)
            {
                prevAx = existing.PrevAx;
                prevAy = existing.PrevAy;
                prevAz = existing.PrevAz;
            }

            _world.Forces[id] = new ForceAccumulator
            {
                PrevAx = prevAx,
                PrevAy = prevAy,
                PrevAz = prevAz,
            };
        }
    }

    private void AccumulateGravity()
    {
        for(int id = 0; id < _world.Masses.Length; id++)
        {
            if(_world.Masses[id] is null) continue;

            if (_world.Transforms[id] is null) continue;

            if(_world.Forces[id] is null) continue;

            var t = _world.Transforms[id]!.Value;

            var m = _world.Masses[id]!.Value.Mass;

            // Ускорение от всех остальных тел
            var a = _gravity.ComputeAcceleration(_world, t.X, t.Y, t.Z, excludeEntity: id);

            // F = m * a
            var f = _world.Forces[id]!.Value;
            f.Fx += a.X * m;
            f.Fy += a.Y * m;
            f.Fz += a.Z * m;

            _world.Forces[id] = f;
        }
    }

    /// <summary>Переводит накопленную силу в ускорение (a = F / m) для интеграторов.</summary>
    private void ComputeAccelerations()
    {
        for (int id = 0; id < _world.Masses.Length; id++)
        {
            if (_world.Masses[id] is not MassComponent m) continue;
            if (_world.Forces[id] is not ForceAccumulator f) continue;

            f.Ax = f.Fx / m.Mass;
            f.Ay = f.Fy / m.Mass;
            f.Az = f.Fz / m.Mass;

            _world.Forces[id] = f;
        }
    }
}