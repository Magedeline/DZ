using System;
using System.Reflection;
using Celeste.Mod.MaggyHelper;
using MaggyHelper.Extensions;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace MaggyHelper;

/// <summary>
/// Hooks into Celeste's camera, trigger, and collision systems to redirect
/// them towards <see cref="MaggyHelper.Entities.Player"/> when the vanilla
/// <c>Celeste.Player</c> is hidden by <see cref="Entities.KirbyPlayerSpawner"/>.
///
/// Hooks registered here:
///   • <b>Camera tracking</b> — <c>Level.Update</c> override to centre the
///     camera on our Player every frame.
///   • <b>Trigger compat</b> — <c>Trigger.OnEnter / OnStay / OnLeave</c> to
///     fire when our Player overlaps instead of only for <c>Celeste.Player</c>.
///   • <b>PlayerCollider compat</b> — patches <c>PlayerCollider.Check</c> to
///     also collide with our entity.
/// </summary>
public static class PlayerCompatShim
{
    // Hook references — stored to prevent GC
    private static Hook playerColliderCheckHook;

    // ─────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────

    public static void Load()
    {
        // 1. Camera tracking
        On.Celeste.Level.Update += Hook_Level_Update;

        // 2. Trigger compat — we poll overlaps in Level.Update for our Player,
        //    so we don't need to individually hook every trigger type.

        // 3. PlayerCollider.Check hook — so entities like spikes, springs, etc.
        //    detect our Player when the vanilla player is hidden.
        try
        {
            MethodInfo checkMethod = typeof(PlayerCollider).GetMethod(
                "Check",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (checkMethod != null)
            {
                playerColliderCheckHook = new Hook(
                    checkMethod,
                    typeof(PlayerCompatShim).GetMethod(
                        nameof(Hook_PlayerCollider_Check),
                        BindingFlags.Static | BindingFlags.NonPublic));

                Logger.Log(LogLevel.Info, "MaggyHelper",
                    "[PlayerCompatShim] PlayerCollider.Check hook registered");
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[PlayerCompatShim] Failed to hook PlayerCollider.Check: {ex.Message}");
        }

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[PlayerCompatShim] All compatibility hooks loaded");
    }

    public static void Unload()
    {
        On.Celeste.Level.Update -= Hook_Level_Update;

        playerColliderCheckHook?.Dispose();
        playerColliderCheckHook = null;

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[PlayerCompatShim] All compatibility hooks unloaded");
    }

    // ─────────────────────────────────────────────────
    //  1. CAMERA TRACKING
    // ─────────────────────────────────────────────────

    private static void Hook_Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        // Only intervene when our Player is present and vanilla player is hidden
        if (!IsMaggyPlayerActive(self))
            return;

        var maggyPlayer = self.Tracker.GetEntity<MaggyHelper.Entities.Player>();
        if (maggyPlayer == null)
            return;

        // Centre camera on our player, matching Celeste's default behaviour
        Vector2 cameraTarget = maggyPlayer.Position
            - new Vector2(320f / 2f, 180f / 2f)  // Screen centre offset
            + new Vector2(0f, -8f);               // Slight upward bias (like vanilla)

        // Clamp to level bounds
        var bounds = self.Bounds;
        cameraTarget.X = Math.Max(bounds.Left, Math.Min(cameraTarget.X, bounds.Right - 320f));
        cameraTarget.Y = Math.Max(bounds.Top, Math.Min(cameraTarget.Y, bounds.Bottom - 180f));

        // Smooth camera follow
        Vector2 currentCam = self.Camera.Position;
        self.Camera.Position = currentCam + (cameraTarget - currentCam) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));

