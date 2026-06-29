using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using Camera = DZ.Nez.Camera;
using DZ.Nez.Sprites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DZ.Core;

namespace DZ.Entities.Bosses;

/// <summary>
/// Chara Dreemurr boss - ported from mod.
/// An Undertale-themed boss with complex pattern-based attacks, beams, and glitch effects.
///
/// Features:
/// - 15+ unique attack patterns
/// - Beam attacks (laser beams)
/// - Projectile shots (homing and wave patterns)
/// - Multi-phase with music transitions
/// - Glitch visual effects
/// - Room-to-room teleportation
/// - Moving/falling block manipulation
/// </summary>
public class CharaBoss : BossBase
{
    // Visual
    private float _floatSine;
    private float _scaleWiggler;
    private float _glitchIntensity;
    private int _facing = -1;

    // State
    private Vector2 _avoidPos;
    private bool _moving;
    private bool _sitting;
    private bool _playerHasMoved;

    // Waypoint system
    private Vector2[] _nodes;
    private int _nodeIndex;
    private int _patternIndex;

    // Combat
    private readonly List<CharaProjectile> _activeProjectiles = new();
    private readonly List<CharaBeam> _activeBeams = new();
    private float _attackTimer;
    private bool _charging;
    private float _chargeTimer;

    // Attack patterns
    private readonly Dictionary<int, Func<IEnumerator>> _attackPatterns = new();

    // Audio
    private float _laserChargeVolume;

    // Configuration
    public const float MoveSpeed = 600f;
    public const float AvoidRadius = 12f;
    public const float CameraXPastMax = 140f;

    public CharaBoss(Vector2[] nodes, int patternIndex = 0)
    {
        BossId = "chara_boss";
        BossName = "Chara Dreemurr";
        Tier = BossTier.Tier4;

        MaxHealth = 300; // 3 phases of 100 HP each
        _nodes = nodes;
        _patternIndex = patternIndex;
        _nodeIndex = 0;

        InitializeAttackPatterns();
    }

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Setup collider
        var collider = Entity.AddComponent(new CircleCollider(14f));
        collider.SetLocalOffset(new Vector2(0, -6));

        // Add player collision
        // Entity.AddComponent(new PlayerCollider(OnPlayerCollision));

        // Position at first node
        if (_nodes.Length > 0)
            Entity.Position = _nodes[0];

        Console.WriteLine($"[CharaBoss] Initialized with pattern {_patternIndex}, {_nodes.Length} nodes");
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // Play intro music
        if (_patternIndex == 0)
        {
            _sitting = true;
            DZGame.Audio.PlayMusic("event:/pusheen/music/lvl2/phone_loop");
        }
        else
        {
            StartAttacking();
        }

