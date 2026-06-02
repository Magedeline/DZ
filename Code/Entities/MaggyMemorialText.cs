namespace Celeste.Entities
{
    /// <summary>
    /// Custom MemorialText that renders dialog text above a MaggyMemorial entity.
    /// Supports both normal and dreamy text modes with typewriter reveal.
    /// Based on vanilla Celeste's MemorialText behaviour.
    /// </summary>
    public class MaggyMemorialText : Entity
    {
        /// <summary>Whether the text is currently visible (set by the parent memorial).</summary>
        public bool Show;

        /// <summary>Whether to use the dreamy wobbly text effect.</summary>
        public bool Dreamy;

        /// <summary>Reference back to the owning memorial entity.</summary>
        public MaggyMemorial Memorial;

        // How far through the message the typewriter has revealed
        private float index;

        // The full dialog string
        private string message;

        // Current fade alpha (0 = hidden, 1 = fully visible)
        private float alpha;

        // Timer driving the dreamy sine-wave animation
        private float timer;

        // Widest single-character width × 0.9  (monospace-ish spacing)
        private float widestCharacter;

        // Length of the first line (shown instantly on re-approach)
        private int firstLineLength;

        // Sound source for the per-character text scroll SFX
        private SoundSource textSfx;
        private bool textSfxPlaying;

        public MaggyMemorialText(MaggyMemorial memorial, bool dreamy, string dialogKey)
        {
            // Render in screen-space (HUD) and keep updating while paused
            Tag = Tags.HUD | Tags.PauseUpdate;

            Memorial = memorial;
            Dreamy = dreamy;

            Add(textSfx = new SoundSource());

            // Fetch the localised dialog from our custom key
            message = Dialog.Clean(dialogKey);

            // Measure first line length for the "instant first line" trick
            firstLineLength = CountFirstLine(message);

            // Calculate monospace-ish character width
            widestCharacter = 0f;
            foreach (char c in message)
            {
                float w = ActiveFont.Measure(c).X;
                if (w > widestCharacter)
                    widestCharacter = w;
            }
            widestCharacter *= 0.9f;
        }

        private static int CountFirstLine(string msg)
        {
            int i = msg.IndexOf('\n');
            return i >= 0 ? i : msg.Length;
        }

        public override void Update()
        {
            base.Update();

            timer += Engine.DeltaTime;

            if (!Show)
            {
                // Fade out
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);

                if (alpha <= 0f)
                {
                    // Reset typewriter so first line pops in on next show
                    index = firstLineLength;
                }
            }
            else
            {
                // Fade in (2× speed)
                alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);

                if (alpha >= 1f)
                {
                    // Typewriter at ~32 chars/sec
                    index = Calc.Approach(index, message.Length, 32f * Engine.DeltaTime);
                }
            }

            // Drive the per-character scroll sound
            if (Show && index < message.Length)
            {
                if (!textSfxPlaying)
                {
                    textSfxPlaying = true;
                    textSfx.Play(Dreamy
                        ? "event:/ui/game/memorial_dream_text_loop"
                        : "event:/ui/game/memorial_text_loop");
                }
            }
            else if (textSfxPlaying)
            {
                textSfxPlaying = false;
                textSfx.Stop();
            }
        }

        public override void Render()
        {
            Level level = SceneAs<Level>();
            if (level == null) return;

            if (level.FrozenOrPaused || level.Completed || index <= 0f || alpha <= 0f)
                return;

            Camera cam = level.Camera;

            // Convert world → screen (320×180 → 1920×1080)
            float screenX = (Memorial.X - cam.X) * 6f;
            float screenY = (Memorial.Y - cam.Y) * 6f - 350f - ActiveFont.LineHeight * 3.3f;

            // Mirror mode
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                screenX = 1920f - screenX;

            float ease = Ease.CubeInOut(alpha);

            // Build per-character draw
            int visibleChars = (int)index;
            if (visibleChars > message.Length) visibleChars = message.Length;

            float yOffset = 64f * (1f - ease);
            int lineCharIndex = 0;
            int lineLength = GetLineLength(message, 0);

            for (int i = 0; i < visibleChars; i++)
            {
                char c = message[i];

                if (c == '\n')
                {
                    // Move down a line
                    yOffset += ActiveFont.LineHeight * 1.1f;
                    lineCharIndex = 0;
                    lineLength = GetLineLength(message, i + 1);
                    continue;
                }

                // Horizontal position for this character (centered per line)
                float cx = screenX - lineLength * widestCharacter / 2f
                           + (lineCharIndex + 0.5f) * widestCharacter;
                float cy = screenY + yOffset;
                float scaleX = 1f;

                if (Dreamy)
                {
                    // Dreamy wave offsets
                    cy += (float)Math.Sin(timer * 2f + i / 8f) * 8f;
                    scaleX = (float)Math.Sin(timer * 4f + i / 16f);
                    // Substitute character (dreamy scramble, like vanilla)
                    c = message[((int)(Math.Sin(timer * 2f + i / 8f) * 4f) + i) % message.Length];
                }

                ActiveFont.Draw(
                    c,
                    new Vector2(cx, cy),
                    new Vector2(0.5f, 1f),
                    new Vector2(scaleX, 1f) * ease,
                    Color.White * ease
                );

                lineCharIndex++;
            }
        }

        /// <summary>Returns the number of visible characters on the line starting at the given offset.</summary>
        private static int GetLineLength(string msg, int start)
        {
            int count = 0;
            for (int i = start; i < msg.Length; i++)
            {
                if (msg[i] == '\n') break;
                count++;
            }
            return count;
        }
    }
}
