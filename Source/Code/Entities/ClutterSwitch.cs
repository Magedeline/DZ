namespace Celeste.Entities;

/// <summary>
/// Ingeste ClutterSwitch entity - a pressure switch that activates when dashed into from above
/// Follows Ingeste mod architecture patterns for entity development and audio system usage
/// </summary>
[CustomEntity(ids: "MaggyHelper/ClutterSwitch")]
[HotReloadable]
public class ClutterSwitch : Solid
{
    #region Constants
    public const float LightingAlphaAdd = 0.05f;

    private const int PressedYOffset = 10;
    private const int PressedSpriteYOffset = 2;
    private const int UnpressedLightRadius = 32;
    private const int PressedLightRadius = 24;
    private const int BrightLightRadius = 64;
    private const int ActivatedLightRadius = 128;

    private const float SquishSpeedApproach = 70f;
    private const float ReturnSpeedApproach = -150f;
    private const float SpeedChangeRate = 200f;
    private const float ScaleApproachRate = 0.8f;
    private const float TargetScalePressed = 1.2f;
    private const float TargetScaleActivated = 1.5f;
    #endregion

    #region Static Particle Types
    public static ParticleType P_Pressed = new ParticleType
    {
        Color = Calc.HexToColor("99e550"),
        ColorMode = ParticleType.ColorModes.Static,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.6f,
        LifeMax = 1f,
        Size = 1f,
        Direction = -(float)Math.PI / 2f,
        DirectionRange = (float)Math.PI / 4f,
        SpeedMin = 40f,
        SpeedMax = 80f,
        SpeedMultiplier = 0.2f
    };

    public static ParticleType P_ClutterFly = new ParticleType
    {
        Color = Calc.HexToColor("99e550"),
        ColorMode = ParticleType.ColorModes.Static,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.8f,
        LifeMax = 1.2f,
        Size = 1f,
        SpeedMin = 20f,
        SpeedMax = 40f,
        SpeedMultiplier = 0.1f
    };
    #endregion

    #region Private Fields
    private readonly ClutterBlock.Colors color;
    private readonly float startY;
    private float atY;
    private float speedY;
    private bool pressed;
    private bool playerWasOnTop;

    // Configurable music/audio fields
    private readonly string musicEvent;
    private readonly string absorbCutsceneSound;
    private readonly bool progressMusic;
    private readonly float configLightingAlphaAdd;
    private readonly bool disableLightning;

    // Visual Components
    private Sprite sprite;
    private Image icon;
    private VertexLight vertexLight;
    private float targetXScale = 1f;

    // Audio Components - Following Ingeste audio guidelines
    private SoundSource cutsceneSfx;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new ClutterSwitch at the specified position with the given color
    /// </summary>
    public ClutterSwitch(Vector2 position, ClutterBlock.Colors color,
        string musicEvent = "event:/music/pusheen/lvl5/clean",
        string absorbCutsceneSound = "event:/game/03_resort/clutterswitch_books",
        bool progressMusic = true,
        float configLightingAlphaAdd = LightingAlphaAdd,
        bool disableLightning = false)
        : base(position, 32f, 16f, safe: true)
    {
        this.color = color;
        this.musicEvent = musicEvent;
        this.absorbCutsceneSound = absorbCutsceneSound;
        this.progressMusic = progressMusic;
        this.configLightingAlphaAdd = configLightingAlphaAdd;
        this.disableLightning = disableLightning;
        startY = atY = base.Y;

        // Configure collision behavior
        OnDashCollide = OnDashed;
        SurfaceSoundIndex = 21;

        InitializeVisualComponents();
        InitializeLighting();
    }

    /// <summary>
    /// Entity data constructor for level editor integration
    /// </summary>
    public ClutterSwitch(EntityData data, Vector2 offset)
        : this(
            data.Position + offset,
            data.Enum("type", ClutterBlock.Colors.Green),
            data.Attr("musicEvent", "event:/music/pusheen/lvl5/clean"),
            data.Attr("absorbCutsceneSound", "event:/game/03_resort/clutterswitch_books"),
            data.Bool("progressMusic", true),
            data.Float("lightingAlphaAdd", LightingAlphaAdd),
            data.Bool("disableLightning", false))
    {
    }
    #endregion

