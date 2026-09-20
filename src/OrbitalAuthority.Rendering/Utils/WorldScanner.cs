using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Rendering.UI.HUD;

namespace OrbitalAuthority.Rendering.Utils;

public static class WorldScanner
{
    /// <summary>
    /// Однократное сканирование мира. Возвращает список всех небесных тел.
    /// Вызывать при создании мира, а не каждый кадр.
    /// </summary>
    public static List<ObjectListItem> Scan(World world)
    {
        var list = new List<ObjectListItem>(world.EntityCount);

        for (int id = 0; id < world.EntityCount; id++)
        {
            if (world.Bodies[id] is not CelestialBodyComponent body) continue;

            list.Add(new ObjectListItem
            {
                EntityId = id,
                Name = GetDisplayName(body.Type, id),
            });
        }

        return list;
    }

    private static string GetDisplayName(CelestialBodyType type, int id)
    {
        // Можно расширить: "Earth", "Moon (Earth)", "Asteroid #42"
        return type switch
        {
            CelestialBodyType.Sun => "Sun",
            CelestialBodyType.Earth => "Earth",
            CelestialBodyType.Moon => "Moon",
            CelestialBodyType.Asteroid => $"Asteroid #{id}",
            _ => $"Body #{id}",
        };
    }
}