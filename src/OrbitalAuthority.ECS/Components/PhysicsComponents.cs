namespace OrbitalAuthority.ECS.Components.PhysicsComponents;

public struct Transform3D
{
    public double X;
    public double Y;
    public double Z;
}

public struct Velocity3D
{
    public double Vx;
    public double Vy;
    public double Vz;
}

public struct MassComponent
{
    public double Mass;
    public double Radius;
}

public struct CelestialBodyComponent
{
    public CelestialBodyType Type;   // Earth, Moon, Sun, Asteroid
    public bool IsStatic;            // Земля/Солнце — статичны, Луна — нет
}

public enum CelestialBodyType : byte
{
    Sun = 0,
    Earth = 1,
    Moon = 2,
    Asteroid = 3,
}

public struct ForceAccumulator
{
    public double Fx, Fy, Fz;
    public double Ax, Ay, Az;
    public double PrevAx, PrevAy, PrevAz;
    public bool IsInitialized;
}