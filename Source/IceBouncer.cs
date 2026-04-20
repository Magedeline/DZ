namespace Celeste.Entities
{
    /// <summary>
    /// Ice Bouncer that gives player double dash when bounced on in core mode
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/IceBouncer")]
    [Tracked]
    public class IceBouncer : Entity
    {
        private Sprite sprite;
        private VertexLight light;
        private SoundSource bounceSfx;
        private Wiggler bounceWiggler;
        private bool isActivated = false;
        private float cooldownTimer = 0f;
        private const float COOLDOWN_TIME = 1f;
        private Level level;
        
        // Configuration
        private bool requiresCoreMode;
        private int dashesGranted;
        private Color iceColor;
        private float bounceStrength;

        public IceBouncer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            requiresCoreMode = data.Bool("requiresCoreMode", true);
            dashesGranted = data.Int("dashesGranted", 2);
            bounceStrength = data.Float("bounceStrength", -180f);
            
            string colorHex = data.Attr("iceColor", "87CEEB");
            if (ColorUtility.TryParseHtmlString($"#{colorHex}", out Color color))
            {
                iceColor = color;
            }
            else
            {
                iceColor = Color.LightSkyBlue;
            }

            Depth = -8500;
            Collider = new Hitbox(16f, 8f, -8f, -8f);
            
            SetupComponents();
        }

        private void SetupComponents()
        {
            // Sprite setup
            sprite = new Sprite(GFX.Game, "objects/Ingeste/iceBouncer/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("activated", "activated", 0.05f);
            sprite.AddLoop("cooldown", "cooldown", 0.08f);
            sprite.Play("idle");
            sprite.Color = iceColor;
            Add(sprite);

            // Lighting
            light = new VertexLight(iceColor, 0.8f, 32, 24);
            Add(light);

            // Sound
            bounceSfx = new SoundSource();
            Add(bounceSfx);

            // Visual effects
            bounceWiggler = Wiggler.Create(0.6f, 4f, null, false, false);
            Add(bounceWiggler);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            // Handle cooldown
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Engine.DeltaTime;
                if (cooldownTimer <= 0f)
                {
                    sprite.Play("idle");
                    isActivated = false;
                }
            }

            // Check for player collision
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !isActivated && CollideCheck(player))
            {
                TryBouncePlayer(player);
            }

            // Update visual effects
            UpdateVisuals();
        }

        private void TryBouncePlayer(global::Celeste.Player player)
        {
            // Check if core mode is required
            if (requiresCoreMode && level?.Session?.CoreMode != Session.CoreModes.Cold)
            {
                return;
            }

            // Bounce the player
            BouncePlayer(player);
        }

        private void BouncePlayer(global::Celeste.Player player)
        {
            // Grant dashes
            if (level?.Session?.Inventory != null)
            {
                level.Session.Inventory.Dashes = dashesGranted;
            }

            // Apply bounce force
            player.Speed.Y = bounceStrength;
            player.Speed.X *= 0.8f; // Slight horizontal dampening

            // Visual and audio feedback
            ActivateBouncer();
            CreateIceEffect(player.Position);

            // Play sound
            Audio.Play("event:/game/09_core/iceblock_reappear", Position);
            bounceSfx.Play("event:/game/general/thing_booped");
        }

        private void ActivateBouncer()
        {
            isActivated = true;
            cooldownTimer = COOLDOWN_TIME;
            sprite.Play("activated");
            bounceWiggler.Start();

            // Light pulse effect
            Add(new Coroutine(LightPulse()));
        }

        private IEnumerator LightPulse()
        {
            float timer = 0f;
            while (timer < 0.5f)
            {
                timer += Engine.DeltaTime;
                float pulse = (float)System.Math.Sin(timer * 20f) * 0.3f + 0.8f;
                light.Alpha = pulse;
                yield return null;
            }
            light.Alpha = 0.8f;
        }

        private void CreateIceEffect(Vector2 playerPos)
        {
            if (level == null) return;

            // Ice crystal particles
            for (int i = 0; i < 8; i++)
            {
                Vector2 direction = Calc.AngleToVector(i * 45f * Calc.DegToRad, Calc.Random.Range(20f, 40f));
                Vector2 particlePos = Position + new Vector2(0f, -4f);

                // Use direction.Length() to get the float direction angle in radians
                float angle = (float)System.Math.Atan2(direction.Y, direction.X);
                SceneAs<Level>().Particles.Emit(ParticleTypes.SparkyDust, particlePos, angle);
            }

            // Frost particles around player
            for (int i = 0; i < 12; i++)
            {
                Vector2 direction = Calc.AngleToVector(Calc.Random.NextFloat() * 360f * Calc.DegToRad, Calc.Random.Range(10f, 25f));
                SceneAs<Level>().Particles.Emit(ParticleTypes.Dust, playerPos, (float)System.Math.Atan2(direction.Y, direction.X));
            }
        }

        private void UpdateVisuals()
        {
            // Apply wiggler effect
            sprite.Scale = Vector2.One * (1f + bounceWiggler.Value * 0.2f);

            // Pulsing light when ready
            if (!isActivated)
            {
                light.Alpha = 0.6f + (float)System.Math.Sin(Scene.TimeActive * 2f) * 0.2f;
            }

            // Core mode visual changes
            if (requiresCoreMode && level?.Session?.CoreMode == Session.CoreModes.Cold)
            {
                sprite.Color = Color.White;
                light.Color = Color.LightCyan;
            }
            else if (requiresCoreMode)
            {
                sprite.Color = iceColor * 0.5f;
                light.Color = iceColor * 0.5f;
            }
        }

        public override void Render()
        {
            base.Render();

            // Render ice crystal effect when activated
            if (isActivated && cooldownTimer > COOLDOWN_TIME * 0.7f)
            {
                float alpha = (COOLDOWN_TIME - cooldownTimer) / (COOLDOWN_TIME * 0.3f);
                Color crystalColor = iceColor * alpha * 0.6f;
                
                Draw.Circle(Position + new Vector2(0f, -4f), 12f, crystalColor, 3);
                Draw.Circle(Position + new Vector2(0f, -4f), 8f, Color.White * alpha * 0.4f, 2);
            }
        }

        // Utility class for color parsing (simplified)
        private static class ColorUtility
        {
            public static bool TryParseHtmlString(string htmlColor, out Color color)
            {
                color = Color.White;
                if (string.IsNullOrEmpty(htmlColor) || !htmlColor.StartsWith("#"))
                    return false;
                
                try
                {
                    string hex = htmlColor.Substring(1);
                    if (hex.Length == 6)
                    {
                        int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);
                        color = new Color(r, g, b);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
                return false;
            }
        }
    }
}



