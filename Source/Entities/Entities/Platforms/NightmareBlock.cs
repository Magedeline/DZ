using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/NightmareBlock")]
    [TrackedAs(typeof(DreamBlock), true)]
    [Tracked]
    public class NightmareBlock : DreamBlock
    {
        // Nightmare color palette - dark red theme for disabled state
        private new static readonly Color activeBackColor = Color.Black;
        private new static readonly Color disabledBackColor = Calc.HexToColor("8B0000");
        private new static readonly Color activeLineColor = Color.White;
        private new static readonly Color disabledLineColor = Calc.HexToColor("780606");

        // Timer for escape challenge - player must get through before timer expires
        private float escapeTimer;
        private float maxEscapeTime;
        private bool timerActive;
        private bool warningTriggered;
        private Player playerInsideBlock;
        private bool destroyed;
        private const float DEFAULT_ESCAPE_TIME = 1.5f; // Default 1.5 seconds to escape
        private const float WARNING_THRESHOLD = 0.5f;   // Start warning at 0.5 seconds left

        public NightmareBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            maxEscapeTime = data.Float("escapeTime", DEFAULT_ESCAPE_TIME);
            escapeTimer = maxEscapeTime;
        }

        public override void Update()
        {
            base.Update();
            if (destroyed)
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            // Detect player entering the block via dream dash
            if (!timerActive && CollideCheck(player) && player.StateMachine.State == Player.StDreamDash)
            {
                playerInsideBlock = player;
                timerActive = true;
                escapeTimer = maxEscapeTime;
                warningTriggered = false;
                Audio.Play("event:/game/06_reflection/dreamblock_enter", player.Position);
            }

            // Tick escape timer while player is inside
            if (timerActive && playerInsideBlock != null)
            {
                escapeTimer -= Engine.DeltaTime;

                // Warning effects when time is running low
                if (escapeTimer <= WARNING_THRESHOLD && !warningTriggered)
                {
                    warningTriggered = true;
                    TriggerWarningEffects();
                }

                // Timer expired - kill player and destroy block
                if (escapeTimer <= 0)
                {
                    TimerExpiredKillPlayer();
                    return;
                }

                // Player left the block or exited dream dash - successful escape
                if (!CollideCheck(playerInsideBlock) || playerInsideBlock.StateMachine.State != Player.StDreamDash)
                {
                    OnEscapeSuccess(playerInsideBlock);
                    return;
                }
            }
        }

        private void OnEscapeSuccess(Player player)
        {
            if (destroyed)
                return;
            destroyed = true;

            // Player successfully escaped before timer expired
            if (escapeTimer > 0)
            {
                Level level = SceneAs<Level>();
                if (level != null)
                {
                    // Green success particles
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 offset = new Vector2(
                            Calc.Random.Range(-16f, 16f),
                            Calc.Random.Range(-16f, 16f)
                        );
                        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, player.Position + offset, Color.Lime);
                    }
                    Audio.Play("event:/game/general/diamond_get", player.Position);
                }
            }

            timerActive = false;
            playerInsideBlock = null;
            ShatterAndDestroy();
        }

        private void ShatterAndDestroy()
        {
            Level level = SceneAs<Level>();
            if (level != null)
            {
                // Create shatter particles
                for (int i = 0; i < Width; i += 4)
                {
                    for (int j = 0; j < Height; j += 4)
                    {
                        Vector2 particlePos = new Vector2(X + i, Y + j);
                        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, particlePos, Color.DarkRed);
                    }
                }

                // Screen shake
                level.Shake(0.5f);

                // Sound effect
                Audio.Play("event:/game/general/thing_booped", Center);
            }

            // Destroy the block
            Collidable = Visible = false;
            DisableStaticMovers();
            RemoveSelf();
        }

        private void TriggerWarningEffects()
        {
            Level level = SceneAs<Level>();
            if (level != null)
            {
                // Intense screen shake
                level.Shake(0.3f);

                // Warning sound
                Audio.Play("event:/game/general/thing_booped", Center);

                // Red warning particles
                for (int i = 0; i < 20; i++)
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(-Width / 2, Width / 2),
                        Calc.Random.Range(-Height / 2, Height / 2)
                    );
                    level.ParticlesFG.Emit(Strawberry.P_WingsBurst, Center + offset, Color.Red);
                }
            }
        }

        private void TimerExpiredKillPlayer()
        {
            if (destroyed)
                return;
            destroyed = true;

            // Kill the player for failing to escape in time
            if (playerInsideBlock != null)
            {
                playerInsideBlock.Die(Vector2.Zero);
                playerInsideBlock = null;
            }

            timerActive = false;

            // Self-destruct the block
            ShatterAndDestroy();
        }

        public override void Render()
        {
            // Custom nightmare rendering with dark-red disabled palette
            // instead of vanilla DreamBlock's grey disabled palette.
            // Accesses base's private fields via the CelesteMod.Publicizer.
            Camera camera = SceneAs<Level>().Camera;
            if (Right < (double) camera.Left || Left > (double) camera.Right || Bottom < (double) camera.Top || Top > (double) camera.Bottom)
                return;

            bool active = playerHasDreamDash;
            Color backColor = active ? activeBackColor : disabledBackColor;
            Color lineColor = active ? activeLineColor : disabledLineColor;

            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, backColor);

            Vector2 cameraPos = SceneAs<Level>().Camera.Position;
            for (int index = 0; index < particles.Length; ++index)
            {
                int layer = particles[index].Layer;
                Vector2 pos = PutInside(particles[index].Position + cameraPos * (float)(0.30000001192092896 + 0.25 * layer));
                Color color = particles[index].Color;
                MTexture particleTexture;
                switch (layer)
                {
                    case 0:
                        particleTexture = particleTextures[3 - (int)((particles[index].TimeOffset * 4.0 + animTimer) % 4.0)];
                        break;
                    case 1:
                        particleTexture = particleTextures[1 + (int)((particles[index].TimeOffset * 2.0 + animTimer) % 2.0)];
                        break;
                    default:
                        particleTexture = particleTextures[2];
                        break;
                }
                if (pos.X >= X + 2.0 && pos.Y >= Y + 2.0 && pos.X < Right - 2.0 && pos.Y < Bottom - 2.0)
                    particleTexture.DrawCentered(pos + shake, color);
            }

            if (whiteFill > 0.0)
                Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);

            WobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0.0f, lineColor, backColor);
            WobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f, lineColor, backColor);
            WobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f, lineColor, backColor);
            WobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f, lineColor, backColor);

            // Draw escape timer bar when player is inside
            if (timerActive && playerInsideBlock != null && !destroyed)
                DrawTimerBar();

            // Corner accents
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, lineColor);
            Draw.Rect(shake + new Vector2((float)(X + (double)Width - 2.0), Y), 2f, 2f, lineColor);
            Draw.Rect(shake + new Vector2(X, (float)(Y + (double)Height - 2.0)), 2f, 2f, lineColor);
            Draw.Rect(shake + new Vector2((float)(X + (double)Width - 2.0), (float)(Y + (double)Height - 2.0)), 2f, 2f, lineColor);
        }

        private void DrawTimerBar()
        {
            float timerPercent = escapeTimer / maxEscapeTime;
            int barWidth = (int)(Width * 0.8f);
            int barHeight = 4;
            float barX = X + (Width - barWidth) / 2f;
            float barY = Y + Height - 8;

            // Background (dark)
            Draw.Rect(barX, barY, barWidth, barHeight, Color.Black * 0.7f);

            // Timer fill color based on urgency
            Color fillColor;
            if (timerPercent > 0.6f)
                fillColor = Color.Lime;      // Safe - green
            else if (timerPercent > 0.3f)
                fillColor = Color.Yellow;    // Warning - yellow
            else
                fillColor = Color.Red;       // Danger - red

            // Fill bar
            int fillWidth = (int)(barWidth * timerPercent);
            if (fillWidth > 0)
            {
                Draw.Rect(barX, barY, fillWidth, barHeight, fillColor);
            }

            // Border
            Draw.Rect(barX - 1, barY - 1, barWidth + 2, 1, Color.White);
            Draw.Rect(barX - 1, barY + barHeight, barWidth + 2, 1, Color.White);
            Draw.Rect(barX - 1, barY, 1, barHeight, Color.White);
            Draw.Rect(barX + barWidth, barY, 1, barHeight, Color.White);
        }

        private new Vector2 PutInside(Vector2 pos)
        {
            while (pos.X < (double) X)
                pos.X += Width;
            while (pos.X > X + (double) Width)
                pos.X -= Width;
            while (pos.Y < (double) Y)
                pos.Y += Height;
            while (pos.Y > Y + (double) Height)
                pos.Y -= Height;
            return pos;
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset, Color lineColor, Color backColor)
        {
            float length = (to - from).Length();
            Vector2 dir = Vector2.Normalize(to - from);
            Vector2 normal = new Vector2(dir.Y, -dir.X);
            Color color1 = lineColor;
            Color color2 = backColor;
            if (whiteFill > 0.0)
            {
                color1 = Color.Lerp(color1, Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }
            float prevAmplitude = 0.0f;
            int segmentStep = 16;
            for (int index = 2; index < length - 2.0; index += segmentStep)
            {
                float amplitude = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
                if (index + segmentStep >= (double) length)
                    amplitude = 0.0f;
                float thisSegmentLen = Math.Min(segmentStep, length - 2f - index);
                Vector2 start = from + dir * index + normal * prevAmplitude;
                Vector2 end = from + dir * (index + thisSegmentLen) + normal * amplitude;
                Draw.Line(start - normal, end - normal, color2);
                Draw.Line(start - normal * 2f, end - normal * 2f, color2);
                Draw.Line(start, end, color1);
                prevAmplitude = amplitude;
            }
        }

        private new float LineAmplitude(float seed, float index) => (float)(Math.Sin(seed + index / 16.0 + Math.Sin(seed * 2.0 + index / 32.0) * 6.2831854820251465) + 1.0) * 1.5f;

        private new float Lerp(float a, float b, float percent) => a + (b - a) * percent;
    }
}
