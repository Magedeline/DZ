namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// AbyssalEye - Floating eye that tracks player
    /// Creates gaze beam that damages if looked at
    /// Sprite path: characters/DZ/abyssal_eye/
    /// </summary>
    [CustomEntity("DZ/AbyssalEye")]
    [Tracked]
    public class AbyssalEye : Actor
    {
        #region Enums
        public enum EyeState
        {
            Dormant,
            Opening,
            Watching,
            Gazing,
            Closing,
            Destroyed
        }
        #endregion

        #region Properties
        public EyeState State { get; private set; }
        public float GazeRange { get; private set; }
        public float GazeWidth { get; private set; }
        public int Health { get; private set; }
        
        private Sprite sprite;
        private float gazeTimer;
        private Player targetPlayer;
        private Level level;
        private List<EyeBeam> beams;
        private VertexLight eyeLight;
        private Color eyeColor;
        private bool isGazing;
        private float closingTimer;
        #endregion

        #region Constructor
        public AbyssalEye(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Float("gazeRange", 200f), data.Float("gazeWidth", 30f), data.Int("health", 2));
        }

        public AbyssalEye(Vector2 position, float gazeRange = 200f, float gazeWidth = 30f, int health = 2)
            : base(position)
        {
            Initialize(gazeRange, gazeWidth, health);
        }

        private void Initialize(float gazeRange, float gazeWidth, int health)
        {
            GazeRange = gazeRange;
            GazeWidth = gazeWidth;
            Health = health;
            
            State = EyeState.Dormant;
            gazeTimer = 0f;
            beams = new List<EyeBeam>();
            eyeColor = Color.DarkRed;
            isGazing = false;
            
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            
            Add(sprite = DZModule.SpriteBank.Create("abyssal_eye"));
            sprite.Play("dormant");
            
            Add(eyeLight = new VertexLight(eyeColor, 0.4f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Open()
        {
            if (State != EyeState.Dormant) return;
            
            State = EyeState.Opening;
            sprite.Play("opening");
            eyeLight.Alpha = 0.6f;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(OpenRoutine()));
        }

        public void Damage(int amount)
        {
            Health -= amount;
            
            if (Health <= 0)
            {
                State = EyeState.Destroyed;
                sprite.Play("destroyed");
                
                Audio.Play("event:/game/char_badeline/disappear", Position);
                level?.Shake(0.3f);
                
                RemoveSelf();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator OpenRoutine()
        {
            yield return 0.5f;
            State = EyeState.Watching;
            sprite.Play("watching");
        }

        private void CreateGazeBeam()
        {
            if (targetPlayer == null) return;
            
            Vector2 gazeDir = (targetPlayer.Position - Position).SafeNormalize();
            var beam = new EyeBeam(Position, gazeDir, GazeRange, GazeWidth, eyeColor);
            beams.Add(beam);
            Scene.Add(beam);
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
                case EyeState.Watching:
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    
                    if (targetPlayer != null)
                    {
                        float distance = Vector2.Distance(Position, targetPlayer.Position);
                        
                        // Face player
                        sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                        
                        if (distance < GazeRange)
                        {
                            State = EyeState.Gazing;
                            gazeTimer = 2f;
                            isGazing = true;
                            sprite.Play("gazing");
                            
                            Audio.Play("event:/game/char_badeline/beam_launch", Position);
                        }
                    }
                    break;
                    
                case EyeState.Gazing:
                    gazeTimer -= Engine.DeltaTime;
                    
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateGazeBeam();
                    }
                    
                    if (gazeTimer <= 0f)
                    {
                        State = EyeState.Closing;
                        isGazing = false;
                        sprite.Play("closing");
                    }
                    break;
                    
                case EyeState.Closing:
                    closingTimer += Engine.DeltaTime;
                    if (closingTimer >= 0.5f)
                    {
                        State = EyeState.Watching;
                        sprite.Play("watching");
                        closingTimer = 0f;
                    }
                    break;
            }
            
            beams.RemoveAll(b => b == null || b.Scene == null);
        }

        public override void Render()
        {
            // Draw gaze indicator
            if (State == EyeState.Gazing)
            {
                Draw.Circle(Position, GazeWidth, eyeColor * 0.2f, 8);
            }
            base.Render();
        }
        #endregion
    }

    public class EyeBeam : Actor
    {
        private Vector2 direction;
        private float range;
        private float width;
        private Color color;
        private float lifetime;

        public EyeBeam(Vector2 position, Vector2 direction, float range, float width, Color color)
            : base(position)
        {
            this.direction = direction;
            this.range = range;
            this.width = width;
            this.color = color;
            lifetime = 0.5f;
        }

        public override void Update()
        {
            base.Update();
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && IsPlayerInBeam(player))
            {
                player.Die(Vector2.Zero);
            }
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        private bool IsPlayerInBeam(Player player)
        {
            Vector2 toPlayer = player.Position - Position;
            float distance = toPlayer.Length();
            
            if (distance > range) return false;
            
            toPlayer.Normalize();
            float dot = Vector2.Dot(direction, toPlayer);
            return dot > 0.9f;
        }

        public override void Render()
        {
            float alpha = lifetime * 2f;
            Vector2 end = Position + direction * range;
            Draw.Line(Position, end, color * (alpha * 0.4f), (int)width);
        }
    }
}
