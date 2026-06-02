namespace Celeste.Triggers
{
    [CustomEntity(ids: "MaggyHelper/PlayerInventoryTrigger")]
    [Tracked]
    [HotReloadable]
    public class PlayerInventoryTrigger : Trigger
    {
        public enum InventoryType
        {
            Default,
            CH6End,
            TheSummit,
            Core,
            OldSite,
            Prologue,
            Farewell,
            Custom,
            // Mod-specific presets
            KirbyPlayer,
            SayGoodbye,
            TitanTowerClimbing,
            Corruption,
            TheEnd
        }

        private enum PlayerState
        {
            NoChange,
            Enable,
            Disable
        }

        private readonly InventoryType inventoryType;
        private readonly PlayerState playerState;
        private readonly int dashes;
        private readonly bool dreamDash;
        private readonly bool backpack;
        private readonly bool noRefills;
        private readonly bool triggerOnce;
        private readonly string requiredFlag;
        private readonly KirbyMode.KirbyPowerState kirbyPower;
        private bool hasTriggered;

        public PlayerInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            if (!Enum.TryParse(data.Attr("inventoryType", "KirbyPlayer"), true, out inventoryType))
                inventoryType = InventoryType.KirbyPlayer;

            if (!Enum.TryParse(data.Attr("playerState", "NoChange"), true, out playerState))
                playerState = PlayerState.NoChange;

            dashes = Math.Clamp(data.Int("dashes", 3), 0, 10);
            dreamDash = data.Bool("dreamDash", false);
            backpack = data.Bool("backpack", true);
            noRefills = data.Bool("noRefills", false);
            triggerOnce = data.Bool("triggerOnce", true);
            requiredFlag = data.Attr("requiredFlag", string.Empty);

            if (!Enum.TryParse(data.Attr("kirbyPower", "None"), true, out kirbyPower))
                kirbyPower = KirbyMode.KirbyPowerState.None;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && hasTriggered)
                return;

            Level level = SceneAs<Level>();
            if (level == null)
                return;

            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
                return;

            ApplyPlayerState(player);
            ApplyInventory(level);
            ApplyKirbyMode(player);

            hasTriggered = true;
        }

        private void ApplyPlayerState(Player player)
        {
            switch (playerState)
            {
                case PlayerState.Enable:
                    player.StateMachine.State = Player.StNormal; // StNormal
                    player.DummyAutoAnimate = true;
                    player.Speed = Vector2.Zero;
                    break;
                case PlayerState.Disable:
                    player.StateMachine.State = Player.StDummy; // StDummy
                    player.Speed = Vector2.Zero;
                    break;
            }
        }

        private void ApplyKirbyMode(Player player)
        {
            switch (inventoryType)
            {
                case InventoryType.KirbyPlayer:
                    player.EnableKirbyMode(dashes > 0 ? dashes : 3);
                    if (kirbyPower != KirbyMode.KirbyPowerState.None)
                        player.SetKirbyPowerState(kirbyPower);
                    break;

                case InventoryType.Default:
                case InventoryType.Prologue:
                case InventoryType.OldSite:
                case InventoryType.TheEnd:
                    if (player.IsKirbyMode())
                        player.DisableKirbyMode();
                    break;
            }
        }

        private void ApplyInventory(Level level)
        {
            PlayerInventory inv = inventoryType switch
            {
                InventoryType.Default => PlayerInventory.Default,
                InventoryType.CH6End => PlayerInventory.CH6End,
                InventoryType.TheSummit => PlayerInventory.TheSummit,
                InventoryType.Core => PlayerInventory.Core,
                InventoryType.OldSite => PlayerInventory.OldSite,
                InventoryType.Prologue => PlayerInventory.Prologue,
                InventoryType.Farewell => PlayerInventory.Farewell,
                InventoryType.Custom => new PlayerInventory
                {
                    Dashes = dashes,
                    DreamDash = dreamDash,
                    Backpack = backpack,
                    NoRefills = noRefills
                },
                InventoryType.KirbyPlayer => new PlayerInventory
                {
                    Dashes = dashes > 0 ? dashes : 10,
                    DreamDash = dreamDash,
                    Backpack = true,
                    NoRefills = false
                },
                InventoryType.SayGoodbye => new PlayerInventory
                {
                    Dashes = 2,
                    DreamDash = true,
                    Backpack = true,
                    NoRefills = false
                },
                InventoryType.TitanTowerClimbing => new PlayerInventory
                {
                    Dashes = 3,
                    DreamDash = false,
                    Backpack = true,
                    NoRefills = false
                },
                InventoryType.Corruption => new PlayerInventory
                {
                    Dashes = 5,
                    DreamDash = true,
                    Backpack = false,
                    NoRefills = true
                },
                InventoryType.TheEnd => new PlayerInventory
                {
                    Dashes = 1,
                    DreamDash = true,
                    Backpack = false,
                    NoRefills = false
                },
                _ => PlayerInventory.Default
            };

            level.Session.Inventory = inv;
        }
    }
}
