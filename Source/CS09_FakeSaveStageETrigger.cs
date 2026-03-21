using MaggyHelper.Cutscenes;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger for CS09_FakeSavePoint Stage E.
    /// Final stage connected to NPC09_SavePoint interaction, triggers the trap.
    /// Sets flag: ch9_fakesave_stage_e
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CS09_FakeSaveStageETrigger")]
    public class CS09_FakeSaveStageETrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool playerOnly;
        private bool hasTriggered;
        private Level level;

        public CS09_FakeSaveStageETrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            playerOnly = data.Bool("playerOnly", true);
            hasTriggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            // Disable if stage E flag is already set
            if (level != null && level.Session.GetFlag(CS09_FakeSavePoint.FlagStageE))
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

            var cutscene = new CS09_FakeSavePoint(player, CS09_FakeSavePoint.SavePointStage.StageE);
            level.Add(cutscene);
        }
    }
}
