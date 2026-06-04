namespace Celeste.Entities
{
    /// <summary>
    /// Ancient switch that can control other entities and maintain persistent state
    /// </summary>
    [CustomEntity(IngesteConstants.EntityNames.ANCIENT_SWITCH)]
    [HotReloadable]
    public class AncientSwitch : Entity
    {
        public enum SwitchType
        {
            Pressure,
            Lever,
            Crystal,
            Button,
            Magical
        }

        private Sprite sprite;
        private SwitchType switchType;
        private bool isActivated;
        private string targetEntity;
        private bool persistent;
        private bool requiresWeight;
        private bool playerOnSwitch;
        private float activationTimer;
        private SoundSource activationSound;

        public bool IsActivated => isActivated;
        public string TargetEntity => targetEntity;

        public AncientSwitch(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string typeString = data.Attr(nameof(switchType), "pressure");
            if (!System.Enum.TryParse(typeString, true, out switchType))
            {
                switchType = SwitchType.Pressure;
            }

            isActivated = data.Bool(nameof(isActivated), false);
            targetEntity = data.Attr(nameof(targetEntity), "");
            persistent = data.Bool(nameof(persistent), true);
            requiresWeight = data.Bool(nameof(requiresWeight), false);

            playerOnSwitch = false;
            activationTimer = 0f;

            setupSprite();
            setupCollision();
            setupAudio();

            Depth = 1000; // Render below most things
        }

        private void setupSprite()
        {
            string spriteName = $"switch_{switchType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                // Fallback sprite
                sprite = new Sprite(GFX.Game, "objects/temple/");
                sprite.AddLoop("off", "switch00", 0.1f);
                sprite.AddLoop("on", "switch01", 0.1f);
                sprite.AddLoop("pressing", "switch00", 0.05f);
            }

            Add(sprite);
            updateSpriteState();
        }

        private void setupCollision()
        {
            switch (switchType)
            {
                case SwitchType.Pressure:
                    Collider = new Hitbox(16f, 8f, -8f, -4f);
                    Add(component: new PlayerCollider(onCollide: OnPlayerStep));

                    void OnPlayerStep(global::Celeste.Player obj)
                    {
                        if (requiresWeight && !obj.CanDash)
                            return;
                        if (!playerOnSwitch)
                        {
                            playerOnSwitch = true;
                            OnSwitchActivated();
                        }
                    }

                    break;

                case SwitchType.Lever:
                case SwitchType.Button:
                case SwitchType.Crystal:
                case SwitchType.Magical:
                    Collider = new Hitbox(16f, 16f, -8f, -8f);
                    Add(component: new PlayerCollider(onCollide: OnPlayerInteract));

                    void OnPlayerInteract(global::Celeste.Player obj)
                    {
                        if (Input.Grab.Pressed && obj.CollideCheck(this))
                        {
                            isActivated = !isActivated;
                            OnSwitchActivated();
                        }
                    }

                    break;
            }
        }

        private void setupAudio()
        {
            activationSound = new SoundSource();
            Add(activationSound);
        }

        private void updateSpriteState()
        {
            if (switchType == SwitchType.Pressure)
            {
                if (playerOnSwitch)
                    sprite?.Play("pressing");
                else if (isActivated)
                    sprite?.Play("on");
                else
                    sprite?.Play("off");
            }
            else
            {
                sprite?.Play(isActivated ? "on" : "off");
            }
        }

        public override void Update()
        {
            base.Update();

            // Handle pressure switch behavior
            if (switchType == SwitchType.Pressure)
            {
                bool wasPlayerOnSwitch = playerOnSwitch;
                playerOnSwitch = CheckPlayerOnSwitch();

                if (playerOnSwitch != wasPlayerOnSwitch)
                {
                    if (playerOnSwitch)
                    {
                        OnSwitchActivated();
                    }
                    else if (!persistent)
                    {
                        OnSwitchDeactivated();
                    }
                }
            }

            // Handle activation timer for certain switch types
            if (activationTimer > 0)
            {
                activationTimer -= Engine.DeltaTime;
                if (activationTimer <= 0 && !persistent)
                {
                    OnSwitchDeactivated();
                }
            }

            updateSpriteState();
        }

        private bool CheckPlayerOnSwitch()
        {
            var player = (Scene as Level)?.Tracker.GetEntity<global::Celeste.Player>();
            return player != null && Collider.Collide(player);
        }

        private void OnSwitchActivated()
        {
            isActivated = true;

            // Play activation sound
            string soundEvent = getActivationSound();
            Audio.Play(soundEvent, Position);

            // Set timer for non-persistent switches
            if (!persistent)
            {
                activationTimer = getActivationDuration();
            }

            // Notify target entities
            notifyTargetEntities(true);

            // Visual feedback
            createActivationEffect();
        }

        private void OnSwitchDeactivated()
        {
            isActivated = false;

            // Play deactivation sound
            Audio.Play("event:/game/general/console_deactivate", Position);

            // Notify target entities
            notifyTargetEntities(false);
        }

        private string getActivationSound()
        {
            switch (switchType)
            {
                case SwitchType.Pressure: return "guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}";
                case SwitchType.Lever: return "event:/game/general/console_activate_complete";
                case SwitchType.Crystal: return "guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}";
                case SwitchType.Button: return "guid://{cf18e899-f774-47e2-b72c-5252231fd47c}";
                case SwitchType.Magical: return "guid://{1acb8e84-5f79-4dcd-86aa-e8a514535b7d}";
                default: return "guid://{097d5975-dee1-42e2-a3f0-1524b9d60fc6}";
            }
        }

        private float getActivationDuration()
        {
            switch (switchType)
            {
                case SwitchType.Crystal: return 3f;
                case SwitchType.Button: return 2f;
                case SwitchType.Magical: return 5f;
                default: return 0f;
            }
        }

        private void notifyTargetEntities(bool activated)
        {
            if (string.IsNullOrEmpty(targetEntity)) return;

            var level = Scene as Level;
            if (level == null) return;

            // Find entities by ID or tag and notify them of switch state change
            // This could be extended to work with specific entity interfaces
            foreach (Entity entity in level.Entities)
            {
                // Check if entity has the target ID as a tag or similar property
                if (entity.GetType().Name == targetEntity ||
                    (entity is ISwitchable switchable))
                {
                    if (entity is ISwitchable switchableEntity)
                    {
                        switchableEntity.OnSwitchStateChanged(this, activated);
                    }
                }
            }
        }

        private void createActivationEffect()
        {
            var level = Scene as Level;
            if (level == null) return;

            Color effectColor = getSwitchColor();

            // Create particle burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 particleDir = Calc.AngleToVector(i * MathHelper.PiOver4, 20f);
                level.ParticlesBG.Emit(ParticleTypes.Chimney, Position, particleDir.Angle());
            }
        }

        private Color getSwitchColor()
        {
            switch (switchType)
            {
                case SwitchType.Pressure: return Color.Gray;
                case SwitchType.Lever: return Color.Brown;
                case SwitchType.Crystal: return Color.Cyan;
                case SwitchType.Button: return Color.Orange;
                case SwitchType.Magical: return Color.Purple;
                default: return Color.White;
            }
        }
    }

    /// <summary>
    /// Interface for entities that can be controlled by switches
    /// </summary>
    public interface ISwitchable
    {
        void OnSwitchStateChanged(AncientSwitch switchEntity, bool activated);
    }
}





