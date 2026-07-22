namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// MatrixRain - Falling digital rain effect with code characters
    /// Creates atmospheric digital environment
    /// Sprite path: effects/matrix_rain/
    /// </summary>
    [CustomEntity("DZ/MatrixRain")]
    [Tracked]
    public class MatrixRain : Actor
    {
        #region Enums
        public enum RainState
        {
            Inactive,
            Light,
            Normal,
            Heavy,
            Intense
        }
        #endregion

        #region Properties
        public RainState State { get; private set; }
        public float RainWidth { get; private set; }
        public float RainHeight { get; private set; }
        public float DropSpeed { get; private set; }
        public float Density { get; private set; }
        public bool IsActive => State != RainState.Inactive;
        
        private List<MatrixColumn> columns;
        private Level level;
        private Random random;
        private Color rainColor;
        private float intensity;
        #endregion

        #region Constructor
        public MatrixRain(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("rainWidth", 200f),
                data.Float("rainHeight", 400f),
                data.Float("dropSpeed", 150f),
                data.Float("density", 0.5f),
                data.Enum("intensity", RainState.Normal)
            );
        }

        public MatrixRain(Vector2 position, float rainWidth = 200f, float rainHeight = 400f,
            float dropSpeed = 150f, float density = 0.5f, RainState intensity = RainState.Normal)
            : base(position)
        {
            Initialize(rainWidth, rainHeight, dropSpeed, density, intensity);
        }

        private void Initialize(float rainWidth, float rainHeight, float dropSpeed, float density, RainState intensity)
        {
            RainWidth = rainWidth;
            RainHeight = rainHeight;
            DropSpeed = dropSpeed;
            Density = density;
            
            State = intensity;
            columns = new List<MatrixColumn>();
            random = new Random();
            rainColor = Color.Green;
            
            // Calculate intensity multiplier
            this.intensity = intensity switch
            {
                RainState.Light => 0.3f,
                RainState.Normal => 1f,
                RainState.Heavy => 1.5f,
                RainState.Intense => 2f,
                _ => 0f
            };
            
            Collider = new Hitbox(rainWidth, rainHeight);
        }
        #endregion

        #region Public Methods
        public void SetIntensity(RainState newState)
        {
            State = newState;
            intensity = newState switch
            {
                RainState.Light => 0.3f,
                RainState.Normal => 1f,
                RainState.Heavy => 1.5f,
                RainState.Intense => 2f,
                _ => 0f
            };
            
            // Recreate columns
            CreateColumns();
        }

        public void SetColor(Color color)
        {
            rainColor = color;
            foreach (var column in columns)
            {
                column.SetColor(color);
            }
        }
        #endregion

        #region Private Methods
        private void CreateColumns()
        {
            columns.Clear();
            
            int columnCount = (int)(RainWidth / 12f * Density * intensity);
            
            for (int i = 0; i < columnCount; i++)
            {
                float x = Position.X + random.NextFloat(RainWidth);
                float speed = DropSpeed * (0.8f + random.NextFloat(0.4f)) * intensity;
                int length = random.Next(5, 15);
                
                var column = new MatrixColumn(
                    new Vector2(x, Position.Y - random.NextFloat(RainHeight)),
                    speed,
                    length,
                    rainColor
                );
                columns.Add(column);
                Scene.Add(column);
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            CreateColumns();
        }

        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            // Random intensity fluctuation
            if (Scene.OnInterval(5f))
            {
                if (random.NextDouble() < 0.2f)
                {
                    // Random intensity change
                    RainState[] states = { RainState.Light, RainState.Normal, RainState.Heavy };
                    SetIntensity(states[random.Next(states.Length)]);
                }
            }
        }

        public override void Render()
        {
            // Draw rain area background
            Draw.Rect(Position.X, Position.Y, RainWidth, RainHeight, rainColor * 0.05f);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// MatrixColumn - Single column of falling characters
    /// </summary>
    public class MatrixColumn : Actor
    {
        private float speed;
        private int length;
        private List<MatrixCharacter> characters;
        private Color color;
        private float minY;
        private float maxY;
        private Random random;

        public MatrixColumn(Vector2 position, float speed, int length, Color color)
            : base(position)
        {
            this.speed = speed;
            this.length = length;
            this.color = color;
            random = new Random();
            characters = new List<MatrixCharacter>();
            
            // Create characters
            for (int i = 0; i < length; i++)
            {
                var ch = new MatrixCharacter(
                    Position - Vector2.UnitY * i * 12,
                    GetRandomChar(),
                    color,
                    i == 0 // First character is brighter
                );
                characters.Add(ch);
            }
        }

        public void SetColor(Color newColor)
        {
            color = newColor;
            foreach (var ch in characters)
            {
                ch.SetColor(color);
            }
        }

        private char GetRandomChar()
        {
            // Mix of numbers, letters, and symbols
            string chars = "0123456789ABCDEF@#$%&*+-=/\\|";
            return chars[random.Next(chars.Length)];
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            minY = Position.Y - 400;
            maxY = Position.Y + 400;
            
            foreach (var ch in characters)
            {
                scene.Add(ch);
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Move column down
            Position += Vector2.UnitY * speed * Engine.DeltaTime;
            
            // Update character positions
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].Position = Position - Vector2.UnitY * i * 12;
                
                // Random character change
                if (random.NextDouble() < 0.05)
                {
                    characters[i].SetChar(GetRandomChar());
                }
            }
            
            // Reset when off screen
            if (Position.Y > maxY)
            {
                Position = new Vector2(Position.X, minY);
            }
        }
    }

    /// <summary>
    /// MatrixCharacter - Single character in matrix rain
    /// </summary>
    public class MatrixCharacter : Actor
    {
        private char character;
        private Color color;
        private bool isHead;
        private float alpha;

        public MatrixCharacter(Vector2 position, char character, Color color, bool isHead)
            : base(position)
        {
            this.character = character;
            this.color = color;
            this.isHead = isHead;
            alpha = isHead ? 1f : 0.6f;
        }

        public void SetChar(char c)
        {
            character = c;
        }

        public void SetColor(Color c)
        {
            color = c;
        }

        public override void Render()
        {
            Color renderColor = isHead ? Color.White : color;
            // Would need proper font rendering here
        }
    }
}