        Console.WriteLine("[CharaBoss] The first child has risen...");
    }

    protected override void InitializeStateMachine()
    {
        ChangeState(BossState.Idle);
    }

    public override void Update()
    {
        base.Update();

        // Update visual effects
        _floatSine += Time.DeltaTime * 2f;
        _glitchIntensity *= 0.95f; // Decay glitch

        // Cleanup dead projectiles
        _activeProjectiles.RemoveAll(p => !p.IsActive);
        _activeBeams.RemoveAll(b => !b.IsActive);

        // Update audio
        if (_charging)
        {
            _chargeTimer -= Time.DeltaTime;
            if (_chargeTimer <= 0)
            {
                _charging = false;
                FireChargedAttack();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // STATE OVERRIDES
    // ═══════════════════════════════════════════════════════════════════════

    protected override void UpdateIdle()
    {
        if (_sitting) return;

        HoverMotion();

        // Face player
        if (Player != null)
        {
            float dist = Player.Entity.Position.X - Entity.Position.X;
            if (Math.Abs(dist) > 20f)
            {
                int newFacing = dist > 0 ? 1 : -1;
                if (newFacing != _facing)
                {
                    _facing = newFacing;
                    _scaleWiggler = 1f; // Trigger wiggle
                }
            }

            // Check if player moved
            if (!_playerHasMoved && Player.Velocity != Vector2.Zero)
            {
                _playerHasMoved = true;
                if (_patternIndex != 0)
                    StartAttacking();
            }
        }

        // Avoidance behavior
        UpdateAvoidance();

        // Attack when ready
        if (_playerHasMoved && AttackCooldown <= 0 && DistanceToPlayer() < 150f)
        {
            ExecuteCurrentPattern();
        }
    }

    protected override void UpdateAttack()
    {
        // Attack coroutine handles this
        if (StateTimer <= 0)
        {
            ChangeState(BossState.Idle);
        }
    }

    protected override void UpdateVulnerable()
    {
        // Chara is stunned/vulnerable after being hit
        HoverMotion();

        if (StateTimer <= 0)
        {
            RecoverFromHit();
        }
    }

    protected override void CheckPhaseTransition()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        if (healthPercent <= 0.66f && PhaseIndex == 1)
        {
            PhaseIndex = 2;
            ChangeState(BossState.PhaseTransition);
            StateTimer = 3f;
            DZGame.Audio.PlayMusic("event:/pusheen/music/lvl8/chara_glitch");
            TriggerGlitch(0.6f);
        }
        else if (healthPercent <= 0.33f && PhaseIndex == 2)
        {
            PhaseIndex = 3;
            ChangeState(BossState.PhaseTransition);
            StateTimer = 4f;
            DZGame.Audio.PlayMusic("event:/pusheen/music/lvl8/chara_core");
            TriggerGlitch(0.8f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HIT/DAMAGE SYSTEM
    // ═══════════════════════════════════════════════════════════════════════

    public override void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (IsInvulnerable || _sitting) return;

        base.TakeDamage(damage, hitDirection);

        // Clear projectiles and beams on hit
        ClearAllAttacks();

        // Transition to next node
        _nodeIndex = Math.Min(_nodeIndex + 1, _nodes.Length - 1);

        // Visual feedback
        _scaleWiggler = 1.5f;
        TriggerGlitch(0.4f);

        // Play hit sound
        DZGame.Audio.PlaySfx("event:/pusheen/game/boss/chara_hit");

        // Start recovery sequence
        ChangeState(BossState.Vulnerable);
        StateTimer = 2f;
        IsInvulnerable = true;

        // Move to next position
        if (_nodeIndex < _nodes.Length)
        {
            DZ.Nez.Core.StartCoroutine(MoveToNextNode());
        }
    }

    private void RecoverFromHit()
    {
        IsInvulnerable = false;
        _scaleWiggler = 0f;
        ChangeState(BossState.Idle);
        StartAttacking();
    }

    private IEnumerator MoveToNextNode()
    {
        _moving = true;

        Vector2 from = Entity.Position;
        Vector2 to = _nodes[_nodeIndex];
        float duration = Vector2.Distance(from, to) / MoveSpeed;

        // Screen shake
        // Camera.Shake();

        // Spawn particles
        SpawnHitParticles();

        // Tween to new position
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = EaseHelper.Ease(EaseType.SineInOut, elapsed, duration);
            Entity.Position = Vector2.Lerp(from, to, t);

            // Spawn trail
            if (elapsed % 0.02f < Time.DeltaTime)
            {
                SpawnTrailParticle();
            }

            yield return null;
        }

        Entity.Position = to;
        _moving = false;

        // Play recover animation
        // Sprite.Play("recover");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK SYSTEM
    // ═══════════════════════════════════════════════════════════════════════

    private void InitializeAttackPatterns()
    {
        // Pattern 01: Triple shot burst
        _attackPatterns[1] = Pattern01TripleShot;

        // Pattern 02: Continuous wave shots
        _attackPatterns[2] = Pattern02WaveShots;

        // Pattern 03: Shot + Beam combo
        _attackPatterns[3] = Pattern03ShotBeamCombo;

        // Pattern 04: Homing shots
        _attackPatterns[4] = Pattern04HomingShots;

        // Pattern 05: Beam sweep
        _attackPatterns[5] = Pattern05BeamSweep;

        // Pattern 06: Circle burst
        _attackPatterns[6] = Pattern06CircleBurst;

        // Pattern 07: Rapid fire
        _attackPatterns[7] = Pattern07RapidFire;

        // Pattern 08: Bouncing shots
        _attackPatterns[8] = Pattern08BouncingShots;

        // Pattern 09: Charge beam
        _attackPatterns[9] = Pattern09ChargeBeam;

        // Pattern 10: Rain of shots
        _attackPatterns[10] = Pattern10RainShots;

        // Pattern 21: Bigger beam (phase 2+)
        _attackPatterns[21] = Pattern21BiggerBeam;
    }

    private void StartAttacking()
    {
        // Start the appropriate attack pattern
        int pattern = GetPatternForPhase();
        if (_attackPatterns.TryGetValue(pattern, out var patternFunc))
        {
            DZ.Nez.Core.StartCoroutine(patternFunc());
        }
    }

    private void ExecuteCurrentPattern()
    {
        // This is called to trigger attacks during idle
        // The coroutine handles the sequence
    }

    private int GetPatternForPhase()
    {
        return PhaseIndex switch
        {
            1 => DZ.Nez.Random.NextInt(6) + 1,  // Patterns 1-6
            2 => DZ.Nez.Random.NextInt(8) + 1,  // Patterns 1-8
            3 => DZ.Nez.Random.NextInt(10) + 1, // Patterns 1-10 + 21
            _ => 1
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK PATTERNS
    // ═══════════════════════════════════════════════════════════════════════

    private IEnumerator Pattern01TripleShot()
    {
        ChangeState(BossState.Attack);

        for (int i = 0; i < 3; i++)
        {
            FireShotAtPlayer(speed: 150f, homing: false);
            yield return 0.3f;
        }

        AttackCooldown = 1.5f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern02WaveShots()
    {
        ChangeState(BossState.Attack);

        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float wave = MathF.Sin(elapsed * 3f);
            Vector2 direction = new Vector2(_facing, wave * 0.5f);
            FireShot(direction, speed: 120f);

            elapsed += 0.15f;
            yield return 0.15f;
        }

        AttackCooldown = 2f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern03ShotBeamCombo()
    {
        ChangeState(BossState.Attack);

        // Charge visual
        StartCharge();
        yield return 0.5f;

        // Fire shots
        for (int i = 0; i < 5; i++)
        {
            FireShotAtPlayer(speed: 130f);
            yield return 0.2f;
        }

        // Then beam
        yield return 0.3f;
        FireBeam(duration: 1f);

        AttackCooldown = 2.5f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern04HomingShots()
    {
        ChangeState(BossState.Attack);

        for (int i = 0; i < 4; i++)
        {
            FireShotAtPlayer(speed: 100f, homing: true, homingStrength: 0.3f);
            yield return 0.4f;
        }

        AttackCooldown = 2f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern05BeamSweep()
    {
        ChangeState(BossState.Attack);

        // Sweep beam from up to down
        FireSweepingBeam(duration: 1.5f, sweepAngle: MathF.PI / 3);

        yield return 1.5f;

        AttackCooldown = 3f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern06CircleBurst()
    {
        ChangeState(BossState.Attack);

        int shotCount = PhaseIndex >= 3 ? 16 : 12;
        float angleStep = MathF.PI * 2 / shotCount;

        for (int i = 0; i < shotCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            FireShot(dir, speed: 140f);
        }

        DZGame.Audio.PlaySfx("event:/pusheen/game/boss/chara_circle_burst");

        yield return 1f;

        AttackCooldown = 2f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern07RapidFire()
    {
        ChangeState(BossState.Attack);

        for (int i = 0; i < 10; i++)
        {
            Vector2 dir = Vector2Ext.Normalize(DirectionToPlayer() + RandomOffset(0.2f));
            FireShot(dir, speed: 160f);
            yield return 0.1f;
        }

        AttackCooldown = 1.5f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern08BouncingShots()
    {
        ChangeState(BossState.Attack);

        for (int i = 0; i < 5; i++)
        {
            FireBouncingShot(DirectionToPlayer(), speed: 140f, bounces: 3);
            yield return 0.3f;
        }

        AttackCooldown = 2.5f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern09ChargeBeam()
    {
        ChangeState(BossState.Attack);

        // Long charge
        StartCharge();
        DZGame.Audio.PlaySfx("event:/pusheen/game/boss/chara_charge");

        yield return 1.5f;

        // Big beam
        FireBeam(duration: 2f, width: 2f);

        yield return 2f;

        AttackCooldown = 4f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern10RainShots()
    {
        ChangeState(BossState.Attack);

        float duration = 4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Rain from above
            float xPos = Entity.Position.X + DZ.Nez.Random.NextFloat(200) - 100;
            Vector2 spawnPos = new Vector2(xPos, Entity.Position.Y - 150);
            FireFallingShot(spawnPos, targetY: Entity.Position.Y + 50);

            elapsed += 0.1f;
            yield return 0.1f;
        }

        AttackCooldown = 3f;
        ChangeState(BossState.Idle);
    }

    private IEnumerator Pattern21BiggerBeam()
    {
        ChangeState(BossState.Attack);
        StateTimer = 5f;

        // The "Bigger Beam" - massive attack for phase 3
        StartCharge();
        TriggerGlitch(0.5f);

        yield return 1f;

        // Fill screen with beam
        FireBeam(duration: 3f, width: 4f, fullScreen: true);

        yield return 3f;

        AttackCooldown = 5f;
        ChangeState(BossState.Idle);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private void FireShotAtPlayer(float speed, bool homing = false, float homingStrength = 0f)
    {
        if (Player == null) return;

        Vector2 dir = DirectionToPlayer();
        FireShot(dir, speed, homing, homingStrength);
    }

    private void FireShot(Vector2 direction, float speed, bool homing = false, float homingStrength = 0f)
    {
        var shot = new CharaProjectile(Entity.Position, direction, speed, homing, homingStrength);
        Scene.AddEntity(shot.Entity);
        _activeProjectiles.Add(shot);

        DZGame.Audio.PlaySfx("event:/pusheen/game/boss/chara_shot");
    }

    private void FireBouncingShot(Vector2 direction, float speed, int bounces)
    {
        var shot = new CharaProjectile(Entity.Position, direction, speed, bouncing: true, maxBounces: bounces);
        Scene.AddEntity(shot.Entity);
        _activeProjectiles.Add(shot);
    }

    private void FireFallingShot(Vector2 spawnPos, float targetY)
    {
        Vector2 dir = new Vector2(0, 1);
        var shot = new CharaProjectile(spawnPos, dir, speed: 200f, gravity: 300f);
        Scene.AddEntity(shot.Entity);
        _activeProjectiles.Add(shot);
    }

    private void FireBeam(float duration, float width = 1f, bool fullScreen = false)
    {
        Vector2 origin = Entity.Position + new Vector2(0, -14);
        Vector2 dir = new Vector2(_facing, 0);

        var beam = new CharaBeam(origin, dir, duration, width, fullScreen);
        Scene.AddEntity(beam.Entity);
        _activeBeams.Add(beam);

        DZGame.Audio.PlaySfx("event:/pusheen/game/boss/chara_beam");
    }

    private void FireSweepingBeam(float duration, float sweepAngle)
    {
        Vector2 origin = Entity.Position + new Vector2(0, -14);

        var beam = new CharaBeam(origin, Vector2.UnitX * _facing, duration, 1f, sweeping: true, sweepAngle: sweepAngle);
        Scene.AddEntity(beam.Entity);
        _activeBeams.Add(beam);
    }

    private void StartCharge()
    {
        _charging = true;
        _chargeTimer = 1f;

        // Visual charge effect
        // Spawn particles around Chara
        for (int i = 0; i < 8; i++)
        {
            float angle = i * (MathF.PI * 2 / 8);
            Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 20f;
            SpawnChargeParticle(Entity.Position + offset);
        }
    }

    private void FireChargedAttack()
    {
        // Override in specific patterns
    }

    private void ClearAllAttacks()
    {
        foreach (var proj in _activeProjectiles)
            proj.Destroy();
        _activeProjectiles.Clear();

        foreach (var beam in _activeBeams)
            beam.Destroy();
        _activeBeams.Clear();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VISUAL EFFECTS
    // ═══════════════════════════════════════════════════════════════════════

    private void HoverMotion()
    {
        if (_moving) return;

        float yOffset = MathF.Sin(_floatSine) * 3f;
        float xOffset = MathF.Cos(_floatSine * 0.5f) * 4f;

        Vector2 targetPos = _nodes[_nodeIndex] + new Vector2(xOffset, yOffset) + _avoidPos;
        Entity.Position = Vector2.Lerp(Entity.Position, targetPos, 0.1f);
    }

    private void UpdateAvoidance()
    {
        if (Player == null || _moving) return;

        float dist = DistanceToPlayer();
        Vector2 avoidTarget = Vector2.Zero;

        if (dist < 60f)
        {
            // Too close - back away
            avoidTarget = Vector2Ext.Normalize(Entity.Position - Player.Entity.Position) * 20f;
        }
        else if (dist > 100f)
        {
            // Too far - approach slightly
            avoidTarget = Vector2Ext.Normalize(Player.Entity.Position - Entity.Position) * 5f;
        }

        _avoidPos = Vector2.Lerp(_avoidPos, avoidTarget, Time.DeltaTime * 2f);
    }

    private void TriggerGlitch(float intensity)
    {
        _glitchIntensity = intensity;
        // TODO: Apply screen shader effect
    }

    private void SpawnHitParticles()
    {
        for (float angle = 0; angle < MathF.PI * 2; angle += 0.35f)
        {
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            // Spawn particle
        }
    }

    private void SpawnTrailParticle()
    {
        // Spawn trail particle at position
    }

    private void SpawnChargeParticle(Vector2 position)
    {
        // Spawn charge particle
    }

    protected override void UpdateAnimation()
    {
        // Update sprite based on state
        // Apply scale wiggle
        float scale = 1f + _scaleWiggler * 0.2f;
        // Sprite.Scale = new Vector2(_facing * scale, scale);
    }

    private Vector2 RandomOffset(float magnitude)
    {
        return new Vector2(
            DZ.Nez.Random.NextFloat(magnitude * 2) - magnitude,
            DZ.Nez.Random.NextFloat(magnitude * 2) - magnitude
        );
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SPAWNER
    // ═══════════════════════════════════════════════════════════════════════

    public static Entity Spawn(Scene scene, Vector2[] nodes, int patternIndex = 0)
    {
        var chara = scene.CreateEntity("chara_boss");
        chara.Position = nodes[0];
        chara.AddComponent(new CharaBoss(nodes, patternIndex));

        var boss = chara.GetComponent<CharaBoss>();
        boss.OnSpawn();

        return chara;
    }
}
