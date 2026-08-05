namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// A 0-based [x, y] char grid, the exchange format every generator produces.
/// '0' means air; any other char is a Celeste tileset id.
///
/// Mirrors pcg_toolkit.lua's newGrid/getTile. Indexing matches Rysy's
/// <see cref="Rysy.Graphics.Tilegrid"/> (x then y, both 0-based), and matches the
/// Lua grid -> Loenn matrix mapping, which writes grid[x][y] to matrix row y+1.
/// Row 0 is therefore the TOP row of the room in both implementations.
/// </summary>
public sealed class PcgGrid {
    public int Width { get; }
    public int Height { get; }

    private readonly char[,] _tiles;

    public PcgGrid(int width, int height, char fill = '0') {
        Width = width;
        Height = height;
        _tiles = new char[width, height];
        if (fill != '\0')
            Fill(fill);
    }

    public char this[int x, int y] {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }

    public void Fill(char tile) {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            _tiles[x, y] = tile;
    }

    /// <summary>Out-of-bounds reads return '0', matching pcg.getTile.</summary>
    public char SafeGet(int x, int y)
        => x < 0 || y < 0 || x >= Width || y >= Height ? '0' : _tiles[x, y];

    /// <summary>Solid border on all four edges, as the Unity generators expect.</summary>
    public void FillBorder(char tile) {
        for (var x = 0; x < Width; x++) {
            _tiles[x, 0] = tile;
            _tiles[x, Height - 1] = tile;
        }
        for (var y = 0; y < Height; y++) {
            _tiles[0, y] = tile;
            _tiles[Width - 1, y] = tile;
        }
    }

    /// <summary>Mirrors the grid top-to-bottom, in place.</summary>
    public void FlipVertical() {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height / 2; y++)
            (_tiles[x, y], _tiles[x, Height - 1 - y]) = (_tiles[x, Height - 1 - y], _tiles[x, y]);
    }

    public int CountSolid() {
        var n = 0;
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            if (_tiles[x, y] != '0')
                n++;
        return n;
    }
}
