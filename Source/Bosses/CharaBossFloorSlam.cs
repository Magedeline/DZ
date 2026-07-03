namespace Celeste.Entities;

// A projectile fired downward from CharaBoss.
// When it reaches the level floor (or a solid) it bursts into horizontal shockwaves.
[Pooled]
[Tracked]
[HotReloadable]
public class CharaBossFloorSlam : Entity
{
    public static ParticleType PImpact;

    private const float FallSpeed        = 280f;
    private const float ShockwaveSpeed   = 180f;
    private const float ShockwaveLife    = 0.9f;
    private const float CantKillTime     = 0.1f;

    private CharaBoss boss;
    private Level level;
    private bool dead;
    private bool hasLanded;
    private readonly Sprite sprite;

    // Sub-entity: the horizontal shockwave shard
    private sealed class Shockwave : Entity
    {
        private float life;
        private readonly float dir; // +1 or -1
        private float cantKillTimer;
        private CharaBoss boss;

        public Shockwave(Vector2 origin, float direction, CharaBoss boss)
            : base(origin)
        {
            this.dir   = direction;
            this.boss  = boss;
            life       = ShockwaveLife;
            cantKillTimer = CantKillTime;
            Collider   = new Hitbox(8f, 10f, -4f, -10f);
            Add(new PlayerCollider(OnPlayer));
            Depth      = -1000000;
        }

        public override void Update()
        {
            base.Update();
            if (cantKillTimer > 0f) cantKillTimer -= Engine.DeltaTime;
            life -= Engine.DeltaTime;
            if (life <= 0f) { RemoveSelf(); return; }
            X += dir * ShockwaveSpeed * Engine.DeltaTime;
        }

        public override void Render()
        {
            // Draw a simple visible indicator: a bright rectangle
            float alpha = Math.Min(life / ShockwaveLife * 2f, 1f);
            Draw.Rect(X - 4f, Y - 10f, 8f, 10f, Color.OrangeRed * alpha);
            Draw.Rect(X - 3f, Y - 9f, 6f, 8f, Color.Yellow * (alpha * 0.6f));
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (cantKillTimer <= 0f)
                player.Die((player.Center - Position).SafeNormalize());
            else
                RemoveSelf();
        }
    }

    static CharaBossFloorSlam()
    {
        PImpact = new ParticleType
        {
            Color  = Color.OrangeRed,
            Color2 = Color.Yellow,
            ColorMode  = ParticleType.ColorModes.Choose,
            FadeMode   = ParticleType.FadeModes.Late,
            LifeMin    = 0.3f,
            LifeMax    = 0.7f,
            Size       = 2f,
            SizeRange  = 0.5f,
            DirectionRange = (float)Math.PI / 3f,
            SpeedMin   = 60f,
            SpeedMax   = 140f,
            SpeedMultiplier = 0.25f,
            Acceleration   = new Vector2(0f, 80f),
            ScaleOut       = true
        };
    }

    public CharaBossFloorSlam() : base(Vector2.Zero)
    {
        sprite = CreateSprite();
        Add(sprite);
        Collider = new Hitbox(6f, 6f, -3f, -3f);
        // No PlayerCollider on the falling ball itself - only shockwaves kill
        Depth = -1000000;
    }

    private static Sprite CreateSprite()
    {
        try   { return DZModule.SpriteBank.Create("chara_projectile"); }
        catch { return DZModule.SpriteBank.Create("badelineBoss"); }
    }

    public CharaBossFloorSlam Init(CharaBoss boss)
    {
        this.boss  = boss;
        Position   = boss.ShotOrigin;
        dead       = false;
        hasLanded  = false;

        if (sprite.Has("charabossDZ_CHarge"))
            sprite.Play("charabossDZ_CHarge", true);
        else if (sprite.Has("charge"))
            sprite.Play("charge", true);

        return this;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        if (boss != null && boss.Moving)
            RemoveSelf();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        level = null;
    }

    public override void Update()
    {
        base.Update();
        if (dead || hasLanded) return;

        Y += FallSpeed * Engine.DeltaTime;

        // Check if we've hit a solid tile below us or reached the level floor
        bool hitFloor = CollideCheck<Solid>() || Y >= level.Bounds.Bottom - 4f;
        if (hitFloor)
        {
            hasLanded = true;
            Land();
        }
    }

    private void Land()
    {
        // Impact particles
        for (int i = 0; i < 24; i++)
        {
            float a = Calc.Random.NextFloat() * (float)Math.PI * 2f;
            level.Particles.Emit(PImpact, Position, a);
        }

        level.Shake(0.2f);
        Audio.Play("event:/game/06_reflection/boss_spikes_burst", Position);

        // Spawn two shockwaves going left and right
        level.Add(new Shockwave(Position, -1f, boss));
        level.Add(new Shockwave(Position, +1f, boss));

        Destroy();
    }

    public override void Render()
    {
        if (hasLanded || dead) return;
        // Draw a glowing orb as the falling projectile
        Draw.Circle(Center, 5f, Color.OrangeRed, 3);
        Draw.Circle(Center, 3f, Color.Yellow, 2);
        base.Render();
    }

    public void Destroy()
    {
        dead = true;
        RemoveSelf();
    }
}
