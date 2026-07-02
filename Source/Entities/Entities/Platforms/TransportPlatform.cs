namespace Celeste.Entities
{
    /// <summary>
    /// Moving transport platform with different materials and movement patterns
    /// </summary>
    [CustomEntity(ids: "DZ/TransportPlatform")]
    public class TransportPlatform : Solid
    {
        public enum PlatformType
        {
            Stone,
            Wood,
            Magical,
            Ancient
        }

        private PlatformType platformType;
        private bool isActive;
        private float moveSpeed;
        private float waitTime;
        private Vector2 start;
        private Vector2[] nodes;
        private int currentNode;
        private float waitTimer;
        private bool waiting;
        private Sprite sprite;
        private SoundSource moveSound;

        public bool IsActive => isActive;

        public TransportPlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            string typeString = data.Attr(nameof(platformType), "stone");
            if (!System.Enum.TryParse(typeString, true, out platformType))
            {
                platformType = PlatformType.Stone;
            }

            isActive = data.Bool(nameof(isActive), true);
            moveSpeed = data.Float(nameof(moveSpeed), 60f);
            waitTime = data.Float(nameof(waitTime), 2f);

            start = Position;
            currentNode = 0;
            waitTimer = 0f;
            waiting = false;

            setupSprite();
            setupAudio();
            setupNodes(data);

            Depth = 1;
        }

        private void setupSprite()
        {
            string spriteName = $"platform_{platformType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                // Fallback sprite
                sprite = new Sprite(GFX.Game, "objects/DZ/DZ/moveBlock/");
                sprite.AddLoop("idle", "base", 0.1f);
                sprite.AddLoop("moving", "base", 0.05f);
            }

            Add(sprite);
            sprite.Play("idle");

            // Color based on platform type
            Color platformColor = getPlatformColor();
            sprite.Color = platformColor;
        }

        private Color getPlatformColor()
        {
            switch (platformType)
            {
                case PlatformType.Stone: return Color.Gray;
                case PlatformType.Wood: return Color.SaddleBrown;
                case PlatformType.Magical: return Color.Purple;
                case PlatformType.Ancient: return Color.DarkGreen;
                default: return Color.White;
            }
        }

        private void setupAudio()
        {
            moveSound = new SoundSource();
            Add(moveSound);
        }

        private void setupNodes(EntityData data)
        {
            // Get nodes from entity data
            Vector2[] dataNodes = data.NodesOffset(Vector2.Zero);
            nodes = new Vector2[dataNodes.Length + 1];
            nodes[0] = start;

            for (int i = 0; i < dataNodes.Length; i++)
            {
                nodes[i + 1] = dataNodes[i];
            }

            // If no nodes specified, create a simple back-and-forth pattern
            if (nodes.Length <= 1)
            {
                nodes = new Vector2[]
                {
                    start,
                    start + new Vector2(100, 0) // Move 100 pixels to the right
                };
            }
        }

        public override void Update()
        {
            base.Update();

            if (!isActive || nodes.Length <= 1) return;

            if (waiting)
            {
                waitTimer += Engine.DeltaTime;
                if (waitTimer >= waitTime)
                {
                    waiting = false;
                    waitTimer = 0f;
                    sprite?.Play("moving");

                    // Start move sound
                    string soundEvent = getMoveSoundForType();
                    moveSound?.Play(soundEvent);
                }
            }
            else
            {
                moveTowardsTarget();
            }

            updateParticleEffects();
        }

        private void moveTowardsTarget()
        {
            Vector2 target = nodes[currentNode];
            Vector2 direction = (target - Position).SafeNormalize();
            float distance = Vector2.Distance(Position, target);

            if (distance < 2f)
            {
                // Reached target
                Position = target;
                currentNode = (currentNode + 1) % nodes.Length;
                waiting = true;
                sprite?.Play("idle");
                moveSound?.Stop();

                // Platform arrival sound
                Audio.Play("event:/game/04_cliffside/platform_vertical_flag", Position);
            }
            else
            {
                // Move towards target
                Vector2 movement = direction * moveSpeed * Engine.DeltaTime;
                MoveH(movement.X);
                MoveV(movement.Y);
            }
        }

        private string getMoveSoundForType()
        {
            switch (platformType)
            {
                case PlatformType.Stone: return "event:/game/04_cliffside/platform_vertical_move";
                case PlatformType.Wood: return "event:/game/03_resort/platform_vert_move";
                case PlatformType.Magical: return "event:/game/05_mirror_temple/platform_activate";
                case PlatformType.Ancient: return "event:/game/06_reflection/platform_horizontal_move";
                default: return "event:/game/04_cliffside/platform_vertical_move";
            }
        }

        private void updateParticleEffects()
        {
            if (!waiting && isActive)
            {
                var level = Scene as Level;
                if (level != null && Calc.Random.Next(8) == 0)
                {
                    Vector2 particlePos = Position + new Vector2(
                        Calc.Random.Range(-Width/2, Width/2),
                        Height + 2
                    );

                    Color particleColor = getPlatformColor() * 0.6f;
                    level.ParticlesBG.Emit(ParticleTypes.Dust, particlePos, particleColor);
                }
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;

            if (!active)
            {
                moveSound?.Stop();
                sprite?.Play("idle");
            }
        }

        public void ResetToStart()
        {
            Position = start;
            currentNode = 0;
            waiting = false;
            waitTimer = 0f;
            moveSound?.Stop();
            sprite?.Play("idle");
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            // Handle objects riding on the platform
            base.OnStaticMoverTrigger(sm);
        }
    }
}




