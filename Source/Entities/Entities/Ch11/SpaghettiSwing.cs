namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// SpaghettiSwing - Papyrus' spaghetti strand hanging from an anchor, swinging as
    /// a pendulum. The player grabs the handle, pumps the swing with left/right, then
    /// jumps off and keeps the handle's tangential velocity.
    ///
    /// Placeholder art: circle anchor, line strand, square handle.
    /// </summary>
    [CustomEntity("DZ/SpaghettiSwing")]
    [Tracked]
    public class SpaghettiSwing : Entity
    {
        #region Fields
        private static readonly Color AnchorFill = Calc.HexToColor("8a6b4a");
        private static readonly Color AnchorOutline = Calc.HexToColor("4a3722");
        private static readonly Color StrandColor = Calc.HexToColor("f2d98c");
        private static readonly Color HandleFill = Calc.HexToColor("d94f3d");
        private static readonly Color HandleOutline = Calc.HexToColor("6e1f16");

        private readonly float length;
        private readonly float maxAngle;
        private readonly float gravity;
        private readonly float damping;
        private readonly float pumpStrength;
        private readonly float launchBoost;
        private readonly float grabRadius;
        private readonly float holdTimeLimit;
        private readonly bool autoGrab;

        private float angle;
        private float angularVelocity;
        private float holdTimer;
        private float regrabCooldown;
        private Player rider;
        private Level level;

        /// <summary>Handle position; angle 0 hangs straight down from the anchor.</summary>
        public Vector2 HandlePosition => Position + new Vector2(
            (float)Math.Sin(angle) * length,
            (float)Math.Cos(angle) * length);

        /// <summary>Tangential velocity of the handle, used as the launch speed.</summary>
        public Vector2 HandleVelocity => new Vector2(
            (float)Math.Cos(angle),
            -(float)Math.Sin(angle)) * (angularVelocity * length);
        #endregion

        #region Constructor
        public SpaghettiSwing(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            length = Math.Max(8f, data.Float("length", 48f));
            maxAngle = MathHelper.ToRadians(MathHelper.Clamp(data.Float("maxAngle", 65f), 5f, 150f));
            gravity = data.Float("gravity", 900f);
            damping = MathHelper.Clamp(data.Float("damping", 0.9992f), 0.9f, 1f);
            pumpStrength = data.Float("pumpStrength", 2.2f);
            launchBoost = data.Float("launchBoost", 1.15f);
            grabRadius = data.Float("grabRadius", 10f);
            holdTimeLimit = data.Float("holdTimeLimit", 0f);
            autoGrab = data.Bool("autoGrab", true);

            angle = MathHelper.ToRadians(data.Float("startAngle", -30f));
            angle = MathHelper.Clamp(angle, -maxAngle, maxAngle);

            Depth = -50;
        }
        #endregion

        #region Public API
        public void Grab(Player player)
        {
            if (rider != null || player == null || player.Dead)
            {
                return;
            }

            rider = player;
            holdTimer = 0f;
            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;
            player.DummyAutoAnimate = false;
            player.Speed = Vector2.Zero;
            Audio.Play(ChapterSfx.SnowSwing, HandlePosition);
        }

        public void Release(bool launched)
        {
            if (rider == null)
            {
                return;
            }

            Player player = rider;
            rider = null;
            regrabCooldown = 0.25f;

            player.StateMachine.State = Player.StNormal;
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;

            if (launched)
            {
                Vector2 launch = HandleVelocity * launchBoost;
                launch.Y -= 90f;
                player.Speed = launch;
                Audio.Play("event:/char/madeline/jump", player.Position);
            }
        }
        #endregion

        #region Private
        private void UpdatePendulum()
        {
            // Simple rigid pendulum: a = -(g / L) * sin(theta)
            float angularAcceleration = -(gravity / length) * (float)Math.Sin(angle);
            angularVelocity += angularAcceleration * Engine.DeltaTime;

            if (rider != null && pumpStrength > 0f)
            {
                // Pumping only helps when pushing the way the swing is already going.
                int input = Input.MoveX.Value;
                if (input != 0)
                {
                    angularVelocity += input * pumpStrength * Engine.DeltaTime;
                }
            }

            angularVelocity *= damping;
            angle += angularVelocity * Engine.DeltaTime;

            if (angle > maxAngle)
            {
                angle = maxAngle;
                angularVelocity = -Math.Abs(angularVelocity) * 0.5f;
            }
            else if (angle < -maxAngle)
            {
                angle = -maxAngle;
                angularVelocity = Math.Abs(angularVelocity) * 0.5f;
            }
        }

        private void UpdateRider()
        {
            if (rider == null)
            {
                return;
            }

            if (rider.Dead || rider.Scene == null)
            {
                rider = null;
                return;
            }

            rider.Position = HandlePosition + new Vector2(0f, 10f);
            rider.Speed = Vector2.Zero;

            holdTimer += Engine.DeltaTime;

            if (Input.Jump.Pressed)
            {
                Input.Jump.ConsumeBuffer();
                Release(launched: true);
                return;
            }

            if (!autoGrab && !Input.Grab.Check)
            {
                Release(launched: true);
                return;
            }

            if (holdTimeLimit > 0f && holdTimer >= holdTimeLimit)
            {
                Release(launched: true);
            }
        }

        private void TryGrab()
        {
            if (regrabCooldown > 0f)
            {
                regrabCooldown -= Engine.DeltaTime;
                return;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }

            if (Vector2.Distance(player.Center, HandlePosition) > grabRadius + 6f)
            {
                return;
            }

            if (autoGrab || Input.Grab.Check)
            {
                Grab(player);
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Removed(Scene scene)
        {
            // Never leave the player stuck in dummy state if the room unloads mid-swing.
            Release(launched: false);
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            UpdatePendulum();

            if (rider != null)
            {
                UpdateRider();
            }
            else
            {
                TryGrab();
            }
        }

        public override void Render()
        {
            base.Render();

            Vector2 handle = HandlePosition;

            // Strand, drawn as a few segments so it reads as a droopy noodle.
            const int segments = 6;
            Vector2 previous = Position;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float segmentAngle = angle * t;
                Vector2 point = Position + new Vector2(
                    (float)Math.Sin(segmentAngle) * length * t,
                    (float)Math.Cos(segmentAngle) * length * t);

                Draw.Line(previous, point, StrandColor);
                previous = point;
            }

            Placeholder.Circle(Position, 4f, AnchorFill, AnchorOutline);
            Placeholder.SquareCentered(handle, 10f, HandleFill, HandleOutline);

            if (rider == null)
            {
                Draw.Circle(handle, grabRadius, HandleOutline * 0.4f, 8);
            }
        }
        #endregion
    }
}
