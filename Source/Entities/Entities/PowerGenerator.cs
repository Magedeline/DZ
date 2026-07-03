#nullable disable
namespace Celeste.Entities
{
    [CustomEntity(new string[] { "DZ/PowerGenerator" })]
    [Tracked]
    public class PowerGenerator : Solid
    {
        public static ParticleType PSmash;
        public static ParticleType PSparks;
        private Sprite sprite;
        private SineWave sine;
        private Vector2 start;
        private float sink;
        private int health = 5;
        private bool flag;
        private float shakeCounter;
        private string music;
        private int musicProgress = -1;
        private bool musicStoreInSession;
        private Vector2 bounceDir;
        private Wiggler bounce;
        private Shaker shaker;
        private bool makeSparks;
        private bool smashParticles;
        private Coroutine pulseRoutine;
        private SoundSource firstHitSfx;
        private bool spikesLeft;
        private bool spikesRight;
        private bool spikesUp;
        private bool spikesDown;

        private const string COUNTER_TOTAL = "ch19_power_generators_total";
        private const string COUNTER_BROKEN = "ch19_power_generators_broken";

        // --- ADDED: end parameter ---
        private Vector2 end;

        // --- Teleportation fields ---
        private bool canTeleport = false;
        private bool hasTeleported = false;

        public bool CanTeleport => canTeleport;
        public bool HasTeleported => hasTeleported;

        /// <summary>
        /// Enables or disables teleportation capability for this generator.
        /// </summary>
        public void SetTeleportation(bool enabled)
        {
            canTeleport = enabled;
        }

        /// <summary>
        /// Toggles teleportation capability on/off.
        /// </summary>
        public void ToggleTeleportation()
        {
            canTeleport = !canTeleport;
        }

        /// <summary>
        /// Triggers teleportation immediately if conditions are met.
        /// Can be called by other entities/triggers.
        /// </summary>
        public bool TriggerTeleportation()
        {
            if (canTeleport && !hasTeleported && end != start)
            {
                return TeleportWithFlash();
            }
            return false;
        }

        public PowerGenerator(Vector2 position, bool flipX, Vector2? end = null, bool canTeleport = false)
          : base(position, 32f, 32f, true)
        {
            this.SurfaceSoundIndex = 9;
            this.start = this.Position;
            this.end = end ?? this.Position; // Default to position if not provided
            this.canTeleport = canTeleport;
            this.sprite = DZModule.SpriteBank.Create("powergen");
            this.sprite.OnLastFrame += (Action<string>)(anim =>
            {
                switch (anim)
                {
                    case "break":
                        this.Visible = false;
                        break;
                    case "open":
                        this.makeSparks = true;
                        break;
                }
            });
            this.sprite.Position = new Vector2(this.Width, this.Height) / 2f;
            this.sprite.FlipX = flipX;
            this.Add((Component)this.sprite);
            this.sine = new SineWave(0.5f);
            this.Add((Component)this.sine);
            this.bounce = Wiggler.Create(1f, 0.5f);
            this.bounce.StartZero = false;
            this.Add((Component)this.bounce);
            this.Add((Component)(this.shaker = new Shaker(false)));
            this.OnDashCollide = new DashCollision(this.Dashed);
        }

        public PowerGenerator(EntityData e, Vector2 levelOffset)
          : this(
                e.Position + levelOffset,
                e.Bool("flipX"),
            e.FirstNodeNullable(levelOffset),
                e.Bool("canTeleport", false))
        {
            this.flag = e.Bool(nameof(flag));
            this.music = e.Attr(nameof(music), (string)null);
            this.musicProgress = e.Int("music_progress", -1);
            this.musicStoreInSession = e.Bool("music_session");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.spikesUp = this.CollideCheck<Spikes>(this.Position - Vector2.UnitY);
            this.spikesDown = this.CollideCheck<Spikes>(this.Position + Vector2.UnitY);
            this.spikesLeft = this.CollideCheck<Spikes>(this.Position - Vector2.UnitX);
            this.spikesRight = this.CollideCheck<Spikes>(this.Position + Vector2.UnitX);

            // --- ROOM "j-mid" LOGIC EXAMPLE ---
            // If you want to check if the generator is in room "j-mid":
            if ((scene as Level)?.Session?.Level == "j-mid")
            {
                this.health = 5;

                // Teleport from boss-19fake to center-01
                Player player = (scene as Level)?.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    Add(new Coroutine(TeleportToRoom(player, "j-mid", false)));
                    return;
                }
            }

