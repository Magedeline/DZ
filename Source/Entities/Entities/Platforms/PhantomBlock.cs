using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Extensions;

namespace Celeste.Entities
{
    /// <summary>
    /// A "dream-like" block that does NOT inherit from the vanilla <see cref="DreamBlock"/>.
    ///
    /// Both Madeline and the Kirby player can dream-dash through it (the player's dream-dash
    /// state machine is patched to recognise <see cref="PhantomBlock"/> in addition to
    /// <see cref="DreamBlock"/>), and Kirby can additionally hover / float through it.
    ///
    /// Because it is not a <see cref="DreamBlock"/>, the Madeline↔Kirby player-swap hooks
    /// (<see cref="DZ.DreamBlockPlayerSwapHooks"/>) never fire for it — dashing through a
    /// PhantomBlock keeps the current character.
    ///
    /// Visual appearance is a faithful re-implementation of the vanilla dream-block renderer
    /// (rainbow particles + wobble outline when the session has dream dash, grey-teal when it
    /// does not).  Kirby mode force-shows the active palette.
    /// </summary>
    [CustomEntity("DZ/PhantomBlock")]
    [Tracked]
    [HotReloadable]
    public class PhantomBlock : Solid
    {
        // ── Particle ──────────────────────────────────────────────────────────────
        private struct DreamParticle
        {
            public Vector2 Position;
            public int Layer;
            public Color Color;
            public float TimeOffset;
        }

        // ── Palette (mirrors vanilla DreamBlock) ──────────────────────────────────
        private static readonly Color activeBackColor   = Color.Black;
        private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");
        private static readonly Color activeLineColor   = Color.White;
        private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");

        // ── State ─────────────────────────────────────────────────────────────────
        private bool playerHasDreamDash;
        private MTexture[] particleTextures;
        private DreamParticle[] particles;
        private float whiteFill;
        private float whiteHeight = 1f;
        private Vector2 shake;
        private float animTimer;
        private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);
        private float wobbleTo   = Calc.Random.NextFloat((float)Math.PI * 2f);
        private float wobbleEase;

        // ── Kirby hover-pass state ────────────────────────────────────────────────
        private bool _kirbyPassActive;

