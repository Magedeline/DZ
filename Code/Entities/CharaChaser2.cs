using System.Runtime.CompilerServices;

namespace Celeste.Entities;

/// <summary>
/// CharaChaser2 - An enhanced variant of CharaChaser with additional abilities.
/// This chaser is faster, more aggressive, and has different visual effects.
/// Used for more intense chase sequences in later chapters.
/// </summary>
[CustomEntity(ids: "MaggyHelper/CharaChaser2")]
[Tracked(true)]
[HotReloadable]
public class CharaChaser2 : Entity
{
    public static ParticleType P_Vanish = new ParticleType
    {
        Size = 1f,
        Color = Calc.HexToColor("FF0000"),  // Red variant
        Color2 = Calc.HexToColor("9B3FB5"),  // Chara's purple color
        ColorMode = ParticleType.ColorModes.Blink,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.8f,
        LifeMax = 1.4f,
        SpeedMin = 16f,
        SpeedMax = 32f,
        DirectionRange = (float)Math.PI * 2f
    };

    public static ParticleType P_Trail = new ParticleType
    {
        Size = 1f,
        Color = Calc.HexToColor("9B3FB5"),
        Color2 = Calc.HexToColor("FF0000"),
        ColorMode = ParticleType.ColorModes.Fade,
        FadeMode = ParticleType.FadeModes.Linear,
        LifeMin = 0.3f,
        LifeMax = 0.6f,
        SpeedMin = 8f,
        SpeedMax = 16f,
        DirectionRange = (float)Math.PI
    };

    public static Color HairColor = Calc.HexToColor("FF3355");

    public Monocle.Sprite Sprite;

    public PlayerHair Hair;

    private LightOcclude occlude;

    private bool ignorePlayerAnim;

    private int index;

    private CelestePlayer player;

    private bool following;

    private float followBehindTime;

    private float followBehindIndexDelay;

    public bool Hovering;

    private float hoveringTimer;

    private Dictionary<string, SoundSource> loopingSounds;

    private List<SoundSource> inactiveLoopingSounds;

    private bool canChangeMusic;

    private bool isAggressive;

    private float aggressionTimer;

    private float speedMultiplier;

