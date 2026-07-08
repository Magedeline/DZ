using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Sprite helper class for Papyrus character animations.
/// Provides a pre-configured Sprite with all Papyrus animations from the sprite bank.
/// </summary>
public class PapyrusSprite : Sprite
{
    public PapyrusSprite()
        : base(GFX.Game, "characters/DZ/papyrus/")
    {
        AddLoop("idle", "idle", 0.1f);
        AddLoop("walk", "walk", 0.08f);
        AddLoop("notfair", "notfair", 0.06f);
        AddLoop("idontgetit", "idontgetit", 0.06f);
        AddLoop("cry", "cry", 0.06f);
        AddLoop("angy", "angy", 0.06f);
        AddLoop("depress", "depress", 0.06f);
        Play("idle");
        JustifyOrigin(0.5f, 1f);
    }

    public PapyrusSprite(string startAnimation)
        : this()
    {
        if (!string.IsNullOrEmpty(startAnimation) && Has(startAnimation))
        {
            Play(startAnimation);
        }
    }
}

/// <summary>
/// Loenn entity for placing Papyrus sprites in maps.
/// Matches the Lua definition: DZ/PapyrusSprite
/// </summary>
[CustomEntity("DZ/PapyrusSprite")]
[Tracked]
public class PapyrusSpriteEntity : Entity
{
    private string animation;
    private Sprite sprite;

    public string Animation
    {
        get => animation;
        set
        {
            animation = value;
            if (sprite != null && sprite.Has(animation))
            {
                sprite.Play(animation);
            }
        }
    }

    public Sprite Sprite => sprite;

    public PapyrusSpriteEntity(Vector2 position, string animation = "idle")
        : base(position)
    {
        Depth = -8500;
        this.animation = animation;

        Add(sprite = new PapyrusSprite(animation));
    }

    public PapyrusSpriteEntity(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("animation", "idle"))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (sprite != null && !string.IsNullOrEmpty(animation))
        {
            sprite.Play(animation);
        }
    }
}