    #region Initialization Methods
    private void InitializeVisualComponents()
    {
        // Setup sprite using Ingeste sprite system
        Add(sprite = GFX.SpriteBank.Create("clutterSwitch"));
        sprite.Position = new Vector2(16f, 16f);
        sprite.Play("idle");

        // Setup color-coded icon
        Add(icon = new Image(GFX.Game[$"objects/resortclutter/icon_{color}"]));
        icon.CenterOrigin();
        icon.Position = new Vector2(16f, 8f);
    }

    private void InitializeLighting()
    {
        Add(vertexLight = new VertexLight(
            new Vector2(base.CenterX - base.X, -1f),
            Color.Aqua,
            1f,
            UnpressedLightRadius,
            UnpressedLightRadius * 2
        ));
    }
    #endregion

    #region Scene Lifecycle
    public override void Added(Scene scene)
    {
        base.Added(scene);

        // Check if this switch was already activated in a previous session
        if (SceneAs<Level>().Session.GetFlag($"oshiro_clutter_cleared_{(int)color}"))
        {
            BePressed();
        }
    }

    public override void Removed(Scene scene)
    {
        // Ensure proper cleanup following Ingeste patterns
        CleanupAudio();
        RestoreLighting(scene as Level);
        base.Removed(scene);
    }

    private void CleanupAudio()
    {
        cutsceneSfx?.Stop();
        cutsceneSfx?.RemoveSelf();
        cutsceneSfx = null;
    }

    private void RestoreLighting(Level level)
    {
        if (level != null)
        {
            level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
        }
    }
    #endregion

    #region Update Logic
    public override void Update()
    {
        base.Update();

        UpdatePlayerInteraction();
        UpdateVisualEffects();
    }

    private void UpdatePlayerInteraction()
    {
        if (HasPlayerOnTop())
        {
            HandlePlayerOnTop();
        }
        else
        {
            HandlePlayerNotOnTop();
        }
    }

    private void HandlePlayerOnTop()
    {
        if (speedY < 0f) speedY = 0f;

        speedY = Calc.Approach(speedY, SquishSpeedApproach, SpeedChangeRate * Engine.DeltaTime);
        MoveTowardsY(atY + (pressed ? 2 : 4), speedY * Engine.DeltaTime);
        targetXScale = TargetScalePressed;

        if (!playerWasOnTop)
        {
            // Use Celeste's Audio system following Ingeste guidelines
            Audio.Play("event:/game/03_resort/clutterswitch_squish", Position);
        }

        playerWasOnTop = true;
    }

    private void HandlePlayerNotOnTop()
    {
        if (speedY > 0f) speedY = 0f;

        speedY = Calc.Approach(speedY, ReturnSpeedApproach, SpeedChangeRate * Engine.DeltaTime);
        MoveTowardsY(atY, -speedY * Engine.DeltaTime);
        targetXScale = 1f;

        if (playerWasOnTop)
        {
            Audio.Play("event:/game/03_resort/clutterswitch_return", Position);
        }

        playerWasOnTop = false;
    }

    private void UpdateVisualEffects()
    {
        sprite.Scale.X = Calc.Approach(sprite.Scale.X, targetXScale, ScaleApproachRate * Engine.DeltaTime);
    }
    #endregion

    #region Activation Logic
    /// <summary>
    /// Handles the switch being pressed (activated state)
    /// </summary>
    private void BePressed()
    {
        pressed = true;
        atY += PressedYOffset;
        base.Y += PressedYOffset;
        sprite.Y += PressedSpriteYOffset;
        sprite.Play("active");

        // Remove the icon when pressed
        if (icon != null)
        {
            Remove(icon);
            icon = null;
        }

        // Update lighting for pressed state
        vertexLight.StartRadius = PressedLightRadius;
        vertexLight.EndRadius = PressedLightRadius * 2;
    }

