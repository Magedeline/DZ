using System;
using Celeste.Entities;
using Celeste.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Trigger to enable and configure the player HP system.
    /// Place this in your map to enable HP-based gameplay for the player.
    /// </summary>
    [CustomEntity(ids: "DZ/HealthSystemTrigger")]
    public class HealthSystemTrigger : Trigger
    {
        #region Fields
        
        private int maxHP;
        private bool kirbyMode;
        private bool showUI;
        private bool persistent;
        private UniversalHealthUI.PlayerHealthDisplayMode displayMode;
        private bool trackBosses;
        private bool healOnEnter;
        private int healAmount;
        
        private bool triggered = false;
        
        #endregion
        
        #region Constructor
        
        public HealthSystemTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            maxHP = data.Int("maxHP", 6);
            kirbyMode = data.Bool("kirbyMode", false);
            showUI = data.Bool("showUI", true);
            persistent = data.Bool("persistent", true);
            displayMode = (UniversalHealthUI.PlayerHealthDisplayMode)data.Int("displayMode", 0);
            trackBosses = data.Bool("trackBosses", true);
            healOnEnter = data.Bool("healOnEnter", false);
            healAmount = data.Int("healAmount", 0);
        }
        
        #endregion
        
        #region Trigger Events
        
        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (triggered && persistent)
                return;
            
            triggered = true;
            
            var level = Scene as Level;
            if (level == null) return;
            
            // Get or create health manager
            var healthManager = PlayerHealthManager.GetOrCreate(level, maxHP);
            
            // Configure mode
            if (kirbyMode)
            {
                healthManager.EnableKirbyMode(maxHP);
            }
            else
            {
                healthManager.SetMaxHP(maxHP);
            }
            
            // Heal if configured
            if (healOnEnter)
            {
                if (healAmount > 0)
                    healthManager.Heal(healAmount);
                else
                    healthManager.FullHeal();
            }
            
            // Setup UI if enabled
            if (showUI)
            {
                var ui = UniversalHealthUI.GetOrCreate(level);
                ui.SetDisplayMode(displayMode);
                ui.ShowPlayerHealth = true;
                ui.ShowBossHealth = trackBosses;
            }
        }
        
        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            if (!persistent)
            {
                triggered = false;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Trigger to configure boss tracking for the health UI.
    /// Use this to show boss health bars when entering boss arenas.
    /// </summary>
    [CustomEntity(ids: "DZ/BossArenaTrigger")]
    public class BossArenaTrigger : Trigger
    {
        #region Fields
        
        private string bossName;
        private bool showHealthBar;
        private bool createHealthUI;
        private string bossEntityType;
        private bool startEncounter;
        private bool triggerOnce;
        private bool triggered;
        private bool pendingRemove;
        
        #endregion
        
        #region Constructor
        
        public BossArenaTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            bossName = data.Attr("bossName", "Boss");
            showHealthBar = data.Bool("showHealthBar", true);
            createHealthUI = data.Bool("createHealthUI", true);
            bossEntityType = data.Attr("bossEntityType", "");
            startEncounter = data.Bool("startEncounter", false);
            triggerOnce = data.Bool("triggerOnce", false);
        }
        
        #endregion
        
        #region Trigger Events
        
        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);

            if (triggerOnce && triggered)
                return;
            
            var level = Scene as Level;
            if (level == null) return;
            bool matchedBoss = false;
            
            // Create UI if needed
            UniversalHealthUI ui = null;
            if (showHealthBar && createHealthUI)
            {
                ui = UniversalHealthUI.GetOrCreate(level);
            }
            
            // Find bosses to start and/or track
            if (showHealthBar || startEncounter)
            {
                // Track all BossActor entities
                foreach (var entity in level.Tracker.GetEntities<BossActor>())
                {
                    if (entity is BossActor boss)
                    {
                        // Check if type filter matches
                        if (string.IsNullOrEmpty(bossEntityType) || 
                            entity.GetType().Name.Contains(bossEntityType))
                        {
                            matchedBoss = true;

                            if (startEncounter)
                                boss.StartBossFight();

                            if (showHealthBar)
                            {
                                ui?.TrackBoss(boss, bossName);
                                
                                // Also create individual health bar
                                if (ui == null)
                                {
                                    BossHealthBar.AttachToBoss(boss, bossName);
                                }
                            }
                        }
                    }
                }
            }

            if (matchedBoss)
                triggered = true;

            if (matchedBoss && triggerOnce)
                pendingRemove = true;
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemove) RemoveSelf();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Trigger to deal damage to the player.
    /// Useful for hazards that should damage but not instantly kill.
    /// </summary>
    [CustomEntity(ids: "DZ/DamageTrigger")]
    public class DamageTrigger : Trigger
    {
        #region Fields
        
        private int damage;
        private float cooldown;
        private bool removeAfterHit;
        private bool pendingRemoveDmg;
        
        private float cooldownTimer = 0f;
        
        #endregion
        
        #region Constructor
        
        public DamageTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            damage = data.Int("damage", 1);
            cooldown = data.Float("cooldown", 1f);
            removeAfterHit = data.Bool("removeAfterHit", false);
        }
        
        #endregion
        
        #region Update & Events
        
        public override void Update()
        {
            base.Update();
            if (pendingRemoveDmg) { RemoveSelf(); return; }
            
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
        }
        
        public override void OnStay(global::Celeste.Player player)
        {
            base.OnStay(player);
            
            if (cooldownTimer > 0f)
                return;
            
            // Try to damage through health manager
            bool damaged = PlayerHealthManager.TryDamagePlayer(damage, Center);
            
            if (damaged)
            {
                cooldownTimer = cooldown;
                
                if (removeAfterHit)
                {
                    pendingRemoveDmg = true;
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Trigger to heal the player.
    /// </summary>
    [CustomEntity(ids: "DZ/HealTrigger")]
    public class HealTrigger : Trigger
    {
        #region Fields
        
        private int healAmount;
        private bool fullHeal;
        private bool removeAfterUse;
        private bool onlyOnce;
        
        private bool used = false;
        private bool pendingRemoveHeal;
        
        #endregion
        
        #region Constructor
        
        public HealTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            healAmount = data.Int("healAmount", 1);
            fullHeal = data.Bool("fullHeal", false);
            removeAfterUse = data.Bool("removeAfterUse", true);
            onlyOnce = data.Bool("onlyOnce", true);
        }
        
        #endregion
        
        #region Trigger Events
        
        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (onlyOnce && used)
                return;
            
            var healthManager = PlayerHealthManager.Instance;
            if (healthManager == null)
                return;
            
            // Check if player needs healing
            if (healthManager.CurrentHP >= healthManager.MaxHP)
                return;
            
            used = true;
            
            if (fullHeal)
            {
                healthManager.FullHeal();
            }
            else
            {
                healthManager.Heal(healAmount);
            }
            
            // Visual feedback
            (Scene as Level)?.Flash(Color.Green * 0.2f, false);
            
            if (removeAfterUse)
            {
                pendingRemoveHeal = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (pendingRemoveHeal) RemoveSelf();
        }
        
        #endregion
    }
}
