using Celeste.Mod.DZ;
using Monocle;

namespace DZ;

/// <summary>
/// Hooks for integrating chapter completion metadata from .DZ.meta.yaml files.
/// Applies custom music events and title text from DZMapMetadata to the AreaComplete screen.
/// </summary>
internal static class ChapterCompletionHooks
{
    private static bool _loaded = false;

    public static void Load()
    {
        if (_loaded) return;

        // Hook into AreaComplete.Begin to apply custom metadata
        On.Celeste.AreaComplete.Begin += OnAreaCompleteBegin;

        _loaded = true;
    }

    public static void Unload()
    {
        if (!_loaded) return;

        On.Celeste.AreaComplete.Begin -= OnAreaCompleteBegin;

        _loaded = false;
    }

    private static void OnAreaCompleteBegin(On.Celeste.AreaComplete.orig_Begin orig, AreaComplete self)
    {
        orig(self);

        // Apply custom music from metadata if available
        ApplyCustomMusic(self, self.Session);
    }

    private static void ApplyCustomMusic(AreaComplete areaComplete, Session session)
    {
        if (session == null) return;

        // Get metadata for the current area
        var metadata = DZMapMetadataRegistry.GetBySid(session.Area.SID);
        if (metadata == null || metadata.MusicBySide == null || metadata.MusicBySide.Count == 0) return;

        // Determine which music event to use based on mode
        int mode = (int)session.Area.Mode;
        int musicIndex = mode switch
        {
            AreaModeExtender.MODE_NORMAL => 0,
            AreaModeExtender.MODE_1  => 1,
            AreaModeExtender.MODE_2  => 2,
            AreaModeExtender.MODE_DSIDE  => 3,
            _                            => 0
        };

        // Apply the music event if the index is valid
        if (musicIndex >= 0 && musicIndex < metadata.MusicBySide.Count)
        {
            string musicEvent = metadata.MusicBySide[musicIndex];
            if (!string.IsNullOrEmpty(musicEvent))
            {
                Audio.Play(musicEvent);
            }
        }
    }
}
