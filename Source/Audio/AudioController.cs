using System;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Centralized audio controller for Desolo Zantas.
    /// Maps FMOD studio audio event instances to level progression states.
    /// Implements dynamic parameter adjustments (e.g., combat_intensity) when entering arena boundaries.
    /// </summary>
    public static class AudioController
    {
        #region State Definitions

        /// <summary>
        /// Music states that correspond to level progression.
        /// </summary>
        public enum MusicState
        {
            Ambient,        // Exploration, puzzle rooms
            Tension,        // Approaching danger
            Combat,         // Arena combat / boss fights
            Puzzle,         // Active puzzle solving
            Cutscene,       // Story sequences
            Victory,        // Post-completion
            Transition      // Screen transitions
        }

        /// <summary>
        /// Ambience states for environmental audio layering.
        /// </summary>
        public enum AmbienceState
        {
            None,
            Ruins,          // Chapter 10 - wind, dust
            Snowdin,        // Chapter 11 - cold wind, bells
            Water,          // Chapter 12 - flowing water
            Fire,           // Chapter 13 - lava rumble
            Digital,        // Chapter 14 - electronic hum
            Castle,         // Chapter 15 - grand halls
            Corruption,     // Chapter 16 - glitch/static
            Space,          // Chapter 19 - cosmic silence, starfield
            Heart           // Chapter 18 - core pulse
        }

        #endregion

        #region Event Paths

        // Base music events by chapter (structural complexity scales with progression)
        private static readonly Dictionary<string, string> ChapterMusicEvents = new Dictionary<string, string>
        {
            ["00_Prologue"]   = "event:/pusheen/music/lvl0/explore",
            ["01_City"]       = "event:/pusheen/music/lvl1/explore",
            ["02_Nightmare"]  = "event:/pusheen/music/lvl2/explore",
            ["03_Stars"]      = "event:/pusheen/music/lvl3/explore",
            ["04_Legend"]     = "event:/pusheen/music/lvl4/explore",
            ["05_Restore"]    = "event:/pusheen/music/lvl5/explore",
            ["06_Stronghold"] = "event:/pusheen/music/lvl6/explore",
            ["07_Hell"]       = "event:/pusheen/music/lvl7/explore",
            ["08_Truth"]      = "event:/pusheen/music/lvl8/explore",
            ["09_Summit"]     = "event:/pusheen/music/lvl9/explore",
            ["10_Ruins"]      = "event:/pusheen/music/lvl10/main",
            ["11_Snow"]       = "event:/pusheen/music/lvl11/explore",
            ["12_Water"]      = "event:/pusheen/music/lvl12/explore",
            ["13_Fire"]       = "event:/pusheen/music/lvl13/explore",
            ["14_Digital"]    = "event:/pusheen/music/lvl14/explore",
            ["15_Castle"]     = "event:/pusheen/music/lvl15/explore",
            ["16_Corruption"] = "event:/pusheen/music/lvl16/explore",
            ["17_Epilogue"]   = "event:/pusheen/music/lvl17/explore",
            ["18_Heart"]      = "event:/pusheen/music/lvl18/explore",
            ["19_Space"]      = "event:/pusheen/music/lvl19/explore",
            ["20_TheEnd"]     = "event:/pusheen/music/lvl20/explore",
            ["21_LastLevel"]  = "event:/pusheen/music/lvl21/explore",
        };

        // Combat/intense music layers per chapter
        private static readonly Dictionary<string, string> ChapterCombatMusicEvents = new Dictionary<string, string>
        {
            ["10_Ruins"]      = "event:/pusheen/music/lvl10/combat",
            ["11_Snow"]       = "event:/pusheen/music/lvl11/combat",
            ["13_Fire"]       = "event:/pusheen/music/lvl13/combat",
            ["16_Corruption"] = "event:/pusheen/music/lvl16/combat",
            ["18_Heart"]      = "event:/pusheen/music/lvl18/combat",
            ["20_TheEnd"]     = "event:/pusheen/music/lvl20/combat",
        };

        // Ambience events by environment type
        private static readonly Dictionary<AmbienceState, string> AmbienceEvents = new Dictionary<AmbienceState, string>
        {
            [AmbienceState.Ruins]      = "event:/pusheen/ambience/ruins",
            [AmbienceState.Snowdin]    = "event:/pusheen/ambience/snow",
            [AmbienceState.Water]      = "event:/pusheen/ambience/water",
            [AmbienceState.Fire]       = "event:/pusheen/ambience/fire",
            [AmbienceState.Digital]    = "event:/pusheen/ambience/digital",
            [AmbienceState.Space]      = "event:/pusheen/ambience/space",
            [AmbienceState.Heart]      = "event:/pusheen/ambience/heart",
            [AmbienceState.Corruption] = "event:/pusheen/ambience/corruption",
        };

        // FMOD parameter names for dynamic mixing
        private const string PARAM_COMBAT_INTENSITY = "combat_intensity";
        private const string PARAM_MUSIC_LAYER = "music_layer";
        private const string PARAM_TENSION = "tension_level";

        #endregion

        #region Runtime State

        private static MusicState _currentMusicState = MusicState.Ambient;
        private static AmbienceState _currentAmbience = AmbienceState.None;
        private static EventInstance _currentMusicInstance;
        private static EventInstance _currentAmbienceInstance;
        private static bool _hasMusicInstance = false;
        private static bool _hasAmbienceInstance = false;
        private static float _combatIntensity = 0f;
        private static string _currentChapter;
        private static bool _initialized = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the audio controller. Call once when the mod loads.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "AudioController initialized");
        }

        /// <summary>
        /// Shutdown and cleanup all active audio instances.
        /// </summary>
        public static void Shutdown()
        {
            StopMusic();
            StopAmbience();
            _initialized = false;
        }

        #endregion

        #region Music State Management

        /// <summary>
        /// Set the music state for the current level, adjusting FMOD parameters dynamically.
        /// </summary>
        public static void SetMusicState(MusicState state, Level level = null)
        {
            if (!_initialized) return;

            _currentMusicState = state;
            string chapter = GetCurrentChapter(level);
            if (chapter == null) return;

            switch (state)
            {
                case MusicState.Ambient:
                    TransitionToAmbient(chapter);
                    break;
                case MusicState.Tension:
                    TransitionToTension(chapter);
                    break;
                case MusicState.Combat:
                    TransitionToCombat(chapter);
                    break;
                case MusicState.Puzzle:
                    TransitionToPuzzle(chapter);
                    break;
                case MusicState.Cutscene:
                    TransitionToCutscene(chapter);
                    break;
                case MusicState.Victory:
                    TransitionToVictory(chapter);
                    break;
                case MusicState.Transition:
                    TransitionToTransition(chapter);
                    break;
            }

            Logger.Log(LogLevel.Debug, "MaggyHelper/Audio", $"Music state set to: {state} (chapter: {chapter})");
        }

        /// <summary>
        /// Set the combat intensity parameter (0.0 to 1.0).
        /// Higher values trigger denser percussion layers.
        /// </summary>
        public static void SetCombatIntensity(float intensity)
        {
            if (!_initialized) return;

            _combatIntensity = Calc.Clamp(intensity, 0f, 1f);

            if (_hasMusicInstance)
            {
                _currentMusicInstance.setParameterValue(PARAM_COMBAT_INTENSITY, _combatIntensity);
            }
        }

        /// <summary>
        /// Called when entering an arena boundary. Ramps up combat intensity.
        /// </summary>
        public static void EnterArenaBoundary(Level level)
        {
            SetMusicState(MusicState.Combat, level);
            SetCombatIntensity(0.5f);
            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "Entered arena boundary - combat music activated");
        }

        /// <summary>
        /// Called when leaving an arena boundary. Fades down combat intensity.
        /// </summary>
        public static void ExitArenaBoundary(Level level)
        {
            SetCombatIntensity(0f);
            SetMusicState(MusicState.Ambient, level);
            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "Exited arena boundary - returning to ambient");
        }

        /// <summary>
        /// Update combat intensity based on proximity to arena center or enemy count.
        /// Call this from a trigger or entity Update loop.
        /// </summary>
        public static void UpdateCombatIntensity(float normalizedProximity)
        {
            SetCombatIntensity(normalizedProximity);
        }

        #endregion

        #region Ambience Management

        /// <summary>
        /// Set the environmental ambience based on room type.
        /// </summary>
        public static void SetAmbience(AmbienceState state)
        {
            if (!_initialized) return;
            if (_currentAmbience == state) return;

            StopAmbience();
            _currentAmbience = state;

            if (AmbienceEvents.TryGetValue(state, out string eventPath) && !string.IsNullOrEmpty(eventPath))
            {
                _currentAmbienceInstance = global::Celeste.Audio.Play(eventPath);
                _hasAmbienceInstance = true;
                Logger.Log(LogLevel.Debug, "MaggyHelper/Audio", $"Ambience set to: {state}");
            }
        }

        /// <summary>
        /// Set ambience based on chapter name.
        /// </summary>
        public static void SetAmbienceForChapter(string chapterName)
        {
            AmbienceState state = chapterName switch
            {
                "10_Ruins"      => AmbienceState.Ruins,
                "11_Snow"       => AmbienceState.Snowdin,
                "12_Water"      => AmbienceState.Water,
                "13_Fire"       => AmbienceState.Fire,
                "14_Digital"    => AmbienceState.Digital,
                "15_Castle"     => AmbienceState.None,
                "16_Corruption" => AmbienceState.Corruption,
                "18_Heart"      => AmbienceState.Heart,
                "19_Space"      => AmbienceState.Space,
                _               => AmbienceState.None
            };

            SetAmbience(state);
        }

        #endregion

        #region Private Transition Methods

        private static void TransitionToAmbient(string chapter)
        {
            if (ChapterMusicEvents.TryGetValue(chapter, out string eventPath))
            {
                PlayMusic(eventPath);
                SetParameter(PARAM_MUSIC_LAYER, 0f); // Subtle melodic layer
                SetParameter(PARAM_TENSION, 0f);
                SetCombatIntensity(0f);
            }
        }

        private static void TransitionToTension(string chapter)
        {
            SetParameter(PARAM_TENSION, 0.5f);
            SetParameter(PARAM_MUSIC_LAYER, 1f); // Add tension instruments
        }

        private static void TransitionToCombat(string chapter)
        {
            // Try combat-specific music first, fall back to explore with high intensity
            if (ChapterCombatMusicEvents.TryGetValue(chapter, out string combatEvent))
            {
                PlayMusic(combatEvent);
            }
            else if (ChapterMusicEvents.TryGetValue(chapter, out string exploreEvent))
            {
                PlayMusic(exploreEvent);
            }

            SetParameter(PARAM_MUSIC_LAYER, 2f); // Dense percussion layer
            SetParameter(PARAM_TENSION, 1f);
            SetCombatIntensity(1f);
        }

        private static void TransitionToPuzzle(string chapter)
        {
            if (ChapterMusicEvents.TryGetValue(chapter, out string eventPath))
            {
                PlayMusic(eventPath);
            }
            SetParameter(PARAM_MUSIC_LAYER, 0.5f); // Light puzzle layer
            SetParameter(PARAM_TENSION, 0.2f);
            SetCombatIntensity(0f);
        }

        private static void TransitionToCutscene(string chapter)
        {
            SetParameter(PARAM_TENSION, 0f);
            SetCombatIntensity(0f);
            // Cutscenes typically set their own music via Audio.SetMusic()
        }

        private static void TransitionToVictory(string chapter)
        {
            SetParameter(PARAM_MUSIC_LAYER, 0f);
            SetParameter(PARAM_TENSION, 0f);
            SetCombatIntensity(0f);
            Audio.Play("event:/game/general/heartgem_get"); // Temporary victory sound
        }

        private static void TransitionToTransition(string chapter)
        {
            SetParameter(PARAM_TENSION, 0.3f);
            SetCombatIntensity(0f);
        }

        #endregion

        #region Playback Helpers

        private static void PlayMusic(string eventPath)
        {
            if (string.IsNullOrEmpty(eventPath)) return;

            StopMusic();
            _currentMusicInstance = global::Celeste.Audio.Play(eventPath);
            _hasMusicInstance = true;
        }

        private static void StopMusic()
        {
            if (_hasMusicInstance)
            {
                global::Celeste.Audio.Stop(_currentMusicInstance);
                _hasMusicInstance = false;
            }
        }

        private static void StopAmbience()
        {
            if (_hasAmbienceInstance)
            {
                global::Celeste.Audio.Stop(_currentAmbienceInstance);
                _hasAmbienceInstance = false;
            }
        }

        private static void SetParameter(string name, float value)
        {
            if (_hasMusicInstance)
            {
                _currentMusicInstance.setParameterValue(name, value);
            }
        }

        private static string GetCurrentChapter(Level level)
        {
            if (level != null && level.Session != null && level.Session.MapData != null && level.Session.MapData.Filename != null)
            {
                string filename = level.Session.MapData.Filename;
                // Extract chapter from filename like "01_City" or "10_Ruins"
                if (filename.Length >= 2)
                {
                    return filename;
                }
            }

            // Fallback: try to infer from area
            if (level != null && level.Session != null && level.Session.Area.SID != null)
            {
                string sid = level.Session.Area.SID;
                int lastSlash = sid.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < sid.Length - 1)
                {
                    return sid.Substring(lastSlash + 1);
                }
            }

            return _currentChapter;
        }

        #endregion

        #region Level Hook Integration

        /// <summary>
        /// Call this when a level loads to set up appropriate music and ambience.
        /// </summary>
        public static void OnLevelLoad(Level level)
        {
            string chapter = GetCurrentChapter(level);
            if (chapter != null)
            {
                _currentChapter = chapter;
                SetMusicState(MusicState.Ambient, level);
                SetAmbienceForChapter(chapter);
            }
        }

        /// <summary>
        /// Call this when exiting a level to clean up audio state.
        /// </summary>
        public static void OnLevelExit()
        {
            StopMusic();
            StopAmbience();
            _currentMusicState = MusicState.Ambient;
            _currentAmbience = AmbienceState.None;
            _combatIntensity = 0f;
        }

        #endregion
    }
}
