using System.Reflection;

namespace Celeste
{
    /// <summary>
    /// Tape Manager - A cassette-like system for C# and Lua that provides A/B/C/D-Side functionality
    /// Manages tape collections, side progression, and level unlocking mechanics
    /// </summary>
    public static class TapeManager
    {
        // Tape collection tracking
        private static Dictionary<AreaKey, TapeCollection> tapeCollections = new Dictionary<AreaKey, TapeCollection>();

        // Side progression tracking
        private static Dictionary<AreaKey, SideProgression> sideProgressions = new Dictionary<AreaKey, SideProgression>();

        // Lua script manager for tape scripting
        private static NLua tapeLuaManager;

        // Tape configuration
        public static bool IsInitialized { get; private set; }
        public static int MaxSides { get; set; } = 4; // A/B/C/D sides (4 total)

        // Events
        public static event Action<AreaKey, char> TapeCollected;
        public static event Action<AreaKey, char> SideUnlocked;
        public static event Action<AreaKey> AllTapesCollected;

        public static void Initialize()
        {
            if (IsInitialized) return;

            Logger.Log(LogLevel.Info, nameof(TapeManager), "Initializing Tape Manager system...");

            initializeLuaState();
            loadTapeConfigurations();
            hookEvents();

            IsInitialized = true;
            Logger.Log(LogLevel.Info, nameof(TapeManager), "Tape Manager initialized successfully!");
        }

        public static void Cleanup()
        {
            if (!IsInitialized) return;

            Logger.Log(LogLevel.Info, nameof(TapeManager), "Cleaning up Tape Manager...");

            unhookEvents();
            cleanupLuaState();
            tapeCollections.Clear();
            sideProgressions.Clear();

            IsInitialized = false;
        }

        private static void initializeLuaState()
        {
            tapeLuaManager = new NLua();

            // Register C# functions for Lua scripts
            tapeLuaManager.RegisterFunction("collectTape", typeof(TapeManager).GetMethod(nameof(CollectTapeFromLua)));
            tapeLuaManager.RegisterFunction("unlockSide", typeof(TapeManager).GetMethod(nameof(UnlockSideFromLua)));
            tapeLuaManager.RegisterFunction("getTapeCount", typeof(TapeManager).GetMethod(nameof(GetTapeCountFromLua)));
            tapeLuaManager.RegisterFunction("getSideUnlocked", typeof(TapeManager).GetMethod(nameof(GetSideUnlockedFromLua)));
            tapeLuaManager.RegisterFunction("playTapeSound", typeof(TapeManager).GetMethod(nameof(PlayTapeSoundFromLua)));
        }

        private static void cleanupLuaState()
        {
            tapeLuaManager.Dispose();
            tapeLuaManager = null;
        }

        private static void loadTapeConfigurations()
        {
            // Load tape configurations from data files
            foreach (var area in AreaData.Areas)
            {
                var areaKey = area.ToKey();
                initializeTapeCollection(areaKey);
            }
        }

        private static void initializeTapeCollection(AreaKey areaKey)
        {
            var collection = new TapeCollection(areaKey);
            var progression = new SideProgression(areaKey);

            tapeCollections[areaKey] = collection;
            sideProgressions[areaKey] = progression;
        }

        private static void hookEvents()
        {
            // Hook into Celeste events for tape collection
            // Note: Custom DesoloZantasTape entities will handle their own collection
            On.Celeste.Level.LoadLevel += OnLevelLoad;
        }

        private static void unhookEvents()
        {
            On.Celeste.Level.LoadLevel -= OnLevelLoad;
        }

        private static void OnLevelLoad(On.Celeste.Level.orig_LoadLevel orig, Level self, global::Celeste.Player.IntroTypes introTypes, bool isFromLoader)
        {
            orig(self, introTypes, isFromLoader);

            // Execute any tape-related Lua scripts for this level
            executeLevelTapeScript(self.Session.Area, self.Session.Level);
        }

        /// <summary>
        /// Validates if a side is within the allowed range (A/B/C/D only)
        /// </summary>
        private static bool IsValidSide(char side)
        {
            return side >= 'A' && side <= 'D';
        }

        /// <summary>
        /// Validates and clamps a side to the allowed range (A/B/C/D only)
        /// </summary>
        private static char ValidateSide(char side)
        {
            if (!IsValidSide(side))
            {
                Logger.Log(LogLevel.Warn, nameof(TapeManager), $"Invalid side '{side}' requested. Only A/B/C/D sides are supported. Defaulting to A.");
                return 'A';
            }
            return side;
        }

