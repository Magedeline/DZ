using System.Runtime.CompilerServices;
using Celeste.Entities;
using FMOD.Studio;
using AscendManager = Celeste.AscendManager;
using Strawberry = global::Celeste.Strawberry;
using BirdNPC = Celeste.Entities.BirdNPC;

namespace Celeste.Cutscenes;

public class CS19_FinalLaunch : CutsceneEntity
{
    private global::Celeste.Player player;
    private CustomCharaBoost boost;
    private BirdNPC bird;
    private float fadeToWhite;
    private Vector2 birdScreenPosition;
    private AscendManagerBeyond.Streaks streaks;
    private Vector2 cameraWaveOffset;
    private Vector2 cameraOffset;
    private float timer;
    private Coroutine wave;
    private bool hasGolden;
    private bool hasPinkPlatinum;
    private bool sayDialog;
    private TimeRateModifier timeRateModifier;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS19_FinalLaunch(global::Celeste.Player player, CustomCharaBoost boost, bool sayDialog = true) : base(false, false)
    {
        this.player = player;
        this.boost = null;
        this.sayDialog = sayDialog;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
        base.Depth = 10010;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS19_FinalLaunch(global::Celeste.Player player, CustomCharaBoost Boost, bool hasGoldenStrawberry = false, bool hasPinkPlatinumBerry = false)
    {
        this.player = player;
        this.boost = Boost;
        this.sayDialog = !hasGoldenStrawberry && !hasPinkPlatinumBerry;
        this.hasGolden = hasGoldenStrawberry;
        this.hasPinkPlatinum = hasPinkPlatinumBerry;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
        base.Depth = 10010;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Audio.SetMusic(null);
        ScreenWipe.WipeColor = Color.White;
        foreach (Follower follower in player.Leader.Followers)
        {
            if (follower.Entity is Strawberry { Golden: not false })
            {
                hasGolden = true;
                break;
            }
            if (follower.Entity is Entities.PinkPlatinumBerry)
            {
                hasPinkPlatinum = true;
                break;
            }
        }
        Add(new Coroutine(Cutscene()));
    }

    private IEnumerator Cutscene()
    {
        timeRateModifier.ResetTimeRateMultiplier();
        if (boost != null)
        {
            boost.Active = false;
        }
        yield return true;
        if (sayDialog)
        {
            yield return Textbox.Say("CH19_CHARA_LAST_BOOST");
        }
        else
        {
            yield return 0.152f;
        }
        cameraOffset = new Vector2(0f, -20f);
        if (boost != null)
        {
            boost.Active = true;
        }
        player.EnforceLevelBounds = false;
        yield return null;

        RainbowBlackholeBg rainbowBlackholeBG = Level.Background.Get<RainbowBlackholeBg>();

        if (rainbowBlackholeBG != null)
        {
            rainbowBlackholeBG.Direction = -2.5f;
            rainbowBlackholeBG.SnapStrength(Level, RainbowBlackholeBg.Strengths.High);
            rainbowBlackholeBG.CenterOffset.Y = 100f;
            rainbowBlackholeBG.OffsetOffset.Y = -50f;
        }

        Add(wave = new Coroutine(WaveCamera()));
        Add(new Coroutine(BirdRoutine(0.8f)));
        Level.Add(streaks = new AscendManagerBeyond.Streaks(null));
        float p;
        for (p = 0f; p < 1f; p += Engine.DeltaTime / 12f)
        {
            fadeToWhite = p;
            streaks.Alpha = p;
            foreach (Parallax item in Level.Foreground.GetEach<Parallax>("rainbowblackhole"))
            {
                item.FadeAlphaMultiplier = 1f - p;
            }
            yield return null;
        }
        while (bird != null)
        {
            yield return null;
        }
        FadeWipe wipe = new FadeWipe(Level, wipeIn: false)
        {
            Duration = 4f
        };
        ScreenWipe.WipeColor = Color.White;
        if (!hasGolden)
        {
            timeRateModifier.ResetTimeRateMultiplier();
        }
        p = cameraOffset.Y;
        int to = 180;
        for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 2f)
        {
            cameraOffset.Y = p + ((float)to - p) * Ease.BigBackIn(p2);
            yield return null;
        }
        while (wipe.Percent < 1f)
        {
            yield return null;
        }
        EndCutscene(Level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        // Handle FMOD cleanup for CustomCharaBoost
        EventInstance sfxToCleanup = null;
        
        if (boost != null && boost.Ch19FinalBoostSfx != null)
        {
            sfxToCleanup = boost.Ch19FinalBoostSfx;
        }
        
        if (WasSkipped && sfxToCleanup != null)
        {
            try
            {
                sfxToCleanup.stop(STOP_MODE.ALLOWFADEOUT);
                sfxToCleanup.release();
            }
            catch (System.Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DesoloZantas", $"FMOD cleanup error (non-critical): {ex.Message}");
            }
        }

        player.Active = true;
        player.Speed = Vector2.Zero;
        player.EnforceLevelBounds = true;
        player.StateMachine.State = 0;
        player.DummyFriction = true;
        player.DummyGravity = true;
        player.DummyAutoAnimate = true;
        player.ForceCameraUpdate = false;
        timeRateModifier.ResetTimeRateMultiplier();
        Level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
        {
            if (Level.Wipe != null)
            {
                Level.Wipe.Cancel();
            }
            
            if (hasGolden)
            {
                Level.SnapColorGrade("golden");
            }
            else if (hasPinkPlatinum)
            {
                Level.SnapColorGrade("pinkgameboy");
            }
            
            new FadeWipe(level, wipeIn: true).Duration = 2f;
            ScreenWipe.WipeColor = Color.White;
            
            // Complete the area - this is the end of Chapter 19 before the final boss in Ch20
            // Use spotlightWipe=true for golden/platinum berries, otherwise normal completion
            level.CompleteArea(spotlightWipe: hasGolden || hasPinkPlatinum, skipScreenWipe: false, skipCompleteScreen: false);
        };
    }

    private IEnumerator WaveCamera()
    {
        float timer = 0f;
        while (true)
        {
            cameraWaveOffset.X = (float)Math.Sin(timer) * 16f;
            cameraWaveOffset.Y = (float)Math.Sin(timer * 0.5f) * 16f + (float)Math.Sin(timer * 0.25f) * 8f;
            timer += Engine.DeltaTime * 2f;
            yield return null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator BirdRoutine(float delay)
    {
        yield return delay;
        Level.Add(bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
        bird.Sprite.Play("flyupIdle");
        Vector2 vector = new Vector2(320f, 180f) / 2f;
        Vector2 topCenter = new Vector2(vector.X, 0f);
        Vector2 vector2 = new Vector2(vector.X, 180f);
        Vector2 from = vector2 + new Vector2(40f, 40f);
        Vector2 to = vector + new Vector2(-32f, -24f);
        for (float t = 0f; t < 1f; t += Engine.DeltaTime / 4f)
        {
            birdScreenPosition = from + (to - from) * Ease.BackOut(t);
            yield return null;
        }
        bird.Sprite.Play("flyupRoll");
        for (float t = 0f; t < 1f; t += Engine.DeltaTime / 2f)
        {
            birdScreenPosition = to + new Vector2(64f, 0f) * Ease.CubeInOut(t);
            yield return null;
        }
        to = birdScreenPosition;
        from = topCenter + new Vector2(-40f, -100f);
        bool playedAnim = false;
        for (float t = 0f; t < 1f; t += Engine.DeltaTime / 4f)
        {
            if (t >= 0.35f && !playedAnim)
            {
                bird.Sprite.Play("flyupRoll");
                playedAnim = true;
            }
            birdScreenPosition = to + (from - to) * Ease.BigBackIn(t);
            birdScreenPosition.X += t * 32f;
            yield return null;
        }
        bird.RemoveSelf();
        bird = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
        if (bird != null)
        {
            bird.Position = Level.Camera.Position + birdScreenPosition;
            bird.Position.X += (float)Math.Sin(timer) * 4f;
            bird.Position.Y += (float)Math.Sin(timer * 0.1f) * 4f + (float)Math.Sin(timer * 0.25f) * 4f;
        }
        Level.CameraOffset = cameraOffset + cameraWaveOffset;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        Camera camera = Level.Camera;
        Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
    }
}







