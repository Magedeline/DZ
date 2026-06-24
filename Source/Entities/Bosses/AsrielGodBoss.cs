using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using static Nez.Time;
using System;
using System.Collections;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Bosses;

/// <summary>
/// Asriel God of Hyperdeath boss - ported from your mod!
///
/// This demonstrates how to convert a Celeste mod boss to standalone.
/// Shows the key differences between Everest Entity and Nez Entity system.
///
/// Original: Celeste.Mod.MaggyHelper.AsrielGodBoss (Celeste Entity)
/// New: KirbyCelesteStandalone.Entities.Bosses.AsrielGodBoss (Nez Component)
/// </summary>
public class AsrielGodBoss : BossBase
{
    // Asriel-specific configuration
    private const int PHASE1_HEALTH = 100;
    private const int PHASE2_HEALTH = 100;
    private const int PHASE3_HEALTH = 100;

    // Attack cooldowns
    private const float STAR_SHOWER_COOLDOWN = 3f;
    private const float CHAOS_SABER_COOLDOWN = 4f;
    private const float HYPER_GONER_COOLDOWN = 8f;

    // Attack state
    private int _consecutiveAttacks;
    private bool _isChaosSabersActive;
    private List<Entity> _activeProjectiles = new();

    // Visual
    private float _wingAngle;
    private float _glowIntensity;

    // Phase 2 transformation
    private bool _hasTransformedToPhase2;
    private bool _hasTransformedToPhase3;

    public AsrielGodBoss()
    {
        BossId = "asriel_god";
        BossName = "Asriel Dreemurr";
        Tier = BossTier.Apex; // Final boss tier
        MaxHealth = PHASE1_HEALTH + PHASE2_HEALTH + PHASE3_HEALTH;
    }

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Register Asriel's unique attacks
        RegisterAttack(new StarShowerAttack());
        RegisterAttack(new ChaosSaberAttack());
        RegisterAttack(new HyperGonerAttack());
        RegisterAttack(new FireMagicAttack());

