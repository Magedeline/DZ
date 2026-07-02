using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;
using AngyOshiro = DZ.AngyOshiro;
using ModRoofEnding = global::Celeste.Entities.ResortRoofEnding;

namespace Celeste.Cutscenes;

public class CS05_Ending : CutsceneEntity
{
    private class BgFlash : Entity
    {
        public float Alpha;

        public BgFlash()
        {
            base.Depth = 10100;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            Camera camera = (base.Scene as Level).Camera;
            Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * Alpha);
        }
    }

    public const string Flag = "oshiroEndingalt";

    private ModRoofEnding roof;

    private AngyOshiro angyOshiro;

    private Player player;

    private Entity oshiro;

    private Sprite oshiroSprite;

    private TimeRateModifier timeRateModifier;

    private EventInstance smashSfx;

    private bool smashRumble;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS05_Ending(ModRoofEnding roof, Player player)
        : base(fadeInOnSkip: false, endingChapterAfter: true)
    {
        this.roof = roof;
        this.player = player;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
        base.Depth = -1000000;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        level.RegisterAreaComplete();
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (smashRumble)
        {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = Player.StDummy;
        player.StateMachine.Locked = true;
        player.ForceCameraUpdate = false;
        Add(new Coroutine(player.DummyRunTo(roof.X + roof.Width - 32f, fastAnim: true)));
        yield return null;
        player.DummyAutoAnimate = false;
        yield return 0.5f;
        angyOshiro = Scene.Entities.FindFirst<AngyOshiro>();
        Add(new Coroutine(MoveGhostTo(new Vector2(roof.X + 40f, roof.Y - 12f))));
        yield return 1f;
        player.DummyAutoAnimate = true;
        yield return level.ZoomTo(new Vector2(130f, 60f), 2f, 0.5f);
        player.Facing = Facings.Left;
        yield return 0.5f;
        yield return Textbox.Say("DZ_CH5_OSHIRODZ_CHASE_END", GhostSmash);
        yield return GhostSmash(0.5f, final: true);
        Audio.SetMusic(null);
        oshiroSprite = null;
        BgFlash bgFlash = new BgFlash
        {
            Alpha = 1f
        };
        level.Add(bgFlash);
        Distort.GameRate = 0f;
        Sprite sprite = GFX.SpriteBank.Create("oshiro_boss_lightning");
        sprite.Position = angyOshiro.Position + new Vector2(140f, -100f);
        sprite.Rotation = Calc.Angle(sprite.Position, angyOshiro.Position + new Vector2(0f, 10f));
        sprite.Play("once");
        Add(sprite);
        yield return null;
        global::Celeste.Celeste.Freeze(0.3f);
        yield return null;
        level.Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        smashRumble = false;
        yield return 0.2f;
        Distort.GameRate = 1f;
        level.Flash(Color.White);
        player.DummyGravity = false;
        angyOshiro.Sprite.Play("transformBack");
        player.Sprite.Play("fall");
        roof.BeginFalling = true;
        yield return null;
        timeRateModifier.SetTimeRateMultiplier(0.01f);
        player.Sprite.Play("fallFast");
        player.DummyGravity = true;
        player.Speed.Y = -200f;
        player.Speed.X = 300f;
        Vector2 oshiroFallSpeed = new Vector2(-100f, -250f);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 1.5f, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween tween2) =>
        {
            angyOshiro.Sprite.Rotation = tween2.Eased * -100f * (MathF.PI / 180f);
        };
        Add(tween);
        float t;
        for (t = 0f; t < 2f; t += Engine.DeltaTime)
        {
            oshiroFallSpeed.X = Calc.Approach(oshiroFallSpeed.X, 0f, Engine.DeltaTime * 400f);
            oshiroFallSpeed.Y += Engine.DeltaTime * 800f;
            angyOshiro.Position += oshiroFallSpeed * Engine.DeltaTime;
            bgFlash.Alpha = Calc.Approach(bgFlash.Alpha, 0f, Engine.RawDeltaTime);
            timeRateModifier.SetTimeRateMultiplier(Calc.Approach(timeRateModifier.CurrentTimeRate(), 1f, Engine.RawDeltaTime * 0.6f));
            yield return null;
        }
        timeRateModifier.ResetTimeRateMultiplier();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
        yield return 1f;
        while (!player.OnGround())
        {
            player.MoveV(1f);
        }
            timeRateModifier.SetTimeRateMultiplier(Calc.Approach(timeRateModifier.CurrentTimeRate(), 1f, Engine.RawDeltaTime * 0.6f));
        player.Sprite.Play("tired");
        angyOshiro.RemoveSelf();
        Scene.Add(oshiro = new Entity(new Vector2(level.Bounds.Left + 110, player.Y)));
        oshiro.Add(oshiroSprite = GFX.SpriteBank.Create("oshiro"));
        oshiroSprite.Play("fall");
        oshiroSprite.Scale.X = 1f;
        oshiro.Collider = new Hitbox(8f, 8f, -4f, -8f);
        oshiro.Add(new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 16, 32));
        yield return CutsceneEntity.CameraTo(player.CameraTarget + new Vector2(0f, 40f), 1f, Ease.CubeOut);
        yield return 1.5f;
        Audio.SetMusic("event:/pusheen/music/lvl5/intro");
        yield return 3f;
        Audio.Play("event:/char/oshiro/chat_get_up", oshiro.Position);
        oshiroSprite.Play("recover");
        float target = oshiro.Y + 4f;
        while (oshiro.Y != target)
        {
            oshiro.Y = Calc.Approach(oshiro.Y, target, 6f * Engine.DeltaTime);
            yield return null;
        }
        yield return 0.6f;
        yield return Textbox.Say("DZ_CH5_ENDING", OshiroTurns);
        Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(-80f, 0f), 3f)));
        yield return 0.5f;
        oshiroSprite.Scale.X = -1f;
        yield return 0.2f;
        t = 0f;
        oshiro.Add(new SoundSource("event:/char/oshiro/move_08_roof07_exit"));
        while (oshiro.X > (float)(level.Bounds.Left - 16))
        {
            oshiro.X -= 40f * Engine.DeltaTime;
            Sprite sprite2 = oshiroSprite;
            float num;
            t = (num = t + Engine.DeltaTime * 2f);
            sprite2.Y = (float)Math.Sin(num) * 2f;
            oshiro.CollideFirst<Door>()?.Open(oshiro.X);
            yield return null;
        }
        Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(80f, 0f), 2f)));
        yield return 1.2f;
        player.DummyAutoAnimate = true;
        yield return player.DummyWalkTo(player.X - 16f);
        yield return 2f;
        player.Facing = Facings.Right;
        yield return 1f;
        player.ForceCameraUpdate = false;
        player.Add(new Coroutine(RunPlayerRight()));
        EndCutscene(level);
    }

    private IEnumerator OshiroTurns()
    {
        yield return 1f;
        oshiroSprite.Scale.X = -1f;
        yield return 0.2f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator MoveGhostTo(Vector2 target)
    {
        if (angyOshiro != null)
        {
            target.Y -= angyOshiro.Height / 2f;
            angyOshiro.EnterDummyMode();
            angyOshiro.Collidable = false;
            while (angyOshiro.Position != target)
            {
                angyOshiro.Position = Calc.Approach(angyOshiro.Position, target, 64f * Engine.DeltaTime);
                yield return null;
            }
        }
    }

    private IEnumerator GhostSmash()
    {
        yield return GhostSmash(0f, final: false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator GhostSmash(float topDelay, bool final)
    {
        if (angyOshiro == null)
        {
            yield break;
        }
        if (final)
        {
            smashSfx = Audio.Play("event:/char/oshiro/boss_slam_final", angyOshiro.Position);
        }
        else
        {
            smashSfx = Audio.Play("event:/char/oshiro/boss_slam_first", angyOshiro.Position);
        }
        float from = angyOshiro.Y;
        float to = angyOshiro.Y - 32f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
        {
            angyOshiro.Y = MathHelper.Lerp(from, to, Ease.CubeOut(p));
            yield return null;
        }
        yield return topDelay;
        float ground = from + 20f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 8f)
        {
            angyOshiro.Y = MathHelper.Lerp(to, ground, Ease.CubeOut(p));
            yield return null;
        }
        angyOshiro.Squish();
        Level.Shake(0.5f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        smashRumble = true;
        roof.StartShaking(0.5f);
        if (!final)
        {
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 16f)
            {
                angyOshiro.Y = MathHelper.Lerp(ground, from, Ease.CubeOut(p));
                yield return null;
            }
        }
        else
        {
            angyOshiro.Y = (ground + from) / 2f;
        }
        if (angyOshiro != null)
        {
            player.DummyAutoAnimate = false;
            if (player.Sprite.Has("shaking"))
            {
                player.Sprite.Play("shaking");
            }
            roof.Wobble(null, final);
            if (!final)
            {
                yield return 0.5f;
            }
        }
    }

    private IEnumerator RunPlayerRight()
    {
        yield return 0.75f;
        yield return player.DummyRunTo(player.X + 128f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        timeRateModifier.ResetTimeRateMultiplier();
        Audio.SetMusic("event:/pusheen/music/lvl5/intro");
        Audio.Stop(smashSfx);
        Level.CompleteArea(true, true);
        SpotlightWipe.FocusPoint = new Vector2(192f, 120f);
    }
}

