using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper
{
    /// <summary>
    /// Cutscene for Chapter 20: Tesseract Soul
    /// Els presents the Tesseract Soul — an entity that can warp space and time.
    /// The camera performs a measured zoom-in to reveal it, then zooms back out
    /// once Els asserts control.
    /// </summary>
    [Tracked(true)]
    public class CS20_TesseractSoul : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_KEY = "CH20_TESSERACT_SOUL";
        private const string FLAG_DONE    = "ch20_tesseract_soul_done";

        private const float ZOOM_IN_TARGET   = 1.5f;
        private const float ZOOM_IN_DURATION  = 1.6f;  // slow and deliberate
        private const float ZOOM_OUT_DURATION = 1.2f;  // snappy pullback as Els reasserts power

        #endregion

        #region Fields

        private Player player;
        private Level  level;
        private float  originalZoom;

        #endregion

        #region Constructor

        public CS20_TesseractSoul(Player player)
        {
            this.player = player;
        }

        public static void Start(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) return;

            level.Add(new CS20_TesseractSoul(player));
        }

        #endregion

        #region Lifecycle

        public override void OnBegin(Level level)
        {
            this.level   = level;
            originalZoom = level.Zoom;
            Add(new Coroutine(CutsceneSequence()));
        }

        public override void OnEnd(Level level)
        {
            level.Zoom = originalZoom;

            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State = Player.StNormal;
                player.DummyAutoAnimate   = true;
            }

            level.Session.SetFlag(FLAG_DONE);
        }

        #endregion

        #region Sequence

        private IEnumerator CutsceneSequence()
        {
            // Lock player input.
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate   = false;

            // ── Zoom IN — slow, deliberate revelation of the Tesseract Soul ─
            yield return ZoomTo(ZOOM_IN_TARGET, ZOOM_IN_DURATION, Ease.CubeInOut);

            yield return 0.5f;

            // ── Dialog ─────────────────────────────────────────────────────
            yield return Textbox.Say(DIALOGUE_KEY);

            // ── Zoom OUT — quick snap back as Els takes back control ─────
            yield return ZoomTo(originalZoom, ZOOM_OUT_DURATION, Ease.CubeOut);

            EndCutscene(level);
        }

        #endregion

        #region Helpers

        /// <summary>Smoothly interpolates <c>level.Zoom</c> to <paramref name="target"/> over <paramref name="duration"/> seconds.</summary>
        private IEnumerator ZoomTo(float target, float duration, Ease.Easer easer)
        {
            float start = level.Zoom;
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                level.Zoom = Calc.LerpClamp(start, target, easer(t / duration));
                yield return null;
            }
            level.Zoom = target;
        }

        #endregion
    }
}
