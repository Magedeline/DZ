using Microsoft.Xna.Framework;

namespace Celeste
{
    /// <summary>
    /// Target center utility for positioning calculations.
    /// Used for UI and camera positioning.
    /// </summary>
    public static class TargetCenter
    {
        /// <summary>
        /// Get the center of the screen as a static property.
        /// </summary>
        public static Vector2 Position => new Vector2(960, 540);

        /// <summary>
        /// Get the center of the screen.
        /// </summary>
        public static Vector2 ScreenCenter()
        {
            return Position;
        }

        /// <summary>
        /// Get the center of a given rectangle.
        /// </summary>
        public static Vector2 GetCenter(Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}
