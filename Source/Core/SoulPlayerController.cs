using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Controller component that handles 8-directional top-down movement for a soul or vessel.
    /// Intended for single-image/path sequences where the player explores a static area,
    /// collides with barriers, and triggers boss cutscenes.
    /// </summary>
    public class SoulPlayerController : Component
    {
        private SoulPlayer soul;
        private Level level;

        private const float MaxSpeed = 120f;
        private const float Acceleration = 900f;
        private const float Deceleration = 800f;
        private const float DashSpeed = 280f;
        private const float DashTime = 0.15f;
        private const float DashCooldown = 0.3f;

        private Vector2 speed;
        private bool canMove = true;
        private float dashTimer;
        private float dashCooldownTimer;
        private int facingX = 1;
        private int facingY;
        private float pulseTimer;

        public bool IsActive { get; set; } = true;
        public bool IsDashing => dashTimer > 0f;
        public Vector2 Facing => new Vector2(facingX, facingY);

        private static bool _hooksLoaded = false;

        public static void Load()
        {
            if (_hooksLoaded) return;
            Logger.Log(LogLevel.Info, "DZ", "[SoulPlayerController] Loaded");
            _hooksLoaded = true;
        }

        public static void Unload()
        {
            if (!_hooksLoaded) return;
            Logger.Log(LogLevel.Info, "DZ", "[SoulPlayerController] Unloaded");
            _hooksLoaded = false;
        }

        public SoulPlayerController()
            : base(active: true, visible: true)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            soul = entity as SoulPlayer;
            if (soul == null)
            {
                throw new InvalidOperationException("SoulPlayerController must be added to a SoulPlayer entity");
            }
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            level = scene as Level;
            if (soul?.Collider == null)
            {
                soul.Collider = new Hitbox(8f, 8f, -4f, -4f);
            }
        }

        public override void Update()
        {
            base.Update();
            if (soul == null || !IsActive || !canMove)
            {
                speed = Vector2.Zero;
                return;
            }

            Vector2 input = Vector2.Zero;
            if (Input.MoveX.Value > 0) input.X = 1;
            else if (Input.MoveX.Value < 0) input.X = -1;
            if (Input.MoveY.Value > 0) input.Y = 1;
            else if (Input.MoveY.Value < 0) input.Y = -1;

            if (input.X != 0) facingX = (int)input.X;
            if (input.Y != 0) facingY = (int)input.Y;

            if (dashTimer > 0f)
            {
                dashTimer -= Engine.DeltaTime;
            }
            else if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Engine.DeltaTime;
            }

            if (Input.Dash.Pressed && dashCooldownTimer <= 0f && input != Vector2.Zero)
            {
                dashTimer = DashTime;
                dashCooldownTimer = DashCooldown;
                speed = input.SafeNormalize() * DashSpeed;
                Audio.Play("event:/char/madeline/dash_red", soul.Position);
                CreateDashEffect();
            }
            else if (!IsDashing)
            {
                float max = MaxSpeed;
                float accel = Acceleration * Engine.DeltaTime;
                float decel = Deceleration * Engine.DeltaTime;
                if (input != Vector2.Zero)
                {
                    speed = Calc.Approach(speed, input.SafeNormalize() * max, accel);
                }
                else
                {
                    speed = Calc.Approach(speed, Vector2.Zero, decel);
                }
            }

            Move(speed * Engine.DeltaTime);
            pulseTimer += Engine.DeltaTime * 3f;
        }

        private void Move(Vector2 amount)
        {
            if (soul == null) return;
            soul.Position += amount;

            // Simple barrier collision: push back out of solids
            if (soul.Collider != null)
            {
                foreach (Solid solid in soul.CollideAll<Solid>())
                {
                    if (solid is BossSoulBarrier barrier)
                    {
                        barrier.OnSoulCollide(soul);
                        continue;
                    }
                    if (!solid.Collidable) continue;

                    Vector2 push = Calc.AngleToVector((soul.Center - solid.Center).Angle(), 1f);
                    if (Math.Abs(push.X) > Math.Abs(push.Y))
                        soul.X = push.X > 0 ? solid.Right + soul.Width / 2f : solid.Left - soul.Width / 2f;
                    else
                        soul.Y = push.Y > 0 ? solid.Bottom + soul.Height / 2f : solid.Top - soul.Height / 2f;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (soul == null) return;

            float pulse = 0.7f + (float)Math.Sin(pulseTimer) * 0.3f;
            Draw.Circle(soul.Center, 6f, Color.LightCyan * pulse, 12);
            Draw.Circle(soul.Center, 4f, Color.White * 0.9f, 8);
            if (IsDashing)
            {
                Draw.Circle(soul.Center, 10f, Color.Cyan * 0.5f, 12);
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                speed = Vector2.Zero;
            }
        }

        public void Reset()
        {
            speed = Vector2.Zero;
            dashTimer = 0f;
            dashCooldownTimer = 0f;
        }

        private void CreateDashEffect()
        {
            if (level == null) return;
            level.Particles.Emit(ParticleTypes.SparkyDust, 8, soul.Center, Vector2.One * 8f);
        }
    }
}
