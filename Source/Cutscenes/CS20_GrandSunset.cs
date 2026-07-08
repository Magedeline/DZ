using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    /// <summary>
    /// Cutscene for Chapter 20: Grand Sunset
    /// Kirby witnesses Zero 3 — a living black hole made of pure darkness.
    /// The camera dramatically zooms in and then pulls back out.
    /// </summary>
    [Tracked(true)]
    public class CS20_GrandSunset : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_KEY = "DZ_CH20_GRAND_SUNSET";
        private const string FLAG_DONE    = "ch20_grand_sunset_done";

        private const float ZOOM_IN_TARGET = 1.6f;
        private const float ZOOM_IN_DURATION  = 1.2f;
        private const float ZOOM_OUT_DURATION = 1.5f;

        #endregion

        #region Fields

        private Player player;
        private Level  level;
        private float  originalZoom;

        #endregion

        #region Constructor

        public CS20_GrandSunset(Player player)
        {
            this.player = player;
        }

        public static void Start(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) return;

            level.Add(new CS20_GrandSunset(player));
        }

        #endregion

        #region Lifecycle

        public override void OnBegin(Level level)
        {
            this.level    = level;
            originalZoom  = level.Zoom;
            Add(new Coroutine(CutsceneSequence()));
        }

        public override void OnEnd(Level level)
        {
            // Restore zoom and player state if the cutscene was skipped.
            level.Zoom = originalZoom;

            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State  = Player.StNormal;
                player.DummyAutoAnimate    = true;
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

            // ── Zoom IN ────────────────────────────────────────────────────
            yield return ZoomTo(ZOOM_IN_TARGET, ZOOM_IN_DURATION, Ease.CubeIn);

            // Brief pause so the player can feel the dread.
            yield return 0.4f;

            // ── Dialog ─────────────────────────────────────────────────────
            yield return Textbox.Say(DIALOGUE_KEY);

            // ── Zoom OUT ───────────────────────────────────────────────────
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
