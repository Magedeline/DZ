namespace Celeste.Entities;

// A slow projectile that steers toward the player over its lifetime.
[Pooled]
[Tracked]
[HotReloadable]
public class CharaBossShotHoming : Entity
{
    public static ParticleType PTrail;

    private const float InitialSpeed    = 60f;
    private const float MaxSpeed        = 160f;
    private const float TurnSpeed       = 2.8f;   // radians/s
    private const float LifeTime        = 4.0f;
    private const float CantKillTime    = 0.2f;

    private CharaBoss boss;
    private Level level;
    private Vector2 speed;
    private float lifeTimer;
    private float cantKillTimer;
    private bool dead;
    private readonly Sprite sprite;

    static CharaBossShotHoming()
    {
        PTrail = new ParticleType
        {
            Color  = Color.MediumPurple,
            Color2 = Color.White,
            ColorMode  = ParticleType.ColorModes.Choose,
            FadeMode   = ParticleType.FadeModes.Late,
            LifeMin    = 0.3f,
            LifeMax    = 0.6f,
            Size       = 1f,
            SizeRange  = 0.4f,
            DirectionRange = (float)Math.PI,
            SpeedMin   = 8f,
            SpeedMax   = 18f,
            SpeedMultiplier = 0.15f,
            ScaleOut   = true
        };
    }

    public CharaBossShotHoming() : base(Vector2.Zero)
    {
        sprite = CreateSprite();
        Add(sprite);
        Collider = new Hitbox(6f, 6f, -3f, -3f);
        Add(new PlayerCollider(OnPlayer));
        Depth = -1000000;
    }

    private static Sprite CreateSprite()
    {
        try   { return GFX.SpriteBank.Create("chara_projectile"); }
        catch { return GFX.SpriteBank.Create("badelineBoss"); }
    }

    public CharaBossShotHoming Init(CharaBoss boss, global::Celeste.Player target)
    {
        this.boss    = boss;
        Position     = boss.ShotOrigin;
        dead         = false;
        lifeTimer    = LifeTime;
        cantKillTimer = CantKillTime;

        // Aim initially at the player, with slight random jitter
        float angle = (target != null)
            ? Calc.Angle(Position, target.Center)
            : Calc.Random.NextFloat((float)Math.PI * 2f);
        angle += Calc.Random.Range(-0.3f, 0.3f);
        speed = Calc.AngleToVector(angle, InitialSpeed);

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

        if (cantKillTimer > 0f)
            cantKillTimer -= Engine.DeltaTime;

        lifeTimer -= Engine.DeltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy();
            return;
        }

        // Steer toward the player
        var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        if (player != null && !player.Dead)
        {
            float desiredAngle = Calc.Angle(Position, player.Center);
            float currentAngle = speed.Angle();
            float diff = Calc.AngleDiff(currentAngle, desiredAngle);
            float maxTurn = TurnSpeed * Engine.DeltaTime;
            currentAngle += MathHelper.Clamp(diff, -maxTurn, maxTurn);
            speed = Calc.AngleToVector(currentAngle, speed.Length());
        }

        // Accelerate up to MaxSpeed
        speed = speed.SafeNormalize(Math.Min(speed.Length() + 80f * Engine.DeltaTime, MaxSpeed));

        Position += speed * Engine.DeltaTime;

        if (dead) return;

        if (!level.IsInCamera(Position, 24f) && lifeTimer < LifeTime - 0.5f)
        {
            Destroy();
            return;
        }

        if (Scene.OnInterval(0.04f))
            level.ParticlesFG.Emit(PTrail, 1, Center, Vector2.One * 2f, (-speed).Angle());
    }

    public override void Render()
    {
        // Draw a black outline like the regular shot
        Color c = sprite.Color;
        Vector2 p = sprite.Position;
        sprite.Color = Color.Black;
        foreach (Vector2 offset in new[] { new Vector2(-1,0), new Vector2(1,0), new Vector2(0,-1), new Vector2(0,1) })
        {
            sprite.Position = p + offset;
            sprite.Render();
        }
        sprite.Color = c;
        sprite.Position = p;
        base.Render();
    }

    public void Destroy()
    {
        dead = true;
        RemoveSelf();
    }

    private void OnPlayer(global::Celeste.Player player)
    {
        if (dead) return;
        if (cantKillTimer > 0f)
            Destroy();
        else
            player.Die((player.Center - Position).SafeNormalize());
    }
}
