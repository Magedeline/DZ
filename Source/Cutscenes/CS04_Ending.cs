using Facings = Celeste.Facings;
using Payphone = Celeste.Mod.DZ.Entities.Payphone;

namespace Celeste.Cutscenes
{
  [HotReloadable]
  public class Cs04Ending : CutsceneEntity
  {

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
      level.RegisterAreaComplete();
      this.payphone = level.Tracker.GetEntity<Payphone>();
      if (this.payphone == null || this.player == null)
      {
        Logger.Log(LogLevel.Warn, "DZ", "[Cs04Ending] Missing payphone or player; aborting cutscene.");
        level.EndCutscene();
        RemoveSelf();
        return;
      }
      this.Add((Component) new Coroutine(this.cutscene(level)));
    }

    private IEnumerator cutscene(Level level)
    {
      if (player == null || payphone == null)
      {
        Logger.Log(LogLevel.Warn, "DZ", "[Cs04Ending] Missing payphone or player during cutscene; aborting.");
        level.EndCutscene();
        yield break;
      }
      player.StateMachine.State = Player.StDummy;
      player.Dashes = 1;
      while (player.Light.Alpha > 0f)
      {
        player.Light.Alpha -= Engine.DeltaTime * 1.25f;
        yield return null;
      }
      yield return 1f;
      yield return player.DummyWalkTo(payphone.X - 4f);
      yield return 0.2f;
      player.Facing = Facings.Right;
      yield return 0.5f;
      player.Visible = false;
      Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
      yield return payphone.Sprite.PlayRoutine("pickUp");
      yield return 0.25f;
      phoneSfx.Position = player.Position;
      phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
      yield return 6f;
      phoneSfx.Stop();
      payphone.Sprite.Play("talkPhone");
      yield return Textbox.Say("DZ_CH4_CALLING_MOM_ENDING");
      yield return 0.3f;
      level.EndCutscene();
    }

    public override void OnEnd(Level level)
    {
      phoneSfx.Stop();
      level.CompleteArea(true, true, false);
    }
  }
}

