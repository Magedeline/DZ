#nullable enable
namespace MaggyHelper.Entities;

// Note: Static class - cannot have [CustomEntity] attribute as Everest cannot instantiate static classes
public static class ClutterBlockGenerator
{
    // Constants to replace magic numbers
    private const int tile_size = 8;
    private const int max_tile_columns = 200;
    private const int max_tile_rows = 200;
    private const int segment_size = 50;
    private const int max_color_types = 4;
    
    private static Level? level;
    private static Tile[,]? tiles;
    private static readonly List<Point> active = new List<Point>();
    private static List<List<TextureSet>>? textures;
    private static int columns;
    private static int rows;
    private static readonly bool[] enabled = new bool[max_color_types];
    private static bool initialized;

    // Helper struct to replace tuple
    private struct TextureSetResult
    {
        public TextureSet? textureSet;
        public int color;
        
        public TextureSetResult(TextureSet? textureSet, int color)
        {
            this.textureSet = textureSet;
            this.color = color;
        }
    }

    public static void Init(Level lvl)
    {
        if (initialized || lvl == null)
            return;
            
        initialized = true;
        level = lvl;
        columns = level.Bounds.Width / tile_size;
        rows = level.Bounds.Height / tile_size + 1;

        // Initialize tiles array with proper bounds checking
        int actualColumns = Math.Min(columns, max_tile_columns);
        int actualRows = Math.Min(rows, max_tile_rows);
        tiles = new Tile[actualColumns, actualRows];
        
        initializeTiles(actualColumns, actualRows);
        initializeEnabledFlags();
        initializeTextures();
        initializeWallData();
    }

