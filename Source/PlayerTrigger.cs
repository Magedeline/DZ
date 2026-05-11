using Celeste.Extensions;

namespace Celeste.Triggers
{
    [CustomEntity(ids: "MaggyHelper/PlayerTrigger")]
    [Tracked]
    [HotReloadable]
    public class PlayerTrigger : Trigger
    {
        private enum PlayerAction
        {
            None,
            EnableKirbyMode,
            DisableKirbyMode,
            SetKirbyPower,
            SetMaxDashes,
            EnablePlayer,
            DisablePlayer,
            EnableCombat,
            DisableCombat,
            SetInventory
        }

        private readonly bool triggerOnEnter;
        private readonly bool triggerOnExit;
        private readonly string onEnterFlag;
        private readonly string onExitFlag;
        private readonly bool setFlagState;
        private readonly bool triggerOnce;
        private readonly string requiredFlag;
        private readonly PlayerAction onEnterAction;
        private readonly PlayerAction onExitAction;
        private readonly KirbyMode.KirbyPowerState kirbyPower;
        private readonly int maxDashes;
        private readonly int inventoryDashes;
        private readonly bool inventoryDreamDash;
        private readonly bool inventoryNoRefills;

        private bool hasTriggered;

        public PlayerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnEnter = data.Bool(nameof(triggerOnEnter), true);
            triggerOnExit = data.Bool(nameof(triggerOnExit), true);
            onEnterFlag = data.Attr(nameof(onEnterFlag), string.Empty);
            onExitFlag = data.Attr(nameof(onExitFlag), string.Empty);
            setFlagState = data.Bool(nameof(setFlagState), true);
            triggerOnce = data.Bool(nameof(triggerOnce), false);
            requiredFlag = data.Attr(nameof(requiredFlag), string.Empty);
            onEnterAction = ParseAction(data.Attr(nameof(onEnterAction), PlayerAction.None.ToString()));
            onExitAction = ParseAction(data.Attr(nameof(onExitAction), PlayerAction.None.ToString()));

            if (!Enum.TryParse(data.Attr(nameof(kirbyPower), KirbyMode.KirbyPowerState.None.ToString()), true, out kirbyPower))
            {
                kirbyPower = KirbyMode.KirbyPowerState.None;
            }

            maxDashes = Math.Clamp(data.Int(nameof(maxDashes), 3), 1, 10);
            inventoryDashes = Math.Clamp(data.Int(nameof(inventoryDashes), 1), 0, 10);
            inventoryDreamDash = data.Bool(nameof(inventoryDreamDash), false);
            inventoryNoRefills = data.Bool(nameof(inventoryNoRefills), false);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerOnEnter)
            {
                Activate(player, isEnterEvent: true);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);

            if (triggerOnExit)
            {
                Activate(player, isEnterEvent: false);
            }
        }

        private void Activate(global::Celeste.Player player, bool isEnterEvent)
        {
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            Level level = SceneAs<Level>();
            if (level == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                return;
            }

            string targetFlag = isEnterEvent ? onEnterFlag : onExitFlag;
            if (!string.IsNullOrEmpty(targetFlag))
            {
                level.Session.SetFlag(targetFlag, setFlagState);
            }

            ApplyPlayerAction(player, isEnterEvent ? onEnterAction : onExitAction);

            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }

        private void ApplyPlayerAction(global::Celeste.Player player, PlayerAction action)
        {
            if (player == null)
            {
                return;
            }

            switch (action)
            {
                case PlayerAction.EnableKirbyMode:
                    player.EnableKirbyMode(maxDashes);
                    break;
                case PlayerAction.DisableKirbyMode:
                    player.DisableKirbyMode();
                    break;
                case PlayerAction.SetKirbyPower:
                    player.SetKirbyPowerState(kirbyPower);
                    break;
                case PlayerAction.SetMaxDashes:
                    player.SetMaxDashes(maxDashes);
                    break;
                case PlayerAction.EnablePlayer:
                    player.StateMachine.State = Player.StNormal;
                    player.DummyAutoAnimate = true;
                    player.Speed = Vector2.Zero;
                    break;
                case PlayerAction.DisablePlayer:
                    player.StateMachine.State = Player.StDummy;
                    player.DummyAutoAnimate = false;
                    player.Speed = Vector2.Zero;
                    break;
                case PlayerAction.EnableCombat:
                    player.EnableCombat();
                    break;
                case PlayerAction.DisableCombat:
                    player.DisableCombat();
                    break;
                case PlayerAction.SetInventory:
                    ApplyInventory(player);
                    break;
                case PlayerAction.None:
                default:
                    break;
            }
        }

        private void ApplyInventory(global::Celeste.Player player)
        {
            Level level = SceneAs<Level>();
            if (level?.Session == null)
                return;

            var inv = level.Session.Inventory;
            inv.Dashes = inventoryDashes;
            inv.DreamDash = inventoryDreamDash;
            inv.NoRefills = inventoryNoRefills;
            level.Session.Inventory = inv;

            player.Dashes = inventoryDashes;
        }

        private static PlayerAction ParseAction(string action)
        {
            if (Enum.TryParse(action, true, out PlayerAction parsed))
            {
                return parsed;
            }

            return PlayerAction.None;
        }
    }
}
