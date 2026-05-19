using System.Runtime.CompilerServices;
using Celeste;
using global::Celeste.Mod.MaggyHelper;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste;

/// <summary>
/// Skill-based Kirby player controller replacing hover/inhale/spit with precision and combat mechanics.
/// New system: Air Drift, Cyclone Slash, Star Shot, Slide Tackle, Counter Stance, Dive Kick
/// Dash is preserved as the core movement ability.
/// </summary>
public static class KirbyPlayerStateController
{
    private class KirbyPlayerData
    {
        // Stamina system (kept for skill management)
        public float Stamina = 100f;
        public float MaxStamina = 100f;
        public int ComboCount;
        public float LastActionTime;
        public bool IsBossFightMode;
        public float InvincibilityTimer;
        
        // New skill state tracking
        public bool IsAirDrifting;
        public bool IsCycloneSlashing;
        public bool IsStarShotCharging;
        public bool IsSlideTackling;
        public bool IsCounterStancing;
        public bool IsDiveKicking;
        
        // Unlockable skills
        public bool HasAquaGrappleUnlocked;
        public bool IsAquaGrappling;
        public Vector2 GrappleTarget;
        public float GrappleLength;
        public float GrappleSwingAngle;
        public bool IsGrappleRetracting;
        
        // Skill timers and charges
        public float AirDriftTimer;
        public float CycloneSlashTimer;
        public float StarShotCharge;
        public float SlideTackleTimer;
        public float CounterStanceTimer;
        public float DiveKickTimer;
        public float ParryWindow;
        public float AquaGrappleTimer;
        public float GrappleRetractTimer;
        
        // Movement tracking
        public Vector2 LastSafePosition;
        public int ConsecutiveHits;
        
        // State IDs
        public int StAirDrift = -1;
        public int StCycloneSlash = -1;
        public int StStarShot = -1;
        public int StSlideTackle = -1;
        public int StCounterStance = -1;
        public int StDiveKick = -1;
        public int StAquaGrapple = -1;
    }

    private static readonly ConditionalWeakTable<Player, KirbyPlayerData> PlayerData = new();

    // New Skill System Constants
    private const float StaminaMax = 100f;
    private const float StaminaRegenRate = 8f;
    private const float StaminaRegenAirRate = 3f;
    private const float ComboResetTime = 3f;
    private const float InvincibilityDuration = 1f;
    
    // Air Drift Constants
    private const float AirDriftDuration = 0.4f;
    private const float AirDriftSpeed = 90f;
    private const float AirDriftStaminaCost = 25f;
    
    // Cyclone Slash Constants
    private const float CycloneSlashDuration = 0.5f;
    private const float CycloneSlashStaminaCost = 20f;
    private const float CycloneSlashRadius = 32f;
    
    // Star Shot Constants
    private const float StarShotChargeMax = 1.0f;
    private const float StarShotMinCharge = 0.15f;
    private const float StarShotStaminaCost = 15f;
    private const float StarShotSpeed = 280f;
    
    // Slide Tackle Constants
    private const float SlideTackleDuration = 0.6f;
    private const float SlideTackleSpeed = 240f;
    private const float SlideTackleStaminaCost = 30f;
    
    // Counter Stance Constants
    private const float CounterStanceDuration = 1.0f;
    private const float CounterStanceStaminaCost = 35f;
    private const float ParryWindowDuration = 0.25f;
    
    // Dive Kick Constants
    private const float DiveKickDuration = 0.5f;
    private const float DiveKickSpeed = 350f;
    private const float DiveKickStaminaCost = 20f;
    
    // Aqua Grapple Constants
    private const float AquaGrappleMaxRange = 120f;
    private const float AquaGrappleStaminaCost = 25f;
    private const float AquaGrappleSwingSpeed = 180f;
    private const float AquaGrappleRetractSpeed = 300f;
    private const float AquaGrappleMaxDuration = 3f;
    private const float AquaGrappleSwingGravity = 400f;
    private const float AquaGrappleBounceFactor = 0.7f;
    
    // Boss fight boost
    private const float BossFightDamageBoost = 1.5f;

    public static void Load()
    {
        On.Celeste.Player.Added += Hook_Player_Added;
        On.Celeste.Player.Update += Hook_Player_Update;
        On.Celeste.Player.NormalUpdate += Hook_Player_NormalUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Loaded with Skill-Based Combat System");
    }

    public static void Unload()
    {
        On.Celeste.Player.Added -= Hook_Player_Added;
        On.Celeste.Player.Update -= Hook_Player_Update;
        On.Celeste.Player.NormalUpdate -= Hook_Player_NormalUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Unloaded Skill-Based Combat System");
    }

