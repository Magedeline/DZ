namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// SwingingDoor - Double doors that swing based on player momentum
    /// Can knock player back if hit with enough force
    /// Sprite path: objects/DZ/DZ/swinging_door/
    /// </summary>
    [CustomEntity("DZ/SwingingDoor")]
    [Tracked]
    public class SwingingDoor : Actor
    {
        #region Enums
        public enum DoorState
        {
            Closed,
            OpeningLeft,
            OpeningRight,
            Open,
            Closing,
            Locked
        }
        #endregion

        #region Properties
        public DoorState State { get; private set; }
        public float SwingSpeed { get; private set; }
        public float KnockbackForce { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsDoubleDoor { get; private set; }
        
        private Sprite leftDoorSprite;
        private Sprite rightDoorSprite;
        private float leftDoorAngle;
        private float rightDoorAngle;
        private float targetLeftAngle;
        private float targetRightAngle;
        private float closeTimer;
        private float autoCloseTime;
        private Level level;
        private SoundSource doorSound;
        private bool isOpening;
        #endregion

        #region Constructor
        public SwingingDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("swingSpeed", 3f),
                data.Float("knockbackForce", 150f),
                data.Bool("isLocked", false),
                data.Bool("isDoubleDoor", true),
                data.Float("autoCloseTime", 2f)
            );
        }

        public SwingingDoor(Vector2 position, float swingSpeed = 3f, float knockbackForce = 150f,
            bool isLocked = false, bool isDoubleDoor = true, float autoCloseTime = 2f)
            : base(position)
        {
            Initialize(swingSpeed, knockbackForce, isLocked, isDoubleDoor, autoCloseTime);
        }

        private void Initialize(float swingSpeed, float knockbackForce, bool isLocked, bool isDoubleDoor, float autoCloseTime)
        {
            SwingSpeed = swingSpeed;
            KnockbackForce = knockbackForce;
            IsLocked = isLocked;
            IsDoubleDoor = isDoubleDoor;
            this.autoCloseTime = autoCloseTime;
            
            State = isLocked ? DoorState.Locked : DoorState.Closed;
            leftDoorAngle = 0f;
            rightDoorAngle = 0f;
            targetLeftAngle = 0f;
            targetRightAngle = 0f;
            closeTimer = 0f;
            isOpening = false;
            
            // Setup collider - door frame
            Collider = new Hitbox(48f, 64f, -24f, -64f);
            
            // Setup sprites
            Add(leftDoorSprite = GFX.SpriteBank.Create("swinging_door_left"));
            leftDoorSprite.Play("closed");
            leftDoorSprite.Position = new Vector2(-12f, -32f);
            
            if (IsDoubleDoor)
            {
                Add(rightDoorSprite = GFX.SpriteBank.Create("swinging_door_right"));
                rightDoorSprite.Play("closed");
                rightDoorSprite.Position = new Vector2(12f, -32f);
            }
            
            // Sound
            Add(doorSound = new SoundSource());
        }
        #endregion

        #region Public Methods
        public void Open(bool fromLeft)
        {
            if (IsLocked) return;
            
            isOpening = true;
            
            if (fromLeft)
            {
                State = DoorState.OpeningLeft;
                targetLeftAngle = -MathHelper.PiOver2;
                if (IsDoubleDoor) targetRightAngle = MathHelper.PiOver4;
            }
            else
            {
                State = DoorState.OpeningRight;
                targetRightAngle = MathHelper.PiOver2;
                if (IsDoubleDoor) targetLeftAngle = -MathHelper.PiOver4;
            }
            
            Audio.Play("event:/game/general/diamond_get", Position);
            closeTimer = autoCloseTime;
        }

        public void Close()
        {
            State = DoorState.Closing;
            targetLeftAngle = 0f;
            targetRightAngle = 0f;
            isOpening = false;
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Lock()
        {
            IsLocked = true;
            State = DoorState.Locked;
            Close();
        }

        public void Unlock()
        {
            IsLocked = false;
            State = DoorState.Closed;
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
            
            // Animate door angles
            leftDoorAngle = Calc.Approach(leftDoorAngle, targetLeftAngle, SwingSpeed * Engine.DeltaTime);
            rightDoorAngle = Calc.Approach(rightDoorAngle, targetRightAngle, SwingSpeed * Engine.DeltaTime);
            
            // Apply rotation to sprites
            leftDoorSprite.Rotation = leftDoorAngle;
            if (IsDoubleDoor) rightDoorSprite.Rotation = rightDoorAngle;
            
            // Check for player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                HandlePlayerCollision(player);
            }
            
            // Auto close timer
            if (isOpening && State != DoorState.Closing)
            {
                closeTimer -= Engine.DeltaTime;
                if (closeTimer <= 0f)
                {
                    Close();
                }
            }
            
            // Check if fully closed
            if (State == DoorState.Closing && leftDoorAngle == 0f && rightDoorAngle == 0f)
            {
                State = DoorState.Closed;
                isOpening = false;
            }
            
            // Check if fully open
            if ((State == DoorState.OpeningLeft || State == DoorState.OpeningRight) &&
                Math.Abs(leftDoorAngle - targetLeftAngle) < 0.01f &&
                Math.Abs(rightDoorAngle - targetRightAngle) < 0.01f)
            {
                State = DoorState.Open;
            }
        }

        private void HandlePlayerCollision(Player player)
        {
            // Get player speed
            float playerSpeed = player.Speed.Length();
            
            // Check if player is moving toward door
            bool movingRight = player.Speed.X > 50f;
            bool movingLeft = player.Speed.X < -50f;
            
            // Door collision bounds
            Rectangle doorBounds = Collider.Bounds;
            
            if (player.Collider.Bounds.Intersects(doorBounds))
            {
                if (State == DoorState.Closed || State == DoorState.Locked)
                {
                    // Player hit closed door
                    if (playerSpeed > 100f && !IsLocked)
                    {
                        // Open door with momentum
                        Open(player.Position.X < Position.X);
                    }
                    else if (playerSpeed > 200f)
                    {
                        // Knockback
                        player.Speed = new Vector2(-player.Speed.X * 0.5f, 0f);
                        Audio.Play("event:/game/char_maddy/land", Position);
                    }
                }
                else if (State == DoorState.OpeningLeft || State == DoorState.OpeningRight || State == DoorState.Open)
                {
                    // Player passing through - let them pass
                }
                else if (State == DoorState.Closing)
                {
                    // Door closing on player - push them through or knockback
                    if (player.Position.X < Position.X)
                    {
                        player.Speed.X = -KnockbackForce;
                    }
                    else
                    {
                        player.Speed.X = KnockbackForce;
                    }
                }
            }
        }

        public override void Render()
        {
            // Draw door frame
            Draw.Rect(Position.X - 24, Position.Y - 68, 48, 4, Color.Brown);
            Draw.Rect(Position.X - 26, Position.Y - 64, 4, 64, Color.Brown);
            Draw.Rect(Position.X + 22, Position.Y - 64, 4, 64, Color.Brown);
            
            base.Render();
            
            // Draw lock indicator
            if (IsLocked)
            {
                Draw.Circle(Position - Vector2.UnitY * 30f, 6f, Color.Gold * 0.8f, 8);
            }
        }
        #endregion
    }

    /// <summary>
    /// SaloonDoorController - Controls multiple saloon doors
    /// </summary>
    [CustomEntity("DZ/SaloonDoorController")]
    public class SaloonDoorController : Entity
    {
        private List<SwingingDoor> doors;
        private bool openAll;

        public SaloonDoorController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            openAll = data.Bool("openAll", false);
            doors = new List<SwingingDoor>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find all doors in scene
            foreach (var door in scene.Tracker.GetEntities<SwingingDoor>())
            {
                doors.Add((SwingingDoor)door);
            }
            
            if (openAll)
            {
                OpenAll();
            }
        }

        public void OpenAll()
        {
            foreach (var door in doors)
            {
                door.Open(true);
            }
        }

        public void CloseAll()
        {
            foreach (var door in doors)
            {
                door.Close();
            }
        }

        public void LockAll()
        {
            foreach (var door in doors)
            {
                door.Lock();
            }
        }

        public void UnlockAll()
        {
            foreach (var door in doors)
            {
                door.Unlock();
            }
        }
    }
}
