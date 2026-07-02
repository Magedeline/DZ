namespace Celeste.Entities
{
    /// <summary>
    /// Darkener Fountain entity - creates a fountain effect that darkens or transforms the environment
    /// </summary>
    [CustomEntity(ids: "DZ/DarkernerFountain")]
    [Tracked]
    [HotReloadable]
    public class DarkernerFountain : Entity
    {
        public enum FountainType
        {
            Chaos,
            Pure,
            Shadow,
            Void
        }

        public FountainType Type { get; private set; }
        public float ActivationRadius { get; private set; }
        public float Intensity { get; private set; }
        public float Duration { get; private set; }
        public int ParticleCount { get; private set; }
        public bool AutoActivate { get; private set; }
        public string RequiresFlag { get; private set; }
        public bool TransformsPlayer { get; private set; }
        public bool PersistentEffect { get; private set; }
        public string SoundEffect { get; private set; }

        private Sprite sprite;
        private bool isActive;
        private float activeTimer;
        private Level level;

        public static ParticleType P_DarkParticle;
        public static ParticleType P_ChaosParticle;
        public static ParticleType P_VoidParticle;

        static DarkernerFountain()
        {
            P_DarkParticle = new ParticleType
            {
                Color = Color.DarkSlateGray,
                Color2 = Color.Black,
                ColorMode = ParticleType.ColorModes.Blink,
                Size = 1f,
                SizeRange = 0.5f,
                Direction = -MathHelper.PiOver2,
                DirectionRange = MathHelper.PiOver4,
                SpeedMin = 20f,
                SpeedMax = 40f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                FadeMode = ParticleType.FadeModes.Late
            };

            P_ChaosParticle = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.Magenta,
                ColorMode = ParticleType.ColorModes.Fade,
                Size = 1.5f,
                SizeRange = 0.5f,
                Direction = -MathHelper.PiOver2,
                DirectionRange = MathHelper.Pi,
                SpeedMin = 30f,
                SpeedMax = 60f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                FadeMode = ParticleType.FadeModes.Linear
            };

            P_VoidParticle = new ParticleType
            {
                Color = Color.Black,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Choose,
                Size = 2f,
                SizeRange = 1f,
                Direction = -MathHelper.PiOver2,
                DirectionRange = MathHelper.PiOver4,
                SpeedMin = 10f,
                SpeedMax = 30f,
                LifeMin = 1f,
                LifeMax = 2f,
                FadeMode = ParticleType.FadeModes.Late
            };
        }

        public DarkernerFountain(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            // Parse fountain type
            string typeStr = data.Attr("fountainType", "Shadow");
            Type = Enum.TryParse<FountainType>(typeStr, true, out var parsed) ? parsed : FountainType.Shadow;

            // Read properties
            ActivationRadius = data.Float("activationRadius", 64f);
            Intensity = data.Float("intensity", 1f);
            Duration = data.Float("duration", 5f);
            ParticleCount = data.Int("particleCount", 30);
            AutoActivate = data.Bool("autoActivate", false);
            RequiresFlag = data.Attr("requiresFlag", "");
            TransformsPlayer = data.Bool("transformsPlayer", false);
            PersistentEffect = data.Bool("persistentEffect", false);
            SoundEffect = data.Attr("soundEffect", "event:/game/general/seed_poof");

            Depth = -5;
            Collider = new Circle(ActivationRadius);

            // Set up sprite
            SetupSprite();
        }

        private void SetupSprite()
        {
            string spritePath = Type switch
            {
                FountainType.Chaos => "objects/DZ/DZ/fountain_chaos",
                FountainType.Pure => "objects/DZ/DZ/fountain_pure",
                FountainType.Void => "objects/DZ/DZ/fountain_void",
                _ => "objects/DZ/DZ/fountain_darkener"
            };

            if (GFX.Game.Has(spritePath))
            {
                sprite = GFX.SpriteBank.Create("darkernerFountain");
                Add(sprite);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;

            if (AutoActivate)
            {
                Activate();
            }
        }

        public override void Update()
        {
            base.Update();

            // Check if flag is required and set
            if (!string.IsNullOrEmpty(RequiresFlag) && level != null)
            {
                if (!level.Session.GetFlag(RequiresFlag))
                {
                    isActive = false;
                    return;
                }
            }

            // Check for player proximity
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !isActive && Vector2.Distance(Position, player.Position) < ActivationRadius)
            {
                Activate();
            }

            // Update active state
            if (isActive)
            {
                activeTimer -= Engine.DeltaTime;
                
                // Spawn particles
                SpawnParticles();

                // Apply effect
                ApplyEffect(player);

                if (activeTimer <= 0 && !PersistentEffect)
                {
                    Deactivate();
                }
            }
        }

        public void Activate()
        {
            if (isActive)
                return;

            isActive = true;
            activeTimer = Duration;

            // Play sound
            if (!string.IsNullOrEmpty(SoundEffect))
            {
                Audio.Play(SoundEffect, Position);
            }

            sprite?.Play("activate");
        }

        public void Deactivate()
        {
            isActive = false;
            sprite?.Play("idle");
        }

        private void SpawnParticles()
        {
            if (Scene == null)
                return;

            ParticleType particleType = Type switch
            {
                FountainType.Chaos => P_ChaosParticle,
                FountainType.Void => P_VoidParticle,
                _ => P_DarkParticle
            };

            int spawnCount = (int)(ParticleCount * Intensity * Engine.DeltaTime);
            for (int i = 0; i < spawnCount; i++)
            {
                float angle = Calc.Random.NextFloat(MathHelper.TwoPi);
                float distance = Calc.Random.NextFloat(ActivationRadius * 0.5f);
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );

                (Scene as Level)?.ParticlesFG.Emit(particleType, Position + offset);
            }
        }

        private void ApplyEffect(global::Celeste.Player player)
        {
            if (player == null || level == null)
                return;

            // Apply screen darkening effect based on type
            float darkness = Type switch
            {
                FountainType.Shadow => 0.3f,
                FountainType.Void => 0.5f,
                FountainType.Chaos => 0.2f,
                _ => 0.1f
            };

            // Transform player if enabled
            if (TransformsPlayer && Vector2.Distance(Position, player.Position) < ActivationRadius * 0.5f)
            {
                // Set transformation flag
                level.Session.SetFlag($"darkener_transform_{Type.ToString().ToLower()}", true);
            }
        }

        public override void Render()
        {
            base.Render();

            // Fallback render if no sprite
            if (sprite == null)
            {
                Color color = Type switch
                {
                    FountainType.Chaos => Color.Purple,
                    FountainType.Pure => Color.White,
                    FountainType.Void => Color.Black,
                    _ => Color.DarkGray
                };

                Draw.Circle(Position, 16f, color, 8);
                Draw.Circle(Position, ActivationRadius, color * 0.3f, 2);
            }
        }
    }
}

