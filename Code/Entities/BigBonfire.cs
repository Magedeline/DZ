using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A larger version of the vanilla Bonfire intended for Chapter 8 maps.
    /// Supports the same Unlit / Lit / Smoking modes but with a bigger visual
    /// footprint: scaled-up sprite, wider light radius and a bigger bloom point.
    /// </summary>
    [CustomEntity("MaggyHelper/BigBonfire")]
    [Tracked(false)]
    public class BigBonfire : Entity
    {
        // ---------------------------------------------------------------
        // Public types
        // ---------------------------------------------------------------
        public enum Mode { Unlit, Lit, Smoking }

        // ---------------------------------------------------------------
        // Inspector-facing properties
        // ---------------------------------------------------------------
        /// <summary>Scale multiplier applied to the campfire sprite (default 2).</summary>
        public float Scale { get; private set; }

        /// <summary>Current lit state.</summary>
        public bool Activated;

        // ---------------------------------------------------------------
        // Private fields
        // ---------------------------------------------------------------
        private Mode mode;
        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private Wiggler wiggle;
        private SoundSource loopSfx;

        private float brightness;
        private float multiplier;

        // ---------------------------------------------------------------
        // Constructors
        // ---------------------------------------------------------------
        public BigBonfire(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            mode = data.Enum("mode", Mode.Unlit);
            Scale = data.Float("scale", 2f);

            // ── Sprite ────────────────────────────────────────────────
            // Reuse the vanilla campfire sprite bank entry.
            sprite = GFX.SpriteBank.Create("campfire");
            sprite.Scale = new Vector2(Scale);
            Add(sprite);

            // ── Lighting ──────────────────────────────────────────────
            float innerRadius = data.Float("lightInner", 64f);
            float outerRadius = data.Float("lightOuter", 128f);
            Vector2 lightOffset = new Vector2(0f, -6f * Scale);

            light = new VertexLight(lightOffset, Color.PaleVioletRed, 1f,
                                    (int)innerRadius, (int)outerRadius);
            Add(light);

            // ── Bloom ─────────────────────────────────────────────────
            float bloomRadius = data.Float("bloomRadius", 64f);
            bloom = new BloomPoint(lightOffset, 1f, bloomRadius);
            Add(bloom);

            // ── Wiggler (flicker effect) ───────────────────────────────
            wiggle = Wiggler.Create(0.2f, 4f, f =>
            {
                light.Alpha = bloom.Alpha =
                    Math.Min(1f, brightness + f * 0.25f) * multiplier;
            });
            Add(wiggle);

            // ── Audio ─────────────────────────────────────────────────
            loopSfx = new SoundSource();
            Add(loopSfx);

            // ── Entity settings ───────────────────────────────────────
            Tag   = Tags.TransitionUpdate;
            Depth = -5;

            // Widen the collider so it is proportional to the bigger sprite.
            Collider = new Hitbox(12f * Scale, 12f * Scale, -6f * Scale, -12f * Scale);
        }

        // ---------------------------------------------------------------
        // Entity lifecycle
        // ---------------------------------------------------------------
        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetMode(mode);
        }

        public override void Update()
        {
            base.Update();

            if (mode == Mode.Lit)
            {
                multiplier = Calc.Approach(multiplier, 1f, Engine.DeltaTime * 2f);

                if (Scene.OnInterval(0.25f))
                {
                    brightness = 0.5f + Calc.Random.NextFloat(0.5f);
                    wiggle.Start();
                }
            }
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------
        public void SetMode(Mode newMode)
        {
            mode = newMode;

            switch (mode)
            {
                default: // Unlit
                    sprite.Play("idle");
                    bloom.Alpha = light.Alpha = brightness = 0f;
                    break;

                case Mode.Lit:
                    bool dreaming = SceneAs<Level>()?.Session?.Dreaming ?? false;

                    if (Activated)
                    {
                        Audio.Play("event:/env/local/campfire_start", Position);
                        loopSfx.Play("event:/env/local/campfire_loop");
                        sprite.Play(dreaming ? "startDream" : "start");
                    }
                    else
                    {
                        loopSfx.Play("event:/env/local/campfire_loop");
                        sprite.Play(dreaming ? "burnDream" : "burn");
                    }
                    break;

                case Mode.Smoking:
                    sprite.Play("smoking");
                    break;
            }

            Activated = true;
        }

        /// <summary>Light the bonfire from another entity or cutscene.</summary>
        public void Light() => SetMode(Mode.Lit);

        /// <summary>Extinguish the bonfire.</summary>
        public void Extinguish()
        {
            loopSfx.Stop();
            SetMode(Mode.Smoking);
        }
    }
}
