namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/FakeHeartGem")]
    public class FakeHeartGem : Entity
    {
        private Sprite sprite;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private Wiggler moveWiggler;
        private Vector2 start;
        private bool collected;
        private float respawnTimer;
        private bool isPersistent;
        private string collectMessage;
        private float respawnTime;
        private ParticleType P_Shatter;
        private ParticleType P_Collect;

        public FakeHeartGem(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            start = Position;
            isPersistent = data.Bool("persistent", false);
            collectMessage = data.Attr("collectMessage", "It's fake!");
            respawnTime = data.Float("respawnTime", 3.0f);

            // Initialize particle types
            P_Shatter = new ParticleType
            {
                Color = Color.Pink,
                Color2 = Color.Red,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)System.Math.PI * 2f,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                SpeedMin = 30f,
                SpeedMax = 80f,
                SpeedMultiplier = 0.25f,
                Acceleration = Vector2.UnitY * 200f
            };

            P_Collect = new ParticleType(P_Shatter)
            {
                Color = Color.LightPink,
                SpeedMin = 10f,
                SpeedMax = 40f
            };

            SetupSprite();
            SetupCollision();
            SetupEffects();
        }

        private void SetupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("maggy_heartgem0"));
            sprite.Play("spin");
            sprite.OnFrameChange = OnSpinFrameChange;

            Collider = new Hitbox(16f, 16f, -8f, -8f);

            Add(new MirrorReflection());
        }

        private void SetupCollision()
        {
            Add(new PlayerCollider(OnPlayer));
        }

        private void SetupEffects()
        {
            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));

            Add(moveWiggler = Wiggler.Create(0.8f, 2f, delegate(float f)
            {
                sprite.Position = Vector2.UnitY * f * -8f;
            }));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            moveWiggler.Start();
        }

        public override void Update()
        {
            base.Update();

            if (collected && respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f && !isPersistent)
                {
                    Respawn();
                }
            }

            // Gentle floating animation
            Position = start + Vector2.UnitY * (float)System.Math.Sin(Scene.TimeActive * 2f) * 2f;
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected) return;

            Logger.Log(LogLevel.Info, "MaggyHelper", "Player collected fake heart gem");

            // Play collection effects
            Audio.Play("event:/game/general/crystalheart_get", Position);
            collected = true;

            // Create particle effects
            level.ParticlesBG.Emit(P_Collect, 10, Position, Vector2.One * 8f);
            level.Shake(0.3f);

            // Show fake message
            Scene.Add(new FakeHeartMessage(collectMessage, Position));

            // Wiggle and hide
            scaleWiggler.Start();
            Collidable = false;

            // Make sprite fade and disappear
            Add(new Coroutine(CollectionSequence()));

            if (!isPersistent)
            {
                respawnTimer = respawnTime;
            }
        }

        private IEnumerator CollectionSequence()
        {
            // Brief pause for effect
            yield return 0.1f;

            // Shatter effect
            level.ParticlesFG.Emit(P_Shatter, 8, Position, Vector2.One * 6f);
            Audio.Play("event:/game/general/crystalheart_bounce", Position);

            // Fade out
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, true);
            tween.OnUpdate = delegate(Tween t)
            {
                sprite.Color = Color.White * (1f - t.Eased);
                light.Alpha = 1f - t.Eased;
            };
            tween.OnComplete = delegate
            {
                if (isPersistent)
                {
                    RemoveSelf();
                }
                else
                {
                    Visible = false;
                }
            };
            Add(tween);

            yield return tween.Duration;
        }

        private void Respawn()
        {
            // Fix: Cast Settings to the correct type before accessing DebugMode
            var settings = IngesteModule.Settings as IngesteModuleSettings;
            if (settings != null && settings.DebugMode)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper", "Fake heart gem respawning");
            }

            collected = false;
            Visible = true;
            Collidable = true;
            sprite.Color = Color.White;
            light.Alpha = 1f;

            // Respawn effect
            level.ParticlesBG.Emit(P_Collect, 5, Position, Vector2.One * 4f);
            Audio.Play("event:/game/general/crystalheart_blue_get", Position);
            scaleWiggler.Start();
        }

        private void OnSpinFrameChange(string anim)
        {
            if (sprite.CurrentAnimationFrame == 0)
            {
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
                scaleWiggler.Start();
                level.ParticlesBG.Emit(P_Collect, 1, Position, Vector2.One * 8f);
            }
        }

        private Level level => (Level)Scene;
    }

    // Helper class for displaying the fake message
    public class FakeHeartMessage : Entity
    {
        private string message;
        private float timer;
        private Vector2 startPos;

        public FakeHeartMessage(string message, Vector2 position) : base(position)
        {
            this.message = message;
            this.startPos = position;
            this.timer = 2.0f;
            Depth = -1000000;
        }

        public override void Update()
        {
            base.Update();
            timer -= Engine.DeltaTime;
            if (timer <= 0f)
            {
                RemoveSelf();
            }

            // Float upward
            Position = startPos + Vector2.UnitY * (2.0f - timer) * -20f;
        }

        public override void Render()
        {
            base.Render();

            float alpha = Calc.Approach(1f, 0f, 1f - (timer / 2.0f));
            Color color = Color.Red * alpha;

            ActiveFont.Draw(message, Position, new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color);
        }
    }
}




