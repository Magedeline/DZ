#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 13 cassette tape vignette.
    /// Plays DZ_CH13_TAPE_00 through DZ_CH13_TAPE_05 in sequence,
    /// then DZ_CH13_TAPE_FINAL, all over a dark "old tape" backdrop
    /// with scanlines and VHS noise.
    /// </summary>
    [HotReloadable]
    public class Cs13TapeVignette : DesoloZantasVignette
    {
        public static class TapeKeys
        {
            public const string Tape00    = "DZ_CH13_TAPE_00";
            public const string Tape01    = "DZ_CH13_TAPE_01";
            public const string Tape02    = "DZ_CH13_TAPE_02";
            public const string Tape03    = "DZ_CH13_TAPE_03";
            public const string Tape04    = "DZ_CH13_TAPE_04";
            public const string Tape05    = "DZ_CH13_TAPE_05";
            public const string TapeFinal = "DZ_CH13_TAPE_FINAL";
        }

        private static readonly string[] TapeSequence =
        {
            TapeKeys.Tape00,
            TapeKeys.Tape01,
            TapeKeys.Tape02,
            TapeKeys.Tape03,
            TapeKeys.Tape04,
            TapeKeys.Tape05,
            TapeKeys.TapeFinal,
        };

        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private readonly string[] keysToPlay;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? sequenceCoroutine;

        private float fade = 1f;
        private float scanlineTimer = 0f;
        private float noiseTimer = 0f;
        private float flickerAlpha = 0f;
        private readonly Random rng = new Random();

        private EventInstance? tapeSfx;

        public override bool CanPause => menu == null;

        /// <summary>Plays all seven tapes in order (00–05 + FINAL).</summary>
        public Cs13TapeVignette(Session session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            keysToPlay = TapeSequence;
            areaMusic = Audio.CurrentMusic;
            Audio.CurrentMusic = null;

            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            Add(new FadeWipe(this, true));

            sequenceCoroutine = new Coroutine(Sequence());
        }

        /// <summary>Plays a single tape dialog key.</summary>
        public Cs13TapeVignette(Session session, string singleKey)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            keysToPlay = new[] { singleKey ?? throw new ArgumentNullException(nameof(singleKey)) };
            areaMusic = Audio.CurrentMusic;
            Audio.CurrentMusic = null;

            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            Add(new FadeWipe(this, true));

            sequenceCoroutine = new Coroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f)
            {
                fade -= Engine.DeltaTime * 0.8f;
                yield return null;
            }
            fade = 0f;

            // Start tape hiss/static ambience
            tapeSfx = Audio.Play("event:/Mods/pusheen/game/13_tenna/video_static", Vector2.Zero);

            yield return 0.5f;

            // Play each tape dialog entry in sequence
            for (int i = 0; i < keysToPlay.Length; i++)
            {
                string key = keysToPlay[i];

                // Brief VHS glitch between tapes
                if (i > 0)
                {
                    flickerAlpha = 1f;
                    yield return 0.08f;
                    flickerAlpha = 0f;
                    yield return 0.05f;
                    flickerAlpha = 0.6f;
                    yield return 0.06f;
                    flickerAlpha = 0f;
                    yield return 0.3f;
                }

                yield return Say(new Textbox(key));

                // Short breath between tapes
                yield return 0.4f;
            }

            yield return 0.5f;

            StartGame();
        }

        private static IEnumerator Say(Textbox textbox)
        {
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
            {
                yield return true;
            }
        }

        public override void Update()
        {
            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    sequenceCoroutine?.Update();
                    scanlineTimer += Engine.DeltaTime;
                    noiseTimer    += Engine.DeltaTime;
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }

            pauseFade = Calc.Approach(pauseFade, menu != null ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0f, Engine.DeltaTime * 3f);
        }

        public override void OpenMenu()
        {
            if (!CanPause || Paused) return;

            Paused = true;
            PauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(CloseMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(StartGame));
            menu.OnCancel = menu.OnESC = menu.OnPause = CloseMenu;
        }

        public override void CloseMenu()
        {
            Paused = false;
            ResumeSfx();
            Audio.Play("event:/ui/game/unpause");
            menu?.RemoveSelf();
            menu = null;
        }

        private void StartGame()
        {
            StopSfx();
            sequenceCoroutine = null;
            Audio.CurrentMusic = areaMusic;
            menu?.RemoveSelf();
            menu = null;

            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new LevelLoader(session);
            });
            exiting = true;
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Dark background
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black);

            // Subtle dark warm tint (old tape feel)
            Draw.Rect(0f, 0f, 1920f, 1080f, new Color(10, 5, 0) * 0.6f);

            // Horizontal scanlines
            float scanlineOffset = (scanlineTimer * 60f) % 4f;
            for (float y = scanlineOffset; y < 1080f; y += 4f)
            {
                Draw.Rect(0f, y, 1920f, 1f, Color.Black * 0.25f);
            }

            // Random VHS noise dots
            if ((int)(noiseTimer * 24f) % 2 == 0)
            {
                for (int i = 0; i < 18; i++)
                {
                    float nx = (float)(rng.NextDouble() * 1920.0);
                    float ny = (float)(rng.NextDouble() * 1080.0);
                    float nw = (float)(rng.NextDouble() * 3.0 + 1.0);
                    Draw.Rect(nx, ny, nw, 1f, Color.White * 0.08f);
                }
            }

            // VHS glitch flash between tapes
            if (flickerAlpha > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White * flickerAlpha * 0.3f);
                // Horizontal tear
                float tearY = (float)(rng.NextDouble() * 1080.0);
                Draw.Rect(0f, tearY, 1920f, 4f, Color.White * flickerAlpha * 0.6f);
            }

            // Vignette edges
            for (int s = 0; s < 6; s++)
            {
                float strength = (6 - s) / 6f * 0.35f;
                Draw.Rect(0f,          0f,            1920f, s * 12f,  Color.Black * strength);
                Draw.Rect(0f,          1080f - s*12f, 1920f, s * 12f,  Color.Black * strength);
                Draw.Rect(0f,          0f,            s*12f, 1080f,    Color.Black * strength);
                Draw.Rect(1920f-s*12f, 0f,            s*12f, 1080f,    Color.Black * strength);
            }

            // Fade overlay
            if (fade > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            Draw.SpriteBatch.End();
        }

        private void PauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
                sound.Pause();
            tapeSfx?.setPaused(true);
        }

        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
                sound.Resume();
            tapeSfx?.setPaused(false);
        }

        private void StopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
                sound.RemoveSelf();
            tapeSfx?.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
