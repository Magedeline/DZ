using System.Reflection;
using DZ;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Entities;

[CustomEntity("DZ/Tape")]
public class DesoloZantasTape : Entity
{
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Inner UI Cutscene â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private sealed class UnlockedCSideCutscene : Entity
    {
        private float alpha, textAlpha;
        public readonly string[] text;
        private bool waitForKeyPress;
        private float timer;
        private readonly string menuSprite;
        public int textIndex;

        public UnlockedCSideCutscene(string[] unlockText, string menuSprite)
        {
            text = unlockText;
            this.menuSprite = menuSprite;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Tag = Tags.HUD | Tags.PauseUpdate;
            for (int i = 0; i < text.Length; i++)
                text[i] = ActiveFont.FontSize.AutoNewline(Dialog.Clean(text[i]), 900);
            Depth = -10000;
        }

        public IEnumerator EaseIn()
        {
            while ((textAlpha = (alpha += Engine.DeltaTime / 0.5f)) < 1f)
                yield return null;
            alpha = 1f;
            yield return 1.5f;
            waitForKeyPress = true;
        }

        public IEnumerator EaseOut()
        {
            waitForKeyPress = false;
            while ((textAlpha = (alpha -= Engine.DeltaTime / 0.5f)) > 0f)
                yield return null;
            alpha = 0f;
            RemoveSelf();
        }

        public IEnumerator NextText()
        {
            while ((textAlpha -= Engine.DeltaTime / 0.5f) > 0f)
                yield return null;
            textIndex++;
            while ((textAlpha += Engine.DeltaTime / 0.5f) < 1f)
                yield return null;
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            float a   = Ease.CubeOut(alpha);
            float ta  = Ease.CubeOut(textAlpha);
            Vector2 center = global::Celeste.Celeste.TargetCenter + new Vector2(0f, 64f);
            Vector2 slide  = Vector2.UnitY * 64f * (1f - a);
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * a * 0.8f);
            GFX.Gui[menuSprite].DrawJustified(center - slide + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * a);
            ActiveFont.Draw(text[Math.Min(textIndex, text.Length - 1)], center + slide, new Vector2(0.5f, 0f), Vector2.One, Color.White * ta);
            if (waitForKeyPress)
                GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((timer % 1f < 0.25f) ? 6 : 0)));
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Static Hook Infrastructure â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //
    // Mirrors the JungleHelper/CassetteCustomPreviewMusic pattern:
    // an ILHook on CollectRoutine's compiler-generated state machine intercepts
    // the hardcoded FMOD event / param strings so each tape instance can supply
    // its own values without the coroutine needing direct field access.

    private static ILHook _hookCollectRoutine;

    /// <summary>Call from DZHooks.Load().</summary>
    public static void Load()
    {
        var smTarget = typeof(DesoloZantasTape)
            .GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetStateMachineTarget();

        if (smTarget != null)
            _hookCollectRoutine = new ILHook(smTarget, PatchCollectRoutineAudio);
        else
            Logger.Log(LogLevel.Warn, "DZ/Tape", "Could not find CollectRoutine state machine â€” audio IL patch skipped.");
    }

    /// <summary>Call from DZHooks.Unload().</summary>
    public static void Unload()
    {
        _hookCollectRoutine?.Dispose();
        _hookCollectRoutine = null;
    }

    private static void PatchCollectRoutineAudio(ILContext il)
    {
        // Grab the compiler-generated "<>4__this" field via reflection (same as JungleHelper).
        // GetStateMachineTarget() returns a System.Reflection.MethodInfo so DeclaringType is System.Type.
        FieldInfo selfField = typeof(DesoloZantasTape)
            .GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetStateMachineTarget()
            ?.DeclaringType
            ?.GetField("<>4__this");

        if (selfField == null)
        {
            Logger.Log(LogLevel.Warn, "DZ/Tape", "Could not find <>4__this in CollectRoutine state machine.");
            return;
        }

        var cursor = new ILCursor(il);

        // Patch preview FMOD event string
        PatchLiteral<string>(cursor, selfField, DefaultPreviewEvent,
            self => self._previewEvent, "preview event");

        cursor.Index = 0;

        // Patch preview FMOD param name string
        PatchLiteral<string>(cursor, selfField, DefaultPreviewParamName,
            self => self._previewParamName, "preview param name");
    }

    private static void PatchLiteral<T>(ILCursor cursor, FieldInfo selfField, string literal,
        Func<DesoloZantasTape, T> getter, string label)
    {
        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr(literal)))
        {
            Logger.Log(LogLevel.Warn, "DZ/Tape", $"Could not find '{literal}' ({label}) in CollectRoutine IL.");
            return;
        }
        Logger.Log(LogLevel.Debug, "DZ/Tape", $"Patching tape {label} at IL index {cursor.Index}.");
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, selfField);
#pragma warning disable CL0001
        cursor.EmitDelegate<Func<T, DesoloZantasTape, T>>((orig, self) => getter(self));
