using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/MainCharaVisionActor")]
    public class MainCharaVisionActor : Actor
    {
        public enum FacingDirection
        {
            Down,
            Right,
            Up,
            Left
        }

        private const string PreferredSpriteBank = "maggy_mainchara_vision";
        private const string FallbackSpriteBank = "maggy_chara";
        private const float DefaultMaxMoveSpeed = 90f;
        private const float DefaultAcceleration = 650f;
        private const float DefaultFriction = 800f;

        public Sprite Sprite { get; private set; }
        public FacingDirection Facing { get; private set; } = FacingDirection.Down;
        public bool GenocideMode { get; private set; }
        public bool PlayerControlled { get; private set; }
        public bool ClampToRoomBounds { get; set; } = true;
        public bool DriveCameraWhenControlled { get; set; } = true;
        public Vector2 Speed { get; private set; }
        public float MaxMoveSpeed { get; set; } = DefaultMaxMoveSpeed;
        public float Acceleration { get; set; } = DefaultAcceleration;
        public float Friction { get; set; } = DefaultFriction;
        public bool ConfirmPressed => MInput.Keyboard.Pressed(Keys.Z);
        public bool AttackPressed => MInput.Keyboard.Pressed(Keys.X);
        public Vector2 CameraTarget
        {
            get
            {
                if (Scene is not Level level)
                {
                    return Position;
                }

                Rectangle bounds = level.Bounds;
                return (Position + new Vector2(-160f, -90f)).Clamp(bounds.Left, bounds.Top, bounds.Right - 320, bounds.Bottom - 180);
            }
        }

        public MainCharaVisionActor(Vector2 position, bool genocideMode = true)
            : base(position)
        {
            Collider = new Hitbox(8f, 10f, -4f, -10f);
            Depth = 10;
            GenocideMode = genocideMode;

            try
            {
                Sprite = GFX.SpriteBank.Create(PreferredSpriteBank);
            }
            catch
            {
                try
                {
                    Sprite = GFX.SpriteBank.Create(FallbackSpriteBank);
                }
                catch
                {
                    Sprite = null;
                }
            }

            if (Sprite != null)
            {
                Add(Sprite);
                PlayIdle();
            }
        }

        public MainCharaVisionActor(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("genocideMode", true))
        {
            ClampToRoomBounds = data.Bool("clampToRoomBounds", true);
            DriveCameraWhenControlled = data.Bool("driveCameraWhenControlled", true);
            MaxMoveSpeed = data.Float("maxMoveSpeed", DefaultMaxMoveSpeed);
            Acceleration = data.Float("acceleration", DefaultAcceleration);
            Friction = data.Float("friction", DefaultFriction);
            Depth = data.Int("depth", Depth);

            Face(ParseFacing(data.Attr("facing", "down")));
            EnablePlayerControl(data.Bool("playerControlled", false));
        }

        public void Face(FacingDirection direction)
        {
            Facing = direction;

            if (Sprite == null)
            {
                return;
            }

            Sprite.Scale.X = Math.Abs(Sprite.Scale.X == 0f ? 1f : Sprite.Scale.X);
        }

        public void EnablePlayerControl(bool enabled = true)
        {
            PlayerControlled = enabled;
            if (!enabled)
            {
                Speed = Vector2.Zero;
                PlayIdle();
            }
        }

        public void PlayIdle()
        {
            TryPlay(BuildCandidates("idle"));
        }

        public void PlayWalk()
        {
            TryPlay(BuildCandidates("walk"), BuildCandidates("run"), new[] { "runFast", "walk", "idle" });
        }

        public void PlayPickup()
        {
            TryPlay(BuildCandidates("pickUp"), BuildCandidates("pickup"), new[] { "pickUp", "pickup", "idle" });
        }

        public void PlayAttack()
        {
            TryPlay(BuildCandidates("slash"), BuildCandidates("attack"), new[] { "lookUp", "idle" });
        }

        public IEnumerator MoveTo(Vector2 target, float speed = 48f)
        {
            while (Position != target)
            {
                Vector2 delta = target - Position;

                if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                {
                    Face(delta.X >= 0f ? FacingDirection.Right : FacingDirection.Left);
                }
                else if (Math.Abs(delta.Y) > 0.01f)
                {
                    Face(delta.Y >= 0f ? FacingDirection.Down : FacingDirection.Up);
                }

                PlayWalk();
                Vector2 next = Calc.Approach(Position, target, speed * Engine.DeltaTime);
                Vector2 movement = next - Position;
                MoveH(movement.X, new Collision(OnCollide), null);
                MoveV(movement.Y, new Collision(OnCollide), null);
                ClampInsideLevel();
                yield return null;
            }

            Speed = Vector2.Zero;
            PlayIdle();
        }

        public override void Update()
        {
            base.Update();

            if (!PlayerControlled)
            {
                return;
            }

            Vector2 aim = ReadMovementInput();
            if (aim.LengthSquared() > 1f)
            {
                aim.Normalize();
            }

            if (Math.Abs(aim.X) > Math.Abs(aim.Y))
            {
                if (Math.Abs(aim.X) > 0.01f)
                {
                    Face(aim.X > 0f ? FacingDirection.Right : FacingDirection.Left);
                }
            }
            else if (Math.Abs(aim.Y) > 0.01f)
            {
                Face(aim.Y > 0f ? FacingDirection.Down : FacingDirection.Up);
            }

            Speed += aim * Acceleration * Engine.DeltaTime;
            if (Speed.LengthSquared() > MaxMoveSpeed * MaxMoveSpeed)
            {
                Speed = Speed.SafeNormalize(MaxMoveSpeed);
            }

            if (Math.Abs(aim.X) <= 0.01f)
            {
                Speed = new Vector2(Calc.Approach(Speed.X, 0f, Friction * Engine.DeltaTime), Speed.Y);
            }

            if (Math.Abs(aim.Y) <= 0.01f)
            {
                Speed = new Vector2(Speed.X, Calc.Approach(Speed.Y, 0f, Friction * Engine.DeltaTime));
            }

            MoveH(Speed.X * Engine.DeltaTime, new Collision(OnCollide), null);
            MoveV(Speed.Y * Engine.DeltaTime, new Collision(OnCollide), null);
            ClampInsideLevel();

            if (DriveCameraWhenControlled && Scene is Level level)
            {
                level.Camera.Position += (CameraTarget - level.Camera.Position) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));
            }

            if (Speed.LengthSquared() > 25f)
            {
                PlayWalk();
            }
            else
            {
                Speed = Vector2.Zero;
                PlayIdle();
            }

            if (AttackPressed)
            {
                PlayAttack();
            }
        }

        public override void Render()
        {
            if (Sprite == null)
            {
                Draw.Rect(X - 4f, Y - 12f, 8f, 12f, GenocideMode ? Color.DarkRed : Color.White);
                return;
            }

            base.Render();
        }

        private void OnCollide(CollisionData data)
        {
            if (data.Direction.X != 0f)
            {
                Speed = new Vector2(0f, Speed.Y);
            }

            if (data.Direction.Y != 0f)
            {
                Speed = new Vector2(Speed.X, 0f);
            }
        }

        private void ClampInsideLevel()
        {
            if (!ClampToRoomBounds || Scene is not Level level)
            {
                return;
            }

            Position = Position.Clamp(level.Bounds.Left + 4f, level.Bounds.Top + 4f, level.Bounds.Right - 4f, level.Bounds.Bottom - 1f);
        }

        private static Vector2 ReadMovementInput()
        {
            Vector2 movement = Vector2.Zero;

            if (MInput.Keyboard.Check(Keys.Left))
            {
                movement.X -= 1f;
            }

            if (MInput.Keyboard.Check(Keys.Right))
            {
                movement.X += 1f;
            }

            if (MInput.Keyboard.Check(Keys.Up))
            {
                movement.Y -= 1f;
            }

            if (MInput.Keyboard.Check(Keys.Down))
            {
                movement.Y += 1f;
            }

            return movement;
        }

        private static FacingDirection ParseFacing(string facing)
        {
            return facing?.ToLowerInvariant() switch
            {
                "right" => FacingDirection.Right,
                "up" => FacingDirection.Up,
                "left" => FacingDirection.Left,
                _ => FacingDirection.Down
            };
        }

        private string[] BuildCandidates(string baseName)
        {
            string suffix = Facing switch
            {
                FacingDirection.Down => "down",
                FacingDirection.Right => "right",
                FacingDirection.Up => "up",
                FacingDirection.Left => "left",
                _ => "down"
            };

            string genocidePrefix = GenocideMode ? "geno_" : string.Empty;

            return new[]
            {
                $"{genocidePrefix}{baseName}_{suffix}",
                $"{genocidePrefix}{baseName}{char.ToUpperInvariant(suffix[0])}{suffix.Substring(1)}",
                $"{baseName}_{suffix}",
                $"{baseName}{char.ToUpperInvariant(suffix[0])}{suffix.Substring(1)}",
                genocidePrefix + baseName,
                baseName,
                "idle"
            };
        }

        private void TryPlay(params string[][] candidateSets)
        {
            if (Sprite == null)
            {
                return;
            }

            foreach (string[] set in candidateSets)
            {
                for (int index = 0; index < set.Length; index++)
                {
                    string candidate = set[index];
                    if (Sprite.Has(candidate))
                    {
                        Sprite.Play(candidate);
                        return;
                    }
                }
            }
        }
    }
}
