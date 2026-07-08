namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/Voidstarberry")]
    [Monocle.Tracked]
    [RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    public class VoidstarBerry : Actor, IStrawberry
    {
        public static ParticleType PVoidGlow;
        public static ParticleType PVoidSwirl;
        public static ParticleType PVoidCollectGlow;
        public static ParticleType PVoidDistortion;

        private Monocle.Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private float wobble = 0f;
        private Vector2 start;
        private float collectTimer;
        private bool collected;
        private EntityID id;
        private float voidPulseTimer = 0f;
        private float distortionTimer = 0f;

        public int CheckpointId { get; private set; } = -1;
        public int Order { get; private set; } = -1;

        public VoidstarBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            CheckpointId = data.Int("checkpointID", -1);
            Order = data.Int("order", -1);

            Depth = -100;
            Collider = new Monocle.Hitbox(14f, 14f, -7f, -10f);
            start = Position;

            Add(sprite = GFX.SpriteBank.Create("voidstarberry"));
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Calc.HexToColor("330033"), 0.6f, 25, 35));
            Add(bloom = new BloomPoint(0.8f, 18f));
            
            // Dark void coloring
            sprite.Color = Calc.HexToColor("220022");
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
                wobble += Engine.DeltaTime * 2.5f;
                sprite.Y = (float)Math.Sin(wobble) * 1.8f;
                
                // Void pulsing effect - darker and more ominous
                voidPulseTimer += Engine.DeltaTime * 1.5f;
                float voidPulse = (float)Math.Sin(voidPulseTimer) * 0.4f + 0.6f;
                
                // Create void distortion effect
                distortionTimer += Engine.DeltaTime * 3f;
                float distortion = (float)Math.Sin(distortionTimer) * 0.2f + 0.8f;
                
                light.Alpha = voidPulse * 0.7f;
                bloom.Alpha = voidPulse * 0.5f;
                
                // Darker color cycling between deep purples and blacks
                Color voidColor = GetVoidColor(voidPulseTimer);
                sprite.Color = Color.Lerp(voidColor, Color.Black, 0.3f);
                light.Color = voidColor;
                
                // Emit ominous void particles occasionally
                if (Scene is Level level && Engine.Scene.OnInterval(0.15f))
                {
                    if (PVoidGlow != null)
                    {
                        level.ParticlesFG?.Emit(PVoidGlow, 1, Position + new Vector2(Calc.Random.Range(-10f, 10f), Calc.Random.Range(-10f, 10f)), Vector2.One * 6f);
                    }
                    
                    // Rare void distortion particles
                    if (Engine.Scene.OnInterval(0.5f) && PVoidDistortion != null)
                    {
                        level.ParticlesBG?.Emit(PVoidDistortion, 1, Position, Vector2.One * 12f);
                    }
                }
            }

            if (collected)
            {
                collectTimer += Engine.DeltaTime;
                if (collectTimer > 0.6f)
                {
                    RemoveSelf();
                }
            }

            base.Update();
        }

        private Color GetVoidColor(float time)
        {
            // Cycle between deep purples, dark blues, and void blacks
            float cycle = (time % (float)(Math.PI * 4)) / (float)(Math.PI * 4);
            
            if (cycle < 0.33f)
            {
                // Deep purple phase
                return Color.Lerp(Calc.HexToColor("330033"), Calc.HexToColor("660066"), cycle * 3f);
            }
            else if (cycle < 0.66f)
            {
                // Dark blue-purple phase
                return Color.Lerp(Calc.HexToColor("660066"), Calc.HexToColor("330066"), (cycle - 0.33f) * 3f);
            }
            else
            {
                // Back to deep purple
                return Color.Lerp(Calc.HexToColor("330066"), Calc.HexToColor("330033"), (cycle - 0.66f) * 3f);
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected)
                return;

            // Ominous void collection sound
            Audio.Play("event:/game/general/strawberry_get", Position, "colour", 3f);
            Audio.Play("event:/new_content/game/10_farewell/fakeheart_get", Position); // Add void-like sound
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

            // Visual effects
            wiggler.Start();
            
            Add(new Coroutine(collectRoutine(player)));
        }

        private System.Collections.IEnumerator collectRoutine(global::Celeste.Player player)
        {
            var level = Scene as Level;
            
            // Ensure particles are initialized before using them
            if (PVoidGlow == null || PVoidSwirl == null || PVoidCollectGlow == null || PVoidDistortion == null)
            {
                LoadParticles();
            }
            
            // Dramatic void implosion effects
            level?.ParticlesFG?.Emit(PVoidSwirl, 12, Position, Vector2.One * 20f);
            level?.ParticlesBG?.Emit(PVoidDistortion, 8, Position, Vector2.One * 25f);
            level?.ParticlesFG?.Emit(PVoidGlow, 15, Position, Vector2.One * 15f);
            
            // Screen effects for void collection
            level?.Shake(0.4f);
            
            // Tween to player with void trail and distortion
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.6f, true);
            tween.OnUpdate = delegate(Tween t)
            {
                Position = Vector2.Lerp(start, player.Center, t.Eased);
                
                // Emit void trail particles during movement
                if (level != null && t.Percent > 0.1f)
                {
                    Color trailColor = GetVoidColor(voidPulseTimer + t.Percent * 15f);
                    level.ParticlesFG?.Emit(PVoidGlow, 2, Position, Vector2.One * 8f);
                    
                    // Distortion trail effect
                    if (t.Percent > 0.3f)
                    {
                        level.ParticlesBG?.Emit(PVoidDistortion, 1, Position, Vector2.One * 10f);
                    }
                }
            };
            
            Add(tween);
            
            yield return 0.6f;
            
            // Final void consumption effects - reality tears
            level?.ParticlesFG?.Emit(PVoidCollectGlow, 25, player.Center, Vector2.One * 12f);
            level?.Shake(0.5f);
            
            // Brief screen distortion effect
            yield return 0.1f;
        }

        public static void LoadParticles()
        {
            PVoidGlow = new ParticleType
            {
                Size = 1.4f,
                Color = Calc.HexToColor("330033"),
                Color2 = Calc.HexToColor("660066"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 2.5f,
                SpeedMin = 4f,
                SpeedMax = 12f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.5f,
                Acceleration = new Vector2(0f, -5f)
            };
            
            PVoidSwirl = new ParticleType
            {
                Size = 2.2f,
                Color = Calc.HexToColor("220022"),
                Color2 = Calc.HexToColor("440044"),
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 3.5f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -15f),
                SpeedMultiplier = 0.3f
            };
            
            PVoidDistortion = new ParticleType
            {
                Size = 3.0f,
                Color = Calc.HexToColor("110011"),
                Color2 = Calc.HexToColor("330066"),
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 8f,
                SpeedMax = 16f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.2f
            };
            
            PVoidCollectGlow = new ParticleType(PVoidGlow)
            {
                Size = 2.5f,
                SpeedMin = 15f,
                SpeedMax = 35f,
                LifeMin = 2.0f,
                LifeMax = 3.0f,
                Color = Calc.HexToColor("440044"),
                Color2 = Calc.HexToColor("110011")
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




