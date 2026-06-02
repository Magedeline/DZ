// Everest mods typically expose the 'On' hooks via this namespace

namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/Maddy_crystal")]
    [Tracked(true)]
    public sealed class MaddyCrystal : Actor {
        private const string SpriteBankId = "madeline_crystal";
        private static ParticleType pImpact;
        public Vector2 Speed;
        public bool OnPedestal;
        private Holdable hold;
        private Sprite sprite;
        private bool dead;
        private Level level;
        private Collision onCollideH;
        private Collision onCollideV;
        private float noGravityTimer;
        private Vector2 prevLiftSpeed;
        private Vector2 previousPosition;
        private HoldableCollider hitSeeker;
        private float swatTimer;
        private bool shattering;
        private float hardVerticalHitSoundCooldown;
        private BirdTutorialGui tutorialGui;
        private float tutorialTimer;

        public MaddyCrystal(EntityData data, Vector2 offset)
          : this(data.Position + offset) {
        }

        public MaddyCrystal(Vector2 position)
          : base(position) {
            Initialize(position);
        }

        public MaddyCrystal(IEnumerable<Component> components) : base(Vector2.One) {
            Initialize(Vector2.One);

            foreach (var component in components) Add(component);
        }

        private void Initialize(Vector2 position) {
            previousPosition = position;
            Depth = 100;
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = CreateSprite());
            sprite.Scale.X = -1f;
            Add(hold = new Holdable());
            hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
            hold.SlowFall = false;
            hold.SlowRun = true;
            hold.OnPickup = OnPickup;
            hold.OnRelease = OnRelease;
            hold.DangerousCheck = dangerous;
            hold.OnHitSeeker = hitSeeker1;
            hold.OnSwat = swat;
            hold.OnHitSpring = hitSpring;
            hold.OnHitSpinner = hitSpinner;
            hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
            Tag = (int)Tags.TransitionUpdate;
            Add(new MirrorReflection());
        }

        private static Sprite CreateSprite() {
            if (!GFX.SpriteBank.Has(SpriteBankId)) {
                throw new Exception($"Missing sprite bank entry '{SpriteBankId}' for {nameof(MaddyCrystal)}.");
            }

            return GFX.SpriteBank.Create(SpriteBankId);
        }

        private static void EnsureImpactParticle() {
            if (pImpact != null) {
                return;
            }

            pImpact = new ParticleType {
                Color = Color.White,
                Color2 = Color.AliceBlue,
                ColorMode = ParticleType.ColorModes.Choose,
                LifeMin = 0.5f,
                LifeMax = 1.0f,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = 0.5f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                FadeMode = ParticleType.FadeModes.Late,
                Acceleration = new Vector2(0f, 40f)
            };
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            this.level = this.SceneAs<Level>();
            foreach (var entity1 in this.level.Tracker.GetEntities<MaddyCrystal>()) {
                var entity = (MaddyCrystal)entity1;
                if (entity != this && entity.hold.IsHeld)
                    this.RemoveSelf();
            }

            if (this.level.Session.Level != "e-00")
                return;
            this.tutorialGui = new BirdTutorialGui((Entity)this, new Vector2(0.0f, -24f),
              (object)Dialog.Clean("tutorial_carry"), new object[2]
              {
        (object)Dialog.Clean("tutorial_hold"),
        (object)BirdTutorialGui.ButtonPrompt.Grab
              });
            this.tutorialGui.Open = false;
            this.Scene.Add((Entity)this.tutorialGui);
        }

        public override void Update() {
            base.Update();
            if (this.shattering || this.dead)
                return;
            if ((double)this.swatTimer > 0.0)
                this.swatTimer -= Engine.DeltaTime;
            this.hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (this.OnPedestal) {
                this.Depth = 8999;
            } else {
                this.Depth = 100;
                if (this.hold.IsHeld) {
                    this.prevLiftSpeed = Vector2.Zero;
                } else {
                    if (this.OnGround()) {
                        this.Speed.X = Calc.Approach(this.Speed.X,
                          this.OnGround(this.Position + Vector2.UnitX * 3f)
                            ? (this.OnGround(this.Position - Vector2.UnitX * 3f) ? 0.0f : -20f)
                            : 20f, 800f * Engine.DeltaTime);
                        Vector2 liftSpeed = this.LiftSpeed;
                        if (liftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero) {
                            this.Speed = this.prevLiftSpeed;
                            this.prevLiftSpeed = Vector2.Zero;
                            this.Speed.Y = Math.Min(this.Speed.Y * 0.6f, 0.0f);
                            if ((double)this.Speed.X != 0.0 && (double)this.Speed.Y == 0.0)
                                this.Speed.Y = -60f;
                            if ((double)this.Speed.Y < 0.0)
                                this.noGravityTimer = 0.15f;
                        } else {
                            this.prevLiftSpeed = liftSpeed;
                            if ((double)liftSpeed.Y < 0.0 && (double)this.Speed.Y < 0.0)
                                this.Speed.Y = 0.0f;
                        }
                    } else if (this.hold.ShouldHaveGravity) {
                        float num1 = 800f;
                        if ((double)Math.Abs(this.Speed.Y) <= 30.0)
                            num1 *= 0.5f;
                        float num2 = 350f;
                        if ((double)this.Speed.Y < 0.0)
                            num2 *= 0.5f;
                        this.Speed.X = Calc.Approach(this.Speed.X, 0.0f, num2 * Engine.DeltaTime);
                        if ((double)this.noGravityTimer > 0.0)
                            this.noGravityTimer -= Engine.DeltaTime;
                        else
                            this.Speed.Y = Calc.Approach(this.Speed.Y, 200f, num1 * Engine.DeltaTime);
                    }

                    this.previousPosition = this.ExactPosition;
                    this.MoveH(this.Speed.X * Engine.DeltaTime, this.onCollideH);
                    this.MoveV(this.Speed.Y * Engine.DeltaTime, this.onCollideV);
                    if ((double)this.Center.X > (double)this.level.Bounds.Right) {
                        this.MoveH(32f * Engine.DeltaTime);
                        if ((double)this.Left - 8.0 > (double)this.level.Bounds.Right)
                            this.RemoveSelf();
                    } else if ((double)this.Left < (double)this.level.Bounds.Left) {
                        this.Left = (float)this.level.Bounds.Left;
                        this.Speed.X *= -0.4f;
                    } else if ((double)this.Top < (double)(this.level.Bounds.Top - 4)) {
                        this.Top = (float)(this.level.Bounds.Top + 4);
                        this.Speed.Y = 0.0f;
                    } else if ((double)this.Bottom > (double)this.level.Bounds.Bottom && SaveData.Instance.Assists.Invincible) {
                        this.Bottom = (float)this.level.Bounds.Bottom;
                        this.Speed.Y = -300f;
                        Audio.Play("event:/game/general/assist_screenbottom", this.Position);
                    } else if ((double)this.Top > (double)this.level.Bounds.Bottom)
                        this.die();

                    if ((double)this.X < (double)(this.level.Bounds.Left + 10))
                        this.MoveH(32f * Engine.DeltaTime);
                    global::Celeste.Player entity = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
                    TempleGate templeGate = this.CollideFirst<TempleGate>();
                    if (templeGate != null && entity != null) {
                        templeGate.Collidable = false;
                        this.MoveH((float)(Math.Sign(entity.X - this.X) * 32) * Engine.DeltaTime);
                        templeGate.Collidable = true;
                    }
                }

                if (!this.dead)
                    this.hold.CheckAgainstColliders();
                if (this.hitSeeker != null && (double)this.swatTimer <= 0.0 && !this.hitSeeker.Check(this.hold))
                    this.hitSeeker = (HoldableCollider)null;
                if (this.tutorialGui == null)
                    return;
                if (!this.OnPedestal && !this.hold.IsHeld && this.OnGround() && this.level.Session.GetFlag("foundMaddyInCrystal"))
                    this.tutorialTimer += Engine.DeltaTime;
                else
                    this.tutorialTimer = 0.0f;
                this.tutorialGui.Open = (double)this.tutorialTimer > 0.25;
            }
        }

        private IEnumerator shatter() {
            MaddyCrystal theoCrystal = this;
            theoCrystal.shattering = true;
            BloomPoint bloom = new BloomPoint(0.0f, 32f);
            VertexLight light = new VertexLight(Color.AliceBlue, 0.0f, 64, 200);
            theoCrystal.Add((Component)bloom);
            theoCrystal.Add((Component)light);
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime) {
                theoCrystal.Position = theoCrystal.Position + theoCrystal.Speed * (1f - p) * Engine.DeltaTime;
                theoCrystal.level.ZoomFocusPoint = theoCrystal.TopCenter - theoCrystal.level.Camera.Position;
                light.Alpha = p;
                bloom.Alpha = p;
                yield return (object)null;
            }

            yield return (object)0.5f;
            theoCrystal.level.Shake();
            theoCrystal.sprite.Play(nameof(shatter));
            yield return (object)1f;
            theoCrystal.level.Shake();
        }

        public void ExplodeLaunch(Vector2 from) {
            if (this.hold.IsHeld)
                return;
            this.Speed = (this.Center - from).SafeNormalize(120f);
            SlashFx.Burst(this.Center, this.Speed.Angle());
        }

        private void swat(HoldableCollider hc, int dir) {
            if (!this.hold.IsHeld || this.hitSeeker != null)
                return;
            this.swatTimer = 0.1f;
            this.hitSeeker = hc;
            this.hold.Holder.Swat(dir);
        }

        private bool dangerous(HoldableCollider holdableCollider) {
            return !this.hold.IsHeld && this.Speed != Vector2.Zero && this.hitSeeker != holdableCollider;
        }

        private void hitSeeker1(Seeker seeker) {
            if (!this.hold.IsHeld)
                this.Speed = (this.Center - seeker.Center).SafeNormalize(120f);
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
        }

        private void hitSpinner(Entity spinner) {
            if (this.hold.IsHeld || (double)this.Speed.Length() >= 0.0099999997764825821)
                return;
            Vector2 vector2 = this.LiftSpeed;
            if ((double)vector2.Length() >= 0.0099999997764825821)
                return;
            vector2 = this.previousPosition - this.ExactPosition;
            if ((double)vector2.Length() >= 0.0099999997764825821 || !this.OnGround())
                return;
            int num = Math.Sign(this.X - spinner.X);
            if (num == 0)
                num = 1;
            this.Speed.X = (float)num * 120f;
            this.Speed.Y = -30f;
        }

        private bool hitSpring(Spring spring) {
            if (!this.hold.IsHeld) {
                if (spring.Orientation == Spring.Orientations.Floor && (double)this.Speed.Y >= 0.0) {
                    this.Speed.X *= 0.5f;
                    this.Speed.Y = -160f;
                    this.noGravityTimer = 0.15f;
                    return true;
                }

                if (spring.Orientation == Spring.Orientations.WallLeft && (double)this.Speed.X <= 0.0) {
                    this.MoveTowardsY(spring.CenterY + 5f, 4f);
                    this.Speed.X = 220f;
                    this.Speed.Y = -80f;
                    this.noGravityTimer = 0.1f;
                    return true;
                }

                if (spring.Orientation == Spring.Orientations.WallRight && (double)this.Speed.X >= 0.0) {
                    this.MoveTowardsY(spring.CenterY + 5f, 4f);
                    this.Speed.X = -220f;
                    this.Speed.Y = -80f;
                    this.noGravityTimer = 0.1f;
                    return true;
                }
            }

            return false;
        }

        private void OnCollideH(CollisionData data) {
            if (data.Hit is DashSwitch) {
                int num = (int)(data.Hit as DashSwitch).OnDashCollide((global::Celeste.Player)null,
                  Vector2.UnitX * (float)Math.Sign(this.Speed.X));
            }

            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
            if ((double)Math.Abs(this.Speed.X) > 100.0)
                this.impactParticles(data.Direction);
            this.Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data) {
            if (data.Hit is DashSwitch) {
                int num = (int)(data.Hit as DashSwitch).OnDashCollide((global::Celeste.Player)null,
                  Vector2.UnitY * (float)Math.Sign(this.Speed.Y));
            }

            if ((double)this.Speed.Y > 0.0) {
                if ((double)this.hardVerticalHitSoundCooldown <= 0.0) {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity",
                      Calc.ClampedMap(this.Speed.Y, 0.0f, 200f));
                    this.hardVerticalHitSoundCooldown = 0.5f;
                } else
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", 0.0f);
            }

            if ((double)this.Speed.Y > 160.0)
                this.impactParticles(data.Direction);
            if ((double)this.Speed.Y > 140.0 && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
                this.Speed.Y *= -0.6f;
            else
                this.Speed.Y = 0.0f;
        }

        private void impactParticles(Vector2 dir) {
            EnsureImpactParticle();
            if (this.level?.Particles == null) {
                return;
            }

            float direction;
            Vector2 position;
            Vector2 positionRange;
            if ((double)dir.X > 0.0) {
                direction = 3.14159274f;
                position = new Vector2(this.Right, this.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if ((double)dir.X < 0.0) {
                direction = 0.0f;
                position = new Vector2(this.Left, this.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if ((double)dir.Y > 0.0) {
                direction = -1.57079637f;
                position = new Vector2(this.X, this.Bottom);
                positionRange = Vector2.UnitX * 6f;
            } else {
                direction = 1.57079637f;
                position = new Vector2(this.X, this.Top);
                positionRange = Vector2.UnitX * 6f;
            }

            this.level.Particles.Emit(MaddyCrystal.pImpact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid) {
            return (double)this.Speed.Y == 0.0 && base.IsRiding(solid);
        }

        public override void OnSquish(CollisionData data)
        {
            if (this.TrySquishWiggle(data) || SaveData.Instance.Assists.Invincible) {
                return;
            }

            this.die();
        }

        private void OnPickup() {
            this.Speed = Vector2.Zero;
            this.AddTag((int)Tags.Persistent);
        }

        private void OnRelease(Vector2 force) {
            this.RemoveTag((int)Tags.Persistent);
            if ((double)force.X != 0.0 && (double)force.Y == 0.0)
                force.Y = -0.4f;
            this.Speed = force * 200f;
            if (!(this.Speed != Vector2.Zero))
                return;
            this.noGravityTimer = 0.1f;
        }

        private void die() {
            if (this.dead)
                return;
            this.dead = true;
            global::Celeste.Player entity = this.level.Tracker.GetEntity<global::Celeste.Player>();
            entity?.Die(-Vector2.UnitX * (float)entity.Facing);
            Audio.Play("event:/pusheen/char/kirby/death", this.Position);
            this.Add((Component)new DeathEffect(Color.ForestGreen, new Vector2?(this.Center - this.Position)));
            this.sprite.Visible = false;
            this.Depth = -1000000;
            this.AllowPushing = false;
        }

        public static void Load() {
            EnsureImpactParticle();
        }

        /// <summary>
        /// Handles the horizontal collision event for the player, invoking the original collision logic
        /// and additionally triggering the <see cref="MaddyCrystal.OnCollideH"/> method if the player is holding a <see cref="MaddyCrystal"/>.
        /// </summary>
        /// <param name="orig">The original OnCollideH delegate to invoke the base collision logic.</param>
        /// <param name="player">The player instance involved in the collision.</param>
        /// <param name="data">The collision data describing the collision event.</param>
        public static void OnPlayerOnCollideH(Action<global::Celeste.Player, CollisionData> orig, global::Celeste.Player player, CollisionData data) {
            orig(player, data);
            if (player?.Holding?.Entity is MaddyCrystal maddyCrystal && maddyCrystal != null) {
                maddyCrystal.OnCollideH(data);
            }
        }

        public static void OnPlayerOnCollideV(Action<global::Celeste.Player, CollisionData> orig, global::Celeste.Player player, CollisionData data) {
            orig(player, data);
            if (player?.Holding?.Entity is MaddyCrystal maddyCrystal && maddyCrystal != null) {
                maddyCrystal.OnCollideV(data);
            }
        }
    }
}
