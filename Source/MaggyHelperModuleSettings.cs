namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSettings : EverestModuleSettings
    {
        public int BossDifficultyMultiplier { get; set; } = 1;
        public bool EnableBossMusic { get; set; } = true;
        public bool EnableKirbyPlayer { get; set; } = false;
        public bool KirbyPlayerEnabled { get; set; } = false;
        public int KirbyMaxFloatJumps { get; set; } = 5;
        public bool DebugMode { get; set; }
        public bool SkipModIntro { get; set; }
    }
}