    public CharaChaser2(Vector2 position, int index)
        : base(position)
    {
        loopingSounds = new Dictionary<string, SoundSource>();
        inactiveLoopingSounds = new List<SoundSource>();
        this.index = index;
        base.Depth = -1;
        base.Collider = new Hitbox(6f, 6f, -3f, -7f);
        Collidable = false;
        Sprite = GFX.SpriteBank.Create("maggy_chara");
        Sprite.Play("fallSlow", restart: true);
        Hair = null;
        Add(Sprite);
        Visible = true;
        followBehindTime = 1.2f;  // Faster than CharaChaser
        followBehindIndexDelay = 1.0f * (float)index;  // 1 second delay per chaser index
        speedMultiplier = 1.25f;  // 25% faster
        isAggressive = false;
        Add(new PlayerCollider(OnPlayer));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CharaChaser2(EntityData data, Vector2 offset, int index)
        : this(data.Position + offset, index)
    {
        canChangeMusic = data.Bool("canChangeMusic", defaultValue: true);
        isAggressive = data.Bool("aggressive", defaultValue: false);
        speedMultiplier = data.Float("speedMultiplier", 1.25f);
    }

    public CharaChaser2(EntityData data, Vector2 offset)
        : this(data.Position + offset, 0)
    {
        canChangeMusic = data.Bool("canChangeMusic", defaultValue: true);
        isAggressive = data.Bool("aggressive", defaultValue: false);
        speedMultiplier = data.Float("speedMultiplier", 1.25f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = scene as Level;
        
        if (level == null)
        {
            RemoveSelf();
            return;
        }

        Session session = level.Session;
        
        // Check if this is the DesoloZantas campaign - if so, apply story-specific logic
        bool isDesoloZantasCampaign = session.Area.GetLevelSet() == "DesoloZantas";
        
        if (isDesoloZantasCampaign)
        {
            // Trigger warning cutscene once before the chapter 4 chase starts.
            if (!session.GetFlag("chara2_warning") && session.Area.Mode == AreaMode.Normal)
            {
                if (scene.Tracker.GetEntity<Cutscenes.CS04_CharaWarning>() == null)
                {
                    scene.Add(new Cutscenes.CS04_CharaWarning(this));
                }
                return;
            }
        }
        
        // Always start chasing (for other maps or when DesoloZantas conditions are met)
        Add(new Coroutine(StartChasingRoutine(level)));
    }

    public IEnumerator StartChasingRoutine(Level level)
    {
        Hovering = true;
        while ((player = Scene.Tracker.GetEntity<global::Celeste.Player>()) == null || player.JustRespawned)
        {
            yield return null;
        }
        Vector2 to = player.Position;
        yield return followBehindIndexDelay;
        if (!Visible)
        {
            PopIntoExistance(0.4f);  // Faster appearance
        }
        Sprite.Play("fallSlow");
        if (Hair != null) Hair.Visible = true;
        Hovering = false;
        if (CanChangeMusic(level.Session.Area.Mode == AreaMode.Normal))
        {
            level.Session.Audio.Music.Event = "guid://{8f06feaf-b21d-431f-aba7-970a5a9fb507}";
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        yield return TweenToPlayer(to);
        Collidable = true;
        following = true;
        Add(occlude = new LightOcclude());
        if (IsChaseEnd(level))
        {
            Add(new Coroutine(StopChasing()));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator TweenToPlayer(Vector2 to)
    {
        Audio.Play("guid://{25294859-6bfd-434e-9528-8433245fe3b5}", Position, "chaser_count", index);
        Vector2 from = Position;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, (followBehindTime - 0.1f) / speedMultiplier, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            Position = Vector2.Lerp(from, to, t.Eased);
            if (to.X != from.X)
            {
                Sprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)Math.Sign(to.X - from.X);
            }
            Trail();
        };
        Add(tween);
        yield return tween.Duration;
    }

    private IEnumerator StopChasing()
    {
        Level level = Scene as Level;
        while (!CollideCheck<BadelineOldsiteEnd>() && !CollideCheck<CharaChaserEnd>())
        {
            yield return null;
        }
        following = false;
        ignorePlayerAnim = true;
        Sprite.Play("laugh");
        Sprite.Scale.X = 1f;
        yield return 0.8f;
        Audio.Play("guid://{16b40879-0a79-4e42-8c91-fe419a8e186c}", Position);
        level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
        level.Particles.Emit(P_Vanish, 16, Center, Vector2.One * 8f);
        RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        if (player != null && player.Dead)
        {
            Sprite.Play("laugh");
            Sprite.X = (float)(Math.Sin(hoveringTimer) * 4.0);
            Hovering = true;
            hoveringTimer += Engine.DeltaTime * 2f;
            Depth = -12500;
            foreach (KeyValuePair<string, SoundSource> loopingSound in loopingSounds)
            {
                loopingSound.Value.Stop();
            }
            Trail();
        }
        else if (following && player != null && player.GetChasePosition(Scene.TimeActive, followBehindTime + followBehindIndexDelay, out var chaseState))
        {
            // Enhanced speed - CharaChaser2 catches up faster
            float approachSpeed = 500f * Engine.DeltaTime * speedMultiplier;
            
            // Aggressive mode - gets faster over time
            if (isAggressive)
            {
                aggressionTimer += Engine.DeltaTime;
                approachSpeed *= 1f + (aggressionTimer * 0.05f);  // 5% faster per second
            }
            
            Position = Calc.Approach(Position, chaseState.Position, approachSpeed);
            if (!ignorePlayerAnim && chaseState.Animation != Sprite.CurrentAnimationID && chaseState.Animation != null && Sprite.Has(chaseState.Animation))
            {
                Sprite.Play(chaseState.Animation, restart: true);
            }
            if (!ignorePlayerAnim)
            {
                Sprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)chaseState.Facing;
            }
            for (int i = 0; i < chaseState.Sounds; i++)
            {
                if (chaseState[i].Action == CelestePlayer.ChaserStateSound.Actions.Oneshot)
                {
                    Audio.Play(chaseState[i].Event, Position, chaseState[i].Parameter, chaseState[i].ParameterValue, "chaser_count", index);
                }
                else if (chaseState[i].Action == CelestePlayer.ChaserStateSound.Actions.Loop && !loopingSounds.ContainsKey(chaseState[i].Event))
                {
                    SoundSource soundSource;
                    if (inactiveLoopingSounds.Count > 0)
                    {
                        soundSource = inactiveLoopingSounds[0];
                        inactiveLoopingSounds.RemoveAt(0);
                    }
                    else
                    {
                        Add(soundSource = new SoundSource());
                    }
                    soundSource.Play(chaseState[i].Event, "chaser_count", index);
                    loopingSounds.Add(chaseState[i].Event, soundSource);
                }
                else if (chaseState[i].Action == CelestePlayer.ChaserStateSound.Actions.Stop)
                {
                    if (loopingSounds.TryGetValue(chaseState[i].Event, out var value))
                    {
                        value.Stop();
                        loopingSounds.Remove(chaseState[i].Event);
                        inactiveLoopingSounds.Add(value);
                    }
                }
            }
            Depth = chaseState.Depth;
            Trail();
            
            // Emit trail particles when aggressive
            if (isAggressive && Scene.OnInterval(0.15f))
            {
                (Scene as Level)?.Particles.Emit(P_Trail, 2, Center, Vector2.One * 4f);
            }
        }
        if (Hovering)
        {
            hoveringTimer += Engine.DeltaTime;
            Sprite.Y = (float)(Math.Sin(hoveringTimer * 2f) * 4f);
        }
        if (occlude != null)
        {
            occlude.Visible = !CollideCheck<Solid>();
        }
        base.Update();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Trail()
    {
        if (base.Scene.OnInterval(0.08f))  // More frequent trails
        {
            TrailManager.Add(this, HairColor, 1f, frozenUpdate: false, useRawDeltaTime: false);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(CelestePlayer player)
    {
        player.Die((player.Position - Position).SafeNormalize());
    }

    private void Die()
    {
        RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PopIntoExistance(float duration)
    {
        Visible = true;
        Sprite.Scale = Vector2.Zero;
        Sprite.Color = Color.Transparent;
        if (Hair != null)
        {
            Hair.Visible = true;
            Hair.Alpha = 0f;
        }
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, duration, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            Sprite.Scale = Vector2.One * t.Eased;
            Sprite.Color = Color.White * t.Eased;
            if (Hair != null) Hair.Alpha = t.Eased;
        };
        Add(tween);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool OnGround(int dist = 1)
    {
        for (int i = 1; i <= dist; i++)
        {
            if (CollideCheck<Solid>(Position + new Vector2(0f, i)))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsChaseEnd(Level level)
    {
        if (level.Tracker.CountEntities<CharaChaserEnd>() != 0)
        {
            return true;
        }
        if (level.Tracker.CountEntities<BadelineOldsiteEnd>() != 0)
        {
            return true;
        }
        return false;
    }

    public bool CanChangeMusic(bool value)
    {
        if ((base.Scene as Level).Session.Area.GetLevelSet() == "DesoloZantas")
        {
            return value;
        }
        return canChangeMusic;
    }

    /// <summary>
    /// Force the chaser into aggressive mode
    /// </summary>
    public void SetAggressive(bool aggressive)
    {
        isAggressive = aggressive;
        if (aggressive)
        {
            Sprite.Play("angry");
        }
    }

    /// <summary>
    /// Set speed multiplier dynamically
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}

