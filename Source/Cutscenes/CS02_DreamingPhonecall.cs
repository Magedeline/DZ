#nullable disable

using Celeste.Entities;
using Payphone = Celeste.Entities.Payphone;
using Wire = Celeste.Entities.Wire;

namespace Celeste.Cutscenes
{
  [HotReloadable]
  public class Cs02DreamingPhonecallPortal : CutsceneEntity
  {
    private CharaDummy evil;

    private global::Celeste.Session flag;

    private global::Celeste.Player player;
    private Payphone payphone;
    private SoundSource ringtone;

    public Cs02DreamingPhonecallPortal(global::Celeste.Player player)
        : base(false)
    {
      this.player = player;
    }

    public override void OnBegin(Level level)
    {
      this.flag = level.Session;
      this.flag.Flags.Contains("DZ_CH2_DREAM_PHONECALL_DONE");
      level.Session.Dreaming = true;
      this.payphone = this.Scene.Tracker.GetEntity<Payphone>();
      this.Add(new Coroutine(this.cutscene(level)));
      this.Add(this.ringtone = new SoundSource());
      if (this.payphone != null)
        this.ringtone.Position = this.payphone.Position;
    }

    private IEnumerator cutscene(Level level)
    {
      player.StateMachine.State = Player.StDummy;
      player.Dashes = 1;
      yield return 0.3f;
      ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
      while (player.Light.Alpha > 0.0f)
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
      this.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => this.ringtone.Param("end", 1f), 0.43f, true));
      player.Visible = false;
      Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
      yield return payphone.Sprite.PlayRoutine("pickUp");
      yield return 1f;
      if (level.Session.Area.Mode == AreaMode.Normal)
        Audio.SetMusic("event:/music/pusheen/lvl2/phone_loop");
      payphone.Sprite.Play("talkPhone");

      yield return Textbox.Say("DZ_CH2_DREAM_PHONECALLPORTAL", showChara);
      if (evil != null)
      {
        if (level.Session.Area.Mode == AreaMode.Normal)
          Audio.SetMusic("event:/music/pusheen/lvl2/phone_end");
        evil.Any();
        evil = null;
        yield return 1f;
      }
      this.Add(new Coroutine(wireFalls()));
      payphone.Broken = true;
      level.Shake(0.2f);
      VertexLight light = new VertexLight(new Vector2(16f, -28f), Color.White, 0.0f, 32, 48);
      payphone.Add(light);
      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 2f, start: true);
      tween.OnUpdate = t => light.Alpha = t.Eased;
      this.Add(tween);
      Audio.Play("event:/game/pusheen/02_nightmare/sequence_phone_portal", payphone.Position);
      yield return payphone.Sprite.PlayRoutine("transform");
      yield return 0.4f;
      yield return payphone.Sprite.PlayRoutine("eat");
      payphone.Sprite.Play("monsterIdle");
      yield return 1.2f;
      level.EndCutscene();
      _ = new FadeWipe(level, false, () => this.endCutscene(level));
    }

        private void endCutscene(Level level)
        {
            level.ResetZoom();
        }

        private IEnumerator showChara()
    {
      Payphone payphone = this.Scene?.Tracker?.GetEntity<Payphone>();
      Level level = this.Scene as Level;
      if (payphone == null || level == null)
        yield break;

      yield return level.ZoomTo(new Vector2(240f, 116f), 2f, 0.5f);

      evil = new CharaDummy(payphone.Position + new Vector2(32f, -24f));
      this.Scene.Add(evil);
        evil.Added(level); // Moved after Add to ensure Scene is set
      yield return 0.2f;
      ++payphone.Blink.X;
      yield return payphone.Sprite.PlayRoutine("jumpBack");
      yield return payphone.Sprite.PlayRoutine("scare");
      yield return 1.2f;
    }

    private IEnumerator wireFalls()
    {
      yield return 0.5f;
      Wire wire = this.Scene.Entities.FindFirst<Wire>();
      Vector2 speed = Vector2.Zero;
      Level level = this.SceneAs<Level>();
      while (wire != null && wire.Curve.Begin.X < level.Bounds.Right)
      {
        speed += new Vector2(0.7f, 1f) * 200f * Engine.DeltaTime;
        // Must replace the entire Curve struct since SimpleCurve is a value type
        var curve = wire.Curve;
        curve.Begin += speed * Engine.DeltaTime;
        wire.Curve = curve;
        yield return null;
      }
    }

    public void EmitParticles() => this.Add(new Coroutine(this.particlesCoroutine()));

    private IEnumerator particlesCoroutine()
    {
      for (int i = 0; i < 10; i++)
      {
        yield return 0.1f;
      }
    }

    /// <summary>
    /// Safely vanishes the evil CharaDummy if it exists.
    /// </summary>
    public void VanishEvil()
    {
        if (evil != null)
        {
            evil.Any();
            evil = null;
        }
    }

    public override void OnEnd(Level level)
    {
      Leader.StoreStrawberries(this.player.Leader);
      level.ResetZoom();
      level.Bloom.Base = 0.0f;
      level.Remove(this.player);
      level.UnloadLevel();
      level.Session.Dreaming = false;
      level.Session.Level = "end_0";
      Session session = level.Session;
      Rectangle bounds = level.Bounds;
      Vector2 from = new Vector2(bounds.Left, bounds.Bottom);
      session.RespawnPoint = level.GetSpawnPoint(from);
      level.Session.Audio.Music.Event = "event:/music/pusheen/lvl2/awake";
      level.Session.Audio.Ambience.Event = "event:/env/pusheen/02_awake";
      level.LoadLevel(global::Celeste.Player.IntroTypes.WakeUp);
      level.EndCutscene();
      Leader.RestoreStrawberries(level.Tracker.GetEntity<global::Celeste.Player>().Leader);
    }
  }
}

