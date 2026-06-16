using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/SnowBerry")]
    [Tracked]
    public class SnowBerry : Entity
    {
        public static ParticleType P_Glow;
        public static ParticleType P_GhostGlow;

        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Level level;
        private SineWave sine;
        private Follower follower;
        private bool collected;
        private float collectTimer;

        public EntityID ID;
        public bool ReturnHomeWhenLost = true;

        public SnowBerry(EntityData data, Vector2 offset, EntityID gid)
            : base(data.Position + offset)
        {
            ID = gid;
            
            base.Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(p => OnPlayer(p)));
            Add(new MirrorReflection());
            
            Add(sprite = AtlasPathHelper.CreateSprite("objects/snowberry"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            
            Add(bloom = new BloomPoint(1f, 12f));
            Add(light = new VertexLight(Color.LightBlue, 1f, 16, 24));
            Add(sine = new SineWave(0.5f).Randomize());
            
            Add(follower = new Follower(ID, null, OnLoseLeader));
            follower.FollowDelay = 0.3f;
            follower.PersistentFollow = false;
            
            base.Depth = -100;
            base.Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            
            // Check if already collected
            if (level.Session.DoNotLoad.Contains(ID))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!collected)
            {
                sprite.Y = sine.Value * 2f;
                bloom.Y = sprite.Y;
            }
            else
            {
                collectTimer += Engine.DeltaTime;
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (!collected && follower.Leader == null)
            {
                Audio.Play("event:/game/general/seed_touch", Position);
                player.Leader.GainFollower(follower);
                Collidable = false;
                collected = true;
                wiggler.Start();
                base.Depth = -1000000;
            }
        }

        private void OnLoseLeader()
        {
            if (!collected)
            {
                return;
            }
            
            Audio.Play("event:/game/general/seed_poof", Position);
            Collidable = false;
            sprite.Visible = false;
            
            // Particles on death
            for (int i = 0; i < 6; i++)
            {
                float angle = Calc.Random.NextAngle();
                level.ParticlesFG.Emit(P_Glow, Position, angle);
            }
            
            if (ReturnHomeWhenLost)
            {
                Add(new Coroutine(ReturnRoutine()));
            }
            else
            {
                RemoveSelf();
            }
        }

        private IEnumerator ReturnRoutine()
        {
            yield return 1.5f;
            
            collected = false;
            Collidable = true;
            sprite.Visible = true;
            
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(P_Glow, 8, Position, Vector2.One * 4f);
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        public static void LoadParticles()
        {
            P_Glow = new ParticleType
            {
                Color = Color.LightBlue,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.4f,
                LifeMax = 0.6f,
                SpeedMin = 20f,
                SpeedMax = 40f
            };
            P_GhostGlow = new ParticleType(P_Glow)
            {
                Color = Color.LightCyan * 0.5f
            };
        }
    }
}

