namespace Celeste.Entities.Chapters.Ch15
{
    #region Enums
    public enum WishType
    {
        Power,
        Knowledge,
        Freedom,
        Sacrifice,
        None
    }
    #endregion

    /// <summary>
    /// WishAltar - Interactive altar where player makes choices affecting ending
    /// Presents dialogue options that set flags for story outcomes
    /// Sprite path: objects/DZ/DZ/wish_altar/
    /// </summary>
    [CustomEntity("DZ/WishAltar")]
    [Tracked]
    public class WishAltar : Actor
    {
        #region Enums
        public enum AltarState
        {
            Inactive,
            Active,
            Offering,
            Wishing,
            Granted,
            Denied,
            Complete
        }
        #endregion

        #region Properties
        public AltarState State { get; private set; }
        public bool CanInteract { get; private set; }
        public string DialoguePrefix { get; private set; }
        public WishType SelectedWish { get; private set; }
        public bool WishGranted { get; private set; }
        public int RequiredHearts { get; private set; }
        
        private Sprite sprite;
        private Sprite flameSprite;
        private TalkComponent talkComponent;
        private Player interactingPlayer;
        private Level level;
        private VertexLight altarLight;
        private List<AltarFlameParticle> flameParticles;
        private float pulseTimer;
        private bool hasWished;
        #endregion

        #region Constructor
        public WishAltar(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Bool("canInteract", true),
                data.Attr("dialoguePrefix", "WISH_ALTAR"),
                data.Int("requiredHearts", 0)
            );
        }

        public WishAltar(Vector2 position, bool canInteract = true, string dialoguePrefix = "WISH_ALTAR", int requiredHearts = 0)
            : base(position)
        {
            Initialize(canInteract, dialoguePrefix, requiredHearts);
        }

        private void Initialize(bool canInteract, string dialoguePrefix, int requiredHearts)
        {
            CanInteract = canInteract;
            DialoguePrefix = dialoguePrefix;
            RequiredHearts = requiredHearts;
            
            State = AltarState.Inactive;
            SelectedWish = WishType.None;
            WishGranted = false;
            hasWished = false;
            pulseTimer = 0f;
            flameParticles = new List<AltarFlameParticle>();
            
            // Setup collider
            Collider = new Hitbox(48f, 40f, -24f, -40f);
            
            // Setup sprites
            Add(sprite = GFX.SpriteBank.Create("wish_altar"));
            sprite.Play("inactive");
            
            Add(flameSprite = GFX.SpriteBank.Create("altar_flame"));
            flameSprite.Position = new Vector2(0f, -48f);
            flameSprite.Visible = false;
            
            // Add mystical glow
            Add(altarLight = new VertexLight(Color.Cyan, 0.3f, 16, 48));
            
            // Add talk component
            if (CanInteract)
            {
                Add(talkComponent = new TalkComponent(
                    new Rectangle(-32, -56, 64, 64),
                    new Vector2(0f, -64f),
                    _ => Interact()
                ));
            }
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            if (State == AltarState.Complete || hasWished) return;
            
            interactingPlayer = player;
            
            Add(new Coroutine(InteractionRoutine(player)));
        }

