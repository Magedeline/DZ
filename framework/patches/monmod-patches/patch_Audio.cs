#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Celeste.Mod;
using Celeste.Mod.Core;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_Audio : Audio {

        // ── Exposed static state (mirrors Audio fields for mod-side access) ─────

        public static new Dictionary<string, EventDescription> cachedEventDescriptions;
        public static new Dictionary<Guid, string> cachedPaths;
        public static new Dictionary<Guid, string> cachedBankPaths;
        public static new Dictionary<string, EventDescription> cachedModEvents;

        // ── Init ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Patched Audio.Init: runs the vanilla initialisation then raises an event
        /// so mod systems (bank loader, GUID ingestion) can hook in after FMOD is ready.
        /// </summary>
        [MonoModReplace]
        public static void Init() {
            orig_Init();
            // Fire the post-init event so registered listeners can load custom banks.
            OnAudioInitialized?.Invoke();
        }

        /// <summary>
        /// Raised after <see cref="Audio.Init"/> completes successfully.
        /// Subscribers should load custom FMOD banks and ingest GUIDs here.
        /// </summary>
        public static event Action OnAudioInitialized;

        [MonoModIgnore]
        public static extern void orig_Init();

        // ── IngestNewBanks ───────────────────────────────────────────────────────

        /// <summary>
        /// Patched Audio.IngestNewBanks: runs the vanilla scan then raises an event
        /// so mods can ingest their own banks at the same time.
        /// </summary>
        [MonoModReplace]
        public static void IngestNewBanks() {
            orig_IngestNewBanks();
            OnIngestNewBanks?.Invoke();
        }

        /// <summary>
        /// Raised after <see cref="Audio.IngestNewBanks"/> completes.
        /// </summary>
        public static event Action OnIngestNewBanks;

        [MonoModIgnore]
        public static extern void orig_IngestNewBanks();

        // ── SetMusic ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Patched Audio.SetMusic: allows subscribers to redirect the event path
        /// before the vanilla method resolves it.
        /// </summary>
        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SetMusic(string path, bool startPlaying = true, bool allowFadeOut = true) {
            string mapped = MapEventPath?.Invoke(path) ?? path;
            return orig_SetMusic(mapped, startPlaying, allowFadeOut);
        }

        [MonoModIgnore]
        public static extern bool orig_SetMusic(string path, bool startPlaying, bool allowFadeOut);

        // ── SetAmbience ──────────────────────────────────────────────────────────

        /// <summary>
        /// Patched Audio.SetAmbience: allows subscribers to redirect the event path
        /// before the vanilla method resolves it.
        /// </summary>
        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SetAmbience(string path, bool startPlaying = true) {
            string mapped = MapEventPath?.Invoke(path) ?? path;
            return orig_SetAmbience(mapped, startPlaying);
        }

        [MonoModIgnore]
        public static extern bool orig_SetAmbience(string path, bool startPlaying);

        // ── Play ─────────────────────────────────────────────────────────────────

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance Play(string path) {
            return orig_Play(MapEventPath?.Invoke(path) ?? path);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_Play(string path);

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance Play(string path, string param, float value) {
            return orig_Play(MapEventPath?.Invoke(path) ?? path, param, value);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_Play(string path, string param, float value);

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance Play(string path, Vector2 position) {
            return orig_Play(MapEventPath?.Invoke(path) ?? path, position);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_Play(string path, Vector2 position);

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance Play(string path, Vector2 position, string param, float value) {
            return orig_Play(MapEventPath?.Invoke(path) ?? path, position, param, value);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_Play(string path, Vector2 position, string param, float value);

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance Play(string path, Vector2 position, string param, float value, string param2, float value2) {
            return orig_Play(MapEventPath?.Invoke(path) ?? path, position, param, value, param2, value2);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_Play(string path, Vector2 position, string param, float value, string param2, float value2);

        // ── CreateInstance ───────────────────────────────────────────────────────

        [MonoModReplace]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static EventInstance CreateInstance(string path, Vector2? position = null) {
            return orig_CreateInstance(MapEventPath?.Invoke(path) ?? path, position);
        }

        [MonoModIgnore]
        public static extern EventInstance orig_CreateInstance(string path, Vector2? position);

        // ── MapEventPath callback ────────────────────────────────────────────────

        /// <summary>
        /// Optional path-mapping delegate.  When set, every event path passed to
        /// <see cref="Play"/>, <see cref="SetMusic"/>, <see cref="SetAmbience"/>, and
        /// <see cref="CreateInstance"/> is run through this function before being
        /// forwarded to the original method.
        ///
        /// The delegate receives the raw path and returns the (possibly remapped) path.
        /// Return the input unchanged to leave a path unmodified.
        ///
        /// Example — redirect all vanilla music to pusheen variants:
        /// <code>
        /// patch_Audio.MapEventPath = path =>
        ///     path.StartsWith("event:/music/")
        ///         ? "event:/pusheen/music/" + path["event:/music/".Length..]
        ///         : path;
        /// </code>
        /// </summary>
        public static Func<string, string> MapEventPath;
    }
}
