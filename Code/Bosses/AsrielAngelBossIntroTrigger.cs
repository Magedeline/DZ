using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger for the Asriel Angel of Death Boss intro cutscene.
    /// Triggers when player enters and optionally checks flags.
    /// </summary>
    [CustomEntity("MaggyHelper/AsrielAngelBossIntroTrigger")]
    public class AsrielAngelBossIntroTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly string requireFlag;
        private readonly string requireNotFlag;
        private readonly string dialogKey;
        private readonly float shakeIntensity;
        private readonly float zoomDuration;
        private bool triggered = false;

        public AsrielAngelBossIntroTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            requireFlag = data.Attr("requireFlag", "");
            requireNotFlag = data.Attr("requireNotFlag", "asriel_angel_boss_intro");
            dialogKey = data.Attr("dialogKey", "ch20_asriel_angel_boss_intro");
            shakeIntensity = data.Float("shakeIntensity", 1.0f);
            zoomDuration = data.Float("zoomDuration", 0.6f);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (triggered) return;
            if (triggerOnce && triggered) return;

            Level level = SceneAs<Level>();
            Session session = level.Session;

            // Check required flag (if specified)
            if (!string.IsNullOrEmpty(requireFlag) && !session.GetFlag(requireFlag))
                return;

            // Check forbidden flag (if specified)
            if (!string.IsNullOrEmpty(requireNotFlag) && session.GetFlag(requireNotFlag))
                return;

            // Mark as triggered
            triggered = true;

            // Trigger the cutscene
            level.Add(new CS20_AsrielAngelOfDeathBossIntro(level.Session.Level));
        }
    }
}
