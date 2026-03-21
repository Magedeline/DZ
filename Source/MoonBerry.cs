namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/Moonberry")]
    [Monocle.Tracked]
    [RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    public class MoonBerry : Actor, IStrawberry
    {
        public static ParticleType PMoonGlow;
        public static ParticleType PMoonWingsBurst;
        public static ParticleType PMoonCollectGlow;

        private Monocle.Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private float wobble = 0f;
        private Vector2 start;
        private float collectTimer;
        private bool collected;
        private EntityID id;
        private float pulseTimer = 0f;

        public int CheckpointId { get; private set; } = -1;
        public int Order { get; private set; } = -1;

        public MoonBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            CheckpointId = data.Int("checkpointID", -1);
            Order = data.Int("order", -1);

            Depth = -100;
            Collider = new Monocle.Hitbox(14f, 14f, -7f, -10f);
            start = Position;

            Add(sprite = GFX.SpriteBank.Create("moonberry"));
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Calc.HexToColor("B8E6FF"), 0.8f, 18, 28));
            Add(bloom = new BloomPoint(0.6f, 14f));
            
            sprite.Color = Calc.HexToColor("E6F3FF");
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
                wobble += Engine.DeltaTime * 3f;
                sprite.Y = (float)Math.Sin(wobble) * 1.5f;
                
                // Moonberries have a gentle pulsing glow
                pulseTimer += Engine.DeltaTime * 2f;
                float pulse = (float)Math.Sin(pulseTimer) * 0.3f + 0.7f;
                light.Alpha = pulse;
                bloom.Alpha = pulse;
                sprite.Color = Color.Lerp(Calc.HexToColor("E6F3FF"), Calc.HexToColor("FFFFFF"), pulse * 0.5f);
            }

            if (collected)
            {
                collectTimer += Engine.DeltaTime;
                if (collectTimer > 0.35f)
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

            Audio.Play("event:/game/general/seed_poof", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            collected = true;
            Collidable = false;
            
            // Add to session
            var level = Scene as Level;
            if (level != null)
            {
                level.Session.Strawberries.Add(id);
                level.Session.UpdateLevelStartDashes();
            }

            // Visual effects
            wiggler.Start();
            
            Add(new Coroutine(collectRoutine(player)));
        }

        private System.Collections.IEnumerator collectRoutine(global::Celeste.Player player)
        {
            var level = Scene as Level;
            
            // Ensure particles are initialized before using them
            if (PMoonGlow == null || PMoonWingsBurst == null || PMoonCollectGlow == null)
            {
                LoadParticles();
            }
            
            // Gentle moonlight particle effects
            level?.ParticlesFG?.Emit(PMoonGlow, 8, Position, Vector2.One * 10f);
            level?.ParticlesBG?.Emit(PMoonWingsBurst, 6, Position, Vector2.One * 14f);
            
            // Tween to player with ethereal movement
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.35f, true);
            tween.OnUpdate = delegate(Tween t)
            {
                Position = Vector2.Lerp(start, player.Center, t.Eased);
                // Emit subtle trail particles
                if (level != null && t.Percent > 0.1f)
                {
                    level.ParticlesFG?.Emit(PMoonGlow, 1, Position, Vector2.One * 3f);
                }
            };
            
            Add(tween);
            
            yield return 0.35f;
            
            // Final collection effects - ethereal and mystical
            level?.ParticlesFG?.Emit(PMoonCollectGlow, 12, player.Center, Vector2.One * 6f);
        }

        public static void LoadParticles()
        {
            PMoonGlow = new ParticleType
            {
                Size = 1.2f,
                Color = Calc.HexToColor("B8E6FF"),
                Color2 = Calc.HexToColor("E6F3FF"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 1.8f,
                SpeedMin = 6f,
                SpeedMax = 14f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.6f
            };
            
            PMoonWingsBurst = new ParticleType
            {
                Size = 1.5f,
                Color = Calc.HexToColor("B8E6FF"),
                Color2 = Calc.HexToColor("FFFFFF"),
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 8f,
                SpeedMax = 18f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -10f)
            };
            
            PMoonCollectGlow = new ParticleType(PMoonGlow)
            {
                Size = 1.8f,
                SpeedMin = 10f,
                SpeedMax = 22f,
                LifeMin = 1.2f,
                LifeMax = 2.0f
            };
        }

        public void OnCollect()
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



