using MaggyHelper.Cutscenes;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that starts the CS09_FakeSavePoint cutscene sequence.
    /// The cutscene will automatically determine which stage to play based on session flags.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CS09_FakeSavePointTrigger")]
    public class CS09_FakeSavePointTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool playerOnly;
        private readonly string specificStage;
        private bool hasTriggered;
        private Level level;

        public CS09_FakeSavePointTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            playerOnly = data.Bool("playerOnly", true);
            specificStage = data.Attr("specificStage", "auto");
            hasTriggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            // Check if we should already be disabled based on flags
            if (level != null && level.Session.GetFlag(CS09_FakeSavePoint.FlagTrapTriggered))
            {
                hasTriggered = true;
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (!triggerOnce || !hasTriggered)
            {
                StartCutscene(player);
            }
        }

        private void StartCutscene(global::Celeste.Player player)
        {
            if (level == null || player == null)
                return;

            hasTriggered = true;

            // Determine which stage to play
            CS09_FakeSavePoint.SavePointStage stage;
            
            if (specificStage == "auto")
            {
                stage = CS09_FakeSavePoint.GetCurrentStage(level);
            }
            else
            {
                // Parse the specific stage from the attribute
                stage = specificStage.ToLowerInvariant() switch
                {
                    "stagea" or "a" => CS09_FakeSavePoint.SavePointStage.StageA,
                    "stageb" or "b" => CS09_FakeSavePoint.SavePointStage.StageB,
                    "stagec" or "c" => CS09_FakeSavePoint.SavePointStage.StageC,
                    "staged" or "d" => CS09_FakeSavePoint.SavePointStage.StageD,
                    "stagee" or "e" => CS09_FakeSavePoint.SavePointStage.StageE,
                    "pretrap" => CS09_FakeSavePoint.SavePointStage.PreTrap,
                    "trap" => CS09_FakeSavePoint.SavePointStage.Trap,
                    "madelinefreakout" or "freakout" => CS09_FakeSavePoint.SavePointStage.MadelineFreakout,
                    _ => CS09_FakeSavePoint.GetCurrentStage(level)
                };
            }

            // Create and add the cutscene
            var cutscene = new CS09_FakeSavePoint(player, stage);
            level.Add(cutscene);
        }
    }
}
