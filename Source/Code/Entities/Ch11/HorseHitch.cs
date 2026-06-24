namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// HorseHitch - Fast-travel point between bar sections
    /// Allows player to quickly travel between connected hitches
    /// Sprite path: objects/horse_hitch/
    /// </summary>
    [CustomEntity("MaggyHelper/HorseHitch")]
    [Tracked]
    public class HorseHitch : Actor
    {
        #region Enums
        public enum HitchState
        {
            Inactive,
            Active,
            Traveling,
            Arriving
        }
        #endregion

        #region Properties
        public HitchState State { get; private set; }
        public string HitchId { get; private set; }
        public string DestinationId { get; private set; }
        public bool IsUnlocked { get; private set; }
        public bool CanTravel => IsUnlocked && State == HitchState.Active;
        
        private Sprite sprite;
        private Sprite horseSprite;
        private TalkComponent talkComponent;
        private Player travelingPlayer;
        private Level level;
        private HorseHitch destination;
        private float travelTimer;
        private List<DustParticle> dustParticles;
        private VertexLight hitchLight;
        #endregion

        #region Constructor
        public HorseHitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("hitchId", ""),
                data.Attr("destinationId", ""),
                data.Bool("isUnlocked", false)
            );
        }

        public HorseHitch(Vector2 position, string hitchId = "", string destinationId = "", bool isUnlocked = false)
            : base(position)
        {
            Initialize(hitchId, destinationId, isUnlocked);
        }

        private void Initialize(string hitchId, string destinationId, bool isUnlocked)
        {
            HitchId = hitchId;
            DestinationId = destinationId;
            IsUnlocked = isUnlocked;
            
            State = isUnlocked ? HitchState.Active : HitchState.Inactive;
            travelTimer = 0f;
            dustParticles = new List<DustParticle>();
            
            // Setup collider
            Collider = new Hitbox(24f, 40f, -12f, -40f);
            
            // Setup sprites
            Add(sprite = GFX.SpriteBank.Create("horse_hitch"));
            sprite.Play(isUnlocked ? "active" : "inactive");
            
            Add(horseSprite = GFX.SpriteBank.Create("travel_horse"));
            horseSprite.Position = new Vector2(0f, -50f);
            horseSprite.Visible = false;
            
            // Add glow when active
            Add(hitchLight = new VertexLight(Color.Brown, isUnlocked ? 0.4f : 0.1f, 8, 24));
            
            // Add talk component
            Add(talkComponent = new TalkComponent(
                new Rectangle(-24, -56, 48, 56),
                new Vector2(0f, -64f),
                _ => Interact()
            ));
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            if (!CanTravel) return;
            
            travelingPlayer = player;
            
            Add(new Coroutine(TravelRoutine()));
        }

        public void Unlock()
        {
            IsUnlocked = true;
            State = HitchState.Active;
            sprite.Play("active");
            hitchLight.Alpha = 0.4f;
            
            Audio.Play("event:/game/general/diamond_get", Position);
        }

        public void Arrive(Player player)
        {
            State = HitchState.Arriving;
            horseSprite.Visible = true;
            horseSprite.Play("arrive");
            
            travelingPlayer = player;
            player.Position = Position;
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            
            Add(new Coroutine(ArriveRoutine()));
        }

        public void SetDestination(HorseHitch dest)
        {
            destination = dest;
        }
        #endregion

        #region Private Methods
        private IEnumerator TravelRoutine()
        {
            // Lock player
            travelingPlayer.StateMachine.State = Player.StDummy;
            travelingPlayer.StateMachine.Locked = true;
            
            State = HitchState.Traveling;
            horseSprite.Visible = true;
            horseSprite.Play("mount");
            
            // Mount animation
            yield return 0.5f;
            
            // Hide player
            travelingPlayer.Visible = false;
            
            // Travel dialogue
            yield return Textbox.Say("HORSE_TRAVEL_START");
            
            // Create dust
            for (int i = 0; i < 10; i++)
            {
                CreateDustParticle();
                yield return 0.05f;
            }
            
            // Screen fade
            level?.Flash(Color.Black * 0.8f);
            Audio.Play("event:/game/char_maddy/dash", Position);
            
            yield return 0.5f;
            
            // Teleport to destination
            if (destination != null)
            {
                // Teleport player
                travelingPlayer.Position = destination.Position;
                
                // Trigger arrival at destination
                destination.Arrive(travelingPlayer);
            }
            
            // Reset this hitch
            horseSprite.Visible = false;
            State = HitchState.Active;
            
            yield break;
        }

        private IEnumerator ArriveRoutine()
        {
            // Arrival animation
            horseSprite.Play("arrive");
            
            // Create dust
            for (int i = 0; i < 10; i++)
            {
                CreateDustParticle();
                yield return 0.05f;
            }
            
            yield return 0.5f;
            
            // Dismount
            horseSprite.Play("dismount");
            travelingPlayer.Visible = true;
            travelingPlayer.Position = Position + new Vector2(20f, 0f);
            
            yield return 0.3f;
            
            // Unlock player
            travelingPlayer.StateMachine.Locked = false;
            travelingPlayer.StateMachine.State = Player.StNormal;
            
            horseSprite.Visible = false;
            State = HitchState.Active;
        }

        private void CreateDustParticle()
        {
            var particle = new DustParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, -Calc.Random.NextFloat() * 30f)
            );
            dustParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Find destination hitch
            if (!string.IsNullOrEmpty(DestinationId))
            {
                foreach (var hitch in scene.Tracker.GetEntities<HorseHitch>())
                {
                    var typedHitch = (HorseHitch)hitch;
                    if (typedHitch.HitchId == DestinationId)
                    {
                        destination = typedHitch;
                        break;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            dustParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw hitch post
            Draw.Rect(Position.X - 4, Position.Y - 40, 8, 40, Color.Brown);
            
            // Draw travel indicator when active
            if (CanTravel)
            {
                float pulse = (float)Math.Sin(Scene.TimeActive * 3f) * 0.2f + 0.8f;
                Draw.Circle(Position - Vector2.UnitY * 48f, 12f * pulse, Color.Brown * 0.3f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// HorseHitchNetwork - Manages all horse hitches
    /// </summary>
    [CustomEntity("MaggyHelper/HorseHitchNetwork")]
    public class HorseHitchNetwork : Entity
    {
        private List<HorseHitch> hitches;
        private Dictionary<string, HorseHitch> hitchDict;

        public HorseHitchNetwork(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            hitches = new List<HorseHitch>();
            hitchDict = new Dictionary<string, HorseHitch>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Register all hitches
            foreach (var hitch in scene.Tracker.GetEntities<HorseHitch>())
            {
                var typedHitch = (HorseHitch)hitch;
                hitches.Add(typedHitch);
                if (!string.IsNullOrEmpty(typedHitch.HitchId))
                {
                    hitchDict[typedHitch.HitchId] = typedHitch;
                }
            }
            
            // Link destinations
            foreach (var hitch in hitches)
            {
                if (!string.IsNullOrEmpty(hitch.DestinationId) && hitchDict.ContainsKey(hitch.DestinationId))
                {
                    hitch.SetDestination(hitchDict[hitch.DestinationId]);
                }
            }
        }

        public void UnlockAll()
        {
            foreach (var hitch in hitches)
            {
                hitch.Unlock();
            }
        }

        public HorseHitch GetHitch(string id)
        {
            return hitchDict.ContainsKey(id) ? hitchDict[id] : null;
        }
    }

    /// <summary>
    /// Stable - Location with multiple horse hitches
    /// </summary>
    [CustomEntity("MaggyHelper/Stable")]
    public class Stable : Actor
    {
        private Sprite sprite;
        private List<HorseHitch> hitches;
        private bool isUnlocked;

        public Stable(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            isUnlocked = data.Bool("isUnlocked", false);
            hitches = new List<HorseHitch>();
            
            Collider = new Hitbox(80f, 60f, -40f, -60f);
            Add(sprite = GFX.SpriteBank.Create("stable"));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find nearby hitches
            foreach (var hitch in scene.Tracker.GetEntities<HorseHitch>())
            {
                var typedHitch = (HorseHitch)hitch;
                if (Vector2.Distance(Position, typedHitch.Position) < 80f)
                {
                    hitches.Add(typedHitch);
                }
            }
        }

        public void Unlock()
        {
            isUnlocked = true;
            
            foreach (var hitch in hitches)
            {
                hitch.Unlock();
            }
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }
    }
}

