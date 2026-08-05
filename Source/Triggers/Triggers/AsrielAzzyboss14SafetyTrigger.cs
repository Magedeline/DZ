using System;

namespace Celeste.Triggers
{
    [CustomEntity("DZ/AsrielAzzyboss14SafetyTrigger")]
    [Tracked]
    [HotReloadable]
    public class AsrielAzzyboss14SafetyTrigger : Trigger
    {
        private const string DefaultRoomName = "azzyboss-14";
        private const string DefaultFlagName = "dz_asriel_azzyboss14_special_attack_ready";

        private readonly string roomName;
        private readonly string flagName;
        private readonly bool triggerOnce;
        private bool hasTriggered;

        public AsrielAzzyboss14SafetyTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            roomName = data.Attr(nameof(roomName), DefaultRoomName);
            flagName = data.Attr(nameof(flagName), DefaultFlagName);
            triggerOnce = data.Bool(nameof(triggerOnce), true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && hasTriggered)
            {
                return;
            }

            Level level = SceneAs<Level>();
            if (level?.Session == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(roomName) && !level.Session.Level.Equals(roomName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(flagName))
            {
                return;
            }

            level.Session.SetFlag(flagName, true);
            hasTriggered = true;
        }
    }
}
