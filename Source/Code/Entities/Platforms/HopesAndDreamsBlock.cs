using Celeste.Entities;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Entities
{
    /// <summary>
    /// HopesAndDreamsBlock - A special dream block that transforms the player between Madeline and Kirby
    /// When Madeline enters, she transforms into Kirby
    /// When Kirby enters, they transform back to Madeline
    /// Inspired by the "Hopes and Dreams" theme from Undertale/Deltarune
    /// </summary>
    [CustomEntity("MaggyHelper/HopesAndDreamsBlock")]
    [Tracked]
    public class HopesAndDreamsBlock : Solid
    {
        #region Constants

        private const string SFX_TRANSFORM_IN = "event:/char/pusheen/kirby/transform_in";
        private const string SFX_TRANSFORM_OUT = "event:/char/pusheen/kirby/transform_out";
        // Legacy/compatibility aliases
        private const string SFX_TRANSFORM = SFX_TRANSFORM_IN;
        private const string SFX_TRANSFORM_OUT_OLD = SFX_TRANSFORM_OUT;
        private const string SFX_DASH_ENTER = "event:/game/general/char_madeline_dreamblock_enter";
        private const string SFX_DASH_EXIT = "event:/game/general/char_madeline_dreamblock_exit";

        #endregion

        #region Fields

        private MTexture[] particleTextures;
        private HopesAndDreamsParticle[] particles;
        private float animTimer;
        private float wobbleEase;
        private float wobbleFrom;
        private float wobbleTo;

        private bool playerHasDreamDash;

        private float transformProgress;
        private bool isTransforming;
        private bool transformToKirby; // true = Madeline->Kirby, false = Kirby->Madeline
        private Vector2? node;
        private bool fastMoving;
        private bool oneUse;
        private LightOcclude occlude;
        private Level level;
        
        // Visual customization
        private Color primaryColor = Calc.HexToColor("FFD700"); // Gold
        private Color secondaryColor = Calc.HexToColor("FF69B4"); // Hot Pink (Kirby's color)
        private Color tertiaryColor = Calc.HexToColor("FF4500"); // Orange Red (Madeline's hair)
        private bool showStars = true;

        #endregion

        #region Constructor

        public HopesAndDreamsBlock(Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below) 
            : base(position, width, height, true)
        {
            Depth = below ? 5000 : -11000;
            this.node = node;
            this.fastMoving = fastMoving;
            this.oneUse = oneUse;
            SurfaceSoundIndex = 11;

            // Custom star particles for "Hopes and Dreams" theme
            particleTextures = new MTexture[]
            {
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7, null),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7, null),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7, null),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7, null)
            };
        }

        public HopesAndDreamsBlock(EntityData data, Vector2 offset) 
            : this(data.Position + offset, data.Width, data.Height, 
                   data.FirstNodeNullable(new Vector2?(offset)), 
                   data.Bool("fastMoving", false), 
                   data.Bool("oneUse", false), 
                   data.Bool("below", false))
        {
            // Load custom colors if specified
            if (!string.IsNullOrEmpty(data.Attr("primaryColor")))
                primaryColor = Calc.HexToColor(data.Attr("primaryColor", "FFD700"));
            if (!string.IsNullOrEmpty(data.Attr("secondaryColor")))
                secondaryColor = Calc.HexToColor(data.Attr("secondaryColor", "FF69B4"));
            if (!string.IsNullOrEmpty(data.Attr("tertiaryColor")))
                tertiaryColor = Calc.HexToColor(data.Attr("tertiaryColor", "FF4500"));
            showStars = data.Bool("showStars", true);
        }

        #endregion

        #region Lifecycle

        public override void Added(Scene scene)
        {
            
            base.Added(scene);
            
            level = SceneAs<Level>();

            // Setup movement if has node
            if (node != null)
            {
                Vector2 start = Position;
                Vector2 end = node.Value;
                float duration = Vector2.Distance(start, end) / 12f;
                if (fastMoving) duration /= 3f;

                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, duration, true);
                tween.OnUpdate = t =>
                {
                    if (Collidable)
                        MoveTo(Vector2.Lerp(start, end, t.Eased));
                    else
                        MoveToNaive(Vector2.Lerp(start, end, t.Eased));
                };
                Add(tween);
            }

            // Always visually active (unlike regular dream blocks)
            Add(occlude = new LightOcclude(0.5f));
            SetupParticles();
        }

        private void SetupParticles()
        {
            particles = new HopesAndDreamsParticle[(int)(Width / 8f * (Height / 8f) * 0.7f)];
            
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                particles[i].TimeOffset = Calc.Random.NextFloat();
                particles[i].IsStar = showStars && Calc.Random.Chance(0.3f);
                
                // Use custom "Hopes and Dreams" colors
                switch (particles[i].Layer)
                {
                case 0:
                    particles[i].Color = Calc.Random.Choose(primaryColor, Color.White, Calc.HexToColor("FFEF11"));
                    break;
                case 1:
                    particles[i].Color = Calc.Random.Choose(secondaryColor, Calc.HexToColor("5fcde4"), tertiaryColor);
                    break;
                case 2:
                    particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), secondaryColor, Calc.HexToColor("7daa64"));
                    break;
                }
            }
        }

        public override void Update()
        {
			base.Update();
			if (this.playerHasDreamDash)
			{
				this.animTimer += 6f * Engine.DeltaTime;
				this.wobbleEase += Engine.DeltaTime * 2f;
				if (this.wobbleEase > 1f)
				{
					this.wobbleEase = 0f;
					this.wobbleFrom = this.wobbleTo;
					this.wobbleTo = Calc.Random.NextFloat(6.2831855f);
				}
				this.SurfaceSoundIndex = 12;
                return;
            }
            animTimer += 6f * Engine.DeltaTime;
            wobbleEase += Engine.DeltaTime * 2f;
            
            if (wobbleEase > 1f)
            {
                wobbleEase = 0f;
                wobbleFrom = wobbleTo;
                wobbleTo = Calc.Random.NextFloat(MathF.PI * 2f);
            }

            SurfaceSoundIndex = 12;

            // Check for player entering
            CheckPlayerTransform();
            
            // Check for Kirby entering
            CheckKirbyTransform();

            // Update transform progress
            if (isTransforming)
            {
                transformProgress += Engine.DeltaTime * 2f;
                if (transformProgress >= 1f)
                {
                    CompleteTransformation();
                }
            }
        }

        #endregion

        #region Transformation Logic

        private void CheckPlayerTransform()
        {
            if (isTransforming) return;

            // Check for player dashing into the block (not just inside)
            foreach (global::Celeste.Player player in Scene.Tracker.GetEntities<global::Celeste.Player>())
            {
                if (player.DashAttacking && !player.IsKirbyPlayerMode())
                {
                    // Check if player's hitbox is colliding with the block, or if the player is about to enter
                    // Use a simple float rectangle struct for hitbox checks
                    var playerHitbox = player.Collider != null
                        ? new FloatRect(player.Collider.Bounds.Left, player.Collider.Bounds.Top, player.Collider.Bounds.Width, player.Collider.Bounds.Height)
                        : new FloatRect(player.X, player.Y, 8, 8);
                    var blockHitbox = new FloatRect(X, Y, Width, Height);
                    // If player is colliding or just about to enter (moving towards block)
                    if (playerHitbox.Intersects(blockHitbox) ||
                        blockHitbox.Contains(player.Position + player.Speed * Engine.DeltaTime))
                    {
                        StartTransformation(player, true);
                        break;
                    }
                }
            }
        }

        private void CheckKirbyTransform()
        {
            if (isTransforming) return;

            var player = CollideFirst<global::Celeste.Player>();
            if (player != null && player.IsKirbyPlayerMode() && player.DashAttacking)
            {
                StartTransformation(player, false);
            }
        }

        private void StartTransformation(global::Celeste.Player player, bool toKirby)
        {
            isTransforming = true;
            transformToKirby = toKirby;
            transformProgress = 0f;

            // Play transformation start sound
            Audio.Play(SFX_DASH_ENTER, Position);
            Audio.Play(SFX_TRANSFORM_IN, player.Position);

            // Create visual burst
            CreateTransformBurst(player.Position);

            // Freeze player briefly during transformation
            if (toKirby)
            {
                player.Speed = Vector2.Zero;
                player.StateMachine.State = global::Celeste.Player.StDummy;
            }

            IngesteLogger.Info($"Starting transformation: {(toKirby ? "Madeline -> Kirby" : "Kirby -> Madeline")}");
        }

        private void CompleteTransformation()
        {
            isTransforming = false;
            transformProgress = 0f;

            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null) return;

            if (transformToKirby)
            {
                // Transform Madeline to Kirby
                player.EnableKirbyPlayerMode();
                
                // Play completion effects
                Audio.Play(SFX_DASH_EXIT, Position);
                CreateTransformBurst(player.Position, true);

                IngesteLogger.Info("Transformation complete: Now Kirby!");
            }
            else
            {
                // Transform Kirby back to Madeline
                player.DisableKirbyPlayerMode();
                
                // Restore player control
                if (player.StateMachine.State == global::Celeste.Player.StDummy)
                {
                    player.StateMachine.State = global::Celeste.Player.StNormal;
                }
                
                // Play completion effects
                Audio.Play(SFX_TRANSFORM_OUT, Position);
                CreateTransformBurst(player.Position, false);

                IngesteLogger.Info("Transformation complete: Now Madeline!");
            }

            // Handle one-use destruction
            if (oneUse)
            {
                OneUseDestroy();
            }
        }

        private void CreateTransformBurst(Vector2 position, bool? toKirby = null)
        {
            if (level == null) return;

            Color burstColor = toKirby.HasValue 
                ? (toKirby.Value ? secondaryColor : tertiaryColor) 
                : primaryColor;

            // Star burst particles
            for (int i = 0; i < 30; i++)
            {
                float angle = Calc.Random.NextFloat(MathF.PI * 2f);
                float speed = Calc.Random.Range(40f, 120f);
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                
                level.ParticlesFG?.Emit(
                    ParticleTypes.SparkyDust, 
                    position + velocity.SafeNormalize() * 8f, 
                    burstColor, 
                    angle
                );
            }

            // Screen shake
            level.Shake(0.2f);
        }

        private void OneUseDestroy()
        {
            Collidable = false;
            Visible = false;
            DisableStaticMovers();
            
            // Fade out effect
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, true);
            tween.OnComplete = t => RemoveSelf();
            Add(tween);
        }

        #endregion

        #region Rendering

        public override void Render()
        {
            Camera camera = level?.Camera;
            if (camera == null)
            {
                base.Render();
                return;
            }

            // Draw block background with transformation effect
            float wobble = MathHelper.Lerp(wobbleFrom, wobbleTo, Ease.SineInOut(wobbleEase));
            
            // Background with gradient based on transformation state
            Color bgColor = isTransforming 
                ? Color.Lerp(Color.Black, transformToKirby ? secondaryColor : tertiaryColor, transformProgress * 0.5f)
                : Color.Black * 0.8f;
            
            Draw.Rect(X, Y, Width, Height, bgColor);

            // Draw particles
            Vector2 cameraPos = camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                ref HopesAndDreamsParticle particle = ref particles[i];
                int layer = particle.Layer;
                Vector2 particlePos = Position + particle.Position;
                particlePos += cameraPos * (0.3f + 0.25f * layer);
                
                // Wrap particles
                particlePos.X = Mod(particlePos.X - X, Width) + X;
                particlePos.Y = Mod(particlePos.Y - Y, Height) + Y;

                Color color = particle.Color;
                if (isTransforming)
                {
                    // Pulse particles during transformation
                    float pulse = MathF.Sin((animTimer + particle.TimeOffset) * 4f) * 0.5f + 0.5f;
                    color = Color.Lerp(particle.Color, Color.White, pulse * transformProgress);
                }

                MTexture texture = particleTextures[layer];
                
                if (particlePos.X >= X && particlePos.Y >= Y && 
                    particlePos.X < X + Width && particlePos.Y < Y + Height)
                {
                    // Draw star or circle
                    if (particle.IsStar)
                    {
                        float starScale = 0.5f + MathF.Sin(animTimer * 2f + particle.TimeOffset * MathF.PI * 2f) * 0.2f;
                        texture.DrawCentered(particlePos, color, starScale);
                    }
                    else
                    {
                        texture.DrawCentered(particlePos, color);
                    }
                }
            }

            // Draw border with glow effect
            DrawBorder(wobble);
        }

        private void DrawBorder(float wobble)
        {
            Color borderColor = isTransforming 
                ? Color.Lerp(primaryColor, Color.White, MathF.Sin(animTimer * 8f) * 0.5f + 0.5f)
                : primaryColor;

            // Wobbling border effect
            float borderWidth = 2f + MathF.Sin(wobble) * 0.5f;
            
            // Top
            Draw.Rect(X - 1, Y - 1, Width + 2, borderWidth, borderColor);
            // Bottom
            Draw.Rect(X - 1, Y + Height - borderWidth + 1, Width + 2, borderWidth, borderColor);
            // Left
            Draw.Rect(X - 1, Y, borderWidth, Height, borderColor);
            // Right
            Draw.Rect(X + Width - borderWidth + 1, Y, borderWidth, Height, borderColor);
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        #endregion

        #region Particle Structure

        private struct HopesAndDreamsParticle
        {
            public Vector2 Position;
            public int Layer;
            public Color Color;
            public float TimeOffset;
            public bool IsStar;
        }

        #endregion

        // Simple float rectangle struct for collision checks
        private struct FloatRect {
            public float X, Y, Width, Height;
            public FloatRect(float x, float y, float width, float height) {
                X = x; Y = y; Width = width; Height = height;
            }
            public bool Intersects(FloatRect other) {
                return !(other.X > X + Width || other.X + other.Width < X ||
                         other.Y > Y + Height || other.Y + other.Height < Y);
            }
            public bool Contains(Vector2 point) {
                return point.X >= X && point.X <= X + Width &&
                       point.Y >= Y && point.Y <= Y + Height;
            }
        }
    }
}
