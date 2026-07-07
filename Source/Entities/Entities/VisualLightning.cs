#nullable disable

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

/// <summary>
/// A visual-only, non-lethal Lightning block used to drive the vanilla
/// <see cref="LightningRenderer"/> during scripted strikes.
/// </summary>
[Tracked(false)]
public class VisualLightning : Lightning
{
    public VisualLightning(Vector2 position, int width, int height)
        : base(position, width, height, null, 0f)
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Collidable = false;
        Visible = true;
    }

    public override void Update()
    {
        // Stay visible but never become a hazard.
        Visible = true;
        Collidable = false;
    }
}
