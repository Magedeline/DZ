#nullable disable
using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity("DZ/DefensivePowerGenerator")]
    [Tracked]
    public class DefensivePowerGenerator : Solid
    {
        private Sprite sprite;
        private Sprite turretSprite;
        private Sprite beamSprite;
        private SineWave sine;
        private Vector2 start;
        private float sink;
        private int health = 10;
        private bool flag;
        private float shakeCounter;
        private bool makeSparks;
        private bool smashParticles;
        private Shaker shaker;
        private Vector2 bounceDir;
        private Wiggler bounce;
        private SoundSource firstHitSfx;
        private bool spikesLeft;
        private bool spikesRight;
        private bool spikesUp;
        private bool spikesDown;

        // --- Defensive weapon state ---
        private float rotationAngle;
        private float rotationSpeed = 1.5f;
        private float laserTimer;
        private float laserInterval = 2.5f;
        private float laserCharge;
        private const float LASER_CHARGE_TIME = 0.6f;
        private const float LASER_ACTIVE_TIME = 0.4f;
        private float laserActiveTimer;
        private Vector2 laserAimDir;
        private bool laserFiring;
        private bool beamEnabled = true;
        private float beamRadius = 48f;
        private float beamHitCooldown;

        public DefensivePowerGenerator(Vector2 position, bool flipX)
            : base(position, 32f, 32f, true)
        {
            this.SurfaceSoundIndex = 9;
            this.start = this.Position;
            this.sprite = GFX.SpriteBank.Create("defensive_power_generator");
            this.sprite.OnLastFrame += (Action<string>)(anim =>
            {
                if (anim == "break")
                    this.Visible = false;
                else if (anim == "open")
                    this.makeSparks = true;
            });
            this.sprite.Position = new Vector2(this.Width, this.Height) / 2f;
            this.sprite.FlipX = flipX;
            this.Add((Component)this.sprite);

            this.turretSprite = GFX.SpriteBank.Create("defensive_power_generator_turret");
            this.turretSprite.Position = new Vector2(this.Width, this.Height) / 2f;
            this.turretSprite.Play("turret");
            this.Add((Component)this.turretSprite);

            this.beamSprite = GFX.SpriteBank.Create("defensive_power_generator_beam");
            this.beamSprite.Position = new Vector2(this.Width, this.Height) / 2f;
            this.beamSprite.Play("beam");
            this.beamSprite.Visible = beamEnabled;
            this.Add((Component)this.beamSprite);
            this.sine = new SineWave(0.5f);
            this.Add((Component)this.sine);
            this.bounce = Wiggler.Create(1f, 0.5f);
            this.bounce.StartZero = false;
            this.Add((Component)this.bounce);
            this.Add((Component)(this.shaker = new Shaker(false)));
            this.OnDashCollide = new DashCollision(this.Dashed);
        }

        public DefensivePowerGenerator(EntityData e, Vector2 levelOffset)
            : this(e.Position + levelOffset, e.Bool("flipX"))
        {
            this.flag = e.Bool(nameof(flag));
            this.beamEnabled = e.Bool("beamEnabled", true);
            this.laserInterval = Math.Max(0.5f, e.Float("laserInterval", 2.5f));
            this.rotationSpeed = e.Float("rotationSpeed", 1.5f);
            this.beamRadius = Math.Max(24f, e.Float("beamRadius", 48f));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.spikesUp = this.CollideCheck<Spikes>(this.Position - Vector2.UnitY);
            this.spikesDown = this.CollideCheck<Spikes>(this.Position + Vector2.UnitY);
            this.spikesLeft = this.CollideCheck<Spikes>(this.Position - Vector2.UnitX);
            this.spikesRight = this.CollideCheck<Spikes>(this.Position + Vector2.UnitX);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!this.flag || !((this.Scene as Level).Session.GetFlag("disable_lightning")))
                return;
            this.RemoveSelf();
        }

        public DashCollisionResults Dashed(global::Celeste.Player player, Vector2 dir)
        {
            if (!SaveData.Instance.Assists.Invincible && (dir == Vector2.UnitX && this.spikesLeft || dir == -Vector2.UnitX && this.spikesRight || dir == Vector2.UnitY && this.spikesUp || dir == -Vector2.UnitY && this.spikesDown))
                return DashCollisionResults.NormalCollision;

            (this.Scene as Level).DirectionalShake(dir);
            this.sprite.Scale = new Vector2((float)(1.0 + (double)Math.Abs(dir.Y) * 0.4 - (double)Math.Abs(dir.X) * 0.4), (float)(1.0 + (double)Math.Abs(dir.X) * 0.4 - (double)Math.Abs(dir.Y) * 0.4));
            --this.health;
            if (this.health > 0)
            {
                if (this.firstHitSfx == null)
                {
                    this.firstHitSfx = new SoundSource("event:/DZ/new_content/game/19_spaces/powergenerator_hit_first");
                    this.Add((Component)this.firstHitSfx);
                }
                else
                {
                    Audio.Play("event:/DZ/new_content/game/19_spaces/powergenerator_hit", this.Position);
                }
                CelesteGame.Freeze(0.1f);
                this.shakeCounter = 0.2f;
                this.shaker.On = true;
                this.bounceDir = dir;
                this.bounce.Start();
                this.smashParticles = true;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
            {
                if (this.firstHitSfx != null)
                    this.firstHitSfx.Stop();
                Audio.Play("event:/DZ/new_content/game/19_spaces/powergenerator_hit", this.Position);
                CelesteGame.Freeze(0.2f);
                player.RefillDash();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                this.smashParticles1(dir.Perpendicular());
                this.smashParticles1(-dir.Perpendicular());
                this.Break();
            }
            return DashCollisionResults.Rebound;
        }

        private void Break()
        {
            this.Tag = (int)Tags.Persistent;
            this.shakeCounter = 0f;
            this.shaker.On = false;
            this.sprite.Play("break");
            this.Collidable = false;
            this.DestroyStaticMovers();

            if (this.Scene != null)
            {
                this.Scene.Add(new LightningStrike(this.Position, 0, 0f, 1f));
                Audio.Play("event:/new_content/game/10_farewell/lightning_strike", this.Position);
                this.Scene.Add(new Flash(this.Center, Color.Red, 0.5f, 64f));
                this.Scene.Add(new GlitchEffect(this.Center, 1.5f));

                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(Calc.Random.Range(-32f, 32f), Calc.Random.Range(-32f, 32f));
                    this.Scene.Add(new LightningStrike(this.Position + offset, Calc.Random.Range(0, 3), i * 0.1f, 0.8f));
                }

                Level fxLevel = this.SceneAs<Level>();
                if (fxLevel != null)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 sparkDir = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(50f, 120f));
                        fxLevel.ParticlesFG.Emit(LightningBreakerBox.P_Sparks, this.Center + sparkDir * 0.1f, sparkDir.Angle());
                    }
                    fxLevel.Shake(0.8f);
                    fxLevel.Displacement.AddBurst(this.Center, 0.8f, 16f, 128f);
                }
            }

            var sfx = Audio.Play("event:/DZ/new_content/game/19_spaces/powergenerator_hit_break", this.Position);
            Audio.SetParameter(sfx, "pitch", 1.2f);

            if (this.flag)
                (this.Scene as Level).Session.SetFlag("disable_lightning");

            this.Add((Component)new Coroutine(Lightning.RemoveRoutine(this.SceneAs<Level>(), new Action(((Entity)this).RemoveSelf))));
            this.RemoveSelf();
        }

        private void smashParticles1(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (dir == Vector2.UnitX)
            {
                direction = 0f;
                position = this.CenterRight - Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
                num = (int)(this.Height / 8f) * 4;
            }
            else if (dir == -Vector2.UnitX)
            {
                direction = MathHelper.Pi;
                position = this.CenterLeft + Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
                num = (int)(this.Height / 8f) * 4;
            }
            else if (dir == Vector2.UnitY)
            {
                direction = MathHelper.PiOver2;
                position = this.BottomCenter - Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
                num = (int)(this.Width / 8f) * 4;
            }
            else
            {
                direction = -MathHelper.PiOver2;
                position = this.TopCenter + Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
                num = (int)(this.Width / 8f) * 4;
            }
            this.SceneAs<Level>().Particles.Emit(LightningBreakerBox.P_Smash, num + 2, position, positionRange, direction);
        }

        public override void Update()
        {
            base.Update();
            if (this.makeSparks && this.Scene.OnInterval(0.03f))
                this.SceneAs<Level>().ParticlesFG.Emit(LightningBreakerBox.P_Sparks, 1, this.Center, Vector2.One * 12f);

            if (this.shakeCounter > 0f)
            {
                this.shakeCounter -= Engine.DeltaTime;
                if (this.shakeCounter <= 0f)
                {
                    this.shaker.On = false;
                    this.sprite.Scale = Vector2.One * 1.2f;
                    this.sprite.Play("open");
                }
            }

            if (this.Collidable)
            {
                this.sink = Calc.Approach(this.sink, this.HasPlayerRider() ? 1f : 0f, 2f * Engine.DeltaTime);
                this.sine.Rate = MathHelper.Lerp(1f, 0.5f, this.sink);
                Vector2 start = this.start;
                start.Y += (float)(this.sink * 6.0 + this.sine.Value * MathHelper.Lerp(4f, 2f, this.sink));
                Vector2 vector2 = start + this.bounce.Value * this.bounceDir * 12f;
                this.MoveToX(vector2.X);
                this.MoveToY(vector2.Y);
                if (this.smashParticles)
                {
                    this.smashParticles = false;
                    this.smashParticles1(this.bounceDir.Perpendicular());
                    this.smashParticles1(-this.bounceDir.Perpendicular());
                }

                // Defensive weapons only active while intact
                UpdateDefensiveWeapons();
            }

            this.sprite.Scale.X = Calc.Approach(this.sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
            this.sprite.Scale.Y = Calc.Approach(this.sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
            this.LiftSpeed = Vector2.Zero;
        }

        private void UpdateDefensiveWeapons()
        {
            rotationAngle += rotationSpeed * Engine.DeltaTime;
            if (rotationAngle > MathHelper.TwoPi)
                rotationAngle -= MathHelper.TwoPi;

            if (beamHitCooldown > 0f)
                beamHitCooldown -= Engine.DeltaTime;

            // Sync visual beam rotation
            if (beamSprite != null)
            {
                beamSprite.Rotation = rotationAngle;
                beamSprite.Visible = beamEnabled;
            }

            // Turret tracks the player
            global::Celeste.Player targetPlayer = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (turretSprite != null && targetPlayer != null && !targetPlayer.Dead)
            {
                Vector2 toPlayer = (targetPlayer.Center - this.Center).SafeNormalize();
                turretSprite.Rotation = toPlayer.Angle();
            }

            // Rotating beam
            if (beamEnabled)
            {
                CheckBeamHit();
            }

            // Aimed laser
            laserTimer += Engine.DeltaTime;
            if (!laserFiring)
            {
                if (laserTimer >= laserInterval)
                {
                    global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                    if (player != null && !player.Dead)
                    {
                        laserAimDir = (player.Center - this.Center).SafeNormalize();
                        laserCharge = LASER_CHARGE_TIME;
                        laserFiring = true;
                        laserTimer = 0f;
                        Audio.Play("event:/DZ/new_content/game/21_finale/charge", this.Position);
                    }
                    else
                    {
                        laserTimer = 0f;
                    }
                }
            }
            else
            {
                if (laserCharge > 0f)
                {
                    laserCharge -= Engine.DeltaTime;
                    if (laserCharge <= 0f)
                    {
                        laserActiveTimer = LASER_ACTIVE_TIME;
                        CheckLaserHit();
                        Audio.Play("event:/DZ/new_content/game/21_finale/fire", this.Position);
                    }
                }
                else if (laserActiveTimer > 0f)
                {
                    laserActiveTimer -= Engine.DeltaTime;
                    CheckLaserHit();
                    if (laserActiveTimer <= 0f)
                    {
                        laserFiring = false;
                        laserTimer = 0f;
                    }
                }
                else
                {
                    laserFiring = false;
                    laserTimer = 0f;
                }
            }
        }

        private void CheckBeamHit()
        {
            if (beamHitCooldown > 0f)
                return;

            Vector2 dir = Calc.AngleToVector(rotationAngle, 1f);
            Vector2 beamStart = this.Center + dir * 18f;
            Vector2 beamEnd = this.Center + dir * beamRadius;

            global::Celeste.Player player = Scene.CollideFirst<global::Celeste.Player>(beamStart, beamEnd);
            if (player != null && !player.Dead)
            {
                player.Die((player.Center - this.Center).SafeNormalize());
                beamHitCooldown = 0.5f;
            }
        }

        private void CheckLaserHit()
        {
            if (laserAimDir == Vector2.Zero)
                return;

            Vector2 laserStart = this.Center + laserAimDir * 18f;
            Vector2 laserEnd = this.Center + laserAimDir * 2000f;

            global::Celeste.Player player = Scene.CollideFirst<global::Celeste.Player>(laserStart, laserEnd);
            if (player != null && !player.Dead)
            {
                player.Die((player.Center - this.Center).SafeNormalize());
            }
        }

        public override void Render()
        {
            Vector2 position = this.sprite.Position;
            this.sprite.Position = this.sprite.Position + this.shaker.Value;
            base.Render();
            this.sprite.Position = position;

            // Draw rotating defensive beam
            if (beamEnabled && Collidable)
            {
                Vector2 dir = Calc.AngleToVector(rotationAngle, 1f);
                Vector2 beamStart = this.Center + dir * 18f;
                Vector2 beamEnd = this.Center + dir * beamRadius;
                Draw.Line(beamStart, beamEnd, Color.Cyan * 0.85f, 3f);
                Draw.Line(beamStart, beamEnd, Color.White * 0.5f, 1f);
            }

            // Draw aimed laser
            if (laserFiring)
            {
                if (laserCharge > 0f)
                {
                    // Charging indicator
                    float chargeProgress = 1f - (laserCharge / LASER_CHARGE_TIME);
                    Vector2 previewEnd = this.Center + laserAimDir * MathHelper.Lerp(16f, 200f, chargeProgress);
                    Draw.Line(this.Center + laserAimDir * 16f, previewEnd, Color.Red * (0.3f + chargeProgress * 0.4f), 1f);
                }
                else if (laserActiveTimer > 0f)
                {
                    Vector2 laserStart = this.Center + laserAimDir * 16f;
                    Vector2 laserEnd = this.Center + laserAimDir * 2000f;
                    Draw.Line(laserStart, laserEnd, Color.Red * 0.9f, 5f);
                    Draw.Line(laserStart, laserEnd, Color.White * 0.7f, 2f);
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }

        public static void Load()
        {
            On.Celeste.Player.DashUpdate += OnPlayerDashUpdate;
        }

        public static void Unload()
        {
            On.Celeste.Player.DashUpdate -= OnPlayerDashUpdate;
        }

        private static int OnPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, global::Celeste.Player self)
        {
            int result = orig(self);

            if (self.Scene is Level level)
            {
                foreach (DefensivePowerGenerator generator in level.Tracker.GetEntities<DefensivePowerGenerator>())
                {
                    if (self.CollideCheck(generator))
                    {
                        generator.Dashed(self, self.DashDir);
                    }
                }
            }

            return result;
        }
    }
}
