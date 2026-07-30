namespace Celeste.Entities.Chapters
{
    /// <summary>
    /// Which existing input activates a chapter power-up. Celeste has no spare button,
    /// so every ability entity exposes this and the mapper picks whichever binding is
    /// free in that room.
    ///
    /// Grab is the default: it is only ever busy while the player is on a wall, and the
    /// ability controllers skip it in <see cref="Celeste.Player.StClimb"/> for that reason.
    /// </summary>
    public enum AbilityButton
    {
        Grab,
        Dash,
        Jump,
        Talk
    }
}
