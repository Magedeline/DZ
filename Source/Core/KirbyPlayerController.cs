using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.DZ;
using Celeste.Mod;

namespace Celeste.Entities
{
    /// <summary>
    /// Controller component that handles Kirby-specific mechanics for K_Player or vanilla Player.
    /// Implements flying (multi-flap hover), inhaling, and glomping mechanics
    /// based on the Lua player code.
    /// </summary>
    public class KirbyPlayerController : Component
    {
        // Reference to the parent player (either K_Player or vanilla Player)
        private Entity player;
        private Level level;

        #region Player abstraction helpers

        /// <summary>Is the host player dead / in a death state?</summary>
        private bool PlayerDead =>
            player is K_Player kp ? kp.Dead :
            player is global::Celeste.Player vp ? vp.Dead :
            false;

        /// <summary>Horizontal speed of the host player.</summary>
        private float PlayerSpeedX
        {
            get => player is K_Player kp2 ? kp2.Speed.X :
                   player is global::Celeste.Player vp2 ? vp2.Speed.X : 0f;
            set {
                if (player is K_Player kp3) kp3.Speed.X = value;
                else if (player is global::Celeste.Player vp3) vp3.Speed.X = value;
            }
        }

        /// <summary>Vertical speed of the host player.</summary>
        private float PlayerSpeedY
        {
            get => player is K_Player kp4 ? kp4.Speed.Y :
                   player is global::Celeste.Player vp4 ? vp4.Speed.Y : 0f;
            set {
                if (player is K_Player kp5) kp5.Speed.Y = value;
                else if (player is global::Celeste.Player vp5) vp5.Speed.Y = value;
            }
        }

        /// <summary>Facing direction of the host player.</summary>
        private Facings PlayerFacing
        {
            get => player is K_Player kp6 ? kp6.Facing :
                   player is global::Celeste.Player vp6 ? vp6.Facing :
                   Facings.Right;
            set {
                if (player is K_Player kp7) kp7.Facing = value;
                else if (player is global::Celeste.Player vp7) vp7.Facing = value;
            }
        }

        /// <summary>Whether the host player is currently on the ground.</summary>
        private bool PlayerOnGround() =>
            player is Actor actor ? actor.OnGround() : false;

        #endregion

        #region Constants (matching Lua values converted to Celeste units)

        // Physics constants - Lua values were in pixels/frame, converted to Celeste units
        private const float MaxRun = 90f;           // ~1 pixel/frame equivalent
        private const float RunAccel = 1000f;         // Ground acceleration
        private const float AirAccel = 400f;          // Air acceleration (0.4 in Lua)
        private const float RunReduce = 400f;         // Deceleration

        // Flying constants
        private const float FlyGravity = 45f;       // 0.15 in Lua -> converted
        private const float FlyMaxFall = 50f;         // 0.5 in Lua -> converted
        private const float FlyGravityLow = 15f;      // Reduced gravity at low speed
        private const float FlyGravityMid = 25f;      // Reduced gravity at mid speed
        private const float NormalGravity = 900f;     // Standard Celeste gravity
        private const float NormalMaxFall = 160f;     // Standard max fall
        private const float HalfGravThreshold = 40f;  // Speed threshold for half gravity

        // Flapping constants
        private const float BaseFlapSpeed = -65f;     // -0.8 in Lua -> converted
        private const float FlapMultMax = 50f;
        private const float FlapMultIncrement = 0.75f;
        private const float FlapMultDecay = 0.9f;
        private const int FlapRepeatFrames = 9;       // Frames between auto-flaps
        private const float FlapRepeatFrameTime = FlapRepeatFrames / 60f; // ~0.15s

        // Timers (converted from frames to seconds)
        private const float GraceTime = 0.1f;         // 6 frames @ 60fps
        private const float JumpBufferTime = 0.067f;  // 4 frames @ 60fps
        private const float FlyBufferTime = 0.017f;   // 1 frame
        private const float MouthOpenTime = 0.1f;     // 6 frames
        private const float InhaleTime = 0.067f;      // 4 frames
        private const float LandingTime = 0.05f;      // 3 frames

        // Hitbox sizes
        private const int HitboxX = 1;
        private const int HitboxY = 3;
        private const int HitboxW = 6;
        private const int HitboxH = 5;

