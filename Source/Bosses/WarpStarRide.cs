#nullable enable
using System;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //  WARP STAR RIDE  —  the flying-Popstar player controller
    //
    //  Two entities, always spawned as a pair by KirbyFinalBattleScene:
    //
    //    WarpStarRideController  — owns player control while riding.
    //    RidingWarpStar          — the visible star drawn under the rider.
    //
    //  ── Why the rider is resolved instead of passed in ────────────────────────
    //  When Kirby is the active character the scene contains BOTH a K_Player
    //  (the real, input-driven Actor) and a hidden vanilla Player "shadow" that
    //  exists purely so vanilla APIs keep working.  K_PlayerHooks suppresses the
    //  shadow's Update/Render and K_Player copies its own Position onto the
    //  shadow every frame — so the shadow *reads* correctly but any write to it
    //  is discarded.  Anything that drives the player must therefore resolve
    //  K_Player FIRST and only fall back to Player.  That is what
    //  WarpStarRideController.ResolveRider does, and every consumer here uses it.
    //
    //  ── Movement model ───────────────────────────────────────────────────────
    //  Purely velocity based, in the same shape as vanilla Celeste physics:
    //
    //      input → target velocity → Calc.Approach → Actor.MoveH / MoveV
    //
    //  Nothing writes Actor.Position directly.  That keeps Celeste's sub-pixel
    //  movementCounter intact (no drift, no jitter at low speed) and keeps the
    //  rider solid against walls instead of phasing through them.  The idle bob
    //  is folded in as a velocity term (d/dt of the sine) rather than a position
    //  offset, so it can never accumulate error against the arena clamp.
    //
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Full player controller for the Warp Star flight sections
    /// (BattlePhase.WarpStarRide + BattlePhase.FlyingVoid).
    ///
    /// Freezes the rider's own physics (StDummy with gravity/friction/maxspeed
    /// all disabled), accepts analog directional input to fly freely inside a
    /// bounded arena anchored at the mount point, layers a floaty idle bob on
    /// top, emits the golden warp trail, and restores normal control the moment
    /// it is removed — including when the level ends underneath it.
    ///
    /// The arena clamp is not cosmetic: it is what stops the rider drifting far
    /// enough to trigger level-chunk streaming, which is the root cause of the
    /// "Collection was modified" EntityList crash this sequence used to throw.
    /// </summary>
    [Tracked]
    [HotReloadable]
    public class WarpStarRideController : Entity
    {
        // ── Tuning ────────────────────────────────────────────────────────────
        /// <summary>Top flight speed in px/s.</summary>
        public float MoveSpeed = 120f;
        /// <summary>How fast the star reaches MoveSpeed, px/s². Lower = floatier.</summary>
        public float Acceleration = 900f;
        /// <summary>How fast the star coasts to a stop with no input, px/s².</summary>
        public float Friction = 1100f;
        /// <summary>Pixels of vertical sine-bob layered on top of input.</summary>
        public float BobAmplitude = 3f;
        /// <summary>Bob frequency in radians/second.</summary>
        public float BobSpeed = 2.5f;
        /// <summary>Half-size of the flight arena, measured from the mount point.</summary>
        public float ArenaHalfWidth = 130f;
        public float ArenaHalfHeight = 70f;

        /// <summary>
        /// Pins the camera to wherever it was when the ride started. Off by
        /// default; turn it on for a fixed-screen shmup feel where the arena
        /// bounds line up exactly with what the player can see.
        /// </summary>
        public bool LockCamera;

        /// <summary>
        /// Seconds to keep waiting for a rider to reappear (character swap,
        /// respawn) before the controller gives up and removes itself.
        /// </summary>
        public float LostRiderGrace = 3f;

        // ── Beam extension point ─────────────────────────────────────────────
        // Reserved for the final phase's player beam. Disabled by default, so
        // leaving it alone changes nothing. When enabled, holding Dash charges
        // and releasing fires: OnBeamRelease(origin, charge01). The controller
        // deliberately owns no beam visuals — the caller decides what spawns.
        public bool  BeamEnabled;
        public float BeamChargeTime = 1f;
        public float BeamCharge { get; private set; }
        public event Action<Vector2, float>? OnBeamRelease;

        // ── Runtime ──────────────────────────────────────────────────────────
        private readonly Entity? riderHint;

        private Level     level = null!;
        private Actor?    rider;
        private Player?   vanillaRider;
        private K_Player? kirbyRider;

        private Vector2 velocity;
        private Vector2 arenaCenter;
        private Vector2 lockedCameraPos;
        private string  startRoom = "";
        private float   bobTimer;
        private float   trailTimer;
        private float   lostRiderTimer;
        private bool    manualAnim;
        private bool    restored;

        private const string RIDE_ANIM   = "starFly";
        private const float  TRAIL_PERIOD = 0.08f;

        /// <summary>Current flight velocity in px/s (bob excluded).</summary>
        public Vector2 Velocity => velocity;
        /// <summary>The Actor currently being driven — K_Player or vanilla Player.</summary>
        public Actor? Rider => rider;
        /// <summary>World-space centre of the flight arena.</summary>
        public Vector2 ArenaCenter => arenaCenter;

        public WarpStarRideController(Entity? riderHint, float moveSpeed, float bobAmplitude,
            float bobSpeed, float arenaHalfWidth = 130f, float arenaHalfHeight = 70f)
            : base(riderHint?.Position ?? Vector2.Zero)
        {
            this.riderHint  = riderHint;
            MoveSpeed       = moveSpeed;
            BobAmplitude    = bobAmplitude;
            BobSpeed        = bobSpeed;
            ArenaHalfWidth  = arenaHalfWidth;
            ArenaHalfHeight = arenaHalfHeight;
            Tag = Tags.Persistent;
        }

        /// <summary>
        /// Returns the Actor that actually responds to input: K_Player when Kirby
        /// is active, otherwise the vanilla Player. Never returns the hidden
        /// K_Player shadow while a K_Player exists.
        /// </summary>
        public static Actor? ResolveRider(Scene scene)
        {
            K_Player? kirby = scene.Tracker.GetEntity<K_Player>();
            if (kirby != null) return kirby;
            return scene.Tracker.GetEntity<Player>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level           = SceneAs<Level>();
            startRoom       = level.Session.Level;
            lockedCameraPos = level.Camera.Position;

            Actor? found = ResolveRider(scene) ?? riderHint as Actor;
            if (found != null) Mount(found);
            else               arenaCenter = Position;
        }

        // ── Mount / dismount ─────────────────────────────────────────────────

        /// <summary>
        /// Takes control of <paramref name="actor"/> and re-anchors the arena on
        /// wherever it currently is, so a mid-ride character swap or respawn does
        /// not yank the player across the room.
        /// </summary>
        private void Mount(Actor actor)
        {
            rider          = actor;
            vanillaRider   = actor as Player;
            kirbyRider     = actor as K_Player;
            arenaCenter    = actor.Center;
            velocity       = Vector2.Zero;
            lostRiderTimer = 0f;
            restored       = false;

            PlayerSprite? sprite = null;

            if (vanillaRider != null)
            {
                vanillaRider.StateMachine.State = Player.StDummy;
                // DummyBegin() re-enables all of these, so they must be cleared
                // AFTER the state assignment. Leaving DummyGravity on is what
                // made the vanilla-Player ride sink out of the arena whenever
                // the stick was released.
                vanillaRider.DummyGravity       = false;
                vanillaRider.DummyFriction      = false;
                vanillaRider.DummyMaxspeed      = false;
                vanillaRider.EnforceLevelBounds = false;
                vanillaRider.Speed              = Vector2.Zero;
                sprite = vanillaRider.Sprite;
            }
            else if (kirbyRider != null)
            {
                kirbyRider.StateMachine.State = K_Player.StDummy;
                kirbyRider.DummyGravity       = false;
                kirbyRider.DummyFriction      = false;
                kirbyRider.DummyMaxspeed      = false;
                kirbyRider.Speed              = Vector2.Zero;
                sprite = kirbyRider.Sprite;
            }

            // DummyAutoAnimate picks an animation from Speed, and Speed is
            // pinned at zero here — so it would lock the rider into "idle" for
            // the whole flight. Drive the flight pose manually when the sprite
            // bank has one, and only fall back to auto-animate when it doesn't.
            manualAnim = sprite != null && sprite.Has(RIDE_ANIM);
            SetAutoAnimate(!manualAnim);
            if (manualAnim) sprite!.Play(RIDE_ANIM);
        }

        private void SetAutoAnimate(bool value)
        {
            if (vanillaRider != null) vanillaRider.DummyAutoAnimate = value;
            if (kirbyRider   != null) kirbyRider.DummyAutoAnimate   = value;
        }

        private bool RiderIsDead =>
            (vanillaRider?.Dead ?? false) || (kirbyRider?.Dead ?? false);

        // ── Update ───────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            // The return teleport swaps rooms out from under us; the ride is over.
            if (level.Session.Level != startRoom)
            {
                RemoveSelf();
                return;
            }

            // Re-acquire across character swaps / respawns instead of dying with
            // the old Actor reference.
            if (rider == null || rider.Scene == null)
            {
                Actor? found = ResolveRider(Scene);
                if (found != null)
                {
                    Mount(found);
                }
                else
                {
                    lostRiderTimer += Engine.DeltaTime;
                    if (lostRiderTimer >= LostRiderGrace) RemoveSelf();
                    return;
                }
            }

            // Don't fight the engine while it owns the frame.
            if (level.FrozenOrPaused || level.Transitioning || level.RetryPlayerCorpse != null)
                return;

            if (RiderIsDead) return;

            float dt = Engine.DeltaTime;

            // The rider's own physics must contribute nothing — we own movement.
            if (vanillaRider != null) vanillaRider.Speed = Vector2.Zero;
            if (kirbyRider   != null) kirbyRider.Speed   = Vector2.Zero;

            // ── Input ────────────────────────────────────────────────────────
            // Input.Aim covers stick + d-pad + keyboard. Normalising anything
            // past unit length is what stops keyboard diagonals being 1.41×
            // faster than the cardinals.
            Vector2 aim = Input.Aim.Value;
            if (aim.LengthSquared() > 1f) aim = aim.SafeNormalize();

            Vector2 target = aim * MoveSpeed;
            velocity.X = Calc.Approach(velocity.X, target.X,
                (aim.X != 0f ? Acceleration : Friction) * dt);
            velocity.Y = Calc.Approach(velocity.Y, target.Y,
                (aim.Y != 0f ? Acceleration : Friction) * dt);

            if (aim.X != 0f)
            {
                Facings facing = aim.X > 0f ? Facings.Right : Facings.Left;
                if (vanillaRider != null)    vanillaRider.Facing = facing;
                else if (kirbyRider != null) kirbyRider.Facing   = facing;
            }

            // ── Idle bob, as a velocity term ─────────────────────────────────
            float bobVelocity = 0f;
            if (BobAmplitude > 0f && BobSpeed > 0f)
            {
                bobTimer += dt * BobSpeed;
                if (bobTimer > MathHelper.TwoPi) bobTimer -= MathHelper.TwoPi;
                bobVelocity = (float)Math.Cos(bobTimer) * BobAmplitude * BobSpeed;
            }

            Vector2 move = new Vector2(velocity.X, velocity.Y + bobVelocity) * dt;

            // ── Arena clamp ──────────────────────────────────────────────────
            // Trim the frame's movement instead of correcting position after the
            // fact, and kill only the outward velocity component so the rider can
            // still slide along the boundary.
            Vector2 centre = rider!.Center;
            float minX = arenaCenter.X - ArenaHalfWidth;
            float maxX = arenaCenter.X + ArenaHalfWidth;
            float minY = arenaCenter.Y - ArenaHalfHeight;
            float maxY = arenaCenter.Y + ArenaHalfHeight;

            if (centre.X + move.X < minX)
            {
                move.X = minX - centre.X;
                if (velocity.X < 0f) velocity.X = 0f;
            }
            else if (centre.X + move.X > maxX)
            {
                move.X = maxX - centre.X;
                if (velocity.X > 0f) velocity.X = 0f;
            }

            if (centre.Y + move.Y < minY)
            {
                move.Y = minY - centre.Y;
                if (velocity.Y < 0f) velocity.Y = 0f;
            }
            else if (centre.Y + move.Y > maxY)
            {
                move.Y = maxY - centre.Y;
                if (velocity.Y > 0f) velocity.Y = 0f;
            }

            // ── Apply ────────────────────────────────────────────────────────
            if (rider.MoveH(move.X)) velocity.X = 0f;
            if (rider.MoveV(move.Y)) velocity.Y = 0f;

            if (LockCamera) level.Camera.Position = lockedCameraPos;

            // Keep the flight pose asserted — cutscenes and hurt states can
            // stomp it while the ride is live.
            if (manualAnim)
            {
                PlayerSprite? sprite = vanillaRider?.Sprite ?? kirbyRider?.Sprite;
                if (sprite != null && sprite.Has(RIDE_ANIM)
                    && sprite.CurrentAnimationID != RIDE_ANIM)
                    sprite.Play(RIDE_ANIM);
            }

            // ── Warp trail ───────────────────────────────────────────────────
            trailTimer -= dt;
            if (trailTimer <= 0f)
            {
                trailTimer = TRAIL_PERIOD;
                Vector2 behind = velocity.LengthSquared() > 1f
                    ? -velocity.SafeNormalize() * 8f
                    : new Vector2(-8f, 0f);
                level.ParticlesFG.Emit(KirbyFinalBattleScene.P_WarpTrail, 1,
                    rider.Center + behind, Vector2.One * 4f);
            }

            if (BeamEnabled) UpdateBeam(dt);
        }

        // ── Beam charge ──────────────────────────────────────────────────────
        private void UpdateBeam(float dt)
        {
            if (Input.Dash.Check)
            {
                BeamCharge = Math.Min(BeamCharge + dt / Math.Max(BeamChargeTime, 0.01f), 1f);
            }
            else if (BeamCharge > 0f)
            {
                float charge = BeamCharge;
                BeamCharge = 0f;
                Input.Dash.ConsumeBuffer();
                OnBeamRelease?.Invoke(rider?.Center ?? Position, charge);
            }
        }

        // ── Teardown ─────────────────────────────────────────────────────────
        public override void Removed(Scene scene)
        {
            RestorePlayer();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            RestorePlayer();
            base.SceneEnd(scene);
        }

        /// <summary>
        /// Hands control back to the rider. Safe to call more than once, and safe
        /// to call after the rider has left the scene or died.
        /// </summary>
        public void RestorePlayer()
        {
            if (restored) return;
            restored = true;

            if (vanillaRider != null && vanillaRider.Scene != null)
            {
                vanillaRider.DummyGravity       = true;
                vanillaRider.DummyFriction      = true;
                vanillaRider.DummyMaxspeed      = true;
                vanillaRider.DummyAutoAnimate   = true;
                vanillaRider.EnforceLevelBounds = true;
                vanillaRider.Speed              = Vector2.Zero;
                if (!vanillaRider.Dead && vanillaRider.StateMachine.State == Player.StDummy)
                    vanillaRider.StateMachine.State = Player.StNormal;
            }

            if (kirbyRider != null && kirbyRider.Scene != null)
            {
                kirbyRider.DummyGravity     = true;
                kirbyRider.DummyFriction    = true;
                kirbyRider.DummyMaxspeed    = true;
                kirbyRider.DummyAutoAnimate = true;
                kirbyRider.Speed            = Vector2.Zero;
                if (!kirbyRider.Dead && kirbyRider.StateMachine.State == K_Player.StDummy)
                    kirbyRider.StateMachine.State = K_Player.StNormal;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RIDING WARP STAR  —  the visible star drawn under the rider.
    //  Spawned alongside WarpStarRideController and removed with it.
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class RidingWarpStar : Entity
    {
        /// <summary>Offset below the rider's centre so the star reads as a seat.</summary>
        private const float RIDER_OFFSET_Y = 8f;

        private Level   level = null!;
        private Actor?  rider;
        private string  startRoom = "";
        private float   spinAngle;
        private float   pulseTimer;
        private float   trailTimer;

        private static readonly Color COL_STAR  = Calc.HexToColor("FFD700");
        private static readonly Color COL_INNER = Calc.HexToColor("ffffff");

        private static readonly ParticleType P_Trail = new ParticleType
        {
            Color          = Calc.HexToColor("FFD700"),
            Color2         = Calc.HexToColor("fffaaa"),
            ColorMode      = ParticleType.ColorModes.Blink,
            FadeMode       = ParticleType.FadeModes.Late,
            Size           = 1f,
            LifeMin        = 0.2f,
            LifeMax        = 0.5f,
            SpeedMin       = 15f,
            SpeedMax       = 45f,
            DirectionRange = MathHelper.TwoPi
        };

        public RidingWarpStar(Vector2 startPos)
            : base(startPos)
        {
            // Higher Depth draws EARLIER, i.e. further back. The player sits at
            // depth 0, so the star needs a positive depth to sit behind them —
            // the old -9900 drew it on top and hid the rider.
            Depth = 100;
            Tag   = Tags.Persistent;
            Add(new VertexLight(COL_STAR, 1f, 32, 80));
            Add(new BloomPoint(0.7f, 32f));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level     = SceneAs<Level>();
            startRoom = level.Session.Level;
            rider     = WarpStarRideController.ResolveRider(scene);
        }

        public override void Update()
        {
            base.Update();

            if (level.Session.Level != startRoom)
            {
                RemoveSelf();
                return;
            }

            if (rider == null || rider.Scene == null)
                rider = WarpStarRideController.ResolveRider(Scene);

            // Nothing to sit under — hide rather than draw at a stale position.
            Visible = rider != null;
            if (rider == null) return;

            spinAngle  += Engine.DeltaTime * 2.8f;
            pulseTimer += Engine.DeltaTime * 3.5f;

            Position = new Vector2(rider.Center.X, rider.Center.Y + RIDER_OFFSET_Y);

            // Trail streams out behind the direction of travel, not blindly to
            // the right of the star.
            trailTimer -= Engine.DeltaTime;
            if (trailTimer <= 0f)
            {
                trailTimer = 0.08f;
                var controller = Scene.Tracker.GetEntity<WarpStarRideController>();
                Vector2 velocity = controller?.Velocity ?? Vector2.Zero;
                Vector2 behind = velocity.LengthSquared() > 1f
                    ? -velocity.SafeNormalize() * 6f
                    : new Vector2(-6f, 0f);
                level.ParticlesFG.Emit(P_Trail, 1, Position + behind, Vector2.One * 3f);
            }
        }

        public override void Render()
        {
            if (!Visible) return;

            float pulse = (float)Math.Sin(pulseTimer) * 2f;
            float r1 = 12f + pulse;
            float r2 = r1 * 0.4f;

            // Outer glow
            Draw.Circle(Position, r1 + 6f, COL_STAR * 0.2f,  12);
            Draw.Circle(Position, r1 + 3f, COL_STAR * 0.35f, 10);

            // 5-point star
            for (int i = 0; i < 5; i++)
            {
                float a1 = spinAngle + i         * MathHelper.TwoPi / 5f;
                float a2 = spinAngle + (i + 0.5f) * MathHelper.TwoPi / 5f;
                float a3 = spinAngle + (i + 1f)   * MathHelper.TwoPi / 5f;
                Vector2 o1    = Position + Calc.AngleToVector(a1, r1);
                Vector2 inner = Position + Calc.AngleToVector(a2, r2);
                Vector2 o2    = Position + Calc.AngleToVector(a3, r1);
                Draw.Line(o1, inner, COL_STAR, 2f);
                Draw.Line(inner, o2, COL_STAR, 2f);
                Draw.Line(o1, o2,    COL_INNER * 0.25f, 1f);
            }

            // Bright core
            Draw.Circle(Position, 4f, COL_INNER * 0.9f, 8);
        }
    }
}
