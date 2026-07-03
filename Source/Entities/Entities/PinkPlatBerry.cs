using CelesteEntities = global::Celeste.Mod.Entities;

#nullable enable
namespace Celeste.Entities
{
    [CelesteEntities.CustomEntity("DZ/PinkPlatinumStrawberry")]
    [Monocle.Tracked]
    [CelesteEntities.RegisterStrawberry(tracked: true, blocksCollection: true)]
    [TrackedAs(typeof(CelesteStrawberry))]
    internal class PinkPlatinumBerry : Entity, IStrawberry
    {
        private enum CollectSound
        {
            Original,
            Elaborate,
            Minimalist,
            Custom,
        }

        public bool CommandSpawned;
        public bool Dead = false;
        public bool Golden = true;
        public bool ReturnHomeWhenLost = true;
        public EntityID Id;
        public Follower Follower;

        public static ParticleType? PPlatGlow;
        public static ParticleType? PGhostGlow;
        private bool collected;
        private float wobble = 0.0f;
        private float collectTimer = 0.0f;
        private bool isGhostBerry;
        private string customCollectSound;
        private Vector2 start;
        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private CollectSound collectSound;

        public PinkPlatinumBerry(EntityData data, Vector2 offset, EntityID gid)
        {
            this.Id = gid;
            this.Position = this.start = data.Position + offset;
            this.isGhostBerry = SaveData.Instance.CheckStrawberry(this.Id);
            this.Depth = -100;
            this.Collider = new Hitbox(14f, 14f, -7f, -7f);
            this.Add(new PlayerCollider(new Action<global::Celeste.Player>(this.OnPlayer)));
            this.Add(new MirrorReflection());
            this.Add(this.Follower = new Follower(this.Id, onLoseLeader: new Action(this.OnLoseLeader)));
            this.Follower.FollowDelay = 0.3f;
            this.sprite = DZModule.SpriteBank.Create("pinkplatinumberry");
            this.wiggler = Wiggler.Create(0.4f, 4f, v => this.sprite.Scale = Vector2.One * (1.0f + v * 0.35f));
            this.bloom = new BloomPoint(0.15f, 12f);
            this.light = new VertexLight(Color.White, 1f, 16, 24);
            this.lightTween = this.light.CreatePulseTween();
            this.collectSound = data.Enum<CollectSound>(nameof(collectSound));
            this.customCollectSound = data.Attr(nameof(customCollectSound), string.Empty);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            ((Level)Scene).Session.GrabbedGolden = true;
            if (Follower.Leader != null || collected) return;

            ReturnHomeWhenLost = false;

            var soundEvent = isGhostBerry
                ? "event:/game/general/strawberry_blue_touch"
                : "event:/game/general/strawberry_touch";

            Audio.Play(soundEvent, Position);

            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }

