using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Madeline God Dummy - Ascended form with rainbow hair and tentacles.
    /// Assists the player (Kirby) against the true final boss "Els".
    /// </summary>
    [CustomEntity(ids: "DZ/MadelineGodDummy")]
    [HotReloadable]
    [Tracked]
    public class MadelineGodDummy : Entity
    {
        // Particle effects
        public static ParticleType P_GodAura;
        public static ParticleType P_RainbowBurst;

        private const float DEFAULT_FLOAT_SPEED = 160f;
        private const float DEFAULT_FLOAT_ACCEL = 320f;
        private const float DEFAULT_FLOATNESS = 3f;
        private const float DEFAULT_SINE_WAVE_RATE = 0.35f;
        private const int DEFAULT_LIGHT_START_RADIUS = 40;
        private const int DEFAULT_LIGHT_END_RADIUS = 120;
        private const float ATTACK_COOLDOWN = 1.2f;
        private const float ATTACK_RANGE = 400f;
        private const int ATTACK_DAMAGE = 1;

        public Sprite Sprite { get; private set; }
        public Image HairImage { get; private set; }
        public DZBadelineAutoAnimator AutoAnimator { get; private set; }
        public SineWave Wave { get; private set; }
        public VertexLight Light { get; private set; }
        public BloomPoint Bloom { get; private set; }

        public float FloatSpeed { get; set; } = DEFAULT_FLOAT_SPEED;
        public float FloatAccel { get; set; } = DEFAULT_FLOAT_ACCEL;
        public float Floatness { get; set; } = DEFAULT_FLOATNESS;

        private Vector2 floatNormal = Vector2.UnitY;
        private bool isInitialized = false;
        internal float Float;

        // Rainbow hair / aura
        private float rainbowTimer = 0f;
        private float hairBobTimer = 0f;

        // Tentacle system
        private GodTentacle[] tentacles;
        private const int TENTACLE_COUNT = 6;
        private float tentacleTimer = 0f;

        // Combat
        private float attackTimer = 0f;
        private bool combatActive = false;
        private Entity targetBoss;

        static MadelineGodDummy()
        {
            P_GodAura = new ParticleType
            {
                Size = 2f,
                Color = Color.White,
                Color2 = Color.Gold,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1.2f,
                SpeedMin = 8f,
                SpeedMax = 24f,
                DirectionRange = (float)Math.PI * 2f
            };

            P_RainbowBurst = new ParticleType
            {
                Size = 3f,
                Color = Color.White,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.3f,
                LifeMax = 0.8f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI
            };
        }

        public MadelineGodDummy(Vector2 position) : base(position)
        {
            try
            {
                Collider = new Hitbox(8f, 8f, -4f, -8f);
                InitializeSprite();
                InitializeHair();
                InitializeComponents();
                InitializeLight();
                InitializeWaveSystem();
                InitializeTentacles();
                InitializeBloom();

                isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Failed to initialize: {ex}");
                SetMinimalState();
            }
        }

        public MadelineGodDummy(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        private void InitializeSprite()
        {
            try
            {
                Sprite = GFX.SpriteBank.Create("madelinegod");
            }
            catch
            {
                try
                {
                    Sprite = GFX.SpriteBank.Create("madelinegod");
                }
                catch
                {
                    Logger.Log(LogLevel.Error, "MadelineGodDummy", "Failed to create sprite from sprite bank");
                    return;
                }
            }

            if (Sprite != null)
            {
                Sprite.Play("boost");
                Sprite.Scale.X = -1f;
                Add(Sprite);
            }
        }

        private void InitializeHair()
        {
            try
            {
                MTexture hairTexture = GFX.Game["characters/madeline/hair00"];
                HairImage = new Image(hairTexture);
                HairImage.CenterOrigin();
                HairImage.Position = new Vector2(0f, -10f);
                HairImage.Color = Color.White;
                Add(HairImage);
            }
            catch
            {
                HairImage = null;
            }
        }

        private void InitializeComponents()
        {
            try
            {
                AutoAnimator = new DZBadelineAutoAnimator();
                Add(AutoAnimator);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Failed to initialize auto animator: {ex.Message}");
            }
        }

        private void InitializeLight()
        {
            try
            {
                Light = new VertexLight(
                    new Vector2(0f, -8f),
                    Color.White,
                    1.2f,
                    DEFAULT_LIGHT_START_RADIUS,
                    DEFAULT_LIGHT_END_RADIUS
                );
                Add(Light);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Failed to initialize light: {ex.Message}");
            }
        }

        private void InitializeBloom()
        {
            try
            {
                Bloom = new BloomPoint(1.5f, 20f);
                Bloom.Position = new Vector2(0f, -8f);
                Add(Bloom);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Failed to initialize bloom: {ex.Message}");
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
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Failed to initialize wave system: {ex.Message}");
            }
        }

        private void InitializeTentacles()
        {
            tentacles = new GodTentacle[TENTACLE_COUNT];
            for (int i = 0; i < TENTACLE_COUNT; i++)
            {
                tentacles[i] = new GodTentacle
                {
                    AngleOffset = (MathHelper.TwoPi / TENTACLE_COUNT) * i,
                    Length = 60f + Calc.Random.NextFloat(40f),
                    Width = 3f + Calc.Random.NextFloat(3f),
                    WaveSpeed = 2f + Calc.Random.NextFloat(2f),
                    WavePhase = Calc.Random.NextFloat(MathHelper.TwoPi)
                };
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
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Error in wave update: {ex.Message}");
            }
        }

        private void SetMinimalState()
        {
            if (Sprite == null)
            {
                try
                {
                    Sprite = GFX.SpriteBank.Create("madeline");
                    Sprite.Play("idle");
                    Add(Sprite);
                    isInitialized = true;
                }
                catch
                {
                    Logger.Log(LogLevel.Error, "MadelineGodDummy", "Critical failure: Unable to create minimal sprite");
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            FindTargetBoss();
        }

        private void FindTargetBoss()
        {
            if (Scene == null) return;

            // Try to find Els (SiamoZeroFinalBoss) or any BossActor
            targetBoss = Scene.Tracker.GetEntity<SiamoZeroFinalBoss>();
            if (targetBoss == null)
            {
                foreach (var boss in Scene.Tracker.GetEntities<BossActor>())
                {
                    if (boss is SiamoZeroFinalBoss)
                    {
                        targetBoss = boss;
                        break;
                    }
                }
            }

            combatActive = targetBoss != null;
        }

        public override void Update()
        {
            if (!isInitialized) return;

            try
            {
                base.Update();

                // Update rainbow color cycle
                rainbowTimer += Engine.DeltaTime * 2.5f;
                Color rainbowColor = GetRainbowColor(rainbowTimer);

                if (HairImage != null)
                {
                    hairBobTimer += Engine.DeltaTime * 3f;
                    HairImage.Position = new Vector2(0f, -10f + (float)Math.Sin(hairBobTimer) * 2f);
                    HairImage.Color = rainbowColor;
                }

                if (Light != null)
                {
                    Light.Color = Color.Lerp(Color.White, rainbowColor, 0.5f);
                }

                if (Sprite != null)
                {
                    Sprite.Color = Color.Lerp(Color.White, rainbowColor, 0.15f);
                }

                // Update tentacles
                tentacleTimer += Engine.DeltaTime;

                // Combat behavior
                if (combatActive && targetBoss != null)
                {
                    attackTimer -= Engine.DeltaTime;

                    if (attackTimer <= 0f)
                    {
                        float dist = (targetBoss.Center - Center).Length();
                        if (dist < ATTACK_RANGE)
                        {
                            PerformRainbowAttack();
                            attackTimer = ATTACK_COOLDOWN;
                        }
                    }
                }
                else
                {
                    // Re-check for boss periodically
                    if (Scene != null && Scene.TimeActive % 2f < Engine.DeltaTime)
                    {
                        FindTargetBoss();
                    }
                }

                // Emit aura particles
                if (Scene is Level level && Engine.Scene.OnInterval(0.15f))
                {
                    level.Particles?.Emit(P_GodAura, 1, Center, Vector2.One * 12f);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Error in Update: {ex.Message}");
            }
        }

        private void PerformRainbowAttack()
        {
            if (targetBoss == null || Scene == null) return;

            try
            {
                Vector2 targetPos = targetBoss.Center;
                Vector2 direction = (targetPos - Center).SafeNormalize();

                // Visual beam effect
                SceneAs<Level>()?.Particles?.Emit(P_RainbowBurst, 8, Center + direction * 16f, Vector2.One * 4f);

                // Deal damage if boss is a BossActor
                if (targetBoss is BossActor boss)
                {
                    boss.TakeDamage(ATTACK_DAMAGE);
                }

                // Screen shake for impact
                if (Scene is Level level)
                {
                    level.Shake(0.15f);
                }

                Audio.Play("event:/char/badeline/maddy_dreamblock_touch", Position);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MadelineGodDummy", $"Attack failed: {ex.Message}");
            }
        }

        public override void Render()
        {
            if (!isInitialized || Sprite == null) return;

            try
            {
                // Render tentacles behind the sprite
                RenderTentacles();

                // Store original render position for pixel-perfect rendering
                Vector2 originalRenderPosition = Sprite.RenderPosition;
                Sprite.RenderPosition = Sprite.RenderPosition.Floor();

                base.Render();

                // Restore original render position
                Sprite.RenderPosition = originalRenderPosition;

                // Render rainbow attack beams if attacking
                if (combatActive && targetBoss != null && attackTimer > ATTACK_COOLDOWN * 0.7f)
                {
                    RenderAttackBeam();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Error in Render: {ex.Message}");
                try
                {
                    base.Render();
                }
                catch (Exception fallbackEx)
                {
                    Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Critical render failure: {fallbackEx.Message}");
                }
            }
        }

        private void RenderTentacles()
        {
            if (tentacles == null) return;

            Vector2 origin = Center + new Vector2(0f, -4f);

            for (int i = 0; i < TENTACLE_COUNT; i++)
            {
                var tentacle = tentacles[i];
                float angle = tentacle.AngleOffset + (float)Math.Sin(tentacleTimer * tentacle.WaveSpeed + tentacle.WavePhase) * 0.4f;

                // Calculate tentacle points
                Vector2 direction = Calc.AngleToVector(angle, 1f);
                Vector2 perp = direction.Perpendicular();

                int segments = 8;
                Vector2[] points = new Vector2[segments + 1];

                for (int s = 0; s <= segments; s++)
                {
                    float t = s / (float)segments;
                    float wave = (float)Math.Sin(t * Math.PI * 3f + tentacleTimer * 4f + tentacle.WavePhase) * tentacle.Width * 0.5f;
                    float currentLength = tentacle.Length * t * (1f + 0.2f * (float)Math.Sin(tentacleTimer * 2f));

                    points[s] = origin + direction * currentLength + perp * wave * (1f - t);
                }

                // Draw tentacle segments with rainbow colors
                for (int s = 0; s < segments; s++)
                {
                    float hue = ((rainbowTimer + s * 0.15f + i * 0.3f) % (MathHelper.TwoPi)) / MathHelper.TwoPi;
                    Color segmentColor = HSVToColor(hue, 0.9f, 1.0f);
                    float thickness = tentacle.Width * (1f - s / (float)segments) + 1f;

                    Draw.Line(points[s], points[s + 1], segmentColor, thickness);
                }
            }
        }

        private void RenderAttackBeam()
        {
            if (targetBoss == null) return;

            Vector2 start = Center;
            Vector2 end = targetBoss.Center;
            float progress = 1f - (attackTimer / (ATTACK_COOLDOWN * 0.3f));
            progress = Calc.Clamp(progress, 0f, 1f);

            Vector2 currentEnd = Vector2.Lerp(start, end, progress);

            // Draw rainbow beam
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float t1 = i / (float)segments;
                float t2 = (i + 1) / (float)segments;

                Vector2 p1 = Vector2.Lerp(start, currentEnd, t1);
                Vector2 p2 = Vector2.Lerp(start, currentEnd, t2);

                float hue = ((rainbowTimer * 2f + i * 0.1f) % (MathHelper.TwoPi)) / MathHelper.TwoPi;
                Color beamColor = HSVToColor(hue, 0.8f, 1.0f) * (0.6f * progress);

                Draw.Line(p1, p2, beamColor);
            }
        }

        private Color GetRainbowColor(float time)
        {
            float hue = (time % MathHelper.TwoPi) / MathHelper.TwoPi;
            return HSVToColor(hue, 0.85f, 1.0f);
        }

        private Color HSVToColor(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return new Color(v, t, p);
                case 1: return new Color(q, v, p);
                case 2: return new Color(p, v, t);
                case 3: return new Color(p, q, v);
                case 4: return new Color(t, p, v);
                case 5: return new Color(v, p, q);
                default: return Color.White;
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

        public IEnumerator WalkTo(float targetX, float speed = 80f)
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
                level.Particles?.Emit(P_RainbowBurst, 20, Center, Vector2.One * 8f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Error in Appear: {ex.Message}");
            }
        }

        public void Vanish()
        {
            if (!isInitialized) return;

            try
            {
                Audio.Play("event:/char/badeline/disappear", Position);
                var level = SceneAs<Level>();
                level?.Particles?.Emit(P_GodAura, 12, Center, Vector2.One * 6f);
                RemoveSelf();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Error in Vanish: {ex.Message}");
                try
                {
                    RemoveSelf();
                }
                catch (Exception fallbackEx)
                {
                    Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Failed to remove self: {fallbackEx.Message}");
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
                Logger.Log(LogLevel.Error, "MadelineGodDummy", $"Error in Removed: {ex.Message}");
            }
        }

        // Simple tentacle data structure
        private struct GodTentacle
        {
            public float AngleOffset;
            public float Length;
            public float Width;
            public float WaveSpeed;
            public float WavePhase;
        }
    }
}

