using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitalAuthority.ECS.Components;
using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Rendering.Camera;
using OrbitalAuthority.Rendering.Meshes;

namespace OrbitalAuthority.Rendering.Renderers;

/// <summary>
/// Рисует небесные тела процедурными сферами: одна общая единичная UV-сфера,
/// масштабируется под физический радиус (MassComponent.Radius) каждого тела.
/// </summary>
public sealed class CelestialBodyRenderer3D
{
    private readonly GraphicsDevice _device;
    private readonly World _world;
    private readonly BasicEffect _effect;

    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly int _primitiveCount;

    public CelestialBodyRenderer3D(GraphicsDevice device, World world)
    {
        _device = device;
        _world = world;

        var (vertices, indices) = SphereMeshGenerator.Generate();

        _vertexBuffer = new VertexBuffer(
            device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);

        _indexBuffer = new IndexBuffer(
            device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);

        _primitiveCount = indices.Length / 3;

        _effect = new BasicEffect(device)
        {
            TextureEnabled = false,
            VertexColorEnabled = false,
        };
        _effect.EnableDefaultLighting();
    }

    public void Draw(Camera3D camera)
    {
        _device.SetVertexBuffer(_vertexBuffer);
        _device.Indices = _indexBuffer;

        var prevRaster = _device.RasterizerState;
        var prevDepth = _device.DepthStencilState;

        // CullNone: сфера замкнута и порядок обхода треугольников не гарантирован —
        // проще не отбрасывать грани, чем гадать с winding order.
        _device.RasterizerState = RasterizerState.CullNone;
        _device.DepthStencilState = DepthStencilState.Default;

        _effect.View = camera.GetViewMatrix();
        _effect.Projection = camera.GetProjectionMatrix();

        int count = _world.EntityCount;
        for (int id = 0; id < count; id++)
        {
            if (_world.Transforms[id] is not Transform3D t) continue;
            if (_world.Masses[id] is not MassComponent m) continue;
            if (_world.Renders[id] is not RenderComponent r) continue;

            float radius = (float)m.Radius;
            if (radius <= 0f) continue;

            var relativePos = camera.ToRelative(t.X, t.Y, t.Z);
            _effect.World = Matrix.CreateScale(radius) * Matrix.CreateTranslation(relativePos);

            var color = new Color(r.R, r.G, r.B).ToVector3();

            bool isStar = _world.Bodies[id] is CelestialBodyComponent body &&
                          body.Type == CelestialBodyType.Sun;

            if (isStar)
            {
                // Звезда светит сама, а не отражает свет соседних тел — иначе половина
                // диска оказывается в "тени" от направленного света, что для источника света бессмысленно.
                _effect.LightingEnabled = false;
                _effect.DiffuseColor = Vector3.Zero;
                _effect.EmissiveColor = color;
            }
            else
            {
                _effect.LightingEnabled = true;
                _effect.DiffuseColor = color;
                _effect.EmissiveColor = Vector3.Zero;
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
            }
        }

        _device.RasterizerState = prevRaster;
        _device.DepthStencilState = prevDepth;
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _effect?.Dispose();
    }
}