        /// <summary>
        /// Collects a tape for the specified area and determines if a new side should be unlocked
        /// </summary>
        public static bool CollectTape(AreaKey areaKey, string levelName, char side = 'A')
        {
            // Validate side
            side = ValidateSide(side);

            if (!tapeCollections.TryGetValue(areaKey, out var collection))
            {
                initializeTapeCollection(areaKey);
                collection = tapeCollections[areaKey];
            }

            bool wasCollected = collection.CollectTape(levelName, side);
            if (wasCollected)
            {
                TapeCollected?.Invoke(areaKey, side);
                playTapeCollectionSound(areaKey, side);

                // Check if this unlocks the next side
                checkSideUnlock(areaKey);

                // Check if all tapes are collected
                if (collection.AllTapesCollected())
                {
                    AllTapesCollected?.Invoke(areaKey);
                }
            }

            return wasCollected;
        }

        /// <summary>
        /// Unlocks a specific side for an area
        /// </summary>
        public static bool UnlockSide(AreaKey areaKey, char side)
        {
            // Validate side
            side = ValidateSide(side);

            if (!sideProgressions.TryGetValue(areaKey, out var progression))
            {
                initializeTapeCollection(areaKey);
                progression = sideProgressions[areaKey];
            }

            bool wasUnlocked = progression.UnlockSide(side);
            if (wasUnlocked)
            {
                SideUnlocked?.Invoke(areaKey, side);
                playSideUnlockSound(areaKey, side);
            }

            return wasUnlocked;
        }

        /// <summary>
        /// Gets the current tape count for a specific area and side
        /// </summary>
        public static int GetTapeCount(AreaKey areaKey, char side = 'A')
        {
            // Validate side
            side = ValidateSide(side);

            if (tapeCollections.TryGetValue(areaKey, out var collection))
            {
                return collection.GetTapeCount(side);
            }
            return 0;
        }

        /// <summary>
        /// Checks if a specific side is unlocked for an area
        /// </summary>
        public static bool IsSideUnlocked(AreaKey areaKey, char side)
        {
            // Validate side
            side = ValidateSide(side);

            if (sideProgressions.TryGetValue(areaKey, out var progression))
            {
                return progression.IsSideUnlocked(side);
            }
            return side == 'A'; // A-Side is always unlocked
        }

        /// <summary>
        /// Gets the highest unlocked side for an area
        /// </summary>
        public static char GetHighestUnlockedSide(AreaKey areaKey)
        {
            if (sideProgressions.TryGetValue(areaKey, out var progression))
            {
                return progression.GetHighestUnlockedSide();
            }
            return 'A';
        }

        /// <summary>
        /// Executes a Lua script for tape-related functionality
        /// </summary>
        public static void ExecuteTapeScript(string scriptPath, AreaKey? areaKey = null)
        {
            if (tapeLuaManager == null) return;

            try
            {
                if (areaKey.HasValue)
                {
                    NLua tapeLuaState = tapeLuaManager;
                }

                var script = File.ReadAllText(scriptPath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(TapeManager), $"Error executing tape script {scriptPath}: {ex}");
            }
        }

        private static void executeLevelTapeScript(AreaKey areaKey, string levelName)
        {
            // Look for level-specific tape scripts
            var scriptPath = Path.Combine("Mods", "MaggyHelper", "TapeScripts", $"{areaKey.SID}_{levelName}.lua");

            if (File.Exists(scriptPath))
            {
                ExecuteTapeScript(scriptPath, areaKey);
            }
        }

        private static void handleTapeCollection(AreaKey areaKey, string levelName)
        {
            // Determine current side
            char currentSide = getCurrentSide(areaKey);
            CollectTape(areaKey, levelName, currentSide);
        }

        private static char getCurrentSide(AreaKey areaKey)
        {
            // Convert AreaMode to side character
            switch (areaKey.Mode)
            {
                case AreaMode.Normal: return 'A';
                case AreaMode.BSide: return 'B';
                case AreaMode.CSide: return 'C';
                default: return 'A';
            }
        }

        private static void checkSideUnlock(AreaKey areaKey)
        {
            if (!tapeCollections.TryGetValue(areaKey, out var collection)) return;

            // Check if A-Side is complete and B-Side should be unlocked
            if (collection.IsSideComplete('A') && !IsSideUnlocked(areaKey, 'B'))
            {
                UnlockSide(areaKey, 'B');
            }

            // Check if B-Side is complete and C-Side should be unlocked
            if (collection.IsSideComplete('B') && !IsSideUnlocked(areaKey, 'C'))
            {
                UnlockSide(areaKey, 'C');
            }

            // Check if C-Side is complete and D-Side should be unlocked
            if (collection.IsSideComplete('C') && !IsSideUnlocked(areaKey, 'D'))
            {
                UnlockSide(areaKey, 'D');
            }
        }

        private static void playTapeCollectionSound(AreaKey areaKey, char side)
        {
            // Play appropriate sound for tape collection
            string soundEvent = $"event:/game/general/cassette_get";
            Audio.Play(soundEvent);
        }

        private static void playSideUnlockSound(AreaKey areaKey, char side)
        {
            // Play appropriate sound for side unlock
            string soundEvent = $"event:/ui/game/unlock_strawberry";
            Audio.Play(soundEvent);
        }