            // Keep blackhole visuals in sync when entering/re-entering rooms.
            RefreshBlackholeFromGeneratorProgress(scene as Level);
        }

        /// <summary>
        /// Recomputes generator break progression and applies it to all RainbowBlackholeBg backdrops.
        /// </summary>
        public static void RefreshBlackholeFromGeneratorProgress(Level level)
        {
            if (level?.Session == null)
                return;

            int broken = Math.Max(0, level.Session.GetCounter(COUNTER_BROKEN));
            int trackedTotal = Math.Max(0, level.Session.GetCounter(COUNTER_TOTAL));

            int active = 0;
            List<Entity> trackedGenerators = null;
            try
            {
                trackedGenerators = level.Tracker.GetEntities<PowerGenerator>();
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                // If the tracker has no entry for this type yet, treat as no active generators.
            }

            if (trackedGenerators != null)
            {
                foreach (Entity entity in trackedGenerators)
                {
                    if (entity is PowerGenerator generator && generator.Collidable && generator.health > 0)
                        active++;
                }
            }

            int total = Math.Max(trackedTotal, broken + active);
            if (total > 0)
                level.Session.SetCounter(COUNTER_TOTAL, total);

            float progress = total <= 0 ? 0f : MathHelper.Clamp((float)broken / total, 0f, 1f);

            foreach (Backdrop backdrop in level.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg blackholeBg)
                    blackholeBg.SetGeneratorBreakProgress(progress);
            }

