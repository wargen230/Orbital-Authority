using OrbitalAuthority.Domain.Core.Math.Vectors;

namespace OrbitalAuthority.Domain.Core.Interfaces.Physics
{
    public interface IPhysicableObject
    {
        double Mass { get; }
        bool UseCustomSpeed { get; }
        Vector2 InitialVelocity { get; }
    }
}
