using Microsoft.Xna.Framework;

namespace Celeste.Entities
{
    public partial class TeleportPipeData
    {
        public string DestinationRoom { get; set; }
        public Vector2 DestinationPosition { get; set; }
        public string PipeIdentifier { get; set; }
        public bool IsActive { get; set; } = true;

        public TeleportPipeData()
        {
        }

        public TeleportPipeData(string destinationRoom, Vector2 destinationPosition, string pipeId)
        {
            DestinationRoom = destinationRoom;
            DestinationPosition = destinationPosition;
            PipeIdentifier = pipeId;
        }
    }
}