        #endregion

        #region State Variables (matching Lua player state)

        // Input state
        private bool pJump;
        private bool pFly;
        private bool wasFlying;

        // Timers
        private float graceTimer;
        private float jumpBuffer;
        private float flyBuffer = 1;
        private float flapTimer;
        private float flapRepeatTimer;
        private int flapAnimTimer;
        private float flapMult;
        private float mouthOpenTimer;
        private float landingTimer;

        // Inhale state
        public bool CanInhale { get; private set; } = true;
        public bool IsInhaling { get; private set; }
        private float inhaleTimer;
        private float glompTimer;
        private bool wasGlomping;
        private FMOD.Studio.EventInstance inhaleSfx;

        // Jump state
        private int djump;
        private int maxDjump = 1;

        // Module integration
        private static bool _hooksLoaded = false;

        // Collision state
        private bool wasOnGround;
        private bool onIce;

        // Visuals
        public Vector2 CenterOffset { get; private set; }
        private float sprOff;

        #endregion

        #region Public Properties

        public bool IsFlying => pFly;
        public bool IsMouthOpen => mouthOpenTimer > 0 || IsInhaling;
        public float FlapMultiplier => flapMult;
        public int CurrentFlapCount => (int)(flapMult / FlapMultMax * 5); // Normalized for UI
        public bool IsLanding => landingTimer > 0;
        public bool IsGlomping => glompTimer > 0;

        #endregion

        #region Components

        // Inhale particles/effects
        private MouthVoidCollider mouthVoid;
        private InhaleParticleSystem inhaleParticles;

        #endregion

        public static void Load()
        {
            if (_hooksLoaded)
                return;

            Logger.Log(LogLevel.Info, "DZ", "[KirbyPlayerController] Loaded");
            _hooksLoaded = true;
        }

        public static void Unload()
        {
            if (!_hooksLoaded)
                return;

            Logger.Log(LogLevel.Info, "DZ", "[KirbyPlayerController] Unloaded");
            _hooksLoaded = false;
        }

        public KirbyPlayerController()
            : base(active: true, visible: true)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (entity is K_Player || entity is global::Celeste.Player)
            {
                player = entity;
            }
            else
            {
                throw new InvalidOperationException("KirbyPlayerController must be added to a K_Player or Player entity");
            }

            // Set up hitbox to match Lua player
            player.Collider = new Hitbox(HitboxW, HitboxH, HitboxX, HitboxY);
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            level = scene as Level;

            // Apply max float jumps from settings
            SyncFromSettings();

            // Restore flying/inhaling state from session if Kirby mode was active
            SyncFromSession();

            // Create inhale effect components
            inhaleParticles = new InhaleParticleSystem(player);
            // Deferred: adding a component here would mutate the Components
            // list while Entity.Added is still enumerating it (crashes with
            // "Collection was modified; enumeration operation may not execute").
            scene.OnEndOfFrame += () => player.Add(inhaleParticles);
        }

        private void SyncFromSettings()
        {
            var settings = DZModule.Settings;
            if (settings != null)
            {
                maxDjump = Math.Max(1, settings.KirbyMaxFloatJumps);
                djump = maxDjump;
            }
        }

        private void SyncFromSession()
        {
            var session = DZModule.Session;
            if (session == null)
                return;

            // If session says Kirby mode is active, ensure we start ready
            if (session.IsKirbyModeActive)
            {
                CanInhale = true;
                djump = maxDjump;
            }
        }

        private void WriteToSession()
        {
            var session = DZModule.Session;
            if (session == null)
                return;

            session.KirbyStamina = flapMult;
        }

        public override void EntityRemoved(Scene scene)
        {
            if (inhaleParticles != null)
            {
                player.Remove(inhaleParticles);
                inhaleParticles = null;
            }
            if (mouthVoid != null)
            {
                mouthVoid.RemoveSelf();
                mouthVoid = null;
            }
            base.EntityRemoved(scene);
        }

