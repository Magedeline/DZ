
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Per-session state for MaggyHelper mod.
    /// Tracks: Boss fights, Kirby abilities, credits state,
    /// overworld 3D state, current area/chapter state, mastery tracking.
    /// </summary>
    public class MaggyHelperModuleSession : EverestModuleSession
    {
        private string currentKirbyPower = global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString();

        public bool BossFightActive { get; set; }
        public string CurrentBossName { get; set; }
        public int BossesDefeated { get; set; }
        public bool IsKirbyModeActive { get; set; }
        public global::Celeste.Entities.Bosses.CopyAbilityType CurrentCopyAbility { get; set; }
        public string CurrentKirbyPower
        {
            get => currentKirbyPower;
            set => currentKirbyPower = string.IsNullOrWhiteSpace(value)
                ? global::Celeste.Extensions.KirbyMode.KirbyPowerState.None.ToString()
                : value;
        }
        public int EnemiesDefeated { get; set; }

        // Credits state
        public bool InCredits { get; set; }
        public int CreditsPhase { get; set; }
        public bool CreditsCompleted { get; set; }

        // ── Mastery first-try tracking ────────────────────────────────────
        /// <summary>True when this run is the player's first attempt at this chapter.</summary>
        public bool IsTrackingFirstTry { get; set; }
        /// <summary>Set the first time the player dies during an IsTrackingFirstTry run.</summary>
        public bool DiedThisRun { get; set; }
        /// <summary>Set the first time PlayerHealthManager reports damage during a tracked run.</summary>
        public bool TookDamageThisRun { get; set; }

        // ── Overworld 3D State ─────────────────────────────────────────────
        /// <summary>Current mountain state being viewed (Normal=0, Dark=1, Void=2).</summary>
        public int CurrentMountainState { get; set; }

        /// <summary>Last chapter number the player was viewing in overworld.</summary>
        public int LastViewedChapterNumber { get; set; } = -1;

        /// <summary>Whether the camera is currently in a transition ease.</summary>
        public bool IsMountainCameraEasing { get; set; }

        /// <summary>Time remaining for camera ease transition window.</summary>
        public float MountainEaseCountdown { get; set; }

        /// <summary>Current camera position override (if any).</summary>
        public Vector3? OverrideCameraPosition { get; set; }

        /// <summary>Current camera target override (if any).</summary>
        public Vector3? OverrideCameraTarget { get; set; }
        // ──────────────────────────────────────────────────────────────────────

        // ── Area/Chapter Session State ────────────────────────────────────────
        /// <summary>SID of the chapter the player is currently in.</summary>
        public string CurrentChapterSID { get; set; }

        /// <summary>Current chapter number (0-20).</summary>
        public int CurrentChapterNumber { get; set; } = -1;

        /// <summary>Current side being played (0=A, 1=B, 2=C, 3=D, 4=DX).</summary>
        public int CurrentSideIndex { get; set; }

        /// <summary>Whether current chapter has D-Side unlocked this session.</summary>
        public bool HasDSideUnlockedThisSession { get; set; }

        /// <summary>Whether current chapter has DX-Side unlocked this session.</summary>
        public bool HasDXSideUnlockedThisSession { get; set; }

        /// <summary>Whether current chapter has C-Side unlocked this session (via tape collection).</summary>
        public bool HasCSideUnlockedThisSession { get; set; }

        /// <summary>Session-start timestamp for speedrun tracking.</summary>
        public long SessionStartTimestamp { get; set; }

        /// <summary>Berries collected this session per chapter.</summary>
        public Dictionary<string, int> SessionBerryCounts { get; set; } = new();

        /// <summary>Whether the player has seen the intro warning this session.</summary>
        public bool HasSeenSessionIntro { get; set; }
        // ──────────────────────────────────────────────────────────────────────
    }
}