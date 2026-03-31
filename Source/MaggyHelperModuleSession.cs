
namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSession : EverestModuleSession
    {
        private string currentKirbyPower = global::MaggyHelper.Extensions.KirbyMode.KirbyPowerState.None.ToString();

        public bool BossFightActive { get; set; }
        public string CurrentBossName { get; set; }
        public int BossesDefeated { get; set; }
        public bool IsKirbyModeActive { get; set; }
        public global::MaggyHelper.Entities.Bosses.CopyAbilityType CurrentCopyAbility { get; set; }
        public string CurrentKirbyPower
        {
            get => currentKirbyPower;
            set => currentKirbyPower = string.IsNullOrWhiteSpace(value)
                ? global::MaggyHelper.Extensions.KirbyMode.KirbyPowerState.None.ToString()
                : value;
        }
        public int EnemiesDefeated { get; set; }

        // Credits state
        public bool InCredits { get; set; }
        public int CreditsPhase { get; set; }
        public bool CreditsCompleted { get; set; }
    }
}