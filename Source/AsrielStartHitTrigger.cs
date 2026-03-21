using MaggyHelper.Entities;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that starts the Asriel God Boss attack sequence and optionally moves Asriel to a target position.
    /// When the player enters this trigger, it finds the AsrielGodBoss entity and triggers the startHit behavior.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/AsrielStartHitTrigger")]
    public class AsrielStartHitTrigger : Trigger
    {
        private bool triggerOnce;
        private bool hasTriggered;
        private bool moveAsriel;
        private float moveSpeed;
        private Level level;
        private Vector2[] nodes;

        public AsrielStartHitTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            moveAsriel = data.Bool(nameof(moveAsriel), false);
            moveSpeed = data.Float(nameof(moveSpeed), 300f);
            hasTriggered = false;
            
            // Get nodes for movement target
            nodes = data.NodesOffset(offset);
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

            hasTriggered = true;
            TriggerAsrielBoss();
        }

        private void TriggerAsrielBoss()
        {
            if (level == null)
                return;

            // Find the AsrielGodBoss entity in the scene
            var asrielBoss = level.Tracker.GetEntity<AsrielGodBoss>();
            
            if (asrielBoss == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "AsrielStartHitTrigger: No AsrielGodBoss found in the scene.");
                return;
            }

            // Trigger the startHit behavior - call the public method
            asrielBoss.TriggerStartHit();

            // Optionally move Asriel to a target position
            if (moveAsriel && nodes != null && nodes.Length > 0)
            {
                asrielBoss.MoveToTarget(nodes[0], moveSpeed);
            }
        }
    }
}

