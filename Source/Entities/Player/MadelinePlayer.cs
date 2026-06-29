using Microsoft.Xna.Framework;
using DZ.Nez;
using DZ.Entities.Core;

namespace DZ.Entities.Player;

/// <summary>
/// DZ Engine player entity for the ported Celeste entities.
/// Extends CelesteActor to provide Width/Height/Position and collision geometry.
/// </summary>
public class MadelinePlayer : CelesteActor
{
    public bool Dead { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Speed { get; set; }
    public Facings Facing { get; set; } = Facings.Right;
    public float Stamina { get; set; } = 110f;
    public bool DashAttacking { get; set; }
    public int Dashes { get; set; }
    public int MaxDashes { get; set; } = 1;

    public MadelinePlayer(Vector2 position) : base(position, 8f, 11f)
    {
    }

    public bool OnGround()
    {
        return false;
    }

    public void Die(Vector2 direction)
    {
        Dead = true;
    }

    public void RefillDash()
    {
        Dashes = MaxDashes;
    }

    public void RefillDash(bool reserve)
    {
        Dashes = MaxDashes;
    }
}
