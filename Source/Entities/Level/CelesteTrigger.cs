using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Base class for DZ Engine trigger entities.
/// Ported from Celeste's Trigger base class.
/// </summary>
public abstract class CelesteTrigger : Entity
{
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    public bool Triggered { get; protected set; }

    protected CelesteTrigger(Vector2 position, int width, int height) : base(position)
    {
        Width = width;
        Height = height;
    }

    public virtual void OnEnter(PlayerController player) { }
}
