using System;
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

        // Shared resources
        public static SpriteBank SpriteBank { get; set; }
        public static ParticleType P_StarExplosion { get; set; }

        public MaggyHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            global::MaggyHelper.AreaMapData.Initialize();
            global::MaggyHelper.AreaModeExtender.Load();
            global::MaggyHelper.IntroRemixHooks.Load();
            global::MaggyHelper.MonoModHooks.Load();
            global::MaggyHelper.VignetteHooks.Load();

            // Reset credits launch flags
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
        }

        public override void Unload()
        {
            global::MaggyHelper.VignetteHooks.Unload();
            global::MaggyHelper.MonoModHooks.Unload();
            global::MaggyHelper.IntroRemixHooks.Unload();
            global::MaggyHelper.AreaModeExtender.Unload();

            // Reset credits state
            LaunchPart1Credits = false;
            LaunchPart2Credits = false;
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
        /// Sets the session to the credits-summit room and adds the CS17_Credits cutscene entity.
        /// </summary>
        public static void LaunchCredits(Session session)
        {
            if (session == null)
                return;

            // Update module session state
            if (Session != null)
            {
                Session.InCredits = true;
                Session.CreditsPhase = 1;
                Session.CreditsCompleted = false;
            }

            session.RespawnPoint = null;
            session.FirstLevel = false;
            session.Level = "credits-summit";
            session.Audio.Music.Event = "event:/desolozantas/music/lvl17/main";
            session.Audio.Apply(false);

            Engine.Scene = new LevelLoader(session)
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