    /// <summary>
    /// Handles dash collision with proper game state management
    /// </summary>
    private DashCollisionResults OnDashed(global::Celeste.Player player1, Vector2 direction)
    {
        // Only activate on downward dash when not already pressed
        if (!pressed && direction == Vector2.UnitY)
        {
            ActivateSwitch(player1);
        }

        return DashCollisionResults.NormalCollision;
    }

    /// <summary>
    /// Main activation sequence following Ingeste cutscene patterns
    /// </summary>
    private void ActivateSwitch(global::Celeste.Player player)
    {
        // Immediate feedback
        CelesteGame.Freeze(0.2f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

        Level level = base.Scene as Level;

        // Set session flags matching ClutterDoor naming conventions
        level.Session.SetFlag($"oshiro_clutter_cleared_{(int)color}");
        level.Session.SetFlag("oshiro_clutter_door_open", setTo: true);

        // Visual effects
        UpdateActivationVisuals(level);
        BePressed();
        sprite.Scale.X = TargetScaleActivated;

        // Start absorption routine following Ingeste coroutine patterns
        Add(new Coroutine(AbsorbRoutine(player)));
    }

    private void UpdateActivationVisuals(Level level)
    {
        vertexLight.StartRadius = BrightLightRadius;
        vertexLight.EndRadius = ActivatedLightRadius;
        level.DirectionalShake(Vector2.UnitY, 0.6f);
        level.Particles.Emit(P_Pressed, 20, base.TopCenter - Vector2.UnitY * 10f, new Vector2(16f, 8f));
    }
    #endregion

    #region Absorption Routine
    /// <summary>
    /// Main absorption cutscene following Ingeste cutscene architecture
    /// Uses proper audio system and state management
    /// </summary>
    private IEnumerator AbsorbRoutine(global::Celeste.Player player)
    {
        yield return StartAbsorptionSequence(player);
        yield return ProcessClutterBlocks();
        yield return CompleteAbsorption(player);
    }

    private IEnumerator StartAbsorptionSequence(global::Celeste.Player player)
    {
        // Setup audio using proper Ingeste audio patterns
        SetupAbsorptionAudio();

        // Lock player state following Ingeste state management
        player.StateMachine.State = Player.StDummy;

        // Create absorption effect
        Vector2 target = Position + new Vector2(Width / 2f, 0f);
        ClutterAbsorbEffect effect = new ClutterAbsorbEffect();
        Scene.Add(effect);

        // Update sprite state
        sprite.Play("break");

        yield return SetupLightingEffects();

        yield return effect;
    }

    private void SetupAbsorptionAudio()
    {
        Add(cutsceneSfx = new SoundSource());

        if (!string.IsNullOrEmpty(absorbCutsceneSound))
        {
            float duration = GetAudioDurationForSound(absorbCutsceneSound);
            cutsceneSfx.Play(absorbCutsceneSound);

            // Setup completion audio using Celeste's Audio system
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
            {
                Audio.Play("event:/game/03_resort/clutterswitch_finish", Position);
            }, duration, start: true));
        }
    }

    private static float GetAudioDurationForSound(string sound)
    {
        return sound switch
        {
            "event:/game/03_resort/clutterswitch_books" => 6.366f,
            "event:/game/03_resort/clutterswitch_linens" => 6.15f,
            "event:/game/03_resort/clutterswitch_boxes" => 6.066f,
            _ => 6.366f
        };
    }

    private IEnumerator SetupLightingEffects()
    {
        Level level = SceneAs<Level>();

        // Update audio progress with configurable music event
        if (progressMusic)
        {
            if (!string.IsNullOrEmpty(musicEvent))
            {
                level.Session.Audio.Music.Event = musicEvent;
            }
            level.Session.Audio.Music.Progress++;
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        level.Session.LightingAlphaAdd -= configLightingAlphaAdd;

        // Create lighting transition effects
        yield return CreateLightingTransitions(level);
    }

    private IEnumerator CreateLightingTransitions(Level level)
    {
        float startAlpha = level.Lighting.Alpha;

        // Initial darkening effect
        Tween darkTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, start: true);
        darkTween.OnUpdate = (Tween t) =>
        {
            level.Lighting.Alpha = MathHelper.Lerp(startAlpha, 0.05f, t.Eased);
        };
        Add(darkTween);

        // Delayed restoration effect
        Alarm.Set(this, 3f, () =>
        {
            CreateLightingRestoration(level);
        });

        yield return null;
    }

    private void CreateLightingRestoration(Level level)
    {
        float startRadius = vertexLight.StartRadius;
        float endRadius = vertexLight.EndRadius;

        Tween restoreTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, start: true);
        restoreTween.OnUpdate = (Tween t) =>
        {
            level.Lighting.Alpha = MathHelper.Lerp(0.05f,
                level.BaseLightingAlpha + level.Session.LightingAlphaAdd, t.Eased);
            vertexLight.StartRadius = (int)Math.Round(MathHelper.Lerp(startRadius, PressedLightRadius, t.Eased));
            vertexLight.EndRadius = (int)Math.Round(MathHelper.Lerp(endRadius, PressedLightRadius * 2, t.Eased));
        };
        Add(restoreTween);
    }

    private IEnumerator ProcessClutterBlocks()
    {
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

        // Disable lightning if this is a lightning switch
        if (disableLightning)
        {
            yield return DisableLightning();
        }

        // Find and absorb clutter blocks following proper entity patterns
        AbsorbClutterEntities();

        yield return 1.5f;
    }

    private IEnumerator DisableLightning()
    {
        Level level = SceneAs<Level>();

        // Set flag to disable lightning
        level.Session.SetFlag("disable_lightning");

        // Find and disable all Lightning entities
        foreach (Lightning lightning in level.Entities.FindAll<Lightning>())
        {
            this.Add((Component)new Coroutine(Lightning.RemoveRoutine(this.SceneAs<Level>(), new Action(((Entity)lightning).RemoveSelf))));
        }

        yield return 0.5f;
    }

    private void AbsorbClutterEntities()
    {
        ClutterAbsorbEffect effect = Scene.Entities.FindFirst<ClutterAbsorbEffect>();
        if (effect == null) return;

        // Process ClutterBlock entities matching this switch's color
        foreach (ClutterBlock block in Scene.Entities.FindAll<ClutterBlock>())
        {
            if (block.BlockColor == color)
            {
                block.Absorb(effect);
            }
        }
    }

    private IEnumerator CompleteAbsorption(global::Celeste.Player player)
    {
        // Restore player state following Ingeste state management
        player.StateMachine.State = Player.StNormal;

        Level level = SceneAs<Level>();
        Vector2 target = Position + new Vector2(Width / 2f, 0f);
        ClutterAbsorbEffect effect = Scene.Entities.FindFirst<ClutterAbsorbEffect>();

        yield return CreateClutterFlyingEffect(level, target, effect);
        yield return FinalizeAbsorption(effect);
    }

    private IEnumerator CreateClutterFlyingEffect(Level level, Vector2 target, ClutterAbsorbEffect effect)
    {
        List<MTexture> images = GFX.Game.GetAtlasSubtextures($"objects/resortclutter/{color}_");

        // Create dramatic clutter flying effect
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Vector2 position = target + Calc.AngleToVector(
                    Calc.Random.NextFloat((float)(Math.PI * 2f)), 320f);
                effect?.FlyClutter(position, Calc.Random.Choose(images), shake: false, 0f);
            }

            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            yield return 0.05f;
        }
    }

    private IEnumerator FinalizeAbsorption(ClutterAbsorbEffect effect)
    {
        yield return 1.5f;

        effect?.CloseCabinets();
        yield return 0.2f;

        Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
        yield return 0.3f;
    }
    #endregion
}




