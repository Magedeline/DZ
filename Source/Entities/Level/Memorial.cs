using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Memorial stone decoration that shows an inscription when the player stands
/// close to it.  Ported from Celeste's Memorial.cs.
///
/// When the player enters the collision box the memorial text panel becomes
/// visible and a sound plays.  A dreaming-mode variant displays a floaty
/// animated text sprite instead.
/// </summary>
public class Memorial : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>Whether the inscription panel is currently displayed.</summary>
    public bool ShowText { get; private set; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly bool _dreamingMode;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _wasShowing;
    private MadelinePlayer? _player;

    // Hitbox dimensions matching Celeste's (60x80, offset -30,-60)
    private const float HitW = 60f, HitH = 80f, HitOX = -30f, HitOY = -60f;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">Base position in world space.</param>
    /// <param name="dreamingMode">
    /// True to use the dreaming-mode floaty text and sound effects.
    /// </param>
    public Memorial(Vector2 position, bool dreamingMode = false)
    {
        _spawnPosition = position;
        _dreamingMode  = dreamingMode;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // TODO: add Image renderer "scenery/memorial/memorial", origin = (w/2, h)
        // TODO: set render depth 100

        if (_dreamingMode)
        {
            // TODO: add animated sprite "scenery/memorial/floatytext", play "dreamy"
            //   position offset: (-width/2, -33)
        }

        // TODO: add BoxCollider (HitW x HitH), offset (HitOX, HitOY), trigger
        // TODO: if area == 1 && mode == Normal → Audio.SetMusicParam("end", 1)
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        _player ??= Entity.Scene?.FindEntityOfType<MadelinePlayer>();

        _wasShowing = ShowText;
        ShowText = _player != null && IsPlayerOverlapping();

        if (ShowText && !_wasShowing)
        {
            // Panel just appeared
            if (_dreamingMode)
            {
                // TODO: play sound: event:/ui/game/memorial_dream_text_in
                // TODO: start loop sound: event:/ui/game/memorial_dream_loop, param end = 0
            }
            else
            {
                // TODO: play sound: event:/ui/game/memorial_text_in
            }
        }
        else if (!ShowText && _wasShowing)
        {
            // Panel just hid
            if (_dreamingMode)
            {
                // TODO: play sound: event:/ui/game/memorial_dream_text_out
                // TODO: set loop sound param end = 1, then stop
            }
            else
            {
                // TODO: play sound: event:/ui/game/memorial_text_out
            }
        }

        // TODO: show/hide the MemorialText entity based on ShowText
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private bool IsPlayerOverlapping()
    {
        if (_player == null) return false;
        Vector2 ePos = Entity.Position;
        Vector2 pPos = _player.Position;
        return pPos.X > ePos.X + HitOX
            && pPos.X < ePos.X + HitOX + HitW
            && pPos.Y > ePos.Y + HitOY
            && pPos.Y < ePos.Y + HitOY + HitH;
    }
}
