using Celeste;
using Monocle;

namespace Celeste;

/// <summary>
/// DEPRECATED: Skill-based Kirby player controller functionality has been integrated into Celeste.Entities.K_Player.
/// This class is kept for backward compatibility but all functionality now resides in the Player class.
/// New skill system: Air Drift, Cyclone Slash, Star Shot, Slide Tackle, Counter Stance, Dive Kick, Aqua Grapple
/// </summary>
public static class KirbyPlayerStateController
{
    /// <summary>
    /// DEPRECATED: No-op. Player skill states are now registered internally in the Player constructor.
    /// </summary>
    public static void Load()
    {
        Logger.Log(LogLevel.Info, "MaggyHelper",
            "[KirbyPlayerStateController] DEPRECATED - Skill system is now integrated into Player class");
    }

    /// <summary>
    /// DEPRECATED: No-op. Player skill states are managed internally by the Player class.
    /// </summary>
    public static void Unload()
    {
        // No-op - functionality moved to Player class
    }
}
