namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/MadelineBandageDummy")]
    [HotReloadable]
    public class MadelineBandageDummy : Entity
    {
        public static ParticleType P_Vanish = new ParticleType
        {
            Size = 1f,
            Color = Color.White,
            Color2 = Calc.HexToColor("FF1144"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.8f,
            LifeMax = 1.4f,
            SpeedMin = 12f,
            SpeedMax = 24f,
            DirectionRange = (float)Math.PI * 2f
        };

        private const float DEFAULT_FLOAT_SPEED = 120f;
        private const float DEFAULT_FLOAT_ACCEL = 240f;
        private const float DEFAULT_FLOATNESS = 2f;
        private const float DEFAULT_SINE_WAVE_RATE = 0.25f;
        private const int DEFAULT_LIGHT_START_RADIUS = 20;
        private const int DEFAULT_LIGHT_END_RADIUS = 60;

        public Sprite Sprite { get; private set; }
        public MaggyBadelineAutoAnimator AutoAnimator { get; private set; }
        public SineWave Wave { get; private set; }
        public VertexLight Light { get; private set; }

        public float FloatSpeed { get; set; } = DEFAULT_FLOAT_SPEED;
        public float FloatAccel { get; set; } = DEFAULT_FLOAT_ACCEL;
        public float Floatness { get; set; } = DEFAULT_FLOATNESS;

        private string dialogKey;
        private TalkComponent talker;
        private Vector2 floatNormal = Vector2.UnitY;
        private bool isInitialized = false;
        internal float Float;

        public MadelineBandageDummy(Vector2 position, string dialogKey = null) : base(position)
        {
            this.dialogKey = dialogKey;

            try
            {
                Collider = new Hitbox(6f, 6f, -3f, -7f);
                InitializeSprite();
                InitializeComponents();
                InitializeLight();
                InitializeWaveSystem();

                if (!string.IsNullOrEmpty(dialogKey))
                {
                    InitializeTalkComponent();
                }

                isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Failed to initialize: {ex}");
                SetMinimalState();
            }
        }

        public MadelineBandageDummy(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        private void InitializeSprite()
        {
            try
            {
                Sprite = GFX.SpriteBank.Create("madeline_bandage");
                Sprite.Play("idle");
                Sprite.Scale.X = -1f;
                Add(Sprite);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Failed to create sprite from sprite bank: {ex.Message}");
            }
        }

        private void InitializeComponents()
        {
            try
            {
                AutoAnimator = new MaggyBadelineAutoAnimator();
                Add(AutoAnimator);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineBandageDummy", $"Failed to initialize auto animator: {ex.Message}");
            }
        }

        private void InitializeLight()
        {
            try
            {
                Light = new VertexLight(
                    new Vector2(0f, -8f),
                    Color.IndianRed,
                    1f,
                    DEFAULT_LIGHT_START_RADIUS,
                    DEFAULT_LIGHT_END_RADIUS
                );
                Add(Light);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineBandageDummy", $"Failed to initialize light: {ex.Message}");
            }
        }

        private void InitializeWaveSystem()
        {
            try
            {
                Wave = new SineWave(DEFAULT_SINE_WAVE_RATE, 0f);
                Wave.OnUpdate = OnWaveUpdate;
                Add(Wave);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineBandageDummy", $"Failed to initialize wave system: {ex.Message}");
            }
        }

        private void InitializeTalkComponent()
        {
            try
            {
                talker = new TalkComponent(
                    new Rectangle(-24, -24, 48, 48),
                    new Vector2(0f, -24f),
                    OnTalk
                );
                talker.PlayerMustBeFacing = false;
                Add(talker);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineBandageDummy", $"Failed to initialize talk component: {ex.Message}");
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (string.IsNullOrEmpty(dialogKey))
                return;

            if (Scene is Level level)
            {
                level.Add(new MiniTextbox(dialogKey));
            }
        }

        private void SetMinimalState()
        {
            if (Sprite == null)
            {
                try
                {
                    Sprite = GFX.SpriteBank.Create("madeline_bandage");
                    Sprite.Play("idle");
                    Add(Sprite);
                    isInitialized = true;
                }
                catch
                {
                    Logger.Log(LogLevel.Error, "MadelineBandageDummy", "Critical failure: Unable to create minimal sprite");
                }
            }
        }

        private void OnWaveUpdate(float waveValue)
        {
            if (Sprite == null) return;

            try
            {
                Sprite.Position = floatNormal * waveValue * Floatness;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineBandageDummy", $"Error in wave update: {ex.Message}");
            }
        }

        public void Appear(Level level, bool silent = false)
        {
            if (!isInitialized || level == null) return;

            try
            {
                if (!silent)
                {
                    Audio.Play("event:/char/badeline/appear", Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                level.Displacement?.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Error in Appear: {ex.Message}");
            }
        }

        public void Vanish()
        {
            if (!isInitialized) return;

            try
            {
                Audio.Play("event:/char/badeline/disappear", Position);
                Level level = SceneAs<Level>();
                level?.Particles?.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
                RemoveSelf();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Error in Vanish: {ex.Message}");
                RemoveSelf();
            }
        }

        public IEnumerator FloatTo(Vector2 target, int? turnAtEndTo = null, bool faceDirection = true, bool fadeLight = false, bool quickEnd = false)
        {
            if (!isInitialized || Sprite == null) yield break;

            Sprite.Play("idle");

            if (faceDirection && Math.Sign(target.X - X) != 0)
            {
                Sprite.Scale.X = Math.Sign(target.X - X);
            }

            Vector2 direction = (target - Position).SafeNormalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float currentSpeed = 0f;

            while (Position != target)
            {
                currentSpeed = Calc.Approach(currentSpeed, FloatSpeed, FloatAccel * Engine.DeltaTime);
                Position = Calc.Approach(Position, target, currentSpeed * Engine.DeltaTime);
                Floatness = Calc.Approach(Floatness, 4f, 8f * Engine.DeltaTime);
                floatNormal = Calc.Approach(floatNormal, perpendicular, Engine.DeltaTime * 12f);

                if (fadeLight && Light != null)
                {
                    Light.Alpha = Calc.Approach(Light.Alpha, 0f, Engine.DeltaTime * 2f);
                }

                yield return null;
            }

            if (quickEnd)
            {
                Floatness = DEFAULT_FLOATNESS;
            }
            else
            {
                while (Math.Abs(Floatness - DEFAULT_FLOATNESS) > 0.01f)
                {
                    Floatness = Calc.Approach(Floatness, DEFAULT_FLOATNESS, 8f * Engine.DeltaTime);
                    yield return null;
                }
                Floatness = DEFAULT_FLOATNESS;
            }

            if (turnAtEndTo.HasValue && Sprite != null)
            {
                Sprite.Scale.X = turnAtEndTo.Value;
            }
        }

        public IEnumerator WalkTo(float targetX, float speed = 64f)
        {
            if (!isInitialized || Sprite == null) yield break;

            Floatness = 0f;
            Sprite.Play("walk");

            if (Math.Sign(targetX - X) != 0)
            {
                Sprite.Scale.X = Math.Sign(targetX - X);
            }

            while (Math.Abs(X - targetX) > 0.1f)
            {
                X = Calc.Approach(X, targetX, Engine.DeltaTime * speed);
                yield return null;
            }

            X = targetX;
            Sprite.Play("idle");
        }

        public override void Update()
        {
            if (!isInitialized) return;

            try
            {
                base.Update();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Error in Update: {ex.Message}");
            }
        }

        public override void Render()
        {
            if (!isInitialized || Sprite == null) return;

            try
            {
                Vector2 originalRenderPosition = Sprite.RenderPosition;
                Sprite.RenderPosition = Sprite.RenderPosition.Floor();
                base.Render();
                Sprite.RenderPosition = originalRenderPosition;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Error in Render: {ex.Message}");
                try
                {
                    base.Render();
                }
                catch (Exception fallbackEx)
                {
                    Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Critical render failure: {fallbackEx.Message}");
                }
            }
        }

        public override void Removed(Scene scene)
        {
            try
            {
                isInitialized = false;
                base.Removed(scene);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineBandageDummy", $"Error in Removed: {ex.Message}");
            }
        }
    }
}

