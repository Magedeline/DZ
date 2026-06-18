using System.Collections;
using Celeste.Mod.DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 - The Real End
    /// Final title-card and fade-out. Runs after CS21_FinalCutscenes regardless
    /// of which ending branch was taken. Marks the entire run as complete and
    /// returns the player to the overworld.
    /// </summary>
    [Tracked]
    public class CS21_RealTheEnd : CutsceneEntity
    {
        private const string MUSIC_THE_END = "event:/Mods/pusheen/music/menu/goodnight";

        private const string MUSIC_FAKE_THE_END = "event:/Mods/pusheen/music/menu/els_win";

        private Player player;
        private bool wasGoodEnding;
        private float overlayAlpha = 1f;
        private float titleAlpha   = 0f;

        public CS21_RealTheEnd(Player player, bool goodEnding) : base(false, true)
        {
            this.player       = player;
            this.wasGoodEnding = goodEnding;
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
            overlayAlpha = 1f;
            titleAlpha   = 0f;

            if (wasGoodEnding)
                Audio.SetMusic(MUSIC_THE_END);
            else
                Audio.SetMusic(MUSIC_FAKE_THE_END);

            // Brief pause in darkness
            yield return 2f;

            // Fade in the title card
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                titleAlpha   = Ease.CubeOut(t / 3f);
                overlayAlpha = 1f - Ease.SineOut(t / 3f) * 0.85f;
                yield return null;
            }
            titleAlpha   = 1f;
            overlayAlpha = 0.15f;

            yield return 4f;

            if (wasGoodEnding)
            {
                // Good ending: final hopeful line
                yield return Textbox.Say("DZ_CH21_REAL_END_GOOD");
            }
            else
            {
                // Normal ending: open-ended last line
                yield return Textbox.Say("DZ_CH21_REAL_END_NORMAL");
            }

            yield return 2f;

            // Fade everything out — title and all
            for (float t = 0f; t < 4f; t += Engine.DeltaTime)
            {
                float p = Ease.CubeIn(t / 4f);
                titleAlpha   = 1f - p;
                overlayAlpha = p;
                yield return null;
            }

            titleAlpha   = 0f;
            overlayAlpha = 1f;

            yield return 1f;

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.CompleteArea(false, false, false);

            level.Session.SetFlag("real_end_complete");
            level.Session.SetFlag("ch21_complete");

            if (DZModule.Session != null)
            {
                DZModule.Session.InCredits   = false;
                DZModule.Session.CreditsPhase = 0;
                DZModule.Session.CreditsCompleted = true;
            }

            level.TimerStopped = false;
            level.TimerHidden  = false;
            level.SaveQuitDisabled = false;
            level.PauseLock = false;

            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;
        }

        public override void Render()
        {
            base.Render();

            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * overlayAlpha);

            if (titleAlpha > 0f)
            {
                // "The End" title card
                Vector2 center = new Vector2(960f, 540f);

                ActiveFont.DrawOutline(
                    "The End",
                    center,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 2.5f,
                    Color.White * titleAlpha,
                    3f,
                    Color.Black * titleAlpha
                );

                string subtitle = wasGoodEnding
                    ? "DESOLO ZANTAS — A Complete Journey"
                    : "DESOLO ZANTAS — The Story Continues...";

                ActiveFont.DrawOutline(
                    subtitle,
                    center + new Vector2(0f, 100f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.8f,
                    Color.LightGray * titleAlpha,
                    2f,
                    Color.Black * titleAlpha
                );
            }
        }
    }
}