    private static void initializeTiles(int cols, int rows)
    {
        if (tiles == null) return;
        
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tiles[x, y].Color = -1;
                tiles[x, y].Block = null;
            }
        }
    }

    private static void initializeEnabledFlags()
    {
        if (level?.Session == null) return;
        
        for (int i = 0; i < enabled.Length; i++)
            enabled[i] = !level.Session.GetFlag($"oshiro_clutter_cleared_{i}");
    }

    private static void initializeTextures()
    {
        if (textures != null) return;
        
        textures = new List<List<TextureSet>>();
        
        for (int colorIndex = 0; colorIndex < max_color_types; colorIndex++)
        {
            var textureSetList = createTextureSetForColor((ClutterBlock.Colors)colorIndex);
            textureSetList.Sort((a, b) => -Math.Sign(a.Columns * a.Rows - b.Columns * b.Rows));
            textures.Add(textureSetList);
        }
    }

    private static List<TextureSet> createTextureSetForColor(ClutterBlock.Colors color)
    {
        var textureSetList = new List<TextureSet>();
        string colorPrefix = $"objects/resortclutter/{color}_";
        
        foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures(colorPrefix))
        {
            int columns = atlasSubtexture.Width / tile_size;
            int rows = atlasSubtexture.Height / tile_size;

            var existingSet = textureSetList.Find(ts => ts.Columns == columns && ts.Rows == rows);
            if (existingSet == null)
            {
                existingSet = new TextureSet { Columns = columns, Rows = rows };
                textureSetList.Add(existingSet);
            }
            
            existingSet.Textures.Add(atlasSubtexture);
        }
        
        return textureSetList;
    }

    private static void initializeWallData()
    {
        if (level?.SolidsData == null || tiles == null) return;
        
        Point levelSolidOffset = level.LevelSolidOffset;

        for (int x = 0; x < columns && x < max_tile_columns; x++)
        {
            for (int y = 0; y < rows && y < max_tile_rows; y++)
            {
                int solidX = levelSolidOffset.X + x;
                int solidY = levelSolidOffset.Y + y;
                
                if (solidX >= 0 && solidX < level.SolidsData.Columns &&
                    solidY >= 0 && solidY < level.SolidsData.Rows)
                {
                    tiles[x, y].Wall = level.SolidsData[solidX, solidY] != '0';
                }
            }
        }
    }

    public static void Dispose()
    {
        textures?.Clear();
        textures = null;
        tiles = null;
        active.Clear();
        level = null;
        initialized = false;
    }

    public static void Add(int x, int y, int w, int h, ClutterBlock.Colors color)
    {
        if (level == null || !initialized) return;
        
        Vector2 basePosition = new Vector2(level.Bounds.X, level.Bounds.Y);
        Vector2 position = basePosition + new Vector2(x, y) * tile_size;

        level.Add(new ClutterBlockBase(position, w * tile_size, h * tile_size, enabled[(int)color], color));

        if (!enabled[(int)color] || tiles == null) return;
        
        addActivePoints(x, y, w, h, color);
    }

    private static void addActivePoints(int x, int y, int w, int h, ClutterBlock.Colors color)
    {
        int startX = Math.Max(0, x);
        int endX = Math.Min(columns, x + w);
        int startY = Math.Max(0, y);
        int endY = Math.Min(rows, y + h);
        
        for (int pointX = startX; pointX < endX; pointX++)
        {
            for (int pointY = startY; pointY < endY; pointY++)
            {
                if (pointX < max_tile_columns && pointY < max_tile_rows)
                {
                    var point = new Point(pointX, pointY);
                    tiles![pointX, pointY].Color = (int)color;
                    active.Add(point);
                }
            }
        }
    }

    public static void Generate()
    {
        if (!initialized || level == null || tiles == null || textures == null)
            return;

        try
        {
            active.Shuffle();
            var clutterBlocks = new List<ClutterBlock>();
            Rectangle bounds = level.Bounds;

            generateClutterBlocks(clutterBlocks, bounds);
            setupClutterBlockRelationships(clutterBlocks);
            markGroundedBlocks(clutterBlocks);
        }
        finally
        {
            // Always cleanup, even if generation fails
            cleanup();
        }
    }

    private static void generateClutterBlocks(List<ClutterBlock> clutterBlocks, Rectangle bounds)
    {
        foreach (Point point in active)
        {
            if (tiles![point.X, point.Y].Block != null) continue;
            
            var result = findBestTextureSet(point);
            var textureSet = result.textureSet;
            var color = result.color;
            
            if (textureSet == null) continue;
            
            var clutterBlock = createClutterBlock(point, textureSet, color, bounds);
            assignBlockToTiles(point, textureSet, clutterBlock);
            
            clutterBlocks.Add(clutterBlock);
            level!.Add(clutterBlock);
        }
    }

    private static TextureSetResult findBestTextureSet(Point point)
    {
        int color = tiles![point.X, point.Y].Color;
        var colorTextures = textures![color];
        
        foreach (var textureSet in colorTextures)
        {
            if (canPlaceTextureSet(point, textureSet, color))
            {
                return new TextureSetResult(textureSet, color);
            }
        }
        
        return new TextureSetResult(null, color);
    }

    private static bool canPlaceTextureSet(Point point, TextureSet textureSet, int color)
    {
        if (point.X + textureSet.Columns > columns || point.Y + textureSet.Rows > rows)
            return false;

        for (int x = point.X; x < point.X + textureSet.Columns; x++)
        {
            for (int y = point.Y; y < point.Y + textureSet.Rows; y++)
            {
                var tile = tiles![x, y];
                if (tile.Block != null || tile.Color != color)
                    return false;
            }
        }
        
        return true;
    }

    private static ClutterBlock createClutterBlock(Point point, TextureSet textureSet, int color, Rectangle bounds)
    {
        Vector2 position = new Vector2(bounds.X, bounds.Y) + new Vector2(point.X, point.Y) * tile_size;
        MTexture texture = Calc.Random.Choose(textureSet.Textures);
        return new ClutterBlock(position, texture, (ClutterBlock.Colors)color);
    }

    private static void assignBlockToTiles(Point point, TextureSet textureSet, ClutterBlock clutterBlock)
    {
        for (int x = point.X; x < point.X + textureSet.Columns; x++)
        {
            for (int y = point.Y; y < point.Y + textureSet.Rows; y++)
            {
                tiles![x, y].Block = clutterBlock;
            }
        }
    }

    private static void setupClutterBlockRelationships(List<ClutterBlock> clutterBlocks)
    {
        for (int x = 0; x < columns && x < max_tile_columns; x++)
        {
            for (int y = 0; y < rows && y < max_tile_rows; y++)
            {
                var tile = tiles![x, y];
                if (tile.Block == null) continue;
                
                setupBlockSides(tile.Block, x, y);
                setupBlockGrounding(tile.Block, x, y);
            }
        }
    }

    private static void setupBlockSides(ClutterBlock block, int x, int y)
    {
        if (!block.TopSideOpen && (y == 0 || tiles![x, y - 1].Empty))
            block.TopSideOpen = true;
            
        if (!block.LeftSideOpen && (x == 0 || tiles![x - 1, y].Empty))
            block.LeftSideOpen = true;
            
        if (!block.RightSideOpen && (x == columns - 1 || tiles![x + 1, y].Empty))
            block.RightSideOpen = true;
    }

    private static void setupBlockGrounding(ClutterBlock block, int x, int y)
    {
        if (block.OnTheGround || y >= rows - 1) return;
        
        var belowTile = tiles![x, y + 1];
        
        if (belowTile.Wall)
        {
            block.OnTheGround = true;
        }
        else if (belowTile.Block != null && belowTile.Block != block && 
                 !block.HasBelow.Contains(belowTile.Block))
        {
            block.HasBelow.Add(belowTile.Block);
            block.Below.Add(belowTile.Block);
            belowTile.Block.Above.Add(block);
        }
    }

    private static void markGroundedBlocks(List<ClutterBlock> clutterBlocks)
    {
        foreach (var block in clutterBlocks)
        {
            if (block.OnTheGround)
                SetAboveToOnGround(block);
        }
    }

    private static void SetAboveToOnGround(ClutterBlock block)
    {
        foreach (var aboveBlock in block.Above)
        {
            if (!aboveBlock.OnTheGround)
            {
                aboveBlock.OnTheGround = true;
                SetAboveToOnGround(aboveBlock);
            }
        }
    }

    private static void cleanup()
    {
        initialized = false;
        level = null;
        active.Clear();
    }

    private struct Tile
    {
        public int Color;
        public bool Wall;
        public ClutterBlock? Block;

        public readonly bool Empty => !Wall && Color == -1;
    }

    private class TextureSet
    {
        public int Columns;
        public int Rows;
        public readonly List<MTexture> Textures = new List<MTexture>();
    }
}





