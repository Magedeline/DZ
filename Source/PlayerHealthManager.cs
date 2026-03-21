namespace MaggyHelper.Entities;

[Tracked]
public class PlayerHealthManager : Entity
{
    public static PlayerHealthManager Instance { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnDamageTaken;

    public int MaxHP { get; private set; } = 6;
    public int CurrentHP { get; private set; } = 6;
    public bool IsKirbyMode { get; private set; }
    public bool IsDead => CurrentHP <= 0;
    public bool IsLowHealth => CurrentHP > 0 && CurrentHP <= Math.Max(1, MaxHP / 3);
    public float HealthPercent => MaxHP <= 0 ? 0f : (float)CurrentHP / MaxHP;

    public PlayerHealthManager() : base(Vector2.Zero)
    {
        Tag = Tags.Persistent | Tags.TransitionUpdate;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Instance = this;
    }

    public override void Removed(Scene scene)
    {
        if (Instance == this)
            Instance = null;

        base.Removed(scene);
    }

    public static PlayerHealthManager GetOrCreate(Level level, int maxHP = 6)
    {
        if (level == null)
            return Instance;

        var manager = Instance ?? level.Tracker.GetEntity<PlayerHealthManager>();
        if (manager == null)
        {
            manager = new PlayerHealthManager();
            level.Add(manager);
        }

        manager.SetMaxHP(maxHP);
        return manager;
    }

    public void EnableKirbyMode(int maxHP = 6)
    {
        IsKirbyMode = true;
        SetMaxHP(maxHP);
    }

    public void DisableKirbyMode()
    {
        IsKirbyMode = false;
    }

    public void SetMaxHP(int maxHP)
    {
        MaxHP = Math.Max(1, maxHP);
        CurrentHP = Calc.Clamp(CurrentHP, 0, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        int next = Calc.Clamp(CurrentHP + amount, 0, MaxHP);
        if (next == CurrentHP)
            return;

        CurrentHP = next;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void FullHeal()
    {
        if (CurrentHP == MaxHP)
            return;

        CurrentHP = MaxHP;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public bool Damage(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        CurrentHP = Math.Max(0, CurrentHP - amount);
        OnDamageTaken?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        return true;
    }

    public static bool TryDamagePlayer(int damage, Vector2 source)
    {
        var manager = Instance;
        if (manager == null)
        {
            Level level = Engine.Scene as Level;
            manager = level?.Tracker?.GetEntity<PlayerHealthManager>();
        }

        if (manager == null)
            return false;

        return manager.Damage(damage);
    }
}
