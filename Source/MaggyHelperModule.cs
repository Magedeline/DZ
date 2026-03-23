using System;
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
            global::MaggyHelper.AreaModeExtender.Load();
            global::MaggyHelper.MonoModHooks.Load();
        }

        public override void Unload()
        {
            global::MaggyHelper.MonoModHooks.Unload();
            global::MaggyHelper.AreaModeExtender.Unload();
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
    }
}