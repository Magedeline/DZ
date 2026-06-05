using System;
using System.Collections.Generic;
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
    /// 
    /// Enhanced version with:
    /// - Polished visual effects and particles
    /// - Copy ability visual indicators (hat colors/glows)
    /// - Knockback and enemy interaction system
    /// - Audio cue system with variations
    /// - Animation triggers and expression changes
    /// - Smooth movement feel
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

        // NEW: Enhanced float state for true Kirby hovering
        private bool isFloating;          // True when actively floating/hovering
        private float floatPuffTimer;     // Timer between float puffs
        private const float FloatPuffInterval = 0.25f;  // Time between automatic puffs while floating

        /// <summary>True while Kirby is actively inhaling.</summary>
        public bool IsInhaling => isInhaling;

        // Squish & stretch visuals
        private float duckSquish;
        private float duckWobble;
        private float landSquish;

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Enhanced visual state
        // ═══════════════════════════════════════════════════════════════════════
        private float abilityGlowPulse;
        private float expressionTimer;
        private KirbyExpression currentExpression = KirbyExpression.Normal;
        private float inhaleVortexAngle;
        private List<InhaleParticle> inhaleParticles = new();
        private float floatWobble;
        private int lastPuffDirection = 5;

        // Movement smoothing
        private float targetSpeedX;
        private float currentAcceleration;
        private float momentumCarry;

        // Attack state
        private bool isAttacking;
        private float attackAnimTimer;
        private CopyAbilityType lastUsedAbility;
        private int comboCount;
        private float comboTimer;

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

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Enhanced constants
        // ═══════════════════════════════════════════════════════════════════════
        private const float MovementAcceleration = 800f;
        private const float MovementDeceleration = 1200f;
        private const float AirAcceleration = 600f;
        private const float MomentumDecay = 0.92f;
        private const float ComboTimeWindow = 0.8f;
        private const float AbilityGlowSpeed = 4f;
        private const float FloatWobbleSpeed = 8f;
        private const float FloatWobbleAmount = 0.08f;
        private const int InhaleParticleCount = 12;

        // Audio GUIDs
        private const string SFX_PUFF = "event:/game/general/diamond_touch";
        private const string SFX_INHALE_START = "event:/game/general/diamond_touch";
        private const string SFX_INHALE_LOOP = "event:/game/general/diamond_touch";
        private const string SFX_SWALLOW = "event:/game/general/diamond_touch";
        private const string SFX_ABILITY_GET = "event:/game/general/diamond_touch";
        private const string SFX_LAND = "event:/game/general/diamond_touch";

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

            float dt = Engine.DeltaTime;

            // Tick timers
            if (inhaleCooldown > 0f)
                inhaleCooldown -= dt;
            if (attackCooldown > 0f)
                attackCooldown -= dt;
            if (floatReleaseCoyote > 0f)
                floatReleaseCoyote -= dt;
            if (comboTimer > 0f)
            {
                comboTimer -= dt;
                if (comboTimer <= 0f)
                    comboCount = 0;
            }
            if (attackAnimTimer > 0f)
            {
                attackAnimTimer -= dt;
                if (attackAnimTimer <= 0f)
                    isAttacking = false;
            }
            if (expressionTimer > 0f)
            {
                expressionTimer -= dt;
                if (expressionTimer <= 0f)
                    currentExpression = KirbyExpression.Normal;
            }

            bool onGround = player.OnGround();

            // Reset air state on landing
            if (onGround && !wasOnGround)
            {
                OnLanding();
            }

            // Only apply Kirby mechanics when the player is in a normal controllable state
            int state = player.StateMachine.State;
            bool canControl = state == global::Celeste.Player.StNormal
                           || state == global::Celeste.Player.StStarFly;

            if (canControl)
            {
                HandleMovementSmoothing(onGround);
                HandleFloat(onGround);
                HandleMultiJump(onGround);
                HandleInhaleAndAbilities();
            }

            // Visual updates (always run for smooth visuals)
            UpdateAbilityGlow(dt);
            UpdateInhaleVortex(dt);
            UpdateFloatWobble(dt);

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

        public override void Render()
        {
            base.Render();

            if (player == null || !player.IsKirbyMode())
                return;

            // Render ability glow
            RenderAbilityGlow();

            // Render inhale vortex effect
            if (isInhaling)
            {
                RenderInhaleVortex();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Landing effects
        // ═══════════════════════════════════════════════════════════════════════
        private void OnLanding()
        {
            jumpCount = 0;
            floatTimer = 0f;
            isInhaling = false;
            inhaleTimer = 0f;
            landSquish = 1f;
            momentumCarry = 0f;
            isFloating = false;       // Reset float state on landing
            floatPuffTimer = 0f;
            // Landing particles and sound
            if (player.Scene is Level level)
            {
                float impactStrength = Math.Min(1f, Math.Abs(player.Speed.Y) / 200f);
                
                // Dust poof particles
                int particleCount = (int)(6 * impactStrength) + 2;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.Pi + Calc.Random.Range(-0.5f, 0.5f);
                    Vector2 offset = new Vector2(Calc.Random.Range(-4f, 4f), 0f);
                    level.Particles.Emit(ParticleTypes.Dust, player.BottomCenter + offset, Color.White * 0.7f, angle);
                }

                // Screen shake for heavy landing
                if (impactStrength > 0.6f)
                {
                    level.Shake(0.08f * impactStrength);
                    SetExpression(KirbyExpression.Surprised, 0.3f);
                }

                // Landing sound with pitch variation
                PlaySoundWithPitch(SFX_LAND, player.Position, 0.9f + impactStrength * 0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Movement smoothing for "heavier" feel
        // ═══════════════════════════════════════════════════════════════════════
        private void HandleMovementSmoothing(bool onGround)
        {
            float dt = Engine.DeltaTime;
            float inputX = Input.MoveX.Value;

            // Calculate target speed based on input
            targetSpeedX = inputX * 90f; // Base Kirby walk speed

            // Apply momentum from attacks/abilities
            if (momentumCarry != 0f)
            {
                player.Speed.X += momentumCarry * dt;
                momentumCarry *= MomentumDecay;
                if (Math.Abs(momentumCarry) < 1f)
                    momentumCarry = 0f;
            }

            // Smooth acceleration/deceleration
            float accel = onGround ? MovementAcceleration : AirAcceleration;
            float decel = onGround ? MovementDeceleration : AirAcceleration * 0.8f;

            if (Math.Abs(inputX) > 0.1f)
            {
                // Accelerating
                currentAcceleration = Calc.Approach(currentAcceleration, accel, accel * 2f * dt);
            }
            else
            {
                // Decelerating - Kirby stops a bit slower for that bouncy feel
                currentAcceleration = Calc.Approach(currentAcceleration, decel, decel * dt);
            }
        }

        private void HandleFloat(bool onGround)
        {
            if (onGround)
            {
                floatWobble = 0f;
                isFloating = false;
                return;
            }

            // Float: holding Jump while falling enables Kirby's signature hover
            if (Input.Jump.Check && jumpCount > 0) // Must have done at least one air jump to float
            {
                floatReleaseCoyote = FloatReleaseCoyoteTime;
                floatTimer += Engine.DeltaTime;
                isFloating = true;

                // Cap fall speed to Kirby's gentle float
                if (player.Speed.Y > FloatMaxFall)
                    player.Speed.Y = FloatMaxFall;

                // Automatic puff animation/particles while floating
                floatPuffTimer -= Engine.DeltaTime;
                if (floatPuffTimer <= 0f)
                {
                    floatPuffTimer = FloatPuffInterval;
                    SpawnFloatPuffParticles();
                    
                    // Small upward boost to maintain altitude
                    if (player.Speed.Y > 0f)
                    {
                        player.Speed.Y -= 40f;
                        if (player.Speed.Y < 0f)
                            player.Speed.Y = 0f;
                    }
                }

                // Float trail particles
                SpawnFloatTrailParticles();
            }
            else
            {
                isFloating = false;
                
                if (floatReleaseCoyote > 0f && player.Speed.Y > FloatMaxFall)
                {
                    // Brief grace period after releasing jump where fall speed stays capped
                    player.Speed.Y = FloatMaxFall;
                }
            }
        }

        private void HandleMultiJump(bool onGround)
        {
            if (onGround || isInhaling)
                return;

            // Kirby gets up to 5 mid-air "puff" jumps
            // Each jump press while in the air does a puff jump
            if (Input.Jump.Pressed && jumpCount < MaxAirJumps)
            {
                jumpCount++;
                
                // Apply puff jump velocity - decreasing strength with each puff
                float puffStrength = 1f - (jumpCount - 1) * 0.1f; // Slightly weaker each puff
                player.Speed.Y = AirJumpSpeed * puffStrength;
                
                floatTimer = 0f; // reset float timer for fresh puff
                floatPuffTimer = FloatPuffInterval; // reset auto-puff timer
                isFloating = true; // Enable floating after a puff jump

                // Alternate puff direction for visual variety
                lastPuffDirection *= -1;

                SpawnPuffParticles();
                PlayPuffSound();
                SetExpression(KirbyExpression.Puffed, 0.4f);

                // Slight horizontal momentum on puff
                if (Math.Abs(Input.MoveX.Value) > 0.1f)
                {
                    player.Speed.X += Input.MoveX.Value * 15f;
                }
            }
        }

        // NEW: Separate method for the periodic float puff particles
        private void SpawnFloatPuffParticles()
        {
            if (player.Scene is not Level level)
                return;

            // Small circular puff below Kirby
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 + Calc.Random.Range(-0.4f, 0.4f); // Downward
                Vector2 offset = new Vector2(Calc.Random.Range(-3f, 3f), 4f);
                level.Particles.Emit(ParticleTypes.Dust, player.Center + offset, Color.White * 0.5f, angle);
            }
        }

        private void HandleInhaleAndAbilities()
        {
            if (isInhaling)
            {
                inhaleTimer -= Engine.DeltaTime;
                TryPullNearbyEntities();
                UpdateInhaleParticles();

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

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Visual effect updates
        // ═══════════════════════════════════════════════════════════════════════
        private void UpdateAbilityGlow(float dt)
        {
            var session = MaggyHelperModule.Session;
            CopyAbilityType ability = session?.CurrentCopyAbility ?? CopyAbilityType.None;

            if (ability != CopyAbilityType.None)
            {
                abilityGlowPulse += dt * AbilityGlowSpeed;
                if (abilityGlowPulse > MathHelper.TwoPi)
                    abilityGlowPulse -= MathHelper.TwoPi;
            }
            else
            {
                abilityGlowPulse = 0f;
            }
        }

        private void UpdateInhaleVortex(float dt)
        {
            if (isInhaling)
            {
                inhaleVortexAngle += dt * 12f;
                if (inhaleVortexAngle > MathHelper.TwoPi)
                    inhaleVortexAngle -= MathHelper.TwoPi;
            }
        }

        private void UpdateFloatWobble(float dt)
        {
            if (!player.OnGround() && Input.Jump.Check)
            {
                floatWobble += dt * FloatWobbleSpeed;
                if (floatWobble > MathHelper.TwoPi)
                    floatWobble -= MathHelper.TwoPi;
            }
            else
            {
                floatWobble = Calc.Approach(floatWobble, 0f, dt * 4f);
            }
        }

        private void UpdateInhaleParticles()
        {
            // Spawn new inhale particles
            if (Calc.Random.Chance(0.4f))
            {
                Vector2 mouthPos = player.Center + new Vector2((int)player.Facing * 8, -4);
                float angle = Calc.Random.Range(-0.8f, 0.8f);
                float dist = InhalePullRange * Calc.Random.Range(0.5f, 1f);
                Vector2 startPos = mouthPos + Calc.AngleToVector(angle + (player.Facing == Facings.Right ? 0 : MathHelper.Pi), dist);
                
                inhaleParticles.Add(new InhaleParticle
                {
                    Position = startPos,
                    Target = mouthPos,
                    Life = 0.5f,
                    Color = Color.White * 0.6f
                });
            }

            // Update existing particles
            for (int i = inhaleParticles.Count - 1; i >= 0; i--)
            {
                var p = inhaleParticles[i];
                p.Life -= Engine.DeltaTime;
                p.Position = Vector2.Lerp(p.Position, p.Target, Engine.DeltaTime * 8f);
                
                if (p.Life <= 0f || Vector2.Distance(p.Position, p.Target) < 4f)
                {
                    inhaleParticles.RemoveAt(i);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Render ability glow around Kirby
        // ═══════════════════════════════════════════════════════════════════════
        private void RenderAbilityGlow()
        {
            var session = MaggyHelperModule.Session;
            CopyAbilityType ability = session?.CurrentCopyAbility ?? CopyAbilityType.None;

            if (ability == CopyAbilityType.None)
                return;

            Color glowColor = GetAbilityColor(ability);
            float pulse = 0.3f + (float)Math.Sin(abilityGlowPulse) * 0.15f;

            Vector2 center = player.Center.Floor();

            // Draw soft glow circle
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi + abilityGlowPulse * 0.5f;
                float dist = 10f + (float)Math.Sin(abilityGlowPulse + i) * 2f;
                Vector2 pos = center + Calc.AngleToVector(angle, dist);
                Draw.Point(pos, glowColor * pulse);
            }
        }

        private void RenderInhaleVortex()
        {
            Vector2 mouthPos = player.Center + new Vector2((int)player.Facing * 12, -4);

            // Draw swirl lines
            for (int i = 0; i < 6; i++)
            {
                float angle = inhaleVortexAngle + (i / 6f) * MathHelper.TwoPi;
                float dist = 8f + (float)Math.Sin(inhaleVortexAngle * 2 + i) * 4f;
                Vector2 pos = mouthPos + Calc.AngleToVector(angle, dist);
                Draw.Point(pos, Color.White * 0.5f);
            }

            // Draw pulled particles
            foreach (var p in inhaleParticles)
            {
                Draw.Point(p.Position, p.Color * (p.Life * 2f));
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

            // --- Float wobble ---
            float floatWobbleScale = (float)Math.Sin(floatWobble) * FloatWobbleAmount;

            // Combine and apply scale
            float totalSquish = Math.Max(duckSquish, landSquish * 0.7f);
            if (totalSquish > 0.001f || Math.Abs(floatWobbleScale) > 0.001f)
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

                // Add float wobble
                targetSx += floatWobbleScale;
                targetSy -= floatWobbleScale * 0.5f;

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
            inhaleParticles.Clear();

            SetExpression(KirbyExpression.Inhaling, InhaleDuration);

            // Visual / audio feedback
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.Dust, player.Center + new Vector2((int)player.Facing * 6, -4), Color.White);
                
                // Screen distortion effect hint
                level.Shake(0.05f);
            }

            PlaySoundWithPitch(SFX_INHALE_START, player.Position, 1f);
        }

        private void EndInhale()
        {
            isInhaling = false;
            inhaleCooldown = InhaleCooldownMax;
            inhaleParticles.Clear();
            TrySwallow();
            SetExpression(KirbyExpression.Normal, 0f);
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
                    
                    // Spawn trail particles behind pulled entity
                    if (Calc.Random.Chance(0.3f) && player.Scene is Level level)
                    {
                        level.Particles.Emit(ParticleTypes.Dust, star.Center, Color.Yellow * 0.5f);
                    }
                }

                // Pull small enemies / items that implement IKirbyCopySource
                if (entity is IKirbyCopySource copySource && pullBox.Contains((int)entity.Center.X, (int)entity.Center.Y))
                {
                    if (entity is Actor actor)
                    {
                        Vector2 dir = (mouthPos - actor.Center).SafeNormalize();
                        actor.Position += dir * 100f * Engine.DeltaTime;
                        
                        // Trail particles
                        if (Calc.Random.Chance(0.2f) && player.Scene is Level level)
                        {
                            level.Particles.Emit(ParticleTypes.Dust, actor.Center, Color.White * 0.4f);
                        }
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
                    PlayAbilityGetEffect();
                    return;
                }

                // Swallow enemies with copy abilities
                if (entity is IKirbyCopySource copySource && Vector2.Distance(mouthPos, entity.Center) < swallowRange)
                {
                    var session = MaggyHelperModule.Session;
                    CopyAbilityType newAbility = copySource.GetCopyAbility();
                    
                    if (session != null)
                    {
                        session.CurrentCopyAbility = newAbility;
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
                    
                    if (newAbility != CopyAbilityType.None)
                    {
                        PlayAbilityGetEffect();
                        SetExpression(KirbyExpression.Happy, 1f);
                    }
                    return;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Ability get celebration effect
        // ═══════════════════════════════════════════════════════════════════════
        private void PlayAbilityGetEffect()
        {
            if (player.Scene is not Level level)
                return;

            var session = MaggyHelperModule.Session;
            CopyAbilityType ability = session?.CurrentCopyAbility ?? CopyAbilityType.None;
            Color abilityColor = GetAbilityColor(ability);

            // Burst of colored particles
            for (int i = 0; i < 20; i++)
            {
                float angle = (i / 20f) * MathHelper.TwoPi;
                level.Particles.Emit(ParticleTypes.Dust, player.Center, abilityColor, angle);
            }

            // Screen flash
            level.Flash(abilityColor * 0.3f, true);
            level.Shake(0.2f);

            // Sound
            Audio.Play(SFX_ABILITY_GET, player.Position);

            // Slow-mo for dramatic effect
            Engine.TimeRate = 0.5f;
            Alarm.Set(player, 0.15f, () => Engine.TimeRate = 1f);
        }

        private void UseCopyAbility(CopyAbilityType ability)
        {
            if (attackCooldown > 0f)
                return;
            
            // Combo system
            if (ability == lastUsedAbility && comboTimer > 0f)
            {
                comboCount = Math.Min(comboCount + 1, 3);
            }
            else
            {
                comboCount = 0;
            }
            lastUsedAbility = ability;
            comboTimer = ComboTimeWindow;
            
            attackCooldown = AttackCooldownMax * (1f - comboCount * 0.1f); // Faster attacks in combo
            isAttacking = true;
            attackAnimTimer = 0.2f;
            
            SetExpression(KirbyExpression.Attacking, 0.3f);

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
            float comboBonus = 1f + comboCount * 0.3f;
            
            if (player.Scene is Level level)
            {
                // Heavy impact particles
                for (int i = 0; i < 8; i++)
                {
                    float angle = Calc.Random.Range(-0.5f, 0.5f) + ((int)player.Facing == 1 ? 0 : MathHelper.Pi);
                    level.Particles.Emit(ParticleTypes.Dust, hitPos, Color.Brown, angle);
                }
                
                level.Shake(0.15f * comboBonus);
                
                // Spawn attack hitbox
                SpawnAttackHitbox(hitPos, 20f, 3 * comboBonus, Color.Brown);
            }
            
            // Momentum from swing
            momentumCarry = (int)player.Facing * 50f * comboBonus;
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseSlash()
        {
            Vector2 slashPos = player.Center + new Vector2((int)player.Facing * 10, -4);
            float comboBonus = 1f + comboCount * 0.2f;
            
            if (player.Scene is Level level)
            {
                // Slash arc particles
                for (int i = 0; i < 6; i++)
                {
                    float angle = -0.8f + (i / 5f) * 1.6f;
                    if ((int)player.Facing == -1) angle = MathHelper.Pi - angle;
                    level.Particles.Emit(ParticleTypes.Dust, slashPos + Calc.AngleToVector(angle, 8f), Color.Silver, angle);
                }
                
                SpawnAttackHitbox(slashPos, 16f, 2 * comboBonus, Color.Silver);
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseProjectile(CopyAbilityType ability)
        {
            Vector2 dir = new Vector2((int)player.Facing, 0);
            if (Math.Abs(Input.MoveY.Value) > 0.1f)
                dir.Y = Input.MoveY.Value;
            dir.Normalize();

            Color c = GetAbilityColor(ability);

            if (player.Scene is Level level)
            {
                // Spawn actual projectile entity
                var projectile = new KirbyProjectile(
                    player.Center + dir * 8,
                    dir * 200f,
                    c,
                    ability,
                    2f
                );
                level.Add(projectile);
                
                // Muzzle flash particles
                for (int i = 0; i < 4; i++)
                {
                    float angle = dir.Angle() + Calc.Random.Range(-0.3f, 0.3f);
                    level.Particles.Emit(ParticleTypes.Dust, player.Center + dir * 8, c, angle);
                }
            }
            
            // Slight recoil
            player.Speed -= dir * 20f;
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseBomb()
        {
            Vector2 throwDir = new Vector2((int)player.Facing, -0.6f);
            throwDir.Normalize();

            if (player.Scene is Level level)
            {
                // Spawn bomb entity
                var bomb = new KirbyBomb(player.Center + throwDir * 8, throwDir * 150f);
                level.Add(bomb);
                
                level.Particles.Emit(ParticleTypes.Dust, player.Center + throwDir * 8, Color.DarkGray, throwDir.Angle());
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseStone()
        {
            player.Speed.Y = Math.Max(player.Speed.Y, 280f);
            player.Speed.X *= 0.3f; // Reduce horizontal movement
            
            SetExpression(KirbyExpression.Stone, 1f);
            
            if (player.Scene is Level level)
            {
                // Stone transformation particles
                for (int i = 0; i < 6; i++)
                {
                    float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                    level.Particles.Emit(ParticleTypes.Dust, player.Center, Color.Gray, angle);
                }
            }
        }

        private void UseWheel()
        {
            float wheelSpeed = 320f * (1f + comboCount * 0.15f);
            player.Speed.X = (int)player.Facing * wheelSpeed;
            player.Speed.Y = 0f;
            
            momentumCarry = (int)player.Facing * 100f;
            
            if (player.Scene is Level level)
            {
                // Wheel dust trail
                for (int i = 0; i < 4; i++)
                {
                    level.Particles.Emit(ParticleTypes.Dust, player.BottomCenter + new Vector2(Calc.Random.Range(-4f, 4f), 0), Color.Red, MathHelper.Pi);
                }
                
                SpawnAttackHitbox(player.Center, 12f, 2f, Color.Red);
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseMelee(CopyAbilityType ability)
        {
            Vector2 hitPos = player.Center + new Vector2((int)player.Facing * 12, 0);
            Color c = ability == CopyAbilityType.Ninja ? Color.Purple : Color.Orange;
            float comboBonus = 1f + comboCount * 0.25f;
            
            if (player.Scene is Level level)
            {
                // Melee strike particles
                for (int i = 0; i < 5; i++)
                {
                    float angle = Calc.Random.Range(-0.4f, 0.4f);
                    if ((int)player.Facing == -1) angle += MathHelper.Pi;
                    level.Particles.Emit(ParticleTypes.Dust, hitPos, c, angle);
                }
                
                SpawnAttackHitbox(hitPos, 14f, 2.5f * comboBonus, c);
            }
            
            // Dash forward on hit
            if (comboCount > 0)
            {
                player.Speed.X = (int)player.Facing * 80f;
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseParasol()
        {
            // Slow fall + shield in front
            if (player.Speed.Y > 40f)
                player.Speed.Y = 40f;
            
            Vector2 shieldPos = player.Center + new Vector2((int)player.Facing * 10, 0);
            
            if (player.Scene is Level level)
            {
                // Gentle floating particles
                level.Particles.Emit(ParticleTypes.Dust, shieldPos, Color.LightPink);
                
                // The parasol acts as a shield - spawn hitbox above
                SpawnAttackHitbox(player.Center + new Vector2(0, -12), 16f, 1f, Color.LightPink);
            }
        }

        private void UseWing()
        {
            // Quick dash in air
            if (!player.OnGround())
            {
                float dashSpeed = 260f * (1f + comboCount * 0.1f);
                player.Speed.X = (int)player.Facing * dashSpeed;
                player.Speed.Y = -60f;
                
                // Reset jump count partially - wing gives mobility
                jumpCount = Math.Max(0, jumpCount - 1);
            }
            
            if (player.Scene is Level level)
            {
                // Feather trail
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.Pi + (int)player.Facing * 0.5f + Calc.Random.Range(-0.5f, 0.5f);
                    level.Particles.Emit(ParticleTypes.Dust, player.Center + new Vector2(Calc.Random.Range(-6f, 6f), Calc.Random.Range(-4f, 4f)), Color.LightSkyBlue, angle);
                }
                
                SpawnAttackHitbox(player.Center, 18f, 1.5f, Color.LightSkyBlue);
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        private void UseStarSpit()
        {
            Vector2 dir = new Vector2((int)player.Facing, 0);
            if (Math.Abs(Input.MoveY.Value) > 0.1f)
                dir.Y = Input.MoveY.Value;
            dir.Normalize();

            if (player.Scene is Level level)
            {
                // Spawn star projectile
                var star = new KirbyProjectile(
                    player.Center + dir * 8,
                    dir * 180f,
                    Color.White,
                    CopyAbilityType.None,
                    1.5f
                );
                level.Add(star);
                
                level.Particles.Emit(ParticleTypes.Dust, player.Center + dir * 8, Color.White, dir.Angle());
            }
            
            Audio.Play(SFX_PUFF, player.Position);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Attack hitbox spawning
        // ═══════════════════════════════════════════════════════════════════════
        private void SpawnAttackHitbox(Vector2 position, float radius, float damage, Color color)
        {
            if (player.Scene is not Level level)
                return;

            // Check for enemies in range
            foreach (Entity entity in level.Entities)
            {
                if (entity == player)
                    continue;

                // Damage IKirbyCopySource enemies
                if (entity is IKirbyCopySource && Vector2.Distance(position, entity.Center) < radius)
                {
                    // Apply knockback
                    if (entity is Actor actor)
                    {
                        Vector2 knockDir = (entity.Center - player.Center).SafeNormalize();
                        // Knockback would be applied here if enemies had velocity
                    }

                    // Spawn hit particles
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = Calc.AngleToVector(angle, Calc.Random.Range(50f, 100f));
                        level.Add(new HitParticle(entity.Center, vel, color));
                    }

                    // Screen shake based on damage
                    level.Shake(0.05f * damage);
                }
            }
        }

        private void SpawnPuffParticles()
        {
            if (player.Scene is not Level level)
                return;

            // Circular burst of puff particles
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi + Calc.Random.Range(-0.2f, 0.2f);
                Vector2 offset = Calc.AngleToVector(angle, 6f);
                level.Particles.Emit(ParticleTypes.Dust, player.Center + offset, Color.White * 0.8f, angle);
            }

            // Extra particles in direction of movement for visual flair
            if (Math.Abs(player.Speed.X) > 10f)
            {
                float moveAngle = player.Speed.X > 0 ? MathHelper.Pi : 0;
                for (int i = 0; i < 3; i++)
                {
                    level.Particles.Emit(ParticleTypes.Dust, player.Center, Color.White * 0.5f, moveAngle + Calc.Random.Range(-0.3f, 0.3f));
                }
            }
        }

        private void SpawnFloatTrailParticles()
        {
            if (player.Scene is not Level level)
                return;

            // Occasional trail particles while floating
            if (Calc.Random.Chance(0.15f))
            {
                Vector2 offset = new Vector2(Calc.Random.Range(-4f, 4f), Calc.Random.Range(4f, 8f));
                level.Particles.Emit(ParticleTypes.Dust, player.Center + offset, Color.White * 0.3f, MathHelper.PiOver2);
            }
        }

        private void SpawnSwallowParticles()
        {
            if (player.Scene is not Level level)
                return;

            // Implosion effect
            for (int i = 0; i < 12; i++)
            {
                float angle = (i / 12f) * MathHelper.TwoPi;
                level.Particles.Emit(ParticleTypes.Dust, player.Center + Calc.AngleToVector(angle, 16f), Color.HotPink, angle + MathHelper.Pi);
            }
        }

        private void PlayPuffSound()
        {
            // Pitch varies with jump count for variety
            float pitch = 1f + jumpCount * 0.08f;
            PlaySoundWithPitch(SFX_PUFF, player.Position, pitch);
        }

        private void PlaySoundWithPitch(string sfx, Vector2 position, float pitch)
        {
            try
            {
                var instance = Audio.Play(sfx, position);
                // Note: FMOD pitch control would go here if available
            }
            catch
            {
                // Ignore missing audio
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // NEW: Expression system
        // ═══════════════════════════════════════════════════════════════════════
        private void SetExpression(KirbyExpression expression, float duration)
        {
            currentExpression = expression;
            expressionTimer = duration;
            
            // This would trigger sprite changes if the sprite bank supports it
            // For now it's tracked for future animation integration
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Helper methods
        // ═══════════════════════════════════════════════════════════════════════
        private Color GetAbilityColor(CopyAbilityType ability)
        {
            return ability switch
            {
                CopyAbilityType.Fire => Color.OrangeRed,
                CopyAbilityType.Ice => Color.LightCyan,
                CopyAbilityType.Spark => Color.Yellow,
                CopyAbilityType.Beam => Color.Cyan,
                CopyAbilityType.Sword => Color.Silver,
                CopyAbilityType.Cutter => Color.LightGreen,
                CopyAbilityType.Stone => Color.Gray,
                CopyAbilityType.Bomb => Color.DarkGray,
                CopyAbilityType.Hammer => Color.Brown,
                CopyAbilityType.Wheel => Color.Red,
                CopyAbilityType.Fighter => Color.Orange,
                CopyAbilityType.Suplex => Color.Gold,
                CopyAbilityType.Ninja => Color.Purple,
                CopyAbilityType.Parasol => Color.LightPink,
                CopyAbilityType.Wing => Color.LightSkyBlue,
                CopyAbilityType.Mirror => Color.MediumPurple,
                CopyAbilityType.UFO => Color.LimeGreen,
                CopyAbilityType.Needle => Color.DarkGreen,
                CopyAbilityType.Sleep => Color.LavenderBlush,
                _ => Color.White
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Inner types
        // ═══════════════════════════════════════════════════════════════════════
        private class InhaleParticle
        {
            public Vector2 Position;
            public Vector2 Target;
            public float Life;
            public Color Color;
        }

        private enum KirbyExpression
        {
            Normal,
            Happy,
            Surprised,
            Puffed,
            Inhaling,
            Attacking,
            Hurt,
            Stone,
            Sleeping
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NEW: Kirby Projectile Entity
    // ═══════════════════════════════════════════════════════════════════════════
    public class KirbyProjectile : Entity
    {
        private Vector2 velocity;
        private Color color;
        private CopyAbilityType abilityType;
        private float damage;
        private float life = 2f;
        private float trailTimer;

        public KirbyProjectile(Vector2 position, Vector2 velocity, Color color, CopyAbilityType ability, float damage)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            this.abilityType = ability;
            this.damage = damage;
            
            Collider = new Circle(6f);
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();

            Position += velocity * Engine.DeltaTime;
            life -= Engine.DeltaTime;

            // Spawn trail particles
            trailTimer -= Engine.DeltaTime;
            if (trailTimer <= 0f && Scene is Level level)
            {
                trailTimer = 0.05f;
                level.Particles.Emit(ParticleTypes.Dust, Position, color * 0.5f, velocity.Angle() + MathHelper.Pi);
            }

            // Check for collision with enemies
            CheckEnemyCollision();

            // Check for wall collision
            if (CollideCheck<Solid>())
            {
                OnHitWall();
                return;
            }

            if (life <= 0f)
            {
                RemoveSelf();
            }
        }

        private void CheckEnemyCollision()
        {
            if (Scene == null) return;

            foreach (Entity entity in Scene.Entities)
            {
                if (entity is IKirbyCopySource && entity.Collider != null)
                {
                    if (CollideCheck(entity))
                    {
                        OnHitEnemy(entity);
                        return;
                    }
                }
            }
        }

        private void OnHitEnemy(Entity enemy)
        {
            if (Scene is Level level)
            {
                // Impact particles
                for (int i = 0; i < 8; i++)
                {
                    float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = Calc.AngleToVector(angle, Calc.Random.Range(60f, 120f));
                    level.Add(new HitParticle(Position, vel, color));
                }

                level.Shake(0.1f);
            }

            RemoveSelf();
        }

        private void OnHitWall()
        {
            if (Scene is Level level)
            {
                // Wall impact particles
                for (int i = 0; i < 4; i++)
                {
                    float angle = velocity.Angle() + MathHelper.Pi + Calc.Random.Range(-0.5f, 0.5f);
                    level.Particles.Emit(ParticleTypes.Dust, Position, color, angle);
                }
            }

            RemoveSelf();
        }

        public override void Render()
        {
            // Draw projectile as a glowing circle
            float pulse = 0.8f + (float)Math.Sin(Scene.TimeActive * 10f) * 0.2f;
            Draw.Circle(Position, 4f, color * pulse, 8);
            Draw.Circle(Position, 2f, Color.White * pulse, 6);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NEW: Kirby Bomb Entity
    // ═══════════════════════════════════════════════════════════════════════════
    public class KirbyBomb : Entity
    {
        private Vector2 velocity;
        private float fuseTimer = 1.5f;
        private bool hasExploded;

        public KirbyBomb(Vector2 position, Vector2 velocity) : base(position)
        {
            this.velocity = velocity;
            Collider = new Circle(5f);
            Depth = -50;
        }

        public override void Update()
        {
            base.Update();

            // Apply gravity
            velocity.Y += 400f * Engine.DeltaTime;

            // Move
            Position += velocity * Engine.DeltaTime;

            // Bounce off walls
            if (CollideCheck<Solid>())
            {
                velocity.X *= -0.5f;
                velocity.Y *= -0.3f;
            }

            // Count down fuse
            fuseTimer -= Engine.DeltaTime;
            if (fuseTimer <= 0f && !hasExploded)
            {
                Explode();
            }
        }

        private void Explode()
        {
            hasExploded = true;

            if (Scene is Level level)
            {
                // Explosion particles
                for (int i = 0; i < 20; i++)
                {
                    float angle = (i / 20f) * MathHelper.TwoPi;
                    Vector2 vel = Calc.AngleToVector(angle, Calc.Random.Range(80f, 160f));
                    level.Add(new HitParticle(Position, vel, Color.Orange));
                }

                // Additional fire particles
                for (int i = 0; i < 10; i++)
                {
                    float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                    level.Particles.Emit(ParticleTypes.Dust, Position + Calc.AngleToVector(angle, 8f), Color.OrangeRed, angle);
                }

                // Screen effects
                level.Shake(0.3f);
                level.Flash(Color.Orange * 0.2f, true);

                // Damage nearby enemies
                float explosionRadius = 32f;
                foreach (Entity entity in level.Entities)
                {
                    if (entity is IKirbyCopySource && Vector2.Distance(Position, entity.Center) < explosionRadius)
                    {
                        // Would apply damage here
                        for (int i = 0; i < 5; i++)
                        {
                            float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                            Vector2 vel = Calc.AngleToVector(angle, Calc.Random.Range(50f, 100f));
                            level.Add(new HitParticle(entity.Center, vel, Color.Orange));
                        }
                    }
                }
            }

            RemoveSelf();
        }

        public override void Render()
        {
            // Draw bomb with flashing fuse
            Color bombColor = Color.DarkGray;
            Color fuseColor = (int)(fuseTimer * 8) % 2 == 0 ? Color.Red : Color.Orange;

            Draw.Circle(Position, 5f, bombColor, 8);
            Draw.Circle(Position + new Vector2(0, -5f), 2f, fuseColor, 4);
        }
    }
}