        public override void Update()
        {
            base.Update();

            if (player == null || PlayerDead) return;

            // Respect the KirbyPlayerEnabled setting
            var settings = DZModule.Settings;
            if (settings != null && !settings.KirbyPlayerEnabled)
            {
                // Controller disabled by settings — ensure clean state
                if (pFly || IsInhaling)
                    Reset();
                return;
            }

            // Update center position (Kirby is 8x8, center at 4,4)
            CenterOffset = new Vector2(4, 4);

            // Handle input (blocking when inhaling)
            int inputX = 0;
            if (IsInhaling)
            {
                inputX = 0;
            }
            else
            {
                if (Input.MoveX.Value > 0) inputX = 1;
                else if (Input.MoveX.Value < 0) inputX = -1;
            }

            // Check ground and ice state
            bool onGround = PlayerOnGround();
            onIce = CheckOnIce();

            // Landing effects
            if (onGround && !wasOnGround)
            {
                if (!IsInhaling)
                {
                    Audio.Play("event:/game/general/assist_dreamblockbounce", player.Position);
                }
                landingTimer = LandingTime;
                HitGroundStar();
            }

            if (landingTimer > 0)
                landingTimer -= Engine.DeltaTime;

            // Update center for inhale calculations
            Vector2 center = player.Position + CenterOffset;

            // Check breath input (Grab button)
            bool breath = Input.Grab.Check;
            bool btnUp = Input.MoveY.Value < 0;
            bool btnDown = Input.MoveY.Value > 0;
            bool jump;

            // Enter flying mode when pressing up (not inhaling and mouth not open)
            if (btnUp && !IsInhaling && !(mouthOpenTimer > 0))
            {
                pFly = true;
                onGround = false;
                flyBuffer = 0;
                graceTimer = 0;
                jumpBuffer = JumpBufferTime;
            }

            // Handle jump input differently when flying
            if (pFly)
            {
                jump = Input.Jump.Check;
            }
            else
            {
                jump = Input.Jump.Pressed;
            }

            pJump = Input.Jump.Check;

            // Start jump/fly buffer
            if (jump && !IsInhaling && !(mouthOpenTimer > 0))
            {
                jumpBuffer = JumpBufferTime;
                if (flyBuffer < 1)
                {
                    pFly = true;
                }
            }
            else if (jumpBuffer > 0)
            {
                jumpBuffer -= Engine.DeltaTime;
            }

            // Flying mechanics
            if (pFly)
            {
                wasFlying = true;
                CanInhale = false;

                if (jump || btnUp)
                {
                    flapTimer += Engine.DeltaTime;
                    if (flapMult <= FlapMultMax)
                    {
                        flapMult += FlapMultIncrement;
                    }
                    // Count down toward the next allowed auto-flap
                    if (flapRepeatTimer > 0)
                    {
                        flapRepeatTimer -= Engine.DeltaTime;
                        if (flapRepeatTimer < 0)
                            flapRepeatTimer = 0;
                    }
                }
                else
                {
                    flapTimer = 0;
                    flapRepeatTimer = 0;
                    if (flapMult > 0)
                    {
                        flapMult *= FlapMultDecay;
                    }
                    else if (flapMult < 0)
                    {
                        flapMult = 0;
                    }
                }

                // Cancel flying with breath or down
                if (breath || btnDown)
                {
                    pFly = false;
                }
            }
            else
            {
                flapTimer = 0;
                flapMult = 0;
                flapRepeatTimer = 0;
            }

            // Exit flying state
            if (!pFly && wasFlying)
            {
                wasFlying = false;
                CreatePuffEffect();
                Audio.Play("event:/game/general/assist_dreamblockbounce", player.Position);
                mouthOpenTimer = MouthOpenTime;
            }

            if (mouthOpenTimer > 0)
                mouthOpenTimer -= Engine.DeltaTime;

            // Ground state handling
            if (onGround && !(glompTimer > 0))
            {
                graceTimer = GraceTime;
                pFly = false;
                flyBuffer = 1;
                CanInhale = true;
                if (djump < maxDjump)
                {
                    Audio.Play("event:/game/general/assist_dreamblockbounce", player.Position);
                    djump = maxDjump;
                }
            }
            else if (!pFly && graceTimer > 0)
            {
                graceTimer -= Engine.DeltaTime;
            }
            else if (flyBuffer > 0)
            {
                flyBuffer -= Engine.DeltaTime;
            }

            // Inhaling mechanics
            if (breath && CanInhale && !wasGlomping && !(mouthOpenTimer > 0))
            {
                if (!IsInhaling)
                {
                    IsInhaling = true;
                    inhaleParticles?.StartInhaling();
                    inhaleSfx = Audio.Play("event:/game/general/assist_dreamblockbounce", player.Position);
                    CreateMouthVoid();
                }
                graceTimer = 0;
                inhaleTimer = InhaleTime;
            }
            else if (IsInhaling && !breath)
            {
                inhaleTimer -= Engine.DeltaTime;
                if (inhaleTimer <= 0)
                {
                    StopInhaling();
                }
            }
            else
            {
                inhaleTimer = 0;
                IsInhaling = false;
            }

            // Update inhale particles
            if (IsInhaling)
            {
                inhaleParticles?.UpdateInhale(PlayerFacing == Facings.Right ? 1 : -1);
                UpdateMouthVoid();
            }

            // Glomping state
            if (!breath && wasGlomping)
            {
                wasGlomping = false;
            }

            if (glompTimer > 0)
            {
                glompTimer -= Engine.DeltaTime;
                flyBuffer = glompTimer;
                CanInhale = false;
                wasGlomping = true;
            }
            else
            {
                CanInhale = true;
            }

            // Movement physics
            UpdateMovement(inputX, onGround);

            // Jump handling
            HandleJump(onGround);

            // Animation
            UpdateAnimation(onGround, inputX);

            // Store state for next frame
            wasOnGround = onGround;
            sprOff += Engine.DeltaTime;
            if (flapAnimTimer > 0) flapAnimTimer--;

            // Sync runtime state back to session
            WriteToSession();
        }

