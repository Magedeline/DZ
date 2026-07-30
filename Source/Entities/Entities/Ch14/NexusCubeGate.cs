namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// NexusTouchSwitch - the "2.5D" touch switch. Functionally a grouped touch switch
    /// (touch every switch sharing a <c>group</c> to open that group's gates), drawn as
    /// a faux-3D cube so it sits in Cyber Nexus' isometric look.
    ///
    /// Pairs with <see cref="NexusGateCubeBlock"/>. Both live in this file because the
    /// group handshake between them is the whole mechanic.
    ///
    /// Placeholder art: extruded square (front face + offset back face + struts).
    /// </summary>
    [CustomEntity("DZ/NexusTouchSwitch")]
    [Tracked]
    public class NexusTouchSwitch : Entity
    {
        #region Fields
        private static readonly Color OffFront = Calc.HexToColor("1e3a4d");
        private static readonly Color OffSide = Calc.HexToColor("142833");
        private static readonly Color OnFront = Calc.HexToColor("2fe0c4");
        private static readonly Color OnSide = Calc.HexToColor("1c8f7d");
        private static readonly Color Edge = Calc.HexToColor("bff7ee");

        public bool Pressed { get; private set; }
        public bool Finished { get; private set; }
        public string Group { get; }

        private readonly string flag;
        private readonly bool requiresDash;
        private readonly bool persistent;
        private readonly float size;
        private readonly float depthOffset;

        private float lerp;
        private float pulse;
        private Level level;
        #endregion

        #region Constructor
        public NexusTouchSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Group = data.Attr("group", "");
            flag = data.Attr("flag", "");
            requiresDash = data.Bool("requiresDash", false);
            persistent = data.Bool("persistent", false);
            size = Math.Max(6f, data.Float("size", 12f));
            depthOffset = Math.Max(0f, data.Float("depthOffset", 4f));

            Collider = new Hitbox(size, size, -size / 2f, -size / 2f);
            Depth = -100;

            Add(new VertexLight(Color.Aquamarine, 0f, 16, 36));
        }
        #endregion

        #region Public API
        public void Press()
        {
            if (Pressed)
            {
                return;
            }

            Pressed = true;
            pulse = 1f;
            Audio.Play("event:/game/general/touchswitch_any", Position);
            NexusGateCubeBlock.NotifyGroup(Scene, Group);
        }

        public void Finish()
        {
            if (Finished)
            {
                return;
            }

            Finished = true;
            Pressed = true;
            pulse = 1f;
            Audio.Play("event:/game/general/touchswitch_last_oneshot", Position);
        }

        internal string GroupFlag()
        {
            if (!string.IsNullOrEmpty(flag))
            {
                return flag;
            }

            return string.IsNullOrEmpty(Group) ? "nexus_switch" : "nexus_switch_" + Group;
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            if (persistent && level != null && level.Session.GetFlag(GroupFlag()))
            {
                Pressed = true;
                Finished = true;
                lerp = 1f;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!Finished)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && CollideCheck(player) && (!requiresDash || player.DashAttacking))
                {
                    Press();
                }
            }

            lerp = Calc.Approach(lerp, Pressed ? 1f : 0f, Engine.DeltaTime * 5f);
            pulse = Calc.Approach(pulse, 0f, Engine.DeltaTime * 2f);

            Get<VertexLight>().Alpha = lerp;
        }

        public override void Render()
        {
            base.Render();

            Color front = Color.Lerp(OffFront, OnFront, lerp);
            Color side = Color.Lerp(OffSide, OnSide, lerp);

            // Pressed switches sink into their own extrusion.
            float depth = depthOffset * (1f - lerp * 0.6f);

            if (pulse > 0f)
            {
                Draw.Circle(Position, size * (1f + pulse), Edge * (pulse * 0.4f), 12);
            }

            Placeholder.Cube(X - size / 2f, Y - size / 2f, size, size, depth, front, side, Edge);

            // Centre dot reads as the actual contact plate.
            Placeholder.FillCircle(Position, size * 0.2f, Edge * (0.3f + lerp * 0.7f));
        }
        #endregion
    }

    /// <summary>
    /// NexusGateCubeBlock - the "2.5D" gate cube. Solid until its group's switches are
    /// all pressed, then it retracts into its own extrusion and stops colliding.
    /// Set <c>invert</c> to close instead of open.
    /// </summary>
    [CustomEntity("DZ/NexusGateCubeBlock")]
    [Tracked]
    public class NexusGateCubeBlock : Solid
    {
        #region Fields
        private static readonly Color ClosedFront = Calc.HexToColor("2b4f6b");
        private static readonly Color ClosedSide = Calc.HexToColor("1a3244");
        private static readonly Color OpenEdge = Calc.HexToColor("2fe0c4");
        private static readonly Color Edge = Calc.HexToColor("9fd4ea");

        public string Group { get; }
        public bool IsOpen { get; private set; }

        private readonly float openTime;
        private readonly float depthOffset;
        private readonly bool invert;
        private readonly bool startOpen;

        private float openLerp;
        private Level level;
        #endregion

        #region Constructor
        public NexusGateCubeBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, Math.Max(8, data.Width), Math.Max(8, data.Height), safe: true)
        {
            Group = data.Attr("group", "");
            openTime = Math.Max(0.05f, data.Float("openTime", 0.45f));
            depthOffset = Math.Max(0f, data.Float("depthOffset", 4f));
            invert = data.Bool("invert", false);
            startOpen = data.Bool("startOpen", false);

            IsOpen = startOpen;
            openLerp = startOpen ? 1f : 0f;
            Collidable = !startOpen;

            SurfaceSoundIndex = SurfaceIndex.Girder;
            Depth = -9000;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Re-evaluates every gate in <paramref name="group"/> against that group's
        /// switches. Called by <see cref="NexusTouchSwitch.Press"/>.
        /// </summary>
        public static void NotifyGroup(Scene scene, string group)
        {
            if (scene == null)
            {
                return;
            }

            List<NexusTouchSwitch> switches = new List<NexusTouchSwitch>();
            foreach (Entity entity in scene.Tracker.GetEntities<NexusTouchSwitch>())
            {
                NexusTouchSwitch touchSwitch = (NexusTouchSwitch)entity;
                if (touchSwitch.Group == group)
                {
                    switches.Add(touchSwitch);
                }
            }

            bool allPressed = switches.Count > 0;
            foreach (NexusTouchSwitch touchSwitch in switches)
            {
                if (!touchSwitch.Pressed)
                {
                    allPressed = false;
                    break;
                }
            }

            if (!allPressed)
            {
                return;
            }

            if (scene is Level level && switches.Count > 0)
            {
                level.Session.SetFlag(switches[0].GroupFlag(), true);
            }

            foreach (NexusTouchSwitch touchSwitch in switches)
            {
                touchSwitch.Finish();
            }

            foreach (Entity entity in scene.Tracker.GetEntities<NexusGateCubeBlock>())
            {
                NexusGateCubeBlock gate = (NexusGateCubeBlock)entity;
                if (gate.Group != group)
                {
                    continue;
                }

                if (gate.invert)
                {
                    gate.Close();
                }
                else
                {
                    gate.Open();
                }
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            Audio.Play("event:/game/general/passage_closed_behind", Center);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            Audio.Play("event:/game/general/passage_closed_behind", Center);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            // A group with no switches and invert set should start solid, not stuck open.
            if (!startOpen && invert)
            {
                Collidable = true;
            }
        }

        public override void Update()
        {
            base.Update();

            openLerp = Calc.Approach(openLerp, IsOpen ? 1f : 0f, Engine.DeltaTime / openTime);

            // Stop colliding as soon as the cube has visibly pulled back.
            bool shouldCollide = openLerp < 0.5f;
            if (shouldCollide != Collidable && !(shouldCollide && CollideCheck<Player>()))
            {
                Collidable = shouldCollide;
            }
        }

        public override void Render()
        {
            if (openLerp >= 0.999f)
            {
                Placeholder.CubeOutline(X, Y, Width, Height, depthOffset, OpenEdge * 0.35f);
                return;
            }

            // Retracting: shrink toward the back face so it reads as sliding into the wall.
            float shrink = openLerp * 0.5f;
            float w = Width * (1f - shrink);
            float h = Height * (1f - shrink);
            float x = X + (Width - w) / 2f + depthOffset * openLerp;
            float y = Y + (Height - h) / 2f - depthOffset * openLerp;

            float alpha = 1f - openLerp * 0.6f;

            Placeholder.Cube(x, y, w, h, depthOffset, ClosedFront * alpha, ClosedSide * alpha, Edge * alpha);

            if (openLerp > 0f)
            {
                Placeholder.CubeOutline(X, Y, Width, Height, depthOffset, OpenEdge * (openLerp * 0.5f));
            }
        }
        #endregion
    }
}
