using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - Final Cutscenes
    /// Branching ending sequence triggered after the epilogue credits.
    ///
    /// Good Ending  : player fully defeated Els (siamo_zero_final_boss_defeated flag set).
    ///                Shows a hopeful resolution and peace.
    ///
    /// Normal Ending: player never faced or defeated the true final boss.
    ///                Shows an ambiguous, bittersweet close â€” the threat lingers.
    ///
    /// Both paths chain into CS21_RealTheEnd.
    /// </summary>
    [Tracked]
    public class CS21_FinalCutscenes : CutsceneEntity
    {
        // Session flag set by SiamoZeroFinalBoss when fully defeated
        private const string FLAG_ELS_DEFEATED = "siamo_zero_final_boss_defeated";

        // Dialogue keys
        private const string DIALOGUE_GOOD    = "DZ_CH21_ENDING_GOOD";
        private const string DIALOGUE_NORMAL  = "DZ_CH21_ENDING_NORMAL";

        // Music
        private const string MUSIC_GOOD_END   = "event:/Mods/pusheen/music/menu/goodnight";
        private const string MUSIC_NORMAL_END  = "event:/Mods/pusheen/music/menu/els_win";

        private Player player;
        private bool goodEnding;
        private float overlayAlpha;
        private Color overlayColor = Color.Black;

        public CS21_FinalCutscenes(Player player) : base(false, true)
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

            // Determine which branch to play
            goodEnding = level.Session.GetFlag(FLAG_ELS_DEFEATED);

            Add(new Coroutine(goodEnding ? GoodEndingSequence(level) : NormalEndingSequence(level)));
        }

        // â”€â”€ Good Ending â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private IEnumerator GoodEndingSequence(Level level)
        {
            overlayAlpha = 1f;
            overlayColor = Color.Black;

            Audio.SetMusic(MUSIC_GOOD_END);

            // Fade in to a warm scene
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                overlayAlpha = 1f - Ease.CubeOut(t / 3f);
                yield return null;
            }
            overlayAlpha = 0f;

            yield return 1f;

            // Els is gone. Peace at last.
            yield return Textbox.Say(DIALOGUE_GOOD);

            yield return 2f;

            // Screen brightens â€” the world heals
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                overlayColor = Color.White;
                overlayAlpha = Ease.CubeIn(t / 2f) * 0.6f;
                yield return null;
            }

            yield return 1f;

            // Fade to black and hand off to the real end
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                overlayColor = Color.Black;
                overlayAlpha = Ease.CubeIn(t / 2f);
                yield return null;
            }
            overlayAlpha = 1f;

            yield return 0.5f;

            level.Session.SetFlag("good_ending_played");
            EndCutscene(level);
        }

        // â”€â”€ Normal Ending â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private IEnumerator NormalEndingSequence(Level level)
        {
            overlayAlpha = 1f;
            overlayColor = Color.Black;

            Audio.SetMusic(MUSIC_NORMAL_END);

            // Slow fade in â€” the world is still uncertain
            for (float t = 0f; t < 4f; t += Engine.DeltaTime)
            {
                overlayAlpha = 1f - Ease.SineOut(t / 4f);
                yield return null;
            }
            overlayAlpha = 0f;

            yield return 1.5f;

            // The threat still lingersâ€¦
            yield return Textbox.Say(DIALOGUE_NORMAL);

            yield return 2f;

            // A distant rumble â€” Els is not truly gone
            Audio.Play("event:/Mods/pusheen/new_content/game/21_desolo_zantas/falling_into_the_void");

            yield return 1f;

            // Fade to black, ambiguous close
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                overlayColor = Color.Black;
                overlayAlpha = Ease.CubeIn(t / 3f);
                yield return null;
            }
            Audio.Play("event:/Mods/pusheen/new_content/game/21_desolo_zantas/final_laugh");
            overlayAlpha = 1f;

            yield return 0.5f;

            level.Session.SetFlag("normal_ending_played");
            EndCutscene(level);
        }

        // â”€â”€ Shared â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public override void OnEnd(Level level)
        {
            level.TimerStopped = false;
            level.TimerHidden  = false;
            level.SaveQuitDisabled = false;
            level.PauseLock = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            level.Session.SetFlag("final_cutscene_played");

            // Chain to the real end
            level.Add(new CS21_RealTheEnd(player, goodEnding));
        }

        public override void Render()
        {
            base.Render();
            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, overlayColor * overlayAlpha);
        }
    }
}
