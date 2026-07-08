using Celeste.Cutscenes;

namespace Celeste.Entities {
    [CustomEntity(ids: "DZ/CharaBoss")]
    [Tracked(true)]
    [HotReloadable]
    public partial class CharaBoss : Entity
    {
        public static ParticleType PBurst;
        
        static CharaBoss()
        {
            // Initialize particle type for burst effect
            PBurst = new ParticleType
            {
                Color = Color.Red,
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                SpeedMultiplier = 0.2f,
                Acceleration = new Vector2(0f, 60f)
            };
        }
        
        public const float CameraXPastMax = 140f;
        private const float MoveSpeed = 600f;
        private const float AvoidRadius = 12f;
        
        public Sprite Sprite;
        public Sprite NormalSprite;
        private PlayerHair normalHair;
        private Vector2 avoidPos;
        public float CameraYPastMax;
        public bool Moving;
        public bool Sitting;
        private int facing;
        private Level level;
        private Monocle.Circle circle;
        private Vector2[] nodes;
        private int nodeIndex;
        private int patternIndex;
        private Coroutine attackCoroutine;
        private Coroutine triggerBlocksCoroutine;
        private List<Entity> fallingBlocks;
        private List<Entity> movingBlocks;
        private bool playerHasMoved;
        private SineWave floatSine;
        private bool dialog;
        private bool startHit;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private CharaBossStarfield bossBg;
        private SoundSource chargeSfx;
        private SoundSource laserSfx;
        private readonly TimeRateModifier timeRateModifier;
        private bool canChangeMusic;
        private string attackSequence;

        private Sprite ActiveVisualSprite => this.Sprite ?? this.NormalSprite;

        public Vector2 BeamOrigin => this.Center + (this.ActiveVisualSprite?.Position ?? Vector2.Zero) + new Vector2(0.0f, -14f);
        public Vector2 ShotOrigin => this.Center + (this.ActiveVisualSprite?.Position ?? Vector2.Zero) + new Vector2(6f * (this.ActiveVisualSprite?.Scale.X ?? (this.facing == 0 ? -1f : (float)this.facing)), 2f);

