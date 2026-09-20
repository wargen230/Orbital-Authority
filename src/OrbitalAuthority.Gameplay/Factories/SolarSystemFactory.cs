using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;

namespace OrbitalAuthority.Gameplay;

public class SolarSystemFactory
{
    // Реальные астрономические данные (в СИ)
    private const double SunMass = 1.989e30;
    private const double EarthMass = 5.972e24;
    private const double MoonMass = 7.342e22;

    private const double EarthOrbitRadius = 1.496e11;    
    private const double MoonOrbitRadius = 3.844e8;      

    private const double EarthOrbitalVelocity = 29_780;  
    private const double MoonOrbitalVelocity = 1_022;    

    public void Create(World world)
    {
        // Солнце — в центре, статично
        var sun = world.CreateBody(
            CelestialBodyType.Sun,
            x: 0, y: 0, z: 0,
            vx: 0, vy: 0, vz: 0, 
            mass: SunMass,
            radius: 6.9634e8,
            isStatic: true,
            r: 255, g: 98, b: 0,
            sizePixels: 10);

        var earth = world.CreateBody(
            CelestialBodyType.Earth,
            x: EarthOrbitRadius, y: 0, z: 0,
            vx: 0, vy: EarthOrbitalVelocity, vz: 0,
            mass: EarthMass,
            radius: 6.371e6,
            isStatic: false,
            r: 77, g: 255, b:0,
            sizePixels: 10);
        var moon = world.CreateBody(
            CelestialBodyType.Moon,
            x: EarthOrbitRadius + MoonOrbitRadius, y: 0, z: 0,
            vx: 0, vy: EarthOrbitalVelocity + MoonOrbitalVelocity, vz: 0,
            mass: MoonMass,
            radius: 1.7374e6,
            isStatic: false,
            r: 180, g: 180, b: 180,
            sizePixels: 6);
    }
}