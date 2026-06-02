using System;
using Celeste.Entities;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// A shadow/dark Kirby boss that mirrors the player's abilities
    /// </summary>
    [CustomEntity("MaggyHelper/KirbyBoss")]
    [Tracked]
    public class KirbyBoss : BaseBoss
    {
        private float floatHeight;
        private Vector2 targetPosition;
        private CopyAbilityType mirroredAbility;
        private float attackTimer;
        
        // Attack patterns
        private int attackPattern;
        
        public KirbyBoss(EntityData data, Vector2 offset) : base(data, offset)
        {
            maxHealth = data.Int("health", 15) * MaggyHelperModule.Settings.BossDifficultyMultiplier;
            currentHealth = maxHealth;
            
            // Create sprite
            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_KirbyBoss"));
            sprite.Play("idle");
            
            Collider = new Hitbox(24f, 24f, -12f, -24f);
        }

        protected override string GetBossName() => "Shadow Kirby";

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
            }
        }

        private void UpdateIntro()
        {
            // Float down from above
            if (stateTimer < 2f)
            {
                sprite?.Play("float");
            }
            else
            {
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateMoving()
        {
            // Move toward player but maintain distance
            var direction = player.Center - Center;
            var distance = direction.Length();
            
            if (distance > 0)
            {
                direction.Normalize();
            }
            
            // Keep at medium range
            if (distance > 100f)
            {
                Position += direction * 80f * Engine.DeltaTime;
            }
            else if (distance < 60f)
            {
                Position -= direction * 60f * Engine.DeltaTime;
            }
            
            // Float up and down
            floatHeight += Engine.DeltaTime * 3f;
            Position.Y += (float)Math.Sin(floatHeight) * 0.5f;
            
            // Decide to attack
            if (currentCooldown <= 0)
            {
                ChooseAttack();
            }
        }

        private void ChooseAttack()
        {
            attackPattern = Calc.Random.Next(4);
            
            switch (attackPattern)
            {
                case 0:
                    // Inhale attack
                    ChangeState(BossState.Charging);
                    sprite?.Play("inhale");
                    break;
                case 1:
                    // Star spit attack
                    ChangeState(BossState.Attacking);
                    sprite?.Play("attack");
                    break;
                case 2:
                    // Float and dive attack
                    ChangeState(BossState.Charging);
                    sprite?.Play("float");
                    break;
                case 3:
                    // Mirror player's ability
                    MirrorPlayerAbility();
                    break;
            }
        }

        private void MirrorPlayerAbility()
        {
            var session = MaggyHelperModule.Session;
            if (session != null && session.CurrentCopyAbility != CopyAbilityType.None)
            {
                mirroredAbility = session.CurrentCopyAbility;
                ChangeState(BossState.Attacking);
                sprite?.Play("ability_attack");
            }
            else
            {
                ChangeState(BossState.Moving);
            }
        }

        private void UpdateAttacking()
        {
            attackTimer += Engine.DeltaTime;
            
            if (attackPattern == 1)
            {
                // Star spit - fire projectile at player
                if (attackTimer > 0.5f && Scene.OnInterval(0.3f))
                {
                    SpawnStarProjectile();
                }
            }
            
            if (mirroredAbility != CopyAbilityType.None)
            {
                UseMirroredAbility();
            }
            
            if (stateTimer > 3f)
            {
                mirroredAbility = CopyAbilityType.None;
                currentCooldown = attackCooldown;
                ChangeState(BossState.Vulnerable);
            }
        }

        private void SpawnStarProjectile()
        {
            if (player == null) return;
            
            var direction = player.Center - Center;
            direction.Normalize();
            
            var star = new Projectiles.BossStarProjectile(Center, direction * 150f);
            Scene.Add(star);
            
            Audio.Play("event:/game/general/thing_booped", Position);
        }

        private void UseMirroredAbility()
        {
            if (!Scene.OnInterval(0.5f)) return;
            
            var facing = player.X > X ? 1 : -1;
            
            switch (mirroredAbility)
            {
                case CopyAbilityType.Fire:
                    Scene.Add(new Projectiles.FireProjectile(Center, facing));
                    break;
                case CopyAbilityType.Ice:
                    Scene.Add(new Projectiles.IceProjectile(Center, facing));
                    break;
                case CopyAbilityType.Beam:
                    Scene.Add(new Projectiles.BeamProjectile(Center, facing));
                    break;
            }
        }

        private void UpdateCharging()
        {
            if (attackPattern == 0)
            {
                // Inhale - create suction effect
                if (player != null)
                {
                    var direction = Center - player.Center;
                    direction.Normalize();
                    // Pull player slightly toward boss
                    player.Speed += direction * 20f * Engine.DeltaTime;
                }
            }
            else if (attackPattern == 2)
            {
                // Float up
                Position.Y -= 100f * Engine.DeltaTime;
                
                if (stateTimer > 1.5f)
                {
                    // Dive toward player
                    targetPosition = player.Center;
                    ChangeState(BossState.Attacking);
                }
            }
            
            if (stateTimer > 3f)
            {
                currentCooldown = attackCooldown;
                ChangeState(BossState.Vulnerable);
            }
        }

        private void UpdateVulnerable()
        {
            // Boss is tired, easier to hit
            sprite?.Play("tired");
            
            if (stateTimer > 2f)
            {
                ChangeState(BossState.Moving);
            }
        }

        protected override void Defeat()
        {
            base.Defeat();
            
            // Special defeat animation for Shadow Kirby
            sprite?.Play("defeat");
            
            // Fade out effect
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.QuadOut, 2f, true);
            tween.OnUpdate = t => sprite.Color = Color.White * (1f - t.Eased);
            tween.OnComplete = t => RemoveSelf();
            Add(tween);
        }

        public override CopyAbilityType GetCopyAbility()
        {
            // Shadow Kirby drops the Mirror ability
            return CopyAbilityType.Mirror;
        }
    }
}
