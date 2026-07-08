namespace DZ;

/// <summary>
/// Extended PlayerSpriteMode enum values for custom characters.
/// These map to sprite bank IDs in Sprites.xml
/// </summary>
public enum PlayerSpriteModeExt
{
    // Base Celeste modes (for reference)
    Madeline = 0,
    MadelineNoBackpack = 1,
    Badeline = 2,
    MadelineAsBadeline = 3,
    Playback = 4,
    Kirby = 100,
}

/// <summary>
/// Extension methods to add custom PlayerSpriteMode values to the Celeste enum.
/// This provides compatibility without modifying the base enum.
/// </summary>
public static class PlayerSpriteModeExtensions
{
    // Custom mode constants that can be used like PlayerSpriteMode values
    public const PlayerSpriteMode Chara = (PlayerSpriteMode)100;
    public const PlayerSpriteMode Madelinealt = PlayerSpriteMode.MadelineNoBackpack;
    public const PlayerSpriteMode Default = PlayerSpriteMode.Madeline;
    
    /// <summary>
    /// Gets the sprite bank ID for a given PlayerSpriteMode
    /// </summary>
    public static string GetSpriteBankId(this PlayerSpriteMode mode)
    {
        return mode switch
        {
            PlayerSpriteMode.Madeline => "DZ_player",
            PlayerSpriteMode.MadelineNoBackpack => "player_no_backpack",
            PlayerSpriteMode.Badeline => "DZ_badeline",
            PlayerSpriteMode.MadelineAsBadeline => "player_badeline",
            PlayerSpriteMode.Playback => "DZ_player_playback",
            (PlayerSpriteMode)100 => "kirby_player",
            _ => "DZ_player"
        };
    }
}
