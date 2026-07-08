using System;
using Celeste.Entities;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Meta Knight boss - A fast, aggressive swordsman boss
    /// </summary>
    [CustomEntity("DZ/MetaKnightBoss")]
    [Tracked]
    public class MetaKnightBoss : BaseBoss
    {
        // Movement
        private float moveSpeed = 150f;
        private bool isFlying;
        private float flyHeight;
        
        // Combat
        private MetaKnightAttack currentAttack;
        private int comboCount;
        private const int MAX_COMBO = 5;
        private Vector2 dashTarget;
        private bool isDashing;
        
        // Dimensional cape
        private bool isTeleporting;
        private float teleportTimer;
        
        // Sword beam
        private float swordBeamCooldown;
        
        // Honor system - gives player sword at start
        private bool gaveSword;
        
        public MetaKnightBoss(EntityData data, Vector2 offset) : base(data, offset)
        {
            maxHealth = data.Int("health", 20) * DZModule.Settings.BossDifficultyMultiplier;
            currentHealth = maxHealth;
            attackCooldown = data.Float("attackCooldown", 0.8f);
            
            // Create sprite
            Add(sprite = GFX.SpriteBank.Create("DZ_MetaKnightBoss"));
            sprite.Play("idle");
            
            // Hitbox
            Collider = new Hitbox(24f, 28f, -12f, -28f);
        }

        protected override string GetBossName() => "Meta Knight";

        protected override void StartFight()
        {
            base.StartFight();
            
            // Meta Knight's honor - offer sword to player
            if (!gaveSword && DZModule.Settings.KirbyPlayerEnabled)
            {
                var session = DZModule.Session;
                if (session != null && session.CurrentCopyAbility == CopyAbilityType.None)
                {
                    // Spawn a sword ability star for the player
                    var sword = new AbilityStar(player.Position + new Vector2(0, -20), CopyAbilityType.Sword);
                    Scene.Add(sword);
                    gaveSword = true;
                }
            }
        }

        protected override void UpdateAI()
        {
            if (player == null) return;
            
            // Update flying
            if (isFlying)
            {
                flyHeight += Engine.DeltaTime * 5f;
                // Hover effect
            }
            
            switch (currentState)
            {
                case BossState.Intro:
                    UpdateIntro();
                    break;
                case BossState.Moving:
                    UpdateMoving();
                    break;
                case BossState.Attacking:
                    UpdateAttacking();
                    break;
                case BossState.Charging:
                    UpdateCharging();
                    break;
                case BossState.Vulnerable:
                    UpdateVulnerable();
                    break;
            }
            
            // Teleport handling
            if (isTeleporting)
            {
                UpdateTeleport();
            }
        }

        private void UpdateIntro()
        {
            // Meta Knight dramatically reveals himself
            if (stateTimer < 1f)
            {
                sprite?.Play("cape_wrap");
            }
            else if (stateTimer < 2f)
            {
                sprite?.Play("draw_sword");
            }
            else
            {
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateMoving()
        {
            var distance = Vector2.Distance(Center, player.Center);
            var direction = player.Center - Center;
            if (direction != Vector2.Zero) direction.Normalize();
            
            // Face player
            if (sprite != null)
            {
                sprite.FlipX = player.X < X;
            }
            
            // Fly around player
            isFlying = true;
            
            // Orbit at medium range
            var orbitRadius = 80f;
            var targetPos = player.Center + new Vector2(
                (float)Math.Cos(stateTimer * 2f) * orbitRadius,
                (float)Math.Sin(stateTimer * 2f) * orbitRadius * 0.5f - 30f
            );
            
            var toTarget = targetPos - Position;
            if (toTarget.Length() > 5f)
            {
                toTarget.Normalize();
                Position += toTarget * moveSpeed * Engine.DeltaTime;
            }
            
            sprite?.Play("fly");
            
            // Choose attack
            if (currentCooldown <= 0)
            {
                ChooseAttack(distance);
            }
        }

        private void ChooseAttack(float distance)
        {
            var random = Calc.Random.NextFloat();
            
            if (distance < 50f)
            {
                // Close range - sword combo
                currentAttack = MetaKnightAttack.SwordCombo;
                comboCount = 0;
                ChangeState(BossState.Attacking);
            }
            else if (distance < 100f)
            {
                // Medium range - dash attack or uppercut
                currentAttack = random < 0.6f ? MetaKnightAttack.DashSlash : MetaKnightAttack.Uppercut;
                ChangeState(BossState.Charging);
            }
            else
            {
                // Far range - tornado, teleport, or sword beam
                if (random < 0.33f)
                {
                    currentAttack = MetaKnightAttack.Tornado;
                    ChangeState(BossState.Charging);
                }
                else if (random < 0.66f)
                {
                    currentAttack = MetaKnightAttack.DimensionalCape;
                    StartTeleport();
                }
                else
                {
                    currentAttack = MetaKnightAttack.SwordBeam;
                    ChangeState(BossState.Attacking);
                }
            }
        }

        private void UpdateAttacking()
        {
            switch (currentAttack)
            {
                case MetaKnightAttack.SwordCombo:
                    UpdateSwordCombo();
                    break;
                case MetaKnightAttack.DashSlash:
                    UpdateDashSlash();
                    break;
                case MetaKnightAttack.Uppercut:
                    UpdateUppercut();
                    break;
                case MetaKnightAttack.Tornado:
                    UpdateTornado();
                    break;
                case MetaKnightAttack.SwordBeam:
                    UpdateSwordBeam();
                    break;
            }
        }

        private void UpdateSwordCombo()
        {
            sprite?.Play("sword_slash");
            
            // Quick successive slashes
            if (Scene.OnInterval(0.15f))
            {
                comboCount++;
                
                // Hit detection
                var slashBox = new Rectangle(
                    (int)(X + (sprite.FlipX ? -40 : 10)),
                    (int)(Y - 30),
                    30,
                    30
                );
                
                if (player.Collider.Collide(slashBox))
                {
                    player.Die(Vector2.UnitX * (sprite.FlipX ? -1 : 1));
                }
                
                // Spawn slash effect
                SpawnSlashEffect();
            }
            
            if (comboCount >= MAX_COMBO)
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Vulnerable);
            }
        }

        private void UpdateDashSlash()
        {
            if (isDashing)
            {
                var direction = dashTarget - Position;
                if (direction.Length() > 10f)
                {
                    direction.Normalize();
                    Position += direction * 400f * Engine.DeltaTime;
                    
                    // Trail effect
                    if (Scene.OnInterval(0.02f))
                    {
                        // Spawn afterimage
                    }
                    
                    // Hit detection along path
                    if (Vector2.Distance(Center, player.Center) < 20f)
                    {
                        player.Die(direction);
                    }
                }
                else
                {
                    isDashing = false;
                    currentCooldown = attackCooldown;
                    ChangeState(BossState.Moving);
                }
            }
        }

        private void UpdateUppercut()
        {
            sprite?.Play("uppercut");
            
            if (stateTimer < 0.3f)
            {
                // Rise up
                Position.Y -= 300f * Engine.DeltaTime;
                
                // Hit detection
                if (Vector2.Distance(Center, player.Center) < 25f)
                {
                    player.Die(Vector2.UnitY * -1);
                }
            }
            else
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateTornado()
        {
            sprite?.Play("tornado");
            
            // Spin rapidly and move toward player
            var direction = player.Center - Center;
            if (direction != Vector2.Zero) direction.Normalize();
            
            Position += direction * 200f * Engine.DeltaTime;
            
            // Damage area
            if (Vector2.Distance(Center, player.Center) < 35f)
            {
                player.Die(direction);
            }
            
            // Spawn wind particles
            if (Scene.OnInterval(0.05f))
            {
                level?.Particles.Emit(
                    ParticleTypes.Dust,
                    Center,
                    Calc.Random.NextFloat() * MathHelper.TwoPi
                );
            }
            
            if (stateTimer > 2f)
            {
                currentCooldown = attackCooldown * 1.5f;
                ChangeState(BossState.Vulnerable);
            }
        }

        private void UpdateSwordBeam()
        {
            sprite?.Play("sword_beam");
            
            if (stateTimer > 0.3f && swordBeamCooldown <= 0)
            {
                // Fire sword beam projectile
                var direction = player.Center - Center;
                direction.Normalize();
                
                var beam = new Projectiles.SwordBeam(Center, direction * 300f);
                Scene.Add(beam);
                
                swordBeamCooldown = 0.5f;
                Audio.Play("event:/game/general/thing_booped", Position);
            }
            
            swordBeamCooldown -= Engine.DeltaTime;
            
            if (stateTimer > 1f)
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateCharging()
        {
            switch (currentAttack)
            {
                case MetaKnightAttack.DashSlash:
                    sprite?.Play("dashDZ_CHarge");
                    if (stateTimer > 0.3f)
                    {
                        dashTarget = player.Center;
                        isDashing = true;
                        ChangeState(BossState.Attacking);
                    }
                    break;
                    
                case MetaKnightAttack.Uppercut:
                    sprite?.Play("crouch");
                    if (stateTimer > 0.2f)
                    {
                        ChangeState(BossState.Attacking);
                    }
                    break;
                    
                case MetaKnightAttack.Tornado:
                    sprite?.Play("tornado_start");
                    if (stateTimer > 0.5f)
                    {
                        ChangeState(BossState.Attacking);
                    }
                    break;
            }
        }

        private void StartTeleport()
        {
            isTeleporting = true;
            teleportTimer = 0f;
            sprite?.Play("cape_vanish");
            Collidable = false;
        }

        private void UpdateTeleport()
        {
            teleportTimer += Engine.DeltaTime;
            
            if (teleportTimer > 0.5f && teleportTimer < 0.6f)
            {
                // Teleport behind player
                var behindPlayer = player.Center + new Vector2(
                    player.Facing == Facings.Left ? 50 : -50,
                    0
                );
                Position = behindPlayer;
            }
            
            if (teleportTimer > 1f)
            {
                isTeleporting = false;
                Collidable = true;
                sprite?.Play("cape_appear");
                
                // Immediate attack after teleport
                currentAttack = MetaKnightAttack.SwordCombo;
                comboCount = 0;
                ChangeState(BossState.Attacking);
            }
        }

        private void UpdateVulnerable()
        {
            sprite?.Play("idle");
            isFlying = false;
            
            if (stateTimer > 1f)
            {
                isFlying = true;
                ChangeState(BossState.Moving);
            }
        }

        private void SpawnSlashEffect()
        {
            // Visual slash effect
            var facing = sprite.FlipX ? -1 : 1;
            // Add particle or animation
        }

        protected override void Defeat()
        {
            base.Defeat();
            
            // Meta Knight gracefully accepts defeat
            sprite?.Play("kneel");
            isFlying = false;
            
            // He flies away after a moment
            Add(new Coroutine(DefeatSequence()));
        }

        private System.Collections.IEnumerator DefeatSequence()
        {
            yield return 2f;
            
            sprite?.Play("fly_away");
            
            float timer = 0f;
            while (timer < 2f)
            {
                Position.Y -= 100f * Engine.DeltaTime;
                Position.X += 50f * Engine.DeltaTime;
                timer += Engine.DeltaTime;
                yield return null;
            }
            
            RemoveSelf();
        }

        public override CopyAbilityType GetCopyAbility()
        {
            return CopyAbilityType.Sword;
        }
    }

    public enum MetaKnightAttack
    {
        SwordCombo,
        DashSlash,
        Uppercut,
        Tornado,
        SwordBeam,
        DimensionalCape
    }
}

