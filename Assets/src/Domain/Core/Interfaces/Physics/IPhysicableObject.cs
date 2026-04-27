using OrbitalAuthority.Domain.Core.Math.Vectors;

namespace OrbitalAuthority.Domain.Core.Interfaces.Physics
{
    public interface IPhysicableObject
    {
        Vector3 Position { get; set; }
        double Mass { get; }
        bool UseCustomSpeed { get; }
        double InitialVelocity { get; }
        double InitialPolarAngle { get; }
        double InitialAzimuthalAngle { get; }
    }
}
