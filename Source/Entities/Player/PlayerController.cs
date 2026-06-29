using Microsoft.Xna.Framework;
using DZ.Nez;
using Component = DZ.Nez.Component;

namespace DZ.Entities.Player;

/// <summary>
/// DZ Engine player controller component.
/// Attached to the player entity to provide velocity, dashing state, and damage API.
/// </summary>
public class PlayerController : Component
{
    public Vector2 Velocity { get; set; }
    public bool IsDashing { get; set; }

    public void TakeDamage(int damage, Vector2 knockback)
    {
    }
}
