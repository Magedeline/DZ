using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using DZ.Nez.Sprites;
using System;
using System.Collections;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Player;

namespace DZ.Entities.Bosses;

/// <summary>
/// Base class for all boss entities.
/// Replaces: Celeste.Mod.DZ.Boss + BaseBoss from your mod
///
/// This provides the foundation for porting your 40+ bosses to standalone.
/// </summary>
public abstract class BossBase : Component, IUpdatable
{
    // Boss identity
    public string BossId { get; protected set; } = "unknown";
    public string BossName { get; protected set; } = "Unknown Boss";
    public BossTier Tier { get; protected set; } = BossTier.Tier1;

    // Health
    public int MaxHealth { get; protected set; } = 100;
    public int CurrentHealth { get; protected set; }
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable { get; protected set; }

    // State machine
    public BossState CurrentState { get; protected set; } = BossState.Idle;
    protected float StateTimer { get; set; }
    protected int PhaseIndex { get; set; } = 0;

    // Positioning
    public Vector2 ArenaCenter { get; set; }
    public float ArenaWidth { get; set; } = 320f;
    public float ArenaHeight { get; set; } = 180f;

    // Combat
    protected List<AttackPattern> AttackPatterns { get; } = new();
    protected int CurrentAttackIndex { get; set; }
    protected float AttackCooldown { get; set; }

    // References
    protected PlayerController Player { get; private set; }
    protected new Scene Scene => Entity.Scene;

    // Events
    public event Action OnBossSpawned;
    public event Action<int> OnHealthChanged; // New health value
    public event Action OnPhaseChanged;
    public event Action OnBossDefeated;

    // Visual
    protected SpriteRenderer SpriteRenderer { get; private set; }
    protected SpriteAnimator Animator { get; private set; }

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Setup components
        SpriteRenderer = Entity.AddComponent(new SpriteRenderer());
        // Animator is a SpriteAnimator - add separately when sprites are available
        // Animator = Entity.AddComponent(new SpriteAnimator());

        // Setup collider (boss hitbox)
        var collider = Entity.AddComponent(new BoxCollider(16, 24));
        collider.PhysicsLayer = 2; // Boss layer

        // Initialize health
        CurrentHealth = MaxHealth;

        // Setup state machine
        InitializeStateMachine();

