using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Sprite helper class for Sans character animations.
/// Provides a pre-configured Sprite with all Sans animations from the sprite bank.
/// </summary>
public class SansSprite : Sprite
{
    public SansSprite()
        : base(GFX.Game, "characters/sans/")
    {
        AddLoop("idle", "idle", 0.1f);
        AddLoop("walk", "walk", 0.08f);
        AddLoop("heh", "heh", 0.06f);
        AddLoop("noeyes", "noeyes", 0.06f);
        AddLoop("patonmadshoulder", "patonmadshoulder", 0.06f);
        AddLoop("sheild", "sheild", 0.06f);
        AddLoop("wtf", "wtf", 0.06f);
        AddLoop("oderup", "oderup", 0.06f);
        AddLoop("eepy", "eepy", 0.06f);
        Play("idle");
        JustifyOrigin(0.5f, 1f);
    }

    public SansSprite(string startAnimation)
        : this()
    {
        if (!string.IsNullOrEmpty(startAnimation) && Has(startAnimation))
        {
            Play(startAnimation);
        }
    }
}

/// <summary>
/// Loenn entity for placing Sans sprites in maps.
/// Matches the Lua definition: DZ/SansSprite
/// </summary>
[CustomEntity("DZ/SansSprite")]
[Tracked]
public class SansSpriteEntity : Entity
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

    public SansSpriteEntity(Vector2 position, string animation = "idle")
        : base(position)
    {
        Depth = -8500;
        this.animation = animation;

        Add(sprite = new SansSprite(animation));
    }

    public SansSpriteEntity(EntityData data, Vector2 offset)
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
