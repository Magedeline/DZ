namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// TowerElevator - Vertical platform that moves between floors
    /// Can be called by player and moves between defined points
    /// Sprite path: objects/DZ/DZ/tower_elevator/
    /// </summary>
    [CustomEntity("DZ/TowerElevator")]
    [Tracked]
    public class TowerElevator : Solid
    {
        #region Enums
        public enum ElevatorState
        {
            Idle,
            Moving,
            Waiting,
            Called
        }
        #endregion

        #region Properties
        public ElevatorState State { get; private set; }
        public float MoveSpeed { get; private set; }
        public float WaitTime { get; private set; }
        public string ElevatorId { get; private set; }
        public bool IsMoving => State == ElevatorState.Moving;
        
        private Sprite sprite;
        private Vector2 startPosition;
        private Vector2 targetPosition;
        private List<Vector2> floorPositions;
        private int currentFloor;
        private float waitTimer;
        private Player ridingPlayer;
        private Level level;
        private List<ElevatorParticle> particles;
        private bool isActivated;
        private SoundSource motorSound;
        #endregion

        #region Constructor
        public TowerElevator(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Initialize(
                data.Float("moveSpeed", 80f),
                data.Float("waitTime", 1f),
                data.Attr("elevatorId", "")
            );
        }

        public TowerElevator(Vector2 position, int width, int height, float moveSpeed = 80f,
            float waitTime = 1f, string elevatorId = "")
            : base(position, width, height, false)
        {
            Initialize(moveSpeed, waitTime, elevatorId);
        }

        private void Initialize(float moveSpeed, float waitTime, string elevatorId)
        {
            MoveSpeed = moveSpeed;
            WaitTime = waitTime;
            ElevatorId = elevatorId;
            
            startPosition = Position;
            targetPosition = Position;
            floorPositions = new List<Vector2> { Position };
            currentFloor = 0;
            waitTimer = 0f;
            isActivated = false;
            particles = new List<ElevatorParticle>();
            
            State = ElevatorState.Idle;
            
            Add(sprite = GFX.SpriteBank.Create("tower_elevator"));
            sprite.Play("idle");
            
            Add(motorSound = new SoundSource());
        }
        #endregion

        #region Public Methods
        public void AddFloor(Vector2 floorPosition)
        {
            floorPositions.Add(floorPosition);
        }

        public void CallToFloor(int floorIndex)
        {
            if (floorIndex < 0 || floorIndex >= floorPositions.Count) return;
            if (State == ElevatorState.Moving) return;
            
            targetPosition = floorPositions[floorIndex];
            State = ElevatorState.Called;
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void MoveUp()
        {
            if (currentFloor >= floorPositions.Count - 1) return;
            CallToFloor(currentFloor + 1);
        }

        public void MoveDown()
        {
            if (currentFloor <= 0) return;
            CallToFloor(currentFloor - 1);
        }

        public void Activate()
        {
            isActivated = true;
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
            
            switch (State)
            {
                case ElevatorState.Idle:
                    // Check for player on elevator
                    ridingPlayer = GetPlayerOnTop();
                    if (ridingPlayer != null && isActivated)
                    {
                        // Check for up/down input
                        if (Input.MoveY.Value > 0.5f)
                            MoveUp();
                        else if (Input.MoveY.Value < -0.5f)
                            MoveDown();
                    }
                    break;
                    
                case ElevatorState.Called:
                    State = ElevatorState.Moving;
                    sprite.Play("moving");
                    motorSound.Play("event:/game/general/diamond_get");
                    break;
                    
                case ElevatorState.Moving:
                    // Move toward target
                    Vector2 direction = (targetPosition - Position).SafeNormalize();
                    Vector2 moveAmount = direction * MoveSpeed * Engine.DeltaTime;
                    
                    // Move elevator and player
                    MoveV(moveAmount.Y);
                    
                    // Create particles
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateParticle();
                    }
                    
                    // Check if reached target
                    if (Vector2.Distance(Position, targetPosition) < 2f)
                    {
                        Position = targetPosition;
                        currentFloor = floorPositions.IndexOf(targetPosition);
                        State = ElevatorState.Waiting;
                        waitTimer = WaitTime;
                        sprite.Play("idle");
                        motorSound.Stop();
                        
                        Audio.Play("event:/game/char_maddy/land", Position);
                    }
                    break;
                    
                case ElevatorState.Waiting:
                    waitTimer -= Engine.DeltaTime;
                    if (waitTimer <= 0f)
                    {
                        State = ElevatorState.Idle;
                    }
                    break;
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        private new Player GetPlayerOnTop()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                // Check if player is standing on elevator
                Rectangle topBounds = new Rectangle(
                    (int)(Left - 2),
                    (int)(Top - 4),
                    (int)Width + 4,
                    8
                );
                
                if (player.Collider.Bounds.Intersects(topBounds) && player.OnGround())
                {
                    return player;
                }
            }
            return null;
        }

        private void CreateParticle()
        {
            var particle = new ElevatorParticle(
                Position + new Vector2(Calc.Random.NextFloat() * Width - Width / 2, Calc.Random.NextFloat() * Height - Height / 2),
                new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f)
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        public override void Render()
        {
            // Draw elevator shaft
            Draw.Rect(Left - 4, Top - 200, 4, 400, Color.DarkGray);
            Draw.Rect(Right, Top - 200, 4, 400, Color.DarkGray);
            
            // Draw floor indicators
            for (int i = 0; i < floorPositions.Count; i++)
            {
                Color indicatorColor = i == currentFloor ? Color.Gold : Color.Gray;
                Draw.Circle(new Vector2(Left - 12, floorPositions[i].Y), 4f, indicatorColor * 0.6f, 4);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// ElevatorParticle - Particle for elevator movement
    /// </summary>
    public class ElevatorParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public ElevatorParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = 0.3f;
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
            Draw.Circle(Position, 3f, Color.Gray * (alpha * 0.4f), 3);
        }
    }

    /// <summary>
    /// ElevatorCallButton - Button to call elevator
    /// </summary>
    [CustomEntity("DZ/ElevatorCallButton")]
    public class ElevatorCallButton : Actor
    {
        private Sprite sprite;
        private string elevatorId;
        private int targetFloor;
        private TowerElevator elevator;

        public ElevatorCallButton(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            elevatorId = data.Attr("elevatorId", "");
            targetFloor = data.Int("targetFloor", 0);
            
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            Add(sprite = GFX.SpriteBank.Create("elevator_button"));
            sprite.Play("idle");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find elevator
            foreach (var elev in scene.Tracker.GetEntities<TowerElevator>())
            {
                var typedElev = (TowerElevator)elev;
                if (typedElev.ElevatorId == elevatorId)
                {
                    elevator = typedElev;
                    break;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                sprite.Play("hover");
                
                if (Input.Grab.Pressed)
                {
                    Press();
                }
            }
            else
            {
                sprite.Play("idle");
            }
        }

        private void Press()
        {
            sprite.Play("pressed");
            Audio.Play("event:/game/general/diamond_get", Position);
            
            elevator?.CallToFloor(targetFloor);
        }
    }
}
