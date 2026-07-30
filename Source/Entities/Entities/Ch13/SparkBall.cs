namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// SparkBall - Hotcliff electrical hazard. One entity covering the three movement
    /// shapes a room usually needs:
    ///   Orbit  - circles its placed position at <c>orbitRadius</c>.
    ///   Patrol - walks its nodes in a loop (or ping-pongs).
    ///   Chase  - accelerates toward the player, capped at <c>chaseMaxSpeed</c>.
    ///   Static - sits still and just crackles.
    ///
    /// Placeholder art: bright circle core, flickering ring, spark lines.
    /// </summary>
    [CustomEntity("DZ/SparkBall")]
    [Tracked]
    public class SparkBall : Entity
    {
        #region Enums
        public enum SparkMode
        {
            Orbit,
            Patrol,
            Chase,
            Static
        }
        #endregion

        #region Fields
        private static readonly Color CoreFill = Calc.HexToColor("fff4b0");
        private static readonly Color CoreOutline = Calc.HexToColor("8fd8ff");
        private static readonly Color ArcColor = Calc.HexToColor("6fb9ff");

        private readonly SparkMode mode;
        private readonly float radius;
        private readonly float orbitRadius;
        private readonly float speed;
        private readonly bool clockwise;
        private readonly float chaseAcceleration;
        private readonly float chaseMaxSpeed;
        private readonly int sparkCount;
        private readonly bool deadly;
        private readonly bool pingPong;
        private readonly string flag;
        private readonly bool invertFlag;

        private readonly Vector2 origin;
        private readonly List<Vector2> waypoints = new List<Vector2>();
        private int waypointIndex;
        private int waypointStep = 1;
        private float angle;
        private Vector2 velocity;
        private float flicker;
        private Level level;
        #endregion

        #region Constructor
        public SparkBall(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            mode = data.Enum("mode", SparkMode.Orbit);
            radius = Math.Max(2f, data.Float("radius", 6f));
            orbitRadius = Math.Max(4f, data.Float("orbitRadius", 32f));
            speed = Math.Max(0f, data.Float("speed", 120f));
            clockwise = data.Bool("clockwise", true);
            chaseAcceleration = Math.Max(1f, data.Float("chaseAcceleration", 180f));
            chaseMaxSpeed = Math.Max(1f, data.Float("chaseMaxSpeed", 90f));
            sparkCount = Math.Max(0, data.Int("sparkCount", 4));
            deadly = data.Bool("deadly", true);
            pingPong = data.Bool("pingPong", false);
            flag = data.Attr("flag", "");
            invertFlag = data.Bool("invertFlag", false);

            angle = MathHelper.ToRadians(data.Float("startAngle", 0f));
            origin = Position;

            waypoints.Add(Position);
            if (data.Nodes != null)
            {
                foreach (Vector2 node in data.Nodes)
                {
                    waypoints.Add(node + offset);
                }
            }

            if (mode == SparkMode.Orbit)
            {
                Position = origin + Calc.AngleToVector(angle, orbitRadius);
            }

            Collider = new Circle(radius);
            Depth = -8500;

            Add(new VertexLight(Color.LightSkyBlue, 0.7f, 16, 40));
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

        private void UpdateOrbit()
        {
            // Convert linear speed to angular speed so orbitRadius does not change pace.
            float angularSpeed = speed / orbitRadius;
            angle += angularSpeed * Engine.DeltaTime * (clockwise ? 1f : -1f);
            Position = origin + Calc.AngleToVector(angle, orbitRadius);
        }

        private void UpdatePatrol()
        {
            if (waypoints.Count < 2 || speed <= 0f)
            {
                return;
            }

            Vector2 target = waypoints[waypointIndex];
            Vector2 toTarget = target - Position;
            float step = speed * Engine.DeltaTime;

            if (toTarget.Length() <= step)
            {
                Position = target;
                AdvanceWaypoint();
                return;
            }

            Position += toTarget.SafeNormalize() * step;
        }

        private void AdvanceWaypoint()
        {
            if (pingPong)
            {
                if (waypointIndex + waypointStep >= waypoints.Count || waypointIndex + waypointStep < 0)
                {
                    waypointStep = -waypointStep;
                }

                waypointIndex = Math.Clamp(waypointIndex + waypointStep, 0, waypoints.Count - 1);
                return;
            }

            waypointIndex = (waypointIndex + 1) % waypoints.Count;
        }

        private void UpdateChase()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                velocity = Calc.Approach(velocity, Vector2.Zero, chaseAcceleration * Engine.DeltaTime);
            }
            else
            {
                Vector2 toPlayer = (player.Center - Position).SafeNormalize();
                velocity += toPlayer * chaseAcceleration * Engine.DeltaTime;

                if (velocity.Length() > chaseMaxSpeed)
                {
                    velocity = velocity.SafeNormalize(chaseMaxSpeed);
                }
            }

            Position += velocity * Engine.DeltaTime;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            if (mode == SparkMode.Patrol && waypoints.Count > 1)
            {
                waypointIndex = 1;
            }
        }

        public override void Update()
        {
            base.Update();

            flicker += Engine.DeltaTime * 18f;

            if (!FlagAllows())
            {
                return;
            }

            switch (mode)
            {
                case SparkMode.Orbit:
                    UpdateOrbit();
                    break;
                case SparkMode.Patrol:
                    UpdatePatrol();
                    break;
                case SparkMode.Chase:
                    UpdateChase();
                    break;
            }

            if (deadly)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && CollideCheck(player))
                {
                    player.Die((player.Center - Position).SafeNormalize());
                }
            }
        }

        public override void Render()
        {
            base.Render();

            if (!FlagAllows())
            {
                return;
            }

            float pulse = 1f + (float)Math.Sin(flicker) * 0.15f;

            Draw.Circle(Position, radius * 2f * pulse, ArcColor * 0.25f, 10);
            Placeholder.Circle(Position, radius * pulse, CoreFill, CoreOutline);

            // Crackling arcs.
            for (int i = 0; i < sparkCount; i++)
            {
                float sparkAngle = flicker * 0.7f + i * (MathHelper.TwoPi / Math.Max(1, sparkCount));
                float reach = radius * (1.6f + (float)Math.Sin(flicker * 2.3f + i) * 0.5f);
                Draw.Line(
                    Position + Calc.AngleToVector(sparkAngle, radius * 0.8f),
                    Position + Calc.AngleToVector(sparkAngle, reach),
                    ArcColor);
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            if (mode == SparkMode.Orbit)
            {
                Draw.Circle(origin, orbitRadius, Color.Yellow * 0.4f, 16);
            }
            else if (mode == SparkMode.Patrol)
            {
                for (int i = 0; i < waypoints.Count - 1; i++)
                {
                    Draw.Line(waypoints[i], waypoints[i + 1], Color.Yellow * 0.4f);
                }
            }
        }
        #endregion
    }
}
