using DZ;
using BirdNPC = Celeste.Entities.DZBirdNPC;
using TimeRateModifier = Celeste.Mod.Entities.TimeRateModifier;

namespace Celeste.Entities 
{
    [CustomEntity("DZ/DZHeartGem")]
    [Tracked]
    [HotReloadable]
    public class DZHeartGem : Entity
    {
        private const string FAKE_HEART_FLAG = "wrong_heart";
        public static ParticleType P_BlueShine;
        public static ParticleType P_RedShine;
        public static ParticleType P_GoldShine;
        public static ParticleType P_FakeShine;
        public static ParticleType P_OrangeShine;
        public static ParticleType P_PurpleShine;
        public static ParticleType P_PinkShine;
        public static ParticleType P_WhiteShine;
        public bool IsGhost;
        public const float GhostAlpha = 0.8f;
        public bool IsFake;

        static DZHeartGem()
        {
            // Initialize Blue shine particle
            P_BlueShine = new ParticleType
            {
                Color = Color.Aqua,
                Color2 = Calc.HexToColor("00BFFF"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Red shine particle
            P_RedShine = new ParticleType
            {
                Color = Color.Red,
                Color2 = Calc.HexToColor("CC0000"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Gold shine particle
            P_GoldShine = new ParticleType
            {
                Color = Color.Gold,
                Color2 = Calc.HexToColor("DAA520"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Fake shine particle
            P_FakeShine = new ParticleType
            {
                Color = Calc.HexToColor("90A4AE"),
                Color2 = Calc.HexToColor("607D8B"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Orange shine particle
            P_OrangeShine = new ParticleType
            {
                Color = Calc.HexToColor("FFA500"),
                Color2 = Calc.HexToColor("FF8C00"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Purple shine particle
            P_PurpleShine = new ParticleType
            {
                Color = Calc.HexToColor("9932CC"),
                Color2 = Calc.HexToColor("8B008B"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize Pink shine particle
            P_PinkShine = new ParticleType
            {
                Color = Calc.HexToColor("FF69B4"),
                Color2 = Calc.HexToColor("FF1493"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };

            // Initialize White shine particle
            P_WhiteShine = new ParticleType
            {
                Color = Color.White,
                Color2 = Calc.HexToColor("F0F0F0"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.2f,
                Acceleration = Vector2.UnitY * 8f,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                RotationMode = ParticleType.RotationModes.Random,
                SpinMin = 0.5f,
                SpinMax = 1.5f
            };
        }

        private Sprite sprite;
        private Sprite white;
        private ParticleType shineParticle;
        public Wiggler ScaleWiggler;
        private Wiggler moveWiggler;
        private Vector2 moveWiggleDir;
        private BloomPoint bloom;
        private VertexLight light;
        private Soul poem;
        private BirdNPC bird;
        private List<BirdNPC> birds;
        private float timer;
        private bool collected;
        private bool autoPulse = true;
        private float bounceSfxDelay;
        private bool removeCameraTriggers;
        private SoundEmitter sfx;
        private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();
        private HoldableCollider holdableCollider;
        private EntityID entityID;
        private InvisibleBarrier fakeRightWall;

        // Extended heart gem color modes
        public enum HeartGemColor
        {
            Blue = 0,    // Normal/A-Side
            Red = 1,     // B-Side
            Gold = 2,    // C-Side
            Orange = 3,  // D-Side
            Purple = 4,  // E-Side
            Pink = 5,    // F-Side
            White = 6    // Special/Ultimate
        }

        public HeartGemColor GemColor { get; private set; }

        public DZHeartGem(Vector2 position)
            : base(position)
        {
            Add(holdableCollider = new HoldableCollider(OnHoldable));
            Add(new MirrorReflection());
        }

        public DZHeartGem(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            removeCameraTriggers = data.Bool(nameof(removeCameraTriggers));
            IsFake = data.Bool("fake");
            entityID = new EntityID(data.Level.Name, data.ID);

            // Get custom color from entity data, default to matching area mode
            string colorStr = data.Attr("color", "");
            if (!string.IsNullOrEmpty(colorStr) && Enum.TryParse<HeartGemColor>(colorStr, true, out var customColor))
            {
                GemColor = customColor;
            }
            else
            {
                // Map to area mode by default
                var mode = (Scene as Level)?.Session.Area.Mode;
                GemColor = (HeartGemColor)(mode.HasValue ? (int)mode.Value : 0);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AreaKey area = (Scene as Level).Session.Area;
            IsGhost = !IsFake && SaveData.Instance.Areas[area.ID].Modes[(int)area.Mode].HeartGem;

            // Determine sprite ID based on color
            string id;
            if (IsFake)
            {
                id = "heartGemWhite";
            }
            else if (IsGhost)
            {
                id = "heartGemGhost";
            }
            else
            {
                // Map color to sprite ID
                id = GemColor switch
                {
                    HeartGemColor.Blue => "heartgem0",
                    HeartGemColor.Red => "heartgem1",
                    HeartGemColor.Gold => "heartgem2",
                    HeartGemColor.Pink => "heartgem3",
                    // No dedicated sprite banks exist yet for Purple/Pink/White in Sprites.xml,
                    // so fall back to an existing heartgem sprite to avoid a missing-animation crash.
                    HeartGemColor.Orange => "heartgem4",  // TODO: add "heartgem4" to Sprites.xml for a custom sprite
                    HeartGemColor.Purple => "heartgem5",    // TODO: add "heartgem5" to Sprites.xml for a custom sprite
                    HeartGemColor.White => "heartGemWhite",
                    _ => "heartgem" + (int)area.Mode
                };
            }

            Add(sprite = GFX.SpriteBank.Create(id));
            sprite.Play("spin");
            sprite.OnLoop = anim =>
            {
                if (!Visible || !(anim == "spin") || !autoPulse)
                    return;
                if (IsFake)
                    Audio.Play("event:/DZ/new_content/game/19_spaces/fakeheart_pulse", Position);
                else
                    Audio.Play("event:/DZ/game/general/crystalheart_pulse", Position);
                ScaleWiggler.Start();
                (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
            };
            if (IsGhost)
                sprite.Color = Color.White * 0.8f;
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (float)(1.0 + f * 0.25)));
            Add(bloom = new BloomPoint(0.75f, 16f));

            // Determine color and shine particle based on heart gem color
            Microsoft.Xna.Framework.Color color;
            if (IsFake)
            {
                color = Calc.HexToColor("78909C");
                shineParticle = DZHeartGem.P_FakeShine;
            }
            else
            {
                switch (GemColor)
                {
                    case HeartGemColor.Blue:
                        color = Microsoft.Xna.Framework.Color.Aqua;
                        shineParticle = DZHeartGem.P_BlueShine;
                        break;
                    case HeartGemColor.Red:
                        color = Microsoft.Xna.Framework.Color.Red;
                        shineParticle = DZHeartGem.P_RedShine;
                        break;
                    case HeartGemColor.Gold:
                        color = Microsoft.Xna.Framework.Color.Gold;
                        shineParticle = DZHeartGem.P_GoldShine;
                        break;
                    case HeartGemColor.Orange:
                        color = Calc.HexToColor("FFA500");
                        shineParticle = DZHeartGem.P_OrangeShine;
                        break;
                    case HeartGemColor.Purple:
                        color = Calc.HexToColor("9932CC");
                        shineParticle = DZHeartGem.P_PurpleShine;
                        break;
                    case HeartGemColor.Pink:
                        color = Calc.HexToColor("FF69B4");
                        shineParticle = DZHeartGem.P_PinkShine;
                        break;
                    case HeartGemColor.White:
                        color = Microsoft.Xna.Framework.Color.White;
                        shineParticle = DZHeartGem.P_WhiteShine;
                        break;
                    default:
                        color = Microsoft.Xna.Framework.Color.Aqua;
                        shineParticle = DZHeartGem.P_BlueShine;
                        break;
                }
            }
            Add(light = new VertexLight(Color.Lerp(color, Color.White, 0.5f), 1f, 32, 64));
            if (IsFake)
            {
                bloom.Alpha = 0.0f;
                light.Alpha = 0.0f;
            }
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            if (!IsFake)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.X > (double)X || (scene as Level).Session.GetFlag("wrong_heart"))
            {
                Visible = false;
                Alarm.Set(this, 0.0001f, () =>
                {
                    FakeRemoveCameraTrigger();
                    RemoveSelf();
                });
            }
            else
                scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(X + 160f, Y - 200f), 8f, 400f));
        }

        public override void Update()
        {
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2.0) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
            if (white != null)
            {
                white.Position = sprite.Position;
                white.Scale = sprite.Scale;
                if (white.CurrentAnimationID != sprite.CurrentAnimationID)
                    white.Play(sprite.CurrentAnimationID);
                white.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
            if (collected)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null || entity.Dead)
                    EndCutscene();
            }
            base.Update();
            if (collected || !Scene.OnInterval(0.1f))
                return;
            if (shineParticle != null)
                SceneAs<Level>().Particles.Emit(shineParticle, 1, Center, Vector2.One * 8f);
        }

        public void OnHoldable(Holdable h)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (collected || entity == null || !h.Dangerous(holdableCollider))
                return;
            Collect(entity);
        }

        public void OnPlayer(Player player)
        {
            if (collected || (Scene as Level).Frozen)
                return;
            if (player.DashAttacking)
            {
                Collect(player);
            }
            else
            {
                if (bounceSfxDelay <= 0.0)
                {
                    if (IsFake)
                        Audio.Play("event:/DZ/new_content/game/19_spaces/fakeheart_bounce", Position);
                    else
                        Audio.Play("event:/DZ/game/general/crystalheart_bounce", Position);
                    bounceSfxDelay = 0.1f;
                }
                player.PointBounce(Center);
                moveWiggler.Start();
                ScaleWiggler.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }

        private void Collect(Player player)
        {
            Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
            Add(new Coroutine(CollectRoutine(player))
            {
                UseRawDeltaTime = true
            });
            collected = true;
            if (!removeCameraTriggers)
                return;
            foreach (Entity entity in Scene.Entities.FindAll<CameraOffsetTrigger>())
                entity.RemoveSelf();
        }

        private IEnumerator CollectRoutine(Player player)
        {
            DZHeartGem follow = this;
            Level level = follow.Scene as Level;
            AreaKey area = level.Session.Area;
            string poemID = AreaData.Get(level).Mode[(int)area.Mode].PoemID;
            bool completeArea = !follow.IsFake;
            if (follow.IsFake)
                level.StartCutscene(follow.SkipFakeHeartCutscene);
            else
                level.CanRetry = false;
            if (completeArea || follow.IsFake)
            {
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
            }
            if (completeArea)
            {
                List<Strawberry> strawberryList = new List<Strawberry>();
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry)
                        strawberryList.Add(follower.Entity as Strawberry);
                }
                foreach (Strawberry strawberry in strawberryList)
                    strawberry.OnCollect();
            }

            // Determine collection sound effect based on color
            string sfx;
            if (follow.IsFake)
            {
                sfx = "event:/DZ/new_content/game/19_spaces/fakeheart_get";
            }
            else
            {
                sfx = follow.GemColor switch
                {
                    HeartGemColor.Blue => "event:/DZ/game/general/crystalheart_blue_get",
                    HeartGemColor.Red => "event:/DZ/game/general/crystalheart_red_get",
                    HeartGemColor.Gold => "event:/DZ/game/general/crystalheart_gold_get",
                    HeartGemColor.Orange => "event:/DZ/game/general/crystalheart_astral_get",  // Using astral for orange
                    HeartGemColor.Purple => "event:/DZ/game/general/crystalheart_rainbow_get", // Using rainbow for purple
                    HeartGemColor.Pink => "event:/DZ/game/general/crystalheart_pink_get",
                    HeartGemColor.White => "event:/DZ/game/18_core/final_heart_get",           // Using final heart for white
                    _ => "event:/DZ/game/general/crystalheart_blue_get"
                };
            }

            follow.sfx = SoundEmitter.Play(sfx, follow);
            // ISSUE: reference to a compiler-generated method
            follow.Add(new LevelEndingHook(delegate
            {
                this.sfx.Source.Stop();
            }));
            follow.walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
            List<InvisibleBarrier> walls1 = follow.walls;
            Rectangle bounds = level.Bounds;
            double x = bounds.Left - 8;
            bounds = level.Bounds;
            double top = bounds.Top;
            InvisibleBarrier invisibleBarrier1 = new InvisibleBarrier(new Vector2((float)x, (float)top), 8f, level.Bounds.Height);
            walls1.Add(invisibleBarrier1);
            List<InvisibleBarrier> walls2 = follow.walls;
            bounds = level.Bounds;
            double left = bounds.Left;
            bounds = level.Bounds;
            double y = bounds.Top - 8;
            InvisibleBarrier invisibleBarrier2 = new InvisibleBarrier(new Vector2((float)left, (float)y), level.Bounds.Width, 8f);
            walls2.Add(invisibleBarrier2);
            foreach (InvisibleBarrier wall in follow.walls)
                follow.Scene.Add(wall);
            follow.Add(follow.white = GFX.SpriteBank.Create("heartGemWhite"));
            follow.Depth = -2000000;
            yield return null;
            Celeste.Freeze(0.2f);
            yield return null;
            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            for (int index = 0; index < 10; ++index)
                follow.Scene.Add(new AbsorbOrb(follow.Position));
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            follow.light.Alpha = follow.bloom.Alpha = 0.0f;
            follow.Visible = false;
            float t;
            for (t = 0.0f; t < 2.0; t += Engine.RawDeltaTime)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0.0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }
            yield return null;
            if (player.Dead)
                yield return 100f;
            Engine.TimeRate = 1f;
            follow.Tag = (int)Tags.FrozenUpdate;
            level.Frozen = true;
            if (!follow.IsFake)
            {
                follow.RegisterAsCollected(level, poemID);
                if (completeArea)
                {
                    level.TimerStopped = true;
                    level.RegisterAreaComplete();
                }
            }
            string text = null;
            if (!string.IsNullOrEmpty(poemID))
                text = Dialog.Clean("soul_" + poemID);
            int poemMode = follow.IsFake ? 3 : (follow.GemColor == HeartGemColor.Orange ? 2 : (int)area.Mode);
            int poemHeartIndex = Math.Clamp(poemMode, 0, 2);
            follow.poem = new Soul(text, poemHeartIndex, (int)area.Mode == 2 || follow.IsFake || follow.GemColor == HeartGemColor.Orange ? 1f : 0.6f);
            follow.poem.Alpha = 0.0f;
            follow.Scene.Add(follow.poem);
            for (t = 0.0f; t < 1.0; t += Engine.RawDeltaTime)
            {
                follow.poem.Alpha = Ease.CubeOut(t);
                yield return null;
            }
            if (follow.IsFake)
            {
                yield return follow.DoFakeRoutineWithBird(player);
            }
            else
            {
                while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
                    yield return null;
                follow.sfx.Source.Param("end", 1f);
                if (!completeArea)
                {
                    level.FormationBackdrop.Display = false;
                    for (t = 0.0f; t < 1.0; t += Engine.RawDeltaTime * 2f)
                    {
                        follow.poem.Alpha = Ease.CubeIn(1f - t);
                        yield return null;
                    }
                    player.Depth = 0;
                    follow.EndCutscene();
                }
                else if (follow.GemColor == HeartGemColor.Purple)
                {
                    // Purple (E-Side) heart gem: reveal Morpho Knight Delta and
                    // fight it immediately instead of completing the area.
                    // TODO: area completion after the boss fight is not wired
                    // up yet -- MorphoKnightDeltaBoss.DefeatSequence() only
                    // sets a session flag for now.
                    level.FormationBackdrop.Display = false;
                    for (t = 0.0f; t < 1.0; t += Engine.RawDeltaTime * 2f)
                    {
                        follow.poem.Alpha = Ease.CubeIn(1f - t);
                        yield return null;
                    }
                    player.Depth = 0;
                    follow.EndCutscene();
                    level.Add(new global::Celeste.Cutscenes.CS_MorphoKnightReveal(player, follow.Position));
                }
                else
                {
                    FadeWipe fadeWipe = new FadeWipe(level, false);
                    fadeWipe.Duration = 3.25f;
                    yield return fadeWipe.Duration;
                    level.CompleteArea(false, true);
                }
            }
        }

        private void EndCutscene()
        {
            Level scene = Scene as Level;
            scene.Frozen = false;
            scene.CanRetry = true;
            scene.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (poem != null)
                poem.RemoveSelf();
            foreach (Entity wall in walls)
                wall.RemoveSelf();
            RemoveSelf();
        }

        private void RegisterAsCollected(Level level, string poemID)
        {
            level.Session.HeartGem = true;
            level.Session.UpdateLevelStartDashes();
            int unlockedModes = SaveData.Instance.UnlockedModes;
            SaveData.Instance.RegisterHeartGem(level.Session.Area);
            if (!string.IsNullOrEmpty(poemID))
                SaveData.Instance.RegisterPoemEntry(poemID);
            // Show pink heart gem postcard for F-Side instead of C-side unlock
            if (GemColor == HeartGemColor.Pink)
                level.Session.UnlockedCSide = true;
            if (SaveData.Instance.TotalHeartGems < 86)
                return;
            //Achievements.Register(Achievement.CSIDES);
        }

        private IEnumerator DoFakeRoutineWithBird(Player player)
        {
            DZHeartGem heartGem = this;
            Level level = heartGem.Scene as Level;
            int panAmount = 64;
            Vector2 panFrom = level.Camera.Position;
            Vector2 panTo = level.Camera.Position + new Vector2(-panAmount, 0.0f);
            Vector2 birdFrom = new Vector2(panTo.X - 16f, player.Y - 20f);
            Vector2 birdTo = new Vector2((float)(panFrom.X + 320.0 + 16.0), player.Y - 20f);
            yield return 2f;

            // Short glitch burst
            Glitch.Value = 1.0f;
            Distort.Anxiety = 1.0f;
            float glitchTimer = 0f;
            while (glitchTimer < 0.4f)
            {
                glitchTimer += Engine.RawDeltaTime;
                float t = 1f - glitchTimer / 0.4f;
                Glitch.Value = Calc.Random.Range(0.6f, 1.0f) * t;
                Distort.Anxiety = Glitch.Value;
                if (level.OnRawInterval(0.08f))
                {
                    level.DirectionalShake(Vector2.UnitX * Calc.Random.Choose(-1f, 1f), 0.3f);
                    level.Flash(Calc.Random.Choose(Color.Red, Color.Black, Color.White) * 0.25f);
                }
                level.Shake(Glitch.Value * 0.5f);
                yield return null;
            }

            Glitch.Value = 0f;
            Distort.Anxiety = 0f;
            yield return 0.1f;
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 2f)
            {
                level.Camera.Position = panFrom + (panTo - panFrom) * Ease.CubeInOut(p);
                heartGem.poem.Offset = new Vector2(panAmount * 8, 0.0f) * Ease.CubeInOut(p);
                yield return null;
            }

            // Create seven birds at different positions
            heartGem.birds = new List<BirdNPC>();
            float[] birdOffsets = new float[] { -60f, -40f, -20f, 0f, 20f, 40f, 60f };

            for (int i = 0; i < 7; i++)
            {
                Vector2 thisBirdFrom = new Vector2(panTo.X - 16f, player.Y - 20f + birdOffsets[i]);
                BirdNPC newBird = new BirdNPC(thisBirdFrom, BirdNPC.Modes.None);
                newBird.Sprite.Play("fly");
                newBird.Sprite.UseRawDeltaTime = true;
                newBird.Facing = Facings.Right;
                newBird.Depth = -2000100 - i; // Slightly different depths for layering
                newBird.Tag = (int)Tags.FrozenUpdate;
                newBird.Add(new VertexLight(Color.White, 0.5f, 8, 32));
                newBird.Add(new BloomPoint(0.5f, 12f));
                heartGem.birds.Add(newBird);
                level.Add(newBird);
            }

            // Keep original single bird for compatibility
            heartGem.bird = heartGem.birds[6]; // Use the middle bird as the main reference
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 2.6f)
            {
                level.Camera.Position = panTo + (panFrom - panTo) * Ease.CubeInOut(p);
                heartGem.poem.Offset = new Vector2(panAmount * 8, 0.0f) * Ease.CubeInOut(1f - p);
                float num1 = 0.1f;
                float num2 = 0.9f;
                if (p > (double)num1 && p <= (double)num2)
                {
                    float num3 = (float)((p - (double)num1) / (num2 - (double)num1));

                    // Update all seven birds with slightly different wave patterns
                    for (int i = 0; i < heartGem.birds.Count; i++)
                    {
                        Vector2 thisBirdFrom = heartGem.birds[i].StartPosition;
                        Vector2 thisBirdTo = new Vector2((float)(panFrom.X + 320.0 + 16.0), thisBirdFrom.Y);
                        float waveOffset = i * 1.2f; // Each bird has a different wave phase
                        heartGem.birds[i].Position = thisBirdFrom + (thisBirdTo - thisBirdFrom) * num3 + Vector2.UnitY * (float)Math.Sin(num3 * 8.0 + waveOffset) * 8f;
                    }
                }
                if (level.OnRawInterval(0.2f))
                {
                    // Add trails to all birds
                    foreach (var bird in heartGem.birds)
                    {
                        TrailManager.Add(bird, Calc.HexToColor("639bff"), frozenUpdate: true, useRawDeltaTime: true);
                    }
                }
                yield return null;
            }

            // Remove all birds
            foreach (var bird in heartGem.birds)
            {
                bird.RemoveSelf();
            }
            heartGem.birds.Clear();
            heartGem.bird = null;
            Engine.TimeRate = 0.0f;
            level.Frozen = false;
            player.Active = false;
            player.StateMachine.State = 11;
            while (Engine.TimeRate != 1.0)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 0.5f * Engine.RawDeltaTime);
                yield return null;
            }
            Engine.TimeRate = 1f;
            yield return Textbox.Say("DZ_CH19_WRONG_HEART");
            heartGem.sfx.Source.Param("end", 1f);
            yield return 0.283f;
            level.FormationBackdrop.Display = false;
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 0.2f)
            {
                heartGem.poem.TextAlpha = Ease.CubeIn(1f - p);
                heartGem.poem.ParticleSpeed = heartGem.poem.TextAlpha;
                yield return null;
            }
            heartGem.poem.Heart.Play("break");
            while (heartGem.poem.Heart.Animating)
            {
                heartGem.poem.Shake += Engine.DeltaTime;
                yield return null;
            }
            heartGem.poem.RemoveSelf();
            heartGem.poem = null;

            // Intense creepy glitch breakdown
            Glitch.Value = 1.0f;
            Distort.Anxiety = 1.0f;
            level.DirectionalShake(Vector2.UnitY, 1.0f);
            level.Flash(Color.Black, true);
            Audio.Play("event:/new_content/game/10_farewell/glitch_short");
            yield return 0.15f;

            // Rapid glitch pulses
            for (int pulse = 0; pulse < 5; ++pulse)
            {
                Glitch.Value = Calc.Random.Range(0.8f, 1.0f);
                level.DirectionalShake(Calc.Random.Choose(Vector2.UnitX, -Vector2.UnitX) * Calc.Random.Range(1f, 2f), 0.2f);
                level.Flash(Color.Red * Calc.Random.Range(0.3f, 0.5f));
                yield return 0.05f;
                Glitch.Value = 0f;
                yield return 0.03f;
            }

            for (int index = 0; index < 10; ++index)
            {
                Vector2 position = level.Camera.Position + new Vector2(320f, 180f) * 0.5f;
                Vector2 vector2 = level.Camera.Position + new Vector2(160f, -64f);
                heartGem.Scene.Add(new AbsorbOrb(position, absorbTarget: vector2));
            }

            level.Shake(1.5f);
            Glitch.Value = 1.0f;
            float glitchDecay = 1.0f;
            float chaosTimer = 0f;
            while (Glitch.Value > 0.0)
            {
                chaosTimer += Engine.DeltaTime;

                // Chaotic visual distortion
                float noise = (float)Math.Sin(chaosTimer * 40f) * 0.3f;
                Glitch.Value = glitchDecay + noise;
                Distort.Anxiety = Glitch.Value * 0.8f;

                // Random screen tears and flashes
                if (level.OnInterval(0.1f))
                {
                    level.Flash(Calc.Random.Choose(Color.Black, Color.Red, Color.White) * 0.4f);
                    level.DirectionalShake(new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)), 0.3f);
                }

                // Jittery camera
                level.Camera.Position += new Vector2(
                    (float)Math.Sin(chaosTimer * 60f) * 8f * Glitch.Value,
                    (float)Math.Cos(chaosTimer * 45f) * 6f * Glitch.Value
                );

                glitchDecay -= Engine.DeltaTime * 2f;
                if (glitchDecay <= 0f)
                    break;

                yield return null;
            }

