using System.Runtime.CompilerServices;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger for the last room that enables gravity, room wrapping, sparkling stars, and rainbow effects
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/LastRoomTrigger")]
    [HotReloadable]
    public class LastRoomTrigger : Trigger
    {
        private bool enableGravity;
        private bool enableRoomWrapper;
        private bool enableSparklingStars;
        private bool enableRainbow;
        private float starIntensity;
        private float rainbowStrength;
        private bool triggerOnce;
        private bool hasTriggered;
        private Level level;
        private bool isWrappingEnabled;
        private bool isGravityEnabled;
        private float rainbowHue;

        public LastRoomTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            enableGravity = data.Bool("enableGravity", true);
            enableRoomWrapper = data.Bool("enableRoomWrapper", true);
            enableSparklingStars = data.Bool("enableSparklingStars", true);
            enableRainbow = data.Bool("enableRainbow", true);
            starIntensity = data.Float("starIntensity", 1.0f);
            rainbowStrength = data.Float("rainbowStrength", 1.0f);
            triggerOnce = data.Bool("triggerOnce", true);
            hasTriggered = false;
            isWrappingEnabled = false;
            isGravityEnabled = false;
            rainbowHue = 0f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (!triggerOnce || !hasTriggered)
            {
                ActivateEffects();
                hasTriggered = true;
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);

            if (!triggerOnce)
            {
                DeactivateEffects();
            }
        }

        private void ActivateEffects()
        {
            if (level == null) return;

            if (enableRoomWrapper && !isWrappingEnabled)
            {
                isWrappingEnabled = true;
                // Add room wrapper component
                Add(new RoomWrapperComponent(level));
            }

            if (enableGravity && !isGravityEnabled)
            {
                isGravityEnabled = true;
                // Add gravity component
                Add(new GravityComponent(level));
            }

            if (enableSparklingStars)
            {
                // Create initial burst of stars
                CreateStarBurst();
            }

            if (enableRainbow)
            {
                // Enable rainbow effects on background
                EnableRainbowEffects();
            }

            // Play activation sound
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
        }

        private void DeactivateEffects()
        {
            if (enableRoomWrapper && isWrappingEnabled)
            {
                isWrappingEnabled = false;
                // Remove room wrapper component
                var wrapper = Get<RoomWrapperComponent>();
                if (wrapper != null)
                {
                    Remove(wrapper);
                }
            }

            if (enableGravity && isGravityEnabled)
            {
                isGravityEnabled = false;
                // Remove gravity component
                var gravity = Get<GravityComponent>();
                if (gravity != null)
                {
                    Remove(gravity);
                }
            }

            if (enableRainbow)
            {
                DisableRainbowEffects();
            }
        }

        private void CreateStarBurst()
        {
            if (level == null) return;

            int starCount = (int)(30 * starIntensity);
            for (int i = 0; i < starCount; i++)
            {
                Vector2 starPos = Position + new Vector2(
                    Calc.Random.Range(-Width / 2, Width / 2),
                    Calc.Random.Range(-Height / 2, Height / 2)
                );

                level.ParticlesFG.Emit(ParticleTypes.Dust, starPos, Color.Gold);
            }
        }

        private void EnableRainbowEffects()
        {
            if (level == null) return;

            // Find and enable rainbow backdrops
            foreach (var backdrop in level.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = true;
                    rainbowBg.Alpha = rainbowStrength;
                }
            }

            foreach (var backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = true;
                    rainbowBg.Alpha = rainbowStrength;
                }
            }

            // Set session flag
            level.Session.SetFlag("last_room_rainbow_enabled", true);
        }

        private void DisableRainbowEffects()
        {
            if (level == null) return;

            // Find and disable rainbow backdrops
            foreach (var backdrop in level.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = false;
                    rainbowBg.Alpha = 0f;
                }
            }

            foreach (var backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = false;
                    rainbowBg.Alpha = 0f;
                }
            }

            // Clear session flag
            level.Session.SetFlag("last_room_rainbow_enabled", false);
        }

        public override void Update()
        {
            base.Update();

            // Continuously generate sparkling stars if enabled
            if (enableSparklingStars && level != null)
            {
                if (Calc.Random.Chance(0.1f * starIntensity))
                {
                    Vector2 starPos = Position + new Vector2(
                        Calc.Random.Range(-Width / 2, Width / 2),
                        Calc.Random.Range(-Height / 2, Height / 2)
                    );

                    level.ParticlesFG.Emit(ParticleTypes.Dust, starPos, Color.Gold);
                }
            }

            // Update rainbow hue for dynamic effect
            if (enableRainbow)
            {
                rainbowHue = (rainbowHue + 0.01f) % 1f;
            }
        }

        /// <summary>
        /// Component that handles room wrapping functionality
        /// </summary>
        private class RoomWrapperComponent : Component
        {
            private Level level;

            public RoomWrapperComponent(Level level) : base(true, true)
            {
                this.level = level;
            }

            public override void Update()
            {
                base.Update();

                Player player = level.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    // Vertical wrapping
                    if (player.Top > level.Camera.Bottom + 12f)
                    {
                        player.Bottom = level.Camera.Top - 4f;
                    }
                    else if (player.Bottom < level.Camera.Top - 4f)
                    {
                        player.Top = level.Camera.Bottom + 12f;
                    }

                    // Horizontal wrapping
                    if (player.Right > level.Camera.Right + 12f)
                    {
                        player.Left = level.Camera.Left - 4f;
                    }
                    else if (player.Left < level.Camera.Left - 4f)
                    {
                        player.Right = level.Camera.Right + 12f;
                    }
                }
            }
        }

        /// <summary>
        /// Component that handles gravity effects
        /// </summary>
        private class GravityComponent : Component
        {
            private Level level;
            private float gravityMultiplier;

            public GravityComponent(Level level) : base(true, true)
            {
                this.level = level;
                this.gravityMultiplier = 1.2f; // Slightly stronger gravity
            }

            public override void Update()
            {
                base.Update();

                Player player = level.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    // Apply enhanced gravity effect
                    if (player.Speed.Y < 0)
                    {
                        player.Speed.Y += 20f * Engine.DeltaTime * gravityMultiplier;
                    }
                    else
                    {
                        player.Speed.Y += 30f * Engine.DeltaTime * gravityMultiplier;
                    }
                }
            }
        }
    }
}
