using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
  [HotReloadable]
  public class Cs02CallKirby : CutsceneEntity
  {

    private global::Celeste.Session flag;
    private global::Celeste.Player player;
    private Payphone payphone;
    private SoundSource phoneSfx;

    public Cs02CallKirby(global::Celeste.Player player)
      : base(false, true)
    {
      this.player = player;
      this.Add((Component) (this.phoneSfx = new SoundSource()));
    }

    public override void OnBegin(Level level)
    {   
      this.flag = level.Session;
      this.flag.Flags.Contains("MAGGYHELPER_CH2_ENDING_DONE");
      level.RegisterAreaComplete();
      this.payphone = this.Scene.Tracker.GetEntity<Payphone>();
      this.Add((Component) new Coroutine(this.cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
      Cs02CallKirby cs02Ending = this;
      cs02Ending.player.StateMachine.State = Player.StDummy;
      cs02Ending.player.Dashes = 1;
      while ((double) cs02Ending.player.Light.Alpha > 0.0)
      {
        cs02Ending.player.Light.Alpha -= Engine.DeltaTime * 1.25f;
        yield return (object) null;
      }
      yield return (object) 1f;
      yield return (object) cs02Ending.player.DummyWalkTo(cs02Ending.payphone.X - 4f);
      yield return (object) 0.2f;
      cs02Ending.player.Facing = Facings.Right;
      yield return (object) 0.5f;
      cs02Ending.player.Visible = false;
      Audio.Play("event:/game/02_old_site/sequence_phone_pickup", cs02Ending.player.Position);
      yield return (object) cs02Ending.payphone.Sprite.PlayRoutine("pickUp");
      yield return (object) 0.25f;
      cs02Ending.phoneSfx.Position = cs02Ending.player.Position;
      cs02Ending.phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
      yield return (object) 6f;
      cs02Ending.phoneSfx.Stop();
      cs02Ending.payphone.Sprite.Play("talkPhone");

      yield return (object) Textbox.Say("MAGGYHELPER_CH2_CALLING_KIRBY");

      yield return (object) 0.3f;
      cs02Ending.endCutscene(level);
    }

        private void endCutscene(Level level)
        {
            level.EndCutscene();
            OnEnd(level);
        }

        public override void OnEnd(Level level) => level.CompleteArea(true, true, false);
  }
}

