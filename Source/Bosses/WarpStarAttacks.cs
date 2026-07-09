#nullable enable
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Bosses
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //  WARP STAR ATTACK ARSENAL
    //
    //  All four weapons the Warp Star fires during KirbyFinalBattleScene.
    //  Each is a self-contained Entity with its own collision, VFX, SFX, and
    //  kill logic — nothing is hard-coded to a specific boss class.
    //
    //  ┌──────────────────────────────────────────────────────────────────────┐
    //  │  StarBullet      — small gold star shot, 6 px radius, homing-lite   │
    //  │  BigStarBullet   — large star shot, 16 px radius, slower, more dmg  │
    //  │  WarpLaser       — standard angle beam, charge→lock→fire pattern    │
    //  │  WarpMegaLaser   — full-screen horizontal beam, longer charge,      │
    //  │                    persistent kill zone, screen-wide darkness fade   │
    //  └──────────────────────────────────────────────────────────────────────┘
    //
    //  All attacks:
    //    • Kill Player and K_Player on contact
    //    • Are spawned by KirbyFinalBattleScene.FireAttack(…) during battle
    //    • Share the same WarpStarAttacks.P_Explode particle type
    //    • Use the same WarpStarAttacks.P_Dissipate beam-charge particle type
    //
    // ══════════════════════════════════════════════════════════════════════════

    // ── Shared particles ──────────────────────────────────────────────────────
    public static class WarpStarAttacks
    {
        public static readonly ParticleType P_Explode = new ParticleType
        {
            Color          = Calc.HexToColor("FFD700"),
            Color2         = Calc.HexToColor("ffffff"),
            ColorMode      = ParticleType.ColorModes.Blink,
            FadeMode       = ParticleType.FadeModes.Late,
            Size           = 1f,
            LifeMin        = 0.4f,
            LifeMax        = 0.9f,
            SpeedMin       = 30f,
            SpeedMax       = 100f,
            DirectionRange = MathHelper.TwoPi
        };

        public static readonly ParticleType P_BigExplode = new ParticleType
        {
            Color          = Calc.HexToColor("FFD700"),
            Color2         = Calc.HexToColor("ff8800"),
            ColorMode      = ParticleType.ColorModes.Blink,
            FadeMode       = ParticleType.FadeModes.Late,
            Size           = 2f,
            LifeMin        = 0.6f,
            LifeMax        = 1.4f,
            SpeedMin       = 60f,
            SpeedMax       = 200f,
            DirectionRange = MathHelper.TwoPi
        };

        public static readonly ParticleType P_Dissipate = new ParticleType
        {
            Color   = Calc.HexToColor("FFD700"),
            Color2  = Calc.HexToColor("ffe566"),
            Size    = 1f,
            LifeMin = 0.8f,
            LifeMax = 1.4f,
        };

        public static readonly ParticleType P_MegaDissipate = new ParticleType
        {
            Color   = Calc.HexToColor("ffffff"),
            Color2  = Calc.HexToColor("FFD700"),
            Size    = 2f,
            LifeMin = 1.2f,
            LifeMax = 2.0f,
        };
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  STAR BULLET  — small, fast, mild homing
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class WarpStarBullet : Entity
    {
        // ── Constants ─────────────────────────────────────────────────────────
        private const float RADIUS     = 6f;
        private const float SPEED      = 220f;
        private const float LIFETIME   = 5f;
        private const float HOME_FORCE = 120f;    // homing acceleration px/s²
        private const float HOME_TIME  = 1.5f;    // how long homing is active

        // ── State ─────────────────────────────────────────────────────────────
        private Vector2  velocity;
        private float    lifeTimer;
        private float    homeTimer;
        private float    spinAngle;
        private bool     pendingExplode;
        private Level    level = null!;
        private Player?  vanillaTarget;
        private K_Player? kirbyTarget;

        // ── Visual ────────────────────────────────────────────────────────────
        private readonly VertexLight light;

        // ── Constructor ───────────────────────────────────────────────────────
        public WarpStarBullet(Vector2 position, Vector2 direction)
            : base(position)
        {
            velocity  = direction.SafeNormalize(SPEED);
            lifeTimer = LIFETIME;
            homeTimer = HOME_TIME;
            Collider  = new Circle(RADIUS);
            Depth     = -10000;

            light = new VertexLight(Calc.HexToColor("FFD700"), 0.8f, 20, 48);
            Add(light);
            Add(new PlayerCollider(OnPlayer));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level       = SceneAs<Level>();
            vanillaTarget = scene.Tracker.GetEntity<Player>();
            kirbyTarget   = scene.Tracker.GetEntity<K_Player>();
        }

        public override void Update()
        {
            base.Update();
            if (pendingExplode) { Explode(); return; }

            spinAngle += Engine.DeltaTime * 5f;

            // Mild homing toward the closest player
            if (homeTimer > 0f)
            {
                homeTimer -= Engine.DeltaTime;
                Vector2 target = GetTargetCenter();
                if (target != Vector2.Zero)
                {
                    Vector2 toTarget = (target - Position).SafeNormalize();
                    velocity += toTarget * HOME_FORCE * Engine.DeltaTime;
                    // Cap speed so homing doesn't accelerate forever
                    if (velocity.Length() > SPEED * 1.4f)
                        velocity = velocity.SafeNormalize(SPEED * 1.4f);
                }
            }

            Position += velocity * Engine.DeltaTime;

            // Kill on solid
            if (CollideCheck<Solid>()) { Explode(); return; }

            // Kill on K_Player directly (PlayerCollider handles vanilla player)
            if (kirbyTarget != null && kirbyTarget.Collidable && !kirbyTarget.Dead
                && CollideCheck(kirbyTarget))
            {
                kirbyTarget.Die(velocity.SafeNormalize());
                Explode();
                return;
            }

            lifeTimer -= Engine.DeltaTime;
            if (lifeTimer <= 0f) { Explode(); }
        }

        private void OnPlayer(Player p)
        {
            p.Die(velocity.SafeNormalize());
            pendingExplode = true;
        }

        private Vector2 GetTargetCenter()
        {
            if (vanillaTarget != null && !vanillaTarget.Dead) return vanillaTarget.Center;
            if (kirbyTarget   != null && !kirbyTarget.Dead)   return kirbyTarget.Center;
            return Vector2.Zero;
        }

        private void Explode()
        {
            level.ParticlesFG.Emit(WarpStarAttacks.P_Explode, 10, Position, Vector2.One * 4f);
            Audio.Play("event:/game/undertale/star_burst", Position);
            RemoveSelf();
        }

        public override void Render()
        {
            // 5-pointed star shape, rendered as a rotating pentagon of lines
            float r1 = RADIUS + 2f, r2 = RADIUS * 0.42f;
            Color c  = Calc.HexToColor("FFD700");
            Color cg = Calc.HexToColor("ffffff") * 0.55f;

            // Outer glow
            Draw.Circle(Position, RADIUS + 3f, c * 0.3f, 8);

            // 5-point star via alternating outer/inner points
            for (int i = 0; i < 5; i++)
            {
                float a1 = spinAngle + i * MathHelper.TwoPi / 5f;
                float a2 = spinAngle + (i + 0.5f) * MathHelper.TwoPi / 5f;
                float a3 = spinAngle + (i + 1f)   * MathHelper.TwoPi / 5f;
                Vector2 outer1 = Position + Calc.AngleToVector(a1, r1);
                Vector2 inner  = Position + Calc.AngleToVector(a2, r2);
                Vector2 outer2 = Position + Calc.AngleToVector(a3, r1);
                Draw.Line(outer1, inner,  c);
                Draw.Line(inner,  outer2, c);
                Draw.Line(outer1, outer2, cg * 0.5f);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  BIG STAR BULLET  — slow, large, strong homing, big explosion
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class WarpStarBigBullet : Entity
    {
        private const float RADIUS     = 16f;
        private const float SPEED      = 130f;
        private const float LIFETIME   = 7f;
        private const float HOME_FORCE = 180f;
        private const float HOME_TIME  = 3f;
        private const float PULSE_SPEED = 3f;

        private Vector2  velocity;
        private float    lifeTimer;
        private float    homeTimer;
        private float    spinAngle;
        private float    pulseTimer;
        private bool     pendingExplode;
        private Level    level = null!;
        private Player?  vanillaTarget;
        private K_Player? kirbyTarget;
        private readonly VertexLight light;
        private readonly BloomPoint  bloom;

        public WarpStarBigBullet(Vector2 position, Vector2 direction)
            : base(position)
        {
            velocity  = direction.SafeNormalize(SPEED);
            lifeTimer = LIFETIME;
            homeTimer = HOME_TIME;
            Collider  = new Circle(RADIUS);
            Depth     = -10000;

            light = new VertexLight(Calc.HexToColor("ff8800"), 1f, 40, 96);
            bloom = new BloomPoint(0.8f, 36f);
            Add(light, bloom);
            Add(new PlayerCollider(OnPlayer));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level         = SceneAs<Level>();
            vanillaTarget = scene.Tracker.GetEntity<Player>();
            kirbyTarget   = scene.Tracker.GetEntity<K_Player>();
        }

        public override void Update()
        {
            base.Update();
            if (pendingExplode) { Explode(); return; }

            spinAngle  += Engine.DeltaTime * 2.5f;
            pulseTimer += Engine.DeltaTime * PULSE_SPEED;

            if (homeTimer > 0f)
            {
                homeTimer -= Engine.DeltaTime;
                Vector2 target = GetTargetCenter();
                if (target != Vector2.Zero)
                {
                    Vector2 toTarget = (target - Position).SafeNormalize();
                    velocity += toTarget * HOME_FORCE * Engine.DeltaTime;
                    if (velocity.Length() > SPEED * 1.6f)
                        velocity = velocity.SafeNormalize(SPEED * 1.6f);
                }
            }

            Position += velocity * Engine.DeltaTime;

            if (CollideCheck<Solid>()) { Explode(); return; }

            if (kirbyTarget != null && kirbyTarget.Collidable && !kirbyTarget.Dead
                && CollideCheck(kirbyTarget))
            {
                kirbyTarget.Die(velocity.SafeNormalize());
                Explode();
                return;
            }

            lifeTimer -= Engine.DeltaTime;
            if (lifeTimer <= 0f) Explode();
        }

        private void OnPlayer(Player p)
        {
            p.Die(velocity.SafeNormalize());
            pendingExplode = true;
        }

        private Vector2 GetTargetCenter()
        {
            if (vanillaTarget != null && !vanillaTarget.Dead) return vanillaTarget.Center;
            if (kirbyTarget   != null && !kirbyTarget.Dead)   return kirbyTarget.Center;
            return Vector2.Zero;
        }

        private void Explode()
        {
            Level l = SceneAs<Level>();
            l.ParticlesFG.Emit(WarpStarAttacks.P_BigExplode, 28, Position, Vector2.One * 12f);
            l.Shake(0.5f);
            l.Flash(Calc.HexToColor("ff8800") * 0.3f, false);
            l.Displacement.AddBurst(Position, 2f, 32f, 128f, 1f);
            Audio.Play("event:/DZ/new_content/game/21_desolo_zantas/warpstar_launch", Position);
            RemoveSelf();
        }

        public override void Render()
        {
            float pulse = ((float)Math.Sin(pulseTimer) * 0.5f + 0.5f) * 3f;
            float r1 = RADIUS + pulse, r2 = RADIUS * 0.4f;
            Color c  = Calc.HexToColor("ff8800");
            Color c2 = Calc.HexToColor("FFD700");

            // Triple glow rings
            Draw.Circle(Position, r1 + 8f, c  * 0.15f, 10);
            Draw.Circle(Position, r1 + 4f, c  * 0.25f, 10);
            Draw.Circle(Position, r1,      c2 * 0.45f, 10);

            // 5-point star (larger)
            for (int i = 0; i < 5; i++)
            {
                float a1 = spinAngle + i * MathHelper.TwoPi / 5f;
                float a2 = spinAngle + (i + 0.5f) * MathHelper.TwoPi / 5f;
                float a3 = spinAngle + (i + 1f)   * MathHelper.TwoPi / 5f;
                Vector2 o1 = Position + Calc.AngleToVector(a1, r1);
                Vector2 inner = Position + Calc.AngleToVector(a2, r2);
                Vector2 o2 = Position + Calc.AngleToVector(a3, r1);
                Draw.Line(o1, inner, c2, 2f);
                Draw.Line(inner, o2, c2, 2f);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WARP LASER  — standard angled beam, tracks player then locks + fires
    //  Pattern mirrors CharaBossBeam exactly: charge→follow→lock→fire→destroy
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class WarpLaser : Entity
    {
        public const float CHARGE_TIME = 1.4f;
        public const float FOLLOW_TIME = 0.9f;
        public const float ACTIVE_TIME = 0.18f;

        private const float BEAM_LENGTH     = 3000f;
        private const float BEAM_START_DIST = 12f;
        private const float SIDE_ALPHA      = 0.35f;
        private const int   BEAM_SEGMENTS   = 20;

        // ── State ─────────────────────────────────────────────────────────────
        private Vector2  origin;
        private float    angle;
        private float    chargeTimer;
        private float    followTimer;
        private float    activeTimer;
        private float    beamAlpha;
        private float    sideFadeAlpha;
        private bool     locked;
        private Player?  vanillaTarget;
        private K_Player? kirbyTarget;
        private Level    level = null!;

        // ── Vertex fade ────────────────────────────────────────────────────────
        private readonly VertexPositionColor[] fade = new VertexPositionColor[24];

        // ── Constructor ───────────────────────────────────────────────────────
        public WarpLaser(Vector2 origin, float aimAngle)
            : base(origin)
        {
            this.origin     = origin;
            this.angle      = aimAngle;
            chargeTimer     = CHARGE_TIME;
            followTimer     = FOLLOW_TIME;
            activeTimer     = ACTIVE_TIME;
            beamAlpha       = 0f;
            sideFadeAlpha   = 0f;
            locked          = false;
            Depth           = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level         = SceneAs<Level>();
            vanillaTarget = scene.Tracker.GetEntity<Player>();
            kirbyTarget   = scene.Tracker.GetEntity<K_Player>();
            DissipateParticles();
        }

        public override void Update()
        {
            base.Update();
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);

            if (chargeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                chargeTimer  -= Engine.DeltaTime;
                followTimer  -= Engine.DeltaTime;

                if (!locked && followTimer > 0f)
                {
                    Vector2 tc = GetTargetCenter();
                    if (tc != Vector2.Zero && tc != origin)
                    {
                        Vector2 closest = Calc.ClosestPointOnLine(
                            origin, origin + Calc.AngleToVector(angle, BEAM_LENGTH), tc);
                        angle = Calc.Angle(origin, Calc.Approach(closest, tc, 200f * Engine.DeltaTime));
                    }
                }
                else if (!locked && followTimer <= 0f)
                {
                    locked = true;
                }

                if (chargeTimer <= 0f)
                {
                    level.DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    FireDissipate();
                }
            }
            else if (activeTimer > 0f)
            {
                sideFadeAlpha  = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
                activeTimer   -= Engine.DeltaTime;
                PlayerCollideCheck();
                if (activeTimer <= 0f) RemoveSelf();
            }
            else
            {
                RemoveSelf();
            }
        }

        private void PlayerCollideCheck()
        {
            Vector2 from = origin + Calc.AngleToVector(angle, BEAM_START_DIST);
            Vector2 to   = origin + Calc.AngleToVector(angle, BEAM_LENGTH);
            Vector2 perp = (to - from).Perpendicular().SafeNormalize(2f);

            Player?  vp = Scene.CollideFirst<Player>(from + perp, to + perp)
                       ?? Scene.CollideFirst<Player>(from - perp, to - perp)
                       ?? Scene.CollideFirst<Player>(from, to);
            vp?.Die((vp.Center - origin).SafeNormalize());

            K_Player? kp = Scene.CollideFirst<K_Player>(from + perp, to + perp)
                        ?? Scene.CollideFirst<K_Player>(from - perp, to - perp)
                        ?? Scene.CollideFirst<K_Player>(from, to);
            if (kp != null && !kp.Dead) kp.Die((kp.Center - origin).SafeNormalize());
        }

        private void DissipateParticles()
        {
            Vector2 lineA = origin + Calc.AngleToVector(angle, BEAM_START_DIST);
            Vector2 lineB = origin + Calc.AngleToVector(angle, BEAM_LENGTH);
            Vector2 perp  = (lineB - lineA).Perpendicular().SafeNormalize();
            Vector2 dir   = (lineB - lineA).SafeNormalize();
            for (int i = 0; i < 200; i += 14)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    level.ParticlesFG.Emit(WarpStarAttacks.P_Dissipate,
                        lineA + dir * i + perp * 2f * s, perp.Angle());
                }
            }
        }

        private void FireDissipate()
        {
            Vector2 lineA = origin + Calc.AngleToVector(angle, BEAM_START_DIST);
            Vector2 lineB = origin + Calc.AngleToVector(angle, BEAM_LENGTH);
            Vector2 perp  = (lineB - lineA).Perpendicular().SafeNormalize();
            Vector2 dir   = (lineB - lineA).SafeNormalize();
            for (int i = 0; i < 300; i += 14)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    level.ParticlesFG.Emit(WarpStarAttacks.P_Dissipate,
                        lineA + dir * i + perp * 2f * s, perp.Angle());
                    if (i > 0)
                        level.ParticlesFG.Emit(WarpStarAttacks.P_Dissipate,
                            lineA - dir * i + perp * 2f * s, (-perp).Angle());
                }
            }
        }

        public override void Render()
        {
            if (beamAlpha <= 0f) return;

            Vector2 dir = Calc.AngleToVector(angle, 1f);
            Color beamColor = Calc.HexToColor("FFD700") * beamAlpha;
            Color coreColor = Color.White * beamAlpha * 0.8f;

            // Beam segments — gold gradient fading outward
            Vector2 pos = origin + Calc.AngleToVector(angle, BEAM_START_DIST);
            float segLen = BEAM_LENGTH / BEAM_SEGMENTS;
            for (int i = 0; i < BEAM_SEGMENTS; i++)
            {
                float fade = 1f - (float)i / BEAM_SEGMENTS;
                Draw.Line(pos, pos + dir * segLen, beamColor * fade, 3f);
                Draw.Line(pos, pos + dir * segLen, coreColor * fade, 1f);
                pos += dir * segLen;
            }

            // Charge bloom at origin
            if (chargeTimer > 0f)
            {
                float ct = 1f - chargeTimer / CHARGE_TIME;
                Draw.Circle(origin, 6f + ct * 14f, Calc.HexToColor("FFD700") * sideFadeAlpha * 0.7f, 8);
            }

            // Side darkness fade (vertex-based, matches CharaBossBeam)
            Vector2 perp    = dir.Perpendicular();
            Color dark      = Color.Black * sideFadeAlpha * SIDE_ALPHA;
            Color trans     = Color.Transparent;
            Vector2 longDir = dir * 4000f;
            Vector2 wideDir = perp * 120f;

            int v = 0;
            Quad(ref v, origin, -longDir + wideDir * 2f, longDir + wideDir * 2f, longDir + wideDir,      -longDir + wideDir,      dark,  dark);
            Quad(ref v, origin, -longDir + wideDir,      longDir + wideDir,      longDir,                -longDir,                dark,  trans);
            Quad(ref v, origin, -longDir,                longDir,                longDir - wideDir,       -longDir - wideDir,      trans, dark);
            Quad(ref v, origin, -longDir - wideDir,      longDir - wideDir,      longDir - wideDir * 2f, -longDir - wideDir * 2f, dark,  dark);

            GameplayRenderer.End();
            GFX.DrawVertices(level.Camera.Matrix, fade, fade.Length);
            GameplayRenderer.Begin();
        }

        private Vector2 GetTargetCenter()
        {
            if (vanillaTarget != null && !vanillaTarget.Dead) return vanillaTarget.Center;
            if (kirbyTarget   != null && !kirbyTarget.Dead)   return kirbyTarget.Center;
            return Vector2.Zero;
        }

        private void Quad(ref int v, Vector2 offset,
            Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color ca, Color cb)
        {
            fade[v++] = new VertexPositionColor(new Vector3(offset + a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + b, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(offset + a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(offset + d, 0f), cb);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WARP MEGA LASER  — full-screen horizontal beam
    //  Longer charge, 5-second persistent kill zone, full darkness fade
    //  Mirrors CharaBossBiggerBeam pattern exactly.
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class WarpMegaLaser : Entity
    {
        public const float CHARGE_TIME = 2.2f;
        public const float FOLLOW_TIME = 0.6f;
        public const float ACTIVE_TIME = 5.0f;

        private const float BEAM_LENGTH      = 8000f;
        private const float BEAM_START_DIST  = 12f;
        private const float SIDE_ALPHA       = 0.55f;
        private const float MAX_ANGLE_OFFSET = 25f * MathHelper.Pi / 180f; // 25° from horizontal

        // Kill zone: persistent chained colliders along the beam
        private const int   KZ_CIRCLES  = 250;
        private const float KZ_SPACING  = 32f;
        private const float KZ_RADIUS   = 44f;

        // ── State ─────────────────────────────────────────────────────────────
        private Vector2    origin;
        public  float      Angle { get; private set; }
        private float      chargeTimer;
        private float      followTimer;
        private float      activeTimer;
        private float      beamAlpha;
        private float      sideFadeAlpha;
        private bool       locked;
        private MegaKillZone? killZone;
        private Player?    vanillaTarget;
        private K_Player?  kirbyTarget;
        private Level      level = null!;

        private readonly VertexPositionColor[] fade = new VertexPositionColor[24];

        // ── Kill zone ─────────────────────────────────────────────────────────
        private class MegaKillZone : Entity
        {
            private readonly WarpMegaLaser laser;
            private readonly Circle[] circles;

            public MegaKillZone(WarpMegaLaser laser) : base(Vector2.Zero)
            {
                this.laser = laser;
                Depth = laser.Depth;

                var colliders = new Collider[KZ_CIRCLES];
                circles = new Circle[KZ_CIRCLES];
                for (int i = 0; i < KZ_CIRCLES; i++)
                {
                    circles[i]   = new Circle(KZ_RADIUS, i * KZ_SPACING, 0f);
                    colliders[i] = circles[i];
                }
                Collider = new ColliderList(colliders);
                Add(new PlayerCollider(OnVanillaPlayer));
            }

            private void OnVanillaPlayer(Player p) =>
                p.Die((p.Center - laser.origin).SafeNormalize());

            public override void Update()
            {
                base.Update();
                Vector2 dir    = Calc.AngleToVector(laser.Angle, 1f);
                Vector2 start  = laser.origin + Calc.AngleToVector(laser.Angle, BEAM_START_DIST);
                Position       = start;
                for (int i = 0; i < KZ_CIRCLES; i++)
                    circles[i].Position = start + dir * (i * KZ_SPACING) - Position;

                // Also kill K_Player
                var kp = Scene?.Tracker.GetEntity<K_Player>();
                if (kp != null && !kp.Dead && CollideCheck(kp))
                    kp.Die((kp.Center - laser.origin).SafeNormalize());
            }
        }

        // ── Constructor ───────────────────────────────────────────────────────
        public WarpMegaLaser(Vector2 origin, bool fireRight)
            : base(origin)
        {
            this.origin   = origin;
            Angle         = fireRight ? 0f : MathHelper.Pi;
            chargeTimer   = CHARGE_TIME;
            followTimer   = FOLLOW_TIME;
            activeTimer   = ACTIVE_TIME;
            Depth         = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level         = SceneAs<Level>();
            vanillaTarget = scene.Tracker.GetEntity<Player>();
            kirbyTarget   = scene.Tracker.GetEntity<K_Player>();
            DissipateParticles();
        }

        public override void Update()
        {
            base.Update();
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);

            if (chargeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                chargeTimer  -= Engine.DeltaTime;
                followTimer  -= Engine.DeltaTime;

                // Clamp-track: stay mostly horizontal but nudge toward player
                if (!locked && followTimer > 0f)
                {
                    Vector2 tc = GetTargetCenter();
                    if (tc != Vector2.Zero)
                    {
                        float target = Calc.Angle(origin, tc);
                        // Clamp within ±MAX_ANGLE_OFFSET of current horizontal direction
                        float hBase  = (float)Math.Round(Angle / MathHelper.Pi) * MathHelper.Pi;
                        target = MathHelper.Clamp(target, hBase - MAX_ANGLE_OFFSET, hBase + MAX_ANGLE_OFFSET);
                        Angle  = Calc.Approach(Angle, target, 2f * Engine.DeltaTime);
                    }
                }
                else if (!locked && followTimer <= 0f)
                {
                    locked = true;
                }

                if (chargeTimer <= 0f)
                {
                    level.DirectionalShake(Calc.AngleToVector(Angle, 1f), 0.35f);
                    level.Flash(Calc.HexToColor("FFD700") * 0.5f, false);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                    FireDissipate();
                    killZone = new MegaKillZone(this);
                    Scene.Add(killZone);
                }
            }
            else if (activeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 6f);
                activeTimer  -= Engine.DeltaTime;

                if (activeTimer <= 0f)
                {
                    killZone?.RemoveSelf();
                    killZone = null;
                    RemoveSelf();
                }
            }
            else
            {
                killZone?.RemoveSelf();
                RemoveSelf();
            }
        }

        private void DissipateParticles()
        {
            Vector2 lineA = origin + Calc.AngleToVector(Angle, BEAM_START_DIST);
            Vector2 lineB = origin + Calc.AngleToVector(Angle, BEAM_LENGTH);
            Vector2 perp  = (lineB - lineA).Perpendicular().SafeNormalize();
            Vector2 dir   = (lineB - lineA).SafeNormalize();
            for (int i = 0; i < 300; i += 12)
            {
                for (int s = -1; s <= 1; s += 2)
                    level.ParticlesFG.Emit(WarpStarAttacks.P_MegaDissipate,
                        lineA + dir * i + perp * 3f * s, perp.Angle());
            }
        }

        private void FireDissipate()
        {
            Vector2 lineA = origin + Calc.AngleToVector(Angle, BEAM_START_DIST);
            Vector2 lineB = origin + Calc.AngleToVector(Angle, BEAM_LENGTH);
            Vector2 perp  = (lineB - lineA).Perpendicular().SafeNormalize();
            Vector2 dir   = (lineB - lineA).SafeNormalize();
            for (int i = 0; i < 500; i += 12)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    level.ParticlesFG.Emit(WarpStarAttacks.P_MegaDissipate,
                        lineA + dir * i + perp * 3f * s, perp.Angle());
                    if (i > 0)
                        level.ParticlesFG.Emit(WarpStarAttacks.P_MegaDissipate,
                            lineA - dir * i + perp * 3f * s, (-perp).Angle());
                }
            }
        }

        public override void Render()
        {
            if (beamAlpha <= 0f) return;

            Vector2 dir = Calc.AngleToVector(Angle, 1f);

            // Core beam — bright white/gold, 5px thick during active phase
            float thickness = (activeTimer > 0f && chargeTimer <= 0f) ? 5f : 3f;
            Color beamColor = (activeTimer > 0f && chargeTimer <= 0f)
                ? Color.White * beamAlpha
                : Calc.HexToColor("FFD700") * beamAlpha;
            Color outerColor = Calc.HexToColor("ff8800") * beamAlpha * 0.6f;

            Vector2 start = origin + Calc.AngleToVector(Angle, BEAM_START_DIST);
            Vector2 end   = origin + Calc.AngleToVector(Angle, BEAM_LENGTH);

            Draw.Line(start, end, outerColor, thickness + 6f);
            Draw.Line(start, end, beamColor, thickness);

            // Charge bloom
            if (chargeTimer > 0f)
            {
                float ct = 1f - chargeTimer / CHARGE_TIME;
                Draw.Circle(origin, 8f + ct * 22f, Calc.HexToColor("FFD700") * sideFadeAlpha * 0.8f, 10);
                Draw.Circle(origin, 4f + ct * 10f, Color.White * sideFadeAlpha * 0.9f, 8);
            }

            // Side darkness fade
            Vector2 perp    = dir.Perpendicular();
            Color dark      = Color.Black * sideFadeAlpha * SIDE_ALPHA;
            Color trans     = Color.Transparent;
            Vector2 longDir = dir * 4000f;
            Vector2 wideDir = perp * 180f;

            int v = 0;
            Quad(ref v, origin, -longDir + wideDir * 2f, longDir + wideDir * 2f, longDir + wideDir,      -longDir + wideDir,      dark,  dark);
            Quad(ref v, origin, -longDir + wideDir,      longDir + wideDir,      longDir,                -longDir,                dark,  trans);
            Quad(ref v, origin, -longDir,                longDir,                longDir - wideDir,       -longDir - wideDir,      trans, dark);
            Quad(ref v, origin, -longDir - wideDir,      longDir - wideDir,      longDir - wideDir * 2f, -longDir - wideDir * 2f, dark,  dark);

            GameplayRenderer.End();
            GFX.DrawVertices(level.Camera.Matrix, fade, fade.Length);
            GameplayRenderer.Begin();
        }

        private Vector2 GetTargetCenter()
        {
            if (vanillaTarget != null && !vanillaTarget.Dead) return vanillaTarget.Center;
            if (kirbyTarget   != null && !kirbyTarget.Dead)   return kirbyTarget.Center;
            return Vector2.Zero;
        }

        private void Quad(ref int v, Vector2 offset,
            Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color ca, Color cb)
        {
            fade[v++] = new VertexPositionColor(new Vector3(offset + a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + b, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(offset + a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(offset + c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(offset + d, 0f), cb);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  NIGHTMARE ORB  — single straight horizontal energy orb, no homing
    //  Spawned from off-screen right, travels left toward the player.
    //  Each orb can carry the visual identity of a specific Zero form or ELS.
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class NightmareOrb : Entity
    {
        private const float RADIUS   = 7f;
        private const float SPEED    = 160f;
        private const float LIFETIME = 6f;

        // ── Named identity palettes ──────────────────────────────────────────
        // Each entry is (outer, inner, glow) matching the entity the orb
        // "belongs to".  Index order mirrors KirbyFinalBattleScene.ZeroAuraColors.
        public enum Identity
        {
            SiamoZero      = 0,  // purple
            Zero3          = 1,  // blue
            ContraVoid     = 2,  // crimson
            TesseractSoul  = 3,  // teal
            HyperMetaMorpho = 4, // olive/gold
            NollusNova     = 5,  // deep violet
            ElsTermina     = 6,  // blood red / dark
            Default        = 7,  // Nightmare's own magenta
        }

        // (outer, inner, glow)
        private static readonly (Color outer, Color inner, Color glow)[] Palettes =
        {
            // SiamoZero
            (Calc.HexToColor("8800ff"), Calc.HexToColor("cc88ff"), Calc.HexToColor("aa44ff")),
            // Zero 3
            (Calc.HexToColor("0044ff"), Calc.HexToColor("88aaff"), Calc.HexToColor("4488ff")),
            // Contra Void
            (Calc.HexToColor("ff0044"), Calc.HexToColor("ff88aa"), Calc.HexToColor("ff2266")),
            // Tesseract Soul
            (Calc.HexToColor("00ffcc"), Calc.HexToColor("aaffee"), Calc.HexToColor("00ddaa")),
            // Hyper Meta Morpho Knight
            (Calc.HexToColor("888800"), Calc.HexToColor("dddd44"), Calc.HexToColor("aaaa00")),
            // Nollus Nova
            (Calc.HexToColor("440044"), Calc.HexToColor("aa44aa"), Calc.HexToColor("660066")),
            // Els Termina  — deep blood red, near-black core
            (Calc.HexToColor("cc0000"), Calc.HexToColor("330000"), Calc.HexToColor("ff2200")),
            // Default / Nightmare
            (Calc.HexToColor("cc22cc"), Calc.HexToColor("ffffff"), Calc.HexToColor("ff44ff")),
        };

        private readonly Vector2 velocity;
        private readonly Color   colOuter;
        private readonly Color   colInner;
        private readonly Color   colGlow;
        private float lifeTimer;
        private float pulseTimer;
        private bool  pendingExplode;
        private Level level = null!;

        public NightmareOrb(Vector2 position, Identity identity = Identity.Default)
            : base(position)
        {
            int idx   = (int)identity;
            if (idx < 0 || idx >= Palettes.Length) idx = (int)Identity.Default;
            (colOuter, colInner, colGlow) = Palettes[idx];

            // Always travels left — orbs come from the right (Nightmare's side)
            velocity  = new Vector2(-SPEED, 0f);
            lifeTimer = LIFETIME;
            Collider  = new Circle(RADIUS);
            Depth     = -10000;
            Add(new VertexLight(colGlow, 0.9f, 24, 60));
            Add(new PlayerCollider(OnPlayer));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (pendingExplode) { Explode(); return; }

            pulseTimer += Engine.DeltaTime * 4f;
            Position   += velocity * Engine.DeltaTime;

            if (CollideCheck<Solid>()) { Explode(); return; }

            var kp = Scene.Tracker.GetEntity<K_Player>();
            if (kp != null && kp.Collidable && !kp.Dead && CollideCheck(kp))
            {
                kp.Die(velocity.SafeNormalize());
                Explode();
                return;
            }

            lifeTimer -= Engine.DeltaTime;
            if (lifeTimer <= 0f || Position.X < level.Camera.Left - 40f)
                RemoveSelf();
        }

        private void OnPlayer(Player p)
        {
            p.Die(velocity.SafeNormalize());
            pendingExplode = true;
        }

        private void Explode()
        {
            // Explosion particles tinted to this orb's identity colour
            var explodeParticle = new ParticleType(WarpStarAttacks.P_Explode)
            {
                Color  = colOuter,
                Color2 = colInner,
            };
            level.ParticlesFG.Emit(explodeParticle, 12, Position, Vector2.One * 5f);
            Audio.Play("event:/game/undertale/star_burst", Position);
            RemoveSelf();
        }

        public override void Render()
        {
            float pulse = (float)Math.Sin(pulseTimer) * 2f;
            Draw.Circle(Position, RADIUS + 5f + pulse, colGlow  * 0.3f, 10);
            Draw.Circle(Position, RADIUS + 2f + pulse, colOuter * 0.6f, 10);
            Draw.Circle(Position, RADIUS,              colOuter,         10);
            Draw.Circle(Position, RADIUS * 0.5f,       colInner * 0.9f,  8);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RIDING WARP STAR  — visible star sprite that sits under the player
    //  during BattlePhase.WarpStarRide.  Follows player.Center every frame.
    //  Spawned by KirbyFinalBattleScene at the start of WarpStarRide and
    //  removed when the phase ends.
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class RidingWarpStar : Entity
    {
        // Offset below the player's feet so the star looks like a seat
        private const float PLAYER_OFFSET_Y = 8f;

        private Player?   vanillaPlayer;
        private K_Player? kirbyPlayer;
        private float     spinAngle;
        private float     pulseTimer;
        private float     trailTimer;
        private Level     level = null!;

        private static readonly Color COL_STAR  = Calc.HexToColor("FFD700");
        private static readonly Color COL_INNER = Calc.HexToColor("ffffff");
        private static readonly Color COL_TRAIL = Calc.HexToColor("FFD700");

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
            Depth = -9900; // just behind the player (player is typically -10000+)
            Add(new VertexLight(COL_STAR, 1f, 32, 80));
            Add(new BloomPoint(0.7f, 32f));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level         = SceneAs<Level>();
            vanillaPlayer = scene.Tracker.GetEntity<Player>();
            kirbyPlayer   = scene.Tracker.GetEntity<K_Player>();
        }

        public override void Update()
        {
            base.Update();
            spinAngle  += Engine.DeltaTime * 2.8f;
            pulseTimer += Engine.DeltaTime * 3.5f;

            // Sit directly below the riding player
            Vector2? pc = vanillaPlayer?.Center ?? kirbyPlayer?.Center;
            if (pc.HasValue)
                Position = new Vector2(pc.Value.X, pc.Value.Y + PLAYER_OFFSET_Y);

            // Golden trail behind the star
            trailTimer -= Engine.DeltaTime;
            if (trailTimer <= 0f)
            {
                trailTimer = 0.05f;
                level.ParticlesFG.Emit(P_Trail, 2,
                    Position + new Vector2(6f, 0f), Vector2.One * 3f);
            }
        }

        public override void Render()
        {
            float pulse = (float)Math.Sin(pulseTimer) * 2f;
            float r1 = 12f + pulse;
            float r2 = r1 * 0.4f;

            // Outer glow
            Draw.Circle(Position, r1 + 6f, COL_STAR  * 0.2f, 12);
            Draw.Circle(Position, r1 + 3f, COL_STAR  * 0.35f, 10);

            // 5-point star
            for (int i = 0; i < 5; i++)
            {
                float a1 = spinAngle + i       * MathHelper.TwoPi / 5f;
                float a2 = spinAngle + (i+0.5f)* MathHelper.TwoPi / 5f;
                float a3 = spinAngle + (i+1f)  * MathHelper.TwoPi / 5f;
                Vector2 o1    = Position + Calc.AngleToVector(a1, r1);
                Vector2 inner = Position + Calc.AngleToVector(a2, r2);
                Vector2 o2    = Position + Calc.AngleToVector(a3, r1);
                Draw.Line(o1, inner,  COL_STAR,  2f);
                Draw.Line(inner, o2,  COL_STAR,  2f);
                Draw.Line(o1, o2,     COL_INNER * 0.25f, 1f);
            }

            // Bright core
            Draw.Circle(Position, 4f, COL_INNER * 0.9f, 8);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WARP DOWNWARD LASER  — vertical-ish beam fired from above the player
    //  Faithful to the Nightmare Phase 2 downward beam attack.
    //  Charge gathers above, then a straight vertical beam fires down.
    // ══════════════════════════════════════════════════════════════════════════
    [Tracked]
    [HotReloadable]
    public class WarpDownwardLaser : Entity
    {
        public const float CHARGE_TIME = 1.8f;
        public const float ACTIVE_TIME = 0.22f;

        private const float BEAM_HALF_WIDTH = 10f;   // half-width of the downward beam
        private const float BEAM_REACH      = 600f;  // pixels downward
        private const float SIDE_ALPHA      = 0.4f;

        private Vector2   origin;          // top of beam (above player, set at spawn)
        private float     chargeTimer;
        private float     activeTimer;
        private float     beamAlpha;
        private float     sideFadeAlpha;
        private Player?   vanillaTarget;
        private K_Player? kirbyTarget;
        private Level     level = null!;

        private readonly VertexPositionColor[] fade = new VertexPositionColor[24];

        // A small warning circle that telegraphs where the beam will land
        private Vector2 warningPos;

        public WarpDownwardLaser(Vector2 abovePlayerPos)
            : base(abovePlayerPos)
        {
            origin      = abovePlayerPos;
            warningPos  = new Vector2(abovePlayerPos.X, abovePlayerPos.Y + BEAM_REACH);
            chargeTimer = CHARGE_TIME;
            activeTimer = ACTIVE_TIME;
            Depth       = -1000000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level         = SceneAs<Level>();
            vanillaTarget = scene.Tracker.GetEntity<Player>();
            kirbyTarget   = scene.Tracker.GetEntity<K_Player>();
        }

        public override void Update()
        {
            base.Update();
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);

            if (chargeTimer > 0f)
            {
                sideFadeAlpha  = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                chargeTimer   -= Engine.DeltaTime;

                // During charge, track the player's X so warning follows them
                Vector2 tc = GetTargetCenter();
                if (tc != Vector2.Zero)
                {
                    origin.X     = Calc.Approach(origin.X, tc.X, 180f * Engine.DeltaTime);
                    warningPos.X = origin.X;
                }

                if (chargeTimer <= 0f)
                {
                    level.DirectionalShake(Vector2.UnitY, 0.2f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
            }
            else if (activeTimer > 0f)
            {
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 10f);
                activeTimer  -= Engine.DeltaTime;
                PlayerCollideCheck();
                if (activeTimer <= 0f) RemoveSelf();
            }
            else
            {
                RemoveSelf();
            }
        }

        private void PlayerCollideCheck()
        {
            // Vertical beam: thin rectangle from origin down BEAM_REACH pixels
            var rect = new Rectangle(
                (int)(origin.X - BEAM_HALF_WIDTH), (int)origin.Y,
                (int)(BEAM_HALF_WIDTH * 2f),       (int)BEAM_REACH);

            Player? vp = Scene.Tracker.GetEntity<Player>();
            if (vp != null && !vp.Dead && rect.Contains((int)vp.Center.X, (int)vp.Center.Y))
                vp.Die(Vector2.UnitY);

            K_Player? kp = Scene.Tracker.GetEntity<K_Player>();
            if (kp != null && !kp.Dead && rect.Contains((int)kp.Center.X, (int)kp.Center.Y))
                kp.Die(Vector2.UnitY);
        }

        public override void Render()
        {
            if (beamAlpha <= 0f) return;

            Vector2 top = origin;
            Vector2 bot = origin + new Vector2(0f, BEAM_REACH);

            if (chargeTimer > 0f)
            {
                // Charge: expanding circle above the warning point
                float ct   = 1f - chargeTimer / CHARGE_TIME;
                float cr   = 4f + ct * 20f;
                Color warn = Calc.HexToColor("cc22cc") * sideFadeAlpha;
                Draw.Circle(new Vector2(origin.X, origin.Y - 20f), cr, warn, 10);

                // Warning line on the ground
                Draw.Line(
                    new Vector2(warningPos.X - 12f, warningPos.Y),
                    new Vector2(warningPos.X + 12f, warningPos.Y),
                    warn * 0.7f, 2f);
            }
            else
            {
                // Fire: solid beam downward
                Color outer = Calc.HexToColor("cc22cc") * beamAlpha;
                Color core  = Color.White * beamAlpha * 0.9f;
                Draw.Line(top, bot, outer, BEAM_HALF_WIDTH * 2f + 4f);
                Draw.Line(top, bot, core,  BEAM_HALF_WIDTH);
            }

            // Side darkness fade — wide horizontal quads that dim left and right of beam
            Color dark  = Color.Black * sideFadeAlpha * SIDE_ALPHA;
            Color trans = Color.Transparent;
            Vector2 across = Vector2.UnitX * 200f;
            Vector2 down   = Vector2.UnitY * BEAM_REACH;
            int v = 0;
            // Left side
            QuadV(ref v, top - across * 2f, top - across, top - across + down, top - across * 2f + down, dark,  trans);
            QuadV(ref v, top - across,      top,          top + down,          top - across + down,       trans, dark);
            // Right side
            QuadV(ref v, top,               top + across, top + across + down, top + down,                dark,  trans);
            QuadV(ref v, top + across,      top + across * 2f, top + across * 2f + down, top + across + down, trans, dark);

            GameplayRenderer.End();
            GFX.DrawVertices(level.Camera.Matrix, fade, v);
            GameplayRenderer.Begin();
        }

        private Vector2 GetTargetCenter()
        {
            if (vanillaTarget != null && !vanillaTarget.Dead) return vanillaTarget.Center;
            if (kirbyTarget   != null && !kirbyTarget.Dead)   return kirbyTarget.Center;
            return Vector2.Zero;
        }

        private void QuadV(ref int v,
            Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color ca, Color cb)
        {
            fade[v++] = new VertexPositionColor(new Vector3(a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(b, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(a, 0f), ca);
            fade[v++] = new VertexPositionColor(new Vector3(c, 0f), cb);
            fade[v++] = new VertexPositionColor(new Vector3(d, 0f), cb);
        }
    }
}
