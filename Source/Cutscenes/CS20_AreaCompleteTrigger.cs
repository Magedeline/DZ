using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Chapter 20 Area Complete Trigger - Handles the final boss completion sequence.
    /// Initiates the CS20_FinalBossDefeat cutscene followed by area completion.
    /// </summary>
    [CustomEntity(ids: "DZ/CS20_AreaCompleteTrigger")]
    public class CS20_AreaCompleteTrigger : Trigger
    {
        private readonly string nextLevel;
        private readonly bool hasGoldenStrawberry;
        private readonly bool hasPinkPlatinumBerry;
        private readonly bool skipCredits;
        private readonly bool triggerOnce;
        private readonly bool playFinalBossDefeatCutscene;
        private bool hasTriggered;
        private Level level;

        public CS20_AreaCompleteTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            nextLevel = data.Attr("nextLevel", "");
            hasGoldenStrawberry = data.Bool("hasGoldenStrawberry", false);
            hasPinkPlatinumBerry = data.Bool("hasPinkPlatinumBerry", false);
            skipCredits = data.Bool("skipCredits", false);
            triggerOnce = data.Bool("triggerOnce", true);
            playFinalBossDefeatCutscene = data.Bool("playFinalBossDefeatCutscene", true);
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

            if (!triggerOnce || !hasTriggered)
            {
                StartChapter20Complete(player);
            }
        }

        private void StartChapter20Complete(global::Celeste.Player player)
        {
            if (level == null || player == null)
                return;

            hasTriggered = true;

            // Check if we should play the final boss defeat cutscene first
            if (playFinalBossDefeatCutscene && !level.Session.GetFlag("final_boss_defeated"))
            {
                // Play the CS20_FinalBossDefeat cutscene
                CS20_FinalBossDefeat finalBossDefeat = new CS20_FinalBossDefeat(player);
                level.Add(finalBossDefeat);

                // After the final boss defeat cutscene, trigger the area complete
                level.OnEndOfFrame += () =>
                {
                    if (!level.Entities.Contains(finalBossDefeat))
                    {
                        TriggerAreaComplete();
                    }
                };
            }
            else
            {
                // Skip straight to area complete if cutscene already played
                TriggerAreaComplete();
            }

            // Remove this trigger to prevent re-triggering
            if (triggerOnce)
            {
                RemoveSelf();
            }
        }

        private void TriggerAreaComplete()
        {
            if (level == null)
                return;

            // Create and add the area complete cutscene
            string nextLevelName = string.IsNullOrEmpty(nextLevel) ? null : nextLevel;
            
            CS20_AreaComplete areaComplete = new CS20_AreaComplete(
                hasGoldenStrawberry: hasGoldenStrawberry,
                hasPinkPlatinumBerry: hasPinkPlatinumBerry,
                skipCredits: skipCredits,
                nextLevelName: nextLevelName
            );

            level.Add(areaComplete);
        }
    }
}
