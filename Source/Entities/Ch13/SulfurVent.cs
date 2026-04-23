namespace Celeste.Entities.Chapters.Ch13
{
    [CustomEntity("MaggyHelper/SulfurVent")]
    [Tracked]
    public class SulfurVent : Actor
    {
        public enum VentState { Idle, Venting, Cooldown }
        
        public VentState State { get; private set; }
        public float VentDuration { get; private set; }
        public float CooldownTime { get; private set; }
        public float GasSpeed { get; private set; }
        public float GasRadius { get; private set; }
        
        private Sprite sprite;
        private float stateTimer;
        private Level level;
        private List<GasCloud> gasClouds;
        private VertexLight ventLight;
        private Color gasColor;

        public SulfurVent(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Initialize(data.Float("ventDuration", 2f), data.Float("cooldownTime", 4f),
                data.Float("gasSpeed", 80f), data.Float("gasRadius", 40f));
        }

        private void Initialize(float ventDuration, float cooldownTime, float gasSpeed, float gasRadius)
        {
            VentDuration = ventDuration;
            CooldownTime = cooldownTime;
            GasSpeed = gasSpeed;
            GasRadius = gasRadius;
            State = VentState.Idle;
            stateTimer = CooldownTime;
            gasClouds = new List<GasCloud>();
            gasColor = Color.YellowGreen;
            Collider = new Hitbox(32f, 16f, -16f, -16f);
            Add(sprite = GFX.SpriteBank.Create("sulfur_vent"));
            sprite.Play("idle");
            Add(ventLight = new VertexLight(gasColor, 0.3f, 8, 24));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            switch (State)
            {
                case VentState.Idle:
                    stateTimer -= Engine.DeltaTime;
                    if (stateTimer <= 0f)
                    {
                        State = VentState.Venting;
                        stateTimer = VentDuration;
                        sprite.Play("venting");
                        Audio.Play("event:/game/gen_crumble_fall", Position);
                    }
                    break;
                    
                case VentState.Venting:
                    stateTimer -= Engine.DeltaTime;
                    if (Scene.OnInterval(0.2f))
                    {
                        var cloud = new GasCloud(Position, new Vector2(0f, -GasSpeed), GasRadius);
                        gasClouds.Add(cloud);
                        Scene.Add(cloud);
                    }
                    if (stateTimer <= 0f)
                    {
                        State = VentState.Cooldown;
                        stateTimer = CooldownTime;
                        sprite.Play("idle");
                    }
                    break;
                    
                case VentState.Cooldown:
                    stateTimer -= Engine.DeltaTime;
                    if (stateTimer <= 0f)
                    {
                        State = VentState.Idle;
                        stateTimer = 0f;
                    }
                    break;
            }
            
            gasClouds.RemoveAll(g => g == null || g.Scene == null);
        }
    }

    public class GasCloud : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float radius;
        private Color color;

        public GasCloud(Vector2 position, Vector2 velocity, float radius)
            : base(position)
        {
            this.velocity = velocity;
            this.radius = radius;
            maxLifetime = 3f;
            lifetime = maxLifetime;
            color = Color.YellowGreen;
            Collider = new Hitbox(radius * 2, radius * 2, -radius, -radius);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.98f;
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
            }
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, radius, color * (alpha * 0.4f), 12);
        }
    }
}
