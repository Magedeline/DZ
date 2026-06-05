namespace Celeste.Entities.Chapters.Ch16
{
    [CustomEntity("MaggyHelper/RealityAnchor")]
    [Tracked]
    public class RealityAnchor : Actor
    {
        public enum AnchorState { Inactive, Activating, Stable, Overloaded }
        
        public AnchorState State { get; private set; }
        public float StabilityRadius { get; private set; }
        
        private Sprite sprite;
        private VertexLight anchorLight;
        private Level level;
        private Color anchorColor;

        public RealityAnchor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Initialize(data.Float("stabilityRadius", 100f));
        }

        private void Initialize(float stabilityRadius)
        {
            StabilityRadius = stabilityRadius;
            State = AnchorState.Inactive;
            anchorColor = Color.White;
            Collider = new Hitbox(32f, 40f, -16f, -40f);
            Add(sprite = GFX.SpriteBank.Create("reality_anchor"));
            sprite.Play("inactive");
            Add(anchorLight = new VertexLight(anchorColor, 0.2f, 12, 32));
        }

        public void Activate()
        {
            State = AnchorState.Stable;
            sprite.Play("stable");
            anchorLight.Alpha = 0.6f;
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            if (State == AnchorState.Stable)
            {
                // Stabilize nearby reality glitches
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Vector2.Distance(Position, player.Position) < StabilityRadius)
                {
                    // Apply stability effect
                }
            }
        }

        public override void Render()
        {
            if (State == AnchorState.Stable)
            {
                Draw.Circle(Position, StabilityRadius, Color.White * 0.1f, 16);
            }
            base.Render();
        }
    }

    [CustomEntity("MaggyHelper/VoidBarrier")]
    [Tracked]
    public class VoidBarrier : Solid
    {
        public enum BarrierState { Active, Fading, Disabled }
        
        public BarrierState State { get; private set; }
        private Sprite sprite;
        private VertexLight barrierLight;
        private Color voidColor;

        public VoidBarrier(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            voidColor = Color.Purple;
            State = BarrierState.Active;
            Add(sprite = GFX.SpriteBank.Create("void_barrier"));
            sprite.Play("active");
            Add(barrierLight = new VertexLight(voidColor, 0.4f, 8, 24));
        }

        public void Disable()
        {
            State = BarrierState.Disabled;
            Collidable = false;
            sprite.Play("disabled");
            barrierLight.Alpha = 0f;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        public override void Render()
        {
            if (State == BarrierState.Active)
            {
                Draw.Rect(Collider.Bounds, voidColor * 0.4f);
            }
            base.Render();
        }
    }
}

