namespace Celeste.Entities
{
    /// <summary>
    /// Crystal heart gate that requires crystal hearts to open
    /// </summary>
    [CustomEntity(ids: "DZ/CrystalHeartGate")]
    [HotReloadable]
    public class CrystalHeartGate : Entity
    {
        private int heartsRequired;
        private bool isOpen;
        private bool permanent;
        private float width;
        private float height;
        private Sprite sprite;
        private VertexLight light;
        private SoundSource gateSound;
        private StaticMover staticMover;
        private Level level;
        private Wiggler wiggler;

        public bool IsOpen => isOpen;
        public int HeartsRequired => heartsRequired;

        public CrystalHeartGate(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            heartsRequired = data.Int(nameof(heartsRequired), 1);
            permanent = data.Bool(nameof(permanent), true);
            width = data.Width;
            height = data.Height;
            isOpen = false;

            setupSprite();
            setupComponents();
            setupCollision();

            Depth = 0;
        }

        private void setupSprite()
        {
            if (GFX.SpriteBank.Has("crystal_heart_gate"))
            {
                sprite = GFX.SpriteBank.Create("crystal_heart_gate");
            }
            else
            {
                sprite = new Sprite(GFX.Game, "objects/DZ/DZ/temple/portal/");
                sprite.AddLoop("closed", "portal", 0.1f);
                sprite.AddLoop("opening", "portal", 0.05f);
                sprite.AddLoop("open", "portal", 0.2f);
            }

            Add(sprite);
            sprite.Play("closed");
        }

        private void setupComponents()
        {
            // Lighting system
            light = new VertexLight(Color.Pink, 1f, (int)width, (int)height);
            Add(light);

            // Sound system
            gateSound = new SoundSource();
            Add(gateSound);

            // Static mover for solid behavior
            staticMover = new StaticMover();
            Add(staticMover);

            // Wiggler for visual effects
            wiggler = Wiggler.Create(0.4f, 4f, null, false, false);
            Add(wiggler);
        }

        private void setupCollision()
        {
            if (!isOpen)
            {
                Collider = new Hitbox(width, height);
                Collidable = true;
            }
            else
            {
                Collidable = false;
            }
        }

        public override void Update()
        {
            base.Update();

            level = Scene as Level;

            if (!isOpen)
            {
                checkHeartCount();
            }

            updateVisuals();
            updateScale();
        }

        private void checkHeartCount()
        {
            if (level?.Session != null)
            {
                // Check how many crystal hearts the player has
                int currentHearts = getCrystalHeartCount();

                if (currentHearts >= heartsRequired)
                {
                    openGate();
                }
            }
        }

        private int getCrystalHeartCount()
        {
            // In a real implementation, this would check the save data for crystal hearts
            // For now, we'll check session flags as an approximation
            if (level?.Session != null)
            {
                int count = 0;

                // Check for crystal heart flags
                for (int i = 1; i <= 8; i++) // Assuming up to 8 crystal hearts
                {
                    if (level.Session.GetFlag($"heart_{i}"))
                    {
                        count++;
                    }
                }

                return count;
            }

            return 0;
        }

        private void openGate()
        {
            if (isOpen) return;

            isOpen = true;

            // Update collision
            Collidable = false;

            // Play opening animation and sound
            sprite?.Play("opening");
            gateSound?.Play("event:/game/general/cassette_bubblereturn");

            // Visual effects
            wiggler?.Start();
            createOpeningEffect();

            // Mark as permanently opened if configured
            if (permanent && level?.Session != null)
            {
                string gateKey = $"IngesteHelper_CrystalGate_{Position.X}_{Position.Y}";
                level.Session.SetFlag(gateKey, true);
            }
        }

        private void createOpeningEffect()
        {
            if (level != null)
            {
                // Create particle explosion
                for (int i = 0; i < 20; i++)
                {
                    Vector2 direction = Calc.AngleToVector(i * 0.314f, 30f);
                    Vector2 particlePos = Position + new Vector2(width / 2, height / 2) + direction * 0.5f;
                    level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, Color.Pink);
                }
            }
        }

        private void updateVisuals()
        {
            if (isOpen)
            {
                light.Alpha = 0.3f;
                sprite?.Play("open");
            }
            else
            {
                // Pulsing light when closed
                light.Alpha = 0.6f + (float)System.Math.Sin(Scene.TimeActive * 2f) * 0.2f;

                // Check if player is near and doesn't have enough hearts
                global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && Vector2.Distance(Position, player.Position) < 64f)
                {
                    int currentHearts = getCrystalHeartCount();
                    if (currentHearts < heartsRequired)
                    {
                        // Pulse red to indicate insufficient hearts
                        light.Color = Color.Lerp(Color.Pink, Color.Red,
                            (float)System.Math.Sin(Scene.TimeActive * 8f) * 0.5f + 0.5f);
                    }
                }
                else
                {
                    light.Color = Color.Pink;
                }
            }
        }

        private void updateScale()
        {
            if (wiggler != null)
            {
                sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.1f);
            }
        }

        public void ForceOpen()
        {
            openGate();
        }

        public void Reset()
        {
            if (!permanent)
            {
                isOpen = false;
                setupCollision();
                sprite?.Play("closed");
            }
        }

        public override void Render()
        {
            if (!isOpen)
            {
                // Render gate visual
                Color gateColor = Color.Pink * 0.4f;
                Draw.Rect(Position, width, height, gateColor);

                // Render crystal heart requirement indicator
                renderHeartRequirement();
            }

            base.Render();
        }

        private void renderHeartRequirement()
        {
            Vector2 centerPos = Position + new Vector2(width / 2, height / 2);

            // Draw heart symbols to show requirement
            for (int i = 0; i < heartsRequired; i++)
            {
                Vector2 heartPos = centerPos + new Vector2(
                    (i - (heartsRequired - 1) / 2f) * 12f,
                    -8f
                );

                int currentHearts = getCrystalHeartCount();
                Color heartColor = i < currentHearts ? Color.Pink : Color.Gray;

                // Simple heart representation (in a real implementation, use proper sprites)
                Draw.Circle(heartPos, 4f, heartColor, 8);
            }
        }
    }
}





