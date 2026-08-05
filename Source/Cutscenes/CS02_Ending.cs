using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS02_End : CutsceneEntity
    {
        private Player player;
        private Payphone payphone;
        private SoundSource phoneSfx;

        public CS02_End(Player player)
            : base(false, true)
        {
            this.player = player;
            Add(phoneSfx = new SoundSource());
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            payphone = Scene.Tracker.GetEntity<Payphone>();
            if (payphone == null)
            {
                RemoveSelf();
                return;
            }
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS02_End cs02Ending = this;
            cs02Ending.player.StateMachine.State = 11;
            cs02Ending.player.Dashes = 1;
            if (cs02Ending.player.Light != null)
            {
                while (cs02Ending.player.Light.Alpha > 0.0)
                {
                    cs02Ending.player.Light.Alpha -= Engine.DeltaTime * 1.25f;
                    yield return null;
                }
            }
            yield return 1f;
            yield return cs02Ending.player.DummyWalkTo(cs02Ending.payphone.X - 4f);
            yield return 0.2f;
            cs02Ending.player.Facing = Facings.Right;
            yield return 0.5f;
            cs02Ending.player.Visible = false;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup", cs02Ending.player.Position);
            yield return cs02Ending.payphone.Sprite.PlayRoutine("pickUp");
            yield return 0.25f;
            cs02Ending.phoneSfx.Position = cs02Ending.player.Position;
            cs02Ending.phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
            yield return 6f;
            cs02Ending.phoneSfx.Stop();
            cs02Ending.payphone.Sprite.Play("talkPhone");
            yield return Textbox.Say("DZ_CH2_END_PHONECALL");
            yield return 0.3f;
            cs02Ending.EndCutscene(level);
        }

        public override void OnEnd(Level level) => level.CompleteArea(true, false, false);
    }
}