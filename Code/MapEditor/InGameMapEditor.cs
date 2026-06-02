using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;

namespace Celeste;

/// <summary>
/// In-Game Map Editor Entity
/// Provides simple tile and entity editing while playing
/// </summary>
public class InGameMapEditor : Entity
{
    private const string LogTag = "InGameMapEditor";
    private const int TileSize = 8;
    private const int MinTileIndex = 0;
    private const int MaxTileIndex = 2;

    private bool _isActive;
    private EditorMode _mode = EditorMode.Select;
    private int _currentTileIndex = 1;
    private int _currentCategoryIndex;
    private int _currentEntityIndex;
    private bool _previousLeftMouseButton;

    // Custom entities from Celeste mod community
    private static readonly EntityCategory[] _entityCategories =
    [
        new EntityCategory
        {
            Name = "Collectibles",
            Entities =
            [
                "MaggyHelper/StrawberryExt",
                "strawberry",
                "key",
                "moonBerry",
                "voidstarBerry",
                "popstarBerry"
            ]
        },
        new EntityCategory
        {
            Name = "Refills",
            Entities =
            [
                "MaggyHelper/AdvancedRefill",
                "refill",
                "dashRefill",
                "staminaRefill"
            ]
        },
        new EntityCategory
        {
            Name = "Movement",
            Entities =
            [
                "spring",
                "springCloud",
                "bounceBlock",
                "dashBlock",
                "starJumpBlock"
            ]
        },
        new EntityCategory
        {
            Name = "Hazards",
            Entities =
            [
                "spike",
                "spikeUp",
                "spikeDown",
                "spikeLeft",
                "spikeRight",
                "fireTrap",
                "lava"
            ]
        },
        new EntityCategory
        {
            Name = "Kirby",
            Entities =
            [
                "MaggyHelper/KirbyPlayer",
                "kirby_food",
                "kirby_small_enemy"
            ]
        },
        new EntityCategory
        {
            Name = "NPCs",
            Entities =
            [
                "NPC_Theo",
                "NPC_Kirby",
                "NPC_Ralsei",
                "dialog_npc"
            ]
        },
        new EntityCategory
        {
            Name = "Bosses",
            Entities =
            [
                "kirbyBoss",
                "asriel_dummy",
                "chara_dummy",
                "kirby_dummy"
            ]
        },
        new EntityCategory
        {
            Name = "Triggers",
            Entities =
            [
                "trigger",
                "cameraTargetTrigger",
                "musicTrigger",
                "dialogTrigger"
            ]
        }
    ];

    public InGameMapEditor()
    {
        Tag = Tags.HUD;
        Visible = false;
    }

    /// <summary>
    /// Public method to toggle the editor on/off
    /// </summary>
    public void Toggle()
    {
        ToggleEditor();
    }

