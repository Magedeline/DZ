namespace Celeste.Entities;

[Pooled]
[Tracked]
[HotReloadable]
public partial class CharaBossShot : Entity
{
    private static readonly ParticleType p_trail;
    private const float move_speed = 100f;
    private const float cant_kill_time = 0.15f;
    private const float appear_time = 0.1f;
    private CharaBoss charaboss;
    private Level level;
    private Vector2 speed;
    private float particleDir;
    private Vector2 anchor;
    private Vector2 perp;
    private global::Celeste.Player target;
    private Vector2 targetPt;
    private float angleOffset;
    private bool dead;
    private float cantKillTimer;
    private float appearTimer;
    private bool hasBeenInCamera;
    private readonly SineWave sine;
    private float sineMult;
    private readonly Sprite sprite;

    static CharaBossShot()
    {
        // Fallback: create a simple white particle type for the trail
        p_trail = new ParticleType {
            Color = Color.White,
            Color2 = Color.LightGray,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.5f,
            LifeMax = 1.0f,
            Size = 1.0f,
            SizeRange = 0.2f,
            DirectionRange = (float)Math.PI,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.2f,
            ScaleOut = true
        };
    }

    public CharaBossShot()
        : base(Vector2.Zero)
    {
        Add(sprite = CreateProjectileSprite());
        Collider = new Hitbox(4f, 4f, -2f, -2f);
        Add(new PlayerCollider(OnPlayer));
        Depth = -1000000;
        Add(sine = new SineWave(1.4f));
    }

    private static Sprite CreateProjectileSprite()
    {
        try
        {
            return DZModule.SpriteBank.Create("chara_projectile");
        }
        catch (global::System.Exception)
        {
            // Fallback to vanilla-like boss sprite entry when custom key is missing.
            return DZModule.SpriteBank.Create("badelineBoss");
        }
    }

    private void PlayProjectileCharge()
    {
        if (sprite.Has("charabossDZ_CHarge"))
            sprite.Play("charabossDZ_CHarge", true);
        else if (sprite.Has("charaboss_loop"))
            sprite.Play("charaboss_loop", true);
        else if (sprite.Has("charge"))
            sprite.Play("charge", true);
        else if (sprite.Has("projectile"))
            sprite.Play("projectile", true);
    }

    public CharaBossShot Init(CharaBoss charaboss, global::Celeste.Player target, float angleOffset = 0.0f)
    {
        this.charaboss = charaboss;
        anchor = Position = charaboss.Center;
        this.target = target;
        this.angleOffset = angleOffset;
        dead = hasBeenInCamera = false;
        cantKillTimer = 0.15f;
        appearTimer = 0.1f;
        sine.Reset();
        sineMult = 0.0f;
        PlayProjectileCharge();
        initSpeed();
        return this;
    }

    public CharaBossShot Init(CharaBoss charaboss, Vector2 target)
    {
        this.charaboss = charaboss;
        anchor = Position = charaboss.Center;
        this.target = null;
        angleOffset = 0.0f;
        targetPt = target;
        dead = hasBeenInCamera = false;
        cantKillTimer = 0.15f;
        appearTimer = 0.1f;
        sine.Reset();
        sineMult = 0.0f;
        PlayProjectileCharge();
        initSpeed();
        return this;
    }

    private void initSpeed()
    {
        speed = target == null ? (targetPt - Center).SafeNormalize(100f) : (target.Center - Center).SafeNormalize(100f);
        if (angleOffset != 0.0)
            speed = speed.Rotate(angleOffset);
        perp = speed.Perpendicular().SafeNormalize();
        particleDir = (-speed).Angle();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        if (charaboss == null || !charaboss.Moving)
            return;
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
        if (appearTimer > 0.0f)
        {
            Position = anchor = charaboss.ShotOrigin;
            appearTimer -= Engine.DeltaTime;
        }
        else
        {
            if (cantKillTimer > 0.0f)
                cantKillTimer -= Engine.DeltaTime;
            anchor += speed * Engine.DeltaTime;
            Position = anchor + perp * sineMult * sine.Value * 3f;
            sineMult = Calc.Approach(sineMult, 1f, 2f * Engine.DeltaTime);
            if (dead)
                return;
            bool isInCamera = level.IsInCamera(Position, 8f);
            if (isInCamera && !hasBeenInCamera)
                hasBeenInCamera = true;
            else if (!isInCamera && hasBeenInCamera)
                Destroy();
            if (Scene.OnInterval(0.04f))
                level.ParticlesFG.Emit(p_trail, 1, Center, Vector2.One * 2f, particleDir);
        }
    }

    public override void Render()
    {
        Color color = sprite.Color;
        Vector2 position = sprite.Position;
        sprite.Color = Color.Black;
        sprite.Position = position + new Vector2(-1f, 0.0f);
        sprite.Render();
        sprite.Position = position + new Vector2(1f, 0.0f);
        sprite.Render();
        sprite.Position = position + new Vector2(0.0f, -1f);
        sprite.Render();
        sprite.Position = position + new Vector2(0.0f, 1f);
        sprite.Render();
        sprite.Color = color;
        sprite.Position = position;
        base.Render();
    }

    public void Destroy()
    {
        dead = true;
        RemoveSelf();
    }

    private void OnPlayer(global::Celeste.Player player)
    {
        if (dead)
            return;
        if (cantKillTimer > 0.0)
            Destroy();
        else
            player.Die((player.Center - Position).SafeNormalize());
    }

    public enum ShotPatterns
    {
        Single,
        Double,
        Triple,
    }
}




