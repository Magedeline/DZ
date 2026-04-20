using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger for CS09_FakeSavePoint Stage D.
    /// Ralsei asks if Chara is okay, Kirby decides to act.
    /// Sets flag: ch9_fakesave_stage_d
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CS09_FakeSaveStageDTrigger")]
    public class CS09_FakeSaveStageDTrigger : Trigger
    {
        private readonly bool triggerOnce;
        private readonly bool playerOnly;
        private bool hasTriggered;
        private Level level;

        public CS09_FakeSaveStageDTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool("triggerOnce", true);
            playerOnly = data.Bool("playerOnly", true);
            hasTriggered = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            // Disable if stage D flag is already set
            if (level != null && level.Session.GetFlag(CS09_FakeSavePoint.FlagStageD))
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

            var cutscene = new CS09_FakeSavePoint(player, CS09_FakeSavePoint.SavePointStage.StageD);
            level.Add(cutscene);
        }
    }
}