        public CharaBoss(
            Vector2 position,
            Vector2[] nodes,
            int patternIndex,
            float cameraYPastMax,
            bool dialog,
            bool startHit,
            bool cameraLockY,
            string attackSequence = "")
            : base(position)
        {
            this.patternIndex = patternIndex;
            this.CameraYPastMax = cameraYPastMax;
            this.dialog = dialog;
            this.startHit = startHit;
            this.attackSequence = attackSequence;
            this.Add((Component)(this.light = new VertexLight(Color.White, 1f, 32, 64)));
            this.Collider = (Collider)(this.circle = new Monocle.Circle(14f, y: -6f));
            this.Add((Component)new PlayerCollider(new Action<global::Celeste.Player>(this.OnPlayer)));
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = this.Position;
            for (int index = 0; index < nodes.Length; ++index)
                this.nodes[index + 1] = nodes[index];
            this.attackCoroutine = new Coroutine(false);
            this.Add((Component)this.attackCoroutine);
            this.triggerBlocksCoroutine = new Coroutine(false);
            this.Add((Component)this.triggerBlocksCoroutine);
            this.Add((Component)new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
            this.Add((Component)(this.floatSine = new SineWave(0.6f)));
            this.Add((Component)(this.scaleWiggler = Wiggler.Create(0.6f, 3f)));
            this.Add((Component)(this.chargeSfx = new SoundSource()));
            this.Add((Component)(this.laserSfx = new SoundSource()));
            this.Add((Component)(this.timeRateModifier = new TimeRateModifier(1f, false)));
        }

        public CharaBoss(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.NodesOffset(offset), e.Int(nameof(patternIndex)),
                  e.Float("cameraPastY", 120f), e.Bool(nameof(dialog)), e.Bool(nameof(startHit)),
                  e.Bool("cameraLockY", true), e.Attr(nameof(attackSequence), ""))
        {
            canChangeMusic = e.Bool("canChangeMusic", defaultValue: true);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.level = this.SceneAs<Level>();
            if (this.patternIndex == 0)
            {
                this.NormalSprite = GFX.SpriteBank.Create("chara");
                this.NormalSprite.Scale.X = -1f;
                if (this.NormalSprite.Has("angry"))
                    this.NormalSprite.Play("angry");
                this.Add((Component)this.NormalSprite);
            }
            else
                this.CreateBossSprite();
            
            this.bossBg = this.level.Background.Get<CharaBossStarfield>();
            if (this.patternIndex == 0 && !this.level.Session.GetFlag("chara_boss_intro") && this.level.Session.Level.Equals("boss-00"))
            {
                this.level.Session.Audio.Music.Event = "event:/pusheen/music/lvl2/phone_loop";
                this.level.Session.Audio.Apply();
                if (this.bossBg != null)
                    this.bossBg.Alpha = 0.0f;
                this.Sitting = true;
                this.Position.Y += 16f;
                if (this.NormalSprite.Has("pretendDead"))
                    this.NormalSprite.Play("pretendDead");
                this.NormalSprite.Scale.X = 1f;
            }
            else if (this.patternIndex == 0 && !this.level.Session.GetFlag("DZ_CH8_CHARA_BOSS_MIDDLE") && this.level.Session.Level.Equals("boss-14"))
            {
                this.level.Add((Entity)new CS08_BossMid());
            }
            else if (this.startHit)
                Alarm.Set((Entity)this, 0.5f, (Action)(() => this.OnPlayer((global::Celeste.Player)null)));
            
            this.light.Position = (this.Sprite != null ? (GraphicsComponent)this.Sprite : (GraphicsComponent)this.NormalSprite).Position + new Vector2(0.0f, -10f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.fallingBlocks = this.Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            this.fallingBlocks.Sort((a, b) => (int)(a.X - b.X));
            this.movingBlocks = this.Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
            this.movingBlocks.Sort((a, b) => (int)(a.X - b.X));
        }

        private void CreateBossSprite()
        {
            this.Add((Component)(this.Sprite = GFX.SpriteBank.Create("chara_boss")));
            this.Sprite.OnFrameChange = (anim =>
            {
                if (anim == "charaboss_idle" && this.Sprite.CurrentAnimationFrame == 18)
                    Audio.Play("event:/char/badeline/boss_idle_air", this.Position);
            });
            this.facing = -1;
            if (this.NormalSprite != null)
            {
                this.Sprite.Position = this.NormalSprite.Position;
                this.Remove((Component)this.NormalSprite);
            }
            if (this.normalHair != null)
                this.Remove((Component)this.normalHair);
            this.NormalSprite = null;
            this.normalHair = null;
        }

        public override void Update()
        {
            base.Update();
            
            Sprite sprite = this.Sprite != null ? this.Sprite : this.NormalSprite;
            if (!this.Sitting)
            {
                var entity = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (!this.Moving && entity != null)
                {
                    if (this.facing == -1 && entity.X > this.X + 20.0f)
                    {
                        this.facing = 1;
                        this.scaleWiggler.Start();
                    }
                    else if (this.facing == 1 && entity.X < this.X - 20.0f)
                    {
                        this.facing = -1;
                        this.scaleWiggler.Start();
                    }
                }
                if (!this.playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
                {
                    this.playerHasMoved = true;
                    if (this.patternIndex != 0)
                        this.StartAttacking();
                    this.TriggerMovingBlocks(0);
                }
                if (!this.Moving)
                    sprite.Position = this.avoidPos + new Vector2(this.floatSine.Value * 3f, this.floatSine.ValueOverTwo * 4f);
                else
                    sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
                float radius = this.circle.Radius;
                this.circle.Radius = 6f;
                var dashBlock = this.CollideFirst<DashBlock>();
                if (dashBlock != null)
                    dashBlock.Break(Center, -Vector2.UnitY, true, false);
                this.circle.Radius = radius;
                if (!this.level.IsInBounds(this.Position, 24f))
                {
                    this.Active = this.Visible = this.Collidable = false;
                    return;
                }
                Vector2 target;
                if (!this.Moving && entity != null)
                {
                    float length = Calc.ClampedMap((this.Center - entity.Center).Length(), 32f, 88f, 12f, 0.0f);
                    target = length > 0.0f ? (this.Center - entity.Center).SafeNormalize(length) : Vector2.Zero;
                }
                else
                    target = Vector2.Zero;
                this.avoidPos = Calc.Approach(this.avoidPos, target, 40f * Engine.DeltaTime);
            }
            this.light.Position = sprite.Position + new Vector2(0.0f, -10f);
        }

        public override void Render()
        {
            if (this.Sprite != null)
            {
                this.Sprite.Scale.X = (float)this.facing;
                this.Sprite.Scale.Y = 1f;
                this.Sprite.Scale *= (float)(1.0 + this.scaleWiggler.Value * 0.2);
            }
            if (this.NormalSprite != null)
            {
                Vector2 position = this.NormalSprite.Position;
                this.NormalSprite.Position = this.NormalSprite.Position.Floor();
                base.Render();
                this.NormalSprite.Position = position;
            }
            else
                base.Render();
        }

        public void OnPlayer(global::Celeste.Player player)
        {
            if (Sprite == null)
                CreateBossSprite();
            
            Sprite.Play("charaboss_getHit");
            Audio.Play("event:/char/badeline/boss_hug", Position);
            chargeSfx.Stop();
            if (laserSfx.EventName == "event:/char/badeline/boss_laserDZ_CHarge" && laserSfx.Playing)
                laserSfx.Stop();

            Collidable = false;
            avoidPos = Vector2.Zero;
            nodeIndex++;
            
            if (dialog)
            {
                if (nodeIndex == 1)
                    Scene.Add(new MiniTextbox("dz_ch8_charaboss_tired_a"));
                else if (nodeIndex == 2)
                    Scene.Add(new MiniTextbox("dz_ch8_charaboss_tired_b"));
                else if (nodeIndex == 3)
                    Scene.Add(new MiniTextbox("dz_ch8_charaboss_tired_c"));
            }

            foreach (var entity in level.Tracker.GetEntities<CharaBossShot>())
                entity.RemoveSelf();

            foreach (var entity in level.Tracker.GetEntities<CharaBossBeam>())
                entity.RemoveSelf();

            // Keep BiggerBeam cleanup
            foreach (var entity in level.Tracker.GetEntities<CharaBossBiggerBeam>())
                entity.RemoveSelf();

            foreach (var entity in level.Tracker.GetEntities<CharaBossShotHoming>())
                entity.RemoveSelf();

            foreach (var entity in level.Tracker.GetEntities<CharaBossFloorSlam>())
                entity.RemoveSelf();

            TriggerFallingBlocks(X);
            TriggerMovingBlocks(nodeIndex);
            attackCoroutine.Active = false;
            Moving = true;
            bool lastHit = nodeIndex == nodes.Length - 1;
            
            // Handle teleportation for specific rooms
            if (lastHit && level.Session.Level.Equals("boss-19fake"))
            {
                // Teleport from boss-19fake to center-01
                Add(new Coroutine(TeleportToRoom(player, "center-01", lastHit)));
                return;
            }
            else if (lastHit && level.Session.Level.Equals("center-10"))
            {
                // Teleport from center-10 to boss-19
                Add(new Coroutine(TeleportToRoom(player, "boss-19", lastHit)));
                return;
            }
            
            if (CanChangeMusic(level.Session.Area.Mode == AreaMode.Normal))
            {
                if (lastHit && level.Session.Level.Equals("boss-19"))
                {
                    Alarm.Set(this, 0.25f, () =>
                    {
                        Audio.Play("event:/game/06_reflection/boss_spikes_burst");
                        foreach (var spinner in Scene.Tracker.GetEntities<CrystalStaticSpinner>())
                            spinner.RemoveSelf();
                    });
                    Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 1f);
                    Audio.SetMusic(null);
                }
                else if (nodeIndex == 4 && level.Session.Audio.Music.Event != "event:/pusheen/music/lvl8/chara_glitch")
                {
                    // After middle phase - switch to glitch music for intensity
                    level.Session.Audio.Music.Event = "event:/pusheen/music/lvl8/chara_glitch";
                    level.Session.Audio.Apply(false);
                }
                else if (startHit && level.Session.Audio.Music.Event != "event:/pusheen/music/lvl8/chara_core")
                {
                    level.Session.Audio.Music.Event = "event:/pusheen/music/lvl8/chara_core";
                    level.Session.Audio.Apply(false);
                }
                else if (startHit && level.Session.Audio.Music.Event != "event:/pusheen/music/lvl8/chara_glitch")
                {
                    level.Session.Audio.Music.Event = "event:/pusheen/music/lvl8/chara_glitch";
                    level.Session.Audio.Apply(false);
                }
                else if (level.Session.Audio.Music.Event != "event:/pusheen/music/lvl8/chara_fight" && level.Session.Audio.Music.Event != "event:/pusheen/music/lvl8/chara_glitch")
                {
                    level.Session.Audio.Music.Event = "event:/pusheen/music/lvl8/chara_fight";
                    level.Session.Audio.Apply(false);
                }
            }

            Add(new Coroutine(MoveSequence(player, lastHit)));
        }

        private IEnumerator MoveSequence(global::Celeste.Player player, bool lastHit)
        {
            if (lastHit)
            {
                Audio.SetMusicParam("boss_pitch", 1f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
                tween.OnUpdate = delegate(Tween t)
                {
                    Glitch.Value = 0.6f * t.Eased;
                };
                Add(tween);
            }
            else
            {
                Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
                tween2.OnUpdate = (Tween t) =>
                {
                    Glitch.Value = 0.5f * (1f - t.Eased);
                };
                Add(tween2);
            }
            
            if (player != null && !player.Dead)
                player.StartAttract(Center + Vector2.UnitY * 4f);
            
            float timer = 0.15f;
            while (player != null && !player.Dead && !player.AtAttractTarget)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            
            if (timer > 0f)
                yield return timer;
            
            foreach (ReflectionTentacles entity2 in Scene.Tracker.GetEntities<ReflectionTentacles>())
                entity2.Retreat();
            
            if (player != null)
            {
                CelesteGame.Freeze(0.1f);
                if (lastHit)
                    timeRateModifier.SetTimeRateMultiplier(0.5f);
                else
                    timeRateModifier.SetTimeRateMultiplier(0.75f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            
            PushPlayer(player);
            level.Shake();
            yield return 0.05f;
            
            // Spawn hit particles
            SpawnHitParticles();
            
            for (float num = 0f; num < (float)(Math.PI * 2.0); num += 0.17453292f)
            {
                Vector2 position = Center + Sprite.Position + Calc.AngleToVector(num + Calc.Random.Range(-(float)(Math.PI / 90.0), (float)(Math.PI / 90.0)), Calc.Random.Range(16, 20));
                level.Particles.Emit(PBurst, position, num);
            }
            
            yield return 0.05f;
            Audio.SetMusicParam("boss_pitch", 0f);
            
            float from = timeRateModifier.CurrentTimeRate();
            float assistAdjustedRate = Engine.TimeRate / Math.Max(from, 0.0001f);
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.35f / assistAdjustedRate, start: true);
            tween3.UseRawDeltaTime = true;
            tween3.OnUpdate = (Tween t) =>
            {
                if (bossBg != null && bossBg.Alpha < t.Eased)
                    bossBg.Alpha = t.Eased;
                timeRateModifier.SetTimeRateMultiplier(MathHelper.Lerp(from, 1f, t.Eased));
                if (lastHit)
                    Glitch.Value = 0.6f * (1f - t.Eased);
            };
            Add(tween3);
            
            yield return 0.2f;
            
            Vector2 from2 = Position;
            Vector2 to = nodes[nodeIndex];
            float duration = Vector2.Distance(from2, to) / MoveSpeed;
            float dir = (to - from2).Angle();
            
            Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, start: true);
            tween4.OnUpdate = (Tween t) =>
            {
                Position = Vector2.Lerp(from2, to, t.Eased);
                if (t.Eased >= 0.1f && t.Eased <= 0.9f && Scene.OnInterval(0.02f))
                {
                    TrailManager.Add(this, global::Celeste.Player.NormalHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                    level.Particles.Emit(global::Celeste.Player.P_DashB, 2, Center, Vector2.One * 3f, dir);
                }
            };
            tween4.OnComplete = (Tween t) =>
            {
                Sprite.Play("charaboss_recoverHit");
                Moving = false;
                Collidable = true;
                global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (entity != null)
                {
                    facing = Math.Sign(entity.X - X);
                    if (facing == 0)
                        facing = -1;
                }
                StartAttacking();
                floatSine.Reset();
            };
            Add(tween4);
        }

        private IEnumerator TeleportToRoom(global::Celeste.Player player, string targetRoom, bool lastHit)
        {
            if (lastHit)
            {
                Audio.SetMusicParam("boss_pitch", 1f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
                tween.OnUpdate = delegate(Tween t)
                {
                    Glitch.Value = 0.6f * t.Eased;
                };
                Add(tween);
            }
            
            if (player != null && !player.Dead)
                player.StartAttract(Center + Vector2.UnitY * 4f);
            
            float timer = 0.15f;
            while (player != null && !player.Dead && !player.AtAttractTarget)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            
            if (timer > 0f)
                yield return timer;
            
            foreach (ReflectionTentacles entity2 in Scene.Tracker.GetEntities<ReflectionTentacles>())
                entity2.Retreat();
            
            if (player != null)
            {
                CelesteGame.Freeze(0.1f);
                timeRateModifier.SetTimeRateMultiplier(0.5f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            
            PushPlayer(player);
            level.Shake();
            yield return 0.05f;
            
            // Spawn hit particles
            SpawnHitParticles();
            
            for (float num = 0f; num < (float)(Math.PI * 2.0); num += 0.17453292f)
            {
                Vector2 position = Center + Sprite.Position + Calc.AngleToVector(num + Calc.Random.Range(-(float)(Math.PI / 90.0), (float)(Math.PI / 90.0)), Calc.Random.Range(16, 20));
                level.Particles.Emit(PBurst, position, num);
            }
            
            yield return 0.05f;
            Audio.SetMusicParam("boss_pitch", 0f);
            
            float from = timeRateModifier.CurrentTimeRate();
            float assistAdjustedRate = Engine.TimeRate / Math.Max(from, 0.0001f);
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.35f / assistAdjustedRate, start: true);
            tween3.UseRawDeltaTime = true;
            tween3.OnUpdate = (Tween t) =>
            {
                if (bossBg != null && bossBg.Alpha < t.Eased)
                    bossBg.Alpha = t.Eased;
                timeRateModifier.SetTimeRateMultiplier(MathHelper.Lerp(from, 1f, t.Eased));
                if (lastHit)
                    Glitch.Value = 0.6f * (1f - t.Eased);
            };
            Add(tween3);
            
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
                
                level.LoadLevel(global::Celeste.Player.IntroTypes.Transition);
                
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

        private void PushPlayer(global::Celeste.Player player)
        {
            if (player != null && !player.Dead && nodeIndex < nodes.Length)
            {
                int num = Math.Sign(X - nodes[nodeIndex].X);
                if (num == 0)
                    num = -1;
                player.FinalBossPushLaunch(num);
            }
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        }

        private void SpawnHitParticles()
        {
            // Spawn custom hit particles in addition to the particle system
            Level level = SceneAs<Level>();
            if (level != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = Calc.Random.NextFloat() * (float)Math.PI * 2f;
                    Vector2 velocity = Calc.AngleToVector(angle, Calc.Random.Range(100f, 200f));
                    Color particleColor = Calc.Random.Choose(Color.Red, Color.Orange, Color.Yellow);
                    level.Add(new HitParticle(Center, velocity, particleColor));
                }
            }
        }

        private void TriggerFallingBlocks(float leftOfX)
        {
            while (fallingBlocks.Count > 0 && fallingBlocks[0].Scene == null)
                fallingBlocks.RemoveAt(0);
            
            int num = 0;
            while (fallingBlocks.Count > 0 && fallingBlocks[0].X < leftOfX)
            {
                FallingBlock obj = fallingBlocks[0] as FallingBlock;
                obj.StartShaking();
                obj.Triggered = true;
                obj.FallDelay = 0.4f * (float)num;
                num++;
                fallingBlocks.RemoveAt(0);
            }
        }

        private void TriggerMovingBlocks(int nodeIndex)
        {
            if (nodeIndex > 0)
                DestroyMovingBlocks(nodeIndex - 1);
            
            float num = 0f;
            foreach (FinalBossMovingBlock movingBlock in movingBlocks)
            {
                if (movingBlock.BossNodeIndex == nodeIndex)
                {
                    movingBlock.StartMoving(num);
                    num += 0.15f;
                }
            }
        }

        private void DestroyMovingBlocks(int nodeIndex)
        {
            float num = 0f;
            foreach (FinalBossMovingBlock movingBlock in movingBlocks)
            {
                if (movingBlock.BossNodeIndex == nodeIndex)
                {
                    movingBlock.Destroy(num);
                    num += 0.05f;
                }
            }
        }

        private void StartAttacking()
        {
            // In center-10, always use BiggerBeam as the final attack phase
            if (level.Session.Level == "center-10" && nodeIndex == nodes.Length - 2)
            {
                attackCoroutine.Replace(Attack21Sequence());
                return;
            }

            // If attackSequence is provided, use custom attack sequence
            if (!string.IsNullOrWhiteSpace(attackSequence))
            {
                attackCoroutine.Replace(CustomAttackSequence());
                return;
            }

            // Otherwise use pattern-based attacks
            switch (patternIndex)
            {
                case 0:
                case 1:
                    attackCoroutine.Replace(Attack01Sequence());
                    break;
                case 2:
                    attackCoroutine.Replace(Attack02Sequence());
                    break;
                case 3:
                    attackCoroutine.Replace(Attack03Sequence());
                    break;
                case 4:
                    attackCoroutine.Replace(Attack04Sequence());
                    break;
                case 5:
                    attackCoroutine.Replace(Attack05Sequence());
                    break;
                case 6:
                    attackCoroutine.Replace(Attack06Sequence());
                    break;
                case 7:
                    attackCoroutine.Replace(Attack07Sequence());
                    break;
                case 8:
                    attackCoroutine.Replace(Attack08Sequence());
                    break;
                case 9:
                    attackCoroutine.Replace(Attack09Sequence());
                    break;
                case 10:
                    attackCoroutine.Replace(Attack10Sequence());
                    break;
                case 11:
                    attackCoroutine.Replace(Attack11Sequence());
                    break;
                case 13:
                    attackCoroutine.Replace(Attack13Sequence());
                    break;
                case 14:
                    attackCoroutine.Replace(Attack14Sequence());
                    break;
                case 15:
                    attackCoroutine.Replace(Attack15Sequence());
                    break;
                // Add pattern for BiggerBeam
                case 21:
                    attackCoroutine.Replace(Attack21Sequence());
                    break;
                case 12:
                    break;
            }
        }

        private void StartShootCharge()
        {
            Sprite.Play("charaboss_attack1Begin");
            chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        private IEnumerator Attack01Sequence()
        {
            StartShootCharge();
            while (true)
            {
                yield return 0.5f;
                Shoot();
                yield return 1f;
                StartShootCharge();
                yield return 0.15f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack02Sequence()
        {
            while (true)
            {
                yield return 0.5f;
                yield return Beam();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.3f;
                Shoot();
                yield return 0.5f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack03Sequence()
        {
            StartShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 4)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 2f;
                StartShootCharge();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack04Sequence()
        {
            StartShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 4)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 1.5f;
                yield return Beam();
                yield return 1.5f;
                StartShootCharge();
            }
        }

        private IEnumerator Attack05Sequence()
        {
            yield return 0.2f;
            while (true)
            {
                yield return Beam();
                yield return 0.6f;
                StartShootCharge();
                yield return 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            ShootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 2)
                    {
                        StartShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 0.8f;
            }
        }

        private IEnumerator Attack06Sequence()
        {
            while (true)
            {
                yield return Beam();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack07Sequence()
        {
            while (true)
            {
                Shoot();
                yield return 0.8f;
                StartShootCharge();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack08Sequence()
        {
            while (true)
            {
                yield return 0.1f;
                yield return Beam();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack09Sequence()
        {
            StartShootCharge();
            while (true)
            {
                yield return 0.5f;
                Shoot();
                yield return 0.15f;
                StartShootCharge();
                Shoot();
                yield return 0.4f;
                StartShootCharge();
                yield return 0.1f;
            }
        }

        private IEnumerator Attack10Sequence()
        {
            yield break;
        }

        private IEnumerator Attack11Sequence()
        {
            if (nodeIndex == 0)
            {
                StartShootCharge();
                yield return 0.6f;
            }
            while (true)
            {
                Shoot();
                yield return 1.9f;
                StartShootCharge();
                yield return 0.6f;
            }
        }

        private IEnumerator Attack13Sequence()
        {
            if (nodeIndex != 0)
                yield return Attack01Sequence();
        }

        private IEnumerator Attack14Sequence()
        {
            while (true)
            {
                yield return 0.2f;
                yield return Beam();
                yield return 0.3f;
            }
        }

        private IEnumerator Attack15Sequence()
        {
            while (true)
            {
                yield return 0.2f;
                yield return Beam();
                yield return 1.2f;
            }
        }

        // Pattern 21 - BiggerBeam attack pattern
        private IEnumerator Attack21Sequence()
        {
            while (true)
            {
                yield return 1.0f;
                yield return BiggerBeam();
                yield return 1.5f;
                StartShootCharge();
                yield return 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    Shoot();
                    yield return 0.3f;
                    if (i < 2)
                    {
                        StartShootCharge();
                        yield return 0.2f;
                    }
                }
                yield return 2.0f;
            }
        }

        // Custom attack sequence parser and executor
        private IEnumerator CustomAttackSequence()
        {
            // Parse attack sequence string
            // Format: "action arg delay, action arg delay, ..."
            // Examples: "shoot 0.5", "beam 1.0", "biggerbeam 2.0", "shoot 15 0.3"
            
            if (string.IsNullOrWhiteSpace(attackSequence))
            {
                // Fallback to pattern 1 if sequence is empty
                yield return Attack01Sequence();
                yield break;
            }

            string[] actions = attackSequence.Split(',');

            while (true)
            {
                foreach (string actionStr in actions)
                {
                    string action = actionStr.Trim();
                    if (string.IsNullOrWhiteSpace(action))
                        continue;

                    string[] parts = action.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                        continue;

                    string command = parts[0].ToLower();
                    
                    switch (command)
                    {
                        case "shoot":
                            {
                                float angleOffset = 0f;
                                float delay = 0.3f;
                                
                                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedAngle))
                                    angleOffset = parsedAngle;
                                
                                if (parts.Length > 2 && float.TryParse(parts[2], out float parsedDelay))
                                    delay = parsedDelay;
                                else if (parts.Length == 2 && parts[1].Contains("."))
                                    delay = angleOffset; // Single float argument is delay
                                
                                StartShootCharge();
                                yield return 0.3f;
                                Shoot(angleOffset);
                                yield return delay;
                                break;
                            }
                        
                        case "beam":
                            {
                                float delay = 0.3f;
                                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedDelay))
                                    delay = parsedDelay;
                                
                                yield return Beam();
                                yield return delay;
                                break;
                            }
                        
                        case "biggerbeam":
                            {
                                float delay = 0.3f;
                                if (parts.Length > 1 && float.TryParse(parts[1], out float parsedDelay))
                                    delay = parsedDelay;
                                
                                yield return BiggerBeam();
                                yield return delay;
                                break;
                            }

                        // spread [spreadAngle] [delay]
                        // Fires 3 bullets in a fan at the player.
                        // spreadAngle defaults to 0.35 rad (~20 deg); delay defaults to 0.5 s.
                        // Example: "spread 0.4 0.6"
                        case "spread":
                            {
                                float spreadAngle = 0.35f;
                                float delay = 0.5f;
                                if (parts.Length > 1 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pA))
                                    spreadAngle = pA;
                                if (parts.Length > 2 && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pD))
                                    delay = pD;

                                StartShootCharge();
                                yield return 0.3f;
                                SpreadShot(spreadAngle);
                                yield return delay;
                                break;
                            }

                        // spiral [count] [delay]
                        // Fires [count] bullets evenly in a full ring (default 8).
                        // Example: "spiral 12 1.0"
                        case "spiral":
                            {
                                int count = 8;
                                float delay = 0.8f;
                                if (parts.Length > 1 && int.TryParse(parts[1], out int pC))
                                    count = Math.Max(2, pC);
                                if (parts.Length > 2 && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pD))
                                    delay = pD;

                                StartShootCharge();
                                yield return 0.4f;
                                SpiralBurst(count);
                                yield return delay;
                                break;
                            }

                        // homing [delay]
                        // Fires a slow homing missile that curves toward the player.
                        // Example: "homing 1.5"
                        case "homing":
                            {
                                float delay = 1.0f;
                                if (parts.Length > 1 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pD))
                                    delay = pD;

                                StartShootCharge();
                                yield return 0.3f;
                                ShootHoming();
                                yield return delay;
                                break;
                            }

                        // floorslam [delay]
                        // Fires a ball downward; on impact it spawns left+right shockwaves.
                        // Example: "floorslam 1.2"
                        case "floorslam":
                            {
                                float delay = 1.0f;
                                if (parts.Length > 1 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pD))
                                    delay = pD;

                                FloorSlam();
                                yield return delay;
                                break;
                            }

                        default:
                            // Unknown command, skip
                            yield return 0.1f;
                            break;
                    }
                }
            }
        }

        // ── New attacks ──────────────────────────────────────────────────────────

        // Fire 3 bullets in a fan centered on the player.
        private void SpreadShot(float spreadAngle = 0.35f)
        {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);

            Sprite.Play("charaboss_attack1Recoil", restart: true);
            global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                level.Add(new CharaBossShot().Init(this, entity, -spreadAngle));
                level.Add(new CharaBossShot().Init(this, entity, 0f));
                level.Add(new CharaBossShot().Init(this, entity,  spreadAngle));
            }
        }

        // Fire bullets in a full ring (spiral burst).
        private void SpiralBurst(int count = 8)
        {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);

            Sprite.Play("charaboss_attack1Recoil", restart: true);
            float step = (float)(Math.PI * 2.0 / count);
            // Use a fixed base angle so every shot goes in a clean ring
            float baseAngle = Calc.Random.NextFloat() * step; // slight random rotation each burst
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + step * i;
                var shot = new CharaBossShot();
                // Init with a fixed target point in the desired direction
                Vector2 target = ShotOrigin + Calc.AngleToVector(angle, 200f);
                level.Add(shot.Init(this, target));
            }
        }

        // Fire a single slow homing missile at the player.
        private void ShootHoming()
        {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);

            Sprite.Play("charaboss_attack1Recoil", restart: true);
            global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
            level.Add(new CharaBossShotHoming().Init(this, entity));
        }

