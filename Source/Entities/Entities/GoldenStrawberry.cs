namespace Celeste.Entities
{
    [CustomEntity("DZ/Goldenstrawberry")]
    [Monocle.Tracked]
    [HotReloadable]
    public class GoldenStrawberry : Actor
    {
        public static ParticleType PGoldenGlow;
        public static ParticleType PGoldenWingsBurst;
        public static ParticleType PGoldenCollectGlow;

        private Monocle.Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private float wobble = 0f;
        private Vector2 start;
        private float collectTimer;
        private bool collected;
        private EntityID id;
        private float flashTimer = 0f;

        public int CheckpointId { get; private set; } = -1;
        public int Order { get; private set; } = -1;

        public GoldenStrawberry(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            CheckpointId = data.Int("checkpointID", -1);
            Order = data.Int("order", -1);

            Depth = -100;
            Collider = new Monocle.Hitbox(14f, 14f, -7f, -10f);
            start = Position;

            Add(sprite = GFX.SpriteBank.Create("goldberry"));
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Color.Gold, 1.2f, 20, 32));
            Add(bloom = new BloomPoint(1.5f, 16f));
            
            sprite.Color = Color.Gold;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected
            var level = scene as Level;
            if (level != null)
            {
                var session = level.Session;
                if (session.Strawberries.Contains(id))
                {
                    RemoveSelf();
                    return;
                }
            }
        }

        public override void Update()
        {
            if (!collected)
            {
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = (float)Math.Sin(wobble) * 2f;
                
                // Golden strawberries flash periodically
                flashTimer += Engine.DeltaTime;
                if (flashTimer > 1f)
                {
                    flashTimer = 0f;
                    sprite.Color = Color.White;
                    light.Color = Color.White;
                }
                else if (flashTimer > 0.9f)
                {
                    sprite.Color = Color.Gold;
                    light.Color = Color.Gold;
                }
            }

            if (collected)
            {
                collectTimer += Engine.DeltaTime;
                if (collectTimer > 0.4f)
                {
                    RemoveSelf();
                }
            }

            base.Update();
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected)
                return;

            Audio.Play("event:/new_content/game/10_farewell/golden_strawberry_get", Position);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);

            collected = true;
            Collidable = false;
            
            // Add to session
            var level = Scene as Level;
            if (level != null)
            {
                level.Session.Strawberries.Add(id);
                level.Session.UpdateLevelStartDashes();
            }

            // Record for large heart gem door counter
            DZ.DZProgressionManager.RecordGoldenStrawberry(level, this.id.ToString());

            // Visual effects
            wiggler.Start();
            
            Add(new Coroutine(collectRoutine(player)));
        }

        private System.Collections.IEnumerator collectRoutine(global::Celeste.Player player)
        {
            var level = Scene as Level;
            
            // Ensure particles are initialized before using them
            if (PGoldenGlow == null || PGoldenWingsBurst == null || PGoldenCollectGlow == null)
            {
                LoadParticles();
            }
            
            // Intense golden particle effects
            level?.ParticlesFG?.Emit(PGoldenGlow, 10, Position, Vector2.One * 12f);
            level?.ParticlesBG?.Emit(PGoldenWingsBurst, 8, Position, Vector2.One * 16f);
            
            // Tween to player with golden trail
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.4f, true);
            tween.OnUpdate = delegate(Tween t)
            {
                Position = Vector2.Lerp(start, player.Center, t.Eased);
                // Emit trail particles during movement
                if (level != null && t.Percent > 0.1f)
                {
                    level.ParticlesFG?.Emit(PGoldenGlow, 1, Position, Vector2.One * 4f);
                }
            };
            
            Add(tween);
            
            yield return 0.4f;
            
            // Final collection effects - more dramatic than regular strawberry
            level?.ParticlesFG?.Emit(PGoldenCollectGlow, 15, player.Center, Vector2.One * 8f);
            level?.Shake(0.3f);
        }

        public static void LoadParticles()
        {
            PGoldenGlow = new ParticleType
            {
                Size = 1.5f,
                Color = Color.Gold,
                Color2 = Color.Yellow,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.2f,
                LifeMax = 2.0f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.8f
            };
            
            PGoldenWingsBurst = new ParticleType
            {
                Size = 2f,
                Color = Color.Gold,
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 20f)
            };
            
            PGoldenCollectGlow = new ParticleType(PGoldenGlow)
            {
                Size = 2f,
                SpeedMin = 15f,
                SpeedMax = 30f,
                LifeMin = 1.5f,
                LifeMax = 2.5f
            };
        }

        internal void OnCollect()
        {
            // Find the player in the scene
            var player = Scene?.Tracker?.GetEntity<global::Celeste.Player>();
            if (player != null && !collected)
            {
                OnPlayer(player);
            }
        }
    }
}



