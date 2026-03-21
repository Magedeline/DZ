
namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSession : EverestModuleSession
    {
        public bool BossFightActive { get; set; }
        public string CurrentBossName { get; set; }
        public int BossesDefeated { get; set; }
        public bool IsKirbyModeActive { get; set; }
        public global::MaggyHelper.Entities.Bosses.CopyAbilityType CurrentCopyAbility { get; set; }
        public string CurrentKirbyPower
        {
            get => CurrentCopyAbility.ToString();
            set => CurrentCopyAbility = Enum.TryParse(value, true, out global::MaggyHelper.Entities.Bosses.CopyAbilityType ability)
                ? ability
                : global::MaggyHelper.Entities.Bosses.CopyAbilityType.None;
        }
        public int EnemiesDefeated { get; set; }
    }
}