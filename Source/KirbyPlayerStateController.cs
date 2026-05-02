using System.Runtime.CompilerServices;
using Celeste;
using global::Celeste.Mod.MaggyHelper;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste;

public static class KirbyPlayerStateController
{
    private class KirbyPlayerData
    {
        public float FloatTimer;
        public float ActionTimer;
        public bool GroundPoundImpacted;
        public KirbyAction CurrentAction;
    }

    private enum KirbyAction
    {
        None,
        Float,
        Punch,
        Kick,
        AirSpin,
        Backflip,
        GroundPound
    }

    private static readonly ConditionalWeakTable<Player, KirbyPlayerData> PlayerData = new();

    private const float KirbyFloatSpeed = -80f;
    private const float KirbyFloatMaxTime = 3f;
    private const float KirbyFloatGravity = 150f;
    private const float KirbyFloatTargetFallSpeed = 30f;
    private const float KirbyFloatHSpeed = 70f;
    private const float KirbyFloatAccel = 600f;
    private const float KirbyFloatJumpBurst = -120f;
    private const float KirbyFloatFastFall = 200f;
    private const float KirbyPunchTime = 0.16f;
    private const float KirbyKickTime = 0.20f;
    private const float KirbyKickSpeed = 130f;
    private const float KirbyAirSpinTime = 0.34f;
    private const float KirbyAirSpinSpeed = 90f;
    private const float KirbyBackflipTime = 0.28f;
    private const float KirbyBackflipXSpeed = 100f;
    private const float KirbyBackflipYSpeed = -150f;
    private const float KirbyGroundPoundWindupTime = 0.08f;
    private const float KirbyGroundPoundTime = 0.40f;
    private const float KirbyGroundPoundSpeed = 340f;
    private const float KirbyGroundPoundBounceSpeed = -150f;

    public static void Load()
    {
        On.Celeste.Player.Update += Hook_Player_Update;
        On.Celeste.Player.NormalUpdate += Hook_Player_NormalUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Loaded");
    }

    public static void Unload()
    {
        On.Celeste.Player.NormalUpdate -= Hook_Player_NormalUpdate;
        On.Celeste.Player.Update -= Hook_Player_Update;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Unloaded");
    }

    private static void Hook_Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        UpdateKirbyAction(self);
        orig(self);

        if (self.Scene == null)
            return;

