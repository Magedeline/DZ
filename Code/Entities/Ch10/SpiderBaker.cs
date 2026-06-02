namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// SpiderBaker - Friendly spider that becomes hostile when attacked
    /// Drops from webs and throws baked goods as projectiles
    /// Can be befriended if player approaches peacefully
    /// Sprite path: characters/spider_baker/
    /// Baked good sprite path: characters/baked_good/
    /// </summary>
    [CustomEntity("MaggyHelper/SpiderBaker")]
    [Tracked]
    public class SpiderBaker : Actor
    {
        #region Enums
        public enum SpiderState
        {
            Hanging,
            Dropping,
            Friendly,
            Hostile,
            Fleeing,
            Defeated
        }

        public enum BakerMood
        {
            Neutral,
            Friendly,
            Hostile
        }
        #endregion

        #region Properties
        public SpiderState State { get; private set; }
        public BakerMood Mood { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float DetectionRange { get; private set; }
        public float WebY { get; private set; }
        public bool IsFriendly => Mood == BakerMood.Friendly;
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private float webLength;
        private float swingAngle;
        private float swingSpeed;
        private int throwCount;
        private float stateTimer;
        private Player targetPlayer;
        private Level level;
        private List<BakedGoodProjectile> activeProjectiles;
        private bool wasAttacked;
        private Image webImage;
        #endregion

        #region Constructor
        public SpiderBaker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("detectionRange", 100f),
                data.Float("webY", Position.Y - 80f),
                data.Bool("startFriendly", false)
            );
        }

        public SpiderBaker(Vector2 position, int health = 2, float detectionRange = 100f,
            float webY = -80f, bool startFriendly = false)
            : base(position)
        {
            Initialize(health, detectionRange, webY + position.Y, startFriendly);
        }

        private void Initialize(int health, float detectionRange, float webY, bool startFriendly)
        {
            Health = health;
            MaxHealth = health;
            DetectionRange = detectionRange;
            WebY = webY;
            webLength = Position.Y - WebY;
            swingAngle = 0f;
            swingSpeed = 2f;
            throwCount = 0;
            stateTimer = 0f;
            wasAttacked = false;
            activeProjectiles = new List<BakedGoodProjectile>();
            
            Mood = startFriendly ? BakerMood.Friendly : BakerMood.Neutral;
            
            // Setup collider
            Collider = new Hitbox(20f, 20f, -10f, -20f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("spider_baker"));
            
            // Create web strand visual
            CreateWebVisual();
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
            
            State = SpiderState.Hanging;
        }

        private void CreateWebVisual()
        {
            // Create a simple web strand using primitives or a texture
            // This will be drawn in Render
        }
        #endregion

        #region State Begin Methods
        private void HangingBegin()
        {
            sprite.Play("hanging");
            State = SpiderState.Hanging;
        }

        private void DroppingBegin()
        {
            sprite.Play("dropping");
            State = SpiderState.Dropping;
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        private void FriendlyBegin()
        {
            sprite.Play("friendly");
            State = SpiderState.Friendly;
            Mood = BakerMood.Friendly;
        }

        private void HostileBegin()
        {
            sprite.Play("hostile");
            State = SpiderState.Hostile;
            Mood = BakerMood.Hostile;
            throwCount = 3;
        }

        private void FleeingBegin()
        {
            sprite.Play("climbing");
            State = SpiderState.Fleeing;
            Audio.Play("event:/game/char_maddy/land", Position);
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = SpiderState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
            ClearProjectiles();
        }
        #endregion

        #region State Routines
        private IEnumerator HangingRoutine()
        {
            while (true)
            {
                // Gentle swinging animation
                swingAngle += swingSpeed * Engine.DeltaTime;
                float swingOffset = (float)Math.Sin(swingAngle) * 8f;
                _ = swingOffset; // TODO: apply swing to a stored base X once anchor is tracked
                
                // Check for player below
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null)
                {
                    float xDist = Math.Abs(targetPlayer.Position.X - Position.X);
                    float yDist = targetPlayer.Position.Y - Position.Y;
                    
                    // Drop if player is below and within range
                    if (yDist > 0 && yDist < DetectionRange * 2f && xDist < DetectionRange)
                    {
                        if (wasAttacked)
                        {
                            stateMachine.State = 3; // Hostile
                        }
                        else
                        {
                            stateMachine.State = 1; // Dropping
                        }
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator DroppingRoutine()
        {
            // Fall to ground
            while (!OnGround())
            {
                MoveV(200f * Engine.DeltaTime);
                yield return null;
            }
            
            // Small landing effect
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 4, Position, Vector2.One * 4f);
            Audio.Play("event:/game/char_maddy/land", Position);
            
            yield return 0.3f;
            
            // Determine mood based on player action
            if (wasAttacked)
            {
                stateMachine.State = 3; // Hostile
            }
            else
            {
                stateMachine.State = 2; // Friendly
            }
        }

        private IEnumerator FriendlyRoutine()
        {
            // Face player
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            if (targetPlayer != null)
            {
                sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
            }
            
            // Offer baked goods dialogue
            yield return Textbox.Say("SPIDER_BAKER_GREETING");
            
            // Give player a treat (healing item)
            var treat = new SpiderTreat(Position + new Vector2(20f, -10f));
            Scene.Add(treat);
            Audio.Play("event:/game/general/diamond_get", Position);
            
            yield return 0.5f;
            
            // Wave goodbye and flee
            sprite.Play("wave");
            yield return 1f;
            
            stateMachine.State = 4; // Fleeing
        }

        private IEnumerator HostileRoutine()
        {
            while (throwCount > 0 && Health > 0)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer == null)
                {
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                    yield return null;
                    continue;
                }
                
                // Face player
                sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                
                // Throw baked good
                sprite.Play("throw");
                yield return 0.3f;
                
                Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                var projectile = new BakedGoodProjectile(Position - Vector2.UnitY * 10f, direction * 120f);
                Scene.Add(projectile);
                activeProjectiles.Add(projectile);
                Audio.Play("event:/game/char_badeline/beam_launch", Position);
                
                throwCount--;
                
                yield return 0.8f;
                sprite.Play("hostile_idle");
            }
            
            // Out of items, flee
            stateMachine.State = 4; // Fleeing
        }

        private IEnumerator FleeingRoutine()
        {
            // Climb back up web
            while (Position.Y > WebY + 10f)
            {
                MoveV(-100f * Engine.DeltaTime);
                yield return null;
            }
            
            // Fade out at top of web
            float fadeTime = 0.5f;
            while (fadeTime > 0f)
            {
                fadeTime -= Engine.DeltaTime;
                sprite.Color = Color.White * (fadeTime / 0.5f);
                yield return null;
            }
            
            RemoveSelf();
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death particles
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 8f);
            
            // Drop a treat as apology
            var treat = new SpiderTreat(Position);
            Scene.Add(treat);
            
            yield return 0.8f;
            RemoveSelf();
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == SpiderState.Defeated) return;
            
            wasAttacked = true;
            Health -= damage;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 4, Position, Vector2.One * 4f);
            
            if (Health <= 0)
            {
                stateMachine.State = 5; // Defeated
            }
            else
            {
                // Become hostile if not already
                if (State == SpiderState.Friendly || State == SpiderState.Hanging)
                {
                    stateMachine.State = 3; // Hostile
                }
            }
        }

        public void Interact()
        {
            // Player can interact to befriend
            if (State == SpiderState.Hanging && !wasAttacked)
            {
                stateMachine.State = 1; // Drop to be friendly
            }
        }
        #endregion

        #region Private Methods
        private void ClearProjectiles()
        {
            foreach (var proj in activeProjectiles)
            {
                if (proj != null && proj.Scene != null)
                {
                    proj.RemoveSelf();
                }
            }
            activeProjectiles.Clear();
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
            activeProjectiles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Removed(Scene scene)
        {
            ClearProjectiles();
            base.Removed(scene);
        }

        public override void Render()
        {
            // Draw web strand
            Draw.Line(Position, new Vector2(Position.X, WebY), Color.White * 0.5f, 1f);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// BakedGoodProjectile - Thrown by SpiderBaker
    /// A baked treat (donut, cookie, etc.) thrown as a projectile
    /// Sprite path: characters/baked_good/
    /// </summary>
    public class BakedGoodProjectile : Actor
    {
        #region Properties
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private float rotation;
        private string goodType;
        #endregion

        #region Constructor
        public BakedGoodProjectile(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 3f;
            rotation = 0f;
            
            // Random baked good type
            string[] types = { "donut", "cookie", "croissant", "muffin" };
            goodType = types[Calc.Random.Next(types.Length)];
            
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(sprite = GFX.SpriteBank.Create("baked_good"));
        }
        #endregion

        #region Entity Overrides
        public override void Update()
        {
            base.Update();
            
            // Move with gravity
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 300f * Engine.DeltaTime;
            
            // Spin
            rotation += Engine.DeltaTime * 8f;
            sprite.Rotation = rotation;
            
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            // Remove on ground or expired
            if (lifetime <= 0f || OnGround())
            {
                RemoveSelf();
            }
        }
        #endregion
    }

    /// <summary>
    /// SpiderTreat - Healing item dropped by friendly SpiderBaker
    /// Restores health when collected
    /// Sprite path: collectables/spider_treat/
    /// </summary>
    [CustomEntity("MaggyHelper/SpiderTreat")]
    public class SpiderTreat : Actor
    {
        #region Properties
        private Sprite sprite;
        private float bounceTimer;
        private float yBounce;
        private bool collected;
        private Level level;
        #endregion

        #region Constructor
        public SpiderTreat(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize();
        }

        public SpiderTreat(Vector2 position)
            : base(position)
        {
            Initialize();
        }

        private void Initialize()
        {
            collected = false;
            bounceTimer = 0f;
            yBounce = 0f;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(sprite = GFX.SpriteBank.Create("spider_treat"));
            
            // Bounce effect
            Add(new Coroutine(BounceRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator BounceRoutine()
        {
            while (!collected)
            {
                bounceTimer += Engine.DeltaTime * 3f;
                yBounce = (float)Math.Sin(bounceTimer) * 4f;
                yield return null;
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
            
            if (collected) return;
            
            // Check player collection
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                Collect(player);
            }
        }

        private void Collect(Player player)
        {
            collected = true;
            Audio.Play("event:/game/general/diamond_get", Position);
            
            // Heal player (implementation depends on health system)
            // player.Heal(1);
            
            // Particle effect
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 6, Position, Vector2.One * 6f, Color.Yellow);
            
            RemoveSelf();
        }

        public override void Render()
        {
            // Draw with bounce offset
            Vector2 pos = Position + new Vector2(0f, yBounce);
            sprite.RenderPosition = pos;
            base.Render();
        }
        #endregion
    }
}
