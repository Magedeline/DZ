namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSettings : EverestModuleSettings
    {
        public bool BossesExampleResetKeysForSession { get; set; }
        public int BossDifficultyMultiplier { get; set; } = 1;
        public bool EnableBossMusic { get; set; } = true;
        public bool EnableKirbyPlayer { get; set; } = true;
        public bool KirbyPlayerEnabled { get; set; } = true;
        public int KirbyMaxFloatJumps { get; set; } = 5;
        public bool DebugMode { get; set; }
        public bool SkipModIntro { get; set; }
        public bool HasSeenIntroWarning { get; set; }
    }
}