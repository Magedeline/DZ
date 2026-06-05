namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// FountainSpirit - Water spirit that heals or creates platforms
    /// Benevolent entity that can help player or grant buffs
    /// Sprite path: characters/fountain_spirit/
    /// </summary>
    [CustomEntity("MaggyHelper/FountainSpirit")]
    [Tracked]
    public class FountainSpirit : Actor
    {
        #region Enums
        public enum SpiritState
        {
            Dormant,
            Emerging,
            Active,
            Healing,
            Blessing,
            Fading,
            Complete
        }

        public enum SpiritType
        {
            Healing,
            Platform,
            Buff,
            Guidance
        }
        #endregion

        #region Properties
        public SpiritState State { get; private set; }
        public SpiritType Type { get; private set; }
        public int HealAmount { get; private set; }
        public float BuffDuration { get; private set; }
        public bool HasInteracted { get; private set; }
        
        private Sprite sprite;
        private TalkComponent talkComponent;
        private Player interactingPlayer;
        private Level level;
        private VertexLight spiritLight;
        private List<WaterParticle> waterParticles;
        private float floatTimer;
        private bool isActive;
        #endregion

        #region Constructor
        public FountainSpirit(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Enum("spiritType", SpiritType.Healing),
                data.Int("healAmount", 3),
                data.Float("buffDuration", 10f)
            );
        }

        public FountainSpirit(Vector2 position, SpiritType type = SpiritType.Healing,
            int healAmount = 3, float buffDuration = 10f)
            : base(position)
        {
            Initialize(type, healAmount, buffDuration);
        }

        private void Initialize(SpiritType type, int healAmount, float buffDuration)
        {
            Type = type;
            HealAmount = healAmount;
            BuffDuration = buffDuration;
            
            State = SpiritState.Dormant;
            HasInteracted = false;
            floatTimer = 0f;
            isActive = false;
            waterParticles = new List<WaterParticle>();
            
            Collider = new Hitbox(24f, 32f, -12f, -32f);
            Add(sprite = GFX.SpriteBank.Create("fountain_spirit"));
            sprite.Play("dormant");
            
            Color lightColor = type == SpiritType.Healing ? Color.Cyan :
                type == SpiritType.Buff ? Color.Gold : Color.LightBlue;
            Add(spiritLight = new VertexLight(lightColor, 0.2f, 12, 32));
            
            Add(talkComponent = new TalkComponent(
                new Rectangle(-20, -40, 40, 48),
                new Vector2(0f, -48f),
                _ => Interact()
            ));
        }
        #endregion

        #region Public Methods
        public void Interact()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;
            if (HasInteracted) return;
            
            interactingPlayer = player;
            Add(new Coroutine(InteractionRoutine()));
        }

        public void Activate()
        {
            if (isActive) return;
            
            isActive = true;
            State = SpiritState.Emerging;
            sprite.Play("emerge");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(EmergeRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator EmergeRoutine()
        {
            for (int i = 0; i < 12; i++)
            {
                CreateWaterParticle();
                yield return 0.05f;
            }
            
            State = SpiritState.Active;
            sprite.Play("active");
            spiritLight.Alpha = 0.6f;
        }

        private IEnumerator InteractionRoutine()
        {
            interactingPlayer.StateMachine.State = Player.StDummy;
            interactingPlayer.StateMachine.Locked = true;
            
            State = SpiritState.Healing;
            sprite.Play("healing");
            
            yield return Textbox.Say("FOUNTAIN_SPIRIT_GREETING");
            
            // Apply spirit effect
            switch (Type)
            {
                case SpiritType.Healing:
                    yield return ApplyHealing();
                    break;
                case SpiritType.Platform:
                    yield return CreatePlatform();
                    break;
                case SpiritType.Buff:
                    yield return ApplyBuff();
                    break;
                case SpiritType.Guidance:
                    yield return ShowGuidance();
                    break;
            }
            
            HasInteracted = true;
            State = SpiritState.Complete;
            sprite.Play("complete");
            
            interactingPlayer.StateMachine.Locked = false;
            interactingPlayer.StateMachine.State = Player.StNormal;
        }

        private IEnumerator ApplyHealing()
        {
            for (int i = 0; i < HealAmount; i++)
            {
                // Heal effect
                level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, 8, interactingPlayer.Position, Vector2.One * 6f, Color.Cyan);
                Audio.Play("event:/game/general/diamond_get", Position);
                yield return 0.3f;
            }
            
            yield return Textbox.Say("FOUNTAIN_SPIRIT_HEALED");
        }

        private IEnumerator CreatePlatform()
        {
            var platform = new WaterPlatform(Position + new Vector2(0f, 40f));
            Scene.Add(platform);
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.1f);
            
            yield return Textbox.Say("FOUNTAIN_SPIRIT_PLATFORM");
        }

        private IEnumerator ApplyBuff()
        {
            // Set buff flag
            level?.Session.SetFlag("fountain_buff_active", true);
            level?.Session.SetFlag("fountain_buff_duration", BuffDuration > 0f);
            
            // Visual effect
            for (int i = 0; i < 15; i++)
            {
                CreateWaterParticle();
                yield return 0.05f;
            }
            
            level?.Flash(Color.Gold * 0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            yield return Textbox.Say("FOUNTAIN_SPIRIT_BUFF");
        }

        private IEnumerator ShowGuidance()
        {
            // Reveal hidden path
            level?.Session.SetFlag("fountain_guidance_shown", true);
            
            yield return Textbox.Say("FOUNTAIN_SPIRIT_GUIDANCE");
        }

        private void CreateWaterParticle()
        {
            var particle = new WaterParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f)
            );
            waterParticles.Add(particle);
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
            
            if (State >= SpiritState.Active)
            {
                floatTimer += Engine.DeltaTime * 2f;
                sprite.Y = (float)Math.Sin(floatTimer) * 4f;
                
                if (Scene.OnInterval(0.15f))
                    CreateWaterParticle();
            }
            
            waterParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (State >= SpiritState.Active)
            {
                Draw.Circle(Position - Vector2.UnitY * 16f, 24f, Color.Cyan * 0.15f, 12);
            }
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// WaterParticle - Particle for water spirit effects
    /// </summary>
    public class WaterParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public WaterParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 40f * Engine.DeltaTime;
            velocity *= 0.97f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f, Color.Cyan * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// WaterPlatform - Platform created by FountainSpirit
    /// </summary>
    public class WaterPlatform : Solid
    {
        private Sprite sprite;
        private float lifetime;
        private float maxLifetime;

        public WaterPlatform(Vector2 position)
            : base(position, 64f, 8f, false)
        {
            maxLifetime = 10f;
            lifetime = maxLifetime;
            Add(sprite = GFX.SpriteBank.Create("water_platform"));
        }

        public override void Update()
        {
            base.Update();
            lifetime -= Engine.DeltaTime;
            
            // Fade out near end
            if (lifetime < 2f)
            {
                sprite.Color = Color.White * (lifetime / 2f);
            }
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            Draw.Rect(Collider.Bounds, Color.Cyan * 0.4f);
            base.Render();
        }
    }
}

