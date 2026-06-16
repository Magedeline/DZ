using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Per-session state for KIRBY_CELESTE mod.
    /// Tracks: Boss fights, Kirby abilities, credits state,
    /// overworld 3D state, current area/chapter state, mastery tracking.
    /// </summary>
    public class DZModuleSession : EverestModuleSession
    {
        // ── Kirby Power (validated setter) ────────────────────────────────────
        private string currentKirbyPower = global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString();

        public string CurrentKirbyPower
        {
            get => currentKirbyPower;
            set => currentKirbyPower = string.IsNullOrWhiteSpace(value)
                ? global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString()
                : value;
        }

        // ── Kirby State ────────────────────────────────────────────────────────
        /// <summary>Whether Kirby mode is currently active in this session.</summary>
        public bool IsKirbyModeActive { get; set; }

        /// <summary>
        /// Currently active character ID for this session.
        /// Reads from and writes through <see cref="global::Celeste.Entities.PlayerSelectionManager"/>
        /// so that all systems stay in sync with the authoritative player selection.
        /// </summary>
        public string ActiveCharacterId
        {
            get
            {
                var selected = global::Celeste.Entities.PlayerSelectionManager.GetSelectedPlayer();
                return selected == global::Celeste.Entities.PlayerSelectionManager.PlayerType.Kirby
                    ? global::Celeste.PlayerCharacterIds.Kirby
                    : global::Celeste.PlayerCharacterIds.Madeline;
            }
            set
            {
                bool isKirby = value == global::Celeste.PlayerCharacterIds.Kirby;
                global::Celeste.Entities.PlayerSelectionManager.SetDefaultPlayer(
                    isKirby
                        ? global::Celeste.Entities.PlayerSelectionManager.PlayerType.Kirby
                        : global::Celeste.Entities.PlayerSelectionManager.PlayerType.Madeline);
            }
        }

        /// <summary>Kirby's current health.</summary>
        public int KirbyHealth { get; set; } = 6;

        /// <summary>Kirby's current stamina.</summary>
        public float KirbyStamina { get; set; } = 100f;

        /// <summary>Whether Knight mode is currently active.</summary>
        public bool IsKnightModeActive { get; set; }

        /// <summary>Time remaining for current timed power (0 = no timer).</summary>
        public float PowerTimeRemaining { get; set; }

        // ── Boss State ─────────────────────────────────────────────────────────
        /// <summary>Whether a boss fight is currently active.</summary>
        public bool IsBossFightActive { get; set; }

        /// <summary>Alias for <see cref="IsBossFightActive"/> used by boss entities.</summary>
        public bool BossFightActive
        {
            get => IsBossFightActive;
            set => IsBossFightActive = value;
        }

        /// <summary>Number of bosses defeated this session.</summary>
        public int BossesDefeated { get; set; }

        /// <summary>Current copy ability granted by a boss fight.</summary>
        public global::Celeste.Entities.Bosses.CopyAbilityType CurrentCopyAbility { get; set; }

        /// <summary>Current boss name (if in a boss fight).</summary>
        public string CurrentBossName { get; set; }

        /// <summary>Current boss phase.</summary>
        public int CurrentBossPhase { get; set; }

        /// <summary>Current boss health as a 0–1 percentage.</summary>
        public float CurrentBossHealthPercent { get; set; } = 1f;

        // ── Gameplay Stats ─────────────────────────────────────────────────────
        /// <summary>Number of enemies defeated in this session.</summary>
        public int EnemiesDefeated { get; set; }

        /// <summary>Total damage dealt in this session.</summary>
        public int TotalDamageDealt { get; set; }

        /// <summary>Total damage received in this session.</summary>
        public int TotalDamageReceived { get; set; }

        /// <summary>Number of powers copied in this session.</summary>
        public int PowersCopied { get; set; }

        // ── Cutscene State ─────────────────────────────────────────────────────
        /// <summary>ID of the last completed cutscene.</summary>
        public string LastCompletedCutscene { get; set; } = "";

        /// <summary>Whether we are currently inside a custom cutscene.</summary>
        public bool InCustomCutscene { get; set; }

        /// <summary>Name of the active cutscene (empty when none).</summary>
        public string CurrentCutsceneName { get; set; } = "";

        // ── Credits State ──────────────────────────────────────────────────────
        /// <summary>Whether the credits sequence is currently playing.</summary>
        public bool InCredits { get; set; }

        /// <summary>Current phase of the credits sequence.</summary>
        public int CreditsPhase { get; set; }

        /// <summary>Whether the credits sequence has been fully completed.</summary>
        public bool CreditsCompleted { get; set; }

        // ── Mastery First-Try Tracking ─────────────────────────────────────────
        /// <summary>True when this run is the player's first attempt at this chapter.</summary>
        public bool IsTrackingFirstTry { get; set; }

        /// <summary>Set the first time the player dies during an <see cref="IsTrackingFirstTry"/> run.</summary>
        public bool DiedThisRun { get; set; }

        /// <summary>Set the first time PlayerHealthManager reports damage during a tracked run.</summary>
        public bool TookDamageThisRun { get; set; }

        // ── NPC State ──────────────────────────────────────────────────────────
        /// <summary>Per-NPC state map (NPC ID → state name).</summary>
        public Dictionary<string, string> NPCStates { get; set; } = new Dictionary<string, string>();

        /// <summary>NPCs that have been talked to this session.</summary>
        public List<string> TalkedToNPCs { get; set; } = new List<string>();

        // ── Custom Session Data ────────────────────────────────────────────────
        /// <summary>Custom boolean flags set by triggers and entities.</summary>
        public Dictionary<string, bool> CustomFlags { get; set; } = new Dictionary<string, bool>();

        /// <summary>Custom integer counters.</summary>
        public Dictionary<string, int> CustomCounters { get; set; } = new Dictionary<string, int>();

        /// <summary>Custom string values.</summary>
        public Dictionary<string, string> CustomStrings { get; set; } = new Dictionary<string, string>();

        // ── Lives System ───────────────────────────────────────────────────────
        /// <summary>Starting lives for a chapter attempt.</summary>
        public const int MaxLives = 3;

        /// <summary>
        /// Lives remaining in the current chapter attempt.
        /// Game-over music plays when this hits zero.
        /// Resets to <see cref="MaxLives"/> on a fresh (non-save) chapter entry.
        /// </summary>
        public int LivesRemaining { get; set; } = MaxLives;

        /// <summary>Whether the current chapter entry used a saved respawn state.</summary>
        public bool UsedSavedChapterRespawn { get; set; }

        /// <summary>Whether a save point/checkpoint has been registered for this session.</summary>
        public bool HasRegisteredChapterSavePoint { get; set; }

        /// <summary>Last save point checkpoint identifier for this session.</summary>
        public string LastCheckpointId { get; set; } = string.Empty;

        // ── Overworld 3D State ─────────────────────────────────────────────────
        /// <summary>Current mountain state being viewed (Normal=0, Dark=1, Void=2).</summary>
        public int CurrentMountainState { get; set; }

        /// <summary>Last chapter number the player was viewing in the overworld.</summary>
        public int LastViewedChapterNumber { get; set; } = -1;

        /// <summary>Whether the camera is currently in a transition ease.</summary>
        public bool IsMountainCameraEasing { get; set; }

        /// <summary>Time remaining for camera ease transition window.</summary>
        public float MountainEaseCountdown { get; set; }

        /// <summary>Current camera position override (if any).</summary>
        public Vector3? OverrideCameraPosition { get; set; }

        /// <summary>Current camera target override (if any).</summary>
        public Vector3? OverrideCameraTarget { get; set; }

        // ── Area/Chapter Session State ─────────────────────────────────────────
        /// <summary>SID of the chapter the player is currently in.</summary>
        public string CurrentChapterSID { get; set; }

        /// <summary>Current chapter number (0-20).</summary>
        public int CurrentChapterNumber { get; set; } = -1;

        /// <summary>Current side being played (0=A, 1=B, 2=C, 3=D, 4=DX).</summary>
        public int CurrentSideIndex { get; set; }

        /// <summary>Whether the current chapter has its D-Side unlocked this session.</summary>
        public bool HasDSideUnlockedThisSession { get; set; }

        /// <summary>Whether the current chapter has its DX-Side unlocked this session.</summary>
        public bool HasDXSideUnlockedThisSession { get; set; }

        /// <summary>Whether the current chapter has its C-Side unlocked this session (via tape collection).</summary>
        public bool HasCSideUnlockedThisSession { get; set; }

        /// <summary>Session-start timestamp for speedrun tracking.</summary>
        public long SessionStartTimestamp { get; set; }

        /// <summary>Berries collected this session per chapter SID.</summary>
        public Dictionary<string, int> SessionBerryCounts { get; set; } = new();

        /// <summary>Whether the player has seen the intro warning this session.</summary>
        public bool HasSeenSessionIntro { get; set; }

        // =====================================================================
        //  Helper Methods
        // =====================================================================

        /// <summary>
        /// Reset Kirby state to defaults.
        /// </summary>
        public void ResetKirbyState()
        {
            IsKirbyModeActive = false;
            CurrentKirbyPower = global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString();
            ActiveCharacterId = global::Celeste.PlayerCharacterIds.Madeline;
            KirbyHealth = 6;
            KirbyStamina = 100f;
            IsKnightModeActive = false;
            PowerTimeRemaining = 0f;
        }

        /// <summary>
        /// Returns the normalized active character ID.
        /// </summary>
        public string GetActiveCharacterId()
        {
            return global::Celeste.PlayerCharacter.NormalizeId(ActiveCharacterId);
        }

        /// <summary>
        /// Set the active character and synchronize Kirby mode state.
        /// </summary>
        public void SetActiveCharacter(string characterId)
        {
            ActiveCharacterId = global::Celeste.PlayerCharacter.NormalizeId(characterId);
            IsKirbyModeActive = ActiveCharacterId == global::Celeste.PlayerCharacterIds.Kirby;
        }

        /// <summary>Get a custom boolean flag value.</summary>
        public bool GetFlag(string flagName)
        {
            return CustomFlags.TryGetValue(flagName, out bool value) && value;
        }

        /// <summary>Set a custom boolean flag value.</summary>
        public void SetFlag(string flagName, bool value)
        {
            CustomFlags[flagName] = value;
        }

        /// <summary>Get a custom counter value (0 if not set).</summary>
        public int GetCounter(string counterName)
        {
            return CustomCounters.TryGetValue(counterName, out int value) ? value : 0;
        }

        /// <summary>Set a custom counter value.</summary>
        public void SetCounter(string counterName, int value)
        {
            CustomCounters[counterName] = value;
        }

        /// <summary>Increment a custom counter by <paramref name="amount"/> and return the new value.</summary>
        public int IncrementCounter(string counterName, int amount = 1)
        {
            int newValue = GetCounter(counterName) + amount;
            SetCounter(counterName, newValue);
            return newValue;
        }

        /// <summary>Get a custom string value (empty string if not set).</summary>
        public string GetString(string stringName)
        {
            return CustomStrings.TryGetValue(stringName, out string value) ? value : "";
        }

        /// <summary>Set a custom string value.</summary>
        public void SetString(string stringName, string value)
        {
            CustomStrings[stringName] = value;
        }
    }
}
