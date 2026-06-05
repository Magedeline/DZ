using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using JumpThru = global::Celeste.JumpThru;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    // =============================================
    // GravityFlipPlatform - Steps on it to flip gravity
    // =============================================
    [CustomEntity("MaggyHelper/GravityFlipPlatform")]
    [Tracked]
    public class GravityFlipPlatform : Solid
    {
        private bool activated = false;
        private float cooldown = 0f;
        private float cooldownTime;
        private bool togglable;
        private Sprite sprite;
        private bool gravityFlipped = false;

        public GravityFlipPlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            cooldownTime = data.Float("cooldown", 2f);
            togglable = data.Bool("togglable", true);
            Depth = -10;

            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_gravityFlipPlatform"));
            sprite.Position = new Vector2(Width / 2f, Height / 2f);
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
            if (cooldown > 0) cooldown -= Engine.DeltaTime;

            Player player = GetPlayerRider();
            if (player != null && cooldown <= 0 && (!activated || togglable))
            {
                Activate(player);
            }
        }

        private void Activate(Player player)
        {
            activated = !activated;
            gravityFlipped = activated;
            cooldown = cooldownTime;
            Audio.Play("event:/game/general/fallblock_shake", Position);
            sprite.Play(activated ? "active" : "idle");

            Level level = SceneAs<Level>();
            level.Session.SetFlag("gravity_flipped", gravityFlipped);
            (Scene as Level)?.Shake(0.3f);
        }
    }

    // =============================================
    // WindTunnel - Directional air current zone
    // =============================================
    [CustomEntity("MaggyHelper/WindTunnel")]
    [Tracked]
    public class WindTunnel : Entity
    {
        private Vector2 direction;
        private float strength;
        private float width, height;
        private bool affectsKirbyMore;
        private ParticleType windParticle;

        public WindTunnel(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            width = data.Width;
            height = data.Height;
            strength = data.Float("strength", 200f);
            affectsKirbyMore = data.Bool("affectsKirbyMore", true);

            string dir = data.Attr("direction", "Up");
            direction = dir switch
            {
                "Up" => -Vector2.UnitY,
                "Down" => Vector2.UnitY,
                "Left" => -Vector2.UnitX,
                "Right" => Vector2.UnitX,
                _ => -Vector2.UnitY
            };

            Collider = new Hitbox(width, height);
            Depth = -500;

            windParticle = new ParticleType
            {
                Color = Color.White * 0.3f,
                Size = 1f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                DirectionRange = 0.2f
            };
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player))
            {
                float mult = affectsKirbyMore && Scene.Tracker.GetEntity<Player>() != null ? 1.5f : 1f;
                player.Speed += direction * strength * mult * Engine.DeltaTime;
            }

            if (Scene.OnInterval(0.05f))
            {
                SceneAs<Level>().Particles.Emit(windParticle,
                    Position + new Vector2(Calc.Random.NextFloat(width), Calc.Random.NextFloat(height)),
                    direction.Angle());
            }
        }
    }

    // =============================================
    // SpringCloud - Bouncy cloud, disappears after use
    // =============================================
    [CustomEntity("MaggyHelper/SpringCloud")]
    [Tracked]
    public class SpringCloud : JumpThru
    {
        private float respawnTime;
        private float timer = 0f;
        private bool broken = false;
        private float extraHeight;
        private Sprite sprite;

        public SpringCloud(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, safe: true)
        {
            respawnTime = data.Float("respawnTime", 3f);
            extraHeight = data.Float("extraHeight", 50f);
            Depth = -50;

            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_springCloud"));
            sprite.Position = new Vector2(Width / 2f, 0f);
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
            if (broken)
            {
                timer -= Engine.DeltaTime;
                if (timer <= 0)
                {
                    broken = false;
                    Collidable = true;
                    Visible = true;
                    sprite.Play("respawn");
                }
                return;
            }

            Player player = GetPlayerRider();
            if (player != null)
            {
                Bounce(player);
            }
        }

        private void Bounce(Player player)
        {
            player.Speed.Y = -(260f + extraHeight);
            Audio.Play("event:/game/general/spring", Position);
            sprite.Play("bounce");
            broken = true;
            timer = respawnTime;
            Collidable = false;

            Add(new Coroutine(FadeOut()));
        }

        private IEnumerator FadeOut()
        {
            for (float t = 1f; t > 0; t -= Engine.DeltaTime * 3f)
            {
                sprite.Color = Color.White * t;
                yield return null;
            }
            Visible = false;
        }
    }

    // =============================================
    // RainbowBridge - Only appears when player is moving
    // =============================================
    [CustomEntity("MaggyHelper/RainbowBridge")]
    [Tracked]
    public class RainbowBridge : Solid
    {
        private float fadeTarget = 0f;
        private float fadeCurrent = 0f;
        private float speedThreshold;
        private Color[] colors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };

        public RainbowBridge(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            speedThreshold = data.Float("speedThreshold", 20f);
            Depth = 100;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                float speed = player.Speed.Length();
                fadeTarget = speed > speedThreshold ? 1f : 0f;
            }

            fadeCurrent = Calc.Approach(fadeCurrent, fadeTarget, Engine.DeltaTime * 4f);
            Collidable = fadeCurrent > 0.5f;
        }

        public override void Render()
        {
            if (fadeCurrent <= 0.01f) return;
            float segWidth = Width / colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                Draw.Rect(Position.X + i * segWidth, Position.Y, segWidth, Height, colors[i] * fadeCurrent * 0.8f);
            }
        }
    }

    // =============================================
    // ConveyorBelt - Moving floor
    // =============================================
    [CustomEntity("MaggyHelper/ConveyorBelt")]
    [Tracked]
    public class ConveyorBelt : JumpThru
    {
        private float speed;
        private int direction; // -1 or 1

        public ConveyorBelt(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, safe: true)
        {
            speed = data.Float("speed", 60f);
            direction = data.Bool("moveRight", true) ? 1 : -1;
            Depth = -10;
            SurfaceSoundIndex = 11;
        }

        public override void Update()
        {
            base.Update();
            Player player = GetPlayerRider();
            if (player != null)
            {
                player.MoveH(speed * direction * Engine.DeltaTime);
            }
        }

        public override void Render()
        {
            base.Render();
            float arrowSpacing = 16f;
            for (float x = 0; x < Width; x += arrowSpacing)
            {
                float offset = (Scene.TimeActive * speed * 0.5f * direction) % arrowSpacing;
                Draw.Rect(Position.X + x + offset, Position.Y + Height / 2 - 1, 6, 2, Color.Gray * 0.5f);
            }
        }
    }

    // =============================================
    // PortalDoor - Paired doors for teleportation
    // =============================================
    [CustomEntity("MaggyHelper/PortalDoor")]
    [Tracked]
    public class PortalDoor : Entity
    {
        private Vector2 exitPosition;
        private string portalId;
        private float cooldown = 0f;
        private Color color;
        private Sprite sprite;

        public PortalDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            portalId = data.Attr("portalId", "portal_A");
            color = Calc.HexToColor(data.Attr("color", "00ffff"));

            if (data.Nodes.Length > 0)
                exitPosition = data.NodesOffset(offset)[0];
            else
                exitPosition = Position;

            Collider = new Hitbox(16f, 24f, -8f, -24f);
            Depth = -500;

            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_portalDoor"));
        }

        public override void Update()
        {
            base.Update();
            if (cooldown > 0) cooldown -= Engine.DeltaTime;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && CollideCheck(player) && cooldown <= 0)
            {
                if (Input.Talk.Pressed)
                {
                    TeleportPlayer(player);
                }
            }
        }

        private void TeleportPlayer(Player player)
        {
            cooldown = 1f;
            Audio.Play("event:/game/general/cassette_bubblereturn", Position);

            // Find paired door
            foreach (PortalDoor door in Scene.Tracker.GetEntities<PortalDoor>())
            {
                if (door != this && door.portalId == portalId)
                {
                    player.Position = door.Position;
                    door.cooldown = 1f;
                    (Scene as Level)?.Flash(color * 0.3f);
                    return;
                }
            }
            // If no pair, use node position
            player.Position = exitPosition;
            (Scene as Level)?.Flash(color * 0.3f);
        }
    }

    // =============================================
    // StickyWall - Player sticks with reduced/no stamina drain
    // =============================================
    [CustomEntity("MaggyHelper/StickyWall")]
    [Tracked]
    public class StickyWall : Solid
    {
        private float stickDuration;
        private bool infiniteStick;

        public StickyWall(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            stickDuration = data.Float("stickDuration", 5f);
            infiniteStick = data.Bool("infiniteStick", false);
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StClimb)
            {
                if (CollideCheck(player, Position + Vector2.UnitX) || CollideCheck(player, Position - Vector2.UnitX))
                {
                    if (infiniteStick)
                    {
                        player.Stamina = Player.ClimbMaxStamina;
                    }
                    else
                    {
                        player.Stamina = Math.Max(player.Stamina, Player.ClimbMaxStamina * 0.5f);
                    }
                }
            }
        }

        public override void Render()
        {
            Draw.Rect(Collider, Color.ForestGreen * 0.3f);
            base.Render();
        }
    }

    // =============================================
    // IcePlatform - Slippery platform
    // =============================================
    [CustomEntity("MaggyHelper/IcePlatform")]
    [Tracked]
    public class IcePlatform : JumpThru
    {
        private float friction;
        private bool canMelt;
        private bool melted = false;

        public IcePlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, safe: true)
        {
            friction = data.Float("friction", 0.98f);
            canMelt = data.Bool("canMelt", true);
            SurfaceSoundIndex = 16;
            Depth = -10;
        }

        public override void Update()
        {
            base.Update();
            if (melted) return;

            Player player = GetPlayerRider();
            if (player != null)
            {
                player.Speed.X *= friction;
            }

            if (canMelt)
            {
                Level level = SceneAs<Level>();
                if (level.Session.GetFlag("fire_active"))
                {
                    Add(new Coroutine(MeltRoutine()));
                }
            }
        }

        private IEnumerator MeltRoutine()
        {
            melted = true;
            Audio.Play("event:/game/general/wall_break_ice", Position);
            for (float t = 1f; t > 0; t -= Engine.DeltaTime * 2f)
            {
                Visible = Scene.OnInterval(0.05f);
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            Draw.Rect(Position, Width, 8f, Color.LightCyan * 0.7f);
        }
    }

    // =============================================
    // MagnetRail - Player grinds along a node path
    // =============================================
    [CustomEntity("MaggyHelper/MagnetRail")]
    [Tracked]
    public class MagnetRail : Entity
    {
        private Vector2[] nodes;
        private float speed;
        private bool attached = false;
        private int currentNode = 0;
        private float t = 0f;
        private Color color;

        public MagnetRail(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            speed = data.Float("speed", 120f);
            color = Calc.HexToColor(data.Attr("color", "ffff00"));

            var nodeList = new List<Vector2> { Position };
            foreach (var node in data.NodesOffset(offset))
                nodeList.Add(node);
            nodes = nodeList.ToArray();

            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Depth = -1000;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;

            if (!attached && CollideCheck(player))
            {
                attached = true;
                currentNode = 0;
                t = 0f;
                Audio.Play("event:/game/general/cassette_bubblereturn", Position);
            }

            if (attached && currentNode < nodes.Length - 1)
            {
                Vector2 from = nodes[currentNode];
                Vector2 to = nodes[currentNode + 1];
                float dist = Vector2.Distance(from, to);
                t += (speed / dist) * Engine.DeltaTime;

                if (t >= 1f)
                {
                    t = 0f;
                    currentNode++;
                    if (currentNode >= nodes.Length - 1)
                    {
                        attached = false;
                        return;
                    }
                }

                player.Position = Vector2.Lerp(nodes[currentNode], nodes[currentNode + 1], t);
                player.Speed = Vector2.Zero;
            }
        }

        public override void Render()
        {
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                Draw.Line(nodes[i], nodes[i + 1], color * 0.7f, 2f);
            }
            foreach (var node in nodes)
            {
                Draw.Circle(node, 4f, color, 8);
            }
        }
    }

    // =============================================
    // PhaseBlock - Phases in/out on a sine wave
    // =============================================
    [CustomEntity("MaggyHelper/PhaseBlock")]
    [Tracked]
    public class PhaseBlock : Solid
    {
        private float phaseSpeed;
        private float phaseOffset;
        private float alpha;

        public PhaseBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            phaseSpeed = data.Float("phaseSpeed", 1f);
            phaseOffset = data.Float("phaseOffset", 0f);
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            alpha = (float)(Math.Sin((Scene.TimeActive * phaseSpeed + phaseOffset) * Math.PI * 2) * 0.5 + 0.5);
            Collidable = alpha > 0.5f;
        }

        public override void Render()
        {
            Draw.Rect(Collider, Color.MediumPurple * alpha * 0.8f);
        }
    }

    // =============================================
    // LaunchCannon - Enter and aim, then fire
    // =============================================
    [CustomEntity("MaggyHelper/LaunchCannon")]
    [Tracked]
    public class LaunchCannon : Entity
    {
        private float launchSpeed;
        private float aimAngle = 0f;
        private bool occupied = false;
        private bool autoFire;
        private float autoAngle;
        private Sprite sprite;

        public LaunchCannon(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            launchSpeed = data.Float("launchSpeed", 400f);
            autoFire = data.Bool("autoFire", false);
            autoAngle = data.Float("autoAngle", -90f);

            Collider = new Hitbox(24f, 24f, -12f, -12f);
            Depth = -500;

            Add(sprite = GFX.SpriteBank.Create("MaggyHelper_launchCannon"));
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null) return;

            if (!occupied && CollideCheck(player))
            {
                EnterCannon(player);
            }

            if (occupied && player != null)
            {
                player.Position = Position;
                player.Speed = Vector2.Zero;

                if (autoFire)
                {
                    FireCannon(player, autoAngle);
                    return;
                }

                // Aim with directional input
                if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    aimAngle = (float)Math.Atan2(Input.MoveY.Value, Input.MoveX.Value);
                }

                if (Input.Jump.Pressed || Input.Dash.Pressed)
                {
                    FireCannon(player, MathHelper.ToDegrees(aimAngle));
                }
            }
        }

        private void EnterCannon(Player player)
        {
            occupied = true;
            player.StateMachine.State = Player.StDummy;
            player.Speed = Vector2.Zero;
            player.Position = Position;
            Audio.Play("event:/game/general/spring", Position);
        }

        private void FireCannon(Player player, float angleDegrees)
        {
            occupied = false;
            float rad = MathHelper.ToRadians(angleDegrees);
            player.Speed = new Vector2(
                (float)Math.Cos(rad) * launchSpeed,
                (float)Math.Sin(rad) * launchSpeed
            );
            player.StateMachine.State = Player.StNormal;
            Audio.Play("event:/game/general/fallblock_impact", Position);
            (Scene as Level)?.DirectionalShake(new Vector2((float)Math.Cos(rad), (float)Math.Sin(rad)), 0.2f);
        }

        public override void Render()
        {
            Draw.Circle(Position, 14f, Color.DarkGray, 16);
            Draw.Circle(Position, 12f, Color.Gray, 16);
            float rad = autoFire ? MathHelper.ToRadians(autoAngle) : aimAngle;
            Draw.Line(Position, Position + new Vector2((float)Math.Cos(rad), (float)Math.Sin(rad)) * 20f, Color.Red, 3f);
        }
    }

    // =============================================
    // BubbleRaft - Floating bubble carrier
    // =============================================
    [CustomEntity("MaggyHelper/BubbleRaft")]
    [Tracked]
    public class BubbleRaft : Entity
    {
        private float maxDuration;
        private float timer;
        private bool carrying = false;
        private Vector2 velocity;
        private float floatSpeed;

        public BubbleRaft(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            maxDuration = data.Float("duration", 5f);
            floatSpeed = data.Float("floatSpeed", 30f);
            velocity = new Vector2(0, -floatSpeed);

            Collider = new Hitbox(20f, 20f, -10f, -10f);
            Depth = -500;
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();

            if (!carrying && player != null && CollideCheck(player))
            {
                carrying = true;
                timer = maxDuration;
            }

            if (carrying)
            {
                Position += velocity * Engine.DeltaTime;
                timer -= Engine.DeltaTime;

                if (player != null)
                {
                    player.Position = Position + new Vector2(0, -12f);
                    player.Speed = Vector2.Zero;

                    if (Input.Dash.Pressed)
                    {
                        Pop(player);
                        return;
                    }
                }

                if (timer <= 0)
                {
                    Pop(player);
                }
            }
        }

        private void Pop(Player player)
        {
            carrying = false;
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            Audio.Play("event:/game/general/platform_disablesolid", Position);
            RemoveSelf();
        }

        public override void Render()
        {
            float pulse = 1f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.05f;
            float alpha = carrying ? Math.Max(timer / maxDuration, 0.3f) : 1f;
            Draw.Circle(Position, 12f * pulse, Color.CornflowerBlue * alpha * 0.6f, 16);
            Draw.Circle(Position, 11f * pulse, Color.LightSkyBlue * alpha * 0.3f, 16);
        }
    }

    // =============================================
    // TimePlatform - Exists in past or future
    // =============================================
    [CustomEntity("MaggyHelper/TimePlatform")]
    [Tracked]
    public class TimePlatform : Solid
    {
        private string timeEra; // "past" or "future"
        private string flagName;

        public TimePlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: true)
        {
            timeEra = data.Attr("timeEra", "past");
            flagName = data.Attr("flagName", "time_state_future");
            Depth = 0;
        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            bool isFuture = level.Session.GetFlag(flagName);
            bool shouldBeActive = (timeEra == "future" && isFuture) || (timeEra == "past" && !isFuture);
            Collidable = shouldBeActive;
            Visible = shouldBeActive;
        }

        public override void Render()
        {
            Color c = timeEra == "past" ? Color.SaddleBrown * 0.6f : Color.Cyan * 0.6f;
            Draw.Rect(Collider, c);
        }
    }
}

