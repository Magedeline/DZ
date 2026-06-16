using global::Celeste.Extensions.Core;
using Celeste.Utils;

namespace DZ;

/// <summary>
/// Serialized per-chapter respawn state used to resume a chapter at the latest save point.
/// </summary>
public class SavedChapterRespawnState
{
    public string LevelName { get; set; } = string.Empty;

    public float RespawnX { get; set; } = 0f;

    public float RespawnY { get; set; } = 0f;

    public string CharacterId { get; set; } = PlayerCharacterIds.Madeline;

    public string CheckpointId { get; set; } = string.Empty;

    public bool KirbyModeActive { get; set; } = false;

    public Vector2 RespawnPoint => new Vector2(RespawnX, RespawnY);
}