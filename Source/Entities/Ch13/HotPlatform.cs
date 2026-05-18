namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// HotPlatform - Platform that heats up over time
    /// Becomes dangerous after being stood on too long
    /// Sprite path: objects/hot_platform/
    /// </summary>
    [CustomEntity("MaggyHelper/HotPlatform")]
    [Tracked]
    public class HotPlatform : Solid
    {
        #region Enums
        public enum PlatformState
        {
            Cool,
            Warming,
            Hot,
            Overheated,
            Cooling
        }
        #endregion

        #region Properties
        public PlatformState State { get; private set; }
        public float HeatRate { get; private set; }
        public float CoolRate { get; private set; }
        public float MaxHeat { get; private set; }
        public float CurrentHeat { get; private set; }
        
        private Sprite sprite;
        private VertexLight platformLight;
        private float stateTimer;
        private Level level;
        private List<SteamParticle> steamParticles;
        private Player standingPlayer;
        private Color baseColor;
        private Color hotColor;
        #endregion

        #region Constructor
        public HotPlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Initialize(
                data.Float("heatRate", 20f),
                data.Float("coolRate", 10f),
                data.Float("maxHeat", 100f)
            );
        }

        public HotPlatform(Vector2 position, int width, int height, float heatRate = 20f,
            float coolRate = 10f, float maxHeat = 100f)
            : base(position, width, height, false)
        {
            Initialize(heatRate, coolRate, maxHeat);
        }

        private void Initialize(float heatRate, float coolRate, float maxHeat)
        {
            HeatRate = heatRate;
            CoolRate = coolRate;
            MaxHeat = maxHeat;
            CurrentHeat = 0f;
            
            State = PlatformState.Cool;
            stateTimer = 0f;
            steamParticles = new List<SteamParticle>();
            baseColor = Color.Gray;
            hotColor = Color.Orange;
            
            Add(sprite = GFX.SpriteBank.Create("hot_platform"));
            sprite.Play("cool");
            
            Add(platformLight = new VertexLight(baseColor, 0.2f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void ForceCool()
        {
            CurrentHeat = 0f;
            State = PlatformState.Cooling;
            sprite.Play("cooling");
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
            
            // Check for player standing on platform
            standingPlayer = GetPlayerOnTop();
            
            if (standingPlayer != null && State != PlatformState.Overheated)
            {
                // Heat up
                CurrentHeat += HeatRate * Engine.DeltaTime;
            }
            else if (standingPlayer == null && State != PlatformState.Overheated)
            {
                // Cool down
                CurrentHeat -= CoolRate * Engine.DeltaTime;
            }
            
            // Clamp heat
            CurrentHeat = Calc.Clamp(CurrentHeat, 0f, MaxHeat);
            
            // Update state based on heat
            UpdateState();
            
            // Update visuals
            UpdateVisuals();
            
            // Create steam when hot
            if (State == PlatformState.Hot || State == PlatformState.Overheated)
            {
                if (Scene.OnInterval(0.1f))
                {
                    CreateSteamParticle();
                }
            }
            
            // Damage player if overheated
            if (State == PlatformState.Overheated && standingPlayer != null)
            {
                standingPlayer.Die(Vector2.Zero);
            }
            
            steamParticles.RemoveAll(s => s == null || s.Scene == null);
        }

        private new Player GetPlayerOnTop()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                Rectangle topBounds = new Rectangle(
                    (int)(Left - 2),
                    (int)(Top - 4),
                    (int)Width + 4,
                    8
                );
                
                if (player.Collider.Bounds.Intersects(topBounds) && player.OnGround())
                {
                    return player;
                }
            }
            return null;
        }

        private void UpdateState()
        {
            float heatPercent = CurrentHeat / MaxHeat;
            
            if (heatPercent >= 1f)
            {
                if (State != PlatformState.Overheated)
                {
                    State = PlatformState.Overheated;
                    sprite.Play("overheated");
                    Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    level?.Shake(0.2f);
                }
            }
            else if (heatPercent >= 0.7f)
            {
                if (State != PlatformState.Hot)
                {
                    State = PlatformState.Hot;
                    sprite.Play("hot");
                }
            }
            else if (heatPercent >= 0.3f)
            {
                if (State != PlatformState.Warming)
                {
                    State = PlatformState.Warming;
                    sprite.Play("warming");
                }
            }
            else if (heatPercent <= 0f)
            {
                if (State != PlatformState.Cool)
                {
                    State = PlatformState.Cool;
                    sprite.Play("cool");
                }
            }
        }

        private void UpdateVisuals()
        {
            float heatPercent = CurrentHeat / MaxHeat;
            
            // Interpolate color
            Color currentColor = Color.Lerp(baseColor, hotColor, heatPercent);
            sprite.Color = currentColor;
            
            // Update light
            platformLight.Color = currentColor;
            platformLight.Alpha = 0.2f + heatPercent * 0.4f;
        }

        private void CreateSteamParticle()
        {
            var steam = new SteamParticle(
                Position + new Vector2(Calc.Random.NextFloat() * Width - Width / 2, 0f)
            );
            steamParticles.Add(steam);
            Scene.Add(steam);
        }

        public override void Render()
        {
            // Draw heat indicator
            float heatPercent = CurrentHeat / MaxHeat;
            Draw.Rect(Left, Bottom + 2, Width * heatPercent, 4, Color.Orange * 0.6f);
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// SteamParticle - Steam particle from hot platform
    /// </summary>
    public class SteamParticle : Actor
    {
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public SteamParticle(Vector2 position)
            : base(position)
        {
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position.Y -= 40f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            scale += Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 6f * scale, Color.White * (alpha * 0.3f), 5);
        }
    }
}
