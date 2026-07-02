namespace Celeste.Entities
{
    /// <summary>
    /// Energy barrier that can be activated/deactivated and blocks player movement
    /// </summary>
    [CustomEntity(ids: "DZ/EnergyBarrier")]
    public class EnergyBarrier : Entity
    {
        public enum BarrierType
        {
            Blue,
            Red,
            Green,
            Purple,
            Orange
        }

        private BarrierType barrierType;
        private bool isActive;
        private bool solidWhenActive;
        private float width;
        private float height;
        private Sprite sprite;
        private VertexLight light;
        private SoundSource barrierSound;
        private StaticMover staticMover;
        private Level level;

        public bool IsActive => isActive;

        public EnergyBarrier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string typeString = data.Attr(nameof(barrierType), "blue");
            if (!System.Enum.TryParse(typeString, true, out barrierType))
            {
                barrierType = BarrierType.Blue;
            }

            isActive = data.Bool(nameof(isActive), true);
            solidWhenActive = data.Bool(nameof(solidWhenActive), true);
            width = data.Width;
            height = data.Height;

            setupSprite();
            setupComponents();
            setupCollision();

            Depth = -10000;
        }

        private void setupSprite()
        {
            string spriteName = $"barrier_{barrierType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                sprite = new Sprite(GFX.Game, "objects/DZ/DZ/temple/portal/");
                sprite.AddLoop("active", "portal", 0.1f);
                sprite.AddLoop("inactive", "portal", 0.3f);
            }

            Add(sprite);
            updateSpriteState();
        }

        private void setupComponents()
        {
            // Lighting system
            Color lightColor = getBarrierColor();
            light = new VertexLight(lightColor, 1f, (int)width, (int)height);
            Add(light);

            // Sound system
            barrierSound = new SoundSource();
            Add(barrierSound);

            // Static mover for solid behavior
            staticMover = new StaticMover();
            Add(staticMover);
        }

        private void setupCollision()
        {
            if (solidWhenActive && isActive)
            {
                Collider = new Hitbox(width, height);
                Collidable = true;
            }
            else
            {
                Collidable = false;
            }
        }

        private Color getBarrierColor()
        {
            switch (barrierType)
            {
                case BarrierType.Blue: return Color.Blue;
                case BarrierType.Red: return Color.Red;
                case BarrierType.Green: return Color.Green;
                case BarrierType.Purple: return Color.Purple;
                case BarrierType.Orange: return Color.Orange;
                default: return Color.White;
            }
        }

        public override void Update()
        {
            base.Update();

            level = Scene as Level;
            updateVisuals();
            updateAudio();
        }

        private void updateVisuals()
        {
            updateSpriteState();

            if (isActive)
            {
                light.Alpha = 0.8f + (float)System.Math.Sin(Scene.TimeActive * 6f) * 0.2f;
                sprite.Color = getBarrierColor();

                // Emit particles occasionally
                if (Calc.Random.Next(10) == 0)
                {
                    emitEnergyParticle();
                }
            }
            else
            {
                light.Alpha = 0.1f;
                sprite.Color = getBarrierColor() * 0.3f;
            }
        }

        private void updateSpriteState()
        {
            if (isActive)
            {
                sprite?.Play("active");
            }
            else
            {
                sprite?.Play("inactive");
            }
        }

        private void updateAudio()
        {
            if (isActive)
            {
                if (!barrierSound.Playing)
                {
                    barrierSound.Play("event:/game/05_mirror_temple/crystaltheo_pulse", "pulse", 0f);
                }
            }
            else
            {
                barrierSound?.Stop();
            }
        }

        private void emitEnergyParticle()
        {
            if (level != null)
            {
                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(0f, width),
                    Calc.Random.Range(0f, height)
                );

                Color particleColor = getBarrierColor() * 0.6f;
                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);
            }
        }

        public void SetActive(bool active)
        {
            if (isActive == active) return;

            isActive = active;

            // Update collision
            setupCollision();

            // Play transition sound
            string soundEvent = active ?
                "event:/game/05_mirror_temple/platform_activate" :
                "event:/game/05_mirror_temple/platform_deactivate";
            Audio.Play(soundEvent, Position);

            // Visual effect
            createActivationEffect();
        }

        private void createActivationEffect()
        {
            if (level != null)
            {
                Color effectColor = getBarrierColor();

                for (int i = 0; i < 12; i++)
                {
                    Vector2 particlePos = Position + new Vector2(
                        Calc.Random.Range(0f, width),
                        Calc.Random.Range(0f, height)
                    );

                    level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, effectColor);
                }
            }
        }

        public void Toggle()
        {
            SetActive(!isActive);
        }

        public bool BlocksPlayer()
        {
            return isActive && solidWhenActive && Collidable;
        }

        public override void Render()
        {
            if (isActive)
            {
                // Render energy field effect
                Draw.Rect(Position, width, height, getBarrierColor() * 0.3f);

                // Render border
                Draw.HollowRect(Position, width, height, getBarrierColor() * 0.8f);
            }

            base.Render();
        }
    }
}




