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
    /// Chapter 13 TV/video vignette.
    /// Plays a frame-based "video" (sequence of images) behind a textbox dialog,
    /// used for Tenna's pre-intro commercial and the Earth destruction news report.
    /// </summary>
    [HotReloadable]
    public class Cs13TennaVideoVignette : DesoloZantasVignette
    {
        public static class Videos
        {
            public const string TennaPreIntro = "DZ_CH13_TENNA_PRE_INTRO";
            public const string EarthDestruction = "DZ_CH13_TENNA_EARTH_VIDEO_destructions";
        }

        private readonly Session session;
        private readonly string dialogKey;
        private readonly string videoPath;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private readonly List<Image> frames = new();
        private readonly Textbox textbox;

        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? sequenceCoroutine;
        private float fade = 1f;
        private float videoAlpha = 0f;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private float frameDuration = 0.1f;
        private bool videoLooping = true;
        private bool playing = false;
        private EventInstance? videoSfx;
        private float scanlineTimer = 0f;

        public override bool CanPause => menu == null;

        public Cs13TennaVideoVignette(Session session, string dialogKey, string videoPath, float frameDuration = 0.1f, bool loop = true)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.dialogKey = dialogKey ?? throw new ArgumentNullException(nameof(dialogKey));
            this.videoPath = videoPath ?? throw new ArgumentNullException(nameof(videoPath));
            this.frameDuration = frameDuration;
            this.videoLooping = loop;

            areaMusic = Audio.CurrentMusic;
            Audio.CurrentMusic = null;

            Add(hud = new HudRenderer());
            RendererList.UpdateLists();

            textbox = new Textbox(dialogKey);
            LoadFrames(videoPath);

            sequenceCoroutine = new Coroutine(Sequence());
        }

        public Cs13TennaVideoVignette(Session session, string dialogKey, string videoPath) : this(session, dialogKey, videoPath, 0.1f, true)
        {
            Add(new FadeWipe(this, true));
        }

        private void LoadFrames(string prefix)
        {
            frames.Clear();
            for (int i = 0; i < 999; i++)
            {
                string path = $"{prefix}_{i:D2}";
                if (!GFX.Game.Has(path))
                {
                    if (i == 0)
                    {
                        // Try a single-frame fallback (e.g. "video/tenna_pre_intro" with no _00 suffix)
                        if (GFX.Game.Has(prefix))
                        {
                            var img = new Image(GFX.Game[prefix]);
                            img.CenterOrigin();
                            img.Position = CelesteGame.TargetCenter;
                            frames.Add(img);
                        }
                    }
                    break;
                }

                var frame = new Image(GFX.Game[path]);
                frame.CenterOrigin();
                frame.Position = CelesteGame.TargetCenter;
                frames.Add(frame);
            }
        }

        private IEnumerator Sequence()
        {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f)
            {
                fade -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 0f;

            // Start video playback
            videoAlpha = 1f;
            currentFrame = 0;
            frameTimer = 0f;
            playing = true;

            // Optional screen hum / static sound
            videoSfx = Audio.Play("event:/DZ/game/13_tenna/video_static", Vector2.Zero);

            yield return 0.5f;

            // Show dialog textbox on top of the video
            yield return Say(textbox);

            yield return 0.5f;

            // Fade out video and end
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

                    if (playing && frames.Count > 0)
                    {
                        frameTimer += Engine.DeltaTime;
                        if (frameTimer >= frameDuration)
                        {
                            frameTimer -= frameDuration;
                            currentFrame++;
                            if (currentFrame >= frames.Count)
                            {
                                currentFrame = videoLooping ? 0 : frames.Count - 1;
                            }
                        }
                    }

                    scanlineTimer += Engine.DeltaTime;
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
            playing = false;
            Audio.CurrentMusic = areaMusic;
            menu?.RemoveSelf();
            menu = null;
            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new LevelLoader(session);
            });
            fadeWipe.OnUpdate = delegate (float f)
            {
                videoAlpha = Math.Min(videoAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Black background
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black);

            // TV / video frame
            if (frames.Count > 0 && currentFrame < frames.Count && videoAlpha > 0f)
            {
                var frame = frames[currentFrame];
                frame.Color = Color.White * videoAlpha;
                frame.Render();

                // Scanline effect
                float scanlineOffset = (scanlineTimer * 100f) % 4f;
                for (float y = scanlineOffset; y < 1080f; y += 4f)
                {
                    Draw.Rect(0f, y, 1920f, 1f, Color.Black * 0.15f * videoAlpha);
                }

                // Vignette darkening around the edges
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * 0.2f * videoAlpha);
            }
            else if (frames.Count == 0)
            {
                // No video frames: show a placeholder message for development
                string missing = $"[video: {videoPath}]";
                var measure = ActiveFont.Measure(missing);
                ActiveFont.DrawOutline(missing, CelesteGame.TargetCenter, new Vector2(0.5f, 0.5f), Vector2.One, Color.White, 2f, Color.Black);
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
            {
                sound.Pause();
            }
            videoSfx?.setPaused(true);
        }

        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            videoSfx?.setPaused(false);
        }

        private void StopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            videoSfx?.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
