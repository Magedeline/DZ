#nullable enable

using DZ;
using FMOD.Studio;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 16 Els Outro Vignette - Session-based outro sequence
    /// </summary>
    [HotReloadable]
    public class CS16_ElsOutro : DesoloZantasVignette
    {
        public const string FLAG = "ch16_els_outro_trigger";
        public static class LoadingVignetteText
        {
            public const string Dialog = "DZ_CH16_ELS_OUTRO";
        }

        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private float fade = 1f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? textCoroutine;
        private Player? player;

        public override bool CanPause => menu == null;

        public CS16_ElsOutro(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();
            textCoroutine = new Coroutine(Cutscene());
        }

        public CS16_ElsOutro(Session session1) : this(session1, null)
        {
            Add(new MaggyHiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator Cutscene()
        {
            yield return 0.5f;

            // Fade in from black
            while (fade > 0f)
            {
                fade -= Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 0f;

            yield return 1f;

            yield return Textbox.Say("DZ_CH16_ELS_OUTRO");

            yield return 0.5f;

            // Fade to black
            fade = 0f;
            while (fade < 1f)
            {
                fade += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 1f;

            yield return 1f;

            // Return to overworld
            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
            });

            exiting = true;
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
        }

        public override void OpenMenu()
        {
            if (!CanPause || Paused) return;
            Paused = true;
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(skipToEnd));
            menu.OnCancel = menu.OnESC = menu.OnPause = closeMenu;
        }

        public override void CloseMenu()
        {
            Paused = false;
            Audio.Play("event:/ui/game/unpause");
            if (menu != null)
            {
                menu.RemoveSelf();
            }
            menu = null;
        }

        private void closeMenu() => CloseMenu();

        private void skipToEnd()
        {
            textCoroutine = null;
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }

            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
            });

            exiting = true;
        }

        public override void Render()
        {
            base.Render();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);

            if (fade > 0f)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            Draw.SpriteBatch.End();
        }
    }
}
