namespace Celeste
{
    /// <summary>
    /// Base class for dummy/follower entities in the Ingeste mod
    /// Provides common functionality for character followers and NPCs
    /// </summary>
    [HotReloadable]
    public abstract class DummyBase : Entity
    {
        public static ParticleType PVanish;
        
        private const float tolerance = 0.01f;
        private const float float_speed = 120f;
        private const float float_accel = 240f;

        public readonly PlayerSprite Sprite;
        private readonly VertexLight light;
        public float Floatness = 2f;
        private Vector2 floatNormal = new Vector2(0.0f, 1f);

        protected DummyBase(Vector2 position, PlayerSpriteMode spriteMode) : base(position)
        {
            Collider = new Hitbox(6f, 6f, -3f, -7f);
            Sprite = new PlayerSprite(spriteMode);
            Sprite.Play("fallSlow");
            Sprite.Scale.X = -1f;
            Add(Sprite);

            // Add sine wave for floating effect
            SineWave wave = new SineWave(0.25f);
            wave.OnUpdate = f => Sprite.Position = floatNormal * f * Floatness;
            Add(wave);

            // Add footstep sounds
            Sprite.OnFrameChange = anim =>
            {
                var currentAnimationFrame = Sprite.CurrentAnimationFrame;
                if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) ||
                    (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) ||
                    (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)))
                {
                    Audio.Play("event:/pusheen/char/kirby/footstep", Position);
                }
            };

            Add(light = new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 20, 60));
        }

        /// <summary>
        /// Make the dummy appear with visual effects
        /// </summary>
        public virtual void Appear(Level level, bool silent = false)
        {
            if (!silent)
            {
                Audio.Play("event:/char/badeline/appear", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }

            level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(PVanish, 12, Center, Vector2.One * 6f);
        }

        /// <summary>
        /// Float to a target position
        /// </summary>
        public virtual IEnumerator FloatTo(
            Vector2 target,
            int? turnAtEndTo = null,
            bool faceDirection = true,
            bool fadeLight = false,
            bool quickEnd = false)
        {
            Sprite.Play("fallSlow");
            if (faceDirection && Math.Sign(target.X - X) != 0)
                Sprite.Scale.X = Math.Sign(target.X - X);

            Vector2 direction = (target - Position).SafeNormalize();
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            float speed = 0.0f;

            while (Position != target)
            {
                speed = Calc.Approach(speed, float_speed, float_accel * Engine.DeltaTime);
                Position = Calc.Approach(Position, target, speed * Engine.DeltaTime);
                Floatness = Calc.Approach(Floatness, 4f, 8f * Engine.DeltaTime);
                floatNormal = Calc.Approach(floatNormal, perp, Engine.DeltaTime * 12f);
                
                if (fadeLight)
                    light.Alpha = Calc.Approach(light.Alpha, 0.0f, Engine.DeltaTime * 2f);
                
                yield return null;
            }

            if (quickEnd)
            {
                Floatness = 2f;
            }
            else
            {
                while (Math.Abs(Floatness - 2.0) > tolerance)
                {
                    Floatness = Calc.Approach(Floatness, 2f, 8f * Engine.DeltaTime);
                    yield return null;
                }
            }

            if (turnAtEndTo.HasValue)
                Sprite.Scale.X = turnAtEndTo.Value;
        }

        /// <summary>
        /// Walk to a specific X position
        /// </summary>
        public virtual IEnumerator WalkTo(float x, float speed = 64f)
        {
            Floatness = 0.0f;
            Sprite.Play("walk");
            
            if (Math.Sign(x - X) != 0)
                Sprite.Scale.X = Math.Sign(x - X);

            while (Math.Abs(X - x) > tolerance)
            {
                X = Calc.Approach(X, x, Engine.DeltaTime * speed);
                yield return null;
            }

            Sprite.Play("idle");
        }

        /// <summary>
        /// Teleport to position instantly
        /// </summary>
        public virtual void TeleportTo(Vector2 position)
        {
            Position = position;
        }

        /// <summary>
        /// Set the facing direction
        /// </summary>
        public virtual void SetFacing(int direction)
        {
            if (direction != 0)
                Sprite.Scale.X = Math.Sign(direction);
        }

        /// <summary>
        /// Play an animation
        /// </summary>
        public virtual void PlayAnimation(string animationName)
        {
            if (Sprite.Has(animationName))
                Sprite.Play(animationName);
        }

        /// <summary>
        /// Check if the dummy is playing a specific animation
        /// </summary>
        public virtual bool IsPlayingAnimation(string animationName)
        {
            return Sprite.CurrentAnimationID == animationName;
        }

        /// <summary>
        /// Vanish with effects
        /// </summary>
        public virtual void Vanish(Level level, bool silent = false)
        {
            if (!silent)
            {
                Audio.Play("event:/char/badeline/disappear", Position);
            }

            level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(PVanish, 12, Center, Vector2.One * 6f);
            
            RemoveSelf();
        }

        public override void Render()
        {
            Vector2 renderPosition = Sprite.RenderPosition;
            Sprite.RenderPosition = Sprite.RenderPosition.Floor();
            base.Render();
            Sprite.RenderPosition = renderPosition;
        }

        /// <summary>
        /// Initialize the vanish particle type
        /// </summary>
        static DummyBase()
        {
            PVanish = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.Magenta,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                SizeRange = 0.5f,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 20f,
                SpeedMax = 60f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.UnitY * -20f
            };
        }
    }
}



