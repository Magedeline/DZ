using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Stub for PlayerCharacter.
    /// </summary>
    public class PlayerCharacter : Entity
    {
        public PlayerCharacter(Vector2 position) : base(position)
        {
        }

        public static string NormalizeId(string id)
        {
            return id;
        }
    }
}
