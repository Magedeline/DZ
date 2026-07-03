namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// CardShark - Enemy that throws playing cards in curved arcs
    /// Throws cards that follow bezier curves toward the player
    /// Sprite path: characters/DZ/card_shark/
    /// </summary>
    [CustomEntity("DZ/CardShark")]
    [Tracked]
    public class CardShark : Actor
    {
        #region Enums
        public enum SharkState
        {
            Idle,
            Patrolling,
            Alert,
            Throwing,
            Shuffling,
            Stunned,
            Defeated
        }

        public enum CardSuit
        {
            Hearts,
            Diamonds,
            Clubs,
            Spades
        }
        #endregion

        #region Properties
        public SharkState State { get; private set; }
        public int Health { get; private set; }
        public float DetectionRange { get; private set; }
        public float ThrowInterval { get; private set; }
        public int CardsPerThrow { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 startPosition;
        private float patrolDistance;
        private Facings facing;
        private float throwCooldown;
        private Player targetPlayer;
        private Level level;
        private List<PlayingCardProjectile> activeCards;
        private CardSuit currentSuit;
        private int cardsThrown;
        #endregion

        #region Constructor
        public CardShark(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("detectionRange", 180f),
                data.Float("throwInterval", 1.5f),
                data.Int("cardsPerThrow", 3),
                data.Float("patrolDistance", 80f)
            );
        }

        public CardShark(Vector2 position, int health = 2, float detectionRange = 180f,
            float throwInterval = 1.5f, int cardsPerThrow = 3, float patrolDistance = 80f)
            : base(position)
        {
            Initialize(health, detectionRange, throwInterval, cardsPerThrow, patrolDistance);
        }

        private void Initialize(int health, float detectionRange, float throwInterval, int cardsPerThrow, float patrolDistance)
        {
            Health = health;
            DetectionRange = detectionRange;
            ThrowInterval = throwInterval;
            CardsPerThrow = cardsPerThrow;
            this.patrolDistance = patrolDistance;
            
            startPosition = Position;
            facing = Facings.Right;
            throwCooldown = 0f;
            cardsThrown = 0;
            currentSuit = CardSuit.Hearts;
            activeCards = new List<PlayingCardProjectile>();
            
            State = SharkState.Idle;
            
            // Setup collider
            Collider = new Hitbox(20f, 32f, -10f, -32f);
            
            // Setup sprite
            Add(sprite = DZModule.SpriteBank.Create("card_shark"));
            sprite.Play("idle");
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            sprite.Play("idle");
            State = SharkState.Idle;
        }

        private void PatrollingBegin()
        {
            sprite.Play("walk");
            State = SharkState.Patrolling;
        }

        private void AlertBegin()
        {
            sprite.Play("alert");
            State = SharkState.Alert;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        private void ThrowingBegin()
        {
            sprite.Play("throw");
            State = SharkState.Throwing;
        }

        private void ShufflingBegin()
        {
            sprite.Play("shuffle");
            State = SharkState.Shuffling;
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = SharkState.Stunned;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = SharkState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            yield return 0.5f;
            stateMachine.State = 1; // Patrolling
        }

        private IEnumerator PatrollingRoutine()
        {
            Vector2 patrolTarget = startPosition + new Vector2(patrolDistance, 0);
            
            while (true)
            {
                // Patrol movement
                Vector2 direction = (patrolTarget - Position).SafeNormalize();
                MoveH(direction.X * 40f * Engine.DeltaTime);
                
                // Flip at bounds
                if (Position.X >= patrolTarget.X && facing == Facings.Right)
                {
                    facing = Facings.Left;
                    patrolTarget = startPosition - new Vector2(patrolDistance, 0);
                    sprite.Scale.X = -1;
                }
                else if (Position.X <= patrolTarget.X && facing == Facings.Left)
                {
                    facing = Facings.Right;
                    patrolTarget = startPosition + new Vector2(patrolDistance, 0);
                    sprite.Scale.X = 1;
                }
                
                // Check for player
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 2; // Alert
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator AlertRoutine()
        {
            // Face player
            if (targetPlayer != null)
            {
                facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                sprite.Scale.X = facing == Facings.Right ? 1 : -1;
            }
            
            yield return 0.3f;
            
            stateMachine.State = 3; // Throwing
        }

        private IEnumerator ThrowingRoutine()
        {
            // Throw cards
            for (int i = 0; i < CardsPerThrow; i++)
            {
                ThrowCard();
                yield return 0.15f;
            }
            
            cardsThrown += CardsPerThrow;
            
            // Shuffle after certain number of throws
            if (cardsThrown >= CardsPerThrow * 3)
            {
                stateMachine.State = 4; // Shuffling
            }
            else
            {
                yield return ThrowInterval;
                
                // Check if player still in range
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 3; // Throwing
                }
                else
                {
                    stateMachine.State = 1; // Patrolling
                }
            }
        }

        private IEnumerator ShufflingRoutine()
        {
            // Shuffle animation - change suit
            yield return 0.8f;
            
            // Change to next suit
            currentSuit = (CardSuit)(((int)currentSuit + 1) % 4);
            cardsThrown = 0;
            
            stateMachine.State = 3; // Throwing
        }

        private IEnumerator StunnedRoutine()
        {
            float stunDuration = 1f;
            while (stunDuration > 0f)
            {
                stunDuration -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 3; // Throwing
        }

        private IEnumerator DefeatedRoutine()
        {
            // Scatter remaining cards
            for (int i = 0; i < 5; i++)
            {
                var card = new PlayingCardProjectile(
                    Position - Vector2.UnitY * 16f,
                    new Vector2(Calc.Random.NextFloat() * 50f - 25f, Calc.Random.NextFloat() * 50f - 25f),
                    currentSuit,
                    true
                );
                activeCards.Add(card);
                Scene.Add(card);
            }
            
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 8, Position, Vector2.One * 6f);
            
            yield return 0.5f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void ThrowCard()
        {
            if (targetPlayer == null) return;
            
            // Calculate throw arc
            Vector2 start = Position - Vector2.UnitY * 16f;
            Vector2 end = targetPlayer.Position;
            
            // Create curved trajectory
            Vector2 controlPoint = (start + end) / 2 + new Vector2(0f, -60f);
            
            var card = new PlayingCardProjectile(start, end, controlPoint, currentSuit);
            activeCards.Add(card);
            Scene.Add(card);
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == SharkState.Defeated) return;
            
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (Health <= 0)
            {
                stateMachine.State = 6; // Defeated
            }
            else
            {
                stateMachine.State = 5; // Stunned
            }
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
            activeCards.RemoveAll(c => c == null || c.Scene == null);
        }
        #endregion
    }

    /// <summary>
    /// PlayingCardProjectile - Card that follows a curved path
    /// </summary>
    public class PlayingCardProjectile : Actor
    {
        private Vector2 start;
        private Vector2 end;
        private Vector2 control;
        private float t;
        private float speed;
        private CardShark.CardSuit suit;
        private Sprite sprite;
        private bool isDropped;
        private Vector2 velocity;
        private float rotation;

        // Bezier curve constructor
        public PlayingCardProjectile(Vector2 start, Vector2 end, Vector2 control, CardShark.CardSuit suit)
            : base(start)
        {
            this.start = start;
            this.end = end;
            this.control = control;
            this.suit = suit;
            t = 0f;
            speed = 1.5f;
            isDropped = false;
            
            Collider = new Hitbox(12f, 16f, -6f, -8f);
            Add(sprite = GFX.SpriteBank.Create("playing_card"));
            sprite.Play(suit.ToString().ToLower());
        }

        // Dropped card constructor
        public PlayingCardProjectile(Vector2 position, Vector2 velocity, CardShark.CardSuit suit, bool dropped)
            : base(position)
        {
            this.velocity = velocity;
            this.suit = suit;
            isDropped = dropped;
            rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            
            Collider = new Hitbox(12f, 16f, -6f, -8f);
            
            Add(sprite = GFX.SpriteBank.Create("playing_card"));
            sprite.Play(suit.ToString().ToLower());
        }

        public override void Update()
        {
            base.Update();
            
            if (isDropped)
            {
                // Simple physics for dropped card
                Position += velocity * Engine.DeltaTime;
                velocity.Y += 200f * Engine.DeltaTime;
                rotation += Engine.DeltaTime * 5f;
                sprite.Rotation = rotation;
                
                if (OnGround())
                {
                    RemoveSelf();
                }
            }
            else
            {
                // Bezier curve movement
                t += speed * Engine.DeltaTime;
                
                if (t >= 1f)
                {
                    RemoveSelf();
                    return;
                }
                
                // Quadratic bezier
                Position = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end;
                
                // Rotate based on trajectory
                Vector2 tangent = 2 * (1 - t) * (control - start) + 2 * t * (end - control);
                sprite.Rotation = Calc.Angle(tangent);
            }
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
            }
        }

        public override void Render()
        {
            // Draw card trail
            if (!isDropped)
            {
                Draw.Circle(Position, 6f, GetSuitColor() * 0.3f, 4);
            }
            base.Render();
        }

        private Color GetSuitColor()
        {
            return suit switch
            {
                CardShark.CardSuit.Hearts => Color.Red,
                CardShark.CardSuit.Diamonds => Color.Red,
                CardShark.CardSuit.Clubs => Color.Black,
                CardShark.CardSuit.Spades => Color.Black,
                _ => Color.White
            };
        }
    }
}
