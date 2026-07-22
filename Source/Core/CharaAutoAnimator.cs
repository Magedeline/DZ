using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ;

public class CharaAutoAnimator : Component
{
    public bool Enabled = true;

    private string lastAnimation;

    private bool wasSyncingSprite;

    private Wiggler pop;

    public CharaAutoAnimator()
        : base(active: true, visible: false)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(pop = Wiggler.Create(0.5f, 4f, [MethodImpl(MethodImplOptions.NoInlining)] (float f) =>
        {
            Sprite sprite = base.Entity.Get<Sprite>();
            if (sprite != null)
            {
                sprite.Scale = new Vector2(Math.Sign(sprite.Scale.X), 1f) * (1f + 0.25f * f);
            }
        }));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Entity entity)
    {
        entity.Remove(pop);
        base.Removed(entity);
    }

    public void SetReturnToAnimation(string anim)
    {
        lastAnimation = anim;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Sprite sprite = base.Entity.Get<Sprite>();
        if (base.Scene == null || sprite == null)
        {
            return;
        }
        bool flag = false;
        Textbox entity = base.Scene.Tracker.GetEntity<Textbox>();
        if (Enabled && entity != null && entity.PortraitName.IsIgnoreCase("chara"))
        {
            if (entity.PortraitAnimation.IsIgnoreCase("laughing"))
            {
                if (!wasSyncingSprite)
                {
                    lastAnimation = sprite.CurrentAnimationID;
                }
                sprite.Play("laugh");
                flag = (wasSyncingSprite = true);
            }
            else if (entity.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
            {
                if (!wasSyncingSprite)
                {
                    pop.Start();
                    lastAnimation = sprite.CurrentAnimationID;
                }
                sprite.Play("angry");
                flag = (wasSyncingSprite = true);
            }
        }
        if (wasSyncingSprite && !flag)
        {
            wasSyncingSprite = false;
            if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
            {
                lastAnimation = "fallSlow";
            }
            if (sprite.CurrentAnimationID == "angry")
            {
                pop.Start();
            }
            sprite.Play(lastAnimation);
        }
    }
}