        private void UpdateMovement(int inputX, bool onGround)
        {
            float maxRun = MaxRun;
            float accel = RunAccel * Engine.DeltaTime;
            float deccel = RunReduce * Engine.DeltaTime;

            if (!onGround)
            {
                accel = AirAccel * Engine.DeltaTime;
            }
            else if (onIce)
            {
                accel = 50f * Engine.DeltaTime; // Very low ice acceleration
            }

            // Apply acceleration/deceleration
            if (Math.Abs(PlayerSpeedX) > maxRun)
            {
                PlayerSpeedX = Calc.Approach(PlayerSpeedX, Math.Sign(PlayerSpeedX) * maxRun, deccel);
            }
            else
            {
                PlayerSpeedX = Calc.Approach(PlayerSpeedX, inputX * maxRun, accel);
            }

            // Update facing
            if (PlayerSpeedX != 0)
            {
                PlayerFacing = (Facings)Math.Sign(PlayerSpeedX);
            }

            // Apply gravity based on state
            float maxFall;
            float gravity;

            if (pFly)
            {
                gravity = FlyGravity;
                maxFall = FlyMaxFall;

                // Reduced gravity at low vertical speeds for better control
                if (Math.Abs(PlayerSpeedY) <= 30f)
                {
                    if (Math.Abs(PlayerSpeedY) <= 15f)
                    {
                        gravity = FlyGravityLow;
                    }
                    else
                    {
                        gravity = FlyGravityMid;
                    }
                }
            }
            else if (mouthOpenTimer > 0 || IsInhaling)
            {
                gravity = FlyGravity;
                maxFall = 0; // Float when mouth is open
            }
            else
            {
                gravity = NormalGravity;
                maxFall = NormalMaxFall;

                // Half gravity at very low speeds (Celeste-style)
                if (Math.Abs(PlayerSpeedY) < HalfGravThreshold)
                {
                    gravity *= 0.35f;
                }
            }

            if (!onGround)
            {
                PlayerSpeedY = Calc.Approach(PlayerSpeedY, maxFall, gravity * Engine.DeltaTime);
            }
        }