        Console.WriteLine($"[Boss:{BossId}] Initialized with {MaxHealth} HP");
    }

    public override void Update()
    {
        if (IsDead) return;

        // Update timers
        StateTimer -= Time.DeltaTime;
        AttackCooldown -= Time.DeltaTime;

        // State machine update
        UpdateStateMachine();

        // Update phase based on health thresholds
        UpdatePhase();

        // Face player
        FacePlayer();

        // Update animation
        UpdateAnimation();
    }

    /// <summary>
    /// Called when boss is spawned into the scene
    /// </summary>
    public virtual void OnSpawn()
    {
        // Find player reference
        Player = Scene.FindEntity("player")?.GetComponent<PlayerController>();

        // Play spawn animation/sound
        PlaySpawnEffect();

        OnBossSpawned?.Invoke();

        Console.WriteLine($"[Boss:{BossId}] Spawned in arena");
    }

    /// <summary>
    /// Take damage from player
    /// </summary>
    public virtual void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (IsDead || IsInvulnerable) return;

        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth);

        // Hit effect
        PlayHitEffect();

        // Check for phase transition
        CheckPhaseTransition();

        // Check for death
        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Brief invulnerability
            StartInvulnerability(0.5f);
        }
    }

    /// <summary>
    /// Heal the boss (for certain mechanics)
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    /// <summary>
    /// Boss death sequence
    /// </summary>
    protected virtual void Die()
    {
        CurrentState = BossState.Dead;

        // Play death animation
        PlayDeathEffect();

        // Play defeat music/SFX
        DZGame.Audio.PlayMusic("event:/DZ/music/boss_defeat");

        // Cleanup after delay
        DZ.Nez.Core.Schedule(3f, _ =>
        {
            // Spawn rewards, open exit, etc.
            OnBossDefeated?.Invoke();

            // Remove boss entity
            Entity.Destroy();
        });

        Console.WriteLine($"[Boss:{BossId}] Defeated!");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // STATE MACHINE
    // ═══════════════════════════════════════════════════════════════════════

    protected abstract void InitializeStateMachine();

    protected virtual void UpdateStateMachine()
    {
        switch (CurrentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;
            case BossState.Chase:
                UpdateChase();
                break;
            case BossState.Attack:
                UpdateAttack();
                break;
            case BossState.Vulnerable:
                UpdateVulnerable();
                break;
            case BossState.Stunned:
                UpdateStunned();
                break;
            case BossState.PhaseTransition:
                UpdatePhaseTransition();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
        // Default: transition to chase if player is nearby
        if (Player != null && Vector2.Distance(Entity.Position, Player.Entity.Position) < 200f)
        {
            ChangeState(BossState.Chase);
        }
    }

    protected virtual void UpdateChase()
    {
        // Move towards player
        if (Player == null) return;

        Vector2 direction = Player.Entity.Position - Entity.Position;
        direction.Normalize();

        float speed = GetChaseSpeed();
        Entity.Position += direction * speed * Time.DeltaTime;

        // Attack if in range and cooldown ready
        if (AttackCooldown <= 0 && IsInAttackRange())
        {
            ChangeState(BossState.Attack);
        }
    }

    protected virtual void UpdateAttack()
    {
        // Attack logic implemented by subclasses
        // Return to idle/chase when done
        if (StateTimer <= 0)
        {
            ChangeState(BossState.Idle);
        }
    }

    protected virtual void UpdateVulnerable()
    {
        // Boss is vulnerable to damage (special states)
        if (StateTimer <= 0)
        {
            ChangeState(BossState.Idle);
        }
    }

    protected virtual void UpdateStunned()
    {
        // Boss can't act
        if (StateTimer <= 0)
        {
            ChangeState(BossState.Idle);
        }
    }

    protected virtual void UpdatePhaseTransition()
    {
        // Phase change animation/effects
        if (StateTimer <= 0)
        {
            EnterNewPhase();
            ChangeState(BossState.Idle);
        }
    }

    protected void ChangeState(BossState newState)
    {
        if (CurrentState == newState) return;

        OnExitState(CurrentState);
        CurrentState = newState;
        StateTimer = 0;
        OnEnterState(newState);

        Console.WriteLine($"[Boss:{BossId}] State: {CurrentState}");
    }

    protected virtual void OnEnterState(BossState state)
    {
        // Override in subclasses
    }

    protected virtual void OnExitState(BossState state)
    {
        // Override in subclasses
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PHASE SYSTEM
    // ═══════════════════════════════════════════════════════════════════════

    protected virtual void UpdatePhase()
    {
        // Override to change behavior based on health percentage
        float healthPercent = (float)CurrentHealth / MaxHealth;

        // Example: Phase transitions at 75%, 50%, 25%
        // This is where you'd implement phase logic
    }

    protected virtual void CheckPhaseTransition()
    {
        // Override to trigger phase changes at specific health thresholds
    }

    protected virtual void EnterNewPhase()
    {
        PhaseIndex++;
        OnPhaseChanged?.Invoke();

        // Play phase transition effect
        PlayPhaseEffect();

        Console.WriteLine($"[Boss:{BossId}] Entered Phase {PhaseIndex}");
    }

    protected void RaisePhaseChanged() => OnPhaseChanged?.Invoke();

    // ═══════════════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════════════

    protected void FacePlayer()
    {
        if (Player == null || SpriteRenderer == null) return;

        float facing = Player.Entity.Position.X - Entity.Position.X;
        SpriteRenderer.FlipX = facing < 0;
    }

    protected float DistanceToPlayer()
    {
        if (Player == null) return float.MaxValue;
        return Vector2.Distance(Entity.Position, Player.Entity.Position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (Player == null) return Vector2.Zero;
        Vector2 dir = Player.Entity.Position - Entity.Position;
        dir.Normalize();
        return dir;
    }

    protected virtual float GetChaseSpeed() => 50f;

    protected virtual bool IsInAttackRange() => DistanceToPlayer() < 40f;

    protected void StartInvulnerability(float duration)
    {
        IsInvulnerable = true;
        DZ.Nez.Core.Schedule(duration, _ => IsInvulnerable = false);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VIRTUAL EFFECT METHODS (Override in subclasses)
    // ═══════════════════════════════════════════════════════════════════════

    protected virtual void PlaySpawnEffect()
    {
        // Play spawn sound
        DZGame.Audio.PlaySfx("event:/DZ/game/boss/spawn");
    }

    protected virtual void PlayHitEffect()
    {
        // Flash white, play hit sound
        DZGame.Audio.PlaySfx("event:/DZ/game/boss/hit");
    }

    protected virtual void PlayDeathEffect()
    {
        // Explosion particles, screen shake
        DZGame.Audio.PlaySfx("event:/DZ/game/boss/death");
    }

    protected virtual void PlayPhaseEffect()
    {
        // Phase transition visual/sound
        DZGame.Audio.PlaySfx("event:/DZ/game/boss/phaseDZ_CHange");
    }

    protected virtual void UpdateAnimation()
    {
        // Update Animator based on state
        // Animator.Play(CurrentState.ToString().ToLower());
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK SYSTEM
    // ═══════════════════════════════════════════════════════════════════════

    protected void RegisterAttack(AttackPattern attack)
    {
        AttackPatterns.Add(attack);
    }

    protected void ExecuteAttack(int index)
    {
        if (index < 0 || index >= AttackPatterns.Count) return;

        var attack = AttackPatterns[index];
        attack.Execute(this);
        AttackCooldown = attack.Cooldown;
    }

    protected void ExecuteRandomAttack()
    {
        if (AttackPatterns.Count == 0) return;

        int randomIndex = DZ.Nez.Random.NextInt(AttackPatterns.Count);
        ExecuteAttack(randomIndex);
    }
}

/// <summary>
/// Boss difficulty tier
/// </summary>
public enum BossTier
{
    Tier1 = 1,   // Tutorial bosses
    Tier2 = 2,   // Chapter mid-bosses
    Tier3 = 3,   // Chapter end bosses
    Tier4 = 4,   // Super bosses
    Tier5 = 5,   // Final/Secret bosses
    Apex = 6     // True final bosses
}

/// <summary>
/// Boss state machine states
/// </summary>
public enum BossState
{
    Idle,
    Chase,
    Attack,
    Vulnerable,
    Stunned,
    PhaseTransition,
    Dead
}

/// <summary>
/// Defines a boss attack pattern
/// </summary>
public abstract class AttackPattern
{
    public string Name { get; set; } = "Unnamed";
    public float Cooldown { get; set; } = 2f;
    public float Damage { get; set; } = 10f;

    public abstract void Execute(BossBase boss);
}
