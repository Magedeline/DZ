using MaggyHelper.Cutscenes;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger for the Chapter 9 ending sequence.
    /// Chains: CS09_FakeSavePoint (trap) -> CS09_Credits -> CS09_MessageEnd -> CS09_AreaComplete
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CS09_EndingTrigger")]
    public class CS09_EndingTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool requireTrapComplete;
        private readonly bool skipCredits;
        private readonly bool skipMessage;
        private readonly string nextLevel;
        private bool hasTriggered;
        private Level level;

        public CS09_EndingTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            requireTrapComplete = data.Bool("requireTrapComplete", true);
            skipCredits = data.Bool("skipCredits", false);
            skipMessage = data.Bool("skipMessage", false);
            nextLevel = data.Attr("nextLevel", "");
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
                // Check if trap must be complete first
                if (requireTrapComplete && level != null)
                {
                    if (!level.Session.GetFlag(CS09_FakeSavePoint.FlagTrapTriggered))
                    {
                        // Trap not triggered yet, don't start ending
                        return;
                    }
                }
                
                StartEndingSequence(player);
            }
        }

        private void StartEndingSequence(global::Celeste.Player player)
        {
            if (level == null || player == null)
                return;

            hasTriggered = true;

            // Create the area complete cutscene which handles the full sequence
            var endingCutscene = new CS09_AreaComplete(
                player: player,
                skipCredits: skipCredits,
                skipMessage: skipMessage,
                nextLevelName: string.IsNullOrEmpty(nextLevel) ? null : nextLevel
            );

            level.Add(endingCutscene);
        }
    }
}
