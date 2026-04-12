using Celeste;
using Celeste.Mod.MaggyHelper;
using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace MaggyHelper;

/// <summary>
/// Registers a minimal Kirby-specific state on the vanilla Celeste.Player.
/// This follows the common Everest pattern of extending the real player via
/// custom states instead of swapping the player object at runtime.
/// </summary>
public static class KirbyPlayerStateController
{
    private const string FloatTimerKey = "MaggyHelper.KirbyFloatTimer";

    private const float KirbyFloatSpeed = -40f;
    private const float KirbyFloatMaxTime = 3f;
    private const float KirbyFloatGravity = 100f;
    private const float KirbyFloatTargetFallSpeed = 20f;
    private const float KirbyFloatHSpeed = 70f;
    private const float KirbyFloatAccel = 600f;

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

        // Mirror the legacy Kirby player behavior closely: the float kicks in
        // after a regular jump attempt fails, not while the player is still rising.
        if (player.Speed.Y < 0f)
            return false;

        return GetFloatTimer(player) > 0f;
    }

    private static void KirbyFloatBegin(Player player)
    {
        if (GetFloatTimer(player) <= 0f)
            SetFloatTimer(player, KirbyFloatMaxTime);

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
            player.Speed.Y = KirbyFloatSpeed;
            SetFloatTimer(player, Math.Max(0f, remaining - 0.15f));

            if (player.Scene is Level level)
                level.Particles.Emit(ParticleTypes.Dust, 2, player.BottomCenter, Vector2.UnitX * 4f, Calc.Down);

            if (player.Sprite != null)
                player.Sprite.Scale = new Vector2(1.3f, 0.7f);
        }

        if (moveX != 0)
            player.Facing = (Facings) moveX;

        player.MoveH(player.Speed.X * Engine.DeltaTime);
        player.MoveV(player.Speed.Y * Engine.DeltaTime);

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
        DynamicData data = DynamicData.For(player);
        return data.TryGet<float>(FloatTimerKey, out float value)
            ? value
            : KirbyFloatMaxTime;
    }

    private static void SetFloatTimer(Player player, float value)
    {
        DynamicData.For(player).Set(FloatTimerKey, value);
    }
}