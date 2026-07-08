using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Standalone trigger for the CS09_MessageEnd cutscene.
    /// Sans voice message warning about Mt. Desolo Zantas.
    /// </summary>
    [CustomEntity(ids: "DZ/CS09_MessageEndTrigger")]
    public class CS09_MessageEndTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool requireFlag;
        private readonly string requiredFlag;
        private bool hasTriggered;
        private Level level;

        public CS09_MessageEndTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            requireFlag = data.Bool("requireFlag", false);
            requiredFlag = data.Attr("requiredFlag", "");
            hasTriggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && hasTriggered)
                return;

            // Check required flag if specified
            if (requireFlag && !string.IsNullOrEmpty(requiredFlag) && level != null)
            {
                if (!level.Session.GetFlag(requiredFlag))
                    return;
            }

            // Check if message already shown
            if (level != null && level.Session.GetFlag(CS09_MessageEnd.FlagMessageComplete))
                return;

            StartMessage(player);
        }

        private void StartMessage(global::Celeste.Player player)
        {
            if (level == null || player == null)
                return;

            hasTriggered = true;

            var messageCutscene = new CS09_MessageEnd(player);
            level.Add(messageCutscene);
        }
    }
}
