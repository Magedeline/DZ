using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    /// <summary>
    /// In-game cutscene event for DZ_CH20_DREAM_FRIENDS_CAPTURED.
    /// Plays dialog and executes trigger callbacks 0..4 from the dialog script.
    /// </summary>
    [Tracked(true)]
    public class CS20_DreamFriendsCaptured : CutsceneEntity
    {
        private const string DIALOGUE_KEY = "DZ_CH20_DREAM_FRIENDS_CAPTURED";
        private const string FLAG_DONE = "ch20_dream_friends_captured_done";

        private readonly Player player;
        private Level level;

        public CS20_DreamFriendsCaptured(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            this.level = level;
            Add(new Coroutine(CutsceneSequence()));
        }

        public override void OnEnd(Level level)
        {
            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyAutoAnimate = true;
            }

            level.Session.SetFlag(FLAG_DONE);
        }

        private IEnumerator CutsceneSequence()
        {
            if (player == null)
            {
                EndCutscene(level);
                yield break;
            }

            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;

            yield return Textbox.Say(
                DIALOGUE_KEY,
                Trigger0ShiftBackToKirby,
                Trigger1CaptureAllies,
                Trigger2LiftKirbySoul,
                Trigger3ZeroDamagesSoul,
                Trigger4MuteMusic
            );

            EndCutscene(level);
        }

        private IEnumerator Trigger0ShiftBackToKirby()
        {
            if (level == null || player == null)
                yield break;

            level.Shake(0.2f);
            level.Displacement.AddBurst(player.Center, 0.25f, 24f, 120f, 0.5f);
            yield return 0.2f;
        }

        private IEnumerator Trigger1CaptureAllies()
        {
            if (level == null || player == null)
                yield break;

            Audio.Play("event:/game/06_reflection/boss_spikes_burst", player.Center);
            level.Flash(Calc.HexToColor("560000") * 0.25f, false);
            level.Shake(0.35f);

            for (int i = 0; i < 12; i++)
            {
                level.ParticlesFG.Emit(Player.P_DashB, 1, player.Center, Vector2.One * 18f);
                yield return 0.02f;
            }

            yield return 0.2f;
        }

        private IEnumerator Trigger2LiftKirbySoul()
        {
            if (player == null)
                yield break;

            Vector2 from = player.Position;
            Vector2 to = from + new Vector2(0f, -28f);

            Audio.Play("event:/game/06_reflection/feather_state", player.Center);
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.6f)
            {
                player.Position = Vector2.Lerp(from, to, Ease.CubeOut(t));
                yield return null;
            }

            player.Position = to;
            yield return 0.1f;
        }

        private IEnumerator Trigger3ZeroDamagesSoul()
        {
            if (level == null || player == null)
                yield break;

            level.Shake(0.5f);
            level.Flash(Color.White * 0.3f, false);
            Audio.Play("event:/char/badeline/boss_hug", player.Center);
            yield return 0.25f;
            yield return 0.2f;
        }

        private IEnumerator Trigger4MuteMusic()
        {
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            yield return null;
        }
    }
}
