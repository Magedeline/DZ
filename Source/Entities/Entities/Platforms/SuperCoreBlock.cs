using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Entities
{
    [CustomEntity("DZ/SuperCoreBlock")]
    [Tracked]
    public class SuperCoreBlock : Solid
    {
        private const float WindUpDelay = 0.0f;
        private const float WindUpDist = 10f;
        private const float IceWindUpDist = 16f;
        private const float BounceDist = 24f;
        private const float LiftSpeedXMult = 0.75f;
        private const float CooldownTime = 0.8f;
        private const float BounceEndTime = 0.05f;

        private enum States
        {
            Waiting,
            WindingUp,
            Bouncing,
            Cooldown,
        }

        private readonly float speedMultiplier;
        private readonly float launchRange;
        private readonly bool requiresCoreMode;
        private readonly Color hotColor;
        private readonly Color coldColor;
        private readonly Color superColor;

        private States state;
        private Level level;
        private Session.CoreModes currentCoreMode;
        private Vector2 bounceDir;
        private Vector2 startPos;
        private float windUpStartTimer;
        private float windUpProgress;
        private float cooldownTimer;
        private float bounceEndTimer;
        private float reappearFlash;
        private bool iceMode;
        private bool iceModeNext;
        private Player targetPlayer;
        private List<Image> hotImages;
        private List<Image> coldImages;
        private Sprite hotCenterSprite;
        private Sprite coldCenterSprite;

        public SuperCoreBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Depth = 8990;
            startPos = Position;

            speedMultiplier = data.Float("speedMultiplier", 3f);
            launchRange = data.Float("launchRange", 400f);
            requiresCoreMode = data.Bool("requiresCoreMode", false);
            iceMode = data.Bool("notCoreMode", false);
            iceModeNext = iceMode;

            TryParseColor(data.Attr("hotColor", "FF4500"), out hotColor);
            TryParseColor(data.Attr("coldColor", "00BFFF"), out coldColor);
            TryParseColor(data.Attr("superColor", "FFD700"), out superColor);

            hotImages = BuildSprite(GFX.Game["objects/DZ/BumpBlockNew/fire00"]);
            hotCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterFire");
            hotCenterSprite.Position = new Vector2(Width, Height) / 2f;
            hotCenterSprite.Visible = false;
            Add(hotCenterSprite);

            coldImages = BuildSprite(GFX.Game["objects/DZ/BumpBlockNew/ice00"]);
            coldCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterIce");
            coldCenterSprite.Position = new Vector2(Width, Height) / 2f;
            coldCenterSprite.Visible = false;
            Add(coldCenterSprite);

            Add(new CoreModeListener(OnChangeMode));
        }

        public SuperCoreBlock(EntityData data, Vector2 offset, bool iceMode)
            : this(data, offset)
        {
            this.iceMode = iceMode;
            iceModeNext = iceMode;
            ToggleSprite();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            currentCoreMode = level.Session.CoreMode;
            ToggleSprite();
        }

        public override void Update()
        {
            base.Update();

            if (level?.Session != null)
                currentCoreMode = level.Session.CoreMode;

            reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);

            if (state == States.Cooldown)
            {
                cooldownTimer -= Engine.DeltaTime;
                if (cooldownTimer <= 0f)
                    state = States.Waiting;
            }

            if (state == States.Waiting)
            {
                CheckModeChange();
                Player player = WindUpPlayerCheck();
                if (player == null || !CanTriggerInCurrentMode())
                    return;

                StartWindUp(player);
                return;
            }

            if (state == States.WindingUp)
            {
                CheckModeChange();

                Player player = WindUpPlayerCheck();
                if (player != null)
                    bounceDir = GetBounceDirection(player);

                if (windUpStartTimer > 0f)
                {
                    windUpStartTimer -= Engine.DeltaTime;
                    windUpProgress = Calc.Approach(windUpProgress, 0f, Engine.DeltaTime * 8f);
                    return;
                }

                windUpProgress = Calc.Approach(windUpProgress, 1f, Engine.DeltaTime * 8f);
                if (windUpProgress < 1f)
                    return;

                LaunchPlayer();
                state = States.Bouncing;
                bounceEndTimer = BounceEndTime;
                return;
            }

            if (state != States.Bouncing)
                return;

            bounceEndTimer -= Engine.DeltaTime;
            if (bounceEndTimer > 0f)
                return;

            state = States.Cooldown;
            cooldownTimer = CooldownTime;
            targetPlayer = null;
        }

        public override void Render()
        {
            base.Render();

            if (state == States.WindingUp || state == States.Bouncing)
            {
                Color flashColor = GetCurrentColor() * (state == States.WindingUp ? 0.18f : 0.25f);
                Draw.Rect(X - 2f, Y - 2f, Width + 4f, Height + 4f, flashColor);

                float flash = state == States.WindingUp ? Ease.CubeOut(windUpProgress) : 1f;
                Draw.Rect(X - 4f, Y - 4f, Width + 8f, Height + 8f, Color.White * (0.12f * flash));
            }

            if (reappearFlash > 0f)
            {
                float amount = Ease.CubeOut(reappearFlash);
                float padding = amount * 2f;
                Draw.Rect(X - padding, Y - padding, Width + padding * 2f, Height + padding * 2f, Color.White * amount);
            }
        }

        private List<Image> BuildSprite(MTexture source)
        {
            List<Image> images = new List<Image>();
            int columns = source.Width / 8;
            int rows = source.Height / 8;

            for (int x = 0; x < Width; x += 8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    int column = x != 0 ? (x < Width - 8f ? Calc.Random.Next(1, columns - 1) : columns - 1) : 0;
                    int row = y != 0 ? (y < Height - 8f ? Calc.Random.Next(1, rows - 1) : rows - 1) : 0;
                    Image image = new Image(source.GetSubtexture(column * 8, row * 8, 8, 8));
                    image.Position = new Vector2(x, y);
                    images.Add(image);
                    Add(image);
                }
            }

            return images;
        }

        private void ToggleSprite()
        {
            hotCenterSprite.Visible = !iceMode;
            coldCenterSprite.Visible = iceMode;

            foreach (Image hotImage in hotImages)
                hotImage.Visible = !iceMode;

            foreach (Image coldImage in coldImages)
                coldImage.Visible = iceMode;
        }

        private void OnChangeMode(Session.CoreModes coreMode)
        {
            iceModeNext = coreMode == Session.CoreModes.Cold;
        }

        private void CheckModeChange()
        {
            if (iceModeNext == iceMode)
                return;

            iceMode = iceModeNext;
            ToggleSprite();
        }

        private bool CanTriggerInCurrentMode()
        {
            if (!requiresCoreMode)
                return true;

            Session.CoreModes requiredMode = iceMode ? Session.CoreModes.Cold : Session.CoreModes.Hot;
            return currentCoreMode == requiredMode;
        }

        private void StartWindUp(Player player)
        {
            state = States.WindingUp;
            targetPlayer = player;
            windUpStartTimer = WindUpDelay;
            windUpProgress = 0f;
            bounceDir = GetBounceDirection(player);
            StartShaking(0.1f);
            Audio.Play(iceMode ? "event:/game/09_core/iceblock_touch" : "event:/game/09_core/bounceblock_touch", Center);
        }

        private void LaunchPlayer()
        {
            Player player = targetPlayer ?? WindUpPlayerCheck();
            if (player == null)
                return;

            float launchSpeed = GetBaseLaunchSpeed() * speedMultiplier;
            player.StateMachine.State = 0;
            player.Speed = bounceDir * launchSpeed;
            player.Speed.X *= LiftSpeedXMult;
            player.StartJumpGraceTime();

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            StartShaking(0.15f);
            reappearFlash = 0.6f;
            Audio.Play(iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_break", Center);
        }

        private float GetBaseLaunchSpeed()
        {
            return iceMode ? 320f : 280f;
        }

        private Vector2 GetBounceDirection(Player player)
        {
            if (iceMode)
                return -Vector2.UnitY;

            Vector2 direction = (player.Center - Center).SafeNormalize(-Vector2.UnitY);
            if (Math.Abs(direction.X) < 0.2f)
                direction = new Vector2(Math.Sign(player.Center.X - Center.X), -0.7f).SafeNormalize(-Vector2.UnitY);

            return direction;
        }

        private Player WindUpPlayerCheck()
        {
            Player player = CollideFirst<Player>(Position - Vector2.UnitY);
            if (player != null && player.Speed.Y < 0f)
                player = null;

            if (player == null)
            {
                player = CollideFirst<Player>(Position + Vector2.UnitX);
                if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Left)
                {
                    player = CollideFirst<Player>(Position - Vector2.UnitX);
                    if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Right)
                        player = null;
                }
            }

            return player;
        }

        private Color GetCurrentColor()
        {
            return iceMode ? coldColor : hotColor;
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = Color.White;
            if (string.IsNullOrEmpty(hex))
                return false;

            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            try
            {
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    color = new Color(r, g, b);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
