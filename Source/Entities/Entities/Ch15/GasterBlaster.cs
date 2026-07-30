namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// GasterBlaster - Citadel skull cannon. Charges (the head swells and the eyes lit
    /// up), then fires a beam along <c>angle</c> for <c>beamDuration</c>, then cools
    /// down and repeats. With <c>aimsAtPlayer</c> it re-aims during the charge only, so
    /// the shot is always dodgeable if the player reads the tell.
    ///
    /// The beam collides by distance-to-segment instead of a rotated hitbox, so any
    /// angle works without special-casing the four cardinals.
    ///
    /// Placeholder art: white square skull, two circle eyes, square muzzle, beam line.
    /// </summary>
    [CustomEntity("DZ/GasterBlaster")]
    [Tracked]
    public class GasterBlaster : Entity
    {
        #region Enums
        public enum BlasterState
        {
            Idle,
            Charging,
            Firing,
            Cooldown
        }
        #endregion

        #region Fields
        private static readonly Color SkullFill = Calc.HexToColor("f2f2f2");
        private static readonly Color SkullOutline = Calc.HexToColor("2a2a33");
        private static readonly Color EyeIdle = Calc.HexToColor("2a2a33");
        private static readonly Color EyeCharged = Calc.HexToColor("ff4d6d");
        private static readonly Color BeamColor = Calc.HexToColor("cfe8ff");
        private static readonly Color BeamCore = Calc.HexToColor("ffffff");

        public BlasterState State { get; private set; } = BlasterState.Idle;

        private readonly float chargeTime;
        private readonly float beamDuration;
        private readonly float cooldown;
        private readonly float beamWidth;
        private readonly float beamLength;
        private readonly float headSize;
        private readonly float startDelay;
        private readonly bool aimsAtPlayer;
        private readonly int shots;
        private readonly bool deadly;
        private readonly string flag;
        private readonly bool invertFlag;

        private float angle;
        private float stateTimer;
        private int shotsFired;
        private float recoil;
        private Level level;
        private VertexLight muzzleLight;

        /// <summary>Unit vector the beam travels along.</summary>
        public Vector2 Direction => Calc.AngleToVector(angle, 1f);

        /// <summary>Where the beam leaves the skull.</summary>
        public Vector2 MuzzlePosition => Position + Direction * (headSize / 2f + 2f - recoil);
        #endregion

        #region Constructor
        public GasterBlaster(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            chargeTime = Math.Max(0.1f, data.Float("chargeTime", 1f));
            beamDuration = Math.Max(0.05f, data.Float("beamDuration", 0.55f));
            cooldown = Math.Max(0.05f, data.Float("cooldown", 1.6f));
            beamWidth = Math.Max(2f, data.Float("beamWidth", 14f));
            beamLength = Math.Max(16f, data.Float("beamLength", 260f));
            headSize = Math.Max(8f, data.Float("headSize", 16f));
            startDelay = Math.Max(0f, data.Float("startDelay", 0f));
            aimsAtPlayer = data.Bool("aimsAtPlayer", false);
            shots = Math.Max(0, data.Int("shots", 0));
            deadly = data.Bool("deadly", true);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            angle = MathHelper.ToRadians(data.Float("angle", 90f));
            stateTimer = startDelay > 0f ? startDelay : cooldown;

            Collider = new Circle(headSize / 2f);
            Depth = -8600;

            Add(muzzleLight = new VertexLight(Color.White, 0f, 24, 64));
        }
        #endregion

        #region Public API
        public void Fire()
        {
            State = BlasterState.Firing;
            stateTimer = beamDuration;
            recoil = 4f;
            shotsFired++;

            Audio.Play(ChapterSfx.CitadelFire, Position);
            level?.Shake(0.25f);
        }
        #endregion

        #region Private
        /// <summary>Whether this entity's enable flag currently permits it to run.</summary>
        private bool FlagAllows()
        {
            if (string.IsNullOrEmpty(flag) || level == null)
            {
                return true;
            }

            return level.Session.GetFlag(flag) != invertFlag;
        }

        private void AimAtPlayer()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }

            float target = Calc.Angle(Position, player.Center);
            angle = Calc.AngleApproach(angle, target, 3f * Engine.DeltaTime);
        }

        private void CheckBeamHit()
        {
            if (!deadly)
            {
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }

            Vector2 start = MuzzlePosition;
            Vector2 end = start + Direction * beamLength;

            // Player hurtbox is roughly 8 wide, so add a little slack to the half-width.
            if (Placeholder.DistanceToSegment(player.Center, start, end) <= beamWidth / 2f + 3f)
            {
                player.Die(Direction);
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            recoil = Calc.Approach(recoil, 0f, 20f * Engine.DeltaTime);

            if (!FlagAllows())
            {
                muzzleLight.Alpha = 0f;
                return;
            }

            if (shots > 0 && shotsFired >= shots && State == BlasterState.Idle)
            {
                muzzleLight.Alpha = 0f;
                return;
            }

            stateTimer -= Engine.DeltaTime;

            switch (State)
            {
                case BlasterState.Idle:
                    if (stateTimer <= 0f)
                    {
                        State = BlasterState.Charging;
                        stateTimer = chargeTime;
                        Audio.Play(ChapterSfx.CitadelTarget, Position);
                    }
                    break;

                case BlasterState.Charging:
                    if (aimsAtPlayer)
                    {
                        AimAtPlayer();
                    }

                    if (stateTimer <= 0f)
                    {
                        Fire();
                    }
                    break;

                case BlasterState.Firing:
                    CheckBeamHit();

                    if (stateTimer <= 0f)
                    {
                        State = BlasterState.Cooldown;
                        stateTimer = cooldown;
                    }
                    break;

                case BlasterState.Cooldown:
                    if (stateTimer <= 0f)
                    {
                        State = BlasterState.Idle;
                        stateTimer = 0f;
                    }
                    break;
            }

            muzzleLight.Alpha = State switch
            {
                BlasterState.Charging => 1f - MathHelper.Clamp(stateTimer / chargeTime, 0f, 1f),
                BlasterState.Firing => 1f,
                _ => 0f
            };
        }

        public override void Render()
        {
            base.Render();

            if (!FlagAllows())
            {
                return;
            }

            float chargeAmount = State == BlasterState.Charging
                ? 1f - MathHelper.Clamp(stateTimer / chargeTime, 0f, 1f)
                : State == BlasterState.Firing ? 1f : 0f;

            RenderBeam();
            RenderSkull(chargeAmount);
        }

        private void RenderBeam()
        {
            if (State == BlasterState.Charging)
            {
                // Thin aiming line as the tell.
                float amount = 1f - MathHelper.Clamp(stateTimer / chargeTime, 0f, 1f);
                Vector2 start = MuzzlePosition;
                Draw.Line(start, start + Direction * beamLength, BeamColor * (amount * 0.35f));
                return;
            }

            if (State != BlasterState.Firing)
            {
                return;
            }

            // Beam width ramps up fast then tapers, so it flashes rather than lingers.
            float progress = 1f - MathHelper.Clamp(stateTimer / beamDuration, 0f, 1f);
            float width = beamWidth * (progress < 0.2f ? progress / 0.2f : 1f - (progress - 0.2f) / 0.8f * 0.35f);

            Vector2 beamStart = MuzzlePosition;
            Vector2 beamEnd = beamStart + Direction * beamLength;

            Draw.Line(beamStart, beamEnd, BeamColor * 0.5f, width);
            Draw.Line(beamStart, beamEnd, BeamCore, Math.Max(1f, width * 0.4f));

            // Muzzle flare.
            Placeholder.FillCircle(beamStart, width * 0.8f, BeamCore * 0.8f);
        }

        private void RenderSkull(float chargeAmount)
        {
            float size = headSize * (1f + chargeAmount * 0.15f);
            Vector2 headCenter = Position - Direction * recoil;

            Placeholder.SquareCentered(headCenter, size, SkullFill, SkullOutline);

            // Eyes sit perpendicular to the firing direction.
            Vector2 side = new Vector2(-Direction.Y, Direction.X);
            Color eye = Color.Lerp(EyeIdle, EyeCharged, chargeAmount);
            float eyeRadius = size * 0.14f;

            Placeholder.FillCircle(headCenter + side * (size * 0.22f) - Direction * (size * 0.1f), eyeRadius, eye);
            Placeholder.FillCircle(headCenter - side * (size * 0.22f) - Direction * (size * 0.1f), eyeRadius, eye);

            // Muzzle block on the firing face.
            Vector2 muzzle = headCenter + Direction * (size * 0.42f);
            Placeholder.SquareCentered(muzzle, size * 0.4f, SkullFill, SkullOutline);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            Vector2 start = MuzzlePosition;
            Draw.Line(start, start + Direction * beamLength, Color.Red * 0.4f);
        }
        #endregion
    }
}