            Glitch.Value = 0f;
            Distort.Anxiety = 0f;
            yield return 0.25f;
            level.Session.Audio.Music.Event = "event:/DZ/new_content/music/lvl19/intermission_inmyway";
            level.Session.Audio.Apply();
            player.Active = true;
            player.Depth = 0;
            player.StateMachine.State = 11;
            while (!player.OnGround() && player.Bottom < (double)level.Bounds.Bottom)
                yield return null;
            player.Facing = Facings.Right;
            yield return 0.5f;
            yield return Textbox.Say("DZ_CH19_KEEP_GOING_KIRBY", heartGem.PlayerStepForward);
            heartGem.SkipFakeHeartCutscene(level);
            level.EndCutscene();
        }

        private IEnumerator PlayerStepForward()
        {
            DZHeartGem heartGem = this;
            yield return 0.1f;
            Player entity = heartGem.Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.CollideCheck<Solid>(entity.Position + new Vector2(12f, 1f)))
                yield return entity.DummyWalkToExact((int)entity.X + 10);
            yield return 0.2f;
        }

        private void SkipFakeHeartCutscene(Level level)
        {
            Engine.TimeRate = 1f;
            Glitch.Value = 0.0f;
            if (sfx != null)
                sfx.Source.Stop();
            level.Session.SetFlag("wrong_heart");
            level.Frozen = false;
            level.FormationBackdrop.Display = false;
            level.Session.Audio.Music.Event = "event:/DZ/new_content/music/lvl19/intermission_inmyway";
            level.Session.Audio.Apply();
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            if (entity1 != null)
            {
                entity1.Sprite.Play("idle");
                entity1.Active = true;
                entity1.StateMachine.State = 0;
                entity1.Dashes = 1;
                entity1.Speed = Vector2.Zero;
                entity1.MoveV(200f);
                entity1.Depth = 0;
                for (int index = 0; index < 10; ++index)
                    entity1.UpdateHair(true);
            }
            foreach (Entity entity2 in Scene.Entities.FindAll<AbsorbOrb>())
                entity2.RemoveSelf();
            if (poem != null)
                poem.RemoveSelf();
            if (bird != null)
                bird.RemoveSelf();
            if (fakeRightWall != null)
                fakeRightWall.RemoveSelf();
            FakeRemoveCameraTrigger();
            foreach (Entity wall in walls)
                wall.RemoveSelf();
            RemoveSelf();
        }

        private void FakeRemoveCameraTrigger()
        {
            CameraTargetTrigger cameraTargetTrigger = CollideFirst<CameraTargetTrigger>();
            if (cameraTargetTrigger == null)
                return;
            cameraTargetTrigger.LerpStrength = 0.0f;
        }
    }
}
