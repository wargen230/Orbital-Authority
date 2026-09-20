
using OrbitalAuthority.ECS.Core;

namespace OrbitalAuthority.Physics.Integrators;

public class SymplecticEulerIntegrator : IIntegrator
{
    public void CompleteStep(World world, double dt)
    {
        
    }

    public void Step(World world, double dt)
    {
        var transforms = world.Transforms;
        var velocities = world.Velocities;
        var bodies = world.Bodies;
        var forces = world.Forces;

        for(int id = 0; id < world.Transforms.Length; id++)
        {
            // Статичные тела (Земля, Солнце) не двигаются
            if (bodies[id] is null)
                continue;

            if(bodies[id]!.Value.IsStatic)
                continue;

            if(transforms[id] is null) continue;
            if (velocities[id] is null) continue;
            if (forces[id] is null) continue;

            // Ускорение (Ax/Ay/Az) уже посчитано пайплайном из накопленной силы.
            var f = forces[id]!.Value;
            var v = velocities[id]!.Value;

            // v += a * dt
            v.Vx += f.Ax * dt;
            v.Vy += f.Ay * dt;
            v.Vz += f.Az * dt;

            // x += v * dt
            var t = transforms[id]!.Value;
            t.X += v.Vx * dt;
            t.Y += v.Vy * dt;
            t.Z += v.Vz * dt;

            transforms[id] = t;
            velocities[id] = v;
        }
    }
}