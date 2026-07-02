// This file is intentionally a thin forwarder to the real DZ.AreaModeExtender.
// It exists only for namespace Celeste compatibility with older callers.
// All logic lives in Integration/Extensions/AreaModeExtender.cs (namespace DZ).
namespace Celeste
{
    /// <summary>
    /// Forwards to <see cref="global::DZ.AreaModeExtender"/> for callers in the
    /// Celeste namespace. Do not add new logic here.
    /// </summary>
    public static class AreaModeExtender
    {
        public const int MODE_NORMAL = global::DZ.AreaModeExtender.MODE_NORMAL;
        public const int MODE_HARD   = global::DZ.AreaModeExtender.MODE_1;
        public const int MODE_LUNAR  = global::DZ.AreaModeExtender.MODE_2;
        public const int MODE_SOLAR  = global::DZ.AreaModeExtender.MODE_DSIDE;
        public const int MODE_1      = global::DZ.AreaModeExtender.MODE_1;
        public const int MODE_2      = global::DZ.AreaModeExtender.MODE_2;
        public const int MODE_DSIDE  = global::DZ.AreaModeExtender.MODE_DSIDE;

        public static bool IsCustomMode(int mode) =>
            mode >= global::DZ.AreaModeExtender.MODE_2;

        public static bool IsOurMap(AreaData area) =>
            global::DZ.AreaModeExtender.IsOurMap(area);

        public static int GetSaveAreaModeCount(int areaId) =>
            global::DZ.AreaModeExtender.GetSaveAreaModeCount(areaId);

        public static bool TryParseMainSideSID(string sid, out string result, out int mode)
        {
            result = sid;
            mode   = MODE_NORMAL;
            return true;
        }
    }
}
