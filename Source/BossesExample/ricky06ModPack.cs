using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using global::Celeste.Mod.MaggyHelper.BossesExample.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.MaggyHelper.BossesExample;

public static class BossesExampleModule
{
    private static readonly HashSet<string> FallbackStoneFlags = new();
    private static readonly string[] PlaceholderAnimations =
    {
        "idle", "ghost", "appear", "disappear", "quick_appear", "spark1", "spark2",
        "screech", "screechfast", "deathBegin", "deathLoop", "deathEnd", "once",
        "aim", "strike", "flash", "xcharge", "ycharge", "xattack", "yattack",
        "yattackend", "chargeup", "chargestrike", "beamcharge", "beamstrike",
        "chargeBeam", "invisible", "charge", "lock", "shoot", "shockwave",
        "shake", "inside", "burst", "createBar", "createStrawberry", "destroyStrawberry"
    };

    private static Hook dashBeginHook;
    private static Hook dashEndHook;
    private static Coroutine destroyDashDelay;
    private static bool loaded;

    public static readonly BossesExampleSpriteBankProxy SpriteBank = new(false);
    public static readonly BossesExampleSpriteBankProxy GuiBank = new(true);
    public static readonly BossesExampleSettingsProxy Settings = new();
    public static readonly BossesExampleSaveDataProxy SaveData = new();

    public static bool hasDestroyDash;
    public static bool destroyDashActive;
    public static int playerHealth;
    public static DashWave dw;
    public static bool lrzTransition;
    public static bool bufferCooldown;
    public static Dictionary<Entity, EntityID> noResetDict { get; } = new();

    public static void LoadContent(bool firstLoad)
    {
        SpriteBank.SetBank(TryLoadBank(GFX.Game, "Graphics/ricky06ModPack/CustomSprites.xml"));
        GuiBank.SetBank(TryLoadBank(GFX.Gui, "Graphics/ricky06ModPack/CustomGuiSprites.xml"));
    }

    public static void Load()
    {
        if (loaded)
        {
            return;
        }

        loaded = true;

        On.Celeste.Player.CallDashEvents += Player_CallDashEvents;
        On.Celeste.Player.Die += Player_Die;
        On.Celeste.MapData.Reload += MapData_Reload;
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
        On.Celeste.Level.Update += Level_Update;

        TryHookPlayerMethod("DashBegin", nameof(Hook_Player_DashBegin), ref dashBeginHook);
        TryHookPlayerMethod("DashEnd", nameof(Hook_Player_DashEnd), ref dashEndHook);

        DarkLightningRenderer.Load();
        ResetZoneRenderer.Load();
    }

    public static void Unload()
    {
        if (!loaded)
        {
            return;
        }

        loaded = false;

        On.Celeste.Player.CallDashEvents -= Player_CallDashEvents;
        On.Celeste.Player.Die -= Player_Die;
        On.Celeste.MapData.Reload -= MapData_Reload;
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        On.Celeste.Level.Update -= Level_Update;

        dashBeginHook?.Dispose();
        dashBeginHook = null;
        dashEndHook?.Dispose();
        dashEndHook = null;

        DarkLightningRenderer.Unload();
        ResetZoneRenderer.Unload();

        ClearDestroyDash();
        noResetDict.Clear();
        bufferCooldown = false;
        lrzTransition = false;
    }

    public static void InitPlayerHealth(int amount)
    {
        playerHealth = amount;
    }

    public static bool DecreasePlayerHealth(Player player, ConquerorBoss boss)
    {
        playerHealth--;
        if (playerHealth <= 0)
        {
            return true;
        }

        Audio.Play("event:/char/madeline/predeath");
        return false;
    }

    private static void Player_CallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
    {
        Scene scene = self.Scene;
        if (scene != null)
        {
            foreach (BoomBooster entity in scene.Tracker.GetEntities<BoomBooster>())
            {
                if (entity.StartedBoosting)
                {
                    entity.PlayerBoosted(self, self.DashDir);
                    return;
                }
            }
        }

        orig(self);
    }