        public void Activate()
        {
            State = AltarState.Active;
            sprite.Play("active");
            flameSprite.Visible = true;
            flameSprite.Play("burn");
            altarLight.Color = Color.Cyan;
            altarLight.Alpha = 0.6f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public void MakeWish(WishType wish)
        {
            SelectedWish = wish;
            State = AltarState.Wishing;
            
            Add(new Coroutine(InteractionRoutine(Scene.Tracker.GetEntity<Player>())));
        }
        #endregion

        #region Private Methods
        private IEnumerator InteractionRoutine(Player player)
        {
            // Lock player
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            
            // Check if altar is active
            if (State == AltarState.Inactive)
            {
                Activate();
                yield return 0.5f;
            }
            
            // Present wish options
            State = AltarState.Offering;
            sprite.Play("offering");
            
            yield return Textbox.Say(DialoguePrefix + "_INTRO");
            
            // Show wish choices
            var choiceMenu = CreateWishMenu();
            yield return choiceMenu;
            
            // Process wish
            if (SelectedWish != WishType.None)
            {
                yield return ProcessWish();
            }
            
            // Unlock player
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
            
            State = AltarState.Complete;
            sprite.Play("complete");
        }

        private IEnumerator CreateWishMenu()
        {
            // In a real implementation, this would show a TextMenu with options
            // For now, we'll use dialogue choices
            
            yield return Textbox.Say(DialoguePrefix + "DZ_CHOICES");
            
            // The actual wish selection would come from dialogue choices
            // This is a simplified version
            SelectedWish = WishType.Freedom; // Default for now
        }

        private IEnumerator ProcessWish()
        {
            State = AltarState.Wishing;
            sprite.Play("wishing");
            
            // Dramatic effect
            level?.Shake(0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Create flame particles
            for (int i = 0; i < 20; i++)
            {
                CreateFlameParticle();
                yield return 0.05f;
            }
            
            yield return 1f;
            
            // Check if wish can be granted
            bool canGrant = CheckWishRequirements();
            
            if (canGrant)
            {
                WishGranted = true;
                State = AltarState.Granted;
                sprite.Play("granted");
                
                level?.Flash(Color.Cyan * 0.4f);
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
                
                yield return Textbox.Say(DialoguePrefix + "_" + SelectedWish.ToString().ToUpper() + "_GRANTED");
                
                // Set flags based on wish
                SetWishFlags();
            }
            else
            {
                WishGranted = false;
                State = AltarState.Denied;
                sprite.Play("denied");
                
                level?.Flash(Color.Red * 0.3f);
                Audio.Play("event:/game/char_badeline/disappear", Position);
                
                yield return Textbox.Say(DialoguePrefix + "_DENIED");
            }
            
            hasWished = true;
        }

        private bool CheckWishRequirements()
        {
            // Check heart count or other requirements
            // For now, always grant
            return true;
        }

        private void SetWishFlags()
        {
            if (level?.Session != null)
            {
                level.Session.SetFlag("wish_" + SelectedWish.ToString().ToLower(), true);
                level.Session.SetFlag("wish_granted", WishGranted);
            }
        }

        private void CreateFlameParticle()
        {
            var particle = new AltarFlameParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 24f - 12f, Calc.Random.NextFloat() * 24f - 12f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 80f)
            );
            flameParticles.Add(particle);
            Scene.Add(particle);
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
            
            // Pulse effect when active
            if (State == AltarState.Active || State == AltarState.Offering)
            {
                pulseTimer += Engine.DeltaTime * 2f;
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
                flameSprite.Scale = Vector2.One * pulse;
                altarLight.Alpha = 0.5f + (float)Math.Sin(pulseTimer) * 0.2f;
            }
            
            // Create ambient particles when active
            if (State >= AltarState.Active && Scene.OnInterval(0.2f))
            {
                CreateFlameParticle();
            }
            
            flameParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw altar base glow
            if (State >= AltarState.Active)
            {
                Draw.Circle(Position - Vector2.UnitY * 20f, 40f, Color.Cyan * 0.15f, 16);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// AltarFlameParticle - Mystical flame particle for altar
    /// </summary>
    public class AltarFlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public AltarFlameParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1.2f - 0.6f) + 0.6f;
            lifetime = maxLifetime;
            
            Color[] colors = { Color.Cyan, Color.LightCyan, Color.White, Color.LightBlue };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 60f * Engine.DeltaTime;
            velocity *= 0.97f;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 6f, color * (alpha * 0.5f), 5);
        }
    }

    /// <summary>
    /// WishOrb - Collectible that unlocks wish options at altar
    /// </summary>
    [CustomEntity("DZ/WishOrb")]
    public class WishOrb : Actor
    {
        private Sprite sprite;
        private bool collected;
        private float floatTimer;
        private WishType wishType;

        public WishOrb(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            wishType = data.Enum("wishType", WishType.Power);
            collected = false;
            floatTimer = 0f;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(sprite = GFX.SpriteBank.Create("wish_orb"));
            Add(new VertexLight(Color.Cyan, 0.8f, 8, 24));
        }

        public override void Update()
        {
            base.Update();
            
            if (collected) return;
            
            // Float animation
            floatTimer += Engine.DeltaTime * 3f;
            sprite.Y = (float)Math.Sin(floatTimer) * 4f;
            
            // Check collection
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                Collect();
            }
        }

        private void Collect()
        {
            collected = true;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Set flag for this wish type
            var level = Scene as Level;
            level?.Session.SetFlag("wish_orb_" + wishType.ToString().ToLower(), true);
            
            RemoveSelf();
        }
    }
}

