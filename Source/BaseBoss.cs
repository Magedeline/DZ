using System;
using MaggyHelper.Entities;
using MaggyHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities.Bosses
{
    /// <summary>
    /// Base class for all boss entities in MaggyHelper
    /// </summary>
    [Tracked(true)]
    public abstract class BaseBoss : Actor, IKirbyCopySource
    {
        // Movement
        public Vector2 Speed;
        
        // Boss stats
        protected int maxHealth;
        protected int currentHealth;
        protected float attackCooldown;
        protected float currentCooldown;
        protected bool isInvulnerable;
        protected float invulnerabilityTimer;
        
        // State
        protected BossState currentState;
        protected float stateTimer;
        protected bool isDefeated;
        protected bool fightStarted;
        
        // Components
        protected Sprite sprite;
        protected PlayerCollider playerCollider;
        protected Hitbox hurtbox;
        
        // References
        protected Player player;
        protected Level level;
        
        // Music
        protected string bossMusic;
        protected string normalMusic;
        
        public BaseBoss(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            maxHealth = data.Int("health", 10) * MaggyHelperModule.Settings.BossDifficultyMultiplier;
            currentHealth = maxHealth;
            attackCooldown = data.Float("attackCooldown", 2f);
            bossMusic = data.Attr("bossMusic", "event:/music/lvl9/main");
            
            Initialize();
        }

        protected virtual void Initialize()
        {
            // Default hitbox
            Collider = new Hitbox(32f, 32f, -16f, -32f);
            hurtbox = new Hitbox(24f, 24f, -12f, -28f);
            
            Add(playerCollider = new PlayerCollider(p => OnPlayerCollide(p)));
            
            Depth = -10000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            if (isDefeated) return;
            
            // Get player reference
            player = Scene.Tracker.GetEntity<Player>();
            
            // Update timers
            if (currentCooldown > 0)
            {
                currentCooldown -= Engine.DeltaTime;
            }
            
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Engine.DeltaTime;
                if (invulnerabilityTimer <= 0)
                {
                    isInvulnerable = false;
                }
            }
            
            stateTimer += Engine.DeltaTime;
            
            // Start fight when player is close
            if (!fightStarted && player != null)
            {
                var distance = Vector2.Distance(Center, player.Center);
                if (distance < 200f)
                {
                    StartFight();
                }
            }
            
            // Update boss AI
            if (fightStarted)
            {
                UpdateAI();
            }
        }

        protected virtual void StartFight()
        {
            fightStarted = true;
            MaggyHelperModule.Session.BossFightActive = true;
            MaggyHelperModule.Session.CurrentBossName = GetBossName();
            
            // Change music
            if (MaggyHelperModule.Settings.EnableBossMusic && !string.IsNullOrEmpty(bossMusic))
            {
                normalMusic = Audio.CurrentMusic;
                Audio.SetMusic(bossMusic);
            }
            
            // Lock camera or room
            level?.Session.SetFlag("boss_fight_active", true);
            
            currentState = BossState.Intro;
            stateTimer = 0f;
        }

        protected abstract void UpdateAI();
        
        protected abstract string GetBossName();

        protected virtual void OnPlayerCollide(Celeste.Player player)
        {
            if (isDefeated || !fightStarted) return;
            
            // Check if player is attacking (dashing, etc.)
            if (player.StateMachine.State == Celeste.Player.StDash)
            {
                TakeDamage(1);
            }
            else if (!isInvulnerable)
            {
                // Damage the player
                player.Die(Vector2.Normalize(player.Center - Center));
            }
        }

        public virtual void TakeDamage(int amount)
        {
            if (isInvulnerable || isDefeated) return;
            
            currentHealth -= amount;
            isInvulnerable = true;
            invulnerabilityTimer = 0.5f;
            
            // Flash effect
            if (sprite != null)
                sprite.Color = Color.White;
            
            // Knockback
            Audio.Play("event:/game/general/thing_booped", Position);
            
            if (currentHealth <= 0)
            {
                Defeat();
            }
        }

        protected virtual void Defeat()
        {
            isDefeated = true;
            currentState = BossState.Defeated;
            
            // Update session and save data
            MaggyHelperModule.Session.BossFightActive = false;
            MaggyHelperModule.Session.BossesDefeated++;
            MaggyHelperModule.SaveData.RecordBossDefeat(GetBossName());
            
            // Restore music
            if (!string.IsNullOrEmpty(normalMusic))
            {
                Audio.SetMusic(normalMusic);
            }
            
            // Clear flag
            level?.Session.SetFlag("boss_fight_active", false);
            
            // Play defeat animation/effects
            PlayDefeatEffects();
            
            // Spawn rewards
            SpawnRewards();
        }

        protected virtual void PlayDefeatEffects()
        {
            Audio.Play("event:/game/06_reflection/boss_spikes_burst", Position);
            
            // Particles
            for (int i = 0; i < 30; i++)
            {
                level?.Particles.Emit(
                    Player.P_DashA,
                    Center,
                    Calc.Random.NextFloat() * MathHelper.TwoPi
                );
            }
        }

        protected virtual void SpawnRewards()
        {
            // Spawn ability star
            var ability = GetCopyAbility();
            if (ability != CopyAbilityType.None)
            {
                var star = new AbilityStar(Center, ability);
                Scene.Add(star);
            }
        }

        public abstract CopyAbilityType GetCopyAbility();

        protected void ChangeState(BossState newState)
        {
            currentState = newState;
            stateTimer = 0f;
            OnStateChanged(newState);
        }

        protected virtual void OnStateChanged(BossState newState)
        {
            // Override in subclasses for state-specific initialization
        }

        public override void Render()
        {
            base.Render();
            
            // Flash when invulnerable
            if (isInvulnerable && Scene.OnInterval(0.05f))
            {
                return; // Skip rendering to create flash effect
            }
            
            // Render health bar above boss
            if (fightStarted && !isDefeated)
            {
                RenderHealthBar();
            }
        }

        protected virtual void RenderHealthBar()
        {
            var barPosition = Position + new Vector2(-20f, -50f);
            var barWidth = 40f;
            var barHeight = 4f;
            
            // Background
            Draw.Rect(barPosition, barWidth, barHeight, Color.DarkGray);
            
            // Health
            var healthPercent = (float)currentHealth / maxHealth;
            Draw.Rect(barPosition, barWidth * healthPercent, barHeight, Color.Red);
        }
    }

    public enum BossState
    {
        Idle,
        Intro,
        Moving,
        Attacking,
        Charging,
        Vulnerable,
        Enraged,
        Defeated
    }
}