    private static PlayerDeadBody Player_Die(
        On.Celeste.Player.orig_Die orig,
        Player self,
        Vector2 direction,
        bool evenIfInvincible,
        bool registerDeathInStats)
    {
        if (!destroyDashActive || !evenIfInvincible)
        {
            ClearDestroyDash();
        }

        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void Level_LoadLevel(
        On.Celeste.Level.orig_LoadLevel orig,
        Level self,
        Player.IntroTypes playerIntro,
        bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);

        if (lrzTransition)
        {
            return;
        }

        ClearDestroyDash();
    }

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);

        DarkLightningRenderer renderer = self?.Tracker.GetEntity<DarkLightningRenderer>();
        if (renderer == null)
        {
            return;
        }

        if (self.Tracker.GetEntity<DarkLightning>() != null)
        {
            renderer.StartAmbience();
        }
        else
        {
            renderer.StopAmbience();
        }
    }

    private static void MapData_Reload(On.Celeste.MapData.orig_Reload orig, MapData self)
    {
        orig(self);
        ClearDestroyDash();
    }

    private static void ClearDestroyDash()
    {
        hasDestroyDash = false;
        destroyDashActive = false;

        destroyDashDelay?.RemoveSelf();
        destroyDashDelay = null;

        if (dw != null)
        {
            dw.RemoveSelf();
            dw = null;
        }
    }

    private delegate void orig_DashBegin(Player self);

    private delegate void orig_DashEnd(Player self);

    private static void Hook_Player_DashBegin(orig_DashBegin orig, Player self)
    {
        orig(self);

        if (!hasDestroyDash)
        {
            return;
        }

        destroyDashActive = true;
        hasDestroyDash = false;
        destroyDashDelay?.RemoveSelf();
        destroyDashDelay = null;
    }

    private static void Hook_Player_DashEnd(orig_DashEnd orig, Player self)
    {
        orig(self);

        if (self.StateMachine.State == 2 || !destroyDashActive)
        {
            return;
        }

        Audio.Play("event:/char/madeline/jump_super");
        destroyDashDelay = new Coroutine(DestroyDashDelay(self), true);
        self.Add(destroyDashDelay);
    }

    private static IEnumerator DestroyDashDelay(Player player)
    {
        yield return 0.1f;
        if (!player.DashAttacking)
        {
            destroyDashActive = false;
        }
    }

    private static void TryHookPlayerMethod(string methodName, string detourName, ref Hook hook)
    {
        try
        {
            MethodInfo target = typeof(Player).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo detour = typeof(BossesExampleModule).GetMethod(detourName, BindingFlags.Static | BindingFlags.NonPublic);

            if (target != null && detour != null)
            {
                hook = new Hook(target, detour);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"[BossesExample] Failed to hook Player.{methodName}: {ex.Message}");
        }
    }

    private static SpriteBank TryLoadBank(Atlas atlas, string path)
    {
        try
        {
            return new SpriteBank(atlas, path);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"[BossesExample] Missing sprite bank '{path}', using placeholders. {ex.Message}");
            return null;
        }
    }

    public sealed class BossesExampleSettingsProxy
    {
        public bool ResetKeysForSession => MaggyHelperModule.Settings?.BossesExampleResetKeysForSession == true;
    }

    public sealed class BossesExampleSaveDataProxy
    {
        public HashSet<string> StoneFlags => MaggyHelperModule.SaveData?.BossesExampleStoneFlags ?? FallbackStoneFlags;
    }

    public sealed class BossesExampleSpriteBankProxy
    {
        private readonly bool isGui;
        private readonly HashSet<string> missingIds = new();
        private SpriteBank bank;

        public BossesExampleSpriteBankProxy(bool isGui)
        {
            this.isGui = isGui;
        }

        public void SetBank(SpriteBank bank)
        {
            this.bank = bank;
            missingIds.Clear();
        }

        public Sprite Create(string spriteId)
        {
            if (bank != null)
            {
                try
                {
                    return bank.Create(spriteId);
                }
                catch
                {
                }
            }

            if (missingIds.Add(spriteId))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"[BossesExample] Missing sprite '{spriteId}', using a placeholder sprite.");
            }

            Sprite sprite = new Sprite(GFX.Game, "characters/player/");
            foreach (string animation in PlaceholderAnimations)
            {
                sprite.AddLoop(animation, "idle", 0.1f);
            }

            sprite.Play("idle");
            if (isGui)
            {
                sprite.Scale = Vector2.One * 0.75f;
            }

            return sprite;
        }
    }
}