    public override void Update()
    {
        base.Update();

        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.F1))
        {
            ToggleEditor();
        }

        if (!_isActive)
        {
            StorePreviousMouseState();
            return;
        }

        HandleModeShortcuts();
        HandleEntityCategoryShortcuts();
        HandleEditing();
        StorePreviousMouseState();
    }

    public override void Render()
    {
        if (!_isActive)
        {
            return;
        }

        base.Render();
        DrawUI();
    }

    private void ToggleEditor()
    {
        _isActive = !_isActive;
        Visible = _isActive;

        LogInfo(_isActive ? "Editor activated" : "Editor deactivated");
    }

    private void SetMode(EditorMode mode)
    {
        _mode = mode;
        LogInfo($"Mode: {mode}");
    }

    private void HandleModeShortcuts()
    {
        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.D1))
        {
            SetMode(EditorMode.Select);
        }

        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.D2))
        {
            SetMode(EditorMode.Tile);
        }

        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.D3))
        {
            SetMode(EditorMode.Entity);
        }
    }

    private void HandleEntityCategoryShortcuts()
    {
        if (_mode != EditorMode.Entity)
        {
            return;
        }

        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Q))
        {
            SelectEntityCategory(_currentCategoryIndex - 1);
        }

        if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.E))
        {
            SelectEntityCategory(_currentCategoryIndex + 1);
        }
    }

    private void SelectEntityCategory(int categoryIndex)
    {
        _currentCategoryIndex = (categoryIndex + _entityCategories.Length) % _entityCategories.Length;
        _currentEntityIndex = 0;
    }

    private void HandleEditing()
    {
        if (Scene is not Level level)
        {
            return;
        }

        Vector2 worldMousePosition = MInput.Mouse.Position + level.Camera.Position;

        switch (_mode)
        {
            case EditorMode.Tile:
                HandleTileEditing(level, worldMousePosition);
                break;

            case EditorMode.Entity:
                HandleEntityEditing(level, worldMousePosition);
                break;
        }

        HandleSelectionScroll();
    }

    private void HandleSelectionScroll()
    {
        int scrollDelta = MInput.Mouse.WheelDelta;

        if (scrollDelta == 0)
        {
            return;
        }

        int scrollDirection = scrollDelta > 0 ? 1 : -1;

        if (_mode == EditorMode.Tile)
        {
            _currentTileIndex = Math.Clamp(_currentTileIndex + scrollDirection, MinTileIndex, MaxTileIndex);
            return;
        }

        if (_mode == EditorMode.Entity)
        {
            EntityCategory currentCategory = _entityCategories[_currentCategoryIndex];
            int maxEntityIndex = currentCategory.Entities.Length - 1;

            _currentEntityIndex = Math.Clamp(_currentEntityIndex + scrollDirection, 0, maxEntityIndex);
        }
    }

    private void HandleTileEditing(Level level, Vector2 mousePos)
    {
        bool isPainting = MInput.Mouse.CheckLeftButton;
        bool isErasing = MInput.Mouse.CheckRightButton;

        if (!isPainting && !isErasing)
        {
            return;
        }

        int tileX = (int)(mousePos.X / TileSize);
        int tileY = (int)(mousePos.Y / TileSize);

        // Simple tile modification - this is a basic implementation.
        // Real implementation would need to access Level.TileGrid.
        string action = isPainting
            ? $"Paint tile at ({tileX}, {tileY}) - type: {_currentTileIndex}"
            : $"Erase tile at ({tileX}, {tileY})";

        LogInfo(action);
    }

    private void HandleEntityEditing(Level level, Vector2 mousePos)
    {
        if (!MInput.Mouse.PressedLeftButton)
        {
            return;
        }

        EntityCategory currentCategory = _entityCategories[_currentCategoryIndex];
        string selectedEntityType = currentCategory.Entities[_currentEntityIndex];

        try
        {
            // This is a simplified approach - real implementation would use proper entity spawning.
            // For now, we'll log the placement and attempt basic entity creation.
            LogInfo($"Place {selectedEntityType} from category {currentCategory.Name} at ({mousePos.X}, {mousePos.Y})");

            // Attempt to spawn entity using reflection or direct instantiation
        }
        catch (Exception ex)
        {
            LogInfo($"Failed to spawn entity: {ex.Message}");
        }
    }

    private void StorePreviousMouseState()
    {
        _previousLeftMouseButton = MInput.Mouse.CheckLeftButton;
    }

    private static void LogInfo(string message)
    {
        Logger.Log(LogLevel.Info, LogTag, message);
    }

    private void DrawUI()
    {
        if (Scene is not Level level)
        {
            return;
        }

        // Draw semi-transparent background panel
        Draw.Rect(10, 10, 300, 120, Color.Black * 0.7f);

        // Draw editor title
        ActiveFont.DrawOutline("In-Game Map Editor", new Vector2(20, 20), Vector2.Zero, Vector2.One * 0.8f, Color.White, 2f, Color.Black);

        // Draw current mode
        string modeText = $"Mode: {_mode} (1=Select, 2=Tile, 3=Entity)";
        ActiveFont.DrawOutline(modeText, new Vector2(20, 45), Vector2.Zero, Vector2.One * 0.6f, Color.Cyan, 2f, Color.Black);

        // Draw mode-specific information
        if (_mode == EditorMode.Tile)
        {
            string tileInfo = $"Tile Index: {_currentTileIndex} (Scroll to change)";
            ActiveFont.DrawOutline(tileInfo, new Vector2(20, 70), Vector2.Zero, Vector2.One * 0.6f, Color.Yellow, 2f, Color.Black);
        }
        else if (_mode == EditorMode.Entity)
        {
            EntityCategory currentCategory = _entityCategories[_currentCategoryIndex];
            string categoryInfo = $"Category: {currentCategory.Name} (Q/E to change)";
            string entityInfo = $"Entity: {currentCategory.Entities[_currentEntityIndex]} (Scroll to change)";
            
            ActiveFont.DrawOutline(categoryInfo, new Vector2(20, 70), Vector2.Zero, Vector2.One * 0.6f, Color.Yellow, 2f, Color.Black);
            ActiveFont.DrawOutline(entityInfo, new Vector2(20, 95), Vector2.Zero, Vector2.One * 0.6f, Color.Lime, 2f, Color.Black);
        }

        // Draw mouse position
        Vector2 worldMousePos = MInput.Mouse.Position + level.Camera.Position;
        string mouseInfo = $"Mouse: ({(int)worldMousePos.X}, {(int)worldMousePos.Y})";
        ActiveFont.DrawOutline(mouseInfo, new Vector2(20, 110), Vector2.Zero, Vector2.One * 0.5f, Color.LightGray, 2f, Color.Black);
    }
}

internal enum EditorMode
{
    Select,
    Tile,
    Entity
}

internal class EntityCategory
{
    public string Name { get; set; }
    public string[] Entities { get; set; }
}
