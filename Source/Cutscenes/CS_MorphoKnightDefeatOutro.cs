using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Plays immediately after MorphoKnightDeltaBoss.DefeatSequence() finishes
    /// vanishing: a corrupted heart vision reveals that Els's heart has been
    /// corrupted, glitches/warps away, then the screen fades to white and back
    /// to hand control back to the player in the same room.
    /// <para>
    /// This originates the first narrative link between MorphoKnightDeltaBoss
    /// (Ch15, Act II) and Els (Ch16+, Act III) -- no prior dialogue or code
    /// establishes this connection, so later Els cutscenes should treat this
    /// as the earliest foreshadowing beat.
    /// </para>
    /// </summary>
    [HotReloadable]
    public class CS_MorphoKnightDefeatOutro : CutsceneEntity
    {
        private const string LineOne = "ELS'S HEART...";
        private const string LineTwo = "CORRUPTED";

        private readonly global::Celeste.Player player;
        private Image heart;
        private float heartAlpha = 0f;
        private float heartCorruption = 0f;
        private float textAlpha = 0f;
        private bool showLineTwo = false;
        private float glitchJitter = 0f;
        private Random rng = new Random();

        public CS_MorphoKnightDefeatOutro(global::Celeste.Player player, Vector2 position) : base(true, false)
        {
            this.player = player;
            Position = position;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            if (player != null)
                player.StateMachine.State = global::Celeste.Player.StDummy;

            yield return 0.5f;

            // A heart takes shape where Morpho Knight Delta vanished.
            Add(heart = new Image(GFX.Game["collectables/heartGem/shape"]));
            heart.CenterOrigin();
            heart.Color = Color.White * 0f;
            heart.Scale = Vector2.Zero;

            Audio.Play("event:/DZ/char/bosses/mkd/heartadsorb", Position);
            level.Displacement.AddBurst(Position, 0.6f, 8f, 40f, 0.3f);

            for (float t = 0f; t < 0.8f; t += Engine.DeltaTime)
            {
                float eased = Ease.CubeOut(t / 0.8f);
                heartAlpha = eased;
                heart.Color = Color.Lerp(Color.Black, new Color(0x99, 0x32, 0xCC), eased) * eased;
                heart.Scale = Vector2.One * (0.3f + eased * 0.7f);
                yield return null;
            }

            yield return 0.3f;

            // The heart curdles from a healthy purple into something wrong.
            Audio.Play("event:/DZ/new_content/char/bosses/els/special/dark_matter_spawn", Position);
            level.Shake(0.4f);
            level.Flash(Color.Purple * 0.3f, false);

            for (float t = 0f; t < 1.2f; t += Engine.DeltaTime)
            {
                heartCorruption = t / 1.2f;
                heart.Color = Color.Lerp(new Color(0x99, 0x32, 0xCC), new Color(0x1a, 0x00, 0x22), heartCorruption);
                glitchJitter = heartCorruption * 2.5f;
                yield return null;
            }
            glitchJitter = 0f;

            yield return 0.3f;

            // First line of the reveal.
            for (float t = 0f; t < 0.4f; t += Engine.DeltaTime)
            {
                textAlpha = t / 0.4f;
                yield return null;
            }
            yield return 1.1f;

            showLineTwo = true;
            for (float t = 0f; t < 0.3f; t += Engine.DeltaTime)
            {
                textAlpha = 1f - t / 0.3f;
                yield return null;
            }
            textAlpha = 0f;
            for (float t = 0f; t < 0.3f; t += Engine.DeltaTime)
            {
                textAlpha = t / 0.3f;
                yield return null;
            }
            yield return 1.3f;

            // Warp out: the vision spins away and is yanked into a rift.
            Audio.Play("event:/DZ/char/bosses/mkd/vanishdefeat", Position);
            Audio.Play("event:/DZ/new_content/char/bosses/els/special/rift_open", Position);
            level.Displacement.AddBurst(Position, 0.8f, 16f, 64f, 0.5f);

            for (float t = 0f; t < 0.6f; t += Engine.DeltaTime)
            {
                float p = t / 0.6f;
                float shrink = 1f - Ease.CubeIn(p);
                heart.Scale = Vector2.One * shrink;
                heart.Rotation += Engine.DeltaTime * (10f + p * 30f);
                heartAlpha = shrink;
                textAlpha = shrink;
                glitchJitter = p * 4f;
                yield return null;
            }
            heart.RemoveSelf();
            glitchJitter = 0f;
            textAlpha = 0f;

            // Fade to white, then back in.
            ScreenWipe.WipeColor = Color.White;
            FadeWipe fadeOut = new FadeWipe(level, wipeIn: false) { Duration = 1.2f };
            yield return fadeOut.Duration;

            ScreenWipe.WipeColor = Color.White;
            FadeWipe fadeIn = new FadeWipe(level, wipeIn: true) { Duration = 1.2f };
            yield return fadeIn.Duration;

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            heart?.RemoveSelf();
            if (player != null)
                player.StateMachine.State = global::Celeste.Player.StNormal;
        }

        public override void Render()
        {
            base.Render();

            if (textAlpha <= 0f)
                return;

            string line = showLineTwo ? LineTwo : LineOne;
            Vector2 center = new Vector2(1920f / 2f, 1080f / 2f);

            // Cheap RGB-split glitch: offset red/cyan copies plus small random jitter.
            float jx = glitchJitter > 0f ? ((float)rng.NextDouble() - 0.5f) * glitchJitter * 4f : 0f;
            float jy = glitchJitter > 0f ? ((float)rng.NextDouble() - 0.5f) * glitchJitter * 2f : 0f;
            Vector2 jitter = new Vector2(jx, jy);

            ActiveFont.DrawOutline(line, center + jitter + new Vector2(-3f, 0f), new Vector2(0.5f, 0.5f), Vector2.One,
                Color.Red * (textAlpha * 0.6f), 0f, Color.Transparent);
            ActiveFont.DrawOutline(line, center + jitter + new Vector2(3f, 0f), new Vector2(0.5f, 0.5f), Vector2.One,
                Color.Cyan * (textAlpha * 0.6f), 0f, Color.Transparent);
            ActiveFont.DrawOutline(line, center + jitter, new Vector2(0.5f, 0.5f), Vector2.One,
                Color.White * textAlpha, 2f, Color.Black * textAlpha);
        }
    }
}
