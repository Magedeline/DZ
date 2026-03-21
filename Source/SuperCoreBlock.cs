namespace MaggyHelper.Entities
{
    /// <summary>
    /// Enhanced core block that launches player at 3x speed with extended range
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/SuperCoreBlock")]
    [Tracked]
    public class SuperCoreBlock : Entity
    {
        private Sprite sprite;
        private VertexLight light;
        private SoundSource coreSfx;
        private Wiggler activationWiggler;
        private Wiggler scaleWiggler;
        private Level level;
        
        // Configuration
        private float speedMultiplier;
        private float launchRange;
        private bool requiresCoreMode;
        private Color hotColor;
        private Color coldColor;
        private Color superColor;
        
        // State
        private bool isActivated = false;
        private float cooldownTimer = 0f;
        private const float COOLDOWN_TIME = 0.8f;
        private Session.CoreModes currentCoreMode;
        
        // Visual effects
        private float energyPulse = 0f;
        private Vector2[] energyParticles;
        private int particleCount = 24;

        public SuperCoreBlock(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            speedMultiplier = data.Float("speedMultiplier", 3f);
            launchRange = data.Float("launchRange", 400f);
            requiresCoreMode = data.Bool("requiresCoreMode", false);
            
            // Color configuration
            string hotHex = data.Attr("hotColor", "FF4500");
            string coldHex = data.Attr("coldColor", "00BFFF");
            string superHex = data.Attr("superColor", "FFD700");
            
            TryParseColor(hotHex, out hotColor);
            TryParseColor(coldHex, out coldColor);
            TryParseColor(superHex, out superColor);

            Depth = -200;
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            
            SetupComponents();
            InitializeEnergyParticles();
        }

        private void SetupComponents()
        {
            // Sprite setup
            sprite = new Sprite(GFX.Game, "objects/Ingeste/superCoreBlock/");
            sprite.AddLoop("hot_idle", "hot_idle", 0.08f);
            sprite.AddLoop("hot_active", "hot_active", 0.04f);
            sprite.AddLoop("cold_idle", "cold_idle", 0.08f);
            sprite.AddLoop("cold_active", "cold_active", 0.04f);
            sprite.AddLoop("super_idle", "super_idle", 0.05f);
            sprite.AddLoop("super_active", "super_active", 0.02f);
            sprite.AddLoop("super_charged", "super_charged", 0.01f);
            sprite.Play("hot_idle");
            Add(sprite);

            // Lighting
            light = new VertexLight(hotColor, 1.2f, 64, 48);
            Add(light);

            // Sound
            coreSfx = new SoundSource();
            Add(coreSfx);

            // Visual effects
            activationWiggler = Wiggler.Create(0.5f, 3f, null, false, false);
            Add(activationWiggler);

            scaleWiggler = Wiggler.Create(0.3f, 4f, null, false, false);
            Add(scaleWiggler);
        }

        private void InitializeEnergyParticles()
        {
            energyParticles = new Vector2[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                float angle = (i / (float)particleCount) * 360f * Calc.DegToRad;
                energyParticles[i] = Calc.AngleToVector(angle, 16f);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            if (level?.Session != null)
            {
                currentCoreMode = level.Session.CoreMode;
                UpdateVisualState();
            }
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
                    isActivated = false;
                    UpdateVisualState();
                }
            }

            // Update core mode
            if (level?.Session != null && currentCoreMode != level.Session.CoreMode)
            {
                currentCoreMode = level.Session.CoreMode;
                UpdateVisualState();
                scaleWiggler.Start();
            }

            // Check for player collision
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player) && !isActivated)
            {
                TryLaunchPlayer(player);
            }

            UpdateVisualEffects();
        }

        private void TryLaunchPlayer(global::Celeste.Player player)
        {
            // Check if core mode is required
            if (requiresCoreMode && currentCoreMode == Session.CoreModes.None)
            {
                return;
            }

            LaunchPlayer(player);
        }

        private void LaunchPlayer(global::Celeste.Player player)
        {
            isActivated = true;
            cooldownTimer = COOLDOWN_TIME;

            // Calculate launch direction and speed
            Vector2 launchDirection = CalculateLaunchDirection(player);
            float baseSpeed = GetBaseLaunchSpeed();
            float finalSpeed = baseSpeed * speedMultiplier;

            // Apply launch force
            player.Speed = launchDirection * finalSpeed;

            // Extend launch range by reducing air friction temporarily
            Add(new Coroutine(ExtendedLaunchCoroutine(player)));

            // Visual and audio feedback
            ActivateBlock();
            CreateLaunchEffect(player.Position, launchDirection);

            // Play sound based on core mode
            PlayLaunchSound();
        }

        private Vector2 CalculateLaunchDirection(global::Celeste.Player player)
        {
            // Default upward launch, but check for input direction
            Vector2 direction = Vector2.UnitY * -1f;

            if (Input.MoveX.Value != 0)
            {
                // Diagonal launch
                direction = new Vector2(Input.MoveX.Value, -0.7f).SafeNormalize();
            }
            else if (Input.MoveY.Value > 0)
            {
                // Downward launch (if holding down)
                direction = Vector2.UnitY;
            }

            return direction;
        }

        private float GetBaseLaunchSpeed()
        {
            switch (currentCoreMode)
            {
                case Session.CoreModes.Hot:
                    return 280f;
                case Session.CoreModes.Cold:
                    return 320f;
                default:
                    return 240f; // Normal mode gets super speed anyway
            }
        }

        private IEnumerator ExtendedLaunchCoroutine(global::Celeste.Player player)
        {
            float timer = 0f;
            float extendDuration = launchRange / (GetBaseLaunchSpeed() * speedMultiplier);

            Vector2 originalGravity = new Vector2(0f, 900f); // Default gravity
            float reducedGravity = 300f; // Reduced gravity for extended flight

            while (timer < extendDuration && player != null)
            {
                timer += Engine.DeltaTime;

                // Reduce gravity effect for extended flight
                if (player.Speed.Y > 0f) // Only when falling
                {
                    player.Speed.Y -= (originalGravity.Y - reducedGravity) * Engine.DeltaTime;
                }

                // Create trail particles
                if (Scene.OnInterval(0.05f))
                {
                    CreateTrailParticles(player.Position);
                }

                yield return null;
            }

            // Final speed boost if still in air
            if (player != null && !player.OnGround())
            {
                player.Speed *= 1.1f;
                CreateSpeedBoostEffect(player.Position);
            }
        }

        private void ActivateBlock()
        {
            UpdateVisualState();
            activationWiggler.Start();
            scaleWiggler.Start();

            // Intensify light
            light.Alpha = 1.5f;
        }

        private void UpdateVisualState()
        {
            if (isActivated)
            {
                switch (currentCoreMode)
                {
                    case Session.CoreModes.Hot:
                        sprite.Play("hot_active");
                        light.Color = hotColor;
                        break;
                    case Session.CoreModes.Cold:
                        sprite.Play("cold_active");
                        light.Color = coldColor;
                        break;
                    default:
                        sprite.Play("super_charged");
                        light.Color = superColor;
                        break;
                }
            }
            else
            {
                switch (currentCoreMode)
                {
                    case Session.CoreModes.Hot:
                        sprite.Play("hot_idle");
                        light.Color = hotColor;
                        break;
                    case Session.CoreModes.Cold:
                        sprite.Play("cold_idle");
                        light.Color = coldColor;
                        break;
                    default:
                        sprite.Play("super_idle");
                        light.Color = superColor;
                        break;
                }
            }
        }

        private void PlayLaunchSound()
        {
            string soundEvent;
            switch (currentCoreMode)
            {
                case Session.CoreModes.Hot:
                    soundEvent = "event:/game/09_core/iceblock_reappear";
                    break;
                case Session.CoreModes.Cold:
                    soundEvent = "event:/game/09_core/icewall_emerge";
                    break;
                default:
                    soundEvent = "event:/game/general/crystalheart_pulse";
                    break;
            }

            coreSfx.Play(soundEvent);
            Audio.Play("event:/game/06_reflection/badeline_boss_bullet", Position);
        }

        private void CreateLaunchEffect(Vector2 playerPos, Vector2 direction)
        {
            if (level == null) return;

            Color effectColor = GetCurrentColor();

            // Explosion effect at block
            for (int i = 0; i < 20; i++)
            {
                float angle = i * 18f * Calc.DegToRad;
                Vector2 particleDir = Calc.AngleToVector(angle, Calc.Random.Range(40f, 80f));
                Vector2 particlePos = Position + particleDir * 0.3f;

                level.ParticlesFG.Emit(ParticleTypes.Dust, particlePos, effectColor, angle);
            }

            // Launch trail effect
            for (int i = 0; i < 12; i++)
            {
                Vector2 trailPos = Position + direction * i * 8f;
                level.ParticlesBG.Emit(ParticleTypes.Dust, trailPos, effectColor, direction.Angle());
            }
        }

        private void CreateTrailParticles(Vector2 position)
        {
            if (level == null) return;

            Color trailColor = GetCurrentColor() * 0.8f;
            // Use the angle of the direction vector as the 4th argument (float)
            Vector2 particleDir = new Vector2(Calc.Random.Range(-20f, 20f), Calc.Random.Range(10f, 30f));
            float direction = (float)Math.Atan2(particleDir.Y, particleDir.X);

            level.ParticlesBG.Emit(ParticleTypes.Dust, position, trailColor, direction);
        }

        private void CreateSpeedBoostEffect(Vector2 position)
        {
            if (level == null) return;

            Color boostColor = superColor;

            for (int i = 0; i < 16; i++)
            {
                float angle = i * 22.5f * Calc.DegToRad;
                float distance = Calc.Random.Range(30f, 60f);
                Vector2 direction = Calc.AngleToVector(angle, distance);
                // Use angle (float) instead of direction (Vector2) for the 4th argument
                level.ParticlesFG.Emit(ParticleTypes.Dust, position, boostColor, angle);
            }
        }

        private void UpdateVisualEffects()
        {
            // Energy pulse animation
            energyPulse += Engine.DeltaTime * 4f;
            
            // Pulsing light
            float basePulse = 0.8f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.2f;
            if (isActivated)
                basePulse += 0.4f + (float)Math.Sin(Scene.TimeActive * 15f) * 0.3f;
            
            light.Alpha = basePulse;

            // Scale effects
            Vector2 scale = Vector2.One;
            scale *= 1f + activationWiggler.Value * 0.3f;
            scale *= 1f + scaleWiggler.Value * 0.1f;
            
            if (isActivated)
            {
                scale *= 1f + (float)Math.Sin(Scene.TimeActive * 12f) * 0.15f;
            }
            
            sprite.Scale = scale;
        }

        private Color GetCurrentColor()
        {
            switch (currentCoreMode)
            {
                case Session.CoreModes.Hot: return hotColor;
                case Session.CoreModes.Cold: return coldColor;
                default: return superColor;
            }
        }

        public override void Render()
        {
            base.Render();

            // Render energy particles when active or charged
            if (isActivated || currentCoreMode != Session.CoreModes.None)
            {
                RenderEnergyField();
            }

            // Render range indicator in debug mode
            if (Engine.Commands.Open)
            {
                Vector2 launchDir = Vector2.UnitY * -1f;
                Vector2 rangeEnd = Position + launchDir * launchRange;
                Draw.Line(Position, rangeEnd, Color.Yellow, 2);
                Draw.Circle(Position, 4f, Color.Red, 3);
            }
        }

        private void RenderEnergyField()
        {
            Color fieldColor = GetCurrentColor();
            float alpha = isActivated ? 0.8f : 0.4f;
            
            // Rotating energy particles
            for (int i = 0; i < particleCount; i++)
            {
                float angle = (i / (float)particleCount) * 360f * Calc.DegToRad + energyPulse;
                float radius = 20f + (float)Math.Sin(Scene.TimeActive * 5f + i * 0.2f) * 4f;
                
                Vector2 particlePos = Position + Calc.AngleToVector(angle, radius);
                Color particleColor = fieldColor * alpha * (0.5f + (float)Math.Sin(Scene.TimeActive * 8f + i * 0.5f) * 0.5f);
                
                Draw.Point(particlePos, particleColor);
                
                if (isActivated)
                {
                    Draw.Point(particlePos + Vector2.One, particleColor * 0.6f);
                }
            }

            // Core energy ring
            if (isActivated)
            {
                float ringRadius = 16f + (float)Math.Sin(Scene.TimeActive * 10f) * 3f;
                Draw.Circle(Position, ringRadius, fieldColor * 0.6f, 2);
                Draw.Circle(Position, ringRadius * 0.7f, Color.White * 0.4f, 1);
            }
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = Color.White;
            if (string.IsNullOrEmpty(hex)) return false;
            
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            try
            {
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = new Color(r, g, b);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}



