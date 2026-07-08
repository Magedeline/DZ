using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Asriel Joking
    /// A brief comedic beat after the dramatic fake god ascension.
    /// Asriel breaks the tension with a lighthearted moment before
    /// chaining into CS21_SpecialThanksDodgeCredits.
    /// </summary>
    [Tracked]
    public class AsrielJoking : CutsceneEntity
    {
        private Player player;
        private float overlayAlpha = 1f;

        public AsrielJoking(Player player) : base(false, true)
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

            level.TimerStopped     = true;
            level.TimerHidden      = true;
            level.SaveQuitDisabled = true;
            level.PauseLock        = true;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StDummy;

            Add(new Coroutine(Sequence(level)));
        }

        private IEnumerator Sequence(Level level)
        {
            overlayAlpha = 1f;

            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                overlayAlpha = 1f - Ease.CubeOut(t / 1.5f);
                yield return null;
            }
            overlayAlpha = 0f;

            yield return 1f;

            // Asriel says something unexpectedly goofy
            yield return Textbox.Say("DZ_CH21_ASRIEL_JOKING");

            yield return 1.5f;

            // Fade back to black
            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                overlayAlpha = Ease.CubeIn(t / 1.5f);
                yield return null;
            }
            overlayAlpha = 1f;

            yield return 0.5f;

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.TimerStopped     = false;
            level.TimerHidden      = false;
            level.SaveQuitDisabled = false;
            level.PauseLock        = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            level.Session.SetFlag("asriel_joking_played");

            // Chain to special thanks / dodge credits
            level.Add(new CS21_SpecialThanksDodgeCredits(player));
        }

        public override void Render()
        {
            base.Render();
            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * overlayAlpha);
        }
    }
}
