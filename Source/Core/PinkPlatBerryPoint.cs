#nullable enable
namespace DZ
{
    [HotReloadable]
    internal class PinkPlatberryPoints : Entity
    {
        private readonly bool ghostberry;
        private readonly BloomPoint bloom;
        private DisplacementRenderer.Burst burst;
        private readonly Sprite sprite;
        private readonly VertexLight light;

        public PinkPlatberryPoints(Vector2 position, bool ghostberry, DisplacementRenderer.Burst burst)
          : base(position)
        {
            Add((Component)(this.sprite = DZModule.SpriteBank.Create("pinkplatinumberry")));
            Add((Component)(this.light = new VertexLight(Color.White, 1f, 16, 24)));
            Add((Component)(this.bloom = new BloomPoint(1f, 12f)));
            Depth = -2000100;
            this.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
            this.ghostberry = ghostberry;
            this.burst = burst;
        }

        public PinkPlatberryPoints(Vector2 position, bool ghostberry) : base(position)
        {
            Add((Component)(sprite = DZModule.SpriteBank.Create("pinkplatinumberry")));
            Add((Component)(light = new VertexLight(Color.White, 1f, 16, 24)));
            Add((Component)(bloom = new BloomPoint(1f, 12f)));
            Depth = -2000100;
            Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
            this.ghostberry = ghostberry;
            burst = new DisplacementRenderer.Burst(GFX.Game["util/displacementcircle"], position, Vector2.Zero, 0.3f); // Initialize _burst with required parameters
        }


        public override void Added(Scene scene)
        {
            this.sprite.Play("points");
            this.sprite.OnFinish = (System.Action<string>)(param1 => this.RemoveSelf());
            base.Added(scene);
            if (scene is Level level)
            {
                this.burst = level.Displacement.AddBurst(this.Position, 0.3f, 16f, 24f, 0.3f);
            }
        }

        public override void Update()
        {
            Level? scene = Scene as Level;
            if (scene != null && scene.Frozen)
            {
                burst.AlphaFrom = this.burst.AlphaTo = 0.0f;
                this.burst.Percent = burst.Duration;
            }
            else
            {
                base.Update();
                if (scene != null)
                {
                    Camera camera = scene.Camera;
                    Y -= 8f * Engine.DeltaTime;
                    X = Calc.Clamp(X, camera.Left + 8f, camera.Right - 8f);
                    Y = Calc.Clamp(Y, camera.Top + 8f, camera.Bottom - 8f);
                    light.Alpha = Calc.Approach(light.Alpha, 0.0f, Engine.DeltaTime * 4f);
                    bloom.Alpha = light.Alpha;
                    
                    if (PinkPlatinumBerryParticles.PPlatGlow != null && PinkPlatinumBerryParticles.PGhostGlow != null)
                    {
                        ParticleType type = ghostberry ? PinkPlatinumBerryParticles.PGhostGlow : PinkPlatinumBerryParticles.PPlatGlow;
                        if (Scene.OnInterval(0.05f))
                        {
                            if (sprite.Color == type.Color2)
                                sprite.Color = type.Color;
                            else
                                sprite.Color = type.Color2;
                        }
                        if (Scene.OnInterval(0.06f) && this.sprite.CurrentAnimationFrame > 11)
                        {
                            scene.ParticlesFG.Emit(type, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
                        }
                    }
                }
            }
        }
    }
}




