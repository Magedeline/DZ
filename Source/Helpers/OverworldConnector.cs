using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Connector for overworld map transitions.
    /// Handles connections between different areas in the overworld.
    /// </summary>
    public class OverworldConnector : Entity
    {
        public string FromArea { get; set; }
        public string ToArea { get; set; }
        public new Vector2 Position { get; set; }

        public OverworldConnector(string fromArea, string toArea, Vector2 position)
        {
            FromArea = fromArea;
            ToArea = toArea;
            Position = position;
        }

        /// <summary>
        /// Check if this connector can be used.
        /// </summary>
        public bool CanConnect()
        {
            // Stub implementation - returns true by default
            return true;
        }

        /// <summary>
        /// Execute the connection transition.
        /// </summary>
        public void Connect()
        {
            // Stub implementation - does nothing
        }

        /// <summary>
        /// Enable the Maggy marker for this connector.
        /// </summary>
        public void EnableMaggyMarker()
        {
            // Stub implementation - does nothing
        }
    }
}