        // Fire a ball straight down that spawns floor shockwaves on impact.
        private void FloorSlam()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            Sprite.Play("charaboss_attack1Recoil", restart: true);
            level.Add(new CharaBossFloorSlam().Init(this));
        }

        // ─────────────────────────────────────────────────────────────────────────

        private void Shoot(float angleOffset = 0f)
        {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);
            
            Sprite.Play("charaboss_attack1Recoil", restart: true);
            global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
                level.Add(new CharaBossShot().Init(this, entity, angleOffset));
        }

        private void ShootAt(Vector2 at)
        {
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
            else
                chargeSfx.Param("end", 1f);
            
            Sprite.Play("charaboss_attack1Recoil", restart: true);
            level.Add(new CharaBossShot().Init(this, at));
        }

        private IEnumerator Beam()
        {
            laserSfx.Play("event:/char/badeline/boss_laserDZ_CHarge");
            Sprite.Play("charaboss_attack2Begin", restart: true);
            yield return 0.1f;
            global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
                level.Add(new CharaBossBeam().Init(this, entity));
            
            yield return 0.9f;
            Sprite.Play("charaboss_attack2Lock", restart: true);
            yield return 0.5f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            Sprite.Play("charaboss_attack2Recoil");
        }

        // Keep BiggerBeam method
        private IEnumerator BiggerBeam()
        {
            laserSfx.Play("event:/char/badeline/boss_laserDZ_CHarge");
            Sprite.Play("charaboss_attack2Begin", restart: true);
            yield return 0.2f; // Longer charge setup
            global::Celeste.Player entity = level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
                level.Add(new CharaBossBiggerBeam().Init(this, entity));
            
            yield return CharaBossBiggerBeam.CHARGE_TIME; // Wait for the bigger beam charge time
            Sprite.Play("charaboss_attack2Lock", restart: true);
            yield return 0.6f; // Wait for lock animation
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            yield return CharaBossBiggerBeam.ACTIVE_TIME; // Wait for 5s beam to finish
            Sprite.Play("charaboss_attack2Recoil");
        }

        public override void Removed(Scene scene)
        {
            if (bossBg != null && patternIndex == 0)
                bossBg.Alpha = 1f;
            base.Removed(scene);
        }

        public bool CanChangeMusic(bool value)
        {
            if ((base.Scene as Level).Session.Area.GetLevelSet() == "Celeste")
                return value;
            return canChangeMusic;
        }
    }
}




