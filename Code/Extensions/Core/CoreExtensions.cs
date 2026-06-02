using Monocle;

namespace Celeste.Extensions.Core
{
    /// <summary>
    /// Core extension helpers for MaggyHelper.
    /// </summary>
    public static class CoreExtensions
    {
        /// <summary>
        /// Gets the first Player entity in the scene, or null if none exists.
        /// </summary>
        public static Player GetPlayer(this Scene scene)
        {
            return scene?.Tracker?.GetEntity<Player>();
        }
    }
}
