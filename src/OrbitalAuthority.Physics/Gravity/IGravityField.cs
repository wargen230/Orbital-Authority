using OrbitalAuthority.Core.Math;
using OrbitalAuthority.ECS.Core;

public interface IGravityField
{
    Vector3D ComputeAcceleration(World world, double x, double y, double z, int excludeEntity = -1);
}