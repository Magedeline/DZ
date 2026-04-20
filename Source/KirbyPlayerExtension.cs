using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Entities;

public abstract class KirbyPlayerExtension : Actor
{
    protected KirbyPlayerExtension(Vector2 position)
        : base(position)
    {
        AbilityManager = new KirbyAbilityManager(this);
    }

    public KirbyAbilityManager AbilityManager { get; }

    public abstract float CurrentStamina { get; set; }
    public abstract float MaxStamina { get; }
    public abstract bool IsDead { get; }
    public abstract int CurrentHealth { get; }
    public abstract int MaxHealth { get; }
    public abstract bool IsHovering { get; }

    public abstract void Heal(int amount);
    public abstract void SetPowerState(KirbyMode.KirbyPowerState powerState);
}

public sealed class KirbyAbilityManager
{
    private readonly KirbyPlayerExtension player;

    public KirbyAbilityManager(KirbyPlayerExtension player)
    {
        this.player = player;
    }

    public T Get<T>() where T : class
    {
        if (typeof(T) == typeof(KirbyHoverAbility))
        {
            return new KirbyHoverAbility(player) as T;
        }

        return null;
    }
}

public sealed class KirbyHoverAbility
{
    private readonly KirbyPlayerExtension player;

    public KirbyHoverAbility(KirbyPlayerExtension player)
    {
        this.player = player;
    }

    public bool IsHovering => player.IsHovering;
}
