using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
  [HotReloadable]
  public class Cs04Ending : CutsceneEntity
  {

    private global::Celeste.Session flag;
    private global::Celeste.Player player;
    private Payphone payphone;
    private SoundSource phoneSfx;

    public Cs04Ending(global::Celeste.Player player)
      : base(false, true)
    {
      this.player = player;
      this.Add((Component) (this.phoneSfx = new SoundSource()));
    }

    public override void OnBegin(Level level)
    {   
      this.flag = level.Session;
      this.flag.Flags.Contains("CH4_ENDING_DONE");
      level.RegisterAreaComplete();
      this.payphone = this.Scene.Tracker.GetEntity<Payphone>();
      this.Add((Component) new Coroutine(this.cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
      Cs04Ending cs04Ending = this;
      cs04Ending.player.StateMachine.State = 11;
      cs04Ending.player.Dashes = 1;
      while ((double) cs04Ending.player.Light.Alpha > 0.0)
      {
        cs04Ending.player.Light.Alpha -= Engine.DeltaTime * 1.25f;
        yield return (object) null;
      }
      yield return (object) 1f;
      yield return (object) cs04Ending.player.DummyWalkTo(cs04Ending.payphone.X - 4f);
      yield return (object) 0.2f;
      cs04Ending.player.Facing = Facings.Right;
      yield return (object) 0.5f;
      cs04Ending.player.Visible = false;
      Audio.Play("event:/game/02_old_site/sequence_phone_pickup", cs04Ending.player.Position);
      yield return (object) cs04Ending.payphone.Sprite.PlayRoutine("pickUp");
      yield return (object) 0.25f;
      cs04Ending.phoneSfx.Position = cs04Ending.player.Position;
      cs04Ending.phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
      yield return (object) 6f;
      cs04Ending.phoneSfx.Stop();
      cs04Ending.payphone.Sprite.Play("talkPhone");
      yield return (object) Textbox.Say("CH4_CALLING_MOM_ENDING");
      yield return (object) 0.3f;
      cs04Ending.endCutscene(level);
    }

        private void endCutscene(Level level)
        {
            level.EndCutscene();
            OnEnd(level);
        }

        public override void OnEnd(Level level) => level.CompleteArea(true, true, false);
  }
}
