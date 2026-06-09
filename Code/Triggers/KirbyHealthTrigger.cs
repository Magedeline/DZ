using System;
using Celeste.Entities;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger to enable or configure the Kirby health system.
    /// Can be used to set health values, enable/disable the system,
    /// or set custom respawn points.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KirbyHealthTrigger")]
    public class KirbyHealthTrigger : Trigger
    {
        #region Fields

        private bool enableHealth;
        private int maxHealth;
        private int healAmount;
        private bool fullHeal;
        private bool setRespawnPoint;
        private bool onlyOnce;
        private bool triggered;

        #endregion

        #region Constructor

        public KirbyHealthTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableHealth = data.Bool("enableHealth", true);
            maxHealth = data.Int("maxHealth", 6);
            healAmount = data.Int("healAmount", 0);
            fullHeal = data.Bool("fullHeal", false);
            setRespawnPoint = data.Bool("setRespawnPoint", false);
            onlyOnce = data.Bool("onlyOnce", true);
        }

        #endregion

        #region Lifecycle

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (onlyOnce && triggered)
                return;

            triggered = true;

            // Get the health manager
            var level = Scene as Level;
            var healthManager = PlayerHealthManager.Instance;

            // Enable/disable health system
            if (enableHealth)
            {
                healthManager?.EnableKirbyMode(maxHealth);

                // Heal if requested
                if (fullHeal)
                {
                    healthManager?.FullHeal();
                }
                else if (healAmount > 0)
                {
                    healthManager?.Heal(healAmount);
                }

                Logger.Log(LogLevel.Info, "KirbyHealthTrigger", $"Health system enabled with max HP: {maxHealth}");
            }
            else
            {
                healthManager?.DisableKirbyMode();
                Logger.Log(LogLevel.Info, "KirbyHealthTrigger", "Health system disabled");
            }

            // Set respawn point if requested
            if (setRespawnPoint)
            {
                level.Session.RespawnPoint = player.Position;
                Logger.Log(LogLevel.Info, "KirbyHealthTrigger", $"Respawn point set to: {player.Position} in room {level.Session.Level}");
            }
        }

        #endregion
    }

    /// <summary>
    /// A room-based controller that automatically enables Kirby health system
    /// when the player enters and is in Kirby mode.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KirbyHealthRoomController")]
    [Tracked]
    public class KirbyHealthRoomController : Entity
    {
        #region Fields

        private int maxHealth;
        private bool autoHealOnEnter;
        private bool setAsRespawnRoom;
        private bool hasActivated;

        #endregion

        #region Constructor

        public KirbyHealthRoomController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            maxHealth = data.Int("maxHealth", 6);
            autoHealOnEnter = data.Bool("autoHealOnEnter", false);
            setAsRespawnRoom = data.Bool("setAsRespawnRoom", false);
            Tag = Tags.TransitionUpdate;
        }

        #endregion

        #region Lifecycle

        public override void Added(Scene scene)
        {
            base.Added(scene);

            var level = Scene as Level;
            if (level == null)
                return;

            // Check if player is in Kirby mode
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && player.IsKirbyMode())
            {
                ActivateHealthSystem(player, level);
            }
        }

        public override void Update()
        {
            base.Update();

            if (hasActivated)
                return;

            var level = Scene as Level;
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();

            if (player != null && player.IsKirbyMode())
            {
                ActivateHealthSystem(player, level);
            }
        }

        private void ActivateHealthSystem(global::Celeste.Player player, Level level)
        {
            hasActivated = true;

            // Get the health manager
            var healthManager = PlayerHealthManager.Instance;

            // Enable health system
            healthManager?.EnableKirbyMode(maxHealth);

            // Auto heal if requested
            if (autoHealOnEnter)
            {
                healthManager?.FullHeal();
            }

            // Set as respawn room if requested
            if (setAsRespawnRoom)
            {
                level.Session.RespawnPoint = player.Position;
            }

            Logger.Log(LogLevel.Info, "KirbyHealthRoomController", $"Health system activated in room: {level.Session.Level}");
        }

        #endregion
    }
}
