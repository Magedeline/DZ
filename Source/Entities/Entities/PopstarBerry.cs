using DZ;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/PopstarBerry")]
    [Monocle.Tracked]
    [RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    public class PopstarBerry : Actor, IStrawberry
    {
        public static ParticleType PStarGlow;
        public static ParticleType PStarBurst;
        public static ParticleType PStarCollectGlow;

        private Monocle.Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private float wobble = 0f;
        private Vector2 start;
        private float collectTimer;
        private bool collected;
        private EntityID id;
        private float rainbowTimer = 0f;
        private string collectSound;
        private string customCollectSound;

        private bool unlocked;
        private int totalRequired;
        private int collectedRequired;
        private int requiredBerriesOverride;

        public int CheckpointId { get; private set; } = -1;
        public int Order { get; private set; } = -1;

        // Secondary audio event paths per mode
        private const string AudioElaborate  = "event:/DZ/game/general/strawberry_get";
        private const string AudioOriginalFx = "event:/game/general/strawberry_blue_touch";

        public PopstarBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            CheckpointId = data.Int("checkpointID", -1);
            Order = data.Int("order", -1);
            collectSound      = data.Attr("collectSound",       "Original");
            customCollectSound = data.Attr("customCollectSound", "");
            requiredBerriesOverride = data.Int("requiredBerries", 0);

            Depth = -100;
            Collider = new Monocle.Hitbox(14f, 14f, -7f, -10f);
            start = Position;

            Add(sprite = GFX.SpriteBank.Create("popstarberry"));
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Color.White, 1.0f, 20, 30));
            Add(bloom = new BloomPoint(1.2f, 15f));
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

                unlocked = ComputeUnlockState(level, out collectedRequired, out totalRequired);
                if (!unlocked)
                {
                    Collidable = false;
                    bloom.Visible = false;
                    light.Visible = false;
                    sprite.Color = Color.Gray * 0.4f;
                }
            }
        }

        public override void Update()
        {
            Level level = Scene as Level;

            if (!collected)
            {
                if (!unlocked)
                {
                    // Hologram appearance until the unlock condition is met.
                    wobble += Engine.DeltaTime * 4f;
                    sprite.Y = (float)Math.Sin(wobble) * 2f;

                    float progress = totalRequired > 0 ? (float)collectedRequired / totalRequired : 0f;
                    sprite.Color = Color.Lerp(Color.Black, Color.White, 0.3f + progress * 0.5f) * (0.3f + progress * 0.5f);

                    if (ComputeUnlockState(level, out collectedRequired, out totalRequired))
                    {
                        unlocked = true;
                        Collidable = true;
                        bloom.Visible = true;
                        light.Visible = true;
                        sprite.Color = Color.White;
                        Audio.Play("event:/game/general/seed_reappear", Position);
                    }
                }
                else
                {
                    wobble += Engine.DeltaTime * 4f;
                    sprite.Y = (float)Math.Sin(wobble) * 2f;

                    // Rainbow cycling effect
                    rainbowTimer += Engine.DeltaTime * 3f;
                    Color rainbowColor = GetRainbowColor(rainbowTimer);
                    sprite.Color = rainbowColor;
                    light.Color = rainbowColor;

                    // Subtle star particles
                    if (level != null && Engine.Scene.OnInterval(0.1f))
                    {
                        if (PStarGlow != null)
                        {
                            level.ParticlesFG?.Emit(PStarGlow, 1, Position + new Vector2(Calc.Random.Range(-8f, 8f), Calc.Random.Range(-8f, 8f)), Vector2.One * 4f);
                        }
                    }
                }
            }

            if (collected)
            {
                collectTimer += Engine.DeltaTime;
                if (collectTimer > 0.5f)
                {
                    RemoveSelf();
                }
            }

            base.Update();
        }

        private Color GetRainbowColor(float time)
        {
            float hue = (time % (float)(Math.PI * 2)) / (float)(Math.PI * 2);
            return HSVToColor(hue, 0.8f, 1.0f);
        }

        private Color HSVToColor(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return new Color(v, t, p);
                case 1: return new Color(q, v, p);
                case 2: return new Color(p, v, t);
                case 3: return new Color(p, q, v);
                case 4: return new Color(t, p, v);
                case 5: return new Color(v, p, q);
                default: return Color.White;
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected || !unlocked)
                return;

            collected = true;
            Collidable = false;

            // Primary collect sound (always)
            Audio.Play("event:/DZ/game/general/strawberry_get", Position, "colour", 5f);

            // Secondary audio event driven by collectSound mode
            switch (collectSound?.ToLowerInvariant())
            {
                case "elaborate":
                    Audio.Play(AudioElaborate, Position);
                    break;
                case "minimalist":
                    // No secondary sound
                    break;
                case "custom" when !string.IsNullOrEmpty(customCollectSound):
                    Audio.Play(customCollectSound, Position);
                    break;
                default: // "Original"
                    Audio.Play(AudioOriginalFx, Position);
                    break;
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            // Add to session
            var level = Scene as Level;
            if (level != null)
            {
                level.Session.Strawberries.Add(id);
                level.Session.UpdateLevelStartDashes();
                global::DZ.DZProgressionManager.RecordPopstarBerry(level, id.ToString());
            }

            // Visual effects
            wiggler.Start();

            Add(new Coroutine(CollectRoutine(player)));

            // Rainbow berry style "POYOFECT!" popup
            Scene.Add(new FloatingText("POYOFECT!", player.Center + new Vector2(0f, -16f), Color.Gold, 1.5f));
        }

        private System.Collections.IEnumerator CollectRoutine(global::Celeste.Player player)
        {
            var level = Scene as Level;
            
            // Ensure particles are initialized before using them
            if (PStarGlow == null || PStarBurst == null || PStarCollectGlow == null)
            {
                LoadParticles();
            }
            
            // Spectacular star burst effects
            level?.ParticlesFG?.Emit(PStarBurst, 15, Position, Vector2.One * 16f);
            level?.ParticlesBG?.Emit(PStarGlow, 12, Position, Vector2.One * 12f);
            
            // Tween to player with star trail
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 0.5f, true);
            tween.OnUpdate = delegate(Tween t)
            {
                Position = Vector2.Lerp(start, player.Center, t.Eased);
                // Emit rainbow trail particles
                if (level != null && t.Percent > 0.1f)
                {
                    Color trailColor = GetRainbowColor(rainbowTimer + t.Percent * 10f);
                    level.ParticlesFG?.Emit(PStarGlow, 2, Position, Vector2.One * 5f);
                }
            };
            
            Add(tween);
            
            yield return 0.5f;
            
            // Final collection effects - rainbow explosion
            level?.ParticlesFG?.Emit(PStarCollectGlow, 20, player.Center, Vector2.One * 10f);
            level?.Shake(0.2f);
        }

        private bool ComputeUnlockState(Level level, out int collected, out int total)
        {
            collected = level?.Session?.Strawberries?.Count ?? 0;
            total = GetTotalBerries(level);
            return total <= 0 || collected >= total;
        }

        private int GetTotalBerries(Level level)
        {
            if (requiredBerriesOverride > 0)
                return requiredBerriesOverride;

            if (level == null)
                return 0;

            var area = global::Celeste.AreaData.Get(level.Session.Area);
            int mode = (int)level.Session.Area.Mode;
            int totalInMap = 0;
            if (area != null && area.Mode != null && mode >= 0 && mode < area.Mode.Length)
            {
                totalInMap = area.Mode[mode].TotalStrawberries;
            }

            return Math.Max(0, totalInMap - CountPopstarBerriesInMap(level));
        }

        private int CountPopstarBerriesInMap(Level level)
        {
            int count = 0;
            var mapData = level.Session.MapData;
            if (mapData == null || mapData.Levels == null)
                return count;

            foreach (var levelData in mapData.Levels)
            {
                if (levelData.Entities == null)
                    continue;

                foreach (var entity in levelData.Entities)
                {
                    if (entity.Name == "DZ/PopstarBerry")
                        count++;
                }
            }

            return count;
        }

        public override void Render()
        {
            base.Render();

            if (!unlocked && !collected && totalRequired > 0 && collectedRequired > 0)
            {
                Level level = SceneAs<Level>();
                if (level != null)
                {
                    string text = $"{collectedRequired}/{totalRequired}";
                    Vector2 pos = Position - level.Camera.Position + new Vector2(0f, 20f);
                    ActiveFont.DrawOutline(text, pos, new Vector2(0.5f, 0f), Vector2.One * 0.6f, Color.White, 2f, Color.Black * 0.8f);
                }
            }
        }

        public static void LoadParticles()
        {
            PStarGlow = new ParticleType
            {
                Size = 1.3f,
                Color = Color.White,
                Color2 = Color.Yellow,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.6f,
                SpeedMin = 8f,
                SpeedMax = 16f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.7f
            };
            
            PStarBurst = new ParticleType
            {
                Size = 2.0f,
                Color = Color.White,
                Color2 = Color.Cyan,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 15f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 15f)
            };
            
            PStarCollectGlow = new ParticleType
            {
                Size = 1.8f,
                Color = Color.White,
                Color2 = Color.Magenta,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 2.5f,
                SpeedMin = 12f,
                SpeedMax = 28f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.9f
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