        // Update trigger overlaps for our player
        UpdateTriggerOverlaps(self, maggyPlayer);
    }

    // ─────────────────────────────────────────────────
    //  2. TRIGGER COMPAT
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Walks through all tracked triggers in the level and fires
    /// OnEnter / OnStay / OnLeave for our MaggyHelper Player.
    /// </summary>
    private static readonly HashSet<Trigger> triggersOurPlayerIsInside = new();

    private static void UpdateTriggerOverlaps(Level level, MaggyHelper.Entities.Player maggyPlayer)
    {
        var triggers = level.Tracker.GetEntities<Trigger>();
        if (triggers == null)
            return;

        // We need a dummy Celeste.Player reference for trigger callbacks.
        // Use the hidden vanilla player (it still exists in the scene).
        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        if (vanillaPlayer == null)
            return;

        HashSet<Trigger> currentOverlaps = new();

        foreach (Entity entity in triggers)
        {
            if (entity is not Trigger trigger || !trigger.Collidable)
                continue;

            bool overlapping = maggyPlayer.CollideCheck(trigger);

            if (overlapping)
            {
                currentOverlaps.Add(trigger);

                if (!triggersOurPlayerIsInside.Contains(trigger))
                {
                    // Just entered
                    try
                    {
                        trigger.OnEnter(vanillaPlayer);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper",
                            $"[PlayerCompatShim] Trigger.OnEnter error: {ex.Message}");
                    }
                }
                else
                {
                    // Staying inside
                    try
                    {
                        trigger.OnStay(vanillaPlayer);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper",
                            $"[PlayerCompatShim] Trigger.OnStay error: {ex.Message}");
                    }
                }
            }
        }

        // Check for triggers we left
        foreach (var trigger in triggersOurPlayerIsInside)
        {
            if (!currentOverlaps.Contains(trigger) && trigger.Scene != null)
            {
                try
                {
                    trigger.OnLeave(vanillaPlayer);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        $"[PlayerCompatShim] Trigger.OnLeave error: {ex.Message}");
                }
            }
        }

        triggersOurPlayerIsInside.Clear();
        foreach (var t in currentOverlaps)
            triggersOurPlayerIsInside.Add(t);
    }

    // ─────────────────────────────────────────────────
    //  3. PLAYER-COLLIDER COMPAT
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Delegate matching <c>PlayerCollider.Check(Entity)</c> signature.
    /// </summary>
    private delegate bool orig_PlayerCollider_Check(PlayerCollider self, Entity entity);

    /// <summary>
    /// Intercepts <c>PlayerCollider.Check</c> to also recognise our
    /// <see cref="MaggyHelper.Entities.Player"/> as a valid collision target.
    /// </summary>
    private static bool Hook_PlayerCollider_Check(
        orig_PlayerCollider_Check orig,
        PlayerCollider self,
        Entity entity)
    {
        // Call original first — handles vanilla Celeste.Player
        if (orig(self, entity))
            return true;

        // If the entity being checked IS our MaggyHelper player, run the
        // collision check manually.
        if (entity is MaggyHelper.Entities.Player maggyPlayer && maggyPlayer.Collidable)
        {
            // Get the parent entity that owns this PlayerCollider component
            Entity owner = self.Entity;
            if (owner == null || !owner.Collidable)
                return false;

            // Use the PlayerCollider's own collider if it has one
            Collider originalCollider = null;
            try
            {
                // PlayerCollider stores its hitbox in a private field
                var data = new DynamicData(self);
                var pcCollider = data.Get<Collider>("collider");

                if (pcCollider != null)
                {
                    originalCollider = owner.Collider;
                    owner.Collider = pcCollider;
                }

                bool result = maggyPlayer.CollideCheck(owner);

                if (result)
                {
                    // Fire the PlayerCollider callback with the hidden vanilla player
                    // because the callback signature expects Celeste.Player
                    var callback = data.Get<Action<CelestePlayer>>("onCollide");
                    var level = owner.Scene as Level;
                    var vanillaPlayer = level?.Tracker.GetEntity<CelestePlayer>();

                    if (callback != null && vanillaPlayer != null)
                    {
                        // Temporarily move the vanilla player to our position
                        // so collision response (knockback etc.) uses correct coords
                        Vector2 savedPos = vanillaPlayer.Position;
                        vanillaPlayer.Position = maggyPlayer.Position;
                        callback(vanillaPlayer);
                        vanillaPlayer.Position = savedPos;
                    }
                }

                return result;
            }
            finally
            {
                if (originalCollider != null)
                    owner.Collider = originalCollider;
            }
        }

        return false;
    }

    // ─────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Returns true when our MaggyHelper Player is in the scene and the
    /// vanilla player is hidden (i.e. <see cref="Entities.KirbyPlayerSpawner"/>
    /// is active).
    /// </summary>
    public static bool IsMaggyPlayerActive(Level level)
    {
        if (level == null)
            return false;

        var vanillaPlayer = level.Tracker.GetEntity<CelestePlayer>();
        var maggyPlayer = level.Tracker.GetEntity<MaggyHelper.Entities.Player>();

        // Our player is active when it exists AND vanilla player is disabled
        return maggyPlayer != null
            && vanillaPlayer != null
            && !vanillaPlayer.Active;
    }

    /// <summary>
    /// Gets our <see cref="MaggyHelper.Entities.Player"/> if it's active,
    /// or null if vanilla controls are in use.
    /// </summary>
    public static MaggyHelper.Entities.Player GetActivePlayer(Level level)
    {
        if (level == null)
            return null;

        return IsMaggyPlayerActive(level)
            ? level.Tracker.GetEntity<MaggyHelper.Entities.Player>()
            : null;
    }
}