        Console.WriteLine("[AsrielGodBoss] God of Hyperdeath rises!");
    }

    protected override void InitializeStateMachine()
    {
        // Asriel starts with dramatic idle
        ChangeState(BossState.Idle);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // Play dramatic music
        KirbyGame.Audio.PlayMusic("event:/pusheen/music/boss/asriel_phase1", fadeIn: true);

        // Create wing effect
        CreateWingEntity();

        // Dramatic entrance
        PlaySpawnAnimation();
    }

    public override void Update()
    {
        base.Update();

        // Update visual effects
        UpdateWings();
        UpdateGlow();

        // Cleanup dead projectiles
        _activeProjectiles.RemoveAll(e => !e.Enabled);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // STATE OVERRIDES (Asriel-specific behavior)
    // ═══════════════════════════════════════════════════════════════════════

    protected override void UpdateIdle()
    {
        // Asriel hovers in place, occasionally moving
        HoverMotion();

        // Decide next action
        if (StateTimer <= 0)
        {
            // Choose between chasing and attacking
            if (DistanceToPlayer() < 150f && AttackCooldown <= 0)
            {
                // Choose attack based on phase
                ChooseAttack();
            }
            else
            {
                ChangeState(BossState.Chase);
            }
        }
    }

    protected override void UpdateAttack()
    {
        // During attack, Asriel maintains position
        // The attack pattern handles the movement/projectiles

        if (StateTimer <= 0)
        {
            _consecutiveAttacks++;

            // After 2-3 attacks, become vulnerable briefly
            if (_consecutiveAttacks >= 2)
            {
                _consecutiveAttacks = 0;
                ChangeState(BossState.Vulnerable);
                StateTimer = 2f; // Vulnerable window
            }
            else
            {
                ChangeState(BossState.Idle);
                StateTimer = 1f; // Brief pause between attacks
            }
        }
    }

    protected override void UpdateVulnerable()
    {
        // Asriel is tired, takes more damage
        IsInvulnerable = false;

        // Slow drift downward
        Entity.Position += new Vector2(0, 10f * Time.DeltaTime);

        if (StateTimer <= 0)
        {
            // Recover and resume
            ChangeState(BossState.Idle);
        }
    }

    protected override void UpdatePhaseTransition()
    {
        // Stop all projectiles during phase change
        ClearProjectiles();

        // Asriel transforms - invulnerable during this
        IsInvulnerable = true;

        // Play transformation animation
        if (StateTimer > 2f) // First half - power up
        {
            _glowIntensity += Time.DeltaTime;
            PlayChargeEffect();
        }
        else if (StateTimer <= 0) // Transition complete
        {
            CompletePhaseTransformation();
            ChangeState(BossState.Idle);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PHASE SYSTEM (Asriel has 3 phases)
    // ═══════════════════════════════════════════════════════════════════════

    protected override void CheckPhaseTransition()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        // Phase 2 at 66% health
        if (healthPercent <= 0.66f && !_hasTransformedToPhase2)
        {
            _hasTransformedToPhase2 = true;
            ChangeState(BossState.PhaseTransition);
            StateTimer = 4f;

            // Change music
            KirbyGame.Audio.PlayMusic("event:/pusheen/music/boss/asriel_phase2");
        }
        // Phase 3 at 33% health
        else if (healthPercent <= 0.33f && !_hasTransformedToPhase3)
        {
            _hasTransformedToPhase3 = true;
            ChangeState(BossState.PhaseTransition);
            StateTimer = 5f;

            // Change to final music
            KirbyGame.Audio.PlayMusic("event:/pusheen/music/boss/asriel_phase3_final");
        }
    }

    private void CompletePhaseTransformation()
    {
        // Update visual
        if (PhaseIndex == 2)
        {
            // Phase 2: Wings grow larger, new color
            UpdateWingVisuals(wingScale: 1.5f, color: Color.Gold);
            Console.WriteLine("[AsrielGodBoss] Phase 2: The real fight begins!");
        }
        else if (PhaseIndex == 3)
        {
            // Phase 3: Final form - rainbow wings
            UpdateWingVisuals(wingScale: 2f, color: Color.Magenta);
            Console.WriteLine("[AsrielGodBoss] Phase 3: God of Hyperdeath!");
        }

        IsInvulnerable = false;
        RaisePhaseChanged();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK SELECTION
    // ═══════════════════════════════════════════════════════════════════════

    private void ChooseAttack()
    {
        // Weighted random based on phase and distance
        List<int> possibleAttacks = new();

        // Always available
        possibleAttacks.Add(0); // Star Shower
        possibleAttacks.Add(3); // Fire Magic

        // Phase 2+ adds Chaos Sabers
        if (PhaseIndex >= 2)
        {
            possibleAttacks.Add(1); // Chaos Sabers
        }

        // Phase 3 adds Hyper Goner (ultimate)
        if (PhaseIndex >= 3 && _consecutiveAttacks == 0)
        {
            possibleAttacks.Add(2); // Hyper Goner
        }

        // Pick one
        int chosen = possibleAttacks[Nez.Random.NextInt(possibleAttacks.Count)];
        ExecuteAttack(chosen);

        // Set attack duration
        StateTimer = GetAttackDuration(chosen);
        ChangeState(BossState.Attack);
    }

    private float GetAttackDuration(int attackIndex) => attackIndex switch
    {
        0 => 2.5f,  // Star Shower
        1 => 3f,    // Chaos Sabers
        2 => 6f,    // Hyper Goner (long)
        3 => 1.5f,  // Fire Magic (quick)
        _ => 2f
    };

    // ═══════════════════════════════════════════════════════════════════════
    // VISUAL EFFECTS
    // ═══════════════════════════════════════════════════════════════════════

    private void CreateWingEntity()
    {
        // Create separate entity for wings so they can animate independently
        var wingEntity = Scene.CreateEntity("asriel_wings");
        wingEntity.Position = Entity.Position;

        // Wing renderer component
        // wingEntity.AddComponent(new AsrielWingRenderer(this));
    }

    private void UpdateWings()
    {
        // Animate wings flapping
        _wingAngle += Time.DeltaTime * 3f;
    }

    private void UpdateWingVisuals(float wingScale, Color color)
    {
        // Update wing entity visuals
        // This would communicate with the wing renderer
    }

    private void UpdateGlow()
    {
        // Pulsing glow effect around Asriel
        _glowIntensity = 0.5f + MathF.Sin(Time.TotalTime * 2f) * 0.3f;
    }

    private void HoverMotion()
    {
        // Gentle floating
        float yOffset = MathF.Sin(Time.TotalTime * 2f) * 5f;
        Entity.Position = new Vector2(Entity.Position.X, ArenaCenter.Y + yOffset);
    }

    private void PlaySpawnAnimation()
    {
        // Dramatic rise from below
        Entity.Position = new Vector2(ArenaCenter.X, ArenaCenter.Y + 100);

        // Tween upward
        // Entity.TweenPositionTo(ArenaCenter, 2f)
        //     .SetEaseType(Nez.EaseType.ExpoOut)
    }

    private void PlayChargeEffect()
    {
        // Spawn energy particles
        for (int i = 0; i < 5; i++)
        {
            // Create charge particle
            var particle = Scene.CreateEntity($"charge_particle_{i}");
            particle.Position = Entity.Position;
            // particle.AddComponent(new ChargeParticle());
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UTILITY
    // ═══════════════════════════════════════════════════════════════════════

    private void ClearProjectiles()
    {
        foreach (var projectile in _activeProjectiles)
        {
            projectile.Destroy();
        }
        _activeProjectiles.Clear();
    }

    public void TrackProjectile(Entity projectile)
    {
        _activeProjectiles.Add(projectile);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ATTACK PATTERNS (Inner classes)
    // ═══════════════════════════════════════════════════════════════════════

    private class StarShowerAttack : AttackPattern
    {
        public StarShowerAttack()
        {
            Name = "Star Shower";
            Cooldown = STAR_SHOWER_COOLDOWN;
            Damage = 5;
        }

        public override void Execute(BossBase boss)
        {
            var asriel = (AsrielGodBoss)boss;

            // Play attack sound
            KirbyGame.Audio.PlaySfx("event:/pusheen/game/boss/asriel/star_shower");

            // Spawn falling stars
            int starCount = asriel.PhaseIndex >= 3 ? 8 : 5;

            for (int i = 0; i < starCount; i++)
            {
                float xOffset = Nez.Random.NextFloat(200) - 100;
                var star = boss.Entity.Scene.CreateEntity($"star_{i}");
                star.Position = new Vector2(
                    boss.Entity.Position.X + xOffset,
                    boss.Entity.Position.Y - 200
                );

                // star.AddComponent(new FallingStarProjectile(damage: Damage));
                asriel.TrackProjectile(star);
            }

            Console.WriteLine("[Asriel] ★ Star Shower! ★");
        }
    }

    private class ChaosSaberAttack : AttackPattern
    {
        public ChaosSaberAttack()
        {
            Name = "Chaos Saber";
            Cooldown = CHAOS_SABER_COOLDOWN;
            Damage = 15;
        }

        public override void Execute(BossBase boss)
        {
            var asriel = (AsrielGodBoss)boss;

            // Play sound
            KirbyGame.Audio.PlaySfx("event:/pusheen/game/boss/asriel/chaos_saber");

            // Spawn saber projectiles that sweep across
            // Left to right then right to left
            bool leftToRight = Nez.Random.Chance(50);

            for (int i = 0; i < 3; i++)
            {
                Nez.Core.Schedule(i * 0.5f, _ =>
                {
                    var saber = boss.Entity.Scene.CreateEntity("chaos_saber");
                    saber.Position = boss.Entity.Position;
                    // saber.AddComponent(new ChaosSaberProjectile(leftToRight, Damage));
                    asriel.TrackProjectile(saber);
                });
            }

            Console.WriteLine("[Asriel] ⚔️ Chaos Saber! ⚔️");
        }
    }

    private class HyperGonerAttack : AttackPattern
    {
        public HyperGonerAttack()
        {
            Name = "Hyper Goner";
            Cooldown = HYPER_GONER_COOLDOWN;
            Damage = 30;
        }

        public override void Execute(BossBase boss)
        {
            // Ultimate attack - only in phase 3
            KirbyGame.Audio.PlaySfx("event:/pusheen/game/boss/asriel/hyper_goner");

            // Large energy beam that fills the arena
            var goner = boss.Entity.Scene.CreateEntity("hyper_goner_beam");
            goner.Position = boss.Entity.Position;
            // goner.AddComponent(new HyperGonerBeam(Damage));

            Console.WriteLine("[Asriel] ☠️ HYPER GONER! ☠️");
        }
    }

    private class FireMagicAttack : AttackPattern
    {
        public FireMagicAttack()
        {
            Name = "Fire Magic";
            Cooldown = 1.5f;
            Damage = 8;
        }

        public override void Execute(BossBase boss)
        {
            // Quick fireballs at player
            KirbyGame.Audio.PlaySfx("event:/pusheen/game/boss/asriel/fire_magic");

            for (int i = 0; i < 3; i++)
            {
                var fireball = boss.Entity.Scene.CreateEntity($"fireball_{i}");
                fireball.Position = boss.Entity.Position;
                // fireball.AddComponent(new FireballProjectile(
                //     target: boss.Scene.FindEntity("player").Position,
                //     damage: Damage
                // ));
            }

            Console.WriteLine("[Asriel] 🔥 Fire Magic! 🔥");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════
// SPAWNER UTILITY
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// Utility to spawn Asriel in a scene
/// </summary>
public static class AsrielSpawner
{
    public static Entity Spawn(Scene scene, Vector2 position)
    {
        var asriel = scene.CreateEntity("asriel_god_boss");
        asriel.Position = position;
        asriel.AddComponent(new AsrielGodBoss());

        // Trigger spawn
        var boss = asriel.GetComponent<AsrielGodBoss>();
        boss.OnSpawn();

        return asriel;
    }
}
