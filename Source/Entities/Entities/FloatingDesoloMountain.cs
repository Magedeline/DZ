#nullable enable

namespace Celeste.Entities
{
    /// <summary>
    /// Renders a floating Desolo Zantas mountain above Mt. Celeste on the overworld,
    /// paired with a soft vortex halo for a cosmic "rift" look.
    /// </summary>
    [HotReloadable]
    public class FloatingDesoloMountain : Entity
    {
        private readonly MountainRenderer renderer;

        private Billboard? mountainBillboard;
        private Billboard? vortexBillboard;

        public bool Show { get; set; } = true;

        /// <summary>Base world position assigned by the overworld updater.</summary>
        public new Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>Offset from the assigned base position.</summary>
        public Vector3 Offset { get; set; } = new Vector3(1.1f, 3.25f, -0.35f);

        public float MountainScale { get; set; } = 2.95f;
        public float VortexScale { get; set; } = 4.9f;

        private float alpha;
        private float phase;

        public FloatingDesoloMountain(MountainRenderer renderer)
        {
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Depth = -9450;
        }

        public void LoadContent()
        {
            // Main floating mountain art.
            if (MTN.Mountain.Has("desolo_vortex_mountain"))
            {
                mountainBillboard = new Billboard(MTN.Mountain["desolo_vortex_mountain"], Vector3.Zero)
                {
                    Scale = Vector2.One * MountainScale,
                    Color = Color.White
                };
                Add(mountainBillboard);
            }
            else
            {
                IngesteLogger.Warn("FloatingDesoloMountain: missing MTN texture 'desolo_vortex_mountain'");
            }

            // Vortex halo behind the mountain.
            if (MTN.Mountain.Has("void"))
            {
                vortexBillboard = new Billboard(MTN.Mountain["void"], Vector3.Zero)
                {
                    Scale = Vector2.One * VortexScale,
                    Color = new Color(115, 70, 200, 90)
                };
                Add(vortexBillboard);
            }
            else
            {
                IngesteLogger.Warn("FloatingDesoloMountain: missing MTN texture 'void'");
            }
        }

        public override void Update()
        {
            base.Update();

            float dt = Engine.DeltaTime;
            phase += dt;
            alpha = Calc.Approach(alpha, Show ? 1f : 0f, dt * 2.2f);

            float bob = (float)Math.Sin(phase * 1.0f) * 0.1f;
            float pulse = 1f + 0.09f * (float)Math.Sin(phase * 0.95f);

            Vector3 worldPos = Position + Offset + new Vector3(0f, bob, 0f);

            if (vortexBillboard != null)
            {
                vortexBillboard.Position = worldPos + new Vector3(0f, 0.18f, 0f);
                vortexBillboard.Scale = Vector2.One * (VortexScale * pulse);
                vortexBillboard.Color = new Color(160, 95, 245, (byte)(105 * alpha));
            }

            if (mountainBillboard != null)
            {
                mountainBillboard.Position = worldPos;
                mountainBillboard.Scale = Vector2.One * MountainScale;
                mountainBillboard.Color = Color.White * alpha;
            }
        }
    }
}