        private void HandleJump(bool onGround)
        {
            if (jumpBuffer > 0)
            {
                if (graceTimer > 0)
                {
                    // Normal jump
                    Audio.Play("event:/char/madeline/jump", player.Position);
                    jumpBuffer = 0;
                    graceTimer = 0;
                    CreateJumpCloud();
                    PlayerSpeedY = -105f; // Standard Celeste jump speed
                }
                else if (pFly && flapRepeatTimer <= 0)
                {
                    // Flying flap
                    Audio.Play("event:/char/madeline/jump", player.Position);
                    CreateSmallSmoke();
                    float flapStrength = BaseFlapSpeed - (0.8f * flapMult);
                    PlayerSpeedY = flapStrength;
                    flapAnimTimer = 6;
                    // Schedule the next auto-flap
                    flapRepeatTimer = FlapRepeatFrameTime;
                }
            }
        }

        private void UpdateAnimation(bool onGround, int inputX)
        {
            // Animation is handled by the main player sprite system.
            // This controller just provides state for the animator.
            // Both K_Player and vanilla Player will use these states to select animations.
        }

        private bool CheckOnIce()
        {
            if (player == null || player.Scene == null) return false;

            // Check if standing on an ice platform
            var platform = player.CollideFirst<Solid>(player.Position + Vector2.UnitY);
            if (platform != null)
            {
                // GetStepSoundIndex requires a vanilla Player; only call it when supported
                if (player is global::Celeste.Player vanillaPlayer)
                    return platform.GetStepSoundIndex(vanillaPlayer) == 7;
            }
            return false;
        }

        private void StopInhaling()
        {
            IsInhaling = false;
            Audio.Stop(inhaleSfx);
            inhaleSfx = default;

            if (mouthVoid != null)
            {
                mouthVoid.RemoveSelf();
                mouthVoid = null;
            }
        }

        private void CreateMouthVoid()
        {
            if (mouthVoid != null)
            {
                mouthVoid.RemoveSelf();
            }
            mouthVoid = new MouthVoidCollider(player, PlayerFacing == Facings.Right ? 1 : -1);
            player.Scene.Add(mouthVoid);
        }

        private void UpdateMouthVoid()
        {
            if (mouthVoid != null)
            {
                mouthVoid.UpdatePosition();
            }
        }

        private void CreatePuffEffect()
        {
            level?.Particles.Emit(ParticleTypes.SparkyDust, 4, player.Center, Vector2.One * 8f);
        }

        private void CreateJumpCloud()
        {
            level?.Particles.Emit(ParticleTypes.Dust, 1, player.BottomCenter, Vector2.One * 4f);
        }

        private void CreateSmallSmoke()
        {
            level?.Particles.Emit(ParticleTypes.SparkyDust, 2, player.BottomCenter, Vector2.One * 4f);
        }

        private void HitGroundStar()
        {
            // Spawn landing star effect
            level?.Particles.Emit(ParticleTypes.SparkyDust, 6, player.BottomCenter, Vector2.One * 6f);
        }

        public override void Render()
        {
            base.Render();
            // Additional rendering if needed
        }

        /// <summary>
        /// Called when player inhales an object
        /// </summary>
        public void OnInhaleObject()
        {
            glompTimer = 0.1f; // Brief glomp after inhaling
            wasGlomping = true;
        }

