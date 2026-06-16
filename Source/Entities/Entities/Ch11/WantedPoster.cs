namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// WantedPoster - Trigger that spawns bounty enemies when read
    /// Shows a poster UI and triggers enemy spawns
    /// Sprite path: objects/wanted_poster/
    /// </summary>
    [CustomEntity("DZ/WantedPoster")]
    [Tracked]
    public class WantedPoster : Actor
    {
        #region Enums
        public enum PosterState
        {
            Unread,
            Reading,
            Read,
            Spawning,
            Complete
        }
        #endregion

        #region Properties
        public PosterState State { get; private set; }
        public string BountyName { get; private set; }
        public int BountyReward { get; private set; }
        public string EnemyType { get; private set; }
        public int EnemyCount { get; private set; }
        public bool HasBeenRead => State >= PosterState.Read;
        
        private Sprite sprite;
        private TalkComponent talkComponent;
        private Player readingPlayer;
        private Level level;
        private List<Entity> spawnedEnemies;
        private float spawnTimer;
        private int spawnedCount;
        private bool bountyComplete;
        #endregion

        #region Constructor
        public WantedPoster(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("bountyName", "OUTLAW"),
                data.Int("bountyReward", 100),
                data.Attr("enemyType", "DZ/BanditoRoller"),
                data.Int("enemyCount", 3)
            );
        }

        public WantedPoster(Vector2 position, string bountyName = "OUTLAW", int bountyReward = 100,
            string enemyType = "DZ/BanditoRoller", int enemyCount = 3)
            : base(position)
        {
            Initialize(bountyName, bountyReward, enemyType, enemyCount);
        }

        private void Initialize(string bountyName, int bountyReward, string enemyType, int enemyCount)
        {
            BountyName = bountyName;
            BountyReward = bountyReward;
            EnemyType = enemyType;
            EnemyCount = enemyCount;
            
            State = PosterState.Unread;
            spawnedEnemies = new List<Entity>();
            spawnTimer = 0f;
            spawnedCount = 0;
            bountyComplete = false;
            
            // Setup collider
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            
            // Setup sprite
            Add(sprite = GFX.SpriteBank.Create("wanted_poster"));
            sprite.Play("unread");
            
            // Add talk component
            Add(talkComponent = new TalkComponent(
                new Rectangle(-20, -32, 40, 40),
                new Vector2(0f, -40f),
                _ => Interact()
            ));
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            if (State == PosterState.Complete) return;
            
            readingPlayer = player;
            
            Add(new Coroutine(ReadRoutine()));
        }

        public void CheckBountyComplete()
        {
            // Check if all spawned enemies are defeated
            spawnedEnemies.RemoveAll(e => e == null || e.Scene == null);
            
            if (spawnedEnemies.Count == 0 && spawnedCount >= EnemyCount)
            {
                bountyComplete = true;
                State = PosterState.Complete;
                sprite.Play("complete");
                
                // Give reward
                var level = Scene as Level;
                level?.Session.SetFlag("bounty_" + BountyName + "_complete", true);
                
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator ReadRoutine()
        {
            State = PosterState.Reading;
            
            // Lock player
            readingPlayer.StateMachine.State = Player.StDummy;
            readingPlayer.StateMachine.Locked = true;
            
            // Show poster dialogue
            yield return Textbox.Say("WANTED_POSTER_" + BountyName);
            
            // Unlock player
            readingPlayer.StateMachine.Locked = false;
            readingPlayer.StateMachine.State = Player.StNormal;
            
            State = PosterState.Read;
            sprite.Play("read");
            
            // Start spawning enemies
            yield return SpawnEnemies();
        }

        private IEnumerator SpawnEnemies()
        {
            State = PosterState.Spawning;
            
            for (int i = 0; i < EnemyCount; i++)
            {
                // Spawn enemy at random position around poster
                Vector2 spawnPos = Position + new Vector2(
                    Calc.Random.NextFloat() * 100f - 50f,
                    Calc.Random.NextFloat() * 100f - 50f
                );
                
                // Create enemy (simplified - would use entity factory)
                var enemy = CreateEnemy(spawnPos);
                if (enemy != null)
                {
                    Scene.Add(enemy);
                    spawnedEnemies.Add(enemy);
                    spawnedCount++;
                    
                    Audio.Play("event:/game/char_badeline/disappear", spawnPos);
                    level?.ParticlesFG.Emit(ParticleTypes.Dust, 5, spawnPos, Vector2.One * 4f);
                }
                
                yield return 0.5f;
            }
            
            State = PosterState.Read;
        }

        private Entity CreateEnemy(Vector2 position)
        {
            // In real implementation, would use entity factory based on EnemyType
            // For now, return null and handle via map placement
            return null;
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
            
            // Check if bounty is complete
            if (State == PosterState.Spawning || State == PosterState.Read)
            {
                CheckBountyComplete();
            }
        }

        public override void Render()
        {
            // Draw poster frame
            Draw.Rect(Position.X - 10, Position.Y - 28, 20, 4, Color.Brown);
            
            // Draw reward indicator when complete
            if (bountyComplete)
            {
                Draw.Circle(Position - Vector2.UnitY * 32f, 8f, Color.Gold * 0.6f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// BountyBoard - Collection of wanted posters
    /// </summary>
    [CustomEntity("DZ/BountyBoard")]
    public class BountyBoard : Actor
    {
        private List<WantedPoster> posters;
        private int completedBounties;
        private Sprite sprite;

        public BountyBoard(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            posters = new List<WantedPoster>();
            completedBounties = 0;
            
            Collider = new Hitbox(80f, 60f, -40f, -60f);
            Add(sprite = GFX.SpriteBank.Create("bounty_board"));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find nearby posters
            foreach (var entity in scene.Tracker.GetEntities<WantedPoster>())
            {
                if (Vector2.Distance(Position, entity.Position) < 100f)
                {
                    posters.Add((WantedPoster)entity);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Count completed bounties
            completedBounties = 0;
            foreach (var poster in posters)
            {
                if (poster.HasBeenRead && poster.State == WantedPoster.PosterState.Complete)
                {
                    completedBounties++;
                }
            }
        }

        public override void Render()
        {
            base.Render();
        }
    }

    /// <summary>
    /// BountyReward - Collectible given after completing bounty
    /// </summary>
    [CustomEntity("DZ/BountyReward")]
    public class BountyReward : Actor
    {
        private Sprite sprite;
        private bool collected;
        private int rewardAmount;
        private float floatTimer;

        public BountyReward(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            rewardAmount = data.Int("rewardAmount", 100);
            collected = false;
            floatTimer = 0f;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(sprite = GFX.SpriteBank.Create("bounty_reward"));
            Add(new VertexLight(Color.Gold, 0.8f, 8, 24));
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
            
            var level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 12, Position, Vector2.One * 8f, Color.Gold);
            
            RemoveSelf();
        }
    }
}

