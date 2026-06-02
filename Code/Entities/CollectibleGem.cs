namespace Celeste.Entities {
    /// <summary>
    /// Collectible gem with different colors and point values
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CollectibleGem")]
    [HotReloadable]
    public class CollectibleGem : Entity {
        public enum GemColor {
            Red,
            Blue,
            Green,
            Yellow,
            Purple,
            Orange,
            Pink,
            White
        }

        private GemColor gemColor;
        private int pointValue;
        private bool collected;
        private Sprite sprite;
        private Wiggler wiggler;
        private VertexLight light;
        private SineWave floatSine;
        private SineWave rotationSine;
        private BloomPoint bloom;
        private SoundSource collectSound;
        private float startY;

        public bool IsCollected => collected;
        public int PointValue => pointValue;

        public CollectibleGem(EntityData data, Vector2 offset) {
            Position = data.Position + offset;
            startY = Position.Y;

            string colorString = data.Attr(nameof(gemColor), "blue");
            if (!System.Enum.TryParse(colorString, true, out gemColor)) {
                gemColor = GemColor.Blue;
            }

            pointValue = data.Int(nameof(pointValue), getDefaultPointValue(gemColor));
            collected = false;

            setupSprite();
            setupComponents();
            setupColliders();

            Depth = -100;
        }

        private int getDefaultPointValue(GemColor color) {
            switch (color) {
                case GemColor.Red: return 10;
                case GemColor.Blue: return 15;
                case GemColor.Green: return 20;
                case GemColor.Yellow: return 25;
                case GemColor.Purple: return 30;
                case GemColor.Orange: return 35;
                case GemColor.Pink: return 40;
                case GemColor.White: return 50;
                default: return 10;
            }
        }

        private void setupSprite() {
            string spriteName = $"gem_{gemColor.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName)) {
                sprite = GFX.SpriteBank.Create(spriteName);
            } else {
                // Fallback sprite using strawberry sprite
                sprite = new Sprite(GFX.Game, "collectables/strawberry/");
                sprite.AddLoop("idle", "normal", 0.1f);
                sprite.AddLoop(nameof(collect), "normal", 0.05f);
            }

            Add(sprite);
            sprite.Play("idle");

            // Apply gem color
            sprite.Color = getGemDisplayColor();
        }

        private Color getGemDisplayColor() {
            switch (gemColor) {
                case GemColor.Red: return Color.Red;
                case GemColor.Blue: return Color.Blue;
                case GemColor.Green: return Color.Green;
                case GemColor.Yellow: return Color.Yellow;
                case GemColor.Purple: return Color.Purple;
                case GemColor.Orange: return Color.Orange;
                case GemColor.Pink: return Color.Pink;
                case GemColor.White: return Color.White;
                default: return Color.White;
            }
        }

        private void setupComponents() {
            // Wiggler for bounce effect
            wiggler = Wiggler.Create(0.4f, 4f, null, false, false);
            Add(wiggler);

            // Floating sine wave
            floatSine = new SineWave(2f, 0f);
            Add(floatSine);

            // Rotation sine wave
            rotationSine = new SineWave(1.5f, 0f);
            Add(rotationSine);

            // Vertex light
            Color lightColor = getGemDisplayColor() * 0.8f;
            light = new VertexLight(lightColor, 1f, 16, 32);
            Add(light);

            // Bloom effect
            bloom = new BloomPoint(0.75f, 8f);
            Add(bloom);

            // Sound source
            collectSound = new SoundSource();
            Add(collectSound);
        }

        private void setupColliders() {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
        }

        public override void Update() {
            base.Update();

            if (collected) return;

            // Floating animation
            float floatOffset = floatSine.Value * 2f;
            Position = new Vector2(Position.X, startY + floatOffset);

            // Rotation animation
            sprite.Rotation = rotationSine.Value * 0.2f;

            // Scale with wiggler
            sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.2f);

            // Pulsing light
            light.Alpha = 0.8f + (float)System.Math.Sin(Scene.TimeActive * 4f) * 0.2f;

            // Shimmer particles occasionally
            if (Calc.Random.Next(30) == 0) {
                emitShimmerParticle();
            }

            // Check for player collision
            checkPlayerCollision();
        }

        private void emitShimmerParticle() {
            var level = Scene as Level;
            if (level != null) {
                Vector2 particlePos = Position + Calc.AngleToVector(Calc.Random.NextFloat() * 6.28f, 8f);
                Color particleColor = getGemDisplayColor() * 0.6f;
                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);
            }
        }

        private void checkPlayerCollision() {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player)) {
                collect();
            }
        }

        private void collect() {
            if (collected) return;

            collected = true;

            // Play collection sound
            Audio.Play("event:/game/general/strawberry_get", Position);

            // Visual effects
            wiggler.Start();
            sprite?.Play(nameof(collect));

            // Award points to player
            var level = Scene as Level;
            if (level?.Session != null) {
                // Add points to session data
                string scoreKey = "IngesteHelper_GemScore";
                int currentScore = 0;

                if (level.Session.GetFlag(scoreKey)) {
                    // Try to get existing score from session counters or flags
                    currentScore = level.Session.GetCounter(scoreKey);
                }

                level.Session.SetCounter(scoreKey, currentScore + pointValue);
            }

            // Particle explosion
            for (int i = 0; i < 8; i++) {
                Vector2 direction = Calc.AngleToVector(i * 0.785f, 20f);
                Vector2 particlePos = Position + direction * 0.5f;
                level?.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, getGemDisplayColor());
            }

            // Remove after animation
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.3f, true);
            tween.OnUpdate = (t) => {
                sprite.Scale = Vector2.One * (1f - t.Eased);
                light.Alpha = 1f - t.Eased;
            };
            tween.OnComplete = (t) => {
                RemoveSelf();
            };
            Add(tween);
        }

        public void Reset() {
            collected = false;
            sprite?.Play("idle");
            sprite.Scale = Vector2.One;
            light.Alpha = 1f;
            Visible = true;
        }

        public static int GetTotalGemScore(Session session) {
            if (session != null) {
                return session.GetCounter("IngesteHelper_GemScore");
            }
            return 0;
        }
    }
}



