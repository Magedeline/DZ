#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses
{
    /// <summary>
    /// HP-gated colour/backdrop trigger for Phase 2 of the Kirby Flying Final Battle.
    ///
    /// Place one of these in the room for each HP threshold you want to react to during
    /// Phase 2. When the player walks into it the trigger calls
    /// KirbyFinalBattleScene.NotifyPhaseShift, which shifts the colour tint and can
    /// boost the scroll backdrop speed.
    ///
    /// Properties (all set from the Loenn entity panel):
    ///   phaseIndex       0..4   — index into KirbyFinalBattleScene.PhaseColors
    ///   colorGrade       string — Celeste colour-grade key (blank = no change)
    ///   triggerOnce      bool   — remove self after first fire (default true)
    ///   requiredFlag     string — session flag that must be set before firing
    ///   setFlag          string — session flag set on fire
    ///   shakeScreen      bool   — camera shake on fire
    ///   shakeIntensity   float  — shake strength
    ///   flashScreen      bool   — phase-colour screen flash
    ///   flashAlpha       float  — flash opacity
    ///   scrollSpeedBoost float  — added to FlyingBattleScrollBackdrop.ScrollSpeedX (px/s)
    /// </summary>
    [CustomEntity("DZ/FinalBattlePhaseShiftTrigger")]
    [HotReloadable]
    public class FinalBattlePhaseShiftTrigger : Trigger
    {
        private readonly int    phaseIndex;
        private readonly string colorGrade;
        private readonly bool   triggerOnce;
        private readonly string requiredFlag;
        private readonly string setFlag;
        private readonly bool   shakeScreen;
        private readonly float  shakeIntensity;
        private readonly bool   flashScreen;
        private readonly float  flashAlpha;
        private readonly float  scrollSpeedBoost;

        private bool fired;

        public FinalBattlePhaseShiftTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            phaseIndex       = System.Math.Clamp(data.Int("phaseIndex", 0), 0,
                                   KirbyFinalBattleScene.PhaseColors.Length - 1);
            colorGrade       = data.Attr("colorGrade", "");
            triggerOnce      = data.Bool("triggerOnce", true);
            requiredFlag     = data.Attr("requiredFlag", "");
            setFlag          = data.Attr("setFlag", "");
            shakeScreen      = data.Bool("shakeScreen", true);
            shakeIntensity   = data.Float("shakeIntensity", 0.3f);
            flashScreen      = data.Bool("flashScreen", true);
            flashAlpha       = data.Float("flashAlpha", 0.4f);
            scrollSpeedBoost = data.Float("scrollSpeedBoost", 0f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (fired) return;

            Level level = SceneAs<Level>();

            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
                return;

            fired = true;

            if (!string.IsNullOrEmpty(colorGrade))
                level.SnapColorGrade(colorGrade);

            if (shakeScreen)
                level.Shake(shakeIntensity);

            if (flashScreen)
            {
                Color c = KirbyFinalBattleScene.PhaseColors[phaseIndex] * flashAlpha;
                level.Flash(c, true);
            }

            KirbyFinalBattleScene.Get(Scene)?.NotifyPhaseShift(phaseIndex, scrollSpeedBoost);

            if (!string.IsNullOrEmpty(setFlag))
                level.Session.SetFlag(setFlag);

            if (triggerOnce)
                RemoveSelf();
        }
    }
}
