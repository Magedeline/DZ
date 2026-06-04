using Celeste;
using Celeste.NPCs;
using System.Collections.Generic;
using Facings = Celeste.Facings;

namespace Celeste.Cutscenes
{
    public class CS17_EndingMod : CutsceneEntity
    {
        private static readonly Vector2 TargetCenter = new Vector2(960f, 540f); // 1920x1080 / 2
        private global::Celeste.Player player;
        private Npc17Toriel toriel;
        private Npc17Theo theo;
        private BadelineDummy badeline;
        private Entity oshiro;
        // Guest NPCs for the ending
        private NpcSans sans;
        private NpcUndyne undyne;
        private NpcAlphys alphys;
        private NpcPapyrus papyrus;
        private NpcSusie susie;
        private NpcMagolor magolor;
        private NpcKingDedede dedede;
        private NpcBandanaDee bandanaDee;
        private NpcAdeleine adeleine;
        private NpcMadelineMother mother;
        private List<Entity> guestNpcs = new List<Entity>();
        private Image vignette;
        private Image vignettebg;
        private string endingDialog;
        private float fade;
        private bool showVersion;
        private float versionAlpha;
        private Coroutine cutscene;
        private string version = Engine.Instance.Version.ToString();

        public CS17_EndingMod() : base(false, true)
        {
            Tag = Tags.HUD | Tags.PauseUpdate;
            RemoveOnSkipped = false;
        }

        public override void OnBegin(Level level)
        {
            level.SaveQuitDisabled = true;
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

            Add(vignettebg = new Image(GFX.Portraits["finalbg"]));
            vignettebg.Visible = false;
            Add(vignette = new Image(GFX.Portraits[id]));
            vignette.Visible = false;
            vignette.CenterOrigin();
            vignette.Position = TargetCenter;
            Add(cutscene = new Coroutine(Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.ZoomSnap(new Vector2(164f, 120f), 2f);
            level.Wipe?.Cancel();
            new FadeWipe(level, true, null);

            while (player == null)
            {
                theo = level.Entities.FindFirst<Npc17Theo>();
                toriel = level.Entities.FindFirst<Npc17Toriel>();
                player = level.Tracker.GetEntity<global::Celeste.Player>();
                // Find guest NPCs
                sans = level.Entities.FindFirst<NpcSans>();
                undyne = level.Entities.FindFirst<NpcUndyne>();
                alphys = level.Entities.FindFirst<NpcAlphys>();
                papyrus = level.Entities.FindFirst<NpcPapyrus>();
                susie = level.Entities.FindFirst<NpcSusie>();
                magolor = level.Entities.FindFirst<NpcMagolor>();
                dedede = level.Entities.FindFirst<NpcKingDedede>();
                bandanaDee = level.Entities.FindFirst<NpcBandanaDee>();
                adeleine = level.Entities.FindFirst<NpcAdeleine>();
                mother = level.Entities.FindFirst<NpcMadelineMother>();
                yield return null;
            }

            player.StateMachine.State = Player.StDummy;
            yield return 1f;
            yield return player.DummyWalkToExact((int)player.X + 16, false, 1f, false);
            yield return 0.25f;

            yield return Textbox.Say("EPMOD_CABIN", new Func<IEnumerator>[]
            {
                BadelineEmerges,
                OshiroEnters,
                OshiroSettles,
                MaddyTurns
            });

            yield return new FadeWipe(level, false, null) { Duration = 1.5f }.Wait();
            fade = 1f;
            yield return Textbox.Say("EP_PIE_START");
            yield return 0.5f;

            vignettebg.Visible = true;
            vignette.Visible = true;
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

            p = 1f;
            float p2;
            for (p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / p)
            {
                float num = Ease.CubeOut(p2);
                vignette.Position = Vector2.Lerp(TargetCenter, TargetCenter + new Vector2(0f, 140f), num);
                vignette.Scale = Vector2.One * (0.65f + 0.35f * (1f - num));
                vignette.Rotation = -0.025f * num;
                yield return null;
            }

            yield return Textbox.Say(endingDialog);
            yield return 0.25f;

            p = 2f;
            Vector2 posFrom = vignette.Position;
            p2 = vignette.Rotation;
            float scaleFrom = vignette.Scale.X;

            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / p)
            {
                float amount = Ease.CubeOut(p3);
                vignette.Position = Vector2.Lerp(posFrom, TargetCenter, amount);
                vignette.Scale = Vector2.One * MathHelper.Lerp(scaleFrom, 1f, amount);
                vignette.Rotation = MathHelper.Lerp(p2, 0f, amount);
                yield return null;
            }

            EndCutscene(level, false);
        }

        public override void OnEnd(Level level)
        {
            if (Scene == null)
            {
                return;
            }

            if (vignette != null)
            {
                vignette.Visible = true;
                vignette.Color = Color.White;
                vignette.Position = TargetCenter;
                vignette.Scale = Vector2.One;
                vignette.Rotation = 0f;
            }

            if (player != null)
            {
                player.Speed = Vector2.Zero;
            }

            Textbox textbox = level.Entities.FindFirst<Textbox>();
            textbox?.RemoveSelf();

            cutscene?.RemoveSelf();
            Add(new Coroutine(EndingRoutine(), true));
        }

        private IEnumerator EndingRoutine()
        {
            Level.InCutscene = true;
            Level.PauseLock = true;
            yield return 0.5f;

            TimeSpan timeSpan = TimeSpan.FromTicks(SaveData.Instance.Time);
            string time = $"{(int)timeSpan.TotalHours}{timeSpan:\\:mm\\:ss\\.fff}";

            StrawberriesCounter strawbs = new StrawberriesCounter(true, SaveData.Instance.TotalStrawberries, 175, true);
            DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, true, SaveData.Instance.TotalDeaths, 0);
            TimeDisplay timeDisplay = new TimeDisplay(time);

            float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(time, 1f);
            Add(strawbs);
            Add(deaths);
            Add(timeDisplay);

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

            Level.CompleteArea(false, false, false);
        }

