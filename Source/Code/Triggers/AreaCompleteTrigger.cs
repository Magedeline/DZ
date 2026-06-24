using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that initiates the area complete sequence with white fade effects.
    /// Optionally transitions to a next level after completion.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/AreaCompleteTrigger")]
    public class AreaCompleteTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private readonly string nextLevel = data.Attr("nextLevel", "");
        private readonly bool hasGoldenStrawberry = data.Bool("hasGoldenStrawberry", false);
        private readonly bool hasPinkPlatinumBerry = data.Bool("hasPinkPlatinumBerry", false);
        private readonly bool skipCredits = data.Bool("skipCredits", false);
        private readonly bool triggerOnce = data.Bool("triggerOnce", true);
        private bool hasTriggered = false;
        private Level level;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!triggerOnce || !hasTriggered)
            {
                StartAreaComplete();
            }
        }

        private void StartAreaComplete()
        {
            if (level == null)
                return;

            hasTriggered = true;

            // Create and add the area complete cutscene
            string nextLevelName = string.IsNullOrEmpty(nextLevel) ? null : nextLevel;
            
            CS19_AreaComplete areaComplete = new(
                hasGoldenStrawberry: hasGoldenStrawberry,
                hasPinkPlatinumBerry: hasPinkPlatinumBerry,
                skipCredits: skipCredits,
                nextLevelName: nextLevelName
            );

            level.Add(areaComplete);

            // Remove this trigger to prevent re-triggering
            if (triggerOnce)
            {
                RemoveSelf();
            }
        }
    }
}
