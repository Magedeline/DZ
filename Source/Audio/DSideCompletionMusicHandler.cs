using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper.AudioHandlers
{
    /// <summary>
    /// Handles D-Side and DX-Side completion music events for DesoloZantas maps.
    /// Integrates with the completion system to play appropriate music based on side and chapter.
    /// </summary>
    public class DSideCompletionMusicHandler
    {
        private static readonly string[] ChapterCompletionEvents = {
            "event:/pusheen/music/menu/complete_area",
            "event:/pusheen/music/menu/complete_bside", 
            "event:/pusheen/music/menu/complete_bside_summit",
            "event:/pusheen/music/menu/complete_cside",
            "event:/pusheen/music/menu/complete_cside_summit",
            "event:/pusheen/music/menu/complete_dside_void",
            "event:/pusheen/music/menu/complete_summit"
        };

        /// <summary>
        /// Gets the appropriate completion music event based on area mode and chapter.
        /// </summary>
        /// <param name="areaMode">The area mode (Normal, BSide, CSide)</param>
        /// <param name="chapterIndex">The chapter index (1-18 for main chapters)</param>
        /// <param name="isSummit">Whether this is the summit chapter</param>
        /// <returns>The FMOD event path for the completion music</returns>
        public static string GetCompletionMusicEvent(AreaMode areaMode, int chapterIndex, bool isSummit = false)
        {
            return areaMode switch
            {
                AreaMode.Normal => "event:/pusheen/music/menu/complete_area",
                AreaMode.BSide => isSummit ? 
                    "event:/pusheen/music/menu/complete_bside_summit" : 
                    "event:/pusheen/music/menu/complete_bside",
                AreaMode.CSide => isSummit ? 
                    "event:/pusheen/music/menu/complete_cside_summit" : 
                    "event:/pusheen/music/menu/complete_cside",
                _ => "event:/pusheen/music/menu/complete_area"
            };
        }

        /// <summary>
        /// Gets the D-Side completion music event for the Void Moon chapter.
        /// </summary>
        /// <returns>The FMOD event path for D-Side completion</returns>
        public static string GetDSideCompletionEvent()
        {
            return "event:/pusheen/music/menu/complete_dside_void";
        }

        /// <summary>
        /// Gets the summit completion music event.
        /// </summary>
        /// <returns>The FMOD event path for summit completion</returns>
        public static string GetSummitCompletionEvent()
        {
            return "event:/pusheen/music/menu/complete_summit";
        }

        /// <summary>
        /// Plays the appropriate completion music based on the current session.
        /// </summary>
        /// <param name="session">The current level session</param>
        public static void PlayCompletionMusic(Session session)
        {
            if (session == null) return;

            string musicEvent = DetermineCompletionMusic(session);
            if (!string.IsNullOrEmpty(musicEvent))
            {
                global::Celeste.Audio.Play(musicEvent);
            }
        }

        /// <summary>
        /// Determines the appropriate completion music event for the given session.
        /// </summary>
        /// <param name="session">The current level session</param>
        /// <returns>The FMOD event path for the completion music</returns>
        private static string DetermineCompletionMusic(Session session)
        {
            // Check if this is the Void Moon (D-Side) chapter
            if (IsVoidMoonChapter(session))
            {
                return GetDSideCompletionEvent();
            }

            // Check if this is the summit chapter
            bool isSummit = IsSummitChapter(session);
            
            // Get the completion music based on area mode
            return GetCompletionMusicEvent(session.Area.Mode, GetChapterIndex(session), isSummit);
        }

        /// <summary>
        /// Checks if the current session is for the Void Moon chapter.
        /// </summary>
        /// <param name="session">The current level session</param>
        /// <returns>True if this is the Void Moon chapter</returns>
        private static bool IsVoidMoonChapter(Session session)
        {
            return session.Area.SID?.Contains("DSide") == true || 
                   session.Area.SID?.Contains("void") == true ||
                   session.Area.SID?.Contains("Void") == true;
        }

        /// <summary>
        /// Checks if the current session is for the summit chapter.
        /// </summary>
        /// <param name="session">The current level session</param>
        /// <returns>True if this is the summit chapter</returns>
        private static bool IsSummitChapter(Session session)
        {
            return session.Area.SID?.Contains("09_Summit") == true ||
                   session.Area.SID?.Contains("Summit") == true ||
                   session.Area.ID == 9; // Chapter 9 is typically the summit
        }

        /// <summary>
        /// Extracts the chapter index from the session.
        /// </summary>
        /// <param name="session">The current level session</param>
        /// <returns>The chapter index (1-18 for main chapters)</returns>
        private static int GetChapterIndex(Session session)
        {
            // Try to extract from SID first
            if (!string.IsNullOrEmpty(session.Area.SID))
            {
                // Look for patterns like "01_City", "02_Nightmare", etc.
                var sidMatch = System.Text.RegularExpressions.Regex.Match(session.Area.SID, @"(\d+)_");
                if (sidMatch.Success && int.TryParse(sidMatch.Groups[1].Value, out int sidIndex))
                {
                    return sidIndex;
                }
            }

            // Fall back to area ID
            return session.Area.ID;
        }

        /// <summary>
        /// Hook into the area completion system to play custom music.
        /// This should be called when the mod initializes.
        /// </summary>
        public static void InitializeHooks()
        {
            // Hook into the area complete screen begin event
            On.Celeste.AreaComplete.Begin += OnAreaCompleteBegin;
        }

        /// <summary>
        /// Hook handler for AreaComplete.Begin.
        /// </summary>
        private static void OnAreaCompleteBegin(On.Celeste.AreaComplete.orig_Begin orig, AreaComplete self)
        {
            // Play our custom completion music when the area complete screen begins
            if (self.Session != null)
            {
                PlayCompletionMusic(self.Session);
            }

            // Call the original Begin method
            orig(self);
        }

        /// <summary>
        /// Unhooks the area completion system.
        /// This should be called when the mod unloads.
        /// </summary>
        public static void UnloadHooks()
        {
            // Unhook from the area complete screen begin event
            On.Celeste.AreaComplete.Begin -= OnAreaCompleteBegin;
        }
    }
}
