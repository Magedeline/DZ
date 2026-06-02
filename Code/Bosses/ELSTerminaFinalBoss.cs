#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Bosses;

[CustomEntity("MaggyHelper/ELSTerminaFinalBoss")]
[Tracked(false)]
public class ELSTerminaFinalBoss : Actor
{
    public Sprite Sprite;
    public int DifficultyMode; // 0 = Normal (darkness), 1 = Morpho, 2 = Celestial Morpho
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
    private bool hasFiveHeartGems;
    private bool morphoModeActive;

    private const float NORMAL_HEALTH = 400f;
    private const float MORPHO_HEALTH = 600f;
    private const float CELESTIAL_HEALTH = 800f;

    public ELSTerminaFinalBoss(Vector2 position, int difficultyMode, bool fromCutscene, bool hasFiveHeartGems)
        : base(position)
    {
        this.fromCutscene = fromCutscene;
        this.hasFiveHeartGems = hasFiveHeartGems;
        this.DifficultyMode = difficultyMode;
        this.morphoModeActive = difficultyMode >= 1;
        
        this.maxHealth = difficultyMode switch
        {
            0 => NORMAL_HEALTH,
            1 => MORPHO_HEALTH,
            2 => CELESTIAL_HEALTH,
            _ => NORMAL_HEALTH
        };
        this.health = this.maxHealth;
        this.currentBossPhase = 1;

        // Placeholder sprite - replace with actual sprite path
        this.Sprite = new Sprite(GFX.Game, "objects/ghostbuster/idle");
        this.Sprite.AddLoop("idle", "", 0.1f);
        this.Sprite.Play("idle");
        this.Sprite.CenterOrigin();
        this.Add(this.Sprite);

        Collider = new Hitbox(100f, 140f, -50f, -70f);
        this.DashCollider = new Hitbox(100f, 140f, -50f, -70f);

        this.state = new StateMachine(15);
        this.state.SetCallbacks(0, IdleUpdate, IdleCoroutine, IdleBegin, null);
        this.state.SetCallbacks(1, ChaseUpdate, ChaseCoroutine, ChaseBegin, null);
        this.state.SetCallbacks(2, AttackUpdate, AttackCoroutine, AttackBegin, null);
        this.state.SetCallbacks(3, PhaseTransitionUpdate, PhaseTransitionCoroutine, PhaseTransitionBegin, null);
        this.state.SetCallbacks(4, BeamSlashUpdate, BeamSlashCoroutine, BeamSlashBegin, null);
        this.state.SetCallbacks(5, BigGunFireUpdate, BigGunFireCoroutine, BigGunFireBegin, null);
        this.state.SetCallbacks(6, BubbleAttackUpdate, BubbleAttackCoroutine, BubbleAttackBegin, null);
        this.state.SetCallbacks(7, TeleportUpdate, TeleportCoroutine, TeleportBegin, null);
        this.state.SetCallbacks(8, DeathUpdate, DeathCoroutine, DeathBegin, null);
        this.state.SetCallbacks(9, DummyUpdate, null, DummyBegin, null);
        this.state.SetCallbacks(10, NormalDeathUpdate, NormalDeathCoroutine, NormalDeathBegin, null);
        this.state.SetCallbacks(11, MorphoDeathUpdate, MorphoDeathCoroutine, MorphoDeathBegin, null);
        this.state.SetCallbacks(12, CelestialDeathUpdate, CelestialDeathCoroutine, CelestialDeathBegin, null);
        this.state.SetCallbacks(13, StarDeathUpdate, StarDeathCoroutine, StarDeathBegin, null);
        this.state.SetCallbacks(14, FinalCoreUpdate, FinalCoreCoroutine, FinalCoreBegin, null);

        this.Add(this.state);
        this.Add(new PlayerCollider(OnPlayerCollide, Collider, null));
        this.Add(new PlayerCollider(OnPlayerDash, DashCollider, null));

        Depth = -12500;
    }

    public ELSTerminaFinalBoss(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Int("difficultyMode", 0), data.Bool("fromCutscene", false), data.Bool("hasFiveHeartGems", false))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = SceneAs<Level>();

        this.healthUI = new ELSTerminaHealth(this.maxHealth, this.DifficultyMode == 2);
        if (this.level != null)
            this.level.Add(this.healthUI);

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

