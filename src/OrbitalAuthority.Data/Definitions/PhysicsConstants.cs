namespace OrbitalAuthority.Data.Definitions;

public static class PhysicsConstants
{
    public const double G = 6.67430e-11;

    public const double MaxStableDt = 3600.0;  // 1 час — безопасно для Symplectic Euler
    public const int MaxTicksPerFrame = 500;

}