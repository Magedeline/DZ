using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Fake Asriel God Of Hyper Death
    /// A dramatic false climax played after CS21_Cast.
    /// Asriel appears to ascend to full godhood â€” it looks like the real end â€”
    /// but it is a feint that chains into AsrielJoking.
    /// </summary>
    [Tracked]
    public class FakeAsrielGodOFHD : CutsceneEntity
    {
        private Player player;
        private float overlayAlpha = 1f;
        private Color overlayColor = Color.Black;

        public FakeAsrielGodOFHD(Player player) : base(false, true)
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
            overlayColor = Color.Black;

            yield return 1f;

            // Asriel's power erupts â€” a blinding white flash
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "end", 1f);

            for (float t = 0f; t < 2.5f; t += Engine.DeltaTime)
            {
                overlayColor = Color.White;
                overlayAlpha = Ease.CubeIn(t / 2.5f);
                yield return null;
            }
            overlayAlpha = 1f;

            yield return 0.8f;

            // Fade back from white â€” the form is revealed
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                overlayColor = Color.White;
                overlayAlpha = 1f - Ease.SineOut(t / 2f);
                yield return null;
            }
            overlayAlpha = 0f;
            overlayColor = Color.Black;

            yield return 1.5f;

            yield return Textbox.Say("MAGGYHELPER_CH21_FAKE_ASRIEL_GOD_OF_HD");

            yield return 1f;

            // "Ascension" flash â€” screen goes pure white again, then blacks out
            Audio.Play("event:/new_content/char/pusheen/asriel/Asriel_Segapower02");

            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                overlayColor = Color.White;
                overlayAlpha = Ease.CubeIn(t);
                yield return null;
            }
            overlayAlpha = 1f;

            yield return 0.3f;

            for (float t = 0f; t < 1.2f; t += Engine.DeltaTime)
            {
                overlayColor = Color.Black;
                overlayAlpha = Ease.CubeIn(t / 1.2f);
                yield return null;
            }
            overlayAlpha = 1f;
            overlayColor = Color.Black;

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

            level.Session.SetFlag("fake_asriel_god_ofhd_played");

            // Chain to Asriel's joke moment
            level.Add(new AsrielJoking(player));
        }

        public override void Render()
        {
            base.Render();
            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, overlayColor * overlayAlpha);
        }
    }
}
