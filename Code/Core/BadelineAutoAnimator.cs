namespace Celeste;

[HotReloadable]
public class BadelineAutoAnimator() : Component(true, false)
{
    private bool _enabled = true;
    private string lastAnimation = "fallSlow";
    private bool wasSyncingSprite;
    private Wiggler pop;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(pop = Wiggler.Create(0.5f, 4f, f =>
        {
            Sprite sprite = Entity.Get<Sprite>();
            if (sprite == null)
                return;
            sprite.Scale = new Vector2(Math.Sign(sprite.Scale.X), 1f) * (float)(1.0 + 0.25 * f);
        }));
    }

    public override void Removed(Entity entity)
    {
        entity.Remove(pop);
        base.Removed(entity);
    }

    public void SetReturnToAnimation(string anim) => lastAnimation = anim;

    public override void Update()
    {
        Sprite sprite = Entity.Get<Sprite>();
        if (Scene == null || sprite == null || !_enabled)
            return;
        bool flag = false;
        Textbox entity = Scene.Tracker.GetEntity<Textbox>();
        if (_enabled && entity != null)
        {
            if (entity.PortraitName.IsIgnoreCase("badeline"))
            {
                if (entity.PortraitAnimation.IsIgnoreCase("laugh", "scoff"))
                {
                    if (!wasSyncingSprite)
                        lastAnimation = sprite.CurrentAnimationID;
                    PlaySafe(sprite, "laugh", "idle", "fallSlow");
                    wasSyncingSprite = flag = true;
                }
                else if (entity.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
                {
                    if (!wasSyncingSprite)
                    {
                        pop.Start();
                        lastAnimation = sprite.CurrentAnimationID;
                    }
                    PlaySafe(sprite, "angry", "idle", "fallSlow");
                    wasSyncingSprite = flag = true;
                }
            }
        }
        if (!wasSyncingSprite || flag)
            return;
        wasSyncingSprite = false;
        if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
            lastAnimation = "fallSlow";
        if (sprite.CurrentAnimationID == "angry")
            pop.Start();
        PlaySafe(sprite, lastAnimation, "idle", "fallSlow");
    }

    private static void PlaySafe(Sprite sprite, string preferred, params string[] fallbacks)
    {
        if (sprite == null)
            return;
        if (!string.IsNullOrEmpty(preferred) && sprite.Has(preferred))
        {
            sprite.Play(preferred);
            return;
        }
        if (fallbacks != null)
        {
            foreach (string fallback in fallbacks)
            {
                if (!string.IsNullOrEmpty(fallback) && sprite.Has(fallback))
                {
                    sprite.Play(fallback);
                    return;
                }
            }
        }
    }
}
