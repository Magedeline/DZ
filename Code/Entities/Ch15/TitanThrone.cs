namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// TitanThrone - Boss arena activator with cinematic trigger
    /// Starts the Roaring Titan King boss battle when activated
    /// Sprite path: objects/titan_throne/
    /// </summary>
    [CustomEntity("MaggyHelper/TitanThrone")]
    [Tracked]
    public class TitanThrone : Actor
    {
        #region Enums
        public enum ThroneState
        {
            Inactive,
            Awakening,
            Active,
            BossBattle,
            Victory,
            Complete
        }
        #endregion

        #region Properties
        public ThroneState State { get; private set; }
        public string BossEntityName { get; private set; }
        public float ActivationRadius { get; private set; }
        public bool AutoActivate { get; private set; }
        public string CutsceneId { get; private set; }
        
        private Sprite sprite;
        private Sprite crownSprite;
        private TalkComponent talkComponent;
        private Player activatingPlayer;
        private Entity bossEntity;
        private Level level;
        private VertexLight throneLight;
        private List<ThroneParticle> particles;
        private float pulseTimer;
        private float shakeIntensity;
        private bool hasActivated;
        private bool bossDefeated;
        #endregion

        #region Constructor
        public TitanThrone(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("bossEntity", "MaggyHelper/KingTitanBoss"),
                data.Float("activationRadius", 100f),
                data.Bool("autoActivate", false),
                data.Attr("cutsceneId", "MAGGYHELPER_CH15_ROARING_TITAN_KING_BATTLE")
            );
        }

        public TitanThrone(Vector2 position, string bossEntity = "MaggyHelper/KingTitanBoss",
            float activationRadius = 100f, bool autoActivate = false, string cutsceneId = "MAGGYHELPER_CH15_ROARING_TITAN_KING_BATTLE")
            : base(position)
        {
            Initialize(bossEntity, activationRadius, autoActivate, cutsceneId);
        }

        private void Initialize(string bossEntity, float activationRadius, bool autoActivate, string cutsceneId)
        {
            BossEntityName = bossEntity;
            ActivationRadius = activationRadius;
            AutoActivate = autoActivate;
            CutsceneId = cutsceneId;
            
            State = ThroneState.Inactive;
            pulseTimer = 0f;
            shakeIntensity = 0f;
            hasActivated = false;
            bossDefeated = false;
            particles = new List<ThroneParticle>();
            
            // Large throne collider
            Collider = new Hitbox(80f, 120f, -40f, -120f);
            
            // Setup sprites
            Add(sprite = GFX.SpriteBank.Create("titan_throne"));
            sprite.Play("inactive");
            
            Add(crownSprite = GFX.SpriteBank.Create("titan_crown"));
            crownSprite.Position = new Vector2(0f, -130f);
            crownSprite.Visible = false;
            
            // Add royal glow
            Add(throneLight = new VertexLight(Color.Gold, 0.2f, 24, 64));
            
            // Add talk component for manual activation
            if (!AutoActivate)
            {
                Add(talkComponent = new TalkComponent(
                    new Rectangle(-48, -136, 96, 136),
                    new Vector2(0f, -144f),
                    _ => Interact()
                ));
            }
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            if (State != ThroneState.Inactive) return;
            
            activatingPlayer = Scene.Tracker.GetEntity<Player>();
            Activate();
        }

        public void Activate()
        {
            if (hasActivated) return;
            
            hasActivated = true;
            State = ThroneState.Awakening;
            
            Add(new Coroutine(AwakeningRoutine()));
        }

        public void OnBossDefeated()
        {
            bossDefeated = true;
            State = ThroneState.Victory;
            
            Add(new Coroutine(VictoryRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator AwakeningRoutine()
        {
            // Lock player
            if (activatingPlayer != null)
            {
                activatingPlayer.StateMachine.State = Player.StDummy;
                activatingPlayer.StateMachine.Locked = true;
            }
            
            // Dramatic awakening sequence
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            
            // Building shake
            for (int i = 0; i < 10; i++)
            {
                shakeIntensity = i / 10f;
                level?.Shake(shakeIntensity * 0.5f);
                CreateThroneParticle();
                yield return 0.1f;
            }
            
            // Flash
            level?.Flash(Color.Gold * 0.5f);
            
            // Show crown
            crownSprite.Visible = true;
            crownSprite.Play("appear");
            
            yield return 0.5f;
            
            // Activate throne
            State = ThroneState.Active;
            sprite.Play("active");
            throneLight.Color = Color.Gold;
            throneLight.Alpha = 0.6f;
            
            yield return 1f;
            
            // Start boss battle
            State = ThroneState.BossBattle;
            yield return StartBossBattle();
        }

        private IEnumerator StartBossBattle()
        {
            // Play cutscene dialogue
            yield return Textbox.Say(CutsceneId);
            
            // Spawn boss
            SpawnBoss();
            
            // Unlock player
            if (activatingPlayer != null)
            {
                activatingPlayer.StateMachine.Locked = false;
                activatingPlayer.StateMachine.State = Player.StNormal;
            }
            
            // Wait for boss defeat
            while (!bossDefeated)
            {
                yield return null;
            }
        }

        private void SpawnBoss()
        {
            // Create boss entity at throne position
            // In real implementation, would use entity factory
            // bossEntity = EntityFactory.Create(BossEntityName, Position + new Vector2(0f, -200f));
            // Scene.Add(bossEntity);
            
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            level?.Shake(0.5f);
        }

        private IEnumerator VictoryRoutine()
        {
            // Victory sequence
            level?.Flash(Color.Gold * 0.4f);
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            
            // Crown descends
            crownSprite.Play("descend");
            
            yield return 1f;
            
            // Victory dialogue
            yield return Textbox.Say("MAGGYHELPER_CH15_VICTORY");
            
            // Set completion flag
            level?.Session.SetFlag("ch15_titan_king_defeated", true);
            
            State = ThroneState.Complete;
            sprite.Play("complete");
            crownSprite.Play("rest");
        }

        private void CreateThroneParticle()
        {
            var particle = new ThroneParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 60f - 30f, Calc.Random.NextFloat() * 60f - 30f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, Calc.Random.NextFloat() * 150f - 30f)
            );
            particles.Add(particle);
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
            
            // Auto-activation check
            if (AutoActivate && State == ThroneState.Inactive)
            {
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Vector2.Distance(Position, player.Position) < ActivationRadius)
                {
                    activatingPlayer = player;
                    Activate();
                }
            }
            
            // Pulse effect when active
            if (State >= ThroneState.Active)
            {
                pulseTimer += Engine.DeltaTime * 2f;
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.05f;
                crownSprite.Scale = Vector2.One * pulse;
            }
            
            // Ambient particles
            if (State >= ThroneState.Active && Scene.OnInterval(0.3f))
            {
                CreateThroneParticle();
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw throne aura
            if (State >= ThroneState.Active)
            {
                Draw.Circle(Position - Vector2.UnitY * 60f, 60f, Color.Gold * 0.15f, 20);
            }
            
            // Draw activation zone when inactive
            if (State == ThroneState.Inactive && AutoActivate)
            {
                Draw.Circle(Position, ActivationRadius, Color.Gold * 0.1f, 24);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// ThroneParticle - Particle effect for throne
    /// </summary>
    public class ThroneParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public ThroneParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            
            Color[] colors = { Color.Gold, Color.Orange, Color.Yellow, Color.White };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 50f * Engine.DeltaTime;
            velocity *= 0.96f;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 5f, color * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// TitanCrown - Collectible crown dropped after boss defeat
    /// </summary>
    [CustomEntity("MaggyHelper/TitanCrown")]
    public class TitanCrown : Actor
    {
        private Sprite sprite;
        private bool collected;
        private float floatTimer;
        private float rotateTimer;

        public TitanCrown(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            collected = false;
            floatTimer = 0f;
            rotateTimer = 0f;
            
            Collider = new Hitbox(24f, 20f, -12f, -20f);
            Add(sprite = GFX.SpriteBank.Create("titan_crown_collectible"));
            Add(new VertexLight(Color.Gold, 1f, 12, 32));
        }

        public override void Update()
        {
            base.Update();
            
            if (collected) return;
            
            // Float and rotate
            floatTimer += Engine.DeltaTime * 2f;
            rotateTimer += Engine.DeltaTime;
            
            sprite.Y = (float)Math.Sin(floatTimer) * 6f - 10f;
            sprite.Rotation = (float)Math.Sin(rotateTimer * 0.5f) * 0.15f;
            
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
            
            var level = Scene as Level;
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            level?.Flash(Color.Gold * 0.3f);
            level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 20, Position, Vector2.One * 12f, Color.Gold);
            
            // Set flag
            level?.Session.SetFlag("titan_crown_collected", true);
            
            RemoveSelf();
        }
    }

    /// <summary>
    /// ThroneRoomController - Manages throne room state
    /// </summary>
    [CustomEntity("MaggyHelper/ThroneRoomController")]
    public class ThroneRoomController : Entity
    {
        private TitanThrone throne;
        private List<CeremonyFlame> flames;
        private List<JudgmentBell> bells;

        public ThroneRoomController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            flames = new List<CeremonyFlame>();
            bells = new List<JudgmentBell>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find throne
            throne = scene.Tracker.GetEntity<TitanThrone>();
            
            // Find flames and bells
            foreach (var flame in scene.Tracker.GetEntities<CeremonyFlame>())
            {
                flames.Add((CeremonyFlame)flame);
            }
            
            foreach (var bell in scene.Tracker.GetEntities<JudgmentBell>())
            {
                bells.Add((JudgmentBell)bell);
            }
        }

        public void OnThroneActivate()
        {
            // Ignite all flames
            foreach (var flame in flames)
            {
                flame.Ignite();
            }
            
            // Ring all bells
            foreach (var bell in bells)
            {
                bell.Ring();
            }
        }
    }
}

