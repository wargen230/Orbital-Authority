using OrbitalAuthority.ECS.Components;
using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Data;
using System.Collections.Generic;

namespace OrbitalAuthority.ECS.Core;

public sealed class World
{
    private int _nextId = -1;
    public int EntityCount => _nextId + 1;

    // По одному словарю на компонент. Ключ — EntityId.
    public readonly Transform3D?[] Transforms = new Transform3D?[EcsConstants.MaxEntities];
    public readonly Velocity3D?[] Velocities = new Velocity3D?[EcsConstants.MaxEntities];
    public readonly MassComponent?[] Masses = new MassComponent?[EcsConstants.MaxEntities];
    public readonly CelestialBodyComponent?[] Bodies = new CelestialBodyComponent?[EcsConstants.MaxEntities];
    public readonly ForceAccumulator?[] Forces = new ForceAccumulator?[EcsConstants.MaxEntities];
    public readonly RenderComponent?[] Renders = new RenderComponent?[EcsConstants.MaxEntities];
    public int CreateEntity()
    {
        return _nextId += 1;
    }

    // Хелперы для добавления компонентов
    public int CreateBody(
        CelestialBodyType type,
        double x, double y, double z,
        double vx, double vy, double vz,
        double mass, double radius,
        bool isStatic,
        byte r = 255, byte g = 255, byte b = 255,  // цвет
        float sizePixels = 4f)
    {
        var id = CreateEntity();
        Transforms[id] = new Transform3D { X = x, Y = y, Z = z };
        Velocities[id] = new Velocity3D { Vx = vx, Vy = vy, Vz = vz };
        Masses[id] = new MassComponent { Mass = mass, Radius = radius };
        Bodies[id] = new CelestialBodyComponent { Type = type, IsStatic = isStatic };
        Forces[id] = default;
        Renders[id] = new RenderComponent { R = r, G = g, B = b, SizePixels = sizePixels};
        
        return id;
    }

    public void Destroy(int entityId)
    {
        // TODO: сделать оптимизированное преобразование массива

        Transforms[entityId] = null;
        Velocities[entityId] = null;
        Masses[entityId] = null;
        Bodies[entityId] = null;
        Forces[entityId] = null;
    }
}