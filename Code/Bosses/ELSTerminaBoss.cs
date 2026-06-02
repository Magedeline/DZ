#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Bosses;

[CustomEntity("MaggyHelper/ELSTerminaBoss")]
[Tracked(false)]
public class ELSTerminaBoss : Actor
{
    public Sprite Sprite;
    public int Phase;
    public bool Dead;
    private StateMachine state;
    private Level? level;
    private float health;
    private float maxHealth;
    private Hitbox DashCollider;
    private float damageCooldown;
    private float playerDamageCooldown;
    private Color currRed;
    private ELSTerminaHealth? healthUI;
    private int currentBossPhase;
    private bool fromCutscene;
    private bool hardMode;

    private const float PHASE_1_HEALTH = 100f;
    private const float PHASE_2_HEALTH = 150f;
    private const float PHASE_3_HEALTH = 200f;
    private const float PHASE_4_HEALTH = 300f;

    public ELSTerminaBoss(Vector2 position, int phase, bool fromCutscene, bool hardMode)
        : base(position)
    {
        this.fromCutscene = fromCutscene;
        this.hardMode = hardMode;
        this.Phase = phase;
        this.maxHealth = phase switch
        {
            1 => PHASE_1_HEALTH,
            2 => PHASE_2_HEALTH,
            3 => PHASE_3_HEALTH,
            4 => PHASE_4_HEALTH,
            _ => PHASE_4_HEALTH
        };
        this.health = this.maxHealth;
        this.currentBossPhase = 1;

        // Placeholder sprite - replace with actual sprite path
        // this.Sprite = GFX.SpriteBank.Create("els_termina_boss");
        this.Sprite = new Sprite(GFX.Game, "objects/ghostbuster/idle");
        this.Sprite.AddLoop("idle", "", 0.1f);
        this.Sprite.Play("idle");
        this.Sprite.CenterOrigin();
        this.Add(this.Sprite);

        Collider = new Hitbox(80f, 120f, -40f, -60f);
        this.DashCollider = new Hitbox(80f, 120f, -40f, -60f);

        this.state = new StateMachine(14);
        this.state.SetCallbacks(0, IdleUpdate, IdleCoroutine, IdleBegin, null);
        this.state.SetCallbacks(1, ChaseUpdate, ChaseCoroutine, ChaseBegin, null);
        this.state.SetCallbacks(2, AttackUpdate, AttackCoroutine, AttackBegin, null);
        this.state.SetCallbacks(3, PhaseTransitionUpdate, PhaseTransitionCoroutine, PhaseTransitionBegin, null);
        this.state.SetCallbacks(4, SpecialAttackUpdate, SpecialAttackCoroutine, SpecialAttackBegin, null);
        this.state.SetCallbacks(5, ShieldUpdate, ShieldCoroutine, ShieldBegin, null);
        this.state.SetCallbacks(6, TeleportUpdate, TeleportCoroutine, TeleportBegin, null);
        this.state.SetCallbacks(7, FinalPhaseUpdate, FinalPhaseCoroutine, FinalPhaseBegin, null);
        this.state.SetCallbacks(8, DeathUpdate, DeathCoroutine, DeathBegin, null);
        this.state.SetCallbacks(9, DummyUpdate, null, DummyBegin, null);
        this.state.SetCallbacks(10, Phase1DeathUpdate, Phase1DeathCoroutine, Phase1DeathBegin, null);
        this.state.SetCallbacks(11, Phase2DeathUpdate, Phase2DeathCoroutine, Phase2DeathBegin, null);
        this.state.SetCallbacks(12, Phase3DeathUpdate, Phase3DeathCoroutine, Phase3DeathBegin, null);
        this.state.SetCallbacks(13, Phase4DeathUpdate, Phase4DeathCoroutine, Phase4DeathBegin, null);

        this.Add(this.state);
        this.Add(new PlayerCollider(OnPlayerCollide, Collider, null));
        this.Add(new PlayerCollider(OnPlayerDash, DashCollider, null));

        Depth = -12500;
    }

    public ELSTerminaBoss(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Int("phase", 4), data.Bool("fromCutscene", false), data.Bool("hardMode", false))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = SceneAs<Level>();

        // Add health UI for phase 4
        if (this.Phase == 4 && this.level != null)
        {
            this.healthUI = new ELSTerminaHealth(this.maxHealth, this.hardMode);
            this.level.Add(this.healthUI);
        }

