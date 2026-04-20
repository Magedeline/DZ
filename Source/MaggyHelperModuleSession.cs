
namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSession : EverestModuleSession
    {
        private string currentKirbyPower = global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString();

        public bool BossFightActive { get; set; }
        public string CurrentBossName { get; set; }
        public int BossesDefeated { get; set; }
        public bool IsKirbyModeActive { get; set; }
        public global::Celeste.Entities.Bosses.CopyAbilityType CurrentCopyAbility { get; set; }
        public string CurrentKirbyPower
        {
            get => currentKirbyPower;
            set => currentKirbyPower = string.IsNullOrWhiteSpace(value)
                ? global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString()
                : value;
        }
        public int EnemiesDefeated { get; set; }

        // Credits state
        public bool InCredits { get; set; }
        public int CreditsPhase { get; set; }
        public bool CreditsCompleted { get; set; }

        // ── Mastery first-try tracking ────────────────────────────────────
        /// <summary>True when this run is the player's first attempt at this chapter.</summary>
        public bool IsTrackingFirstTry { get; set; }
        /// <summary>Set the first time the player dies during an IsTrackingFirstTry run.</summary>
        public bool DiedThisRun { get; set; }
        /// <summary>Set the first time PlayerHealthManager reports damage during a tracked run.</summary>
        public bool TookDamageThisRun { get; set; }
    }
}