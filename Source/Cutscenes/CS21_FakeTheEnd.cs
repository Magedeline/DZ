using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Fake The End
    /// A brief "it seems like it's over" moment played after CS21_Cast.
    /// Fades to black, plays a moment of silence, then chains to
    /// CS21_EpilogueCredits for the real credits sequence.
    /// </summary>
    [Tracked]
    public class CS21_FakeTheEnd : CutsceneEntity
    {
        private Player player;
        private float overlayAlpha = 1f;

        public CS21_FakeTheEnd(Player player) : base(false, true)
        {
            this.player = player;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (player == null)
                player = Scene.Tracker.GetEntity<Player>();
        }

        public override void OnBegin(Level level)
        {
            if (player == null)
                player = Scene.Tracker.GetEntity<Player>();

            level.TimerStopped = true;
            level.TimerHidden  = true;
            level.SaveQuitDisabled = true;
            level.PauseLock = true;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StDummy;

            Add(new Coroutine(Sequence(level)));
        }

        private IEnumerator Sequence(Level level)
        {
            // Start fully black
            overlayAlpha = 1f;
            yield return 1f;

            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                overlayAlpha = 1f - Ease.CubeOut(t / 2f);
                yield return null;
            }
            overlayAlpha = 0f;

            yield return 2f;

            // Dialogue: a moment of false peace
            yield return Textbox.Say("DZ_CH21_FAKE_THE_END");

            yield return 1f;

            // Something stirs in the dark â€” the light flickers
            Audio.Play("event:/DZ/new_content/game/21_desolo_zantas/transcendences");

            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                overlayAlpha = Ease.CubeIn(t);
                yield return null;
            }
            overlayAlpha = 1f;

            yield return 0.5f;

            Audio.SetMusic("event:/DZ/music/menu/els_win");

            // Fade back in â€” it's not over
            for (float t = 0f; t < 0.8f; t += Engine.DeltaTime)
            {
                overlayAlpha = 1f - Ease.CubeOut(t / 0.8f);
                yield return null;
            }
            overlayAlpha = 0f;

            yield return 0.5f;

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.TimerStopped = false;
            level.TimerHidden  = false;
            level.SaveQuitDisabled = false;
            level.PauseLock = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            level.Session.SetFlag("fake_end_played");

            // Chain to the epilogue credits
            level.Add(new CS21_EpilogueCredits(player));
        }

        public override void Render()
        {
            base.Render();
            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * overlayAlpha);
        }
    }
}
