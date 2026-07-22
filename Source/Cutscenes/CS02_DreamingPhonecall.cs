using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ;

public class CS02_DreamingPhonecallPortal : CutsceneEntity
{
    private global::Celeste.Entities.CharaDummy child;

    private Player player;

    private global::Celeste.Mod.DZ.Entities.Payphone payphone;

    private SoundSource ringtone;

    public CS02_DreamingPhonecallPortal(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        payphone = level.Tracker.GetEntity<global::Celeste.Mod.DZ.Entities.Payphone>();
        if (payphone == null || player == null)
        {
            Logger.Log(LogLevel.Warn, "DZ", "[CS02_DreamingPhonecallPortal] Missing payphone or player; aborting cutscene.");
            level.EndCutscene();
            RemoveSelf();
            return;
        }
        Add(new Coroutine(Cutscene(level)));
        Add(ringtone = new SoundSource());
        ringtone.Position = payphone.Position;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = 11;
        player.Dashes = 1;
        yield return 0.3f;
        ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
        while (player.Light.Alpha > 0f)
        {
            player.Light.Alpha -= Engine.DeltaTime * 2f;
            yield return null;
        }
        yield return 3.2f;
        yield return player.DummyWalkTo(payphone.X - 24f);
        yield return 1.5f;
        player.Facing = Facings.Left;
        yield return 1.5f;
        player.Facing = Facings.Right;
        yield return 0.25f;
        yield return player.DummyWalkTo(payphone.X - 4f);
        yield return 1.5f;
        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
        {
            ringtone.Param("end", 1f);
        }, 0.43f, start: true));
        player.Visible = false;
        Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
        yield return payphone.Sprite.PlayRoutine("pickUp");
        yield return 1f;
        if (level.Session.Area.Mode == AreaMode.Normal)
        {
            Audio.SetMusic("event:/DZ/music/lvl2/phone_loop");
        }
        payphone.Sprite.Play("talkPhone");
        yield return Textbox.Say("DZ_CH2_DREAM_PHONECALLPORTAL", ShowShadowMadeline);
        if (child != null)
        {
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                Audio.SetMusic("event:/DZ/music/lvl2/phone_end");
            }
            child.Vanish();
            child = null;
            yield return 1f;
        }
        Add(new Coroutine(WireFalls()));
        payphone.Broken = true;
        level.Shake(0.2f);
        VertexLight light = new VertexLight(new Vector2(16f, -28f), Color.White, 0f, 32, 48);
        payphone.Add(light);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 2f, start: true);
        tween.OnUpdate = delegate(Tween t)
        {
            light.Alpha = t.Eased;
        };
        Add(tween);
        Audio.Play("event:/game/02_old_site/sequence_phone_transform", payphone.Position);
        yield return payphone.Sprite.PlayRoutine("transform");
        yield return 0.4f;
        yield return payphone.Sprite.PlayRoutine("eat");
        payphone.Sprite.Play("monsterIdle");
        yield return 1.2f;
        level.EndCutscene();
        new FadeWipe(level, wipeIn: false, delegate
        {
            EndCutscene(level);
        });
    }

    private IEnumerator ShowShadowMadeline()
    {
        Level level = Scene as Level;
        if (level == null || payphone == null)
        {
            yield break;
        }
        yield return level.ZoomTo(new Vector2(240f, 116f), 2f, 0.5f);
        child = new global::Celeste.Entities.CharaDummy(payphone.Position + new Vector2(32f, -24f));
        child.Appear(level);
        Scene.Add(child);
        yield return 0.2f;
        payphone.Blink.X += 1f;
        yield return payphone.Sprite.PlayRoutine("jumpBack");
        yield return payphone.Sprite.PlayRoutine("scare");
        yield return 1.2f;
    }

    private IEnumerator WireFalls()
    {
        yield return 0.5f;
        Wire wire = Scene.Entities.FindFirst<Wire>();
        Vector2 speed = Vector2.Zero;
        Level level = SceneAs<Level>();
        while (wire != null && wire.Curve.Begin.X < (float)level.Bounds.Right)
        {
            speed += new Vector2(0.7f, 1f) * 200f * Engine.DeltaTime;
            wire.Curve.Begin += speed * Engine.DeltaTime;
            yield return null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        ringtone?.Stop();
        Leader.StoreStrawberries(player.Leader);
        level.ResetZoom();
        level.Bloom.Base = 0f;
        level.Remove(player);
        level.UnloadLevel();
        level.Session.Dreaming = false;
        level.Session.Level = "c-end_0";
        level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom));
        level.Session.Audio.Music.Event = "event:/DZ/music/lvl2/awake";
        level.Session.Audio.Ambience.Event = "event:/DZ/env/amb/02_awake";
        level.LoadLevel(Player.IntroTypes.WakeUp);
        level.EndCutscene();
        Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
    }
}
