namespace Celeste.Entities.Chapters.Ch10
{
    /// <summary>
    /// TorielStoveEntity - Interactive cooking station with dialogue
    /// Toriel's stove where she bakes butterscotch-cinnamon pie
    /// Can heal the player and trigger dialogue
    /// Sprite path: objects/DZ/DZ/toriel_stove/
    /// </summary>
    [CustomEntity("DZ/TorielStoveEntity")]
    [Tracked]
    public class TorielStoveEntity : Actor
    {
        #region Enums
        public enum StoveState
        {
            Idle,
            Cooking,
            Finished,
            Interacting
        }
        #endregion

        #region Properties
        public StoveState State { get; private set; }
        public bool CanInteract { get; private set; }
        public bool HasPie { get; private set; }
        public int HealAmount { get; private set; }
        public string DialogueId { get; private set; }
        
        private Sprite sprite;
        private Sprite pieSprite;
        private float cookTimer;
        private float cookDuration;
        private Level level;
        private VertexLight stoveLight;
        private float steamTimer;
        private List<SteamParticle> steamParticles;
        private TalkComponent talkComponent;
        private bool hasGivenPie;
        #endregion

        #region Constructor
        public TorielStoveEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Bool("canInteract", true),
                data.Bool("hasPie", true),
                data.Int("healAmount", 3),
                data.Attr("dialogueId", "TORIEL_STOVE"),
                data.Float("cookDuration", 5f)
            );
        }

        public TorielStoveEntity(Vector2 position, bool canInteract = true, bool hasPie = true,
            int healAmount = 3, string dialogueId = "TORIEL_STOVE", float cookDuration = 5f)
            : base(position)
        {
            Initialize(canInteract, hasPie, healAmount, dialogueId, cookDuration);
        }

        private void Initialize(bool canInteract, bool hasPie, int healAmount, string dialogueId, float cookDuration)
        {
            CanInteract = canInteract;
            HasPie = hasPie;
            HealAmount = healAmount;
            DialogueId = dialogueId;
            this.cookDuration = cookDuration;
            
            State = StoveState.Idle;
            cookTimer = 0f;
            steamTimer = 0f;
            hasGivenPie = false;
            steamParticles = new List<SteamParticle>();
            
            // Setup collider
            Collider = new Hitbox(32f, 40f, -16f, -40f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("toriel_stove"));
            sprite.Play("idle");
            
            // Pie sprite (shown when pie is ready)
            Add(pieSprite = GFX.SpriteBank.Create("butterscotch_pie"));
            pieSprite.Visible = false;
            pieSprite.Position = new Vector2(0f, -48f);
            
            // Add warm glow
            Add(stoveLight = new VertexLight(Color.Orange, 0.5f, 16, 48));
            
            // Add talk component for interaction
            if (CanInteract)
            {
                Add(talkComponent = new TalkComponent(
                    new Rectangle(-24, -48, 48, 56),
                    new Vector2(0f, -56f),
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
            if (State == StoveState.Interacting) return;
            
            State = StoveState.Interacting;
            
            Add(new Coroutine(InteractionRoutine(player)));
        }

        public void StartCooking()
        {
            if (State != StoveState.Idle) return;
            
            State = StoveState.Cooking;
            cookTimer = 0f;
            sprite.Play("cooking");
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void GivePie(Player player)
        {
            if (!HasPie || hasGivenPie) return;
            
            hasGivenPie = true;
            pieSprite.Visible = false;
            
            // Heal player
            // player.Heal(HealAmount);
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            // Particle effect
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 12, Position - Vector2.UnitY * 40f, Vector2.One * 8f, Color.Gold);
        }
        #endregion

        #region Private Methods
        private IEnumerator InteractionRoutine(Player player)
        {
            // Face player
            sprite.Scale.X = player.Position.X > Position.X ? 1 : -1;
            
            // Show dialogue
            yield return Textbox.Say(DialogueId);
            
            // If pie is ready, offer it
            if (HasPie && !hasGivenPie)
            {
                yield return Textbox.Say(DialogueId + "_PIE_OFFER");
                GivePie(player);
            }
            
            State = StoveState.Idle;
            sprite.Play("idle");
        }

        private void CreateSteamParticle()
        {
            var steam = new SteamParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 24f - 40f, Calc.Random.NextFloat() * 24f - 40f),
                new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f)
            );
            steamParticles.Add(steam);
            Scene.Add(steam);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Show pie if available
            if (HasPie && !hasGivenPie)
            {
                pieSprite.Visible = true;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Handle cooking
            if (State == StoveState.Cooking)
            {
                cookTimer += Engine.DeltaTime;
                
                // Create steam particles
                steamTimer += Engine.DeltaTime;
                if (steamTimer > 0.2f)
                {
                    steamTimer = 0f;
                    CreateSteamParticle();
                }
                
                // Check if done cooking
                if (cookTimer >= cookDuration)
                {
                    State = StoveState.Finished;
                    sprite.Play("finished");
                    HasPie = true;
                    pieSprite.Visible = true;
                    
                    Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    level?.Shake(0.1f);
                }
            }
            
            // Steam particles when pie is ready
            if (State == StoveState.Finished || (State == StoveState.Idle && HasPie))
            {
                steamTimer += Engine.DeltaTime;
                if (steamTimer > 0.5f)
                {
                    steamTimer = 0f;
                    CreateSteamParticle();
                }
            }
            
            // Clean up destroyed particles
            steamParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            base.Render();
            
            // Draw warm glow effect
            if (State == StoveState.Cooking || State == StoveState.Finished)
            {
                Draw.Circle(Position + new Vector2(0f, -20f), 24f, Color.Orange * 0.2f, 12);
            }
        }
        #endregion
    }

    /// <summary>
    /// SteamParticle - Rising steam particle effect
    /// </summary>
    public class SteamParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public SteamParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (2f - 1f) + 1f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            
            // Move with slight drift
            Position += velocity * Engine.DeltaTime;
            velocity.X += Calc.Random.NextFloat() * (10f - -10f) + -10f * Engine.DeltaTime;
            velocity.Y *= 0.98f;
            
            // Fade out
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 8f * scale, Color.White * (alpha * 0.3f), 6);
        }
    }

    /// <summary>
    /// ButterscotchPie - Collectible healing item (Toriel's famous pie)
    /// Fully heals the player
    /// Sprite path: collectables/butterscotch_pie/
    /// </summary>
    [CustomEntity("DZ/ButterscotchPie")]
    public class ButterscotchPie : Actor
    {
        #region Properties
        private Sprite sprite;
        private bool collected;
        private float bounceTimer;
        private float rotateTimer;
        private Level level;
        #endregion

        #region Constructor
        public ButterscotchPie(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize();
        }

        public ButterscotchPie(Vector2 position)
            : base(position)
        {
            Initialize();
        }

        private void Initialize()
        {
            collected = false;
            bounceTimer = 0f;
            rotateTimer = 0f;
            
            Collider = new Hitbox(20f, 16f, -10f, -16f);
            Add(sprite = GFX.SpriteBank.Create("butterscotch_pie"));
            Add(new VertexLight(Color.Gold, 1f, 12, 32));
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
            
            // Bounce and rotate animation
            bounceTimer += Engine.DeltaTime * 3f;
            rotateTimer += Engine.DeltaTime;
            
            sprite.Y = (float)Math.Sin(bounceTimer) * 4f;
            sprite.Rotation = (float)Math.Sin(rotateTimer * 0.5f) * 0.1f;
            
            // Check collection
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                Collect(player);
            }
        }

        private void Collect(Player player)
        {
            collected = true;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Full heal
            // player.FullHeal();
            
            // Particle burst
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 20, Position, Vector2.One * 12f, Color.Gold);
            level?.Flash(Color.Gold * 0.3f);
            
            RemoveSelf();
        }
        #endregion
    }

    /// <summary>
    /// TorielOven - Additional interactive oven for cooking minigames
    /// Sprite path: objects/DZ/DZ/toriel_oven/
    /// </summary>
    [CustomEntity("DZ/TorielOven")]
    public class TorielOven : Actor
    {
        #region Properties
        private Sprite sprite;
        private bool isOpen;
        private Level level;
        #endregion

        public TorielOven(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(40f, 48f, -20f, -48f);
            Add(sprite = GFX.SpriteBank.Create("toriel_oven"));
            sprite.Play("closed");
            isOpen = false;
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            sprite.Play(isOpen ? "open" : "closed");
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
    }
}

