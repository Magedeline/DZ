using Celeste.Cutscenes;
using System.Runtime.CompilerServices;

namespace Celeste.Entities;

/// <summary>
/// CharaChaser - A chaser entity that follows the player with Chara's appearance.
/// This chaser triggers the CS02_CharaIntro cutscene when appropriate conditions are met.
/// </summary>
[CustomEntity(ids: "DZ/CharaChaser")]
[Tracked]
[HotReloadable]
public class CharaChaser : Entity
{
    public static ParticleType P_Vanish = new ParticleType
    {
        Size = 1f,
        Color = Color.White,
        Color2 = Calc.HexToColor("9B3FB5"),  // Chara's purple color
        ColorMode = ParticleType.ColorModes.Blink,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.8f,
        LifeMax = 1.4f,
        SpeedMin = 12f,
        SpeedMax = 24f,
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

    public static Color HairColor = Calc.HexToColor("9B3FB5");

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

    private bool introComplete = false;

    private bool triggerIntro = true;

    public CharaChaser(Vector2 position, int index)
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
        // Chara doesn't use PlayerHair - skip hair initialization to avoid NullReferenceException
        Hair = null;
        Add(Sprite);
        Visible = true;
        followBehindTime = 1.55f;
        followBehindIndexDelay = 1.0f * (float)index;  // 1 second delay per chaser index
        speedMultiplier = 1.0f;
        isAggressive = false;
        Add(new PlayerCollider(OnPlayer));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CharaChaser(EntityData data, Vector2 offset, int index)
        : this(data.Position + offset, index)
    {
        canChangeMusic = data.Bool("canChangeMusic", defaultValue: true);
        isAggressive = data.Bool("aggressive", defaultValue: false);
        speedMultiplier = data.Float("speedMultiplier", 1.0f);
        triggerIntro = data.Bool("triggerIntro", defaultValue: true);
    }

    public CharaChaser(EntityData data, Vector2 offset)
        : this(data.Position + offset, 0)
    {
        canChangeMusic = data.Bool("canChangeMusic", defaultValue: true);
        isAggressive = data.Bool("aggressive", defaultValue: false);
        speedMultiplier = data.Float("speedMultiplier", 1.0f);
        triggerIntro = data.Bool("triggerIntro", defaultValue: true);
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
            // Check if already completed the chara intro
            if (session.GetLevelFlag("b-14") && session.Area.Mode == AreaMode.Normal)
            {
                RemoveSelf();
                return;
            }

            // Trigger CS02 intro once when entering the intended room before the intro flag is set.
            if (!session.GetFlag("evil_chara_intro") && session.Level == "b-3" && session.Area.Mode == AreaMode.Normal)
            {
                // Set up for cutscene - player finds Chara pretending to be dead
                Hovering = false;
                Visible = true;
                if (Hair != null) Hair.Visible = false;
                if (Sprite.Has("pretendDead"))
                {
                    Sprite.Play("pretendDead");
                }
                else if (Sprite.Has("fallSlow"))
                {
                    Sprite.Play("fallSlow");
                }
                
                session.Audio.Music.Event = "event:/pusheen/music/lvl2/evil_chara";
                session.Audio.Apply(forceSixteenthNoteHack: false);
                
                // Add the intro cutscene only once per room load (if enabled).
                if (triggerIntro && scene.Tracker.GetEntity<CS02_CharaIntro>() == null)
                {
                    scene.Add(new CS02_CharaIntro(this));
                }
                return;
            }

            // Normal chase behavior for DesoloZantas - only start if intro was already seen
            if (session.GetFlag("evil_chara_intro") || session.GetFlag("chara_intro"))
            {
                Add(new Coroutine(StartChasingRoutine(level)));
            }
        }
        else
        {
            // For other maps/mods - check if intro cutscene is running
            try
            {
                if (scene.Tracker.GetEntity<CS02_CharaIntro>() != null)
                {
                    // Intro cutscene is playing - don't chase yet, wait for it to complete
                    introComplete = false;
                }
                else
                {
                    // No intro cutscene - start chasing immediately
                    introComplete = true;
                    Add(new Coroutine(StartChasingRoutine(level)));
                }
            }
            catch
            {
                // CS02_CharaIntro type not registered - start chasing immediately
                introComplete = true;
                Add(new Coroutine(StartChasingRoutine(level)));
            }
        }
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
            PopIntoExistance(0.5f);
        }
        Sprite.Play("fallSlow");
        if (Hair != null) Hair.Visible = true;
        Hovering = false;
        if (CanChangeMusic(level.Session.Area.Mode == AreaMode.Normal))
        {
            // Set music based on current level
            string musicEvent = level.Session.Level.StartsWith("b-3") 
                ? "event:/pusheen/music/lvl2/evil_chara"
                : "event:/pusheen/music/lvl2/chase";
            level.Session.Audio.Music.Event = musicEvent;
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        yield return TweenToPlayer(to);
        Collidable = true;
        following = true;
        Add(occlude = new LightOcclude());
        if (level.Tracker.GetEntity<CharaChaserMusicHandler>() == null)
        {
            level.Add(new CharaChaserMusicHandler());
        }
        if (IsChaseEnd(level))
        {
            Add(new Coroutine(StopChasing()));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator TweenToPlayer(Vector2 to)
    {
        Audio.Play("event:/char/badeline/level_entry", Position, "chaser_count", index);
        Vector2 from = Position;
        float duration = (followBehindTime - 0.1f) / speedMultiplier;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, duration, start: true);
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
        if (level.Session.Area.GetLevelSet() == "DesoloZantas")
        {
            return custom_StopChasing();
        }
        return orig_StopChasing();
    }

    private IEnumerator orig_StopChasing()
    {
        // Original vanilla-style behavior - no custom end zone
        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator custom_StopChasing()
    {
        Level level = Scene as Level;
        while (!CollideCheck<CharaChaserEnd>())
        {
            yield return null;
        }
        following = false;
        ignorePlayerAnim = true;
        Sprite.Play("laugh");
        Sprite.Scale.X = 1f;
        yield return 1f;
        Audio.Play("event:/char/badeline/disappear", Position);
        level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
        level.Particles.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
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
            // Enhanced speed with multiplier
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
        if (base.Scene.OnInterval(0.1f))
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
        if (level.Session.Area.GetLevelSet() == "DesoloZantas")
        {
            return level.Tracker.CountEntities<CharaChaserEnd>() != 0;
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

    /// <summary>
    /// Called by CS02_CharaIntro when the cutscene completes.
    /// Starts the chasing behavior.
    /// </summary>
    public void BeginChasing()
    {
        if (!introComplete && Scene is Level level)
        {
            introComplete = true;
            Add(new Coroutine(StartChasingRoutine(level)));
        }
    }
}

