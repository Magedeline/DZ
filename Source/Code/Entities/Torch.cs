namespace Celeste.Entities
{
    /// <summary>
    /// Interactive torch entity with lighting effects and multiple types
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/Torch")]
    public class Torch : Entity
    {
        public enum TorchType
        {
            Wall,
            Floor,
            Magical,
            Eternal
        }

        private Sprite sprite;
        private TorchType torchType;
        private bool isLit;
        private float lightRadius;
        private VertexLight light;
        private SoundSource ambientSound;
        private float flickerTimer;
        private float baseAlpha;

        public bool IsLit => isLit;
        public float LightRadius => lightRadius;

        public Torch(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            string typeString = data.Attr(nameof(torchType), "wall");
            if (!System.Enum.TryParse(typeString, true, out torchType))
            {
                torchType = TorchType.Wall;
            }

            isLit = data.Bool(nameof(isLit), true);
            lightRadius = data.Float(nameof(lightRadius), 64f);

            setupSprite();
            setupLighting();
            setupCollision();
            setupAudio();

            Depth = -1000;
            baseAlpha = 1f;
            flickerTimer = 0f;
        }

        private void setupSprite()
        {
            string spriteName = getSpriteNameForType();

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                // Fallback sprite
                sprite = new Sprite(GFX.Game, "objects/temple/");
                sprite.AddLoop("lit", "torch00", 0.1f);
                sprite.AddLoop("unlit", "torch01", 0.1f);
                sprite.AddLoop("magical", "torch00", 0.05f); // Faster animation for magical
            }

            Add(sprite);
            updateSpriteState();
        }

        private string getSpriteNameForType()
        {
            switch (torchType)
            {
                case TorchType.Wall: return "torch_wall";
                case TorchType.Floor: return "torch_floor";
                case TorchType.Magical: return "torch_magical";
                case TorchType.Eternal: return "torch_eternal";
                default: return "torch_wall";
            }
        }

        private void setupLighting()
        {
            if (isLit)
            {
                Color lightColor = getLightColor();
                light = new VertexLight(lightColor, lightRadius, 1, (int)(lightRadius * 1.5f));
                Add(light);
            }
        }

        private Color getLightColor()
        {
            switch (torchType)
            {
                case TorchType.Wall:
                case TorchType.Floor:
                    return Color.Orange;
                case TorchType.Magical:
                    return Color.Purple;
                case TorchType.Eternal:
                    return Color.Cyan;
                default:
                    return Color.Orange;
            }
        }

        private void setupCollision()
        {
            Collider = new Hitbox(16f, 32f, -8f, -32f);
            Add(new PlayerCollider(OnPlayerInteract));
        }

        private void setupAudio()
        {
            if (isLit)
            {
                ambientSound = new SoundSource();
                Add(ambientSound);
                ambientSound.Play("event:/env/local/09_core/fireballs_idle");
            }
        }

        private void updateSpriteState()
        {
            if (isLit)
            {
                if (torchType == TorchType.Magical)
                    sprite?.Play("magical");
                else
                    sprite?.Play("lit");
            }
            else
            {
                sprite?.Play("unlit");
            }
        }

        public override void Update()
        {
            base.Update();

            if (isLit)
            {
                updateFlicker();
                updateParticles();
            }
        }

        private void updateFlicker()
        {
            flickerTimer += Engine.DeltaTime * 8f;

            // Create subtle flicker effect
            float flicker = 0.9f + 0.1f * (float)System.Math.Sin(flickerTimer);

            if (light != null)
            {
                light.Alpha = flicker * baseAlpha;
            }

            if (sprite != null)
            {
                sprite.Color = Color.White * flicker;
            }
        }

        private void updateParticles()
        {
            var level = Scene as Level;
            if (level == null) return;

            // Emit flame particles periodically
            if (Calc.Random.Next(5) == 0)
            {
                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(-4f, 4f),
                    Calc.Random.Range(-8f, -16f)
                );

                Color particleColor = getLightColor();
                level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);
            }

            // Special effects for magical torches
            if (torchType == TorchType.Magical && Calc.Random.Next(3) == 0)
            {
                Vector2 magicPos = Position + new Vector2(
                    Calc.Random.Range(-8f, 8f),
                    Calc.Random.Range(-4f, -20f)
                );
                level.ParticlesBG.Emit(ParticleTypes.Dust, magicPos, Color.Purple);
            }
        }

        private void OnPlayerInteract(global::Celeste.Player player)
        {
            // Allow player to light/extinguish torch (except eternal type)
            if (torchType != TorchType.Eternal)
            {
                ToggleLit();
            }
        }

        public void ToggleLit()
        {
            isLit = !isLit;

            if (isLit)
            {
                setupLighting();
                setupAudio();
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
            }
            else
            {
                if (light != null)
                {
                    Remove(light);
                    light = null;
                }

                if (ambientSound != null)
                {
                    ambientSound.Stop();
                    Remove(ambientSound);
                    ambientSound = null;
                }

                Audio.Play("event:/ui/main/button_back", Position);
            }

            updateSpriteState();
        }

        public void SetLit(bool lit)
        {
            if (isLit != lit)
            {
                ToggleLit();
            }
        }
    }
}





