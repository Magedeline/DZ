namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// DataStream - Flowing data particles that carry player
    /// Streams of binary data that push player in a direction
    /// Sprite path: objects/DZ/DZ/data_stream/
    /// </summary>
    [CustomEntity("DZ/DataStream")]
    [Tracked]
    public class DataStream : Actor
    {
        #region Enums
        public enum StreamState
        {
            Inactive,
            Flowing,
            Surging,
            Reversing
        }

        public enum StreamDirection
        {
            Right,
            Left,
            Up,
            Down
        }
        #endregion

        #region Properties
        public StreamState State { get; private set; }
        public StreamDirection Direction { get; private set; }
        public float FlowSpeed { get; private set; }
        public float StreamWidth { get; private set; }
        public float StreamLength { get; private set; }
        public bool IsActive => State != StreamState.Inactive;
        
        private Sprite sprite;
        private Rectangle streamArea;
        private Vector2 flowVector;
        private float stateTimer;
        private Level level;
        private List<DataParticle> dataParticles;
        private List<BinaryDigit> binaryDigits;
        private float particleTimer;
        private float currentSpeed;
        #endregion

        #region Constructor
        public DataStream(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("direction", StreamDirection.Right),
                data.Float("flowSpeed", 150f),
                data.Int("streamWidth", 32),
                data.Int("streamLength", 200)
            );
        }

        public DataStream(Vector2 position, StreamDirection direction = StreamDirection.Right,
            float flowSpeed = 150f, int streamWidth = 32, int streamLength = 200)
            : base(position)
        {
            Initialize(direction, flowSpeed, streamWidth, streamLength);
        }

        private void Initialize(StreamDirection direction, float flowSpeed, int streamWidth, int streamLength)
        {
            Direction = direction;
            FlowSpeed = flowSpeed;
            StreamWidth = streamWidth;
            StreamLength = streamLength;
            
            currentSpeed = flowSpeed;
            flowVector = GetFlowVector(direction);
            
            // Calculate stream area based on direction
            if (direction == StreamDirection.Right || direction == StreamDirection.Left)
            {
                streamArea = new Rectangle((int)Position.X, (int)Position.Y, streamLength, streamWidth);
            }
            else
            {
                streamArea = new Rectangle((int)Position.X, (int)Position.Y, streamWidth, streamLength);
            }
            
            State = StreamState.Flowing;
            stateTimer = 0f;
            particleTimer = 0f;
            dataParticles = new List<DataParticle>();
            binaryDigits = new List<BinaryDigit>();
            
            Collider = new Hitbox(streamArea.Width, streamArea.Height);
            
            Add(sprite = GFX.SpriteBank.Create("data_stream"));
            sprite.Play("flowing");
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            State = StreamState.Flowing;
            currentSpeed = FlowSpeed;
            sprite.Play("flowing");
        }

        public void Deactivate()
        {
            State = StreamState.Inactive;
            currentSpeed = 0f;
            sprite.Play("inactive");
        }

        public void Surge()
        {
            State = StreamState.Surging;
            currentSpeed = FlowSpeed * 2f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.2f);
            
            Add(new Coroutine(SurgeRoutine()));
        }

        public void Reverse()
        {
            State = StreamState.Reversing;
            Direction = (StreamDirection)(((int)Direction + 2) % 4);
            flowVector = GetFlowVector(Direction);
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region Private Methods
        private IEnumerator SurgeRoutine()
        {
            float surgeDuration = 2f;
            stateTimer = surgeDuration;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                
                // Create extra particles
                for (int i = 0; i < 3; i++)
                {
                    CreateDataParticle();
                }
                
                yield return null;
            }
            
            State = StreamState.Flowing;
            currentSpeed = FlowSpeed;
        }

        private Vector2 GetFlowVector(StreamDirection dir)
        {
            return dir switch
            {
                StreamDirection.Right => Vector2.UnitX,
                StreamDirection.Left => -Vector2.UnitX,
                StreamDirection.Up => -Vector2.UnitY,
                StreamDirection.Down => Vector2.UnitY,
                _ => Vector2.UnitX
            };
        }

        private void CreateDataParticle()
        {
            Vector2 startPos = GetParticleStartPosition();
            var particle = new DataParticle(startPos, flowVector * currentSpeed);
            dataParticles.Add(particle);
            Scene.Add(particle);
        }

        private void CreateBinaryDigit()
        {
            Vector2 startPos = GetParticleStartPosition();
            string digit = Calc.Random.Next(2) == 0 ? "0" : "1";
            var binary = new BinaryDigit(startPos, flowVector * currentSpeed, digit);
            binaryDigits.Add(binary);
            Scene.Add(binary);
        }

        private Vector2 GetParticleStartPosition()
        {
            return Direction switch
            {
                StreamDirection.Right => Position + new Vector2(0, Calc.Random.NextFloat() * StreamWidth - StreamWidth / 2),
                StreamDirection.Left => Position + new Vector2(StreamLength, Calc.Random.NextFloat() * StreamWidth - StreamWidth / 2),
                StreamDirection.Up => Position + new Vector2(Calc.Random.NextFloat() * StreamLength - StreamLength / 2, 0),
                StreamDirection.Down => Position + new Vector2(Calc.Random.NextFloat() * StreamLength - StreamLength / 2, StreamWidth),
                _ => Position
            };
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            if (!IsActive) return;
            
            particleTimer += Engine.DeltaTime;
            
            // Create particles
            if (Scene.OnInterval(0.05f))
            {
                CreateDataParticle();
            }
            
            if (Scene.OnInterval(0.1f))
            {
                CreateBinaryDigit();
            }
            
            // Apply flow to player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && streamArea.Contains(new Point((int)player.Position.X, (int)player.Position.Y)))
            {
                player.Speed += flowVector * currentSpeed * Engine.DeltaTime;
            }
            
            dataParticles.RemoveAll(p => p == null || p.Scene == null);
            binaryDigits.RemoveAll(b => b == null || b.Scene == null);
        }

        public override void Render()
        {
            // Draw stream area
            Draw.Rect(streamArea, Color.Cyan * 0.2f);
            
            // Draw flow lines
            int lineCount = (int)(StreamLength / 20f);
            for (int i = 0; i < lineCount; i++)
            {
                float offset = (particleTimer * currentSpeed * 0.5f) % 20f;
                Vector2 linePos = Position + flowVector * (i * 20 + offset);
                Draw.Line(linePos, linePos + flowVector.Perpendicular() * StreamWidth, Color.Cyan * 0.3f, 1f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// DataParticle - Particle in data stream
    /// </summary>
    public class DataParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public DataParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = 1f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 2f, Color.Cyan * (alpha * 0.6f), 3);
        }
    }

    /// <summary>
    /// BinaryDigit - Floating 0 or 1 in data stream
    /// </summary>
    public class BinaryDigit : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private string digit;
        private float scale;

        public BinaryDigit(Vector2 position, Vector2 velocity, string digit)
            : base(position)
        {
            this.velocity = velocity;
            this.digit = digit;
            maxLifetime = Calc.Random.NextFloat() * (1.5f - 0.8f) + 0.8f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            // Would need proper font rendering here
        }
    }
}