        /// <summary>
        /// Force stop all special states
        /// </summary>
        public void Reset()
        {
            pFly = false;
            IsInhaling = false;
            flapMult = 0;
            flapTimer = 0;
            graceTimer = 0;
            CanInhale = true;

            Audio.Stop(inhaleSfx);
            inhaleSfx = default;

            if (mouthVoid != null)
            {
                mouthVoid.RemoveSelf();
                mouthVoid = null;
            }
        }
    }

    #region Helper Classes

    /// <summary>
    /// Mouth void collider for inhaling - acts as a vacuum zone in front of Kirby.
    /// Accepts either a K_Player or a vanilla Player entity as the owner.
    /// </summary>
    public class MouthVoidCollider : Entity
    {
        private Entity player;
        private int facingDir;
        private Vector2 offset;
        private Hitbox hitbox;
        private readonly System.Collections.Generic.HashSet<Entity> consumed = new System.Collections.Generic.HashSet<Entity>();

        // Resolve facing from whichever player type we have
        private Facings PlayerFacing =>
            player is K_Player kp ? kp.Facing :
            player is global::Celeste.Player vp ? vp.Facing :
            Facings.Right;

        public MouthVoidCollider(Entity player, int facingDir)
            : base(player.Position)
        {
            this.player = player;
            this.facingDir = facingDir;
            this.offset = new Vector2(10 * facingDir, -2);

            // Mouth hitbox: x=0, y=-2, w=10, h=12 (from Lua)
            hitbox = new Hitbox(10, 12, 0, -2);
            Collider = hitbox;

            Collidable = true;
            Visible = false;
        }

        public override void Update()
        {
            base.Update();
            UpdatePosition();

            // Check for inhaleable objects using components
            foreach (Entity entity in Scene.Entities)
            {
                if (consumed.Contains(entity))
                    continue;

                var inhaleable = entity.Get<InhaleableComponent>();
                if (inhaleable != null && CollideCheck(entity))
                {
                    consumed.Add(entity);
                    inhaleable.OnInhaled(player);
                }
            }

            // Remove if player stops inhaling
            if (!(player.Get<KirbyPlayerController>()?.IsInhaling ?? false))
            {
                RemoveSelf();
            }
        }

        public void UpdatePosition()
        {
            if (player != null)
            {
                Position = player.Position + offset;
                facingDir = PlayerFacing == Facings.Right ? 1 : -1;
                offset.X = 10 * facingDir;

                // Adjust for facing
                if (PlayerFacing == Facings.Left)
                {
                    hitbox.Position = new Vector2(-10, -2);
                }
                else
                {
                    hitbox.Position = new Vector2(0, -2);
                }
            }
        }
    }

    /// <summary>
    /// Particle system for inhale effect.
    /// Accepts either a K_Player or a vanilla Player entity as the owner.
    /// </summary>
    public class InhaleParticleSystem : Component
    {
        private Entity player;
        private Particle[] particles;
        private bool isInhaling;

        private struct Particle
        {
            public Vector2 Position;
        }

        public InhaleParticleSystem(Entity player)
            : base(active: true, visible: true)
        {
            this.player = player;
            this.particles = new Particle[5];
        }

        public void StartInhaling()
        {
            isInhaling = true;
            // Initialize particles
            for (int i = 0; i < particles.Length; i++)
            {
                ResetParticle(ref particles[i], 1);
            }
        }

        public void StopInhaling()
        {
            isInhaling = false;
        }

        public void UpdateInhale(int facingDir)
        {
            if (!isInhaling) return;

            Vector2 center = player.Position + new Vector2(4, 4);

            for (int i = 0; i < particles.Length; i++)
            {
                float dist = Vector2.Distance(particles[i].Position, center);

                if (dist <= 2 || dist >= 18)
                {
                    ResetParticle(ref particles[i], facingDir);
                }

                // Move toward center
                Vector2 dir = (center - particles[i].Position).SafeNormalize();
                particles[i].Position += dir * 40f * Engine.DeltaTime;
            }
        }

        private void ResetParticle(ref Particle p, int facingDir)
        {
            Vector2 center = player.Position + new Vector2(4, 4);
            float distance = 10 + Calc.Random.Range(0f, 5f);
            float angle = Calc.Random.Range(-0.5f, 0.5f) * (float)Math.PI;

            p.Position = center + new Vector2(
                (float)Math.Cos(angle) * distance * facingDir,
                (float)Math.Sin(angle) * distance
            );
        }

        public override void Render()
        {
            if (!isInhaling) return;

            foreach (var p in particles)
            {
                Draw.Pixel.Draw(p.Position, Vector2.Zero, Color.White);
            }
        }
    }

    #endregion

    #region Inhaleable Component

    /// <summary>
    /// Component that can be added to entities to make them inhaleable by Kirby.
    /// When the entity's collider overlaps with Kirby's mouth void, OnInhaled is called.
    /// </summary>
    public class InhaleableComponent : Component
    {
        public InhaleableComponent()
            : base(active: true, visible: false)
        {
        }

        /// <summary>
        /// Called when Kirby inhales this entity.
        /// The <paramref name="player"/> argument is either a <see cref="K_Player"/> or a
        /// vanilla <see cref="global::Celeste.Player"/>.
        /// Override to implement custom behavior (e.g., being swallowed, dropping loot, etc.)
        /// </summary>
        public virtual void OnInhaled(Entity player)
        {
            // Default behavior: remove the entity
            Entity?.RemoveSelf();
        }
    }

    #endregion
}
