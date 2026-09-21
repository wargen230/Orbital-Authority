using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitalAuthority.ECS.Components;
using OrbitalAuthority.ECS.Components.PhysicsComponents;
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Rendering.Camera;

namespace OrbitalAuthority.Rendering.Renderers;

/// <summary>
/// Рисует сцену не растеризацией меша сферы (как <see cref="CelestialBodyRenderer3D"/>),
/// а трассировкой лучей в пиксельном шейдере (Content/SunLight.fx): один draw call
/// full-screen quad на весь кадр, шейдер сам перебирает все тела как массив параметров.
/// Свет и жёсткие тени от Солнца считаются в шейдере аналитически (ray-sphere intersection).
/// </summary>
public sealed class RayTracedSunRenderer
{
    public const int MaxSpheres = 8;

    private readonly GraphicsDevice _device;
    private readonly World _world;
    private readonly Effect _effect;
    private readonly VertexBuffer _quad;

    private readonly Vector3[] _positions = new Vector3[MaxSpheres];
    private readonly float[] _radii = new float[MaxSpheres];
    private readonly Vector3[] _colors = new Vector3[MaxSpheres];
    private readonly float[] _isStar = new float[MaxSpheres];

    public RayTracedSunRenderer(GraphicsDevice device, World world, Effect effect)
    {
        _device = device;
        _world = world;
        _effect = effect;

        // Квад на весь экран в clip space — вершинному шейдеру не нужны World/View/Projection,
        // вся работа (луч через каждый пиксель) происходит в пиксельном шейдере.
        var verts = new[]
        {
            new VertexPosition(new Vector3(-1, -1, 0)),
            new VertexPosition(new Vector3(-1, 1, 0)),
            new VertexPosition(new Vector3(1, -1, 0)),
            new VertexPosition(new Vector3(1, 1, 0)),
        };

        _quad = new VertexBuffer(device, typeof(VertexPosition), verts.Length, BufferUsage.WriteOnly);
        _quad.SetData(verts);
    }

    public void Draw(Camera3D camera)
    {
        Vector3 lightPosition = Vector3.Zero;
        int written = 0;

        for (int id = 0; id < _world.EntityCount && written < MaxSpheres; id++)
        {
            if (_world.Transforms[id] is not Transform3D t) continue;
            if (_world.Masses[id] is not MassComponent m) continue;
            if (_world.Renders[id] is not RenderComponent r) continue;
            if (_world.Bodies[id] is not CelestialBodyComponent body) continue;

            var relative = camera.ToRelative(t.X, t.Y, t.Z);

            _positions[written] = relative;
            _radii[written] = (float)m.Radius;
            _colors[written] = new Color(r.R, r.G, r.B).ToVector3();

            bool isStar = body.Type == CelestialBodyType.Sun;
            _isStar[written] = isStar ? 1f : 0f;
            if (isStar) lightPosition = relative;

            written++;
        }

        var (right, up, forward) = camera.GetBasisVectors();

        _effect.Parameters["CameraRight"].SetValue(right);
        _effect.Parameters["CameraUp"].SetValue(up);
        _effect.Parameters["CameraForward"].SetValue(forward);
        _effect.Parameters["TanHalfFovY"].SetValue((float)System.Math.Tan(camera.FovRadians / 2.0));
        _effect.Parameters["AspectRatio"].SetValue(camera.ViewportWidth / (float)camera.ViewportHeight);

        _effect.Parameters["LightPosition"].SetValue(lightPosition);
        _effect.Parameters["BackgroundColor"].SetValue(new Vector3(5, 5, 15) / 255f);

        _effect.Parameters["SphereCount"].SetValue(written);
        _effect.Parameters["SpherePositions"].SetValue(_positions);
        _effect.Parameters["SphereRadii"].SetValue(_radii);
        _effect.Parameters["SphereColors"].SetValue(_colors);
        _effect.Parameters["SphereIsStar"].SetValue(_isStar);

        _device.SetVertexBuffer(_quad);

        var prevDepth = _device.DepthStencilState;
        // Один quad на весь экран поверх уже очищенного буфера — тест глубины не нужен.
        _device.DepthStencilState = DepthStencilState.None;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        _device.DepthStencilState = prevDepth;
    }

    public void Dispose()
    {
        _quad?.Dispose();
    }
}
