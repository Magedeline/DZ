using Celeste;
using Monocle;

namespace MaggyHelper.Extensions
{
    /// <summary>
    /// Manages Kirby mode state and power abilities.
    /// Can be added to a scene as an Entity to track global Kirby mode state.
    /// </summary>
    [Tracked]
    public class KirbyMode : Entity
    {
        public enum KirbyPowerState
        {
            None,
            Fire,
            Ice,
            Spark,
            Sword,
            Cutter,
            Beam,
            Stone,
            Needle,
            Parasol,
            Wheel,
            Bomb,
            Fighter,
            Suplex,
            Ninja,
            Mirror,
            Hammer,
            Knight,
            Wing,
            UFO,
            Sleep
        }

        public KirbyPowerState CurrentPower { get; set; } = KirbyPowerState.None;
        public bool IsActive { get; set; }
        public bool IsDead { get; set; }
        public bool IsInhaling { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }

        public KirbyMode() : base()
        {
            Tag = Tags.Global | Tags.Persistent;
            Visible = false;
        }

        public void SetPowerState(KirbyPowerState powerState)
        {
            CurrentPower = powerState;
            IsActive = powerState != KirbyPowerState.None;
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MaxHealth = Math.Max(MaxHealth, 1);
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }
    }
}
