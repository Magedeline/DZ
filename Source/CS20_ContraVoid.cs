using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper
{
    /// <summary>
    /// Cutscene for Chapter 20: Contra Void
    /// An overwhelming blinding light pulls Kirby into the event horizon —
    /// Els's mind of memories. The camera zooms in sharply, then eases back out.
    /// </summary>
    [Tracked(true)]
    public class CS20_ContraVoid : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_KEY = "CH20_CONTRA_VOID";
        private const string FLAG_DONE    = "ch20_contra_void_done";

        // The zoom surges further than Grand Sunset to sell the forced pull.
        private const float ZOOM_IN_TARGET   = 1.8f;
        private const float ZOOM_IN_DURATION  = 0.8f;   // fast, jarring pull
        private const float ZOOM_OUT_DURATION = 2.0f;   // slow, dreamlike return

        #endregion

        #region Fields

        private Player player;
        private Level  level;
        private float  originalZoom;

        #endregion

        #region Constructor

        public CS20_ContraVoid(Player player)
        {
            this.player = player;
        }

        public static void Start(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) return;

            level.Add(new CS20_ContraVoid(player));
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
            Glitch.Value = 0f;

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

            // ── White-out flash to represent the blinding light ─────────────
            level.Flash(Color.White * 0.85f, drawPlayerOver: false);
            Glitch.Value = 0.3f;
            yield return 0.15f;
            Glitch.Value = 0f;

            // ── Zoom IN — rapid, disorienting pull ─────────────────────────
            yield return ZoomTo(ZOOM_IN_TARGET, ZOOM_IN_DURATION, Ease.SineIn);

            yield return 0.3f;

            // ── Dialog ─────────────────────────────────────────────────────
            yield return Textbox.Say(DIALOGUE_KEY);

            // ── Zoom OUT — slow drift, like surfacing from a nightmare ──────
            yield return ZoomTo(originalZoom, ZOOM_OUT_DURATION, Ease.SineOut);

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