        if (!this.fromCutscene)
            this.state.State = 9;
        else
            this.state.State = 0;
    }

    public override void Update()
    {
        base.Update();

        if (this.damageCooldown > 0f)
            this.damageCooldown -= Engine.DeltaTime;

        if (this.playerDamageCooldown > 0f)
            this.playerDamageCooldown -= Engine.DeltaTime;

        if (this.Sprite.Color != Color.White)
            this.Sprite.Color = Color.Lerp(this.currRed, Color.White, 0.2f);

        // Check phase transitions based on health
        if (this.Phase == 4)
        {
            CheckPhaseTransition();
        }
    }

    private void CheckPhaseTransition()
    {
        float healthPercent = this.health / this.maxHealth;

        int newPhase = healthPercent switch
        {
            <= 0.25f => 4,
            <= 0.5f => 3,
            <= 0.75f => 2,
            _ => 1
        };

        if (newPhase != this.currentBossPhase && this.state.State != 3)
        {
            this.currentBossPhase = newPhase;
            this.state.State = 3; // Phase transition state
        }
    }

    #region State 0: Idle
    private int IdleUpdate()
    {
        return 0;
    }

    private void IdleBegin()
    {
        this.Sprite.Play("idle");
    }

    private IEnumerator IdleCoroutine()
    {
        yield return 1f;
        this.state.State = 1;
    }
    #endregion

    #region State 1: Chase Player
    private int ChaseUpdate()
    {
        Player? player = this.level.Tracker.GetEntity<Player>();
        if (player == null)
            return 1;

        float speed = this.currentBossPhase switch
        {
            1 => 100f,
            2 => 150f,
            3 => 200f,
            4 => 300f,
            _ => 150f
        };

        Vector2 target = player.Center;
        Position = Vector2.Lerp(Position, target, speed * Engine.DeltaTime / 100f);

        return 1;
    }

    private void ChaseBegin()
    {
        this.Sprite.Play("idle");
    }

    private IEnumerator ChaseCoroutine()
    {
        yield return 2f;
        this.state.State = 2;
    }
    #endregion

    #region State 2: Attack
    private int AttackUpdate()
    {
        Player? player = this.level.Tracker.GetEntity<Player>();
        if (player == null)
            return 1;

        float attackSpeed = this.currentBossPhase switch
        {
            1 => 200f,
            2 => 300f,
            3 => 400f,
            4 => 500f,
            _ => 300f
        };

        Vector2 direction = Vector2.Normalize(player.Center - Center);
        Position += direction * attackSpeed * Engine.DeltaTime;

        if (this.level.OnInterval(0.05f))
            TrailManager.Add(this, Color.Purple * 0.8f, 1f, false, false);

        return 2;
    }

    private void AttackBegin()
    {
        this.Sprite.Play("idle");
    }

    private IEnumerator AttackCoroutine()
    {
        yield return 1.5f;
        this.state.State = 1;
    }
    #endregion

    #region State 3: Phase Transition
    private int PhaseTransitionUpdate()
    {
        return 3;
    }

    private void PhaseTransitionBegin()
    {
        this.level.Shake(0.5f);
        this.level.Flash(Color.White * 0.5f);
        this.Sprite.Color = Color.Gold;
        this.currRed = Color.Gold;
    }

    private IEnumerator PhaseTransitionCoroutine()
    {
        yield return 1f;
        
        // Choose next attack pattern based on phase
        int nextPattern = this.currentBossPhase switch
        {
            2 => 4, // Special attack
            3 => 5, // Shield
            4 => 6, // Teleport
            _ => 1
        };

        this.state.State = nextPattern;
    }
    #endregion

    #region State 4: Special Attack
    private int SpecialAttackUpdate()
    {
        return 4;
    }

    private void SpecialAttackBegin()
    {
        this.Sprite.Play("idle");
    }

    private IEnumerator SpecialAttackCoroutine()
    {
        // Phase 2 special: Spread attack
        for (int i = 0; i < 8; i++)
        {
            this.level.Shake(0.2f);
            yield return 0.3f;
        }
        this.state.State = 1;
    }
    #endregion

    #region State 5: Shield
    private int ShieldUpdate()
    {
        return 5;
    }

    private void ShieldBegin()
    {
        this.Sprite.Color = Color.Cyan;
        this.currRed = Color.Cyan;
    }

    private IEnumerator ShieldCoroutine()
    {
        // Phase 3: Shielded attack
        yield return 2f;
        this.Sprite.Color = Color.White;
        this.state.State = 2;
    }
    #endregion

    #region State 6: Teleport
    private int TeleportUpdate()
    {
        return 6;
    }

    private void TeleportBegin()
    {
        this.Sprite.Color = Color.Magenta;
        this.currRed = Color.Magenta;
    }

    private IEnumerator TeleportCoroutine()
    {
        // Phase 4: Teleport attack
        Player? player = this.level.Tracker.GetEntity<Player>();
        if (player != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Position = new Vector2(
                    Calc.Random.Range(this.level.Bounds.Left + 50, this.level.Bounds.Right - 50),
                    Calc.Random.Range(this.level.Bounds.Top + 50, this.level.Bounds.Bottom - 50)
                );
                this.level.Shake(0.3f);
                yield return 0.5f;
            }
        }
        this.Sprite.Color = Color.White;
        this.state.State = 7;
    }
    #endregion

    #region State 7: Final Phase
    private int FinalPhaseUpdate()
    {
        return 7;
    }

    private void FinalPhaseBegin()
    {
        this.Sprite.Color = Color.Red;
        this.currRed = Color.Red;
    }

    private IEnumerator FinalPhaseCoroutine()
    {
        // Phase 4 final: Aggressive pattern
        while (this.health > 0)
        {
            this.state.State = 2; // Attack
            yield return 1f;
            this.state.State = 6; // Teleport
            yield return 1f;
        }
    }
    #endregion

    #region State 8: Death
    private int DeathUpdate()
    {
        return 8;
    }

    private void DeathBegin()
    {
        this.Dead = true;
        if (this.level != null)
            this.level.Shake(1f);
        if (this.level != null)
            this.level.Flash(Color.White);
        RemoveSelf();
        
        if (this.healthUI != null)
            this.healthUI.RemoveSelf();
    }

    private IEnumerator DeathCoroutine()
    {
        yield break;
    }
    #endregion

    #region State 9: Dummy
    private int DummyUpdate()
    {
        return 9;
    }

    private void DummyBegin()
    {
        Visible = false;
    }
    #endregion

    #region State 10: Phase 1 Death - Demon Walking and Crushing
    private int Phase1DeathUpdate()
    {
        return 10;
    }

    private void Phase1DeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.DarkRed;
        this.currRed = Color.DarkRed;
        // Transform to demon form - placeholder sprite change
        // this.Sprite.Play("demon_transform");
    }

    private IEnumerator Phase1DeathCoroutine()
    {
        // Demon walking animation
        for (int i = 0; i < 5; i++)
        {
            if (this.level != null)
                this.level.Shake(0.3f);
            yield return 0.5f;
        }

        // Crushing attack
        if (this.level != null)
        {
            this.level.Shake(1f);
            this.level.Flash(Color.DarkRed * 0.5f);
            this.level.Displacement.AddBurst(Center, 0.5f, 0f, 100f, 1f, null, null);
        }

        yield return 1f;

        // Transition to next phase or death
        if (this.Phase < 4)
        {
            // Reset health for next phase
            this.Phase++;
            this.maxHealth = this.Phase switch
            {
                2 => PHASE_2_HEALTH,
                3 => PHASE_3_HEALTH,
                4 => PHASE_4_HEALTH,
                _ => PHASE_4_HEALTH
            };
            this.health = this.maxHealth;
            this.currentBossPhase = 1;
            this.Sprite.Color = Color.White;
            this.state.State = 0;
        }
        else
        {
            this.state.State = 8;
        }
    }
    #endregion

    #region State 11: Phase 2 Death - Vessel of Heart
    private int Phase2DeathUpdate()
    {
        return 11;
    }

    private void Phase2DeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.Pink;
        this.currRed = Color.Pink;
        // Transform to vessel of heart form
        // this.Sprite.Play("vessel_heart_transform");
    }

    private IEnumerator Phase2DeathCoroutine()
    {
        // Heart vessel pulsing animation
        for (int i = 0; i < 8; i++)
        {
            float scale = 1f + (float)Math.Sin(i * 0.5f) * 0.2f;
            this.Sprite.Scale = new Vector2(scale, scale);
            if (this.level != null)
                this.level.Shake(0.2f);
            yield return 0.3f;
        }

        this.Sprite.Scale = Vector2.One;

        // Heart explosion effect
        if (this.level != null)
        {
            this.level.Shake(0.8f);
            this.level.Flash(Color.Pink * 0.6f);
            this.level.Displacement.AddBurst(Center, 0.6f, 0f, 120f, 1f, null, null);
        }

        yield return 1f;

        // Transition to next phase or death
        if (this.Phase < 4)
        {
            this.Phase++;
            this.maxHealth = this.Phase switch
            {
                3 => PHASE_3_HEALTH,
                4 => PHASE_4_HEALTH,
                _ => PHASE_4_HEALTH
            };
            this.health = this.maxHealth;
            this.currentBossPhase = 1;
            this.Sprite.Color = Color.White;
            this.state.State = 0;
        }
        else
        {
            this.state.State = 8;
        }
    }
    #endregion

    #region State 12: Phase 3 Death - Angel of Sorrow and Chaos
    private int Phase3DeathUpdate()
    {
        return 12;
    }

    private void Phase3DeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.Gold;
        this.currRed = Color.Gold;
        // Transform to angel form
        // this.Sprite.Play("angel_transform");
    }

    private IEnumerator Phase3DeathCoroutine()
    {
        // Angel ascending animation with chaos energy
        for (int i = 0; i < 6; i++)
        {
            Position.Y -= 10f;
            if (this.level != null)
            {
                this.level.Shake(0.4f);
                // Chaos energy particles - placeholder
                // this.level.Particles.Emit(...);
            }
            yield return 0.4f;
        }

        // Sorrow and chaos explosion
        if (this.level != null)
        {
            this.level.Shake(1f);
            this.level.Flash(Color.Gold * 0.7f);
            this.level.Flash(Color.Purple * 0.5f);
            this.level.Displacement.AddBurst(Center, 0.8f, 0f, 150f, 1f, null, null);
        }

        yield return 1.5f;

        // Transition to final phase or death
        if (this.Phase < 4)
        {
            this.Phase = 4;
            this.maxHealth = PHASE_4_HEALTH;
            this.health = this.maxHealth;
            this.currentBossPhase = 1;
            Position = Center; // Reset position
            this.Sprite.Color = Color.White;
            this.state.State = 0;
        }
        else
        {
            this.state.State = 8;
        }
    }
    #endregion

    #region State 13: Phase 4 Death - Final Core with Multi-Character Faces
    private int Phase4DeathUpdate()
    {
        return 13;
    }

    private void Phase4DeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.White;
        this.currRed = Color.White;
        // Transform to final core form
        // this.Sprite.Play("final_core_transform");
    }

    private IEnumerator Phase4DeathCoroutine()
    {
        // Cycle through character faces
        string[] characterFaces = new string[]
        {
            "kirby",      // Kirby series
            "dedede",     // Kirby series
            "meta_knight", // Kirby series
            "frisk",      // Undertale series
            "chara",      // Undertale series
            "sans",       // Undertale series
            "madeline",   // Celeste
            "badeline",   // Celeste
            "zero_eye"    // Zero eye reference
        };

        for (int i = 0; i < characterFaces.Length; i++)
        {
            // Change face - placeholder
            // this.Sprite.Play(characterFaces[i]);
            
            if (this.level != null)
                this.level.Shake(0.3f);
            
            // Flash colors based on character
            Color faceColor = i switch
            {
                0 or 1 or 2 => Color.Pink,      // Kirby characters
                3 or 4 or 5 => Color.Yellow,     // Undertale characters
                6 or 7 => Color.Red,             // Celeste characters
                8 => Color.Cyan,                 // Zero eye
                _ => Color.White
            };
            
            this.Sprite.Color = faceColor;
            this.currRed = faceColor;
            
            yield return 0.4f;
        }

        // Final core explosion with all characters
        if (this.level != null)
        {
            this.level.Shake(2f);
            this.level.Flash(Color.White);
            this.level.Flash(Color.Pink);
            this.level.Flash(Color.Yellow);
            this.level.Flash(Color.Red);
            this.level.Displacement.AddBurst(Center, 1f, 0f, 200f, 1f, null, null);
        }

        yield return 2f;

        // Final death
        this.state.State = 8;
    }
    #endregion

    private void OnPlayerCollide(Player player)
    {
        if (!Visible || this.Dead)
            return;

        if (this.Phase == 4 && this.state.State != 3)
        {
            PlayerTakeDamage(player);
        }
        else
        {
            player.Die(Vector2.Normalize(player.Center - Center), false, true);
        }
    }

    private void OnPlayerDash(Player player)
    {
        if (!Visible || this.Dead || !player.DashAttacking || this.damageCooldown > 0)
            return;

        if (this.Phase == 4)
        {
            BossTakeDamage();
        }
    }

    private void BossTakeDamage()
    {
        if (this.damageCooldown > 0 || this.Dead)
            return;

        float damage = 10f;
        this.health = Math.Max(0, this.health - damage);
        this.damageCooldown = 0.5f;

        this.Sprite.Color = Color.Red;
        this.currRed = Color.Red;
        if (this.level != null)
            this.level.Shake(0.2f);

        if (this.healthUI != null)
            this.healthUI.UpdateHealth(this.health, this.maxHealth);

        if (this.health <= 0)
        {
            // Trigger phase-specific death transformation
            if (this.Phase < 4)
            {
                this.state.State = this.Phase switch
                {
                    1 => 10, // Phase 1 death transformation
                    2 => 11, // Phase 2 death transformation
                    3 => 12, // Phase 3 death transformation
                    _ => 8
                };
            }
            else
            {
                this.state.State = 8; // Final death
            }
        }
    }

    private void PlayerTakeDamage(Player player)
    {
        if (this.playerDamageCooldown > 0)
            return;

        this.playerDamageCooldown = 1f;
        this.level.Shake(0.3f);
        this.level.Flash(Color.Red * 0.3f);
        
        // Player damage logic here
        // You can integrate with your health system
    }
}