        public static void LoadParticles()
        {
            if (PPlatGlow == null)
            {
                PPlatGlow = new ParticleType
                {
                    Size = 1f,
                    Color = Calc.HexToColor("f1cff4"),
                    Color2 = Calc.HexToColor("d4ffff"),
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.8f,
                    LifeMax = 1.4f,
                    SpeedMin = 8f,
                    SpeedMax = 16f,
                    DirectionRange = (float)Math.PI * 2f
                };
            }

            if (PGhostGlow == null)
            {
                PGhostGlow = new ParticleType
                {
                    Size = 1f,
                    Color = Calc.HexToColor("d4ffff"),
                    Color2 = Calc.HexToColor("f1cff4"),
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.8f,
                    LifeMax = 1.4f,
                    SpeedMin = 8f,
                    SpeedMax = 16f,
                    DirectionRange = (float)Math.PI * 2f
                };
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            LoadParticles();
            this.Add(this.sprite);
            if (!this.isGhostBerry)
                this.sprite.Play("idle");
            else
                this.sprite.Play("ghost");
            this.sprite.OnFrameChange = new Action<string>(this.OnAnimate);
            this.Add(this.wiggler);
            this.Add(this.bloom);
            this.Add(this.light);
            this.Add(this.lightTween);
        }

        public override void Update()
        {
            if (!this.collected)
            {
                this.wobble += Engine.DeltaTime * 3f;
                float num = sprite.Y = this.bloom.Y = this.light.Y = (float)Math.Sin(this.wobble) * 2f;
                
                // Pink Platinum Berry acts like golden - no auto-collect
                // Only collects when triggered by AreaComplete or specific triggers
            }

            base.Update();
            if (this.Follower.Leader == null || !this.Scene.OnInterval(0.08f))
                return;
            if (PPlatGlow != null && PGhostGlow != null)
            {
                ParticleType type = this.isGhostBerry ? PGhostGlow : PPlatGlow;
                this.SceneAs<Level>().ParticlesFG.Emit(type,
                    this.Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
        }

        private void OnAnimate(string id)
        {
            if (this.sprite.CurrentAnimationFrame != 30)
                return;
            this.lightTween.Start();
            if (!this.collected && (this.CollideCheck<FakeWall>() || this.CollideCheck<Solid>()))
            {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.1f);
            }
            else
            {
                Audio.Play("event:/game/general/strawberry_pulse", this.Position);
                this.SceneAs<Level>().Displacement.AddBurst(this.Position, 0.6f, 4f, 28f, 0.2f);
            }
        }

        public void OnCollect()
        {
            if (this.collected)
                return;
            this.collected = true;
            if (this.Follower.Leader != null)
            {
                global::Celeste.Player? entity = this.Follower.Leader?.Entity as global::Celeste.Player;
                if (entity != null)
                {
                    ++entity.StrawberryCollectIndex;
                    entity.StrawberryCollectResetTimer = 2.5f;
                    this.Follower.Leader?.LoseFollower(this.Follower);
                }
            }

            Session session = ((Level)this.Scene).Session;
            if (!this.CommandSpawned)
            {
                SaveData.Instance.AddStrawberry(this.Id, this.Golden);
                session.DoNotLoad.Add(this.Id);
                session.Strawberries.Add(this.Id);
            }

            DZProgressionManager.RecordPinkPlatinumBerry(this.Scene as Level, this.Id.ToString());

            session.UpdateLevelStartDashes();
            this.Add(new Coroutine(this.collectRoutine()));
        }

        private void OnLoseLeader()
        {
            if (this.collected)
                return;

            // Golden berry behavior - kill the player when berry is lost
            // Use Tracker.GetEntity instead of FindAll to avoid iterating all entities
            var player = this.Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !player.Dead)
            {
                player.Die(Vector2.Zero);
            }

            Audio.Play("event:/new_content/char/madeline/death_golden");
            this.Dead = true;
        }

        private IEnumerator collectRoutine()
        {
            Scene scene = this.Scene;
            this.Tag = (int)Tags.TransitionUpdate;
            this.Depth = -2000010;
            switch (this.collectSound)
            {
                case CollectSound.Original:
                    Audio.Play("event:/game/general/strawberry_get", this.Position, "colour",
                        this.isGhostBerry ? 1f : 2f, "count", 10f);
                    break;
                case CollectSound.Elaborate:
                    Audio.Play("event:/game/general/plat_berry_get");
                    break;
                case CollectSound.Minimalist:
                    Audio.Play("event:/game/general/plat_berry_get_minimalist");
                    break;
                case CollectSound.Custom:
                    Audio.Play(this.customCollectSound);
                    break;
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            this.sprite.Play("collect");
            while (this.sprite.Animating)
                yield return null;
            this.Scene.Add(new PinkPlatberryPoints(this.Position, this.isGhostBerry));
            this.RemoveSelf();
        }

        [Command("give_plat", "gives you a platinum strawberry")]
        private static void cmdGivePlat()
        {
            if (!(Engine.Scene is Level scene))
                return;
            global::Celeste.Player entity = scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                EntityData data = new EntityData();
                data.Position = entity.Position + new Vector2(0.0f, -16f);
                data.ID = Calc.Random.Next();
                data.Name = "DZ/PinkPlatinumStrawberry";
                scene.Add(new PinkPlatinumBerry(data, Vector2.Zero, new EntityID(scene.Session.Level, data.ID))
                {
                    CommandSpawned = true
                });
            }
        }
    }
}

