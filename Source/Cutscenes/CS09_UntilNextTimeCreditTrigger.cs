using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Standalone trigger for the CS09_Credits cutscene.
    /// "Until Next Time..." credits sequence for Chapter 9.
    /// </summary>
    [CustomEntity(ids: "DZ/CS09_UntilNextTimeCreditTrigger")]
    public class CS09_UntilNextTimeCreditTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool requireFlag;
        private readonly string requiredFlag;
        private bool hasTriggered;
        private Level level;

        public CS09_UntilNextTimeCreditTrigger(EntityData data, Vector2 offset) : base(data, offset)
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

            // Check if credits already shown
            if (level != null && level.Session.GetFlag(CS09_Credits.FlagCreditsComplete))
                return;

            StartCredits(player);
        }

        private void StartCredits(global::Celeste.Player player)
        {
            if (level == null || player == null)
                return;

            hasTriggered = true;

            var creditsCutscene = new CS09_Credits(player);
            level.Add(creditsCutscene);
        }
    }
}