            foreach (Backdrop backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg blackholeBg)
                    blackholeBg.SetGeneratorBreakProgress(progress);
            }
        }

        private void RegisterPermanentBreakProgress(Level level)
        {
            if (level?.Session == null)
                return;

            int broken = Math.Max(0, level.Session.GetCounter(COUNTER_BROKEN)) + 1;
            level.Session.SetCounter(COUNTER_BROKEN, broken);

            RefreshBlackholeFromGeneratorProgress(level);
        }

        public DashCollisionResults Dashed(global::Celeste.Player player, Vector2 dir)
        {
            if (!SaveData.Instance.Assists.Invincible && (dir == Vector2.UnitX && this.spikesLeft || dir == -Vector2.UnitX && this.spikesRight || dir == Vector2.UnitY && this.spikesUp || dir == -Vector2.UnitY && this.spikesDown))
                return DashCollisionResults.NormalCollision;

            (this.Scene as Level).DirectionalShake(dir);
            this.sprite.Scale = new Vector2((float)(1.0 + (double)Math.Abs(dir.Y) * 0.40000000596046448 - (double)Math.Abs(dir.X) * 0.40000000596046448), (float)(1.0 + (double)Math.Abs(dir.X) * 0.40000000596046448 - (double)Math.Abs(dir.Y) * 0.40000000596046448));
            --this.health;
            if (this.health > 0)
            {
                // Only add the firstHitSfx if it doesn't already exist
                if (this.firstHitSfx == null)
                {
                    this.firstHitSfx = new SoundSource("event:/pusheen/new_content/game/19_spaces/powergenerator_hit_first");
                    this.Add((Component)this.firstHitSfx);
                }
                else
                {
                    // Play multiple hits sound for subsequent hits
                    Audio.Play("event:/pusheen/new_content/game/19_spaces/powergenerator_hit", this.Position);
                }
                CelesteGame.Freeze(0.1f);
                this.shakeCounter = 0.2f;
                this.shaker.On = true;
                this.bounceDir = dir;
                this.bounce.Start();
                this.smashParticles = true;
                this.pulse();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
            {
                if (this.firstHitSfx != null)
                    this.firstHitSfx.Stop();
                Audio.Play("event:/pusheen/new_content/game/19_spaces/powergenerator_hit", this.Position);
                CelesteGame.Freeze(0.2f);
                player.RefillDash();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                this.smashParticles1(dir.Perpendicular());
                this.smashParticles1(-dir.Perpendicular());

                if (!this.TryTeleportOnDestroyed(dir))
                    this.@break();
            }
            return DashCollisionResults.Rebound;
        }

        private bool TryTeleportOnDestroyed(Vector2 dir)
        {
            if (!this.TeleportWithFlash())
                return false;

            this.health = 5;
            this.Collidable = true;
            this.shakeCounter = 0.2f;
            this.shaker.On = true;
            this.bounceDir = dir;
            this.bounce.Start();
            this.smashParticles = true;
            this.sprite.Play("open");
            this.pulse();
            Audio.Play("event:/game/general/seed_complete", this.Position);
            return true;
        }

        private void smashParticles1(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (dir == Vector2.UnitX)
            {
                direction = 0.0f;
                position = this.CenterRight - Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
                num = (int)((double)this.Height / 8.0) * 4;
            }
            else if (dir == -Vector2.UnitX)
            {
                direction = 3.14159274f;
                position = this.CenterLeft + Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
                num = (int)((double)this.Height / 8.0) * 4;
            }
            else if (dir == Vector2.UnitY)
            {
                direction = 1.57079637f;
                position = this.BottomCenter - Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
                num = (int)((double)this.Width / 8.0) * 4;
            }
            else
            {
                direction = -1.57079637f;
                position = this.TopCenter + Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
                num = (int)((double)this.Width / 8.0) * 4;
            }
            int amount = num + 2;
            this.SceneAs<Level>().Particles.Emit(LightningBreakerBox.P_Smash, amount, position, positionRange, direction);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!this.flag || !(this.Scene as Level).Session.GetFlag("disable_lightning"))
                return;
            this.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (this.makeSparks && this.Scene.OnInterval(0.03f))
                this.SceneAs<Level>().ParticlesFG.Emit(LightningBreakerBox.P_Sparks, 1, this.Center, Vector2.One * 12f);
            if ((double)this.shakeCounter > 0.0)
            {
                this.shakeCounter -= Engine.DeltaTime;
                if ((double)this.shakeCounter <= 0.0)
                {
                    this.shaker.On = false;
                    this.sprite.Scale = Vector2.One * 1.2f;
                    this.sprite.Play("open");
                }
            }
            if (this.Collidable)
            {
                this.sink = Calc.Approach(this.sink, this.HasPlayerRider() ? 1f : 0.0f, 2f * Engine.DeltaTime);
                this.sine.Rate = MathHelper.Lerp(1f, 0.5f, this.sink);
                Vector2 start = this.start;
                start.Y += (float)((double)this.sink * 6.0 + (double)this.sine.Value * (double)MathHelper.Lerp(4f, 2f, this.sink));
                Vector2 vector2 = start + this.bounce.Value * this.bounceDir * 12f;
                this.MoveToX(vector2.X);
                this.MoveToY(vector2.Y);
                if (this.smashParticles)
                {
                    this.smashParticles = false;
                    this.smashParticles1(this.bounceDir.Perpendicular());
                    this.smashParticles1(-this.bounceDir.Perpendicular());
                }
            }
            this.sprite.Scale.X = Calc.Approach(this.sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
            this.sprite.Scale.Y = Calc.Approach(this.sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
            this.LiftSpeed = Vector2.Zero;
        }

        public override void Render()
        {
            Vector2 position = this.sprite.Position;
            Sprite sprite = this.sprite;
            sprite.Position = sprite.Position + this.shaker.Value;
            base.Render();
            this.sprite.Position = position;
        }

        private void pulse()
        {
            // Lightning pulse effect removed - Lightning.PulseRoutine doesn't exist in custom Lightning class
            this.pulseRoutine = new Coroutine(Lightning.PulseRoutine(this.SceneAs<Level>()));
            this.Add((Component)this.pulseRoutine);
        }

        private void @break()
        {
            Session session = (this.Scene as Level)?.Session;
            Level level = this.Scene as Level;

            RegisterPermanentBreakProgress(level);

            RumbleTrigger.ManuallyTrigger(this.Center.X, 1.2f);
            this.Tag = (int)Tags.Persistent;
            this.shakeCounter = 0.0f;
            this.shaker.On = false;
            this.sprite.Play("break");
            this.Collidable = false;
            this.DestroyStaticMovers();

            // --- LIGHTNING EFFECT ---
            // Example: Add a lightning effect entity or play a lightning sound
            if (this.Scene != null)
            {
                // Use the correct constructor for LightningStrike
                // Example: new LightningStrike(position, colorIndex, delay, lifeTime)
                this.Scene.Add(new LightningStrike(this.Position, 0, 0f, 1f));
                Audio.Play("event:/new_content/game/10_farewell/lightning_strike", this.Position);
            }

            // --- STRONG GLITCH EFFECTS ---
            if (this.Scene != null)
            {
                // Screen flash effect
                this.Scene.Add(new Flash(this.Center, Color.Red, 0.5f, 64f));
                
                // Add glitch displacement effect
                this.Scene.Add(new GlitchEffect(this.Center, 1.5f));
                
                // Multiple lightning strikes for dramatic effect
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(Calc.Random.Range(-32f, 32f), Calc.Random.Range(-32f, 32f));
                    this.Scene.Add(new LightningStrike(this.Position + offset, Calc.Random.Range(0, 3), i * 0.1f, 0.8f));
                }
                
                // Particle burst effect
                Level fxLevel = this.SceneAs<Level>();
                if (fxLevel != null)
                {
                    // Electric sparks burst
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 sparkDir = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(50f, 120f));
                        fxLevel.ParticlesFG.Emit(LightningBreakerBox.P_Sparks, this.Center + sparkDir * 0.1f, sparkDir.Angle());
                    }
                    
                    // Screen shake for impact
                    fxLevel.Shake(0.8f);
                    
                    // Distort effect - using correct Celeste distortion
                    fxLevel.Displacement.AddBurst(this.Center, 0.8f, 16f, 128f);
                }
            }

            // --- PITCH PARAMETER ---
            // Example: Play destruction sound with pitch parameter
            var sfx = Audio.Play("event:/pusheen/new_content/game/19_spaces/powergenerator_hit_scream", this.Position);
            Audio.SetParameter(sfx, "pitch", 1.2f); // Set pitch as needed

            if (this.flag)
                session.SetFlag("disable_lightning");
            if (this.musicStoreInSession)
            {
                if (!string.IsNullOrEmpty(this.music))
                    session.Audio.Music.Event = SFX.EventnameByHandle(this.music);
                if (this.musicProgress >= 0)
                    session.Audio.Music.SetProgress(this.musicProgress);
                session.Audio.Apply();
            }
            else
            {
                if (!string.IsNullOrEmpty(this.music))
                    Audio.SetMusic(SFX.EventnameByHandle(this.music), false);
                if (this.musicProgress >= 0)
                    Audio.SetMusicParam("progress", (float)this.musicProgress);
                if (!string.IsNullOrEmpty(this.music) && Audio.CurrentMusicEventInstance.isValid())
                {
                    int num = (int)Audio.CurrentMusicEventInstance.start();
                }
            }
                if (this.pulseRoutine != null)
                this.pulseRoutine.Active = false;
            // Lightning.RemoveRoutine doesn't exist in custom Lightning class - just remove self directly
            this.Add((Component)new Coroutine(Lightning.RemoveRoutine(this.SceneAs<Level>(), new Action(((Entity)this).RemoveSelf))));
            this.RemoveSelf();
        }

        /// <summary>
        /// Teleports the PowerGenerator to its 'end' position with a flash effect.
        /// Also teleports any player riding or touching the generator.
        /// Only works if teleportation is enabled and hasn't already been used.
        /// </summary>
        public bool TeleportWithFlash()
        {
            if (!canTeleport || hasTeleported || end == start)
                return false;

            // Flash effect at current position
            if (this.Scene != null)
            {
                this.Scene.Add(new Flash(this.Center, Color.Cyan, 0.3f, 48f));
                // Add flash at destination too
                this.Scene.Add(new Flash(end + new Vector2(16f, 16f), Color.White, 0.25f, 32f));
            }

            // Store old position for effect
            Vector2 oldPos = this.Position;

            // Calculate the offset for player teleportation
            Vector2 teleportOffset = end - this.Position;

            // Move to the 'end' position
            this.Position = end;
            this.start = end;

            // Teleport player if they're riding or touching this generator
            Level level = this.SceneAs<Level>();
            if (level != null)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null && (HasPlayerRider() || player.CollideCheck(this)))
                {
                    // Teleport player by the same offset
                    player.Position += teleportOffset;

                    // Add player flash effect
                    level.Add(new Flash(player.Center, Color.Cyan, 0.2f, 24f));
                }
            }

            // Play teleport sound
            Audio.Play("event:/game/general/seed_touch", this.Position);
            Audio.Play("event:/game/general/seed_poof", oldPos);

            // Mark as teleported (can only teleport once)
            hasTeleported = true;

            // Add particle effects
            if (this.Scene != null)
            {
                if (level != null)
                {
                    // Particle burst at old position
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 sparkDir = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(30f, 80f));
                        level.ParticlesFG.Emit(LightningBreakerBox.P_Sparks, oldPos + new Vector2(16f, 16f) + sparkDir * 0.1f, sparkDir.Angle());
                    }

                    // Particle burst at new position
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 sparkDir = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(20f, 60f));
                        level.ParticlesFG.Emit(LightningBreakerBox.P_Sparks, this.Center + sparkDir * 0.1f, sparkDir.Angle());
                    }
                }
            }

            return true;
        }

        private IEnumerator TeleportToRoom(Player player, string targetRoom, bool lastHit)
        {
            Level level = Scene as Level;
            if (level == null || player == null)
                yield break;

            // Attract player to center
            if (!player.Dead)
                player.StartAttract(Center + Vector2.UnitY * 4f);

            float timer = 0.15f;
            while (!player.Dead && !player.AtAttractTarget)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            if (timer > 0f)
                yield return timer;

            // Screen effects
            CelesteGame.Freeze(0.1f);
            level.Shake();
            yield return 0.05f;

            yield return 0.2f;

            // Perform the room transition
            level.OnEndOfFrame += () =>
            {
                Vector2 levelOffset = level.LevelOffset;
                Facings facing = player.Facing;

                level.Remove(player);
                level.UnloadLevel();

                level.Session.Level = targetRoom;
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.Session.FirstLevel = false;

                level.LoadLevel(Player.IntroTypes.Transition);

                // Calculate center position of the new room
                Vector2 roomCenter = new Vector2(
                    level.Bounds.Left + level.Bounds.Width / 2f,
                    level.Bounds.Top + level.Bounds.Height / 2f
                );

                level.Add(player);
                player.Position = roomCenter;
                player.Facing = facing;
                player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                // Center camera on player
                level.Camera.Position = new Vector2(
                    roomCenter.X - level.Camera.Viewport.Width / 2f,
                    roomCenter.Y - level.Camera.Viewport.Height / 2f
                );

                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
                level.Flash(Color.White);
                level.Shake();

                // Play teleport sound
                Audio.Play("event:/game/06_reflection/badeline_disappear");
            };
        }

        public static void Load()
        {
            // Fix for CS0407: Use the correct delegate signature for DashUpdate
            On.Celeste.Player.DashUpdate += OnPlayerDashUpdate;
        }

        public static void Unload()
        {
            Logger.Log(nameof(PowerGenerator), "Unloading PowerGenerator...");
            // Clean up by removing the event hook we added in Load
            On.Celeste.Player.DashUpdate -= OnPlayerDashUpdate;
        }

        // New method to handle the dash collision check
        private static int OnPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, global::Celeste.Player self)
        {
            // Call the original method first
            int result = orig(self);

            // Then perform our custom logic
            if (self.Scene is Level level)
            {
                // Optimization: Use the tracker instead of iterating over all entities
                foreach (PowerGenerator generator in level.Tracker.GetEntities<PowerGenerator>())
                {
                    if (self.CollideCheck(generator))
                    {
                        generator.Dashed(self, self.DashDir);
                    }
                }
            }

            return result; // Ensure the correct return type
        }
    }
    public class Flash : Entity
    {
        private Color color;
        private float duration;
        private float radius;
        private float timer;

        public Flash(Vector2 position, Color color, float duration, float radius)
        {
            Position = position;
            Depth = -10000; // Render above most entities
            this.color = color;
            this.duration = duration;
            this.radius = radius;
            this.timer = 0f;
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            
            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            if (timer < duration)
            {
                var progress = timer / duration;
                var alpha = 1f - progress;
                var currentRadius = radius * progress;
                Draw.Circle(Position, currentRadius, color * alpha, 32);
            }
            
            base.Render();
        }
    }

    public class GlitchEffect : Entity
    {
        private float duration;
        private float timer;
        private Vector2 basePosition;
        private SineWave glitchWave;
        private SineWave colorWave;

        public GlitchEffect(Vector2 position, float duration)
        {
            Position = position;
            basePosition = position;
            this.duration = duration;
            Depth = -9999; // Render above most entities but below flash
            
            glitchWave = new SineWave(Calc.Random.Range(8f, 15f));
            colorWave = new SineWave(Calc.Random.Range(20f, 30f));
            Add(glitchWave);
            Add(colorWave);
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            
            // Random position jitter for glitch effect
            Position = basePosition + new Vector2(
                Calc.Random.Range(-4f, 4f) * (1f - timer / duration),
                Calc.Random.Range(-4f, 4f) * (1f - timer / duration)
            );

            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            if (timer < duration)
            {
                float alpha = 1f - (timer / duration);
                
                // Draw glitchy rectangles with different colors
                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = new Vector2(
                        glitchWave.Value * Calc.Random.Range(-8f, 8f),
                        Calc.Random.Range(-16f, 16f)
                    );
                    
                    Color glitchColor = i switch
                    {
                        0 => Color.Red * alpha,
                        1 => Color.Cyan * alpha,
                        2 => Color.Yellow * alpha,
                        3 => Color.Magenta * alpha,
                        _ => Color.White * alpha
                    };
                    
                    Rectangle rect = new Rectangle(
                        (int)(Position.X + offset.X - 16),
                        (int)(Position.Y + offset.Y - 2),
                        Calc.Random.Range(16, 48),
                        Calc.Random.Range(2, 8)
                    );
                    
                    Draw.Rect(rect, glitchColor);
                }
                
                // Add some scanlines
                for (int y = -32; y < 32; y += 4)
                {
                    Draw.Line(
                        Position + new Vector2(-32, y), 
                        Position + new Vector2(32, y),
                        Color.White * (alpha * 0.3f)
                    );
                }
            }
            
            base.Render();
        }
    }
}




