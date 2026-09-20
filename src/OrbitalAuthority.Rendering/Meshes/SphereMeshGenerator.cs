using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitalAuthority.Rendering.Meshes;

/// <summary>Процедурная генерация UV-сферы единичного радиуса с центром в начале координат.</summary>
public static class SphereMeshGenerator
{
    public static (VertexPositionNormalTexture[] Vertices, short[] Indices) Generate(
        int latSegments = 16, int lonSegments = 24)
    {
        var vertices = new List<VertexPositionNormalTexture>();

        for (int lat = 0; lat <= latSegments; lat++)
        {
            // theta: 0 (северный полюс, +Z) .. π (южный полюс, -Z)
            double theta = MathHelper.Pi * lat / latSegments;
            double sinTheta = System.Math.Sin(theta);
            double cosTheta = System.Math.Cos(theta);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                double phi = 2 * MathHelper.Pi * lon / lonSegments;
                double sinPhi = System.Math.Sin(phi);
                double cosPhi = System.Math.Cos(phi);

                var normal = new Vector3(
                    (float)(sinTheta * cosPhi),
                    (float)(sinTheta * sinPhi),
                    (float)cosTheta);

                var uv = new Vector2((float)lon / lonSegments, (float)lat / latSegments);

                // Сфера единичного радиуса: позиция совпадает с нормалью.
                vertices.Add(new VertexPositionNormalTexture(normal, normal, uv));
            }
        }

        var indices = new List<short>();
        int stride = lonSegments + 1;

        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int i0 = lat * stride + lon;
                int i1 = i0 + stride;

                indices.Add((short)i0);
                indices.Add((short)i1);
                indices.Add((short)(i0 + 1));

                indices.Add((short)(i0 + 1));
                indices.Add((short)i1);
                indices.Add((short)(i1 + 1));
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }
}
