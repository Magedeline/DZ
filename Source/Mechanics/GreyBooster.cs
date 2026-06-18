namespace Celeste.Entities
{
    [CustomEntity("DesoloZatnas/GreyBooster")]
    [Tracked]
    public class GreyBooster : Entity
    {
        public static ParticleType P_Burst;
        public static ParticleType P_BurstRed;
        public static ParticleType P_Appear;

        private const float RespawnTime = 1f;
        private const float BoostSpeed = 240f;

        private Sprite sprite;
        private Entity outline;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private DashListener dashListener;
        private ParticleType particleType;
        private Coroutine dashRoutine;
        private SoundSource loopingSfx;
        
        private float respawnTimer;
        private float cannotUseTimer;
        private bool red;
        
        // Glider/holdable tracking
        private Entity carriedGlider;
        private bool gliderWasCarried;
        
        public bool BoostingPlayer { get; private set; }

        public GreyBooster(Vector2 position, bool red)
            : base(position)
        {
            // Initialize particles if not already done
            if (P_Burst == null)
            {
                InitializeParticles();
            }
            
            this.red = red;
            Depth = -8500;
            Collider = new Circle(10f, 0f, 2f);

            Add(new PlayerCollider(OnPlayer));
            Add(sprite = GFX.SpriteBank.Create("greyBooster"));
            Add(new MirrorReflection());
            Add(dashListener = new DashListener());
            dashListener.OnDash = OnPlayerDashed;
            Add(dashRoutine = new Coroutine(false));
            Add(loopingSfx = new SoundSource());
            Add(light = new VertexLight(Color.White, 1f, 16, 32));
            Add(bloom = new BloomPoint(Position, 0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));

            particleType = red ? P_BurstRed : P_Burst;
        }

        public GreyBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("red", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Image image = new Image(GFX.Game["objects/greybooster/outline"]);
            image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            outline = new Entity(Position);
            outline.Depth = 8999;
            outline.Visible = false;
            outline.Add(image);
            outline.Add(new MirrorReflection());
            scene.Add(outline);
        }

        public override void Update()
        {
            base.Update();

            if (cannotUseTimer > 0f)
            {
                cannotUseTimer -= Engine.DeltaTime;
            }

            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }

            if (!dashRoutine.Active && respawnTimer <= 0f)
            {
                Vector2 target = Vector2.Zero;
                global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && CollideCheck(player))
                {
                    target = player.Center + new Vector2(0f, -2f) - Position;
                }
                sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
            }

            if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<global::Celeste.Player>())
            {
                sprite.Play("loop");
            }
        }

        private void Respawn()
        {
            string respawnEvent = red 
                ? "event:/Mods/pusheen/game/07_inferno/redbooster_reappear"
                : "event:/Mods/pusheen/game/06_stronghold/greenbooster_reappear";
            Audio.Play(respawnEvent, Position);
            
            sprite.Position = Vector2.Zero;
            sprite.Play("loop", true);
            sprite.Visible = true;
            outline.Visible = false;
            AppearParticles();
        }

        private void AppearParticles()
        {
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(P_Appear, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
            {
                cannotUseTimer = 0.45f;
                
                // Check if player is carrying a glider/jellyfish
                CheckForCarriedGlider(player);
                
                if (red)
                {
                    RedBoost(player);
                }
                else
                {
                    GreenBoost(player);
                }
                
                // Play enter sound
                string enterEvent = red 
                    ? "event:/Mods/pusheen/game/07_inferno/redbooster_enter"
                    : "event:/Mods/pusheen/game/06_stronghold/greenbooster_enter";
                Audio.Play(enterEvent, Position);
                
                wiggler.Start();
                sprite.Play("inside");
                sprite.FlipX = player.Facing == Facings.Left;
            }
        }

        private void CheckForCarriedGlider(global::Celeste.Player player)
        {
            // Reset previous glider state
            carriedGlider = null;
            gliderWasCarried = false;
            
            // Check if player is holding something
            if (player.Holding != null && player.Holding.Entity != null)
            {
                Entity heldEntity = player.Holding.Entity;
                
                // Check if it's a glider type entity (GlitchGlider, Jellyfish, etc.)
                if (IsGliderEntity(heldEntity))
                {
                    carriedGlider = heldEntity;
                    gliderWasCarried = true;
                    
                    // Make glider follow the boost
                    carriedGlider.Collidable = false;
                }
            }
            
            // Also check for nearby glider entities
            if (carriedGlider == null)
            {
                // Check all entities in the scene
                foreach (var entity in Scene.Entities)
                {
                    if (IsGliderEntity(entity) && Vector2.Distance(entity.Position, player.Position) < 20f)
                    {
                        carriedGlider = entity;
                        gliderWasCarried = true;
                        carriedGlider.Collidable = false;
                        break;
                    }
                }
            }
        }

        private bool IsGliderEntity(Entity entity)
        {
            if (entity == null) return false;
            
            // Check entity type name for glider-like entities
            string typeName = entity.GetType().Name;
            
            return typeName.Contains("Glider") || 
                   typeName.Contains("Jellyfish") || 
                   typeName.Contains("Feather");
        }

        private IEnumerator BoostRoutine(global::Celeste.Player player, Vector2 dir)
        {
            float duration = red ? 0.3f : 0.25f;
            float timer = 0f;

            while (timer < duration && player.StateMachine.State == Player.StDash)
            {
                player.Speed = dir;
                sprite.RenderPosition = player.Center + new Vector2(0f, -2f);
                outline.Position = sprite.Position;
                
                // Move carried glider alongside player
                if (gliderWasCarried && carriedGlider != null && carriedGlider.Scene != null)
                {
                    // Position glider slightly offset from player
                    Vector2 gliderOffset = new Vector2(
                        player.Facing == Facings.Left ? -8f : 8f,
                        -6f
                    );
                    carriedGlider.Position = player.Center + gliderOffset;
                    
                    // Emit particles around glider to show it's being carried
                    if (Scene.OnInterval(0.04f))
                    {
                        (Scene as Level).ParticlesBG.Emit(particleType, 1, 
                            carriedGlider.Center, Vector2.One * 4f);
                    }
                }
                
                if (Scene.OnInterval(0.02f))
                {
                    (Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center, Vector2.One * 3f);
                }
                
                timer += Engine.DeltaTime;
                yield return null;
            }

            PlayerReleased();
            
            // Release glider
            if (gliderWasCarried && carriedGlider != null && carriedGlider.Scene != null)
            {
                ReleaseGlider(player);
            }
            
            if (red)
            {
                player.StateMachine.State = Player.StNormal;
            }
        }

        private void ReleaseGlider(global::Celeste.Player player)
        {
            if (carriedGlider == null) return;
            
            // Restore glider's collidable state
            carriedGlider.Collidable = true;
            
            // Give glider a small velocity using reflection/dynamic to handle different types
            try
            {
                // Try to set speed on the glider if it has a Speed property
                var speedProperty = carriedGlider.GetType().GetProperty("Speed");
                if (speedProperty != null && speedProperty.CanWrite)
                {
                    Vector2 releaseVelocity = player.Speed * 0.3f;
                    speedProperty.SetValue(carriedGlider, releaseVelocity);
                }
            }
            catch
            {
                // If setting speed fails, just position the glider
            }
            
            // Emit particles when releasing
            (Scene as Level).ParticlesBG.Emit(particleType, 6, 
                carriedGlider.Center, Vector2.One * 6f);
            
            // Position glider near player
            carriedGlider.Position = player.Center + new Vector2(
                player.Facing == Facings.Left ? -12f : 12f,
                -8f
            );
            
            carriedGlider = null;
            gliderWasCarried = false;
        }

        private void RedBoost(global::Celeste.Player player)
        {
            // Play dash sound
            Audio.Play("event:/Mods/pusheen/game/07_inferno/redbooster_dash", Position);
            
            player.StateMachine.State = Player.StDash; // RedDash state
            player.Speed = Vector2.Zero;
            player.DashDir = Vector2.UnitX * (float)player.Facing;
            BoostingPlayer = true;
            Tag = Tags.Persistent | Tags.TransitionUpdate;
            sprite.Play("spin");
            sprite.FlipX = player.Facing == Facings.Left;
            outline.Visible = true;
            
            // Start looping movement sound
            loopingSfx.Play("event:/Mods/pusheen/game/07_inferno/redbooster_move");
            
            dashRoutine.Replace(BoostRoutine(player, Vector2.UnitX * (float)player.Facing * BoostSpeed));
        }

        private void GreenBoost(global::Celeste.Player player)
        {
            // Play dash sound
            Audio.Play("event:/Mods/pusheen/game/06_stronghold/greenbooster_dash", Position);
            
            player.StateMachine.State = Player.StDash; // RedDash state  
            player.Speed = Vector2.Zero;
            Vector2 direction = (player.Center - Center).SafeNormalize();
            player.DashDir = direction;
            BoostingPlayer = true;
            Tag = Tags.Persistent | Tags.TransitionUpdate;
            sprite.Play("spin");
            sprite.FlipX = player.Facing == Facings.Left;
            outline.Visible = true;
            
            // Start looping movement sound
            loopingSfx.Play("event:/Mods/pusheen/game/07_inferno/redbooster_move");
            
            dashRoutine.Replace(BoostRoutine(player, direction * BoostSpeed));
        }

        private void PlayerReleased()
        {
            // Stop looping sound
            loopingSfx.Stop();
            
            // Play end sound
            string endEvent = red 
                ? "event:/Mods/pusheen/game/07_inferno/redbooster_end"
                : "event:/Mods/pusheen/game/06_stronghold/greenbooster_end";
            Audio.Play(endEvent, sprite.RenderPosition);
            
            sprite.Play("pop");
            cannotUseTimer = 0f;
            respawnTimer = RespawnTime;
            BoostingPlayer = false;
            Tag = 0;
        }

        private void OnPlayerDashed(Vector2 direction)
        {
            if (BoostingPlayer)
            {
                BoostingPlayer = false;
            }
        }

        public override void Render()
        {
            Vector2 position = sprite.Position;
            sprite.Position = position.Floor();
            if (sprite.CurrentAnimationID != "pop" && sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
            sprite.Position = position;
        }

        public static void InitializeParticles()
        {
            P_Burst = new ParticleType
            {
                Color = Calc.HexToColor("808080"),
                Color2 = Calc.HexToColor("505050"),
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                DirectionRange = (float)Math.PI * 2f,
                LifeMin = 0.4f,
                LifeMax = 0.6f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                SpeedMultiplier = 0.25f
            };

            P_BurstRed = new ParticleType(P_Burst)
            {
                Color = Calc.HexToColor("ff6a6a"),
                Color2 = Calc.HexToColor("ff0000")
            };

            P_Appear = new ParticleType
            {
                Color = Calc.HexToColor("a0a0a0"),
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                DirectionRange = (float)Math.PI * 2f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 2f,
                SpeedMax = 16f
            };
        }
    }
}