#pragma warning restore CL0001
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Audio/Visual Defaults â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private const string DefaultCollectSfx      = "event:/pusheen/game/general/tape_get";
    private const string DefaultPreviewEvent     = "event:/pusheen/game/general/tape_preview";
    private const string DefaultPreviewParamName = "remix";

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Particle Types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public static ParticleType P_Shine;
    public static ParticleType P_Collect;

    private static void EnsureParticles()
    {
        if (P_Shine == null)
        {
            P_Shine = new ParticleType
            {
                Color = Calc.HexToColor("9CFCFF"),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                Size = 1f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 15f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f
            };
        }

        if (P_Collect == null)
        {
            P_Collect = new ParticleType(P_Shine)
            {
                LifeMin = 0.2f,
                LifeMax = 0.6f,
                SpeedMin = 20f,
                SpeedMax = 40f
            };
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Instance State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public bool IsGhost;

    private Sprite  _sprite;
    private SineWave _hover;
    private BloomPoint _bloom;
    private VertexLight _light;
    private Wiggler _scaleWiggler;
    private bool _collected;
    private bool _collecting;
    private Vector2[] _nodes;
    private EventInstance _remixSfx;

    // â”€â”€â”€â”€â”€ Configurable audio â”€â”€â”€â”€â”€
    private readonly string _collectSfx;       // sound played on touch
    private readonly string _previewEvent;     // FMOD event for the preview music loop
    private readonly string _previewParamName; // FMOD parameter name on the preview event
    private readonly float  _previewParamValue; // parameter value (-1 = use area ID)

    // â”€â”€â”€â”€â”€ Configurable visuals â”€â”€â”€â”€â”€
    private readonly string _spritePath;  // in-game sprite atlas folder
    private readonly string _menuSprite;  // UI sprite shown in the unlock cutscene

    // â”€â”€â”€â”€â”€ Unlock data â”€â”€â”€â”€â”€
    private readonly string[]  _unlockText;
    private readonly string    _cSideToUnlock;

    // â”€â”€â”€â”€â”€ Visual tuning â”€â”€â”€â”€â”€
    private readonly Color _particleColor;
    private readonly float _glowStrength;
    private readonly float _bloomStrength;
    private readonly float _wiggleIntensity;
    private readonly float _floatSpeed;
    private readonly float _floatRange;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Constructors â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [MethodImpl(MethodImplOptions.NoInlining)]
    public DesoloZantasTape(Vector2 position, Vector2[] nodes) : base(position)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        _nodes   = nodes;
        Add(new PlayerCollider(OnPlayer));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public DesoloZantasTape(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.NodesOffset(offset))
    {
        // Visuals
        _spritePath     = data.Attr("spritePath",   "collectables/cassette/");
        _menuSprite     = data.Attr("menuSprite",   "collectables/tape");
        _particleColor  = Calc.HexToColor(data.Attr("particleColor", "FF9CCF"));
        _glowStrength   = data.Float("glowStrength",   1.0f);
        _bloomStrength  = data.Float("bloomStrength",  0.8f);
        _wiggleIntensity= data.Float("wiggleIntensity",0.35f);
        _floatSpeed     = data.Float("floatSpeed",     2.0f);
        _floatRange     = data.Float("floatRange",     2.0f);

        // Audio
        _collectSfx       = data.Attr("collectSfx",       DefaultCollectSfx);
        _previewEvent     = data.Attr("previewEvent",      DefaultPreviewEvent);
        _previewParamName = data.Attr("previewParamName",  DefaultPreviewParamName);
        _previewParamValue= data.Float("previewParamValue",-1f); // -1 = use area ID

        // Unlock  (e.g. "map/campaingname/mapname/map.bin")
        _cSideToUnlock = data.Attr("cSideToUnlock", "");
        _unlockText    = ResolveUnlockText(data.Attr("unlockText", ""), _cSideToUnlock);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Entity Lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        EnsureParticles();

        IsGhost = !string.IsNullOrEmpty(_cSideToUnlock)
               && IngesteModule.SaveData.UnlockedCSideIDs.Contains(_cSideToUnlock);

        string animKey = IsGhost ? "ghost" : "idle";
        _sprite = new Sprite(GFX.Game, _spritePath);
        _sprite.Add("idle",  animKey, 0.07f, "pulse", new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        _sprite.Add("spin",  animKey, 0.07f, "spin",  new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
        _sprite.Add("pulse", animKey, 0.04f, "idle",  new[] { 13, 14, 15, 16, 17, 18 });
        _sprite.CenterOrigin();
        _sprite.Play("idle");
        Add(_sprite);

        Add(_scaleWiggler = Wiggler.Create(0.25f, 4f, f =>
            _sprite.Scale = Vector2.One * (1f + f * _wiggleIntensity)));
        Add(_bloom = new BloomPoint(_bloomStrength, 16f));
        Add(_light = new VertexLight(_particleColor, _glowStrength * 0.4f, 32, 64));
        Add(_hover = new SineWave(_floatSpeed, 0f));
        _hover.OnUpdate = f =>
        {
            float y = f * _floatRange;
            _sprite.Y = _light.Y = _bloom.Y = y;
        };

        if (IsGhost)
            _sprite.Color = Color.White * 0.8f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        Audio.Stop(_remixSfx);
        base.SceneEnd(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        Audio.Stop(_remixSfx);
        base.Removed(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (!_collecting && Scene.OnInterval(0.1f))
            SceneAs<Level>().Particles.Emit(P_Shine, 1, Center, new Vector2(12f, 10f));
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Collection Logic â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(CelestePlayer player)
    {
        if (_collected) return;

        player?.RefillStamina();
        Audio.Play(_collectSfx, Position);   // custom unlock SFX on touch
        _collected = true;
        global::Celeste.Celeste.Freeze(0.1f);
        Add(new Coroutine(CollectRoutine(player)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator CollectRoutine(CelestePlayer player)
    {
        _collecting = true;
        Level level = SceneAs<Level>();
        TapeBlockManager cbm = Scene.Tracker.GetEntity<TapeBlockManager>();

        level.PauseLock = true;
        level.Frozen    = true;
        Tag = Tags.FrozenUpdate;

        level.Session.Cassette = true;
        level.Session.RespawnPoint = level.GetSpawnPoint(_nodes[1]);
        level.Session.UpdateLevelStartDashes();

        if (!string.IsNullOrEmpty(_cSideToUnlock))
        {
            IngesteModule.SaveData.UnlockedCSideIDs.Add(_cSideToUnlock);
            if (!IngesteModule.SaveData.PendingCSideUnlockIDs.Contains(_cSideToUnlock))
                IngesteModule.SaveData.PendingCSideUnlockIDs.Add(_cSideToUnlock);
        }

        DZ.DZProgressionManager.RecordCassette(level);
        if (level.Session.RespawnPoint.HasValue)
            DZ.DZProgressionManager.RecordCheckpoint(level, level.Session.RespawnPoint.Value, _cSideToUnlock);

        cbm?.StopBlocks();
        Depth = -1000000;
        level.Shake();
        level.Flash(Color.White);
        level.Displacement.Clear();

        Vector2 camWas = level.Camera.Position;
        Vector2 camTo  = (Position - new Vector2(160f, 90f)).Clamp(
            level.Bounds.Left - 64, level.Bounds.Top - 32,
            level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
        level.Camera.Position = camTo;
        level.ZoomSnap((Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2f);

        _sprite.Play("spin", restart: true);
        _sprite.Rate = 2f;
        for (float p = 0f; p < 1.5f; p += Engine.DeltaTime)
        {
            _sprite.Rate += Engine.DeltaTime * 4f;
            yield return null;
        }
        _sprite.Rate = 0f;
        _sprite.SetAnimationFrame(0);
        _scaleWiggler.Start();
        yield return 0.25f;

        Vector2 from     = Position;
        Vector2 to       = new Vector2(X, level.Camera.Top - 16f);
        float   duration = 0.4f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
        {
            _sprite.Scale.X = MathHelper.Lerp(1f, 0.1f, p);
            _sprite.Scale.Y = MathHelper.Lerp(1f, 3.0f, p);
            Position = Vector2.Lerp(from, to, Ease.CubeIn(p));
            yield return null;
        }
        Visible = false;

        // These string literals are intercepted at runtime by PatchCollectRoutineAudio
        // so that _previewEvent / _previewParamName take effect â€” exact JungleHelper pattern.
        float paramValue = _previewParamValue >= 0f ? _previewParamValue : (float)level.Session.Area.ID;
        _remixSfx = Audio.Play(DefaultPreviewEvent, DefaultPreviewParamName, paramValue);

        var message = new UnlockedCSideCutscene(_unlockText, _menuSprite);
        Scene.Add(message);
        yield return message.EaseIn();

        while (message.textIndex < message.text.Length)
        {
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            if (message.textIndex != message.text.Length - 1)
                yield return message.NextText();
            else
                break;
        }

        Audio.SetParameter(_remixSfx, "end", 1f);
        yield return message.EaseOut();

        duration = 0.25f;
        Add(new Coroutine(level.ZoomBack(duration - 0.05f)));
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
        {
            level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p));
            yield return null;
        }

        if (!player.Dead && _nodes != null && _nodes.Length >= 2)
        {
            Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
            player.StartCassetteFly(_nodes[1], _nodes[0]);
        }

        foreach (SandwichLava lava in level.Entities.FindAll<SandwichLava>())
            lava.Leave();

        level.Frozen = false;
        yield return 0.25f;
        cbm?.Finish();
        level.PauseLock = false;
        level.ResetZoom();
        RemoveSelf();
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static string[] ResolveUnlockText(string custom, string cSideToUnlock)
    {
        if (!string.IsNullOrEmpty(custom))
            return custom.Split(',');

        string up = cSideToUnlock.ToUpperInvariant();
        if (up.Contains("BSIDE") || up.Contains("B_SIDE")) return new[] { "Maggy_BSide_unlocked" };
        if (up.Contains("CSIDE") || up.Contains("C_SIDE")) return new[] { "Maggy_CSide_unlocked" };
        if (up.Contains("DSIDE") || up.Contains("D_SIDE")) return new[] { "Maggy_DSide_unlocked" };
        if (up.Contains("REMIX"))                           return new[] { "Maggy_RemixExtra_unlocked" };
        return new[] { "Maggy_CSide_unlocked" };
    }
}



