using System;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Celeste.Entities
{
    /// <summary>
    /// Heart Staff collectible inspired by Kirby Star Allies.
    /// Collect all colored Heart Staffs to unlock the final chapter door.
    /// Colors: Red, Blue, Yellow, Green, Purple, Orange, Pink (matching Friend Hearts)
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/HeartStaff")]
    [Tracked(false)]
    public class HeartStaff : Entity
    {
        // Collection flag prefix for save data persistence
        public const string COLLECTED_FLAG_PREFIX = "heart_staff_collected_";
        public const string TOTAL_STAFFS_KEY = "heart_staff_total_collected";

        /// <summary>
        /// Heart Staff colors based on Kirby Star Allies Friend Hearts
        /// </summary>
        public enum StaffColor
        {
            Red,      // Fire/Power
            Blue,     // Water/Ice
            Yellow,   // Spark/Electric
            Green,    // Leaf/Nature
            Purple,   // Poison/Dark
            Orange,   // Beam/Light
            Pink      // Heart/Love (Final Staff)
        }

        // Staff properties
        private readonly StaffColor staffColor;
        private readonly string staffId;
        private readonly EntityID entityId;

        // Visual components
        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private SineWave floatSine;
        private SineWave glowSine;
        private ParticleType sparkleParticles;
        private Wiggler scaleWiggler;
        private Wiggler collectWiggler;

        // State
        private bool collected;
        private bool isGhost; // Already collected, showing ghost version
        private float particleTimer;
        private Vector2 startPosition;

        // Audio
        private SoundSource ambientSound;

        public bool IsCollected => collected;
        public StaffColor Color => staffColor;
        public string StaffId => staffId;

        public HeartStaff(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            staffId = data.Attr("staffId", "");
            entityId = new EntityID(data.Level.Name, data.ID);

            // Parse staff color from data
            string colorStr = data.Attr("staffColor", "red");
            if (!Enum.TryParse(colorStr, true, out staffColor))
            {
                staffColor = StaffColor.Red;
            }

            // Generate unique staff ID if not provided
            if (string.IsNullOrEmpty(staffId))
            {
                staffId = $"{staffColor}_{entityId.Key}";
            }

            Collider = new Hitbox(20f, 32f, -10f, -24f);
            Add(new PlayerCollider(OnPlayer));

            Depth = -100;
            Tag = Tags.TransitionUpdate;

            startPosition = Position;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = SceneAs<Level>();
            string collectedFlag = COLLECTED_FLAG_PREFIX + staffId;

            // Check if already collected
            collected = level?.Session.GetFlag(collectedFlag) ?? false;

            if (collected)
            {
                // Check if we should show ghost version
                bool showGhost = true; // Could be a data option
                if (showGhost)
                {
                    isGhost = true;
                    CreateVisuals();
                    Collidable = false;
                }
                else
                {
                    RemoveSelf();
                    return;
                }
            }
            else
            {
                CreateVisuals();
            }

            CreateParticles();
        }

        private void CreateVisuals()
        {
            // Create sprite based on color
            string spriteName = $"heartstaff_{staffColor.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                // Fallback to generic staff sprite
                sprite = GFX.SpriteBank.Create("heartstaff_red");
            }

            sprite.Play("idle");
            sprite.CenterOrigin();

            if (isGhost)
            {
                sprite.Color = XnaColor.White * 0.3f;
            }

            Add(sprite);

            // Light effect with color matching
            Color lightColor = GetStaffDisplayColor();
            light = new VertexLight(lightColor, isGhost ? 0.3f : 1f, 32, 64);
            Add(light);

            // Bloom effect
            bloom = new BloomPoint(isGhost ? 0.3f : 0.8f, 20f);
            Add(bloom);

            // Floating animation
            floatSine = new SineWave(0.5f, 0f);
            floatSine.Randomize();
            Add(floatSine);

            // Glow pulse animation
            glowSine = new SineWave(2f, 0f);
            Add(glowSine);

            // Scale wiggler for collection
            scaleWiggler = Wiggler.Create(0.5f, 4f, v => {
                if (sprite != null)
                    sprite.Scale = Vector2.One * (1f + v * 0.3f);
            });
            Add(scaleWiggler);

            collectWiggler = Wiggler.Create(1f, 2f);
            Add(collectWiggler);

            // Ambient sound for uncollected staffs
            if (!isGhost && !collected)
            {
                ambientSound = new SoundSource();
                Add(ambientSound);
                // Play subtle ambient loop
                ambientSound.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}");
            }
        }

        private void CreateParticles()
        {
            XnaColor particleColor = GetStaffDisplayColor();

            sparkleParticles = new ParticleType
            {
                Source = GFX.Game["particles/sparkle"],
                Color = particleColor,
                Color2 = XnaColor.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                Direction = (float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 3f,
                SpeedMin = 6f,
                SpeedMax = 24f,
                SpeedMultiplier = 0.01f,
                LifeMin = 0.6f,
                LifeMax = 1.4f
            };
        }

        private XnaColor GetStaffDisplayColor()
        {
            return staffColor switch
            {
                StaffColor.Red => Calc.HexToColor("ff4444"),
                StaffColor.Blue => Calc.HexToColor("4488ff"),
                StaffColor.Yellow => Calc.HexToColor("ffdd44"),
                StaffColor.Green => Calc.HexToColor("44ff88"),
                StaffColor.Purple => Calc.HexToColor("aa44ff"),
                StaffColor.Orange => Calc.HexToColor("ff8844"),
                StaffColor.Pink => Calc.HexToColor("ff88cc"),
                _ => XnaColor.White
            };
        }

        public override void Update()
        {
            base.Update();

            if (collected && !isGhost) return;

            // Floating animation
            float floatOffset = floatSine.Value * 4f;
            Position = new Vector2(startPosition.X, startPosition.Y + floatOffset);

            // Update light position
            if (light != null)
            {
                light.Position = Vector2.Zero;
            }

            if (bloom != null)
            {
                bloom.Position = Vector2.Zero;
            }

            // Pulsing glow effect
            if (!isGhost)
            {
                float glowPulse = 0.8f + glowSine.Value * 0.2f;
                if (light != null)
                    light.Alpha = glowPulse;
                if (bloom != null)
                    bloom.Alpha = glowPulse * 0.8f;
            }

            // Emit sparkle particles
            particleTimer += Engine.DeltaTime;
            if (particleTimer >= 0.15f && !isGhost)
            {
                particleTimer = 0f;
                EmitSparkleParticle();
            }

            // Sprite rotation for visual flair
            if (sprite != null && !isGhost)
            {
                sprite.Rotation = (float)Math.Sin(Scene.TimeActive * 1.5f) * 0.05f;
            }
        }

        private void EmitSparkleParticle()
        {
            Level level = SceneAs<Level>();
            if (level == null) return;

            Vector2 particlePos = Center + Calc.AngleToVector(Calc.Random.NextFloat() * (float)Math.PI * 2f, Calc.Random.Range(8f, 16f));
            level.ParticlesBG.Emit(sparkleParticles, 1, particlePos, Vector2.One * 2f);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected || isGhost) return;

            Collect(player);
        }

        private void Collect(global::Celeste.Player player)
        {
            collected = true;
            Collidable = false;

            Level level = SceneAs<Level>();
            if (level == null) return;

            // Set collection flag
            string collectedFlag = COLLECTED_FLAG_PREFIX + staffId;
            level.Session.SetFlag(collectedFlag, true);

            // Update total count
            int currentTotal = level.Session.GetCounter(TOTAL_STAFFS_KEY);
            level.Session.SetCounter(TOTAL_STAFFS_KEY, currentTotal + 1);

            // Start collection sequence
            Add(new Coroutine(CollectRoutine(player)));
        }

        private System.Collections.IEnumerator CollectRoutine(global::Celeste.Player player)
        {
            Level level = SceneAs<Level>();

            // Play collection sound
            Audio.Play("event:/game/general/crystalheart_collect", Position);

            // Stop ambient sound
            ambientSound?.Stop();

            // Freeze briefly
            CelesteGame.Freeze(0.1f);
            level?.Shake(0.3f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

            // Particle burst
            for (int i = 0; i < 20; i++)
            {
                float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                Vector2 particlePos = Center + Calc.AngleToVector(angle, Calc.Random.Range(4f, 12f));
                level?.ParticlesFG.Emit(sparkleParticles, particlePos, angle);
            }

            // Start scale wiggler
            scaleWiggler.Start();

            // Flash effect
            sprite.Color = XnaColor.White;
            yield return 0.1f;
            sprite.Color = GetStaffDisplayColor();

            // Rise up animation
            float startY = Y;
            float targetY = Y - 32f;
            float riseTime = 0f;

            while (riseTime < 0.5f)
            {
                riseTime += Engine.DeltaTime;
                float t = Ease.CubeOut(riseTime / 0.5f);
                Y = MathHelper.Lerp(startY, targetY, t);
                sprite.Scale = Vector2.One * (1f + t * 0.5f);
                yield return null;
            }

            // Spin faster
            sprite.Play("collect");

            // More particles during spin
            for (int i = 0; i < 30; i++)
            {
                float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                level?.ParticlesFG.Emit(sparkleParticles, Center, angle);
                yield return 0.02f;
            }

            // Fade out
            float fadeTime = 0f;
            while (fadeTime < 0.3f)
            {
                fadeTime += Engine.DeltaTime;
                float alpha = 1f - (fadeTime / 0.3f);
                sprite.Color = GetStaffDisplayColor() * alpha;
                if (light != null) light.Alpha = alpha;
                if (bloom != null) bloom.Alpha = alpha * 0.8f;
                yield return null;
            }

            // Show collection message
            string staffName = GetStaffName();
            int totalCollected = (level?.Session.GetCounter(TOTAL_STAFFS_KEY) ?? 0);
            
            // Display mini-textbox or floating text
            level?.Add(new HeartStaffCollectedDisplay(Center, staffColor, totalCollected));

            RemoveSelf();
        }

        private string GetStaffName()
        {
            return staffColor switch
            {
                StaffColor.Red => "Blazing Heart Staff",
                StaffColor.Blue => "Glacial Heart Staff",
                StaffColor.Yellow => "Spark Heart Staff",
                StaffColor.Green => "Nature Heart Staff",
                StaffColor.Purple => "Void Heart Staff",
                StaffColor.Orange => "Radiant Heart Staff",
                StaffColor.Pink => "Love Heart Staff",
                _ => "Heart Staff"
            };
        }

        /// <summary>
        /// Get the total number of collected Heart Staffs from save data
        /// </summary>
        public static int GetTotalCollected(Session session)
        {
            if (session == null) return 0;
            return session.GetCounter(TOTAL_STAFFS_KEY);
        }

        /// <summary>
        /// Check if a specific staff color has been collected
        /// </summary>
        public static bool IsStaffCollected(Session session, StaffColor color, string staffId)
        {
            if (session == null) return false;
            string flag = COLLECTED_FLAG_PREFIX + staffId;
            return session.GetFlag(flag);
        }
    }

    /// <summary>
    /// Floating display shown when collecting a Heart Staff
    /// </summary>
    public class HeartStaffCollectedDisplay : Entity
    {
        private readonly HeartStaff.StaffColor staffColor;
        private readonly int totalCollected;
        private float timer;
        private float alpha;

        public HeartStaffCollectedDisplay(Vector2 position, HeartStaff.StaffColor color, int total) : base(position)
        {
            staffColor = color;
            totalCollected = total;
            timer = 0f;
            alpha = 1f;
            Depth = -1000000;
            Tag = Tags.HUD | Tags.FrozenUpdate | Tags.TransitionUpdate;
        }

        public override void Update()
        {
            base.Update();

            timer += Engine.DeltaTime;

            // Fade in for first 0.3s, hold for 1.5s, fade out for 0.5s
            if (timer < 0.3f)
            {
                alpha = timer / 0.3f;
            }
            else if (timer < 1.8f)
            {
                alpha = 1f;
            }
            else if (timer < 2.3f)
            {
                alpha = 1f - ((timer - 1.8f) / 0.5f);
            }
            else
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();

            Level level = SceneAs<Level>();
            if (level == null) return;

            Vector2 screenPos = level.Camera.Position;
            Vector2 displayPos = new Vector2(960f, 200f); // Center-top of screen

            Color textColor = GetDisplayColor() * alpha;
            Color shadowColor = Color.Black * alpha * 0.5f;

            string staffName = GetStaffDisplayName();
            string countText = $"Heart Staffs: {totalCollected}/7";

            // Draw staff name
            ActiveFont.DrawOutline(
                staffName,
                displayPos,
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.2f,
                textColor,
                2f,
                shadowColor
            );

            // Draw count below
            ActiveFont.DrawOutline(
                countText,
                displayPos + new Vector2(0f, 40f),
                new Vector2(0.5f, 0.5f),
                Vector2.One * 0.8f,
                XnaColor.White * alpha,
                2f,
                shadowColor
            );
        }

        private XnaColor GetDisplayColor()
        {
            return staffColor switch
            {
                HeartStaff.StaffColor.Red => Calc.HexToColor("ff4444"),
                HeartStaff.StaffColor.Blue => Calc.HexToColor("4488ff"),
                HeartStaff.StaffColor.Yellow => Calc.HexToColor("ffdd44"),
                HeartStaff.StaffColor.Green => Calc.HexToColor("44ff88"),
                HeartStaff.StaffColor.Purple => Calc.HexToColor("aa44ff"),
                HeartStaff.StaffColor.Orange => Calc.HexToColor("ff8844"),
                HeartStaff.StaffColor.Pink => Calc.HexToColor("ff88cc"),
                _ => XnaColor.White
            };
        }

        private string GetStaffDisplayName()
        {
            return staffColor switch
            {
                HeartStaff.StaffColor.Red => "Blazing Heart Staff",
                HeartStaff.StaffColor.Blue => "Glacial Heart Staff",
                HeartStaff.StaffColor.Yellow => "Spark Heart Staff",
                HeartStaff.StaffColor.Green => "Nature Heart Staff",
                HeartStaff.StaffColor.Purple => "Void Heart Staff",
                HeartStaff.StaffColor.Orange => "Radiant Heart Staff",
                HeartStaff.StaffColor.Pink => "Love Heart Staff",
                _ => "Heart Staff"
            };
        }
    }
}

