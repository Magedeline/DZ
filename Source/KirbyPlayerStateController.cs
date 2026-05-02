using System.Runtime.CompilerServices;
using Celeste;
using global::Celeste.Mod.MaggyHelper;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste;

/// <summary>
/// Registers a minimal Kirby-specific state on the vanilla global::Celeste.Player.
/// This follows the common Everest pattern of extending the real player via
/// custom states instead of swapping the player object at runtime.
/// </summary>
public static class KirbyPlayerStateController
{
    private class KirbyPlayerData
    {
        public float FloatTimer;
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

    public static int StKirbyFloat { get; private set; } = -1;

    public static void Load()
    {
        Everest.Events.Player.OnRegisterStates += RegisterStates;
        On.Celeste.Player.Update += Hook_Player_Update;
        On.Celeste.Player.NormalUpdate += Hook_Player_NormalUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Loaded");
    }

    public static void Unload()
    {
        On.Celeste.Player.NormalUpdate -= Hook_Player_NormalUpdate;
        On.Celeste.Player.Update -= Hook_Player_Update;
        Everest.Events.Player.OnRegisterStates -= RegisterStates;

        StKirbyFloat = -1;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] Unloaded");
    }

    private static void RegisterStates(Player player)
    {
        StKirbyFloat = player.AddState(
            "MaggyHelperKirbyFloat",
            KirbyFloatUpdate,
            null,
            KirbyFloatBegin,
            KirbyFloatEnd);
    }

    private static void Hook_Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.Scene == null)
            return;

        if (self.OnGround())
            SetFloatTimer(self, KirbyFloatMaxTime);
    }

    private static int Hook_Player_NormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        int nextState = orig(self);

        if (nextState != Player.StNormal)
            return nextState;

        if (ShouldStartKirbyFloat(self))
        {
            Input.Jump.ConsumeBuffer();
            return StKirbyFloat;
        }

        return nextState;
    }

    private static bool ShouldStartKirbyFloat(Player player)
    {
        if (!IsKirbyFloatEnabled(player) || StKirbyFloat < 0)
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

    private static int KirbyFloatUpdate(Player player)
    {
        if (!IsKirbyFloatEnabled(player))
            return Player.StNormal;

        if (Input.Dash.Pressed || Input.Grab.Pressed)
            return Player.StNormal;

        if (Input.MoveY.Value > 0)
        {
            player.Speed.Y = KirbyFloatFastFall;
            return Player.StNormal;
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
            return Player.StNormal;

        return GetFloatTimer(player) <= 0f
            ? Player.StNormal
            : StKirbyFloat;
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