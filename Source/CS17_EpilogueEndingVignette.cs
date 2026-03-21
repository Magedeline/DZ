#nullable enable

using FMOD.Studio;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Chapter 17 Epilogue Ending Vignette - Full screen pie cutscene
    /// </summary>
    public class Cs17EpilogueEndingVignette : Scene
    {
        public static class LoadingVignetteText
        {
            public const string Dialog = "EP_PIE_START";
        }

        private readonly Session session;
        private readonly string? areaMusic;
        private readonly HudRenderer hud;
        private float fade = 0f;
        private TextMenu? menu;
        private float pauseFade = 0f;
        private bool exiting;
        private Coroutine? textCoroutine;
        private Image? vignette;
        private Image? vignettebg;
        private string endingDialog;
        private bool showVersion;
        private float versionAlpha;
        private string version = Celeste.Celeste.Instance.Version.ToString();

        public bool CanPause => menu == null;

        public Cs17EpilogueEndingVignette(Session session, TextMenu? menu = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.menu = menu;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply(forceSixteenthNoteHack: false);
            Add(hud = new HudRenderer());
            RendererList.UpdateLists();

            // Determine ending based on strawberry count
            int totalStrawberries = SaveData.Instance.TotalStrawberries;
            string id;

            if (totalStrawberries < 20)
            {
                id = "final1";
                endingDialog = "EPMOD_PIE_DISAPPOINTED";
            }
            else if (totalStrawberries < 50)
            {
                id = "final2";
                endingDialog = "EPMOD_PIE_GROSSED_OUT";
            }
            else if (totalStrawberries < 90)
            {
                id = "final3";
                endingDialog = "EPMOD_PIE_OKAY";
            }
            else if (totalStrawberries < 150)
            {
                id = "final4";
                endingDialog = "EPMOD_PIE_REALLY_GOOD";
            }
            else if (totalStrawberries < 175)
            {
                id = "final5";
                endingDialog = "EPMOD_PIE_AMAZING";
            }
            else
            {
                id = "truefinal";
                endingDialog = "EPMOD_PIE_CHEESECAKE_STRAWBERRY";
            }

            // Create entity to hold the images
            Entity imageHolder = new Entity();
            vignettebg = new Image(GFX.Portraits["finalbg"]);
            vignettebg.Visible = false;
            imageHolder.Add(vignettebg);
            
            vignette = new Image(GFX.Portraits[id]);
            vignette.Visible = false;
            vignette.CenterOrigin();
            vignette.Position = Celeste.Celeste.TargetCenter;
            imageHolder.Add(vignette);
            
            Add(imageHolder);

            textCoroutine = new Coroutine(epilogueSequence());
        }

        public Cs17EpilogueEndingVignette(Session session1) : this(session1, null)
        {
            Add(new HiresSnow());
            Add(new FadeWipe(this, true));
        }

        private IEnumerator epilogueSequence()
        {
            yield return 1f;

            // Fade to black
            while (fade < 1f)
            {
                fade += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            fade = 1f;

            yield return 0.5f;

            // Play pie start dialog
            yield return Textbox.Say("EP_PIE_START");
            yield return 0.5f;

            // Show vignette
            vignettebg!.Visible = true;
            vignette!.Visible = true;
            vignettebg.Color = Color.Black;
            vignette.Color = Color.White * 0f;

            float p;
            for (p = 0f; p < 1f; p += Engine.DeltaTime)
            {
                vignette.Color = Color.White * Ease.CubeIn(p);
                vignette.Scale = Vector2.One * (1f + 0.25f * (1f - p));
                vignette.Rotation = 0.05f * (1f - p);
                yield return null;
            }

            vignette.Color = Color.White;
            vignettebg.Color = Color.White;
            yield return 2f;

            // Zoom in on pie
            p = 1f;
            float p2;
            for (p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / p)
            {
                float num = Ease.CubeOut(p2);
                vignette.Position = Vector2.Lerp(Celeste.Celeste.TargetCenter, Celeste.Celeste.TargetCenter + new Vector2(0f, 140f), num);
                vignette.Scale = Vector2.One * (0.65f + 0.35f * (1f - num));
                vignette.Rotation = -0.025f * num;
                yield return null;
            }

            // Show ending dialog
            yield return Textbox.Say(endingDialog);
            yield return 0.25f;

            // Zoom back out
            p = 2f;
            Vector2 posFrom = vignette.Position;
            p2 = vignette.Rotation;
            float scaleFrom = vignette.Scale.X;

            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / p)
            {
                float amount = Ease.CubeOut(p3);
                vignette.Position = Vector2.Lerp(posFrom, Celeste.Celeste.TargetCenter, amount);
                vignette.Scale = Vector2.One * MathHelper.Lerp(scaleFrom, 1f, amount);
                vignette.Rotation = MathHelper.Lerp(p2, 0f, amount);
                yield return null;
            }

            yield return endingRoutine();
        }

        private IEnumerator endingRoutine()
        {
            yield return 0.5f;

            TimeSpan timeSpan = TimeSpan.FromTicks(SaveData.Instance.Time);
            string time = $"{(int)timeSpan.TotalHours}{timeSpan:\\:mm\\:ss\\.fff}";

            // Create entities to hold the components
            Entity statsEntity = new Entity();
            
            StrawberriesCounter strawbs = new StrawberriesCounter(true, SaveData.Instance.TotalStrawberries, 175, true);
            DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, true, SaveData.Instance.TotalDeaths, 0);
            TimeDisplay timeDisplay = new TimeDisplay(time);

            statsEntity.Add(strawbs);
            statsEntity.Add(deaths);
            statsEntity.Add(timeDisplay);
            
            float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(time, 1f);
            Add(statsEntity);

            Vector2 from = new Vector2(960f, 1180f);
            Vector2 to = new Vector2(960f, 940f);

            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
            {
                Vector2 value = Vector2.Lerp(from, to, Ease.CubeOut(p));
                strawbs.Position = value + new Vector2(-170f, 0f);
                deaths.Position = value + new Vector2(170f, 0f);
                timeDisplay.Position = value + new Vector2(-timeWidth / 2f, 100f);
                yield return null;
            }

            showVersion = true;
            yield return 0.25f;

            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }

            showVersion = false;
            yield return 0.25f;

            // Return to overworld
            completeEpilogue();
        }

        private void completeEpilogue()
        {
            stopSfx();
            textCoroutine = null;
            
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }

            var fadeWipe = new FadeWipe(this, false, delegate
            {
                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, new HiresSnow());
            });
            
            exiting = true;
        }

        public override void Update()
        {
            versionAlpha = Calc.Approach(versionAlpha, showVersion ? 1f : 0f, Engine.DeltaTime * 5f);

            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    textCoroutine?.Update();
                    if (Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        OpenMenu();
                    }
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }
            pauseFade = Calc.Approach(pauseFade, menu != null ? 1 : 0, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0f, Engine.DeltaTime * 3f);
        }

        public void OpenMenu()
        {
            pauseSfx();
            Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(closeMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(completeEpilogue));
            menu.OnCancel = menu.OnESC = menu.OnPause = closeMenu;
        }

        private void closeMenu()
        {
            resumeSfx();
            Audio.Play("event:/ui/game/unpause");
            if (menu != null)
            {
                menu.RemoveSelf();
            }
            menu = null;
        }

        public override void Render()
        {
            base.Render();
            
            if (fade > 0f)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
            }

            if (global::Celeste.Settings.Instance.SpeedrunClock != SpeedrunType.Off && versionAlpha > 0f)
            {
                AreaComplete.VersionNumberAndVariants(version, versionAlpha, 1f);
            }
        }

        private void pauseSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Pause();
            }
        }

        private void resumeSfx()
        {
            foreach (SoundSource sound in Tracker.GetComponents<SoundSource>())
            {
                sound.Resume();
            }
        }

        private void stopSfx()
        {
            List<Component> components = new();
            components.AddRange(Tracker.GetComponents<SoundSource>());
            foreach (SoundSource sound in components)
            {
                sound.RemoveSelf();
            }
        }

        public class TimeDisplay : Component
        {
            public Vector2 Position;
            public string Time;

            public TimeDisplay(string time) : base(true, true)
            {
                Time = time;
            }

            public override void Render()
            {
                SpeedrunTimerDisplay.DrawTime(RenderPosition, Time, 1f, true, false, false, 1f);
            }

            public Vector2 RenderPosition => ((Entity?.Position ?? Vector2.Zero) + Position).Round();
        }
    }
}





