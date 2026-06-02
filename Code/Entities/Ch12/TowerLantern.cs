namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// TowerLantern - Light source that can be lit or extinguished
    /// Provides illumination and can activate connected mechanisms
    /// Sprite path: objects/tower_lantern/
    /// </summary>
    [CustomEntity("MaggyHelper/TowerLantern")]
    [Tracked]
    public class TowerLantern : Actor
    {
        #region Enums
        public enum LanternState
        {
            Unlit,
            Lighting,
            Lit,
            Flickering,
            Extinguishing,
            Extinguished
        }
        #endregion

        #region Properties
        public LanternState State { get; private set; }
        public float LightRadius { get; private set; }
        public float FlickerIntensity { get; private set; }
        public string LanternId { get; private set; }
        public bool IsLit => State == LanternState.Lit || State == LanternState.Flickering;
        
        private Sprite sprite;
        private VertexLight lanternLight;
        private float flickerTimer;
        private Level level;
        private List<FlameParticle> flameParticles;
        private bool isActivated;
        private Color lightColor;
        #endregion

        #region Constructor
        public TowerLantern(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("lightRadius", 80f),
                data.Float("flickerIntensity", 0.1f),
                data.Attr("lanternId", ""),
                data.Bool("startLit", false),
                data.HexColor("lightColor", Color.Orange)
            );
        }

        public TowerLantern(Vector2 position, float lightRadius = 80f, float flickerIntensity = 0.1f,
            string lanternId = "", bool startLit = false, Color? lightColor = null)
            : base(position)
        {
            Initialize(lightRadius, flickerIntensity, lanternId, startLit, lightColor ?? Color.Orange);
        }

        private void Initialize(float lightRadius, float flickerIntensity, string lanternId, bool startLit, Color lightColor)
        {
            LightRadius = lightRadius;
            FlickerIntensity = flickerIntensity;
            LanternId = lanternId;
            this.lightColor = lightColor;
            
            State = startLit ? LanternState.Lit : LanternState.Unlit;
            flickerTimer = 0f;
            isActivated = startLit;
            flameParticles = new List<FlameParticle>();
            
            Collider = new Hitbox(12f, 20f, -6f, -20f);
            
            Add(sprite = GFX.SpriteBank.Create("tower_lantern"));
            sprite.Play(startLit ? "lit" : "unlit");
            
            Add(lanternLight = new VertexLight(lightColor, startLit ? 0.8f : 0f, 8, (int)lightRadius));
        }
        #endregion

        #region Public Methods
        public void Light()
        {
            if (IsLit) return;
            
            State = LanternState.Lighting;
            sprite.Play("lighting");
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            Add(new Coroutine(LightRoutine()));
        }

        public void Extinguish()
        {
            if (!IsLit) return;
            
            State = LanternState.Extinguishing;
            sprite.Play("extinguishing");
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(ExtinguishRoutine()));
        }

        public void Toggle()
        {
            if (IsLit)
                Extinguish();
            else
                Light();
        }

        public void SetFlickering(bool flicker)
        {
            if (flicker && IsLit)
            {
                State = LanternState.Flickering;
            }
            else if (IsLit)
            {
                State = LanternState.Lit;
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator LightRoutine()
        {
            for (int i = 0; i < 8; i++)
            {
                CreateFlameParticle();
                lanternLight.Alpha = i / 8f * 0.8f;
                yield return 0.05f;
            }
            
            State = LanternState.Lit;
            sprite.Play("lit");
            lanternLight.Alpha = 0.8f;
            isActivated = true;
            
            // Notify connected mechanisms
            var controller = Scene.Tracker.GetEntity<LanternPuzzleController>();
            controller?.OnLanternLit(this);
        }

        private IEnumerator ExtinguishRoutine()
        {
            for (int i = 8; i >= 0; i--)
            {
                lanternLight.Alpha = i / 8f * 0.8f;
                yield return 0.03f;
            }
            
            State = LanternState.Extinguished;
            sprite.Play("unlit");
            lanternLight.Alpha = 0f;
            isActivated = false;
        }

        private void CreateFlameParticle()
        {
            var particle = new FlameParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 8f - 4f, Calc.Random.NextFloat() * 8f - 4f),
                new Vector2(Calc.Random.NextFloat() * 20f - 10f, -Calc.Random.NextFloat() * 30f),
                lightColor
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
            
            if (IsLit)
            {
                flickerTimer += Engine.DeltaTime * 10f;
                
                // Flicker effect
                float flicker = 1f;
                if (State == LanternState.Flickering)
                {
                    flicker = 0.7f + (float)Math.Sin(flickerTimer) * FlickerIntensity * 3f;
                }
                else
                {
                    flicker = 0.9f + (float)Math.Sin(flickerTimer) * FlickerIntensity;
                }
                
                lanternLight.Alpha = 0.8f * flicker;
                
                // Create flame particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateFlameParticle();
                }
            }
            
            flameParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw light radius indicator
            if (IsLit)
            {
                Draw.Circle(Position - Vector2.UnitY * 10f, LightRadius * 0.5f, lightColor * 0.1f, 16);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FlameParticle - Small flame particle
    /// </summary>
    public class FlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public FlameParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.5f - 0.2f) + 0.2f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 60f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, color * (alpha * 0.5f), 3);
        }
    }

    /// <summary>
    /// LanternPuzzleController - Manages lantern puzzle sequences
    /// </summary>
    [CustomEntity("MaggyHelper/LanternPuzzleController")]
    public class LanternPuzzleController : Entity
    {
        private List<TowerLantern> lanterns;
        private List<int> requiredSequence;
        private List<int> currentSequence;
        private bool puzzleComplete;

        public LanternPuzzleController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            lanterns = new List<TowerLantern>();
            requiredSequence = new List<int>();
            currentSequence = new List<int>();
            puzzleComplete = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find all lanterns
            foreach (var lantern in scene.Tracker.GetEntities<TowerLantern>())
            {
                lanterns.Add((TowerLantern)lantern);
            }
        }

        public void OnLanternLit(TowerLantern lantern)
        {
            int index = lanterns.IndexOf(lantern);
            currentSequence.Add(index);
            
            // Check sequence
            if (currentSequence.Count == requiredSequence.Count)
            {
                bool correct = true;
                for (int i = 0; i < requiredSequence.Count; i++)
                {
                    if (currentSequence[i] != requiredSequence[i])
                    {
                        correct = false;
                        break;
                    }
                }
                
                if (correct)
                {
                    CompletePuzzle();
                }
                else
                {
                    ResetPuzzle();
                }
            }
        }

        private void CompletePuzzle()
        {
            puzzleComplete = true;
            var level = Scene as Level;
            level?.Session.SetFlag("lantern_puzzle_complete", true);
            level?.Flash(Color.Gold * 0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        private void ResetPuzzle()
        {
            currentSequence.Clear();
            
            // Extinguish all lanterns
            foreach (var lantern in lanterns)
            {
                lantern.Extinguish();
            }
        }

        public void SetRequiredSequence(List<int> sequence)
        {
            requiredSequence = sequence;
        }
    }
}