        private IEnumerator MaddyTurns()
        {
            yield return 0.1f;
            // Flip facing direction then flip back (causes animation)
            player.Facing = (Facings)(-(int)player.Facing);
            yield return 0.1f;
        }

        private IEnumerator BadelineEmerges()
        {
            Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f, null, null);
            Level.Session.Inventory.Dashes = 1;
            player.Dashes = 1;
            Level.Add(badeline = new BadelineDummy(player.Position));
            Audio.Play("guid://{450fb5b3-e9e3-45d8-9f34-ba05e292958f}", player.Position);
            badeline.Sprite.Scale.X = 1f;
            yield return badeline.FloatTo(player.Position + new Vector2(-12f, -16f), new int?(1), false, false, false);
        }

        private IEnumerator OshiroEnters()
        {
            yield return new FadeWipe(Level, false, null) { Duration = 1.5f }.Wait();
            fade = 1f;
            yield return 0.25f;

            if (toriel == null || theo == null || player == null || badeline == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "CS17_EndingMod: Missing required entities (toriel, theo, player, or badeline)");
                yield break;
            }

            float x = player.X;
            player.X = toriel.X + 8f;
            badeline.X = player.X + 12f;
            player.Facing = Facings.Left;
            badeline.Sprite.Scale.X = -1f;
            toriel.X = x + 8f;
            theo.X += 16f;

            // Position guest NPCs in a semi-circle behind the main group
            float baseX = toriel.X;
            float baseY = toriel.Y;
            if (sans != null) { sans.X = baseX - 80f; sans.Y = baseY; guestNpcs.Add(sans); }
            if (papyrus != null) { papyrus.X = baseX - 100f; papyrus.Y = baseY; guestNpcs.Add(papyrus); }
            if (undyne != null) { undyne.X = baseX - 60f; undyne.Y = baseY - 10f; guestNpcs.Add(undyne); }
            if (alphys != null) { alphys.X = baseX - 120f; alphys.Y = baseY - 5f; guestNpcs.Add(alphys); }
            if (susie != null) { susie.X = baseX + 60f; susie.Y = baseY; guestNpcs.Add(susie); }
            if (magolor != null) { magolor.X = baseX + 80f; magolor.Y = baseY - 10f; guestNpcs.Add(magolor); }
            if (dedede != null) { dedede.X = baseX + 100f; dedede.Y = baseY; guestNpcs.Add(dedede); }
            if (bandanaDee != null) { bandanaDee.X = baseX + 120f; bandanaDee.Y = baseY + 5f; guestNpcs.Add(bandanaDee); }
            if (adeleine != null) { adeleine.X = baseX - 40f; adeleine.Y = baseY - 15f; guestNpcs.Add(adeleine); }
            if (mother != null) { mother.X = baseX + 40f; mother.Y = baseY - 10f; guestNpcs.Add(mother); }

            Level.Add(oshiro = new Entity(new Vector2(toriel.X - 24f, toriel.Y + 4f)));
            oshiro.Add(new global::Celeste.OshiroSprite(1));
            oshiro.Depth = 100;
            fade = 0f;

            FadeWipe fadeWipe = new FadeWipe(Level, true, null) { Duration = 1f };
            yield return 0.25f;

            while (oshiro.Y > toriel.Y - 4f)
            {
                oshiro.Y -= Engine.DeltaTime * 32f;
                yield return null;
            }
        }

        private IEnumerator OshiroSettles()
        {
            if (oshiro == null || toriel == null)
            {
                yield break;
            }

            Vector2 from = oshiro.Position;
            Vector2 to = oshiro.Position + new Vector2(40f, 8f);

            for (float p = 0f; p < 1f; p += Engine.DeltaTime)
            {
                oshiro.Position = Vector2.Lerp(from, to, p);
                yield return null;
            }

            toriel.Sprite.Scale.X = 1f;
        }

        public override void Update()
        {
            versionAlpha = Calc.Approach(versionAlpha, showVersion ? 1f : 0f, Engine.DeltaTime * 5f);
            base.Update();
        }

        public override void Render()
        {
            if (fade > 0f)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
            }

            base.Render();

            if (global::Celeste.Settings.Instance.SpeedrunClock != SpeedrunType.Off && versionAlpha > 0f)
            {
                AreaComplete.VersionNumberAndVariants(version, versionAlpha, 1f);
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