        CheckPhaseTransition();
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
            this.state.State = 3;
        }
    }

    #region Audio Event Helpers
    private void PlayAudio(string eventName)
    {
        Audio.Play(eventName, Position);
    }

    private void PlayActivate() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Activate");
    private void PlayBeamSlash() => PlayAudio("event:/pusheen/extra_content/char/els/Els_BeamSlash");
    private void PlayBiggerGunFire() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Bigger_Gun_Fire");
    private void PlayBigHit() => PlayAudio("event:/pusheen/extra_content/char/els/Els_BigHit");
    private void PlayBubble() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Bubble");
    private void PlayBuild() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Build");
    private void PlayCharge() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Charge");
    private void PlayConsume() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Consume");
    private void PlayCreate() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Create");
    private void PlayCryingDeath() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Crying_Death");
    private void PlayDarkmatterSpawn() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Darkmatter_Spawn");
    private void PlayFinalCry() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Final_Cry");
    private void PlayIdle() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Idle");
    private void PlayImpact() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Impact");
    private void PlayIntro() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Intro");
    private void PlayKnockout() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Knockout");
    private void PlayLaugh() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Laugh");
    private void PlayPrecreate() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Precreate");
    private void PlayPredeath() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Predeath");
    private void PlayPreImpact() => PlayAudio("event:/pusheen/extra_content/char/els/Els_PreImpact");
    private void PlayRevival() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Revival");
    private void PlayRift() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Rift");
    private void PlayRiftBullet() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Rift_Bullet");
    private void PlayScreamHit() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Scream_Hit");
    private void PlayShellScreamer() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Shell_Screamer");
    private void PlayShellcrack() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Shellcrack");
    private void PlaySlice() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Slice");
    private void PlaySpawn() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Spawn");
    private void PlayStarDeath() => PlayAudio("event:/pusheen/extra_content/char/els/Els_StarDeath");
    private void PlayStarDeathGlitch() => PlayAudio("event:/pusheen/extra_content/char/els/Els_StarDeath_Glitch");
    private void PlayTeleport() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Teleport");
    private void PlayTimeManipulatorEnd() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Time_Manipulator_End");
    private void PlayTimeManipulatorStart() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Time_Manipulator_Start");
    private void PlayWoosh() => PlayAudio("event:/pusheen/extra_content/char/els/Els_Woosh");
    #endregion

    #region State 0: Idle
    private int IdleUpdate() => 0;

    private void IdleBegin()
    {
        this.Sprite.Play("idle");
        PlayIdle();
    }

    private IEnumerator IdleCoroutine()
    {
        yield return 1f;
        this.state.State = 1;
    }
    #endregion

    #region State 1: Chase
    private int ChaseUpdate()
    {
        Player? player = this.level?.Tracker.GetEntity<Player>();
        if (player == null)
            return 1;

        float speed = this.currentBossPhase switch
        {
            1 => 150f,
            2 => 200f,
            3 => 250f,
            4 => 350f,
            _ => 200f
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
        
        // Choose attack based on difficulty mode
        int nextAttack = this.DifficultyMode switch
        {
            0 => Calc.Random.Next(2, 7), // Normal mode attacks
            1 => Calc.Random.Next(4, 8), // Morpho mode includes beam slash
            2 => Calc.Random.Next(4, 9), // Celestial mode includes all attacks
            _ => 2
        };
        
        this.state.State = nextAttack;
    }
    #endregion

    #region State 2: Attack
    private int AttackUpdate()
    {
        Player? player = this.level?.Tracker.GetEntity<Player>();
        if (player == null)
            return 1;

        float attackSpeed = this.currentBossPhase * 100f;
        Vector2 direction = Vector2.Normalize(player.Center - Center);
        Position += direction * attackSpeed * Engine.DeltaTime;

        if (this.level?.OnInterval(0.05f) == true)
            TrailManager.Add(this, Color.Purple * 0.8f, 1f, false, false);

        return 2;
    }

    private void AttackBegin()
    {
        this.Sprite.Play("idle");
        PlayImpact();
    }

    private IEnumerator AttackCoroutine()
    {
        yield return 1.5f;
        this.state.State = 1;
    }
    #endregion

    #region State 3: Phase Transition
    private int PhaseTransitionUpdate() => 3;

    private void PhaseTransitionBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.level?.Flash(Color.White * 0.5f);
        this.Sprite.Color = Color.Gold;
        this.currRed = Color.Gold;
        PlayBuild();
    }

    private IEnumerator PhaseTransitionCoroutine()
    {
        yield return 1f;
        this.state.State = 1;
    }
    #endregion

    #region State 4: Beam Slash (Morpho mode)
    private int BeamSlashUpdate() => 4;

    private void BeamSlashBegin()
    {
        this.Sprite.Play("idle");
        PlayBeamSlash();
        PlayCharge();
    }

    private IEnumerator BeamSlashCoroutine()
    {
        yield return 0.5f;
        
        if (this.level != null)
        {
            this.level.Shake(0.3f);
            this.level.Displacement.AddBurst(Center, 0.4f, 0f, 80f, 1f, null, null);
        }
        
        PlaySlice();
        yield return 0.5f;
        this.state.State = 1;
    }
    #endregion

    #region State 5: Big Gun Fire
    private int BigGunFireUpdate() => 5;

    private void BigGunFireBegin()
    {
        this.Sprite.Play("idle");
        PlayBiggerGunFire();
        PlayCharge();
    }

    private IEnumerator BigGunFireCoroutine()
    {
        yield return 0.8f;
        
        if (this.level != null)
        {
            this.level.Shake(0.5f);
            this.level.Flash(Color.Red * 0.4f);
        }
        
        PlayBigHit();
        yield return 1f;
        this.state.State = 1;
    }
    #endregion

    #region State 6: Bubble Attack
    private int BubbleAttackUpdate() => 6;

    private void BubbleAttackBegin()
    {
        this.Sprite.Play("idle");
        PlayBubble();
    }

    private IEnumerator BubbleAttackCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            if (this.level != null)
                this.level.Shake(0.2f);
            yield return 0.3f;
        }
        this.state.State = 1;
    }
    #endregion

    #region State 7: Teleport
    private int TeleportUpdate() => 7;

    private void TeleportBegin()
    {
        this.Sprite.Color = Color.Magenta;
        this.currRed = Color.Magenta;
        PlayTeleport();
    }

    private IEnumerator TeleportCoroutine()
    {
        yield return 0.3f;
        
        if (this.level != null)
        {
            Position = new Vector2(
                Calc.Random.Range(this.level.Bounds.Left + 50, this.level.Bounds.Right - 50),
                Calc.Random.Range(this.level.Bounds.Top + 50, this.level.Bounds.Bottom - 50)
            );
            this.level.Shake(0.3f);
        }
        
        this.Sprite.Color = Color.White;
        yield return 0.3f;
        this.state.State = 1;
    }
    #endregion

    #region State 8: Death
    private int DeathUpdate() => 8;

    private void DeathBegin()
    {
        this.Dead = true;
        if (this.level != null)
            this.level.Shake(1f);
        this.level?.Flash(Color.White);
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
    private int DummyUpdate() => 9;

    private void DummyBegin()
    {
        Visible = false;
    }
    #endregion

    #region State 10: Normal Death (Darkness ELS)
    private int NormalDeathUpdate() => 10;

    private void NormalDeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.DarkRed;
        this.currRed = Color.DarkRed;
        PlayPredeath();
        PlayDarkmatterSpawn();
    }

    private IEnumerator NormalDeathCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            if (this.level != null)
                this.level.Shake(0.3f);
            yield return 0.5f;
        }

        if (this.level != null)
        {
            this.level.Shake(1f);
            this.level.Flash(Color.DarkRed * 0.5f);
            this.level.Displacement.AddBurst(Center, 0.5f, 0f, 100f, 1f, null, null);
        }

        PlayCryingDeath();
        yield return 1f;

        if (this.level != null && this.healthUI != null)
        {
            this.level.Shake(2f);
            this.level.Flash(Color.Black);
            this.healthUI.RemoveSelf();
        }

        RemoveSelf();
    }
    #endregion

    #region State 11: Morpho Death
    private int MorphoDeathUpdate() => 11;

    private void MorphoDeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.Purple;
        this.currRed = Color.Purple;
        PlayPredeath();
        PlayRift();
    }

    private IEnumerator MorphoDeathCoroutine()
    {
        // Morpho butterfly dissolution
        for (int i = 0; i < 8; i++)
        {
            float scale = 1f + (float)Math.Sin(i * 0.5f) * 0.3f;
            this.Sprite.Scale = new Vector2(scale, scale);
            if (this.level != null)
                this.level.Shake(0.3f);
            PlayRiftBullet();
            yield return 0.3f;
        }

        this.Sprite.Scale = Vector2.One;

        if (this.level != null)
        {
            this.level.Shake(1f);
            this.level.Flash(Color.Purple * 0.6f);
            this.level.Displacement.AddBurst(Center, 0.6f, 0f, 120f, 1f, null, null);
        }

        PlayStarDeath();
        yield return 1.5f;

        if (this.level != null && this.healthUI != null)
        {
            this.level.Shake(2f);
            this.level.Flash(Color.Purple);
            this.healthUI.RemoveSelf();
        }

        RemoveSelf();
    }
    #endregion

    #region State 12: Celestial Death
    private int CelestialDeathUpdate() => 12;

    private void CelestialDeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.5f);
        this.Sprite.Color = Color.Gold;
        this.currRed = Color.Gold;
        PlayPredeath();
        PlayFinalCry();
    }

    private IEnumerator CelestialDeathCoroutine()
    {
        // Celestial ascension with heart gem energy
        for (int i = 0; i < 10; i++)
        {
            Position.Y -= 8f;
            if (this.level != null)
            {
                this.level.Shake(0.4f);
                this.level.Flash(Color.Gold * 0.3f);
            }
            yield return 0.3f;
        }

        if (this.level != null)
        {
            this.level.Shake(1.5f);
            this.level.Flash(Color.Gold * 0.8f);
            this.level.Flash(Color.White * 0.6f);
            this.level.Displacement.AddBurst(Center, 1f, 0f, 200f, 1f, null, null);
        }

        PlayStarDeathGlitch();
        yield return 2f;

        if (this.level != null && this.healthUI != null)
        {
            this.level.Shake(3f);
            this.level.Flash(Color.White);
            this.level.Flash(Color.Gold);
            this.healthUI.RemoveSelf();
        }

        RemoveSelf();
    }
    #endregion

    #region State 13: Star Death
    private int StarDeathUpdate() => 13;

    private void StarDeathBegin()
    {
        if (this.level != null)
            this.level.Shake(0.8f);
        this.Sprite.Color = Color.White;
        this.currRed = Color.White;
        PlayStarDeath();
    }

    private IEnumerator StarDeathCoroutine()
    {
        // Star explosion
        for (int i = 0; i < 12; i++)
        {
            if (this.level != null)
            {
                this.level.Shake(0.5f);
                this.level.Flash(Color.White * 0.4f);
            }
            yield return 0.2f;
        }

        if (this.level != null)
        {
            this.level.Shake(2f);
            this.level.Flash(Color.White);
            this.level.Displacement.AddBurst(Center, 1.5f, 0f, 300f, 1f, null, null);
        }

        yield return 2f;
        this.state.State = 14;
    }
    #endregion

    #region State 14: Final Core
    private int FinalCoreUpdate() => 14;

    private void FinalCoreBegin()
    {
        if (this.level != null)
            this.level.Shake(1f);
        this.Sprite.Color = Color.White;
        this.currRed = Color.White;
        PlayFinalCry();
    }

    private IEnumerator FinalCoreCoroutine()
    {
        // Final core collapse
        for (int i = 0; i < 5; i++)
        {
            float scale = 1f - (i * 0.2f);
            this.Sprite.Scale = new Vector2(scale, scale);
            if (this.level != null)
                this.level.Shake(0.6f);
            yield return 0.4f;
        }

        if (this.level != null && this.healthUI != null)
        {
            this.level.Shake(3f);
            this.level.Flash(Color.White);
            this.healthUI.RemoveSelf();
        }

        RemoveSelf();
    }
    #endregion

    private void OnPlayerCollide(Player player)
    {
        if (!Visible || this.Dead)
            return;

        if (this.state.State != 3)
        {
            PlayerTakeDamage(player);
        }
    }

    private void OnPlayerDash(Player player)
    {
        if (!Visible || this.Dead || !player.DashAttacking || this.damageCooldown > 0)
            return;

        BossTakeDamage();
    }

    private void BossTakeDamage()
    {
        if (this.damageCooldown > 0 || this.Dead)
            return;

        float damage = 15f;
        this.health = Math.Max(0, this.health - damage);
        this.damageCooldown = 0.5f;

        this.Sprite.Color = Color.Red;
        this.currRed = Color.Red;
        if (this.level != null)
            this.level.Shake(0.2f);
        PlayScreamHit();

        if (this.healthUI != null)
            this.healthUI.UpdateHealth(this.health, this.maxHealth);

        if (this.health <= 0)
        {
            // Trigger difficulty-specific death
            this.state.State = this.DifficultyMode switch
            {
                0 => 10, // Normal death
                1 => 11, // Morpho death
                2 => 12, // Celestial death
                _ => 10
            };
        }
    }

    private void PlayerTakeDamage(Player player)
    {
        if (this.playerDamageCooldown > 0)
            return;

        this.playerDamageCooldown = 1f;
        if (this.level != null)
        {
            this.level.Shake(0.3f);
            this.level.Flash(Color.Red * 0.3f);
        }
    }
}
