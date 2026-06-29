using MonocleEngine = Monocle.Engine;

namespace DZ.Nez
{
    /// <summary>
    /// Bridges DZ.Nez namespace to Monocle.Engine for ported code that
    /// references Engine.DeltaTime, Engine.Commands, etc. from within DZ.* namespaces.
    /// The type member takes priority over the DZ.Nez namespace in name lookup.
    /// </summary>
    public static class Engine
    {
        public static float DeltaTime => MonocleEngine.DeltaTime;
        public static float RawDeltaTime => MonocleEngine.RawDeltaTime;
        public static float TimeRate
        {
            get => MonocleEngine.TimeRate;
            set => MonocleEngine.TimeRate = value;
        }
        public static float FreezeTimer
        {
            get => MonocleEngine.FreezeTimer;
            set => MonocleEngine.FreezeTimer = value;
        }
        public static float Freezetime
        {
            get => MonocleEngine.FreezeTimer;
            set => MonocleEngine.FreezeTimer = value;
        }
        public static int Width => MonocleEngine.Width;
        public static int Height => MonocleEngine.Height;
        public static int ViewWidth => MonocleEngine.ViewWidth;
        public static int ViewHeight => MonocleEngine.ViewHeight;
        public static Monocle.Scene? Scene => MonocleEngine.Scene;
        public static Monocle.Commands? Commands => MonocleEngine.Commands;
        public static Monocle.Pooler? Pooler => MonocleEngine.Pooler;
        public static Monocle.Scene? Instance => MonocleEngine.Scene;
    }
}
