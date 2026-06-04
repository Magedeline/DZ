using System;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste.Entities.Bosses;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Attaches to the vanilla <see cref="global::Celeste.Player"/> to provide
    /// Kirby-specific mechanics: float, multi-jump (up to 5 air puffs), inhale,
    /// and copy-ability attacks.
    /// </summary>
    public class KirbyPlayerController : Component
    {
        private global::Celeste.Player player;

        private int jumpCount;
        private bool wasOnGround;
        private float floatTimer;
        private bool isInhaling;
        private float inhaleTimer;
        private float inhaleCooldown;
        private float attackCooldown;
        private float floatReleaseCoyote;

        /// <summary>True while Kirby is actively inhaling.</summary>
        public bool IsInhaling => isInhaling;

        // Squish & stretch visuals
        private float duckSquish;
        private float duckWobble;
        private float landSquish;

        // Constants
        private const int MaxAirJumps = 5;
        private const float FloatMaxFall = 35f;
        private const float InhaleDuration = 1.2f;
        private const float InhaleCooldownMax = 0.25f;
        private const float AttackCooldownMax = 0.3f;
        private const float InhalePullRange = 72f;
        private const float AirJumpSpeed = -95f;
        private const float FloatReleaseCoyoteTime = 0.08f;

        // Squish & stretch constants
        private const float DuckSquishX = 1.28f;
        private const float DuckSquishY = 0.68f;
        private const float DuckSquishSpeed = 12f;
        private const float DuckUnsquishSpeed = 10f;
        private const float DuckWobbleSpeed = 6f;
        private const float DuckWobbleAmount = 0.03f;
        private const float LandSquishX = 1.18f;
        private const float LandSquishY = 0.78f;
        private const float LandSquishSpeed = 14f;
        private const float LandUnsquishSpeed = 8f;

        public KirbyPlayerController() : base(true, true) { }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as global::Celeste.Player;
        }

        public override void Update()
        {
            if (player == null || !player.IsKirbyMode())
                return;

            // Tick timers
            if (inhaleCooldown > 0f)
                inhaleCooldown -= Engine.DeltaTime;
            if (attackCooldown > 0f)
                attackCooldown -= Engine.DeltaTime;
            if (floatReleaseCoyote > 0f)
                floatReleaseCoyote -= Engine.DeltaTime;

            bool onGround = player.OnGround();

            // Reset air state on landing
            if (onGround && !wasOnGround)
            {
                jumpCount = 0;
                floatTimer = 0f;
                isInhaling = false;
                inhaleTimer = 0f;
                landSquish = 1f; // trigger land squish
            }

            // Only apply Kirby mechanics when the player is in a normal controllable state
            int state = player.StateMachine.State;
            bool canControl = state == global::Celeste.Player.StNormal
                           || state == global::Celeste.Player.StStarFly;

            if (canControl)
            {
                HandleFloat(onGround);
                HandleMultiJump(onGround);
                HandleInhaleAndAbilities();
            }

            // Squish & stretch visuals (only in normal states so we don't fight dream-dash etc.)
            if (state == global::Celeste.Player.StNormal)
            {
                UpdateSquishAndStretch();
            }
            else
            {
                // Reset scales when leaving normal state
                duckSquish = 0f;
                landSquish = 0f;
            }

            wasOnGround = onGround;
        }

        private void HandleFloat(bool onGround)
        {
            if (onGround)
                return;

            // Float: holding Jump while falling slows descent (Kirby's puff)
            if (Input.Jump.Check)
            {
                floatReleaseCoyote = FloatReleaseCoyoteTime;
                floatTimer += Engine.DeltaTime;

                // Cap fall speed to Kirby's gentle float
                if (player.Speed.Y > FloatMaxFall)
                    player.Speed.Y = FloatMaxFall;

                // Small initial upward boost when first starting to float while falling
                if (floatTimer < 0.15f && player.Speed.Y > 0f)
                {
                    player.Speed.Y -= 250f * Engine.DeltaTime;
                    if (player.Speed.Y < FloatMaxFall * -0.5f)
                        player.Speed.Y = FloatMaxFall * -0.5f;
                }
            }
            else if (floatReleaseCoyote > 0f && player.Speed.Y > FloatMaxFall)
            {
                // Brief grace period after releasing jump where fall speed stays capped
                player.Speed.Y = FloatMaxFall;
            }
        }

        private void HandleMultiJump(bool onGround)
        {
            if (onGround || isInhaling)
                return;

            // Kirby gets up to 5 mid-air "puff" jumps
            if (Input.Jump.Pressed && jumpCount < MaxAirJumps)
            {
                jumpCount++;
                player.Speed.Y = AirJumpSpeed;
                floatTimer = 0f; // reset float timer for fresh puff

                SpawnPuffParticles();
                PlayPuffSound();
            }
        }

        private void HandleInhaleAndAbilities()
        {
            if (isInhaling)
            {
                inhaleTimer -= Engine.DeltaTime;
                TryPullNearbyEntities();

                if (inhaleTimer <= 0f || !Input.Grab.Check)
                {
                    EndInhale();
                }
                return;
            }

            if (!Input.Grab.Pressed || inhaleCooldown > 0f)
                return;

            var session = MaggyHelperModule.Session;
            CopyAbilityType currentPower = session?.CurrentCopyAbility ?? CopyAbilityType.None;

            if (currentPower == CopyAbilityType.None)
            {
                StartInhale();
            }
            else
            {
                UseCopyAbility(currentPower);
            }
        }

        private void UpdateSquishAndStretch()
        {
            float dt = Engine.DeltaTime;
            bool ducking = player.Ducking;

            // --- Duck squish ---
            if (ducking)
            {
                duckSquish = Calc.Approach(duckSquish, 1f, dt * DuckSquishSpeed);
            }
            else
            {
                duckSquish = Calc.Approach(duckSquish, 0f, dt * DuckUnsquishSpeed);
            }

            // --- Land squish decay ---
            if (landSquish > 0f)
            {
                landSquish = Calc.Approach(landSquish, 0f, dt * LandUnsquishSpeed);
            }

            // Combine and apply scale
            float totalSquish = Math.Max(duckSquish, landSquish * 0.7f);
            if (totalSquish > 0.001f)
            {
                // Ease out cubic for smoother settle
                float t = 1f - (1f - totalSquish) * (1f - totalSquish) * (1f - totalSquish);

                float targetSx, targetSy;
                if (duckSquish >= landSquish)
                {
                    // Duck shape dominates
                    targetSx = MathHelper.Lerp(1f, DuckSquishX, t);
                    targetSy = MathHelper.Lerp(1f, DuckSquishY, t);

                    // Idle wobble while held down
                    if (ducking && duckSquish > 0.8f)
                    {
                        duckWobble += dt * DuckWobbleSpeed;
                        float w = (float)Math.Sin(duckWobble) * DuckWobbleAmount;
                        targetSx += w;
                        targetSy -= w * 0.5f;
                    }
                }
                else
                {
                    // Land bounce shape
                    targetSx = MathHelper.Lerp(1f, LandSquishX, t);
                    targetSy = MathHelper.Lerp(1f, LandSquishY, t);

                    // Overshoot bounce when recovering
                    if (landSquish > 0f && landSquish < 0.5f)
                    {
                        float bounce = (float)Math.Sin(landSquish * Math.PI * 2f) * 0.08f * (1f - landSquish);
                        targetSx += bounce;
                        targetSy -= bounce * 0.6f;
                    }
                }

                player.Sprite.Scale = new Vector2(targetSx, targetSy);
            }
            else
            {
                player.Sprite.Scale = Vector2.One;
            }
        }

        private void StartInhale()
        {
            isInhaling = true;
            inhaleTimer = InhaleDuration;
            player.Speed.X *= 0.3f; // slow horizontal movement while inhaling

            // Visual / audio feedback
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + new Vector2((int)player.Facing * 6, -4), Color.White);
            }
        }

        private void EndInhale()
        {
            isInhaling = false;
            inhaleCooldown = InhaleCooldownMax;
            TrySwallow();
        }

        private void TryPullNearbyEntities()
        {
            if (player.Scene == null)
                return;

            Vector2 mouthPos = player.Center + new Vector2((int)player.Facing * 8, -4);
            Rectangle pullBox = new Rectangle(
                (int)(mouthPos.X - InhalePullRange / 2f),
                (int)(mouthPos.Y - InhalePullRange / 2f),
                (int)InhalePullRange,
                (int)InhalePullRange);

            foreach (Entity entity in player.Scene.Entities)
            {
                if (entity == player || entity.Collider == null)
                    continue;

                // Pull ability stars
                if (entity is AbilityStar star && pullBox.Contains((int)star.Center.X, (int)star.Center.Y))
                {
                    Vector2 dir = (mouthPos - star.Center).SafeNormalize();
                    star.Position += dir * 120f * Engine.DeltaTime;
                }

                // Pull small enemies / items that implement IKirbyCopySource
                if (entity is IKirbyCopySource copySource && pullBox.Contains((int)entity.Center.X, (int)entity.Center.Y))
                {
                    if (entity is Actor actor)
                    {
                        Vector2 dir = (mouthPos - actor.Center).SafeNormalize();
                        actor.Position += dir * 100f * Engine.DeltaTime;
                    }
                }
            }
        }

        private void TrySwallow()
        {
            if (player.Scene == null)
                return;

            Vector2 mouthPos = player.Center + new Vector2((int)player.Facing * 8, -4);
            float swallowRange = 20f;

            foreach (Entity entity in player.Scene.Entities)
            {
                if (entity == player)
                    continue;

                // Swallow ability stars
                if (entity is AbilityStar star && Vector2.Distance(mouthPos, star.Center) < swallowRange)
                {
                    var session = MaggyHelperModule.Session;
                    if (session != null)
                    {
                        session.CurrentCopyAbility = CopyAbilityType.None; // star already set its own ability on touch
                    }
                    star.RemoveSelf();
                    SpawnSwallowParticles();
                    return;
                }

                // Swallow enemies with copy abilities
                if (entity is IKirbyCopySource copySource && Vector2.Distance(mouthPos, entity.Center) < swallowRange)
                {
                    var session = MaggyHelperModule.Session;
                    if (session != null)
                    {
                        session.CurrentCopyAbility = copySource.GetCopyAbility();
                        session.CurrentKirbyPower = session.CurrentCopyAbility.ToString();
                    }

                    if (entity is Actor actor && actor is not global::Celeste.Player)
                    {
                        actor.RemoveSelf();
                    }
                    else
                    {
                        entity.RemoveSelf();
                    }

                    SpawnSwallowParticles();
                    return;
                }
            }
        }

        private void UseCopyAbility(CopyAbilityType ability)
        {
            if (attackCooldown > 0f)
                return;
            attackCooldown = AttackCooldownMax;

            switch (ability)
            {
                case CopyAbilityType.Hammer:
                    UseHammer();
                    break;
                case CopyAbilityType.Sword:
                case CopyAbilityType.Cutter:
                    UseSlash();
                    break;
                case CopyAbilityType.Fire:
                case CopyAbilityType.Ice:
                case CopyAbilityType.Spark:
                case CopyAbilityType.Beam:
                    UseProjectile(ability);
                    break;
                case CopyAbilityType.Bomb:
                    UseBomb();
                    break;
                case CopyAbilityType.Stone:
                    UseStone();
                    break;
                case CopyAbilityType.Wheel:
                    UseWheel();
                    break;
                case CopyAbilityType.Fighter:
                case CopyAbilityType.Suplex:
                case CopyAbilityType.Ninja:
                    UseMelee(ability);
                    break;
                case CopyAbilityType.Parasol:
                    UseParasol();
                    break;
                case CopyAbilityType.Wing:
                    UseWing();
                    break;
                default:
                    UseStarSpit();
                    break;
            }
        }

        private void UseHammer()
        {
            Vector2 hitPos = player.Center + new Vector2((int)player.Facing * 14, 0);
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, hitPos, Color.Brown, (float)Math.PI / 2f);
                level.Shake(0.15f);
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseSlash()
        {
            Vector2 slashPos = player.Center + new Vector2((int)player.Facing * 10, -4);
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, slashPos, Color.Silver, (float)Math.PI / 4f);
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseProjectile(CopyAbilityType ability)
        {
            Vector2 dir = new Vector2((int)player.Facing, 0);
            if (Math.Abs(Input.MoveY.Value) > 0.1f)
                dir.Y = Input.MoveY.Value;
            dir.Normalize();

            Color c = ability switch
            {
                CopyAbilityType.Fire => Color.OrangeRed,
                CopyAbilityType.Ice => Color.LightCyan,
                CopyAbilityType.Spark => Color.Yellow,
                CopyAbilityType.Beam => Color.Cyan,
                _ => Color.White
            };

            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + dir * 8, c, dir.Angle());
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseBomb()
        {
            Vector2 throwDir = new Vector2((int)player.Facing, -0.6f);
            throwDir.Normalize();

            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + throwDir * 8, Color.DarkGray, throwDir.Angle());
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseStone()
        {
            player.Speed.Y = Math.Max(player.Speed.Y, 280f);
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.BottomCenter, Color.Gray, (float)Math.PI / 2f);
            }
        }

        private void UseWheel()
        {
            player.Speed.X = (int)player.Facing * 320f;
            player.Speed.Y = 0f;
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.BottomCenter, Color.Red, (float)Math.PI / 2f);
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseMelee(CopyAbilityType ability)
        {
            Vector2 hitPos = player.Center + new Vector2((int)player.Facing * 12, 0);
            Color c = ability == CopyAbilityType.Ninja ? Color.Purple : Color.Orange;
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, hitPos, c, (float)Math.PI / 2f);
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseParasol()
        {
            // Slow fall + shield in front
            if (player.Speed.Y > 40f)
                player.Speed.Y = 40f;
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + new Vector2((int)player.Facing * 10, 0), Color.LightPink);
            }
        }

        private void UseWing()
        {
            // Quick dash in air
            if (!player.OnGround())
            {
                player.Speed.X = (int)player.Facing * 260f;
                player.Speed.Y = -60f;
            }
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center, Color.LightSkyBlue);
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void UseStarSpit()
        {
            Vector2 dir = new Vector2((int)player.Facing, 0);
            if (Math.Abs(Input.MoveY.Value) > 0.1f)
                dir.Y = Input.MoveY.Value;
            dir.Normalize();

            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + dir * 8, Color.White, dir.Angle());
            }
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
        }

        private void SpawnPuffParticles()
        {
            if (player.Scene is not Level level)
                return;

            for (int i = 0; i < 6; i++)
            {
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.Particles.Emit(ParticleTypes.Dust, player.Center + Calc.AngleToVector(angle, 4f), Color.White, angle);
            }
        }

        private void SpawnSwallowParticles()
        {
            if (player.Scene is not Level level)
                return;

            for (int i = 0; i < 10; i++)
            {
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.Particles.Emit(ParticleTypes.Dust, player.Center, Color.HotPink, angle);
            }
        }

        private void PlayPuffSound()
        {
            try
            {
                Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
            }
            catch
            {
                // Ignore missing audio
            }
        }
    }
}

