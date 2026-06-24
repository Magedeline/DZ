namespace Celeste.Entities
{
    /// <summary>
    /// Wave-based puzzle machine for tutorials and challenges
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/WavePhazeMachine")]
    public class WavePhazeMachine : Entity
    {
        public enum MachineType
        {
            Tutorial,
            Challenge,
            Practice,
            Exam
        }

        private MachineType machineType;
        private int currentWave;
        private int maxWaves;
        private bool isActive;
        private bool isCompleted;
        private Sprite sprite;
        private VertexLight light;
        private SoundSource machineSound;
        private float waveTimer;
        private float waveDuration;
        private Level level;

        public bool IsActive => isActive;
        public bool IsCompleted => isCompleted;
        public int CurrentWave => currentWave;

        public WavePhazeMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string typeString = data.Attr(nameof(machineType), "tutorial");
            if (!System.Enum.TryParse(typeString, true, out machineType))
            {
                machineType = MachineType.Tutorial;
            }

            maxWaves = data.Int(nameof(maxWaves), 3);
            waveDuration = data.Float(nameof(waveDuration), 10f);
            currentWave = 0;
            waveTimer = 0f;
            isActive = false;
            isCompleted = false;

            setupSprite();
            setupComponents();

            Depth = 1000;
            Collider = new Hitbox(32f, 32f, -16f, -16f);
        }

        private void setupSprite()
        {
            string spriteName = $"machine_{machineType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                sprite = new Sprite(GFX.Game, "objects/temple/portal/");
                sprite.AddLoop("idle", "portal", 0.1f);
                sprite.AddLoop("active", "portal", 0.05f);
                sprite.AddLoop("complete", "portal", 0.2f);
            }

            Add(sprite);
            sprite.Play("idle");
        }

        private void setupComponents()
        {
            // Lighting system
            Color lightColor = getMachineColor();
            light = new VertexLight(lightColor, 1f, 32, 64);
            Add(light);

            // Sound system
            machineSound = new SoundSource();
            Add(machineSound);
        }

        private Color getMachineColor()
        {
            switch (machineType)
            {
                case MachineType.Tutorial: return Color.Green;
                case MachineType.Challenge: return Color.Orange;
                case MachineType.Practice: return Color.Blue;
                case MachineType.Exam: return Color.Red;
                default: return Color.White;
            }
        }

        public override void Update()
        {
            base.Update();

            level = Scene as Level;

            if (isActive && !isCompleted)
            {
                updateWaveLogic();
            }

            updateVisuals();
            checkPlayerInteraction();
        }

        private void updateWaveLogic()
        {
            waveTimer += Engine.DeltaTime;

            if (waveTimer >= waveDuration)
            {
                advanceWave();
            }
        }

        private void advanceWave()
        {
            currentWave++;
            waveTimer = 0f;

            if (currentWave >= maxWaves)
            {
                completeChallenge();
            }
            else
            {
                startNextWave();
            }
        }

        private void startNextWave()
        {
            // Play wave start sound
            Audio.Play(getWaveSoundEvent(), Position);

            // Trigger wave-specific mechanics
            triggerWaveMechanics();
        }

        private void triggerWaveMechanics()
        {
            // This method can be overridden or extended for specific wave mechanics
            switch (machineType)
            {
                case MachineType.Tutorial:
                    triggerTutorialWave();
                    break;
                case MachineType.Challenge:
                    triggerChallengeWave();
                    break;
                case MachineType.Practice:
                    triggerPracticeWave();
                    break;
                case MachineType.Exam:
                    triggerExamWave();
                    break;
            }
        }

        private void triggerTutorialWave()
        {
            // Tutorial-specific wave mechanics
            if (level != null)
            {
                // Could spawn tutorial elements or display instructions
            }
        }

        private void triggerChallengeWave()
        {
            // Challenge-specific wave mechanics
            if (level != null)
            {
                // Could spawn enemies or obstacles
            }
        }

        private void triggerPracticeWave()
        {
            // Practice-specific wave mechanics
            if (level != null)
            {
                // Could create practice scenarios
            }
        }

        private void triggerExamWave()
        {
            // Exam-specific wave mechanics
            if (level != null)
            {
                // Could create test conditions
            }
        }

        private void completeChallenge()
        {
            isCompleted = true;
            isActive = false;

            sprite?.Play("complete");
            Audio.Play("event:/game/general/cassette_bubblereturn", Position);

            // Award completion
            awardCompletion();
        }

        private void awardCompletion()
        {
            if (level?.Session != null)
            {
                string completionKey = $"Machine_{machineType}_{Position.X}_{Position.Y}";
                level.Session.SetFlag(completionKey, true);
            }
        }

        private string getWaveSoundEvent()
        {
            switch (machineType)
            {
                case MachineType.Tutorial: return "event:/game/general/thing_booped";
                case MachineType.Challenge: return "event:/game/04_cliffside/arrowblock_activate";
                case MachineType.Practice: return "event:/game/05_mirror_temple/crystaltheo_pulse";
                case MachineType.Exam: return "event:/game/06_reflection/badeline_disappear";
                default: return "event:/game/general/thing_booped";
            }
        }

        private void updateVisuals()
        {
            if (isCompleted)
            {
                light.Alpha = 1f;
                sprite.Color = Color.White;
            }
            else if (isActive)
            {
                light.Alpha = 0.8f + (float)System.Math.Sin(Scene.TimeActive * 4f) * 0.2f;
                sprite.Color = getMachineColor();
            }
            else
            {
                light.Alpha = 0.3f;
                sprite.Color = Color.Gray;
            }
        }

        private void checkPlayerInteraction()
        {
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player))
            {
                if (!isActive && !isCompleted)
                {
                    Activate();
                }
            }
        }

        public void Activate()
        {
            if (isActive || isCompleted) return;

            isActive = true;
            currentWave = 0;
            waveTimer = 0f;

            sprite?.Play("active");
            machineSound?.Play("event:/game/05_mirror_temple/platform_activate");

            startNextWave();
        }

        public void Reset()
        {
            isActive = false;
            isCompleted = false;
            currentWave = 0;
            waveTimer = 0f;
            sprite?.Play("idle");
        }

        public float GetWaveProgress()
        {
            if (!isActive || waveDuration <= 0f) return 0f;
            return waveTimer / waveDuration;
        }
    }
}