        public static bool CollectTapeFromLua(string areaKeySid, string levelName, string side) {
            var areaData = AreaData.Get(areaKeySid);
            if (areaData != null) {
                var areaKey = areaData.ToKey();
                char sideChar = string.IsNullOrEmpty(side) ? 'A' : side[0];
                sideChar = ValidateSide(sideChar);
                return CollectTape(areaKey, levelName, sideChar);
            }
            return false;
        }
        public static bool UnlockSideFromLua(string areaKeySid, string side) {
            var areaData = AreaData.Get(areaKeySid);
            if (areaData != null) {
                var areaKey = areaData.ToKey();
                char sideChar = string.IsNullOrEmpty(side) ? 'A' : side[0];
                sideChar = ValidateSide(sideChar);
                return UnlockSide(areaKey, sideChar);
            }
            return false;
        }
        public static int GetTapeCountFromLua(string areaKeySid, string side) {
            var areaData = AreaData.Get(areaKeySid);
            if (areaData != null) {
                var areaKey = areaData.ToKey();
                char sideChar = string.IsNullOrEmpty(side) ? 'A' : side[0];
                sideChar = ValidateSide(sideChar);
                return GetTapeCount(areaKey, sideChar);
            }
            return 0;
        }
        public static bool GetSideUnlockedFromLua(string areaKeySid, string side) {
            var areaData = AreaData.Get(areaKeySid);
            if (areaData != null) {
                var areaKey = areaData.ToKey();
                char sideChar = string.IsNullOrEmpty(side) ? 'A' : side[0];
                sideChar = ValidateSide(sideChar);
                return IsSideUnlocked(areaKey, sideChar);
            }
            return false;
        }


        public static void PlayTapeSoundFromLua(string soundEvent)
        {
            if (!string.IsNullOrEmpty(soundEvent))
            {
                Audio.Play(soundEvent);
            }
        }
    }

    internal class NLua
    {
        internal void RegisterFunction(string v, MethodInfo methodInfo)
        {
            Logger.Log(LogLevel.Warn, "NLua", $"RegisterFunction '{v}' called on stub NLua — actual Lua scripting not yet wired.");
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Manages tape collection for a specific area
    /// </summary>
    public class TapeCollection
    {
        private readonly AreaKey areaKey;
        private readonly Dictionary<char, HashSet<string>> tapesBySide;
        private readonly Dictionary<char, int> requiredTapesBySide;

        public TapeCollection(AreaKey areaKey)
        {
            this.areaKey = areaKey;
            tapesBySide = new Dictionary<char, HashSet<string>>();
            requiredTapesBySide = new Dictionary<char, int>();

            initializeSideRequirements();
        }

        private void initializeSideRequirements()
        {
            // Initialize tape requirements for A/B/C/D sides only
            for (char side = 'A'; side <= 'D'; side++)
            {
                tapesBySide[side] = new HashSet<string>();
                requiredTapesBySide[side] = 1; // Default: 1 tape per side
            }
        }

        public bool CollectTape(string levelName, char side)
        {
            // Only allow A/B/C/D sides
            if (side < 'A' || side > 'D')
                return false;

            if (!tapesBySide.ContainsKey(side))
                tapesBySide[side] = new HashSet<string>();

            return tapesBySide[side].Add(levelName);
        }

        public int GetTapeCount(char side)
        {
            return tapesBySide.TryGetValue(side, out var tapes) ? tapes.Count : 0;
        }

        public bool IsSideComplete(char side)
        {
            int requiredTapes = 1;
            if (requiredTapesBySide.TryGetValue(side, out int value))
                requiredTapes = value;
            return GetTapeCount(side) >= requiredTapes;
        }

        public bool AllTapesCollected()
        {
            // Check only A/B/C/D sides
            for (char side = 'A'; side <= 'D'; side++)
            {
                if (!IsSideComplete(side))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Manages side progression for a specific area
    /// </summary>
    public class SideProgression
    {
        private readonly AreaKey areaKey;
        private readonly HashSet<char> unlockedSides;

        public SideProgression(AreaKey areaKey)
        {
            this.areaKey = areaKey;
            unlockedSides = new HashSet<char> { 'A' }; // A-Side always unlocked
        }

        public bool UnlockSide(char side)
        {
            // Only allow A/B/C/D sides
            if (side < 'A' || side > 'D')
                return false;

            return unlockedSides.Add(side);
        }

        public bool IsSideUnlocked(char side)
        {
            // Only allow A/B/C/D sides
            if (side < 'A' || side > 'D')
                return false;

            return unlockedSides.Contains(side);
        }

        public char GetHighestUnlockedSide()
        {
            char highest = 'A';
            // Check only A/B/C/D sides
            for (char side = 'A'; side <= 'D'; side++)
            {
                if (unlockedSides.Contains(side))
                    highest = side;
            }
            return highest;
        }
    }
}
