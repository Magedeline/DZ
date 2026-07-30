namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// TouchTorchSwitch - Ruins wall torch that lights when the player touches it.
    /// Torches sharing a <c>group</c> form one puzzle: once every torch in the group
    /// is lit, the group's session flag is set and each torch locks into a "finished"
    /// state. Unlit torches relight on room reload unless <c>persistent</c> is set.
    ///
    /// Placeholder art: circle - dark when unlit, orange with a halo when lit.
    /// </summary>
    [CustomEntity("DZ/TouchTorchSwitch")]
    [Tracked]
    public class TouchTorchSwitch : Entity
    {
        #region Fields
        private static readonly Color UnlitFill = Calc.HexToColor("31281f");
        private static readonly Color UnlitOutline = Calc.HexToColor("6b5c46");
        private static readonly Color LitFill = Calc.HexToColor("ffb14e");
        private static readonly Color LitOutline = Calc.HexToColor("ffe6a8");
        private static readonly Color FinishedFill = Calc.HexToColor("fff6d8");

        public bool Lit { get; private set; }
        public bool Finished { get; private set; }
        public string Group { get; }

        private readonly string flag;
        private readonly bool requiresDash;
        private readonly bool persistent;
        private readonly float radius;

        private VertexLight light;
        private float glow;
        private float flicker;
        private Level level;
        #endregion

        #region Constructor
        public TouchTorchSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Group = data.Attr("group", "");
            flag = data.Attr("flag", "");
            requiresDash = data.Bool("requiresDash", false);
            persistent = data.Bool("persistent", false);
            radius = data.Float("radius", 8f);

            Collider = new Circle(radius);
            Depth = -100;

            Add(light = new VertexLight(Color.Orange, 0f, 16, 40));
        }
        #endregion

        #region Public API
        public void Light()
        {
            if (Lit)
            {
                return;
            }

            Lit = true;
            glow = 1f;
            Audio.Play(ChapterSfx.RuinsTouchSwitchAny, Position);
            CheckGroupComplete();
        }

        public void Unlight()
        {
            if (Finished)
            {
                return;
            }

            Lit = false;
            glow = 0f;
        }

        /// <summary>Locks this torch on once the whole group has been lit.</summary>
        public void Finish()
        {
            if (Finished)
            {
                return;
            }

            Finished = true;
            Lit = true;
            glow = 1f;
            Audio.Play(ChapterSfx.RuinsTouchSwitchLastOneshot, Position);
            level?.Shake(0.15f);
        }
        #endregion

        #region Private
        private string GroupFlag()
        {
            if (!string.IsNullOrEmpty(flag))
            {
                return flag;
            }

            return string.IsNullOrEmpty(Group) ? "torch_switch" : "torch_switch_" + Group;
        }

        private void CheckGroupComplete()
        {
            List<TouchTorchSwitch> siblings = new List<TouchTorchSwitch>();
            foreach (Entity entity in Scene.Tracker.GetEntities<TouchTorchSwitch>())
            {
                TouchTorchSwitch torch = (TouchTorchSwitch)entity;
                if (torch.Group == Group)
                {
                    siblings.Add(torch);
                }
            }

            foreach (TouchTorchSwitch torch in siblings)
            {
                if (!torch.Lit)
                {
                    return;
                }
            }

            level?.Session.SetFlag(GroupFlag(), true);
            foreach (TouchTorchSwitch torch in siblings)
            {
                torch.Finish();
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            if (persistent && level != null && level.Session.GetFlag(GroupFlag()))
            {
                Lit = true;
                Finished = true;
                glow = 1f;
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
                    Light();
                }
            }

            glow = Calc.Approach(glow, Lit ? 1f : 0f, Engine.DeltaTime * 4f);
            flicker += Engine.DeltaTime * 9f;

            light.Alpha = glow * (Finished ? 1f : 0.8f);
            light.Color = Finished ? FinishedFill : Color.Orange;
        }

        public override void Render()
        {
            base.Render();

            Color fill = Color.Lerp(UnlitFill, Finished ? FinishedFill : LitFill, glow);
            Color outline = Color.Lerp(UnlitOutline, LitOutline, glow);

            if (glow > 0f)
            {
                float halo = radius + 4f + (float)Math.Sin(flicker) * 1.5f;
                Draw.Circle(Position, halo, LitFill * (glow * 0.35f), 10);
            }

            Placeholder.Circle(Position, radius, fill, outline);

            // Flame nub so lit torches read at a glance.
            if (glow > 0.1f)
            {
                float bob = (float)Math.Sin(flicker * 1.7f) * 1.2f;
                Placeholder.FillCircle(Position + new Vector2(0f, -radius - 2f + bob), 2.5f * glow, LitOutline * glow);
            }
        }
        #endregion
    }
}
