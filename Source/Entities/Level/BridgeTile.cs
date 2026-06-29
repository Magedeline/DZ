using Microsoft.Xna.Framework;
using DZ.Nez;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// A single tile of a <see cref="Bridge"/>.
///
/// When <see cref="Fall"/> is called the tile begins to drop with gravity and
/// rotates as it falls.  After it leaves the screen it removes itself from
/// the scene.
///
/// The tile also acts as a JumpThru while it is still solid (before falling).
/// Sprite rendering is TODO; for now draws a placeholder rectangle.
/// </summary>
public class BridgeTile : DZ.Entities.Core.CelesteJumpThru
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float Gravity     = 400f;
    private const float OffscreenY  = 2000f; // remove self beyond this Y

    // ── Source rect (for sprite slicing, TODO) ────────────────────────────────

    private readonly Microsoft.Xna.Framework.Rectangle _srcRect;

    // ── Fall state ────────────────────────────────────────────────────────────

    private bool  _falling;
    private float _fallDelay;
    private float _speedY;
    private float _rotation;
    private float _rotationSpeed;

    // ── Constructor ───────────────────────────────────────────────────────────

    public BridgeTile(Vector2 position, Microsoft.Xna.Framework.Rectangle srcRect)
        : base(position, srcRect.Width)
    {
        _srcRect = srcRect;
        Name     = "BridgeTile";
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>Starts the falling sequence, optionally after a delay.</summary>
    public void Fall(float delay = 0f)
    {
        if (_falling) return;
        _falling       = true;
        _fallDelay     = delay;
        _rotationSpeed = DZ.Nez.Random.Range(-3f, 3f);
        Collidable     = false;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        if (!_falling)
        {
            base.Update();
            return;
        }

        float dt = Time.DeltaTime;

        if (_fallDelay > 0f)
        {
            _fallDelay -= dt;
            return;
        }

        _speedY    += Gravity * dt;
        _rotation  += _rotationSpeed * dt;
        Position   += new Vector2(0f, _speedY * dt);

        if (Position.Y > OffscreenY)
            Destroy();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        // TODO: draw actual bridge tile sprite slice at _rotation
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y, _srcRect.Width, Math.Min(_srcRect.Height, 8f), Color.SaddleBrown);
    }
}