    private static void Hook_Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);

        var data = PlayerData.GetOrCreateValue(self);
        
        // Register new skill states
        data.StAirDrift = self.StateMachine.AddState(
            "KirbyAirDrift",
            () => AirDriftUpdate(self),
            null,
            () => AirDriftBegin(self),
            () => AirDriftEnd(self));
        data.StCycloneSlash = self.StateMachine.AddState(
            "KirbyCycloneSlash",
            () => CycloneSlashUpdate(self),
            null,
            () => CycloneSlashBegin(self),
            () => CycloneSlashEnd(self));
        data.StStarShot = self.StateMachine.AddState(
            "KirbyStarShot",
            () => StarShotUpdate(self),
            null,
            () => StarShotBegin(self),
            () => StarShotEnd(self));
        data.StSlideTackle = self.StateMachine.AddState(
            "KirbySlideTackle",
            () => SlideTackleUpdate(self),
            null,
            () => SlideTackleBegin(self),
            () => SlideTackleEnd(self));
        data.StCounterStance = self.StateMachine.AddState(
            "KirbyCounterStance",
            () => CounterStanceUpdate(self),
            null,
            () => CounterStanceBegin(self),
            () => CounterStanceEnd(self));
        data.StDiveKick = self.StateMachine.AddState(
            "KirbyDiveKick",
            () => DiveKickUpdate(self),
            null,
            () => DiveKickBegin(self),
            () => DiveKickEnd(self));
        data.StAquaGrapple = self.StateMachine.AddState(
            "KirbyAquaGrapple",
            () => AquaGrappleUpdate(self),
            null,
            () => AquaGrappleBegin(self),
            () => AquaGrappleEnd(self));
    }

    private static void Hook_Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.Scene == null)
            return;

        var data = PlayerData.GetOrCreateValue(self);
        
        // Update combo timer
        if (Engine.Scene.RawTimeActive - data.LastActionTime > ComboResetTime)
        {
            data.ComboCount = 0;
        }
        
        // Update invincibility
        if (data.InvincibilityTimer > 0f)
        {
            data.InvincibilityTimer -= Engine.DeltaTime;
        }
        
        // Regenerate stamina
        RegenerateStamina(self, data);
        
        // Check for boss fight mode
        UpdateBossFightMode(self, data);
        
        // Reset states on ground
        if (self.OnGround())
        {
            data.ComboCount = 0;
        }
    }

    private static int Hook_Player_NormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        int nextState = orig(self);

        if (nextState != Player.StNormal)
            return nextState;

        var data = PlayerData.GetOrCreateValue(self);
        
        // Check for skill activation - prioritize based on context
        
        // Air Drift: Jump (in air) for brief air control
        if (ShouldStartAirDrift(self, data) && data.StAirDrift >= 0)
        {
            Input.Jump.ConsumeBuffer();
            return data.StAirDrift;
        }
        
        // Counter Stance: Grab (on ground) to parry
        if (ShouldStartCounterStance(self, data) && data.StCounterStance >= 0)
        {
            Input.Grab.ConsumeBuffer();
            return data.StCounterStance;
        }
        
        // Dive Kick: Down + Jump (in air) for fast attack
        if (ShouldStartDiveKick(self, data) && data.StDiveKick >= 0)
        {
            Input.Jump.ConsumeBuffer();
            return data.StDiveKick;
        }
        
        // Cyclone Slash: Dash + Jump (in air) for spin attack
        if (ShouldStartCycloneSlash(self, data) && data.StCycloneSlash >= 0)
        {
            Input.Dash.ConsumeBuffer();
            return data.StCycloneSlash;
        }
        
        // Star Shot: Grab (in air) to charge and shoot
        if (ShouldStartStarShot(self, data) && data.StStarShot >= 0)
        {
            Input.Grab.ConsumeBuffer();
            return data.StStarShot;
        }
        
        // Slide Tackle: Down + Dash (on ground) for low dash attack
        if (ShouldStartSlideTackle(self, data) && data.StSlideTackle >= 0)
        {
            Input.Dash.ConsumeBuffer();
            return data.StSlideTackle;
        }
        
        // Aqua Grapple: Grab + Up (in air, unlocked) to shoot water tether
        if (ShouldStartAquaGrapple(self, data) && data.StAquaGrapple >= 0)
        {
            Input.Grab.ConsumeBuffer();
            return data.StAquaGrapple;
        }

        return nextState;
    }

    #region Skill Activation Checks

    private static bool ShouldStartAirDrift(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StAirDrift < 0)
            return false;
        if (player.OnGround() || data.Stamina < AirDriftStaminaCost)
            return false;
        // Air Drift: Jump while in air
        return Input.Jump.Pressed && !player.OnGround();
    }

    private static bool ShouldStartCycloneSlash(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StCycloneSlash < 0)
            return false;
        if (player.OnGround() || data.Stamina < CycloneSlashStaminaCost)
            return false;
        // Cyclone Slash: Dash while in air
        return Input.Dash.Pressed && !player.OnGround();
    }

    private static bool ShouldStartStarShot(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StStarShot < 0)
            return false;
        if (player.OnGround() || data.Stamina < StarShotStaminaCost)
            return false;
        // Star Shot: Grab while in air
        return Input.Grab.Pressed && !player.OnGround();
    }

    private static bool ShouldStartSlideTackle(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StSlideTackle < 0)
            return false;
        if (!player.OnGround() || data.Stamina < SlideTackleStaminaCost)
            return false;
        // Slide Tackle: Down + Dash on ground
        return Input.Dash.Pressed && Input.MoveY.Value > 0 && player.OnGround();
    }

    private static bool ShouldStartCounterStance(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StCounterStance < 0)
            return false;
        if (!player.OnGround() || data.Stamina < CounterStanceStaminaCost)
            return false;
        // Counter Stance: Grab on ground
        return Input.Grab.Pressed && player.OnGround();
    }

    private static bool ShouldStartDiveKick(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StDiveKick < 0)
            return false;
        if (player.OnGround() || data.Stamina < DiveKickStaminaCost)
            return false;
        // Dive Kick: Down + Jump in air
        return Input.Jump.Pressed && Input.MoveY.Value > 0 && !player.OnGround();
    }

    #endregion

    #region Skill Begin Methods

    private static void AirDriftBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsAirDrifting = true;
        data.AirDriftTimer = AirDriftDuration;
        data.Stamina -= AirDriftStaminaCost;
        
        // Brief upward momentum and air control
        player.Speed.Y = -60f;
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("float"))
                player.Sprite.Play("float");
            else if (player.Sprite.Has("hover"))
                player.Sprite.Play("hover");
            
            player.Sprite.Scale = new Vector2(1.1f, 0.9f);
        }
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 6, player.Center, Vector2.One * 12f);
            Audio.Play("event:/desolozantas/char/kirby/dash_charge", player.Position);
        }
    }

    private static void CycloneSlashBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsCycloneSlashing = true;
        data.CycloneSlashTimer = CycloneSlashDuration;
        data.Stamina -= CycloneSlashStaminaCost;
        
        // Spinning attack that damages enemies
        player.Speed.X = 0f;
        player.Speed.Y = -30f; // Slight upward lift
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("spin"))
                player.Sprite.Play("spin");
            else if (player.Sprite.Has("dash"))
                player.Sprite.Play("dash");
            
            player.Sprite.Scale = new Vector2(1.3f, 0.7f);
            player.Sprite.Color = Color.Orange * 0.8f;
        }
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 12, player.Center, Vector2.One * 20f);
            level.Shake(0.15f);
            Audio.Play("event:/desolozantas/char/kirby/kirby_knight/spin", player.Position);
        }
        
        // Damage nearby enemies immediately
        DamageNearbyEnemies(player, CycloneSlashRadius, 1);
    }

    private static void StarShotBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsStarShotCharging = true;
        data.StarShotCharge = 0f;
        data.Stamina -= StarShotStaminaCost;
        
        // Slow descent while charging
        player.Speed.Y = -20f;
        player.Speed.X *= 0.5f;
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("attack"))
                player.Sprite.Play("attack");
            else if (player.Sprite.Has("spit"))
                player.Sprite.Play("spit");
            
            player.Sprite.Scale = new Vector2(1.1f, 0.9f);
            player.Sprite.Color = Color.Yellow * 0.8f;
        }
        
        if (player.Scene is Level level)
        {
            Audio.Play("event:/desolozantas/char/kirby/inhale_start", player.Position);
        }
    }

    private static void SlideTackleBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsSlideTackling = true;
        data.SlideTackleTimer = SlideTackleDuration;
        data.Stamina -= SlideTackleStaminaCost;
        
        // Low profile dash attack
        player.Speed.X = SlideTackleSpeed * (int)player.Facing;
        player.Speed.Y = 0f;
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("dash"))
                player.Sprite.Play("dash");
            else if (player.Sprite.Has("runFast"))
                player.Sprite.Play("runFast");
            
            player.Sprite.Scale = new Vector2(1.4f, 0.5f); // Flattened for low profile
            player.Sprite.Color = Color.Cyan * 0.8f;
        }
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 8, player.Center, Vector2.One * 16f);
            level.Shake(0.1f);
            Audio.Play("event:/desolozantas/char/kirby/kirby_knight/punch_A", player.Position);
        }
        
        // Break through crumble blocks if any in path
        BreakCrumbleBlocksInPath(player);
    }

    private static void CounterStanceBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsCounterStancing = true;
        data.CounterStanceTimer = CounterStanceDuration;
        data.ParryWindow = ParryWindowDuration;
        data.Stamina -= CounterStanceStaminaCost;
        
        // Defensive stance - reduced movement
        player.Speed *= 0.1f;
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("determined"))
                player.Sprite.Play("determined");
            else if (player.Sprite.Has("idleA"))
                player.Sprite.Play("idleA");
            
            player.Sprite.Scale = new Vector2(1.1f, 1.1f);
            player.Sprite.Color = Color.Gold * 0.7f;
        }
        
        if (player.Scene is Level level)
        {
            // Create shield effect
            for (int i = 0; i < 6; i++)
            {
                float angle = (MathHelper.TwoPi / 6f) * i;
                Vector2 shieldPos = player.Center + new Vector2(
                    (float)Math.Cos(angle) * 30f,
                    (float)Math.Sin(angle) * 30f
                );
                level.Particles.Emit(ParticleTypes.SparkyDust, 2, shieldPos, Vector2.One * 8f);
            }
            
            Audio.Play("event:/desolozantas/char/kirby/core_hair_charged", player.Position);
        }
    }

    private static void DiveKickBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsDiveKicking = true;
        data.DiveKickTimer = DiveKickDuration;
        data.Stamina -= DiveKickStaminaCost;
        
        // Fast downward attack
        player.Speed.Y = DiveKickSpeed;
        player.Speed.X = Input.MoveX.Value * 100f;
        
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("dash"))
                player.Sprite.Play("dash");
            else if (player.Sprite.Has("jumpFast"))
                player.Sprite.Play("jumpFast");
            
            player.Sprite.Scale = new Vector2(0.7f, 1.4f); // Stretched for dive
            player.Sprite.Color = Color.Red * 0.8f;
        }
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 10, player.Center, Vector2.One * 20f);
            level.Shake(0.2f);
            Audio.Play("event:/desolozantas/char/kirby/kirby_knight/punch_Final", player.Position);
        }
    }

    #endregion

    #region Skill End Methods

    private static void AirDriftEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsAirDrifting = false;
        data.AirDriftTimer = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
    }

    private static void CycloneSlashEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsCycloneSlashing = false;
        data.CycloneSlashTimer = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 6, player.Center, Vector2.One * 12f);
        }
    }

    private static void StarShotEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsStarShotCharging = false;
        
        // Fire projectile if charged enough
        if (data.StarShotCharge >= StarShotMinCharge)
        {
            FireStarShot(player, data.StarShotCharge);
        }
        
        data.StarShotCharge = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
    }

    private static void SlideTackleEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsSlideTackling = false;
        data.SlideTackleTimer = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
        
        // Brief slide stop
        player.Speed.X *= 0.3f;
    }

    private static void CounterStanceEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsCounterStancing = false;
        data.CounterStanceTimer = 0f;
        data.ParryWindow = 0f;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
    }

    private static void DiveKickEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsDiveKicking = false;
        data.DiveKickTimer = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        // Create impact if hit ground
        if (player.OnGround())
        {
            if (player.Scene is Level level)
            {
                level.Particles.Emit(ParticleTypes.SparkyDust, 12, player.BottomCenter, Vector2.One * 24f);
                level.Shake(0.3f);
                Audio.Play("event:/desolozantas/char/kirby/kirby_knight/punch_Final", player.Position);
            }
            
            // Damage enemies on ground
            DamageNearbyEnemies(player, 40f, 2);
        }
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
    }

    #endregion

    #region Skill Update Methods

    private static int AirDriftUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.AirDriftTimer -= Engine.DeltaTime;
        
        if (data.AirDriftTimer <= 0f || player.OnGround())
        {
            AirDriftEnd(player);
            return Player.StNormal;
        }
        
        // Enhanced air control
        int moveX = Input.MoveX.Value;
        player.Speed.X = Calc.Approach(
            player.Speed.X,
            AirDriftSpeed * moveX,
            400f * Engine.DeltaTime);
        
        // Slow fall
        player.Speed.Y = Calc.Approach(
            player.Speed.Y,
            20f,
            200f * Engine.DeltaTime);
        
        if (moveX != 0)
            player.Facing = (Facings) moveX;
        
        return data.StAirDrift;
    }

    private static int CycloneSlashUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.CycloneSlashTimer -= Engine.DeltaTime;
        
        if (data.CycloneSlashTimer <= 0f || player.OnGround())
        {
            CycloneSlashEnd(player);
            return Player.StNormal;
        }
        
        // Spinning rotation
        if (player.Sprite != null)
        {
            player.Sprite.Rotation += Engine.DeltaTime * 20f;
        }
        
        // Continual damage to nearby enemies
        if (Engine.Scene.OnInterval(0.1f))
        {
            DamageNearbyEnemies(player, CycloneSlashRadius, 1);
        }
        
        // Slight hover
        player.Speed.Y = Calc.Approach(player.Speed.Y, -20f, 300f * Engine.DeltaTime);
        
        return data.StCycloneSlash;
    }

    private static int StarShotUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        // Charge up
        data.StarShotCharge = Math.Min(StarShotChargeMax, data.StarShotCharge + Engine.DeltaTime);
        
        // Slow hover while charging
        player.Speed.Y = Calc.Approach(player.Speed.Y, -15f, 150f * Engine.DeltaTime);
        player.Speed.X *= 0.9f;
        
        // Release when grab released
        if (!Input.Grab.Check)
        {
            StarShotEnd(player);
            return Player.StNormal;
        }
        
        // Charging visual
        if (player.Sprite != null && Engine.Scene.OnInterval(0.1f))
        {
            float pulse = 1f + (data.StarShotCharge * 0.3f);
            player.Sprite.Scale = new Vector2(1.1f, 0.9f) * pulse;
        }
        
        if (player.Scene is Level level && Engine.Scene.OnInterval(0.15f))
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 2, player.Center, Vector2.One * 8f);
        }
        
        return data.StStarShot;
    }

    private static int SlideTackleUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.SlideTackleTimer -= Engine.DeltaTime;
        
        if (data.SlideTackleTimer <= 0f || !player.OnGround())
        {
            SlideTackleEnd(player);
            return Player.StNormal;
        }
        
        // Maintain slide speed
        player.Speed.X = Calc.Approach(
            player.Speed.X,
            SlideTackleSpeed * (int)player.Facing,
            100f * Engine.DeltaTime);
        
        // Can jump cancel
        if (Input.Jump.Pressed)
        {
            Input.Jump.ConsumeBuffer();
            SlideTackleEnd(player);
            player.Speed.Y = -200f;
            return Player.StNormal;
        }
        
        // Damage enemies in path
        if (Engine.Scene.OnInterval(0.05f))
        {
            DamageNearbyEnemies(player, 24f, 1);
        }
        
        return data.StSlideTackle;
    }

    private static int CounterStanceUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.CounterStanceTimer -= Engine.DeltaTime;
        data.ParryWindow -= Engine.DeltaTime;
        
        if (data.CounterStanceTimer <= 0f || !player.OnGround())
        {
            CounterStanceEnd(player);
            return Player.StNormal;
        }
        
        // Parry window active
        if (data.ParryWindow > 0f)
        {
            // Check for enemy contact during parry window
            if (CheckParryContact(player))
            {
                // Successful parry!
                PerformParryCounter(player, data);
                return Player.StNormal;
            }
            
            // Visual indication of parry window
            if (player.Sprite != null && Engine.Scene.OnInterval(0.05f))
            {
                player.Sprite.Color = Color.White * 0.5f;
            }
        }
        else
        {
            // Normal counter stance
            if (player.Sprite != null)
            {
                player.Sprite.Color = Color.Gold * 0.7f;
            }
        }
        
        // Can cancel early with dash
        if (Input.Dash.Pressed)
        {
            CounterStanceEnd(player);
            return Player.StNormal;
        }
        
        return data.StCounterStance;
    }

    private static int DiveKickUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.DiveKickTimer -= Engine.DeltaTime;
        
        if (data.DiveKickTimer <= 0f || player.OnGround())
        {
            DiveKickEnd(player);
            return Player.StNormal;
        }
        
        // Maintain dive speed
        player.Speed.Y = DiveKickSpeed;
        
        // Slight horizontal control
        int moveX = Input.MoveX.Value;
        if (moveX != 0)
        {
            player.Speed.X = Calc.Approach(
                player.Speed.X,
                moveX * 80f,
                200f * Engine.DeltaTime);
        }
        
        // Damage enemies while diving
        DamageNearbyEnemies(player, 28f, 1);
        
        // Trail effect
        if (player.Scene is Level level && Engine.Scene.OnInterval(0.05f))
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 2, player.Center, Vector2.One * 8f);
        }
        
        return data.StDiveKick;
    }

    #endregion

    #region Helper Methods

    private static bool IsKirbyMode(Player player)
    {
        if (player?.IsKirbyMode() != true)
            return false;

        var settings = MaggyHelperModule.Settings;
        return settings == null || settings.KirbyMaxFloatJumps > 0;
    }

    private static void RegenerateStamina(Player player, KirbyPlayerData data)
    {
        float regenRate = player.OnGround() ? StaminaRegenRate : StaminaRegenAirRate;
        
        // Only regen if not using skills
        if (!data.IsAirDrifting && !data.IsCycloneSlashing && !data.IsStarShotCharging &&
            !data.IsSlideTackling && !data.IsCounterStancing && !data.IsDiveKicking)
        {
            data.Stamina = Math.Min(data.MaxStamina, data.Stamina + regenRate * Engine.DeltaTime);
        }
    }

    private static void UpdateBossFightMode(Player player, KirbyPlayerData data)
    {
        if (player.Scene is Level level)
        {
            bool nearBoss = false;
            foreach (var entity in level.Entities)
            {
                if (entity.GetType().Name.Contains("Boss") && 
                    Vector2.Distance(player.Center, entity.Center) < 200f)
                {
                    nearBoss = true;
                    break;
                }
            }
            
            data.IsBossFightMode = nearBoss;
        }
    }

    private static void DamageNearbyEnemies(Player player, float radius, int damage)
    {
        if (player.Scene is Level level)
        {
            foreach (var entity in level.Tracker.GetEntities<Entity>())
            {
                if (Vector2.Distance(player.Center, entity.Center) < radius)
                {
                    // Check if entity can be damaged
                    var hurtMethod = entity.GetType().GetMethod("Hurt");
                    if (hurtMethod != null)
                    {
                        try
                        {
                            float finalDamage = damage * (PlayerData.GetOrCreateValue(player).IsBossFightMode ? BossFightDamageBoost : 1f);
                            hurtMethod.Invoke(entity, new object[] { (int)finalDamage });
                            
                            level.Particles.Emit(ParticleTypes.SparkyDust, 4, entity.Center, Vector2.One * 12f);
                        }
                        catch { }
                    }
                }
            }
        }
    }

    private static void FireStarShot(Player player, float charge)
    {
        if (player.Scene is Level level)
        {
            Vector2 direction = new Vector2(Input.MoveX.Value, Input.MoveY.Value).SafeNormalize();
            if (direction == Vector2.Zero) 
                direction = Vector2.UnitX * (int)player.Facing;
            
            // Create star projectile effect
            int particleCount = (int)(8 * charge);
            level.Particles.Emit(ParticleTypes.SparkyDust, particleCount, player.Center, Vector2.One * 15f);
            
            // Flash effect
            level.Flash(Color.Yellow * 0.3f * charge, true);
            
            // Shoot in direction
            Vector2 shotPos = player.Center + direction * 20f;
            for (int i = 0; i < 5; i++)
            {
                Vector2 trailPos = shotPos + direction * (i * 10f);
                level.Particles.Emit(ParticleTypes.SparkyDust, 2, trailPos, Vector2.One * 8f);
            }
            
            Audio.Play("event:/desolozantas/char/kirby/spit", player.Position);
            
            // Apply knockback to player
            player.Speed = -direction * 80f * charge;
        }
    }

    private static bool CheckParryContact(Player player)
    {
        if (player.Scene is Level level)
        {
            foreach (var entity in level.Tracker.GetEntities<Entity>())
            {
                // Check for enemy projectiles or nearby enemies
                if (Vector2.Distance(player.Center, entity.Center) < 40f)
                {
                    if (entity.GetType().Name.Contains("Enemy") || 
                        entity.GetType().Name.Contains("Projectile"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static void PerformParryCounter(Player player, KirbyPlayerData data)
    {
        if (player.Scene is Level level)
        {
            // Successful parry rewards
            data.InvincibilityTimer = InvincibilityDuration;
            data.Stamina = Math.Min(data.MaxStamina, data.Stamina + 20f);
            data.ComboCount++;
            
            // Free dash in facing direction
            player.Speed.X = 300f * (int)player.Facing;
            player.Speed.Y = -100f;
            
            // Visual feedback
            level.Particles.Emit(ParticleTypes.SparkyDust, 16, player.Center, Vector2.One * 24f);
            level.Flash(Color.Gold * 0.4f, true);
            level.Shake(0.3f);
            
            Audio.Play("event:/desolozantas/char/kirby/transform_in", player.Position);
            
            CounterStanceEnd(player);
        }
    }

    private static void BreakCrumbleBlocksInPath(Player player)
    {
        // This would check for and break crumble blocks in the slide path
        // Implementation depends on the crumble block type used in the mod
    }

    #endregion

    #region Aqua Grapple Skill

    private static bool ShouldStartAquaGrapple(Player player, KirbyPlayerData data)
    {
        if (!IsKirbyMode(player) || data.StAquaGrapple < 0)
            return false;
        if (!data.HasAquaGrappleUnlocked)
            return false;
        if (player.OnGround() || data.Stamina < AquaGrappleStaminaCost)
            return false;
        // Aqua Grapple: Grab + Up while in air (or just Grab in air if no direction)
        return Input.Grab.Pressed && !player.OnGround();
    }

    private static void AquaGrappleBegin(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsAquaGrappling = true;
        data.AquaGrappleTimer = AquaGrappleMaxDuration;
        data.Stamina -= AquaGrappleStaminaCost;
        data.IsGrappleRetracting = false;
        
        // Find grapple target
        Vector2 aimDirection = new Vector2(Input.MoveX.Value, Math.Min(Input.MoveY.Value, 0)).SafeNormalize();
        if (aimDirection == Vector2.Zero)
            aimDirection = new Vector2((int)player.Facing, -0.5f).SafeNormalize();
        
        // Raycast to find anchor point
        data.GrappleTarget = FindGrappleAnchor(player, aimDirection);
        data.GrappleLength = Vector2.Distance(player.Center, data.GrappleTarget);
        
        if (data.GrappleLength > AquaGrappleMaxRange)
        {
            data.GrappleLength = AquaGrappleMaxRange;
            data.GrappleTarget = player.Center + aimDirection * AquaGrappleMaxRange;
        }
        
        // Initial swing angle
        Vector2 toTarget = data.GrappleTarget - player.Center;
        data.GrappleSwingAngle = (float)Math.Atan2(toTarget.Y, toTarget.X);
        
        // Visuals
        if (player.Sprite != null)
        {
            if (player.Sprite.Has("dash"))
                player.Sprite.Play("dash");
            else if (player.Sprite.Has("hover"))
                player.Sprite.Play("hover");
            
            player.Sprite.Scale = new Vector2(0.9f, 1.2f);
            player.Sprite.Color = Color.Cyan * 0.9f;
        }
        
        if (player.Scene is Level level)
        {
            // Create water stream effect from player to target
            CreateWaterStream(level, player.Center, data.GrappleTarget);
            
            level.Particles.Emit(ParticleTypes.SparkyDust, 10, player.Center, Vector2.One * 16f);
            level.Particles.Emit(ParticleTypes.SparkyDust, 8, data.GrappleTarget, Vector2.One * 12f);
            level.Shake(0.1f);
            
            Audio.Play("event:/desolozantas/char/kirby/kirby_knight/punch_A", player.Position);
        }
    }

    private static int AquaGrappleUpdate(Player player)
    {
        if (!IsKirbyMode(player))
            return Player.StNormal;
            
        var data = PlayerData.GetOrCreateValue(player);
        
        data.AquaGrappleTimer -= Engine.DeltaTime;
        
        // Release grapple if timer expires, on ground, or jump pressed
        if (data.AquaGrappleTimer <= 0f || player.OnGround() || Input.Jump.Pressed)
        {
            if (Input.Jump.Pressed)
            {
                Input.Jump.ConsumeBuffer();
                // Boost jump when releasing grapple
                player.Speed.Y = -250f;
            }
            AquaGrappleEnd(player);
            return Player.StNormal;
        }
        
        // Swing physics
        Vector2 toTarget = data.GrappleTarget - player.Center;
        float currentAngle = (float)Math.Atan2(toTarget.Y, toTarget.X);
        float currentDistance = toTarget.Length();
        
        // Swing mechanics - player can swing around the anchor point
        int moveX = Input.MoveX.Value;
        if (moveX != 0)
        {
            // Change swing angle based on input
            data.GrappleSwingAngle += moveX * 2f * Engine.DeltaTime;
        }
        
        // Apply gravity to swing
        float swingGravity = 300f * Engine.DeltaTime;
        float angleVelocity = swingGravity * (float)Math.Sin(data.GrappleSwingAngle);
        data.GrappleSwingAngle += angleVelocity * Engine.DeltaTime;
        
        // Calculate target position on the swing arc
        Vector2 targetPos = data.GrappleTarget - new Vector2(
            (float)Math.Cos(data.GrappleSwingAngle) * data.GrappleLength,
            (float)Math.Sin(data.GrappleSwingAngle) * data.GrappleLength
        );
        
        // Move player towards target position with momentum
        Vector2 moveDir = targetPos - player.Center;
        player.Speed = moveDir * 5f;
        
        // Allow retraction/extension with Up/Down
        if (Input.MoveY.Value < 0) // Up - retract
        {
            data.GrappleLength = Math.Max(30f, data.GrappleLength - AquaGrappleRetractSpeed * Engine.DeltaTime);
        }
        else if (Input.MoveY.Value > 0) // Down - extend
        {
            data.GrappleLength = Math.Min(AquaGrappleMaxRange, data.GrappleLength + AquaGrappleRetractSpeed * Engine.DeltaTime * 0.5f);
        }
        
        // Visual updates
        if (player.Scene is Level level && Engine.Scene.OnInterval(0.05f))
        {
            // Water droplets along the rope
            Vector2 ropeDir = (data.GrappleTarget - player.Center).SafeNormalize();
            for (int i = 1; i < 5; i++)
            {
                Vector2 dropPos = player.Center + ropeDir * (data.GrappleLength * i / 5f);
                level.Particles.Emit(ParticleTypes.SparkyDust, 1, dropPos, Vector2.One * 4f);
            }
        }
        
        // Can release early with Grab
        if (Input.Grab.Pressed)
        {
            AquaGrappleEnd(player);
            return Player.StNormal;
        }
        
        return data.StAquaGrapple;
    }

    private static void AquaGrappleEnd(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.IsAquaGrappling = false;
        data.AquaGrappleTimer = 0f;
        data.ComboCount++;
        data.LastActionTime = Engine.Scene.RawTimeActive;
        
        // Preserve some momentum from swing
        player.Speed *= 0.8f;
        
        if (player.Sprite != null)
        {
            player.Sprite.Scale = Vector2.One;
            player.Sprite.Color = Color.White;
        }
        
        if (player.Scene is Level level)
        {
            // Water splash effect at grapple point
            level.Particles.Emit(ParticleTypes.SparkyDust, 12, player.Center, Vector2.One * 20f);
            Audio.Play("event:/desolozantas/char/kirby/spit", player.Position);
        }
    }

    private static Vector2 FindGrappleAnchor(Player player, Vector2 direction)
    {
        if (player.Scene is Level level)
        {
            Vector2 start = player.Center;
            Vector2 end = start + direction * AquaGrappleMaxRange;
            
            // Simple linecast to find solid terrain
            for (float t = 0; t <= 1f; t += 0.05f)
            {
                Vector2 checkPos = Vector2.Lerp(start, end, t);
                
                // Check for solid tiles
                if (level.CollideCheck<Solid>(checkPos))
                {
                    return checkPos;
                }
                
                // Check for specific grapple points (if any exist in the level)
                foreach (var entity in level.Tracker.GetEntities<Entity>())
                {
                    if (entity.GetType().Name.Contains("Grapple") || 
                        entity.GetType().Name.Contains("Hook"))
                    {
                        if (Vector2.Distance(checkPos, entity.Center) < 16f)
                        {
                            return entity.Center;
                        }
                    }
                }
            }
            
            // Return max range point if nothing hit
            return end;
        }
        
        return player.Center + direction * AquaGrappleMaxRange;
    }

    private static void CreateWaterStream(Level level, Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).SafeNormalize();
        float distance = Vector2.Distance(from, to);
        int segments = (int)(distance / 10f);
        
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector2 pos = Vector2.Lerp(from, to, t);
            level.Particles.Emit(ParticleTypes.SparkyDust, 3, pos, Vector2.One * 8f, Color.Cyan);
        }
    }

    /// <summary>
    /// Unlock the Aqua Grapple ability for a player (call this when player unlocks the skill)
    /// </summary>
    public static void UnlockAquaGrapple(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.HasAquaGrappleUnlocked = true;
        Logger.Log(LogLevel.Info, "MaggyHelper", "[KirbyPlayer] Aqua Grapple unlocked!");
        
        if (player.Scene is Level level)
        {
            level.Particles.Emit(ParticleTypes.SparkyDust, 20, player.Center, Vector2.One * 24f);
            level.Flash(Color.Cyan * 0.3f, true);
            Audio.Play("event:/desolozantas/char/kirby/transform_in", player.Position);
        }
    }

    /// <summary>
    /// Check if player has unlocked Aqua Grapple
    /// </summary>
    public static bool HasAquaGrappleUnlocked(Player player)
    {
        return PlayerData.GetOrCreateValue(player).HasAquaGrappleUnlocked;
    }

    #endregion
}
