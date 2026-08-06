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

        #region State (delegates to K_Player's real Kirby StateMachine states)

        // NOTE: The original independent movement/flight/inhale simulation that
        // used to live in this component's Update() has been removed. It ran in
        // parallel with K_Player's own StateMachine states (StKirbyFloat,
        // StKirbyInhale) and, because this component was added to the entity
        // *after* the StateMachine, silently overwrote whatever the state
        // machine had just computed every frame — it never actually executed
        // (a hard `return` at the top of Update() short-circuited it). K_Player's
        // StateMachine states are the sole authoritative implementation of Kirby
        // physics; this component is now just a thin read-only facade plus the
        // inhale visual/collider helpers (MouthVoidCollider, InhaleParticleSystem).

        private K_Player KPlayer => player as K_Player;

        /// <summary>Is the host player currently floating (StKirbyFloat)?</summary>
        public bool IsFlying => KPlayer != null && KPlayer.StateMachine.State == K_Player.StKirbyFloat;

        /// <summary>Is the host player currently inhaling (StKirbyInhale)?</summary>
        public bool IsInhaling => KPlayer != null && KPlayer.StateMachine.State == K_Player.StKirbyInhale;

        public bool IsMouthOpen => IsInhaling;

        public bool CanInhale => KPlayer == null || KPlayer.StateMachine.State == K_Player.StNormal;

        // Module integration
        private static bool _hooksLoaded = false;

        // Visuals
        public Vector2 CenterOffset { get; private set; }

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

            // Note: hitbox is managed by K_Player's own StateMachine states
            // (normalHitbox / Kirby-specific colliders) and must not be
            // overridden here — doing so previously pinned the player to a
            // permanently tiny hitbox regardless of state.
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            level = scene as Level;

            // Create inhale effect components
            inhaleParticles = new InhaleParticleSystem(player);
            // Deferred: adding a component here would mutate the Components
            // list while Entity.Added is still enumerating it (crashes with
            // "Collection was modified; enumeration operation may not execute").
            scene.OnEndOfFrame += () => player.Add(inhaleParticles);
        }

        public override void EntityRemoved(Scene scene)
        {
            if (inhaleParticles != null)
            {
                // Deferred: removing a component here would mutate the Components
                // list while Entity.Removed is still enumerating it (crashes with
                // "Collection was modified; enumeration operation may not execute").
                var particles = inhaleParticles;
                inhaleParticles = null;
                scene.OnEndOfFrame += () => player?.Remove(particles);
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
            // All Kirby movement/flight/inhale physics live in K_Player's own
            // StateMachine states (StKirbyFloat, StKirbyInhale, etc.) — see the
            // note in the State region above. This component intentionally does
            // no per-frame physics work of its own.
        }

        public override void Render()
        {
            base.Render();
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