        // ── Constructor ───────────────────────────────────────────────────────────
        public PhantomBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Bool("below")) { }

        public PhantomBlock(Vector2 position, float width, float height, bool below = false)
            : base(position, width, height, safe: true)
        {
            Depth = -11000;
            if (below)
                Depth = 5000;
            SurfaceSoundIndex = 11;

            particleTextures = new MTexture[4]
            {
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
            };
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        public override void Added(Scene scene)
        {
            base.Added(scene);
            playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;
            Setup();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            TryActivateKirbyVisuals();
        }

        /// <summary>
        /// Force the active (colourful) palette when a Kirby-mode player is present, so
        /// the block reads as usable even without the dream-dash inventory item.
        /// </summary>
        private void TryActivateKirbyVisuals()
        {
            var player = Scene?.Tracker.GetEntity<Player>();
            if (player == null || !player.IsKirbyMode())
                return;

            playerHasDreamDash = true;
            Setup();
        }

        // ── Particles ─────────────────────────────────────────────────────────────
        private void Setup()
        {
            particles = new DreamParticle[(int)(Width / 8.0 * (Height / 8.0) * 0.699999988079071)];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position   = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                particles[i].Layer      = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
                particles[i].TimeOffset = Calc.Random.NextFloat();
                particles[i].Color      = Color.LightGray * (0.5f + particles[i].Layer / 2f * 0.5f);

                if (playerHasDreamDash)
                {
                    switch (particles[i].Layer)
                    {
                        case 0:
                            particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                            break;
                        case 1:
                            particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                            break;
                        case 2:
                            particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                            break;
                    }
                }
            }
        }

        // ── Update ────────────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            if (playerHasDreamDash)
            {
                animTimer  += 6f * Engine.DeltaTime;
                wobbleEase += Engine.DeltaTime * 2f;
                if (wobbleEase > 1f)
                {
                    wobbleEase = 0f;
                    wobbleFrom = wobbleTo;
                    wobbleTo   = Calc.Random.NextFloat((float)Math.PI * 2f);
                }
                SurfaceSoundIndex = 12;
            }

            HandleKirbyHover();
        }

        /// <summary>
        /// While the Kirby hover ability is active and the player is adjacent to or inside
        /// the block, the solid becomes temporarily non-collidable so Kirby drifts through.
        /// Collidability is restored as soon as Kirby exits or stops hovering.
        /// </summary>
        private void HandleKirbyHover()
        {
            var player = Scene?.Tracker.GetEntity<Player>();

            // Not in Kirby mode → make sure collidable is restored.
            if (player == null || !player.IsKirbyMode())
            {
                RestoreCollidable();
                return;
            }

            var kirbyExt = (Scene as Level)?.Tracker.GetEntity<KirbyPlayerExtension>();
            bool isHovering = kirbyExt?.AbilityManager?.Get<KirbyHoverAbility>()?.IsHovering ?? false;

            if (isHovering)
            {
                bool nearOrInside = CollideCheck(player)
                    || (player.Speed.LengthSquared() > 4f
                        && CollideCheck(player, player.Position + player.Speed.SafeNormalize(4f)));

                if (nearOrInside)
                {
                    Collidable = false;
                    _kirbyPassActive = true;
                }
                else if (_kirbyPassActive && !CollideCheck(player))
                {
                    RestoreCollidable();
                }
            }
            else
            {
                RestoreCollidable();
            }
        }

        private void RestoreCollidable()
        {
            if (_kirbyPassActive)
            {
                Collidable = true;
                _kirbyPassActive = false;
            }
        }

        // ── Player enter / exit (called by the patched dream-dash logic) ───────────
        /// <summary>Called by the player when it enters the block via a dream dash.</summary>
        public void OnPlayerEnter(Player player)
        {
            // Mirror the vanilla dream-block enter cue.
            Audio.Play("event:/char/madeline/dreamblock_enter", player.Position);
        }

        /// <summary>Called by the player when it exits the block after a dream dash.</summary>
        public void OnPlayerExit(Player player)
        {
            Dust.Burst(player.Position, player.Speed.Angle(), 16, null);
        }

        /// <summary>Footstep ripple, matching vanilla DreamBlock.FootstepRipple.</summary>
        public void FootstepRipple(Vector2 position)
        {
            if (playerHasDreamDash)
            {
                var burst = (Scene as Level)?.Displacement?.AddBurst(position, 0.5f, 0f, 40f);
                if (burst != null)
                {
                    burst.WorldClipCollider = Collider;
                    burst.WorldClipPadding  = 1;
                }
            }
        }

        // ── Render (faithful re-implementation of vanilla DreamBlock.Render) ───────
        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            if (Right < camera.Left || Left > camera.Right || Bottom < camera.Top || Top > camera.Bottom)
                return;

            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, playerHasDreamDash ? activeBackColor : disabledBackColor);

            Vector2 camPos = SceneAs<Level>().Camera.Position;
            for (int i = 0; i < particles.Length; i++)
            {
                int layer = particles[i].Layer;
                Vector2 pos2 = particles[i].Position;
                pos2 += camPos * (0.3f + 0.25f * layer);
                pos2  = PutInside(pos2);
                Color color = particles[i].Color;
                MTexture tex;

                switch (layer)
                {
                    case 0:
                    {
                        int n = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
                        tex = particleTextures[3 - n];
                        break;
                    }
                    case 1:
                    {
                        int n = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
                        tex = particleTextures[1 + n];
                        break;
                    }
                    default:
                        tex = particleTextures[2];
                        break;
                }

                if (pos2.X >= X + 2f && pos2.Y >= Y + 2f && pos2.X < Right - 2f && pos2.Y < Bottom - 2f)
                    tex.DrawCentered(pos2 + shake, color);
            }

            if (whiteFill > 0f)
                Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);

            WobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0f);
            WobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f);
            WobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f);
            WobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f);

            Color lineCol = playerHasDreamDash ? activeLineColor : disabledLineColor;
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, lineCol);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, lineCol);
            Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, lineCol);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, lineCol);
        }

        private Vector2 PutInside(Vector2 pos)
        {
            if (pos.X > Right)
                pos.X -= (float)Math.Ceiling((pos.X - Right) / Width) * Width;
            else if (pos.X < Left)
                pos.X += (float)Math.Ceiling((Left - pos.X) / Width) * Width;
            if (pos.Y > Bottom)
                pos.Y -= (float)Math.Ceiling((pos.Y - Bottom) / Height) * Height;
            else if (pos.Y < Top)
                pos.Y += (float)Math.Ceiling((Top - pos.Y) / Height) * Height;
            return pos;
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float len = (to - from).Length();
            Vector2 dir = Vector2.Normalize(to - from);
            Vector2 norm = new Vector2(dir.Y, -dir.X);
            Color color  = playerHasDreamDash ? activeLineColor  : disabledLineColor;
            Color color2 = playerHasDreamDash ? activeBackColor  : disabledBackColor;

            if (whiteFill > 0f)
            {
                color  = Color.Lerp(color,  Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }

            float amp = 0f;
            int step = 16;
            for (int i = 2; i < len - 2f; i += step)
            {
                float next = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
                if (i + step >= len)
                    next = 0f;
                float seg = Math.Min(step, len - 2f - i);
                Vector2 a = from + dir * i + norm * amp;
                Vector2 b = from + dir * (i + seg) + norm * next;
                Draw.Line(a - norm, b - norm, color2);
                Draw.Line(a - norm * 2f, b - norm * 2f, color2);
                Draw.Line(a, b, color);
                amp = next;
            }
        }

        private float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin(seed + index / 16f + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }

        private static float Lerp(float a, float b, float percent) => a + (b - a) * percent;
    }
}
