using OrbitalAuthority.Core.Math;
using OrbitalAuthority.Data.Definitions;
using OrbitalAuthority.ECS.Core;

public class NewtonianGravityField : IGravityField
{
        public Vector3D ComputeAcceleration(World world, double x, double y, double z, int excludeEntity = -1)
    {
        double ax = 0, ay = 0, az = 0;

        for(int id = 0; id < world.Masses.Length; id++)
        {
            if (id == excludeEntity) 
                continue;

            if(world.Masses[id] is null)
                continue;
            
            if(world.Transforms[id] is null)
                continue;

            var m = world.Masses[id]!.Value.Mass;
            var t = world.Transforms[id]!.Value;

            var dx = t.X - x;
            var dy = t.Y - y;
            var dz = t.Z - z;
            var r2 = dx * dx + dy * dy + dz * dz;
            if (r2 < 1e-9) continue; // защита от деления на ноль

            var invR = 1.0 / System.Math.Sqrt(r2);
            var invR3 = invR / r2;
            var factor = PhysicsConstants.G * m * invR3;

            ax += factor * dx;
            ay += factor * dy;
            az += factor * dz;
        }

        return new Vector3D(ax, ay, az);
    }
}