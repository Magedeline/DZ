namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/DreamOrb")]
    [Tracked]
    public class DreamOrb : Entity
    {
        public static ParticleType P_Shatter;
        public static ParticleType P_Regen;
        public static ParticleType P_Glow;

        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Level level;
        private SineWave sine;
        private bool oneUse;
        private float respawnTimer;
        private bool collected;

        public DreamOrb(Vector2 position, bool oneUse)
            : base(position)
        {
            this.oneUse = oneUse;
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            
            string spritePath = "objects/MaggyHelper/dreamorb/";
            Add(sprite = new Sprite(GFX.Game, spritePath));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.2f);
            }));
            
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.MediumPurple, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f).Randomize());
            
            base.Depth = -100;
        }

        public DreamOrb(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (base.Scene.OnInterval(0.1f) && !collected)
            {
                level.ParticlesFG.Emit(P_Glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                collected = false;
                Collidable = true;
                sprite.Visible = true;
                base.Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(P_Regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            sprite.Y = bloom.Y = sine.Value * 2f;
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            // Enable dream dashing for the player via session
            level.Session.Inventory.DreamDash = true;
            
            Audio.Play("event:/game/05_mirror_temple/mirrormask_emit", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            collected = true;
            Collidable = false;
            
            Add(new Coroutine(CollectRoutine(player)));
            
            if (!oneUse)
            {
                respawnTimer = 5f;
            }
        }

        private IEnumerator CollectRoutine(Player player)
        {
            Celeste.Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = false;
            
            base.Depth = 8999;
            yield return 0.05f;
            
            float angle = player.Speed.Angle();
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, angle - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, angle + (float)Math.PI / 2f);
            SlashFx.Burst(Position, angle);
            
            if (oneUse)
            {
                RemoveSelf();
            }
        }

        public static void LoadParticles()
        {
            P_Shatter = new ParticleType
            {
                Color = Color.MediumPurple,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.25f,
                LifeMax = 0.4f,
                SpeedMin = 80f,
                SpeedMax = 120f
            };
            P_Regen = new ParticleType(P_Shatter);
            P_Glow = new ParticleType
            {
                Color = Color.MediumPurple * 0.5f,
                Color2 = Color.DarkViolet * 0.5f,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 4f,
                SpeedMax = 8f
            };
        }
    }
}
