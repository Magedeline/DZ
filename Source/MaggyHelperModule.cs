using System;
using Celeste.Mod.MaggyHelper.BossesExample;
using MaggyHelper.Cutscenes;
using Monocle;

namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModule : EverestModule
    {
        public static MaggyHelperModule Instance { get; private set; }

        public override Type SettingsType => typeof(MaggyHelperModuleSettings);
        public static new MaggyHelperModuleSettings Settings => (MaggyHelperModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(MaggyHelperModuleSession);
        public static new MaggyHelperModuleSession Session => (MaggyHelperModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(MaggyHelperModuleSaveData);
        public static new MaggyHelperModuleSaveData SaveData => (MaggyHelperModuleSaveData)Instance._SaveData;

        // Runtime flags
        public static bool LaunchPart1Credits { get; set; }
        public static bool LaunchPart2Credits { get; set; }

        public static readonly string Chapter16CorruptionSid = AreaModeExtender.BuildASideSID("16_Corruption");
        public static readonly string Chapter17EpilogueSid = AreaModeExtender.BuildASideSID("17_Epilogue");
        public const string Chapter17CreditsLevel = "credits-summit";

        // Shared resources
        public static SpriteBank SpriteBank { get; set; }
        public static ParticleType P_StarExplosion { get; set; }
        public static global::MaggyHelper.ProphecyFontRenderer ProphecyFont { get; private set; }

        public MaggyHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            BossesExampleModule.Load();
            global::MaggyHelper.AreaMapData.Initialize();
            global::MaggyHelper.AreaModeExtender.Load();
            global::MaggyHelper.AreaCompleteHooks.Load();
            global::MaggyHelper.IntroRemixHooks.Load();
            global::MaggyHelper.MonoModHooks.Load();
            global::MaggyHelper.VignetteHooks.Load();
            global::MaggyHelper.Cutscenes.IntroWarning.Load();

            // Hook level exit to clean up static state
            Everest.Events.Level.OnExit += OnLevelExit;

            // Reset credits launch flags
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
        }

        private static void OnLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            global::MaggyHelper.Effects.IceEffects.ClearAll();
            global::MaggyHelper.Effects.LightningEffects.ClearAll();
            global::MaggyHelper.Effects.ElementalEffectsManager.StopAllEffects();
            global::MaggyHelper.Entities.EnemyBossManager.Reset();
        }

        public override void Unload()
        {
            global::MaggyHelper.Cutscenes.IntroWarning.Unload();
            global::MaggyHelper.VignetteHooks.Unload();
            global::MaggyHelper.MonoModHooks.Unload();
            global::MaggyHelper.IntroRemixHooks.Unload();
            global::MaggyHelper.AreaCompleteHooks.Unload();
            global::MaggyHelper.AreaModeExtender.Unload();
            BossesExampleModule.Unload();

            // Unhook level exit cleanup
            Everest.Events.Level.OnExit -= OnLevelExit;

            // Reset credits state
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
            ProphecyFont = null;
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            BossesExampleModule.LoadContent(firstLoad);
            ProphecyFont = new global::MaggyHelper.ProphecyFontRenderer();
        }

        public static bool IsChapter17EpilogueCompleted()
        {
            return MaggyHelperModule.SaveData?.IsChapterCompleted(Chapter17EpilogueSid) == true;
        }

        public static void MarkChapter17EpilogueCompleted()
        {
            MaggyHelperModule.SaveData?.CompleteChapter(Chapter17EpilogueSid);

            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = false;
                MaggyHelperModule.Session.CreditsPhase = 0;
                MaggyHelperModule.Session.CreditsCompleted = true;
            }
        }

        public static void LaunchChapter17Epilogue()
        {
            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = false;
                MaggyHelperModule.Session.CreditsPhase = 2;
                MaggyHelperModule.Session.CreditsCompleted = false;
            }

            AreaKey targetArea = AreaData.Get(Chapter17EpilogueSid)?.ToKey() ?? new AreaKey(8);
            LevelEnter.Go(new Session(targetArea), false);
        }

        /// <summary>
        /// Allows other mods (like BrokemiaHelper) to detect if MaggyHelper/Player is available.
        /// Returns true if the MaggyHelper Player type is loaded and available.
        /// </summary>
        public static bool IsMaggyPlayerAvailable()
        {
            try
            {
                Type playerType = Type.GetType("MaggyHelper.Entities.Player, MaggyHelper");
                return playerType != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the MaggyHelper Player type if available. Use this for reflection-based interaction.
        /// </summary>
        public static Type GetMaggyPlayerType()
        {
            try
            {
                return Type.GetType("MaggyHelper.Entities.Player, MaggyHelper");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Launches the Chapter 17 credits sequence from a level session.
        /// Loads the Chapter 17 epilogue area directly into the credits-summit room.
        /// </summary>
        public static void LaunchCredits(Session session)
        {
            Session creditsSession = session;
            AreaData creditsArea = AreaData.Get(Chapter17EpilogueSid);

            if (creditsArea != null)
            {
                creditsSession = new Session(creditsArea.ToKey());
                creditsSession.RespawnPoint = null;
                creditsSession.FirstLevel = false;
                creditsSession.Level = Chapter17CreditsLevel;
            }
            else if (creditsSession == null)
            {
                return;
            }
            else
            {
                creditsSession.RespawnPoint = null;
                creditsSession.FirstLevel = false;
                creditsSession.Level = Chapter17CreditsLevel;
            }

            // Update module session state
            if (MaggyHelperModule.Session != null)
            {
                MaggyHelperModule.Session.InCredits = true;
                MaggyHelperModule.Session.CreditsPhase = 1;
                MaggyHelperModule.Session.CreditsCompleted = false;
            }

            creditsSession.Audio.Music.Event = "event:/desolozantas/music/lvl17/main";
            creditsSession.Audio.Apply(false);

            Engine.Scene = new LevelLoader(creditsSession)
            {
                PlayerIntroTypeOverride = Player.IntroTypes.None,
                Level =
                {
                    new CS17_Credits()
                }
            };
        }

        /// <summary>
        /// Console command: maggy_credits — launches the credits sequence from the current level.
        /// </summary>
        [Command("maggy_credits", "Launches the Chapter 17 credits sequence from the current level.")]
        private static void Cmd_LaunchCredits()
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands?.Log("[MaggyHelper] Must be in a level to launch credits.");
                return;
            }

            Engine.Commands?.Log("[MaggyHelper] Launching Chapter 17 credits...");
            LaunchCredits(level.Session);
        }
    }
}