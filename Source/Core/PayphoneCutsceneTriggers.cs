#nullable disable

using System;
using Celeste;
using Celeste.Cutscenes;
using Celeste.Entities;
using Monocle;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Triggers cutscenes for payphone dream/awake sequences.
    ///
    /// Hooks into Level.LoadLevel to detect specific level names and
    /// automatically spawn the appropriate cutscene entity.
    /// </summary>
    public static class PayphoneCutsceneTriggers
    {
        private static On.Celeste.Level.hook_LoadLevel payphoneLoadLevelHook;

        public static void Load()
        {
            if (payphoneLoadLevelHook == null)
            {
                payphoneLoadLevelHook = new On.Celeste.Level.hook_LoadLevel(Hook_Level_LoadLevel);
                On.Celeste.Level.LoadLevel += payphoneLoadLevelHook;
            }

            Logger.Log(LogLevel.Info, "DZ",
                "[PayphoneCutsceneTriggers] Payphone cutscene triggers loaded");
        }

        public static void Unload()
        {
            if (payphoneLoadLevelHook != null)
                On.Celeste.Level.LoadLevel -= payphoneLoadLevelHook;
            payphoneLoadLevelHook = null;

            Logger.Log(LogLevel.Info, "DZ",
                "[PayphoneCutsceneTriggers] Payphone cutscene triggers unloaded");
        }

        private static void Hook_Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self,
            Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Call original LoadLevel first
            orig(self, playerIntro, isFromLoader);

            try
            {
                if (self.Session?.Level == null)
                    return;

                var player = self.Tracker.GetEntity<Player>();
                if (player == null)
                    return;

                string levelName = self.Session.Level.ToLowerInvariant();

                // Spawn CharaChaser in room b-3
                if (levelName == "b-3")
                {
                    if (self.Tracker.GetEntity<CharaChaser>() == null)
                    {
                        var charaChaser = new CharaChaser(new Vector2(160f, 100f), 0);
                        self.Add(charaChaser);
                        Logger.Log(LogLevel.Info, "DZ",
                            "[PayphoneCutsceneTriggers] Spawned CharaChaser in room b-3");
                    }
                }

                // CS02_Ending - Awake sequence where you call Kirby
                if (levelName.Contains("02_") && levelName.Contains("end"))
                {
                    // Check if payphone exists in this level
                    var payphone = self.Tracker.GetEntity<Payphone>();
                    if (payphone != null)
                    {
                        var cutscene = new Cs02CallKirby(player);
                        self.Add(cutscene);
                        Logger.Log(LogLevel.Info, "DZ",
                            "[PayphoneCutsceneTriggers] Triggered Cs02CallKirby cutscene for level: " + levelName);
                    }
                }

                // CS02_DreamingPhonecall - Dream sequence phonecall
                else if (levelName.Contains("02_") && self.Session.Dreaming)
                {
                    // Check for specific dream phonecall trigger
                    if (levelName.Contains("dream") || levelName.Contains("nightmare") || levelName.Contains("trap"))
                    {
                        var payphone = self.Tracker.GetEntity<Payphone>();
                        if (payphone != null && self.Tracker.GetEntity<Cs02DreamingPhonecallPortal>() == null)
                        {
                            var cutscene = new Cs02DreamingPhonecallPortal(player);
                            self.Add(cutscene);
                            Logger.Log(LogLevel.Info, "DZ",
                                "[PayphoneCutsceneTriggers] Triggered Cs02DreamingPhonecallPortal cutscene for level: " + levelName);
                        }
                    }
                }

                // CS04_DreamingPhonecall (Cs02CharaTrap) - Chara trap sequence
                else if (levelName.Contains("04_") && self.Session.Dreaming)
                {
                    if (levelName.Contains("dream") || levelName.Contains("nightmare") || levelName.Contains("trap"))
                    {
                        var payphone = self.Tracker.GetEntity<Payphone>();
                        if (payphone != null && self.Tracker.GetEntity<Cs02CharaTrap>() == null)
                        {
                            var cutscene = new Cs02CharaTrap(player);
                            self.Add(cutscene);
                            Logger.Log(LogLevel.Info, "DZ",
                                "[PayphoneCutsceneTriggers] Triggered Cs02CharaTrap cutscene for level: " + levelName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[PayphoneCutsceneTriggers] Error in cutscene trigger hook: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
