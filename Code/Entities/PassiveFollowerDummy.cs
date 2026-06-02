namespace Celeste.Entities;

/// <summary>
/// Shared dummy implementation for passive companions that trail Madeline without colliding or harming her.
/// Uses player chase history so companions follow grounded movement instead of hovering beside the player.
/// </summary>
public abstract class PassiveFollowerDummy : Entity
{
    private readonly string logTag;
    private readonly string spriteBankId;
    private readonly string configuredAnimation;
    private readonly float spriteScale;
    private readonly float spriteAlpha;
    private readonly bool playAnimationOnSpawn;
    private int facing;

    protected PassiveFollowerDummy(
        Vector2 position,
        string logTag,
        string spriteBankId,
        string animation,
        int facing,
        float scale,
        float alpha,
        bool isVisible,
        bool playAnimationOnSpawn,
        bool autoFollow,
        float followDelay,
        float followSpeed,
        int depth = 0)
        : base(position)
    {
        this.logTag = logTag;
        this.spriteBankId = spriteBankId;
        configuredAnimation = string.IsNullOrWhiteSpace(animation) ? "idle" : animation.Trim();
        this.facing = NormalizeFacing(facing);
        spriteScale = Math.Max(0.1f, scale);
        spriteAlpha = MathHelper.Clamp(alpha, 0f, 1f);
        this.playAnimationOnSpawn = playAnimationOnSpawn;

        AutoFollow = autoFollow;
        FollowDelay = Math.Max(0f, followDelay);
        FollowSpeed = Math.Max(32f, followSpeed);

        Depth = depth;
        Visible = isVisible;
        Collidable = false;
        Collider = new Hitbox(8f, 16f, -4f, -16f);

        InitializeSprite();
    }

    protected PassiveFollowerDummy(
        EntityData data,
        Vector2 offset,
        string logTag,
        string spriteBankId,
        float defaultFollowDelay,
        float defaultFollowSpeed)
        : this(
            data.Position + offset,
            logTag,
            spriteBankId,
            data.Attr("animation", "idle"),
            data.Int("facing", 1),
            data.Float("scale", 1f),
            data.Float("alpha", 1f),
            data.Bool("isVisible", true),
            data.Bool("playAnimationOnSpawn", true),
            data.Bool("autoFollow", true),
            data.Float("followDelay", defaultFollowDelay),
            data.Float("followSpeed", defaultFollowSpeed),
            data.Int("depth", 0))
    {
    }

    public Sprite Sprite { get; private set; }

    public bool AutoFollow { get; set; }

    public float FollowDelay { get; set; }

    public float FollowSpeed { get; set; }

    public override void Update()
    {
        base.Update();

        if (Sprite == null)
        {
            return;
        }

        ApplySpriteAppearance();

        if (!AutoFollow || !Visible)
        {
            return;
        }

        Player player = Scene?.Tracker?.GetEntity<Player>();
        if (player == null || player.Dead || Scene == null)
        {
            return;
        }

        if (!player.GetChasePosition(Scene.TimeActive, FollowDelay, out Player.ChaserState chaseState))
        {
            return;
        }

        Vector2 previousPosition = Position;
        Position = Calc.Approach(Position, chaseState.Position, FollowSpeed * Engine.DeltaTime);

        int desiredFacing = chaseState.Facing == Facings.Left ? -1 : 1;
        int chaseDirection = Math.Sign(chaseState.Position.X - previousPosition.X);
        if (chaseDirection != 0)
        {
            desiredFacing = chaseDirection;
        }

        facing = desiredFacing;

        Depth = chaseState.Depth;
        UpdateFollowAnimation(chaseState, previousPosition);
        ApplySpriteAppearance();
    }

    private void InitializeSprite()
    {
        try
        {
            Sprite = GFX.SpriteBank.Create(spriteBankId);
            Add(Sprite);
            ApplySpriteAppearance();

            if (playAnimationOnSpawn)
            {
                PlayAnimationSafe(configuredAnimation, "idle");
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, logTag, $"Failed to create sprite bank entry '{spriteBankId}': {ex}");
        }
    }

    private void ApplySpriteAppearance()
    {
        if (Sprite == null)
        {
            return;
        }

        Sprite.Scale = new Vector2(spriteScale * facing, spriteScale);
        Sprite.Color = Color.White * spriteAlpha;
        Sprite.Visible = Visible;
    }

    private void UpdateFollowAnimation(Player.ChaserState chaseState, Vector2 previousPosition)
    {
        if (Sprite == null)
        {
            return;
        }

        bool movingHorizontally = Math.Abs(Position.X - previousPosition.X) > 0.2f;
        string desiredAnimation = "idle";

        if (!chaseState.OnGround)
        {
            if (Sprite.Has("fall"))
            {
                desiredAnimation = "fall";
            }
        }
        else if (movingHorizontally)
        {
            bool wantsRun = Sprite.Has("run") &&
                !string.IsNullOrEmpty(chaseState.Animation) &&
                (chaseState.Animation.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 chaseState.Animation.IndexOf("dash", StringComparison.OrdinalIgnoreCase) >= 0);

            desiredAnimation = wantsRun ? "run" : (Sprite.Has("walk") ? "walk" : "idle");
        }

        PlayAnimationSafe(desiredAnimation, configuredAnimation, "idle");
    }

    protected void PlayAnimationSafe(params string[] animationCandidates)
    {
        if (Sprite == null)
        {
            return;
        }

        foreach (string animation in animationCandidates)
        {
            if (string.IsNullOrWhiteSpace(animation) || !Sprite.Has(animation))
            {
                continue;
            }

            if (Sprite.CurrentAnimationID != animation)
            {
                Sprite.Play(animation, restart: false);
            }

            return;
        }
    }

    private static int NormalizeFacing(int rawFacing)
    {
        return rawFacing < 0 ? -1 : 1;
    }
}