namespace Celeste.Entities {
    /// <summary>
    /// Enhanced power generator with fuel management and different types
    /// </summary>
    [CustomEntity("DZ/PowerGenerator", "DZ/EnhancedPowerGenerator", "MaggyHelper/EnhancedPowerGenerator")]
    public class EnhancedPowerGenerator : Entity {
        public enum GeneratorType {
            Basic,
            Advanced,
            Magical,
            Nuclear
        }

        private Sprite sprite;
        private GeneratorType generatorType;
        private bool isActive;
        private float powerOutput;
        private float fuelLevel;
        private float maxFuelLevel;
        private float particleTimer;
        private SoundSource humSound;

        public bool IsActive => isActive;
        public float PowerOutput => isActive ? powerOutput : 0f;
        public float FuelLevel => fuelLevel;
        public float FuelPercentage => fuelLevel / maxFuelLevel;

        public EnhancedPowerGenerator(EntityData data, Vector2 offset) : base(data.Position + offset) {
            string typeString = data.Attr(nameof(generatorType), "basic");
            if (!System.Enum.TryParse(typeString, true, out generatorType)) {
                generatorType = GeneratorType.Basic;
            }

            isActive = data.Bool(nameof(isActive), true);
            powerOutput = data.Float(nameof(powerOutput), 100f);
            fuelLevel = data.Float(nameof(fuelLevel), 100f);

            setupGeneratorStats();
            setupSprite();
            setupCollision();
            setupAudio();

            Depth = -1000;
        }

        private void setupGeneratorStats() {
            switch (generatorType) {
                case GeneratorType.Basic:
                    maxFuelLevel = 100f;
                    powerOutput = Math.Min(powerOutput, 150f);
                    break;
                case GeneratorType.Advanced:
                    maxFuelLevel = 200f;
                    powerOutput = Math.Min(powerOutput, 300f);
                    break;
                case GeneratorType.Magical:
                    maxFuelLevel = 500f; // Magical generators have more "fuel"
                    powerOutput = Math.Min(powerOutput, 600f);
                    break;
                case GeneratorType.Nuclear:
                    maxFuelLevel = 1000f;
                    powerOutput = Math.Min(powerOutput, 1000f);
                    break;
            }

            fuelLevel = Math.Min(fuelLevel, maxFuelLevel);
        }

        private void setupSprite() {
            // Try to load generator-specific sprite
            string spriteName = $"generator_{generatorType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName)) {
                sprite = GFX.SpriteBank.Create(spriteName);
            } else {
                // Fallback sprite
                sprite = new Sprite(GFX.Game, "objects/DZ/DZ/kevins_pc/");
                sprite.AddLoop("idle", "pc_idle", 0.1f);
                sprite.AddLoop("active", "pc_idle", 0.05f);
            }

            Add(sprite);
            updateSpriteState();
        }

        private void setupCollision() {
            Collider = new Hitbox(32f, 32f, -16f, -32f);
            Add(new PlayerCollider(OnPlayerInteract));
        }

        private void setupAudio() {
            humSound = new SoundSource();
            Add(humSound);
        }

        private void updateSpriteState() {
            if (isActive && fuelLevel > 0) {
                sprite?.Play("active");
                if (!humSound.Playing) {
                    humSound.Play("event:/game/general/console_activate_run", "generator_type", (int)generatorType);
                }
            } else {
                sprite?.Play("idle");
                humSound?.Stop();
            }
        }

        public override void Update() {
            base.Update();

            if (isActive && fuelLevel > 0) {
                // Consume fuel over time
                float consumptionRate = powerOutput * 0.01f; // Higher power = more fuel consumption
                fuelLevel = Math.Max(0, fuelLevel - consumptionRate * Engine.DeltaTime);

                // Deactivate if out of fuel
                if (fuelLevel <= 0) {
                    isActive = false;
                    updateSpriteState();
                }

                // Update particles
                updateParticles();
            }
        }

        private void updateParticles() {
            particleTimer += Engine.DeltaTime;

            if (particleTimer > 0.1f) {
                particleTimer = 0f;

                Color particleColor = getGeneratorColor();
                Vector2 particlePos = Position + new Vector2(0, -20);

                var level = Scene as Level;
                level?.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);

                // More particles for higher tier generators
                if (generatorType >= GeneratorType.Advanced) {
                    level?.ParticlesBG.Emit(ParticleTypes.SparkyDust, particlePos, particleColor);
                }

                if (generatorType >= GeneratorType.Magical) {
                    level?.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);
                }
            }
        }

        private Color getGeneratorColor() {
            switch (generatorType) {
                case GeneratorType.Basic: return Color.Gray;
                case GeneratorType.Advanced: return Color.CornflowerBlue;
                case GeneratorType.Magical: return Color.Purple;
                case GeneratorType.Nuclear: return Color.Yellow;
                default: return Color.White;
            }
        }

        private void OnPlayerInteract(global::Celeste.Player player) {
            // Toggle generator on/off
            if (fuelLevel > 0) {
                isActive = !isActive;
                updateSpriteState();

                Audio.Play(isActive ? "event:/game/general/console_activate_complete" : "event:/game/general/console_deactivate");
            } else {
                // Show refuel prompt or play error sound
                Audio.Play("event:/ui/main/button_invalid");
            }
        }

        public void Refuel(float amount) {
            fuelLevel = Math.Min(maxFuelLevel, fuelLevel + amount);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public void SetActive(bool active) {
            isActive = active && fuelLevel > 0;
            updateSpriteState();
        }
    }
}




