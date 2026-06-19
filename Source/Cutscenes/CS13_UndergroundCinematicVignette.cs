#nullable enable

using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 13 underground cinematic vignette.
    /// Plays Ralsei's recounting of the last reality and the monsters' fall.
    /// </summary>
    [HotReloadable]
    class Cs13UndergroundCinematicVignette : DesoloZantasVignette
    {
        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private readonly Textbox mainTextbox;
        private readonly Textbox endTextbox;
        private float fade = 0f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? textCoroutine;
        private float textAlpha = 0f;
        private EventInstance? cinematicMusic;

        public override bool CanPause => menu == null;

        public Cs13UndergroundCinematicVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = Audio.CurrentMusic;
            Audio.CurrentMusic = null;
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            mainTextbox = new Textbox("DZ_CH13_UNDERGROUND_CINEMATIC");
            endTextbox = new Textbox("DZ_CH13_RALSEI_END_CINEMATIC");
            textCoroutine = new Coroutine(TextSequence());
        }

        public Cs13UndergroundCinematicVignette(Session session) : this(session, null)
        {
            Add(new FadeWipe(this, true));
        }

        private IEnumerator TextSequence()
        {
            yield return 1f;

            // Atmospheric music for the underground story
            cinematicMusic = Audio.Play("event:/music/lvl6/madeline_and_theo");
            Audio.SetMusicParam(nameof(fade), 1f);
            yield return 2f;

            yield return Say(mainTextbox);
            yield return 0.5f;

            yield return Say(endTextbox);
            yield return 1f;

            Audio.SetMusicParam("pitch", 1f);
            yield return 1f;
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
                    textCoroutine?.Update();
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }
            pauseFade = Calc.Approach(pauseFade, menu != null ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0f, Engine.DeltaTime * 3f);
            fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
        }

        public override void OpenMenu()
        {
            if (!CanPause || Paused)
                return;

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
            textCoroutine = null;
            Audio.CurrentMusic = areaMusic;
            menu?.RemoveSelf();
            menu = null;
            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new LevelLoader(session);
            });
            fadeWipe.OnUpdate = delegate (float f)
            {
                textAlpha = Math.Min(textAlpha, 1f - f);
            };
            exiting = true;
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            // Dark underground backdrop
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black);
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
            cinematicMusic?.setPaused(true);
        }

        private void ResumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
            cinematicMusic?.setPaused(false);
        }

        private void StopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
            cinematicMusic?.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
