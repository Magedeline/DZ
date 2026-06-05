namespace Celeste.Entities 
{
    [CustomEntity("MaggyHelper/HeartGem")]
    [Tracked]
    [HotReloadable]
    public class HeartGem : Entity {

        // Fields
        public static ParticleType PBlueShine;
        public static ParticleType PRedShine;
        public static ParticleType PGoldShine;
        public static ParticleType PFakeShine;
        public static ParticleType PPinkShine;
        public static ParticleType PRainbowShine;
        private readonly TimeRateModifier timeRateModifier;
        
      // Static constructor to initialize particle types
      static HeartGem()
        {
            LoadParticles();
        }
        
        /// <summary>
    /// Initialize all particle types for HeartGem
        /// </summary>
        public static void LoadParticles()
        {
    // Blue shine (A-Side)
         PBlueShine = new ParticleType
          {
   Color = Color.Aqua,
           Color2 = Color.LightBlue,
        ColorMode = ParticleType.ColorModes.Blink,
     FadeMode = ParticleType.FadeModes.Late,
     Size = 1f,
     SizeRange = 0.5f,
                Direction = -(float)Math.PI / 2f,
     DirectionRange = (float)Math.PI / 8f,
      LifeMin = 0.8f,
  LifeMax = 1.2f,
      SpeedMin = 4f,
                SpeedMax = 12f,
   SpeedMultiplier = 0.01f,
        Acceleration = Vector2.UnitY * 5f
            };
   
     // Red shine (B-Side)
  PRedShine = new ParticleType
            {
         Color = Color.Red,
    Color2 = Color.OrangeRed,
     ColorMode = ParticleType.ColorModes.Blink,
     FadeMode = ParticleType.FadeModes.Late,
     Size = 1f,
                SizeRange = 0.5f,
        Direction = -(float)Math.PI / 2f,
         DirectionRange = (float)Math.PI / 8f,
LifeMin = 0.8f,
              LifeMax = 1.2f,
     SpeedMin = 4f,
         SpeedMax = 12f,
    SpeedMultiplier = 0.01f,
          Acceleration = Vector2.UnitY * 5f
            };
       
            // Gold shine (C-Side)
PGoldShine = new ParticleType
            {
     Color = Color.Gold,
     Color2 = Color.Yellow,
     ColorMode = ParticleType.ColorModes.Blink,
             FadeMode = ParticleType.FadeModes.Late,
     Size = 1f,
        SizeRange = 0.5f,
                Direction = -(float)Math.PI / 2f,
          DirectionRange = (float)Math.PI / 8f,
         LifeMin = 0.8f,
    LifeMax = 1.2f,
      SpeedMin = 4f,
     SpeedMax = 12f,
       SpeedMultiplier = 0.01f,
  Acceleration = Vector2.UnitY * 5f
            };
            // Pink shine (D-Side)
            PPinkShine = new ParticleType
    {
            Color = Color.Pink,
         Color2 = Color.LightPink,
   ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
      Size = 1f,
 SizeRange = 0.5f,
    Direction = -(float)Math.PI / 2f,
    DirectionRange = (float)Math.PI / 8f,
     LifeMin = 0.8f,
        LifeMax = 1.2f,
   SpeedMin = 4f,
        SpeedMax = 12f,
   SpeedMultiplier = 0.01f,
  Acceleration = Vector2.UnitY * 5f
     };
         
            // Fake shine
PFakeShine = new ParticleType
            {
          Color = Calc.HexToColor("dad8cc"),
 Color2 = Color.White,
         ColorMode = ParticleType.ColorModes.Blink,
         FadeMode = ParticleType.FadeModes.Late,
       Size = 1f,
       SizeRange = 0.5f,
        Direction = -(float)Math.PI / 2f,
          DirectionRange = (float)Math.PI / 8f,
     LifeMin = 0.8f,
        LifeMax = 1.2f,
      SpeedMin = 4f,
       SpeedMax = 12f,
      SpeedMultiplier = 0.01f,
  Acceleration = Vector2.UnitY * 5f
            };

            // Rainbow shine (Astral/Final Remix)
            PRainbowShine = new ParticleType
            {
                Color = Calc.HexToColor("ff0000"),
                Color2 = Calc.HexToColor("ff8000"),
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 8f,
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                SpeedMin = 4f,
                SpeedMax = 12f,
                SpeedMultiplier = 0.01f,
                Acceleration = Vector2.UnitY * 5f
            };
        }

        public HeartGem(Vector2 position) : base(position) {
          Add(this.timeRateModifier = new TimeRateModifier(1f, false));
        Add(this.holdableCollider = new HoldableCollider(this.OnHoldable, null));
       Add(new MirrorReflection());
 }

        public HeartGem(EntityData data, Vector2 offset) : this(data.Position + offset) {
            this.removeCameraTriggers = data.Bool(nameof(removeCameraTriggers), false);
            this.IsFake = data.Bool("fake", false);
            this.IsAstral = data.Bool("astral", false);
            this.endLevelOnCollect = data.Bool(nameof(endLevelOnCollect), false);
            this.entityId = new EntityID(data.Level.Name, data.ID);
            this.sevenbirdFlyby = data.Bool("sevenbirdFlyby", false);
            this.unlockDside = data.Bool("unlockDside", false);
            this.customSfx = data.Attr("customSfx", "");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            AreaKey area = (Scene as Level).Session.Area;
            this.IsGhost = (!this.IsFake && !this.IsAstral && SaveData.Instance.Areas[area.ID].Modes[(int)area.Mode].HeartGem);
            string id;
            if (this.IsAstral) {
                id = "maggy_heartgem4"; // Astral/Final Remix - Rainbow heartgem
            } else if (this.IsFake) {
                id = "heartgem4"; // Grey heartgem for fake/wrong heart
            } else if (this.IsGhost) {
                id = "heartgem5";
            } else if (area.Mode == AreaMode.Normal) {
                id = "maggy_heartgem0"; // A-Side - Blue heartgem
            } else if (area.Mode == AreaMode.BSide) {
                id = "maggy_heartgem1"; // B-Side - Red heartgem
            } else if (area.Mode == AreaMode.CSide) {
                id = "maggy_heartgem2"; // C-Side - Golden heartgem
            } else if (area.Mode == (AreaMode)3) {
                id = "maggy_heartgem3"; // D-Side - Pink heartgem
            } else {
                id = "maggy_heartgem0"; // Default to blue
            }
            Add(this.sprite = GFX.SpriteBank.Create(id));
          this.sprite.Play("spin", false, false);
            this.sprite.OnLoop = delegate (string anim) {
                if (this.Visible && anim == "spin" && this.autoPulse) {
                    if (this.IsAstral) {
                        Audio.Play("event:/game/general/crystalheart_pulse", this.Position);
                    } else if (this.IsFake) {
                        Audio.Play("event:/new_content/game/pusheen/19_spaces/fakeheart_pulse", this.Position);
                    } else {
                        Audio.Play("event:/game/general/crystalheart_pulse", this.Position);
                    }
                    this.ScaleWiggler.Start();
 (Scene as Level).Displacement.AddBurst(this.Position, 0.35f, 8f, 48f, 0.25f, null, null);
         }
  };
        if (this.IsGhost) {
      this.sprite.Color = Color.White * 0.8f;
            }
          Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(this.OnPlayer, null, null));
      Add(this.ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
        this.sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }, false, false));
        Add(this.bloom = new BloomPoint(0.75f, 16f));
  Color color;
     if (this.IsAstral) {
                // Astral/Final Remix - Rainbow colors
                color = Calc.HexToColor("ff4080");
                this.shineParticle = HeartGem.PRainbowShine;
            } else if (this.IsFake) {
                color = Calc.HexToColor("dad8cc");
                this.shineParticle = HeartGem.PFakeShine;
            } else if (area.Mode == AreaMode.Normal) {
                color = Color.Aqua;
                this.shineParticle = HeartGem.PBlueShine;
            } else if (area.Mode == AreaMode.BSide) {
                color = Color.Red;
                this.shineParticle = HeartGem.PRedShine;
            } else if (area.Mode == AreaMode.CSide) {
                color = Color.Gold;
                this.shineParticle = HeartGem.PGoldShine;
            } else if (area.Mode == (AreaMode)3) {
                // D-Side - Pink heart
                color = Color.Pink;
                this.shineParticle = HeartGem.PPinkShine;
            } else {
                color = Color.White;
                this.shineParticle = HeartGem.PBlueShine;
            }
            color = Color.Lerp(color, Color.White, 0.5f);
    Add(this.light = new VertexLight(color, 1f, 32, 64));
            if (this.IsFake) {
                this.bloom.Alpha = 0f;
                this.light.Alpha = 0f;
            }
            if (this.IsAstral) {
                this.bloom.Alpha = 1f;
                this.bloom.Radius = 32f;
                this.light.Alpha = 1f;
                this.light.StartRadius = 48f;
                this.light.EndRadius = 96f;
            }
            this.moveWiggler = Wiggler.Create(0.8f, 2f, null, false, false);
  this.moveWiggler.StartZero = true;
            Add(this.moveWiggler);
            if (this.IsFake) {
     Add(new PlayerCollider(player => this.OnPlayer(player), null, null));
                global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
    if (entity != null && (entity.X > X || (scene as Level).Session.GetFlag("wrong_heart"))) {
       this.Visible = false;
               Alarm.Set(this, 0.0001f, delegate {
        this.FakeRemoveCameraTrigger();
       RemoveSelf();
   }, Alarm.AlarmMode.Oneshot);
           return;
       }
scene.Add(this.fakeRightWall = new InvisibleBarrier(new Vector2(X + 160f, Y - 200f), 8f, 400f));
  }
        }

        private void OnPlayer(global::Celeste.Player player) {
         if (!this.collected && !(Scene as Level).Frozen) {
              if (player.DashAttacking) {
              this.collect(player);
               return;
   }
      if (this.bounceSfxDelay <= 0f) {
           if (this.IsAstral) {
      Audio.Play("event:/game/general/crystalheart_bounce", this.Position);
          } else if (this.IsFake) {
      Audio.Play("event:/new_content/game/pusheen/19_spaces/fakeheart_bounce", this.Position);
          } else {
        Audio.Play("event:/game/general/crystalheart_bounce", this.Position);
         }
           this.bounceSfxDelay = 0.1f;
           }
   player.PointBounce(Center);
                this.moveWiggler.Start();
                this.ScaleWiggler.Start();
        this.moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
  }
    }

  public override void Update() {
         this.bounceSfxDelay -= Engine.DeltaTime;
  this.timer += Engine.DeltaTime;

            // Rainbow color cycling for astral heartgem
            if (this.IsAstral && !this.collected) {
                this.rainbowHue += Engine.DeltaTime * 0.5f;
                if (this.rainbowHue > 1f) {
                    this.rainbowHue -= 1f;
                }
                Color rainbowColor = Calc.HsvToColor(this.rainbowHue, 0.8f, 1f);
                this.sprite.Color = rainbowColor;
                this.light.Color = rainbowColor;
            }

            this.sprite.Position = Vector2.UnitY * (float)Math.Sin((double)(this.timer * 2f)) * 2f + this.moveWiggleDir * this.moveWiggler.Value * -8f;
    if (this.white != null) {
          this.white.Position = this.sprite.Position;
    this.white.Scale = this.sprite.Scale;
           if (this.white.CurrentAnimationID != this.sprite.CurrentAnimationID) {
          this.white.Play(this.sprite.CurrentAnimationID, false, false);
         }
                this.white.SetAnimationFrame(this.sprite.CurrentAnimationFrame);
            }
  if (this.collected) {
         global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
          if (entity == null || entity.Dead) {
this.endCutscene();
        }
  }
       base.Update();
   if (!this.collected && Scene.OnInterval(0.1f)) {
           SceneAs<Level>().Particles.Emit(this.shineParticle, 1, Center, Vector2.One * 8f);
}
        }

        public void OnHoldable(Holdable h) {
            global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (!this.collected && entity != null && h.Dangerous(this.holdableCollider)) {
         this.collect(entity);
    }
        }

      private void collect(global::Celeste.Player entity) {
            AngryOshiro oshiro = Scene.Tracker.GetEntity<AngryOshiro>();
        if (oshiro != null) {
  oshiro.StopControllingTime();
    }
  Add(new Coroutine(this.collectRoutine(entity), true) {
             UseRawDeltaTime = true
            });
      this.collected = true;
    if (this.removeCameraTriggers) {
   foreach (CameraOffsetTrigger cameraOffsetTrigger in Scene.Entities.FindAll<CameraOffsetTrigger>()) {
       cameraOffsetTrigger.RemoveSelf();
       }
            }
        }

        private IEnumerator collectRoutine(global::Celeste.Player player) {
     Level level = Scene as Level;
            AreaKey area = level.Session.Area;
          string poemId = AreaData.Get(level).Mode[(int)area.Mode].PoemID;
            bool completeArea = !this.IsFake && (this.endLevelOnCollect || area.Mode != AreaMode.Normal || area.ID == 19);
            bool isCrystalHeartVariant = this.sevenbirdFlyby || !string.IsNullOrEmpty(this.customSfx) || this.unlockDside;
            if (this.IsFake) {
           level.StartCutscene(this.SkipFakeHeartCutscene, true, false, true);
           foreach (BirdNPC existingBird in level.Entities.FindAll<BirdNPC>()) {
               existingBird.RemoveSelf();
           }
            } else {
                level.CanRetry = false;
 }
   if (completeArea || this.IsFake) {
                Audio.SetMusic(null, true, true);
      Audio.SetAmbience(null, true);
}
            if (completeArea) {
   List<global::Celeste.Strawberry> list = new List<global::Celeste.Strawberry>();
foreach (Follower follower in player.Leader.Followers) {
         if (follower.Entity is global::Celeste.Strawberry) {
 list.Add(follower.Entity as global::Celeste.Strawberry);
        }
 }
         foreach (global::Celeste.Strawberry strawberry in list) {
     strawberry.OnCollect();
            }
 }
     string text = "event:/game/general/crystalheart_blue_get";
          if (!string.IsNullOrEmpty(this.customSfx)) {
                text = this.customSfx;
            } else if (this.IsAstral) {
                text = "event:/game/pusheen/general/crystalheart_astral_void_get";
            } else if (this.IsFake) {
                text = "event:/new_content/game/pusheen/19_spaces/fakeheart_get";
            } else if (area.Mode == AreaMode.BSide) {
                text = "event:/game/general/crystalheart_red_get";
            } else if (area.Mode == AreaMode.CSide) {
                text = "event:/game/general/crystalheart_gold_get";
            } else if (area.Mode == (AreaMode)3) {
                text = "event:/game/pusheen/general/crystalheart_rainbow_get";
            }
     this.sfx = SoundEmitter.Play(text, this, null);
            Add(new LevelEndingHook(delegate () {
    this.sfx.Source.Stop(true);
   }));
   this.walls.Add(new InvisibleBarrier(new Vector2((float)level.Bounds.Right, (float)level.Bounds.Top), 8f, (float)level.Bounds.Height));
    this.walls.Add(new InvisibleBarrier(new Vector2((float)(level.Bounds.Left - 8), (float)level.Bounds.Top), 8f, (float)level.Bounds.Height));
            this.walls.Add(new InvisibleBarrier(new Vector2((float)level.Bounds.Left, (float)(level.Bounds.Top - 8)), (float)level.Bounds.Width, 8f));
   foreach (InvisibleBarrier entity in this.walls) {
                Scene.Add(entity);
 }
      Add(this.white = GFX.SpriteBank.Create("heartgem4")); // Use grey heartgem for the white flash effect
            Depth = -2000000;
            yield return null;
            CelesteGame.Freeze(0.2f);
      yield return null;
       this.timeRateModifier.SetTimeRateMultiplier(0.5f);
          player.Depth = -2000000;
            for (int i = 0; i < 10; i++) {
       Scene.Add(new AbsorbOrb(this.Position, null, null));
        }
 level.Shake(0.3f);
 Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
          level.Flash(Color.White, false);
        level.FormationBackdrop.Display = true;
 level.FormationBackdrop.Alpha = 1f;
       this.light.Alpha = (this.bloom.Alpha = 0f);
   this.Visible = false;
 for (float t = 0f; t < 2f; t += Engine.RawDeltaTime) {
                this.timeRateModifier.SetTimeRateMultiplier(Calc.Approach(this.timeRateModifier.CurrentTimeRate(), 0f, Engine.RawDeltaTime * 0.25f));
      yield return null;
}
            yield return null;
      if (player.Dead) {
    yield return 100f;
    }
  this.timeRateModifier.ResetTimeRateMultiplier();
    Tag = Tags.FrozenUpdate;
      level.Frozen = true;
   if (!this.IsFake && !this.IsAstral) {
                this.registerAsCollected(level, poemId);
                if (completeArea) {
                    level.TimerStopped = true;
                    level.RegisterAreaComplete();
                }
            }
            if (this.IsAstral && completeArea) {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            if (isCrystalHeartVariant) {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
   string text2 = null;
            if (!string.IsNullOrEmpty(poemId)) {
             text2 = Dialog.Clean("soul_" + poemId, null);
     }
     this.poem = new Poem(text2, (int)(this.IsAstral ? ((AreaMode)4) : (this.IsFake ? ((AreaMode)3) : area.Mode)), (area.Mode == AreaMode.CSide || area.Mode == (AreaMode)3 || this.IsAstral || this.IsFake || isCrystalHeartVariant) ? 1f : 0.6f);
      this.poem.Alpha = 0f;
         Scene.Add(this.poem);
 for (float t = 0f; t < 1f; t += Engine.RawDeltaTime) {
  this.poem.Alpha = Ease.CubeOut(t);
            yield return null;
        }
        // Sevenbird flyby effect for crystal heart variants
        if (this.sevenbirdFlyby) {
            yield return SevenbirdFlybyRoutine();
        }
  if (this.IsFake) {
  yield return this.doFakeRoutineWithBird(player);
            } else {
      while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed) {
 yield return null;
    }
   this.sfx.Source.Param("end", 1f);

            // Check for D-side unlock condition (18_core_cside) for crystal heart variants
            bool shouldUnlockDside = this.unlockDside && area.ID == 18 && area.Mode == AreaMode.CSide;
            // Check for all crystal hearts collected
            bool allCrystalHeartsCollected = this.CheckAllCrystalHeartsCollected();

            if (allCrystalHeartsCollected && isCrystalHeartVariant) {
                // Show ultra completion postcard
                level.FormationBackdrop.Display = false;
                for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
                    this.poem.Alpha = Ease.CubeIn(1f - t);
                    yield return null;
                }
                player.Depth = 0;
                this.endCutscene();
                yield return ShowUltraPostcard(level);
            } else if (shouldUnlockDside) {
                // Show D-side unlock postcard
                level.FormationBackdrop.Display = false;
                for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
                    this.poem.Alpha = Ease.CubeIn(1f - t);
                    yield return null;
                }
                player.Depth = 0;
                this.endCutscene();
                yield return ShowDsidePostcard(level);
            } else if (!completeArea && !isCrystalHeartVariant) {
     level.FormationBackdrop.Display = false;
  for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
this.poem.Alpha = Ease.CubeIn(1f - t);
 yield return null;
     }
         player.Depth = 0;
            this.endCutscene();
  } else {
  yield return new FadeWipe(level, false, null) {
      Duration = 3.25f
               }.Duration;
    level.CompleteArea(false, true, false);
      }
            }
        yield break;
        }

     private IEnumerator doFakeRoutineWithBird(global::Celeste.Player player) {
            Level level = Scene as Level;
            int panAmount = 64;
  Vector2 panFrom = level.Camera.Position;
            Vector2 panTo = level.Camera.Position + new Vector2((float)(-(float)panAmount), 0f);
  Vector2 birdFrom = new Vector2(panTo.X - 16f, player.Y - 20f);
     Vector2 birdTo = new Vector2(panFrom.X + 320f + 16f, player.Y - 20f);
          yield return 2f;
Glitch.Value = 0.75f;
            while (Glitch.Value > 0f) {
       Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
    level.Shake(0.3f);
    yield return null;
            }
         yield return 1.1f;
        Glitch.Value = 0.75f;
         while (Glitch.Value > 0f) {
    Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
  level.Shake(0.3f);
       yield return null;
 }
 yield return 0.4f;
    for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / 2f) {
                level.Camera.Position = panFrom + (panTo - panFrom) * Ease.CubeInOut(p);
             this.poem.Offset = new Vector2((float)(panAmount * 8), 0f) * Ease.CubeInOut(p);
       yield return null;
}
      this.bird = new BirdNPC(birdFrom, BirdNPC.Modes.None);
       this.bird.Sprite.Play("fly", false, false);
     this.bird.Sprite.UseRawDeltaTime = true;
            this.bird.Facing = Facings.Right;
      this.bird.Depth = -2000100;
      this.bird.Tag = Tags.FrozenUpdate;
            this.bird.Add(new VertexLight(Color.White, 0.5f, 8, 32));
   this.bird.Add(new BloomPoint(0.5f, 12f));
            level.Add(this.bird);
            for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / 2.6f) {
              level.Camera.Position = panTo + (panFrom - panTo) * Ease.CubeInOut(p);
     this.poem.Offset = new Vector2((float)(panAmount * 8), 0f) * Ease.CubeInOut(1f - p);
     float num = 0.1f;
         float num2 = 0.9f;
           if (p > num && p <= num2) {
        float num3 = (p - num) / (num2 - num);
     this.bird.Position = birdFrom + (birdTo - birdFrom) * num3 + Vector2.UnitY * (float)Math.Sin((double)(num3 * 8f)) * 8f;
       }
    if (level.OnRawInterval(0.2f)) {
         TrailManager.Add(this.bird, Calc.HexToColor("639bff"), 1f, true, true);
           }
      yield return null;
            }
        this.bird.RemoveSelf();
          this.bird = null;
     this.timeRateModifier.SetTimeRateMultiplier(0f);
            level.Frozen = false;
            player.Active = false;
    player.StateMachine.State = Player.StDummy;
            while (this.timeRateModifier.CurrentTimeRate() < 0.9999f) {
 this.timeRateModifier.SetTimeRateMultiplier(Calc.Approach(this.timeRateModifier.CurrentTimeRate(), 1f, 0.5f * Engine.RawDeltaTime));
      yield return null;
     }
   this.timeRateModifier.ResetTimeRateMultiplier();
            yield return Textbox.Say("MAGGYHELPER_CH19_WRONG_HEART", new Func<IEnumerator>[]
            {
                this.SevenBirdGonersFlyPast
            });
       this.sfx.Source.Param("end", 1f);
 yield return 0.283f;
  level.FormationBackdrop.Display = false;
            for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / 0.2f) {
         this.poem.TextAlpha = Ease.CubeIn(1f - p);
   this.poem.ParticleSpeed = this.poem.TextAlpha;
             yield return null;
   }
            // Check if the "break" animation exists before playing it
            if (this.poem.Heart.Has("break")) {
                this.poem.Heart.Play("break", false, false);
                while (this.poem.Heart.Animating) {
                    this.poem.Shake += Engine.DeltaTime;
                    yield return null;
                }
            } else {
                // Fallback: just wait a bit and shake
                for (float t = 0f; t < 0.5f; t += Engine.DeltaTime) {
                    this.poem.Shake += Engine.DeltaTime;
                    yield return null;
                }
            }
       this.poem.RemoveSelf();
        this.poem = null;
            for (int i = 0; i < 10; i++) {
    Vector2 position = level.Camera.Position + new Vector2(320f, 180f) * 0.5f;
     Vector2 value = level.Camera.Position + new Vector2(160f, -64f);
   Scene.Add(new AbsorbOrb(position, null, new Vector2?(value)));
        }
     level.Shake(0.3f);
         Glitch.Value = 0.8f;
         while (Glitch.Value > 0f) {
 Glitch.Value -= Engine.DeltaTime * 4f;
     yield return null;
     }
            yield return 0.25f;
            level.Session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl19/inmyway";
   level.Session.Audio.Apply(false);
            player.Active = true;
    player.Depth = 0;
       player.StateMachine.State = Player.StDummy;
            while (player.OnGround(1) && player.Bottom < (float)level.Bounds.Bottom) {
     yield return null;
          }
         player.Facing = Facings.Right;
  yield return 0.5f;
        yield return Textbox.Say("MAGGYHELPER_CH19_KEEP_GOING_KIRBY", new Func<IEnumerator>[]
   {
             this.playerStepForward
   });
            this.SkipFakeHeartCutscene(level);
    level.EndCutscene();
        }

        private IEnumerator playerStepForward() {
            yield return 0.1f;
   global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
   if (entity != null && entity.CollideCheck<Solid>(entity.Position + new Vector2(12f, 1f))) {
           yield return entity.DummyWalkToExact((int)entity.X + 10, false, 1f, false);
    }
            yield return 0.2f;
            yield break;
        }

        private IEnumerator SevenBirdGonersFlyPast() {
            Level level = SceneAs<Level>();
            global::Celeste.Player entity = Scene?.Tracker?.GetEntity<global::Celeste.Player>();
            if (level == null) {
                yield break;
            }

            if (entity != null) {
                Audio.Play("event:/game/general/seed_complete_main", entity.Position);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }

            this.ClearFakeBirdGoners();
            Audio.Play("event:/new_content/game/10_farewell/glitch_short", level.Camera.Position + new Vector2(160f, 90f));

            Vector2 cameraPosition = level.Camera.Position;
            for (int i = 0; i < FakeBirdGonerSpriteIds.Length; i++) {
                Vector2 start = cameraPosition + new Vector2(-20f - i * 16f, 60f + i * 14f);
                FakeBirdGoner birdGoner = new FakeBirdGoner(FakeBirdGonerSpriteIds[i], start);
                this.fakeBirdGoners.Add(birdGoner);
                level.Add(birdGoner);
            }

            for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / 1f) {
                for (int i = 0; i < this.fakeBirdGoners.Count; i++) {
                    FakeBirdGoner birdGoner = this.fakeBirdGoners[i];
                    Vector2 start = cameraPosition + new Vector2(-20f - i * 16f, 60f + i * 14f);
                    Vector2 end = cameraPosition + new Vector2(340f + i * 10f, 40f + i * 10f);
                    float birdProgress = Math.Clamp((p - i * 0.04f) / 0.65f, 0f, 1f);
                    float eased = Ease.CubeInOut(birdProgress);
                    birdGoner.Position = start + (end - start) * eased + Vector2.UnitY * (float)Math.Sin((double)(birdProgress * 8f + i)) * 8f;
                    if (birdProgress > 0f && level.OnRawInterval(0.2f)) {
                        TrailManager.Add(birdGoner, Calc.HexToColor("639bff"), 1f, true, true);
                    }
                }
                yield return null;
            }

            this.ClearFakeBirdGoners();
            yield return 0.3f;
        }

        public void SkipFakeHeartCutscene(Level level) {
          this.timeRateModifier.ResetTimeRateMultiplier();
            Glitch.Value = 0.0f;

            if (sfx != null) {
     sfx.Source.Stop();
         }

       level.Session.SetFlag("wrong_heart");
level.Frozen = false;
         level.FormationBackdrop.Display = false;
      level.Session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl19/inmyway";
            level.Session.Audio.Apply();

            global::Celeste.Player entity1 = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
    if (entity1 != null) {
           entity1.Sprite.Play("idle", false, false);
    entity1.Active = true;
          entity1.StateMachine.State = Player.StNormal;
      entity1.Dashes = 1;
                entity1.Speed = Vector2.Zero;
            entity1.MoveV(200f, null, null);
       entity1.Depth = 0;
        for (int index = 0; index < 10; ++index)
       entity1.UpdateHair(true);
            }

            foreach (AbsorbOrb entity2 in this.Scene.Entities.FindAll<AbsorbOrb>())
                entity2.RemoveSelf();

            this.ClearFakeBirdGoners();

            if (poem != null) {
        poem.RemoveSelf();
   poem = null;
            }

            if (bird != null) {
    bird.RemoveSelf();
           bird = null;
          }

   if (fakeRightWall != null) {
   fakeRightWall.RemoveSelf();
    fakeRightWall = null;
            }

            this.FakeRemoveCameraTrigger();

    foreach (InvisibleBarrier wall in this.walls)
        wall.RemoveSelf();

         this.RemoveSelf();
        }

  public void FakeRemoveCameraTrigger() {
   Scene scene = this.Scene;
        if (scene == null) return;
    
            CameraTargetTrigger cameraTargetTrigger = CollideFirst<CameraTargetTrigger>();
         if (cameraTargetTrigger != null) {
      cameraTargetTrigger.LerpStrength = 0.0f;
      }
          
       foreach (CameraOffsetTrigger trigger in scene.Entities.FindAll<CameraOffsetTrigger>()) {
        if (Vector2.Distance(trigger.Position, this.Position) < 200f) {
           trigger.RemoveSelf();
           }
        }
     }

        private void endCutscene() {
Level level = Scene as Level;
            level.Frozen = false;
     level.CanRetry = true;
    level.FormationBackdrop.Display = false;
         this.timeRateModifier.ResetTimeRateMultiplier();
    this.ClearFakeBirdGoners();
    if (poem != null) {
         poem.RemoveSelf();
     }
            foreach (InvisibleBarrier invisibleBarrier in this.walls) {
     invisibleBarrier.RemoveSelf();
 }
            RemoveSelf();
        }

 private void registerAsCollected(Level level, string poemId) {
          level.Session.HeartGem = true;
      level.Session.UpdateLevelStartDashes();
        int unlockedModes = SaveData.Instance.UnlockedModes;
     SaveData.Instance.RegisterHeartGem(level.Session.Area);
     if (!string.IsNullOrEmpty(poemId)) {
        SaveData.Instance.RegisterPoemEntry(poemId);
            }
            if (unlockedModes < 3 && SaveData.Instance.UnlockedModes >= 3) {
   level.Session.UnlockedCSide = true;
  }
       if (SaveData.Instance.TotalHeartGems >= 24) {
  Achievements.Register(Achievement.CSIDES);
  }
        }

        private void ClearFakeBirdGoners() {
            foreach (FakeBirdGoner birdGoner in this.fakeBirdGoners) {
                birdGoner.RemoveSelf();
            }
            this.fakeBirdGoners.Clear();
        }

        private sealed class FakeBirdGoner : Entity {
            public FakeBirdGoner(string spriteId, Vector2 position) : base(position) {
                Depth = -2000100;
                Tag = Tags.FrozenUpdate;

                Sprite sprite = GFX.SpriteBank.Create(spriteId);
                sprite.Play("fly", false, false);
                sprite.UseRawDeltaTime = true;
                sprite.Scale.X = 1f;
                Add(sprite);
                Add(new VertexLight(Color.White, 0.5f, 8, 32));
                Add(new BloomPoint(0.5f, 12f));
            }
        }

        // Fields - these are declared at the top of the class and initialized in LoadParticles()
        private static readonly string[] FakeBirdGonerSpriteIds = new string[]
        {
            "bird",
            "birdgoner_clover",
            "birdgoner_cody",
            "birdgoner_emily",
            "birdgoner_odin",
            "birdgoner_robin",
            "birdgoner_sabel"
        };

        public bool IsGhost;
        public const float GHOST_ALPHA = 0.8f;
        public bool IsFake;
        public bool IsAstral;
        private float rainbowHue;
        private bool endLevelOnCollect;
    private Sprite sprite;
     private Sprite white;
  private ParticleType shineParticle;
        public Wiggler ScaleWiggler;
    private Wiggler moveWiggler;
        private Vector2 moveWiggleDir;
 private BloomPoint bloom;
      private VertexLight light;
    private Poem poem;
      private BirdNPC bird;
  private float timer;
        private bool collected;
 private bool autoPulse = true;
        private float bounceSfxDelay;
     private bool removeCameraTriggers;
  private SoundEmitter sfx;
     private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();
        private readonly List<FakeBirdGoner> fakeBirdGoners = new List<FakeBirdGoner>();
        private HoldableCollider holdableCollider;
        private EntityID entityId;
    private InvisibleBarrier fakeRightWall;

        // Crystal Heart variant fields
        private bool sevenbirdFlyby;
        private bool unlockDside;
        private string customSfx;

        // Sevenbird flyby routine for crystal heart variants
        private IEnumerator SevenbirdFlybyRoutine()
        {
            Level level = SceneAs<Level>();
            if (level == null) yield break;

            Vector2 cameraPosition = level.Camera.Position;

            // Create seven birds that fly across the screen
            for (int i = 0; i < FakeBirdGonerSpriteIds.Length; i++)
            {
                Vector2 start = cameraPosition + new Vector2(-20f - i * 16f, 60f + i * 14f);
                FakeBirdGoner bird = new FakeBirdGoner(FakeBirdGonerSpriteIds[i], start);
                this.fakeBirdGoners.Add(bird);
                level.Add(bird);
            }

            // Animate birds flying across
            for (float p = 0f; p < 1f; p += Engine.RawDeltaTime / 1.5f)
            {
                for (int i = 0; i < this.fakeBirdGoners.Count; i++)
                {
                    FakeBirdGoner bird = this.fakeBirdGoners[i];
                    Vector2 start = cameraPosition + new Vector2(-20f - i * 16f, 60f + i * 14f);
                    Vector2 end = cameraPosition + new Vector2(340f + i * 10f, 40f + i * 10f);
                    float birdProgress = Math.Clamp((p - i * 0.04f) / 0.65f, 0f, 1f);
                    float eased = Ease.CubeInOut(birdProgress);
                    bird.Position = start + (end - start) * eased + Vector2.UnitY * (float)Math.Sin((double)(birdProgress * 8f + i)) * 8f;

                    if (birdProgress > 0f && level.OnRawInterval(0.2f))
                    {
                        TrailManager.Add(bird, Calc.HexToColor("639bff"), 1f, true, true);
                    }
                }
                yield return null;
            }

            ClearFakeBirdGoners();
            yield return 0.3f;
        }

        private bool CheckAllCrystalHeartsCollected()
        {
            var saveData = MaggyHelperModule.SaveData;
            if (saveData == null) return false;

            int collectedCount = 0;
            for (int areaId = 1; areaId <= 21; areaId++)
            {
                for (int mode = 0; mode <= 3; mode++)
                {
                    string heartId = $"crystalheart_{areaId}_{mode}_";
                    for (int id = 0; id < 100; id++)
                    {
                        if (saveData.HasCollectedHeartGem(heartId + id))
                        {
                            collectedCount++;
                            break;
                        }
                    }
                }
            }
            return collectedCount >= 18;
        }

        private IEnumerator ShowDsidePostcard(Level level)
        {
            var scene = new Scene();
            var snow = new HiresSnow();
            scene.Add(snow);

            var entity = new Entity();
            entity.Add(new Coroutine(DsidePostcardRoutine(scene)));
            scene.Add(entity);

            Engine.Scene = scene;
            yield return null;
        }

        private IEnumerator DsidePostcardRoutine(Scene scene)
        {
            yield return 0.5f;

            string dialogText = Dialog.Get("POSTCARD_DSIDE_CRYSTAL_UNLOCK");
            if (string.IsNullOrEmpty(dialogText))
                dialogText = "D-Side Unlocked!\nPink Platinum Berries now available.";

            var postcard = new PostcardMaggy(dialogText,
                "event:/ui/pusheen/main/postcard_dsides_in",
                "event:/ui/pusheen/main/postcard_dsides_out");

            if (GFX.Gui.Has("postcards/dside_crystal_unlock"))
                postcard.Postcard = GFX.Gui["postcards/dside_crystal_unlock"];
            else if (GFX.Gui.Has("postcards/dside_unlock"))
                postcard.Postcard = GFX.Gui["postcards/dside_unlock"];

            scene.Add(postcard);
            yield return postcard.DisplayRoutine();

            yield return 0.5f;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, new HiresSnow());
        }

        private IEnumerator ShowUltraPostcard(Level level)
        {
            var scene = new Scene();
            var snow = new HiresSnow();
            scene.Add(snow);

            var entity = new Entity();
            entity.Add(new Coroutine(UltraPostcardRoutine(scene)));
            scene.Add(entity);

            Engine.Scene = scene;
            yield return null;
        }

        private IEnumerator UltraPostcardRoutine(Scene scene)
        {
            yield return 0.5f;

            string dialogText = Dialog.Get("POSTCARD_ULTRA_VARIANT_UNLOCK");
            if (string.IsNullOrEmpty(dialogText))
                dialogText = "All Crystal Hearts Collected!\nThe ultimate challenge awaits.";

            var postcard = new PostcardMaggy(dialogText,
                "event:/new_content/ui/pusheen/postcard_desolo_variants_in",
                "event:/new_content/ui/pusheen/postcard_desolo_variants_out");

            if (GFX.Gui.Has("postcards/ultra_variant_unlock"))
                postcard.Postcard = GFX.Gui["postcards/ultra_variant_unlock"];
            else if (GFX.Gui.Has("Maggy/postcard"))
                postcard.Postcard = GFX.Gui["Maggy/postcard"];

            scene.Add(postcard);

            if (!string.IsNullOrEmpty(PostcardUnlockSystem.UltraVariantConfig.UnlockMusic))
                Audio.SetMusic(PostcardUnlockSystem.UltraVariantConfig.UnlockMusic);

            yield return postcard.DisplayRoutine();

            MaggyHelperModule.SaveData?.UnlockAchievement("ultra_crystal_hearts_postcard_shown");

            yield return 0.5f;
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, new HiresSnow());
        }

        // Sevenbird inner class for crystal heart flyby (kept for compatibility)
        private sealed class Sevenbird : Entity
        {
            public Sevenbird(string spriteId, Vector2 position) : base(position)
            {
                Depth = -2000100;
                Tag = Tags.FrozenUpdate;

                Sprite sprite = GFX.SpriteBank.Create(spriteId);
                sprite.Play("fly", false, false);
                sprite.UseRawDeltaTime = true;
                sprite.Scale.X = 1f;
                Add(sprite);
                Add(new VertexLight(Color.White, 0.5f, 8, 32));
                Add(new BloomPoint(0.5f, 12f));
            }
        }

        public void OnCollect()
        {
            // Find the player in the scene
            var player = Scene?.Tracker?.GetEntity<global::Celeste.Player>();
            if (player != null && !collected)
            {
                OnPlayer(player);
            }
        }
    }
}