        if (self.OnGround())
            SetFloatTimer(self, KirbyFloatMaxTime);
    }

    private static int Hook_Player_NormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        int nextState = orig(self);
        return nextState;
    }

    private static bool ShouldStartKirbyFloat(Player player)
    {
        if (!IsKirbyFloatEnabled(player))
            return false;

        if (player.Scene is not Level)
            return false;

        if (player.OnGround())
            return false;

        if (!Input.Jump.Pressed)
            return false;

        // Allow floating to start even while rising. Kirby can start floating at any time in the air.
        // Removed: if (player.Speed.Y < 0f) return false;

        return GetFloatTimer(player) > 0f;
    }

    private static KirbyAction GetRequestedKirbyAction(Player player)
    {
        if (player?.IsKirbyMode() != true || player.Scene is not Level)
            return KirbyAction.None;

        if (Input.Dash.Pressed && Input.MoveY.Value > 0 && !player.OnGround())
        {
            Input.Dash.ConsumeBuffer();
            return KirbyAction.GroundPound;
        }

        if (!Input.Grab.Pressed)
            return KirbyAction.None;

        if (Input.MoveY.Value < 0)
            return KirbyAction.Backflip;

        if (!player.OnGround())
            return KirbyAction.AirSpin;

        if (Input.MoveX.Value != 0)
            return KirbyAction.Kick;

        return KirbyAction.Punch;
    }

    private static void KirbyFloatBegin(Player player)
    {
        if (GetFloatTimer(player) <= 0f)
            SetFloatTimer(player, KirbyFloatMaxTime);

        if (player.Speed.Y > KirbyFloatSpeed)
            player.Speed.Y = KirbyFloatSpeed;

        if (player.Sprite != null)
        {
            if (player.Sprite.Has(PlayerSprite.FallSlow))
                player.Sprite.Play(PlayerSprite.FallSlow);
            else if (player.Sprite.Has(PlayerSprite.Fall))
                player.Sprite.Play(PlayerSprite.Fall);

            player.Sprite.Scale = new Vector2(1.2f, 0.8f);
        }
    }

    private static void KirbyFloatEnd(Player player)
    {
        if (player.Sprite != null)
            player.Sprite.Scale = Vector2.One;

        if (player.Scene is Level level && !player.OnGround())
            level.Particles.Emit(ParticleTypes.Dust, 3, player.BottomCenter, Vector2.One * 4f, Calc.Down);
    }

    private static bool KirbyFloatUpdate(Player player)
    {
        if (!IsKirbyFloatEnabled(player))
            return false;

        KirbyAction kirbyAction = GetRequestedKirbyAction(player);
        if (kirbyAction != KirbyAction.None)
        {
            StartAction(player, kirbyAction);
            return true;
        }

        if (Input.Dash.Pressed || Input.Grab.Pressed)
            return false;

        if (Input.MoveY.Value > 0)
        {
            player.Speed.Y = KirbyFloatFastFall;
            return false;
        }

        float remaining = Math.Max(0f, GetFloatTimer(player) - Engine.DeltaTime);
        SetFloatTimer(player, remaining);

        int moveX = Input.MoveX.Value;
        player.Speed.X = Calc.Approach(
            player.Speed.X,
            KirbyFloatHSpeed * moveX,
            KirbyFloatAccel * Engine.DeltaTime);
        player.Speed.Y = Calc.Approach(
            player.Speed.Y,
            KirbyFloatTargetFallSpeed,
            KirbyFloatGravity * Engine.DeltaTime);

        if (Input.Jump.Pressed)
        {
            Input.Jump.ConsumeBuffer();
            player.Speed.Y = KirbyFloatJumpBurst;
            SetFloatTimer(player, Math.Max(0f, remaining - 0.15f));

            if (player.Scene is Level level)
                level.Particles.Emit(ParticleTypes.Dust, 2, player.BottomCenter, Vector2.UnitX * 4f, Calc.Down);

            if (player.Sprite != null)
                player.Sprite.Scale = new Vector2(1.3f, 0.7f);
        }

        if (moveX != 0)
            player.Facing = (Facings) moveX;

        if (player.OnGround() && player.Speed.Y >= 0f)
            return false;

        return GetFloatTimer(player) > 0f;
    }

    private static void KirbyPunchBegin(Player player)
    {
        BeginKirbyAction(player, KirbyPunchTime, "kirby_punchA", "punchA", "attack");
        player.Speed.X = 30f * (int) player.Facing;
    }

    private static bool KirbyPunchUpdate(Player player)
    {
        UpdateActionTimer(player);
        player.Speed.X = Calc.Approach(player.Speed.X, 0f, 800f * Engine.DeltaTime);
        EmitMeleeParticles(player, 10f);
        return GetActionTimer(player) > 0f;
    }

    private static void KirbyKickBegin(Player player)
    {
        if (Input.MoveX.Value != 0)
            player.Facing = (Facings) Input.MoveX.Value;

        BeginKirbyAction(player, KirbyKickTime, "combat_slide", "kirby_punchB", "punchB", "attack");
        player.Speed.X = KirbyKickSpeed * (int) player.Facing;
    }

    private static bool KirbyKickUpdate(Player player)
    {
        UpdateActionTimer(player);
        player.Speed.X = Calc.Approach(player.Speed.X, 0f, 600f * Engine.DeltaTime);
        EmitMeleeParticles(player, 14f);
        return GetActionTimer(player) > 0f;
    }

    private static void KirbyAirSpinBegin(Player player)
    {
        BeginKirbyAction(player, KirbyAirSpinTime, "kirby_spin", "spin", "spins");
        player.Speed.Y = Math.Min(player.Speed.Y, KirbyFloatTargetFallSpeed);
    }

    private static bool KirbyAirSpinUpdate(Player player)
    {
        UpdateActionTimer(player);

        int moveX = Input.MoveX.Value;
        if (moveX != 0)
            player.Facing = (Facings) moveX;

        player.Speed.X = Calc.Approach(player.Speed.X, KirbyAirSpinSpeed * moveX, KirbyFloatAccel * Engine.DeltaTime);
        player.Speed.Y = Calc.Approach(player.Speed.Y, KirbyFloatTargetFallSpeed, KirbyFloatGravity * Engine.DeltaTime);
        EmitMeleeParticles(player, 12f);

        if (player.OnGround() && player.Speed.Y >= 0f)
            return false;

        return GetActionTimer(player) > 0f;
    }

    private static void KirbyBackflipBegin(Player player)
    {
        BeginKirbyAction(player, KirbyBackflipTime, "kirby_backflip", "backflip", "jumpSlow");
        player.Speed.X = -KirbyBackflipXSpeed * (int) player.Facing;
        player.Speed.Y = KirbyBackflipYSpeed;
    }

    private static bool KirbyBackflipUpdate(Player player)
    {
        UpdateActionTimer(player);
        player.Speed.Y = Calc.Approach(player.Speed.Y, KirbyFloatTargetFallSpeed, KirbyFloatGravity * Engine.DeltaTime);
        return GetActionTimer(player) > 0f;
    }

    private static void KirbyGroundPoundBegin(Player player)
    {
        BeginKirbyAction(player, KirbyGroundPoundTime, "kirby_groundpound", "groundpound", "fallFast");
        var data = PlayerData.GetOrCreateValue(player);
        data.GroundPoundImpacted = false;
        player.Speed = Vector2.Zero;
    }

    private static bool KirbyGroundPoundUpdate(Player player)
    {
        UpdateActionTimer(player);
        var data = PlayerData.GetOrCreateValue(player);
        float elapsed = KirbyGroundPoundTime - data.ActionTimer;

        if (elapsed < KirbyGroundPoundWindupTime)
        {
            player.Speed = Vector2.Zero;
            return true;
        }

        if (!data.GroundPoundImpacted)
        {
            player.Speed.X = 0f;
            player.Speed.Y = KirbyGroundPoundSpeed;
        }

        if (player.OnGround() && !data.GroundPoundImpacted)
        {
            data.GroundPoundImpacted = true;
            player.Speed.Y = KirbyGroundPoundBounceSpeed;
            DoGroundPoundImpact(player);
        }

        return !data.GroundPoundImpacted && data.ActionTimer > 0f;
    }

    private static void KirbyActionEnd(Player player)
    {
        if (player.Sprite != null)
            player.Sprite.Scale = Vector2.One;
    }

    private static void UpdateKirbyAction(Player player)
    {
        if (player == null)
            return;

        var data = PlayerData.GetOrCreateValue(player);
        if (data.CurrentAction == KirbyAction.None)
        {
            KirbyAction requestedAction = GetRequestedKirbyAction(player);
            if (requestedAction != KirbyAction.None)
                StartAction(player, requestedAction);
            else if (ShouldStartKirbyFloat(player))
            {
                Input.Jump.ConsumeBuffer();
                StartAction(player, KirbyAction.Float);
            }
        }

        if (data.CurrentAction == KirbyAction.None)
            return;

        bool keepAction = data.CurrentAction switch
        {
            KirbyAction.Float => KirbyFloatUpdate(player),
            KirbyAction.Punch => KirbyPunchUpdate(player),
            KirbyAction.Kick => KirbyKickUpdate(player),
            KirbyAction.AirSpin => KirbyAirSpinUpdate(player),
            KirbyAction.Backflip => KirbyBackflipUpdate(player),
            KirbyAction.GroundPound => KirbyGroundPoundUpdate(player),
            _ => false
        };

        if (!keepAction)
            EndAction(player);
    }

    private static void StartAction(Player player, KirbyAction action)
    {
        var data = PlayerData.GetOrCreateValue(player);
        if (data.CurrentAction != KirbyAction.None)
            EndAction(player);

        data.CurrentAction = action;
        switch (action)
        {
            case KirbyAction.Float:
                KirbyFloatBegin(player);
                break;
            case KirbyAction.Punch:
                KirbyPunchBegin(player);
                break;
            case KirbyAction.Kick:
                KirbyKickBegin(player);
                break;
            case KirbyAction.AirSpin:
                KirbyAirSpinBegin(player);
                break;
            case KirbyAction.Backflip:
                KirbyBackflipBegin(player);
                break;
            case KirbyAction.GroundPound:
                KirbyGroundPoundBegin(player);
                break;
        }
    }

    private static void EndAction(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        KirbyAction action = data.CurrentAction;
        data.CurrentAction = KirbyAction.None;

        if (action == KirbyAction.Float)
            KirbyFloatEnd(player);
        else
            KirbyActionEnd(player);
    }

    private static void BeginKirbyAction(Player player, float duration, params string[] animations)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.ActionTimer = duration;

        PlayFirstAvailable(player, animations);

        if (player.Sprite != null)
            player.Sprite.Scale = new Vector2(1.2f, 0.8f);

        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
    }

    private static void PlayFirstAvailable(Player player, params string[] animations)
    {
        if (player.Sprite == null)
            return;

        foreach (string animation in animations)
        {
            if (player.Sprite.Has(animation))
            {
                player.Sprite.Play(animation);
                return;
            }
        }
    }

    private static void UpdateActionTimer(Player player)
    {
        var data = PlayerData.GetOrCreateValue(player);
        data.ActionTimer = Math.Max(0f, data.ActionTimer - Engine.DeltaTime);
    }

    private static float GetActionTimer(Player player)
    {
        return PlayerData.TryGetValue(player, out var data) ? data.ActionTimer : 0f;
    }

    private static void EmitMeleeParticles(Player player, float reach)
    {
        if (player.Scene is not Level level || !level.OnInterval(0.04f))
            return;

        Vector2 origin = player.Center + Vector2.UnitX * (int) player.Facing * reach;
        level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 1, origin, Vector2.One * 3f);
    }

    private static void DoGroundPoundImpact(Player player)
    {
        if (player.Scene is not Level level)
            return;

        CelesteGame.Freeze(0.04f);
        level.DirectionalShake(Vector2.UnitY, 0.15f);
        level.Displacement.AddBurst(player.BottomCenter, 0.5f, 4f, 64f, 0.4f, Ease.QuadOut, Ease.QuadOut);
        level.Particles.Emit(ParticleTypes.Dust, 8, player.BottomCenter, Vector2.One * 8f, Calc.Up);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
    }

    private static bool IsKirbyFloatEnabled(Player player)
    {
        if (player?.IsKirbyMode() != true)
            return false;

        var settings = MaggyHelperModule.Settings;
        return settings == null || settings.KirbyMaxFloatJumps > 0;
    }

    private static float GetFloatTimer(Player player)
    {
        if (player == null)
            return KirbyFloatMaxTime;

        return PlayerData.TryGetValue(player, out var data) ? data.FloatTimer : KirbyFloatMaxTime;
    }

    private static void SetFloatTimer(Player player, float value)
    {
        if (player == null)
            return;

        PlayerData.GetOrCreateValue(player).FloatTimer = value;
    }
}