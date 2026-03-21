namespace MaggyHelper.Entities;

/// <summary>
/// Entity that automatically sets the player's starting inventory when the level begins.
/// Unlike PlayerInventoryTrigger, this applies the inventory change immediately when the level loads,
/// making it suitable for setting initial player capabilities at the start of a level.
/// </summary>
[CustomEntity(ids: "MaggyHelper/StartingInventory")]
[Tracked]
public class StartingInventory : Entity
{
    public enum InventoryType
    {
        Default,
        Heart,
        KirbyPlayer,
        SayGoodbye,
        TitanTowerClimbing,
        Corruption,
        TheEnd
    }

    private readonly InventoryType inventoryType;
    private readonly bool debugVisible;
    private bool applied = false;

    public StartingInventory(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // Read the inventory type from entity data
        string typeString = data.Attr("inventoryType", "Default");
        if (!System.Enum.TryParse<InventoryType>(typeString, true, out inventoryType))
        {
            inventoryType = InventoryType.Default;
            IngesteLogger.Warn($"Invalid inventory type '{typeString}', defaulting to Default");
        }

        debugVisible = data.Bool("debugVisible", false);
        
        // Make the entity invisible by default unless debug mode is enabled
        Visible = debugVisible;
        
        // Add a small collider for editor selection
        Collider = new Hitbox(16, 16, -8, -8);
        
        IngesteLogger.Debug($"StartingInventory created with type: {inventoryType}");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        // Apply the inventory change immediately when added to the scene
        ApplyStartingInventory();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        // Ensure inventory is applied even if Added was missed
        if (!applied)
        {
            ApplyStartingInventory();
        }
    }

    private void ApplyStartingInventory()
    {
        if (applied)
            return;

        var level = Scene as Level;
        if (level == null)
            return;

        try
        {
            ApplyInventoryType(level, inventoryType);
            applied = true;

            // Play a subtle activation sound (optional)
            if (debugVisible)
            {
                Audio.Play("event:/char/kirby/inventory_change", Position);
                
                // Visual effect for debug mode
                level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 10, Center, Vector2.One * 16f);
            }

            IngesteLogger.Info($"Applied starting inventory type: {inventoryType}");
        }
        catch (System.Exception ex)
        {
            IngesteLogger.Error(ex, $"Error applying starting inventory type {inventoryType}");
        }
    }

    private static void ApplyInventoryType(Level level, InventoryType type)
    {
        switch (type)
        {
            case InventoryType.Heart:
                PlayerInventoryManager.EnableHeartPower(level);
                break;

            case InventoryType.KirbyPlayer:
                PlayerInventoryManager.EnableKirbyPlayer(level);
                break;

            case InventoryType.SayGoodbye:
                PlayerInventoryManager.EnableSayGoodbye(level);
                break;

            case InventoryType.TitanTowerClimbing:
                PlayerInventoryManager.EnableTitanTowerClimbing(level);
                break;

            case InventoryType.Corruption:
                PlayerInventoryManager.EnableCorruption(level);
                break;

            case InventoryType.TheEnd:
                PlayerInventoryManager.EnableTheEnd(level);
                break;

            case InventoryType.Default:
            default:
                PlayerInventoryManager.ResetToDefault(level);
                break;
        }
    }

    public override void Render()
    {
        base.Render();

        // Debug visualization when visible
        if (debugVisible && Engine.Commands.Open)
        {
            var color = inventoryType switch
            {
                InventoryType.Heart => Color.Red,
                InventoryType.KirbyPlayer => Color.Yellow,
                InventoryType.SayGoodbye => Color.Pink,
                InventoryType.TitanTowerClimbing => Color.Blue,
                InventoryType.Corruption => Color.Purple,
                InventoryType.TheEnd => Color.Gold,
                _ => Color.White
            };

            Draw.HollowRect(Collider, color);
            
            // Display inventory type text
            var font = Draw.DefaultFont;
            var text = $"Start: {inventoryType}";
            var textSize = font.MeasureString(text);
            
            Draw.Rect(Center.X - textSize.X / 2 - 2, Center.Y - textSize.Y / 2 - 2, 
                     textSize.X + 4, textSize.Y + 4, Color.Black * 0.8f);
            
            Draw.SpriteBatch.DrawString(font, text, Center - textSize / 2, color);
        }
    }
}



