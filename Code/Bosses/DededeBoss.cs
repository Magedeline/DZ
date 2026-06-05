using System;
using Celeste.Entities;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// King Dedede boss - A heavy-hitting melee boss with hammer attacks
    /// </summary>
    [CustomEntity("MaggyHelper/DededeBoss")]
    [Tracked]
    public class DededeBoss : BaseBoss
    {
        // Movement
        private float moveSpeed = 60f;
        private float jumpStrength = -300f;
        private bool isGrounded;
        
        // Attack types
        private DededeAttack currentAttack;
        
        // Jump slam attack
        private bool isJumpSlamming;
        private Vector2 jumpTarget;
        
        // Super dedede jump
        private int superJumpCount;
        private const int MAX_SUPER_JUMPS = 3;
        
        public DededeBoss(EntityData data, Vector2 offset) : base(data, offset)
        {
            maxHealth = data.Int("health", 25) * MaggyHelperModule.Settings.BossDifficultyMultiplier;
            currentHealth = maxHealth;
            attackCooldown = data.Float("attackCooldown", 1.5f);
            
            // Create sprite
            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_DededeBoss"));
            sprite.Play("idle");
            
            // Larger hitbox for Dedede
            Collider = new Hitbox(40f, 48f, -20f, -48f);
            
            // Gravity
            Add(new Coroutine(GravityRoutine()));
        }

        protected override string GetBossName() => "King Dedede";

        private System.Collections.IEnumerator GravityRoutine()
        {
            while (true)
            {
                if (!isGrounded && !isJumpSlamming)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 200f, 400f * Engine.DeltaTime);
                }
                yield return null;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Check ground
            isGrounded = CollideCheck<Solid>(Position + Vector2.UnitY);
            
            // Apply movement
            MoveH(Speed.X * Engine.DeltaTime);
            MoveV(Speed.Y * Engine.DeltaTime);
        }

        protected override void UpdateAI()
        {
            if (player == null) return;
            
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
                case BossState.Enraged:
                    UpdateEnraged();
                    break;
            }
        }

        private void UpdateIntro()
        {
            // Dedede lands with a big slam
            if (stateTimer < 0.5f)
            {
                sprite?.Play("fall");
            }
            else if (stateTimer < 1f)
            {
                if (isGrounded)
                {
                    sprite?.Play("land");
                    // Screen shake
                    level?.Shake(0.3f);
                }
            }
            else
            {
                sprite?.Play("idle");
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateMoving()
        {
            if (!isGrounded) return;
            
            // Walk toward player
            var direction = Math.Sign(player.X - X);
            Speed.X = direction * moveSpeed;
            
            sprite?.Play(Math.Abs(Speed.X) > 0 ? "walk" : "idle");
            
            // Face player
            if (sprite != null)
            {
                sprite.FlipX = direction < 0;
            }
            
            // Choose attack when in range
            if (currentCooldown <= 0)
            {
                var distance = Vector2.Distance(Center, player.Center);
                
                if (distance < 60f)
                {
                    // Close range - hammer swing
                    currentAttack = DededeAttack.HammerSwing;
                    ChangeState(BossState.Attacking);
                }
                else if (distance < 150f)
                {
                    // Medium range - jump slam or charge
                    currentAttack = Calc.Random.Chance(0.5f) ? DededeAttack.JumpSlam : DededeAttack.HammerCharge;
                    ChangeState(BossState.Charging);
                }
                else
                {
                    // Far range - super dedede jump
                    currentAttack = DededeAttack.SuperJump;
                    ChangeState(BossState.Charging);
                }
            }
            
            // Check for enrage at low health
            if (currentHealth <= maxHealth / 3 && currentState != BossState.Enraged)
            {
                ChangeState(BossState.Enraged);
            }
        }

        private void UpdateAttacking()
        {
            switch (currentAttack)
            {
                case DededeAttack.HammerSwing:
                    UpdateHammerSwing();
                    break;
                case DededeAttack.JumpSlam:
                    UpdateJumpSlam();
                    break;
                case DededeAttack.HammerCharge:
                    UpdateHammerCharge();
                    break;
                case DededeAttack.SuperJump:
                    UpdateSuperJump();
                    break;
                case DededeAttack.Inhale:
                    UpdateInhale();
                    break;
            }
        }

        private void UpdateHammerSwing()
        {
            sprite?.Play("hammer_swing");
            Speed.X = 0;
            
            // Damage frame
            if (stateTimer > 0.3f && stateTimer < 0.6f)
            {
                // Check for player hit
                var attackBox = new Rectangle(
                    (int)(X + (sprite.FlipX ? -50 : 10)),
                    (int)(Y - 40),
                    40,
                    40
                );
                
                if (player.Collider.Collide(attackBox))
                {
                    player.Die(Vector2.UnitX * (sprite.FlipX ? -1 : 1));
                }
            }
            
            if (stateTimer > 0.8f)
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateJumpSlam()
        {
            if (isJumpSlamming)
            {
                // Coming down
                Speed.Y = 400f;
                
                if (isGrounded)
                {
                    isJumpSlamming = false;
                    sprite?.Play("slam_land");
                    level?.Shake(0.5f);
                    Audio.Play("event:/game/general/fallblock_shake", Position);
                    
                    // Shockwave
                    SpawnShockwave();
                    
                    currentCooldown = attackCooldown;
                    ChangeState(BossState.Vulnerable);
                }
            }
        }

        private void UpdateHammerCharge()
        {
            sprite?.Play("hammer_charge");
            Speed.X = 0;
            
            if (stateTimer > 1f)
            {
                // Release charge
                var direction = Math.Sign(player.X - X);
                Speed.X = direction * 300f;
                ChangeState(BossState.Attacking);
                currentAttack = DededeAttack.HammerSwing;
            }
        }

        private void UpdateSuperJump()
        {
            if (!isJumpSlamming && isGrounded)
            {
                superJumpCount++;
                isJumpSlamming = true;
                Speed.Y = jumpStrength * 1.5f;
                jumpTarget = player.Center;
                sprite?.Play("super_jump");
            }
            else if (isJumpSlamming)
            {
                // Track player X while in air
                var direction = Math.Sign(jumpTarget.X - X);
                Speed.X = direction * 100f;
                
                // Start falling at peak
                if (Speed.Y > 0)
                {
                    isJumpSlamming = false;
                    currentAttack = DededeAttack.JumpSlam;
                    isJumpSlamming = true;
                }
            }
            
            // Continue super jumps
            if (isGrounded && superJumpCount < MAX_SUPER_JUMPS)
            {
                ChangeState(BossState.Charging);
                currentAttack = DededeAttack.SuperJump;
            }
            else if (isGrounded)
            {
                superJumpCount = 0;
                currentCooldown = attackCooldown * 2;
                ChangeState(BossState.Vulnerable);
            }
        }

        private void UpdateInhale()
        {
            sprite?.Play("inhale");
            Speed.X = 0;
            
            // Pull player
            if (player != null)
            {
                var direction = Center - player.Center;
                direction.Normalize();
                player.Speed += direction * 50f * Engine.DeltaTime;
            }
            
            if (stateTimer > 2f)
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateCharging()
        {
            switch (currentAttack)
            {
                case DededeAttack.JumpSlam:
                    if (stateTimer > 0.5f && isGrounded)
                    {
                        isJumpSlamming = true;
                        Speed.Y = jumpStrength;
                        jumpTarget = player.Center;
                        sprite?.Play("jump");
                        ChangeState(BossState.Attacking);
                    }
                    break;
                    
                case DededeAttack.SuperJump:
                    if (stateTimer > 0.3f)
                    {
                        ChangeState(BossState.Attacking);
                    }
                    break;
                    
                default:
                    if (stateTimer > 0.5f)
                    {
                        ChangeState(BossState.Attacking);
                    }
                    break;
            }
        }

        private void UpdateVulnerable()
        {
            sprite?.Play("tired");
            Speed.X = 0;
            
            if (stateTimer > 1.5f)
            {
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateEnraged()
        {
            if (stateTimer < 1f)
            {
                // Rage animation
                sprite?.Play("enrage");
                Speed.X = 0;
                
                if (Scene.OnInterval(0.1f))
                {
                    level?.Shake(0.1f);
                }
            }
            else
            {
                // Increase speed and aggression
                moveSpeed = 100f;
                attackCooldown = 1f;
                ChangeState(BossState.Moving);
            }
        }

        private void SpawnShockwave()
        {
            // Spawn ground shockwaves left and right
            var leftWave = new Projectiles.Shockwave(Position, -1);
            var rightWave = new Projectiles.Shockwave(Position, 1);
            Scene.Add(leftWave);
            Scene.Add(rightWave);
        }

        protected override void Defeat()
        {
            base.Defeat();
            
            sprite?.Play("defeat");
            Speed = Vector2.Zero;
        }

        public override CopyAbilityType GetCopyAbility()
        {
            return CopyAbilityType.Hammer;
        }
    }

    public enum DededeAttack
    {
        HammerSwing,
        JumpSlam,
        HammerCharge,
        SuperJump,
        Inhale
    }
}

