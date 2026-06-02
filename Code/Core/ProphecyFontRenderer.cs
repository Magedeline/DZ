using Monocle;

namespace Celeste;

/// <summary>
/// Sprite-based bitmap font renderer for the ancient prophecy font.
/// Loads glyph tiles from the "desolo_font/font_prophecy" atlas and
/// draws text character-by-character with optional outline and wavy effects.
/// </summary>
public class ProphecyFontRenderer
{
    private MTexture atlas;
    private readonly Dictionary<char, int> charMap = new(64);
    private const int GlyphWidth = 32;
    private const int GlyphHeight = 32;
    private const int Columns = 16;
    private const float DefaultSpacing = 2f;

    public ProphecyFontRenderer()
    {
        atlas = GFX.Game["desolo_font/font_prophecy"];
        BuildCharMap();
    }

    private void BuildCharMap()
    {
        string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?;:'\"\\-+/ ";
        for (int i = 0; i < upper.Length; i++)
            charMap[upper[i]] = i;

        // Lowercase aliases -> same indices as uppercase
        for (char c = 'a'; c <= 'z'; c++)
            charMap[c] = c - 'a';
    }

    private MTexture GetGlyph(char c)
    {
        if (!charMap.TryGetValue(c, out int index))
            return null;

        int col = index % Columns;
        int row = index / Columns;
        return atlas.GetSubtexture(col * GlyphWidth, row * GlyphHeight, GlyphWidth, GlyphHeight);
    }

    public Vector2 MeasureString(string text, float scale)
    {
        if (string.IsNullOrEmpty(text))
            return Vector2.Zero;

        float width = 0f;
        for (int i = 0; i < text.Length; i++)
        {
            width += GlyphWidth * scale;
            if (i < text.Length - 1)
                width += DefaultSpacing * scale;
        }

        return new Vector2(width, GlyphHeight * scale);
    }

    public void Draw(string text, Vector2 position, Vector2 justify, float scale, Color color)
    {
        if (string.IsNullOrEmpty(text) || atlas == null)
            return;

        Vector2 size = MeasureString(text, scale);
        Vector2 origin = position - size * justify;

        float x = origin.X;
        for (int i = 0; i < text.Length; i++)
        {
            MTexture glyph = GetGlyph(text[i]);
            glyph?.Draw(new Vector2(x, origin.Y), Vector2.Zero, color, scale);
            x += (GlyphWidth + DefaultSpacing) * scale;
        }
    }

    public void DrawOutline(string text, Vector2 position, Vector2 justify, float scale, Color color, float outlineWidth, Color outlineColor)
    {
        if (string.IsNullOrEmpty(text) || atlas == null)
            return;

        // Draw outline at 8 cardinal+diagonal offsets
        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0)
                    continue;

                Vector2 offset = new Vector2(ox * outlineWidth, oy * outlineWidth);
                Draw(text, position + offset, justify, scale, outlineColor);
            }
        }

        // Draw main text on top
        Draw(text, position, justify, scale, color);
    }

    /// <summary>
    /// Draws text with a per-character sinusoidal Y-offset for an arcane wave effect.
    /// </summary>
    public void DrawWavy(string text, Vector2 position, Vector2 justify, float scale, Color color, float time, float amplitude = 3f, float frequency = 4f)
    {
        if (string.IsNullOrEmpty(text) || atlas == null)
            return;

        Vector2 size = MeasureString(text, scale);
        Vector2 origin = position - size * justify;

        float x = origin.X;
        for (int i = 0; i < text.Length; i++)
        {
            float yOffset = amplitude * (float)Math.Sin(time * frequency + i * 0.5f);
            MTexture glyph = GetGlyph(text[i]);
            glyph?.Draw(new Vector2(x, origin.Y + yOffset), Vector2.Zero, color, scale);
            x += (GlyphWidth + DefaultSpacing) * scale;
        }
    }

    /// <summary>
    /// Draws wavy text with an outline for readability.
    /// </summary>
    public void DrawWavyOutline(string text, Vector2 position, Vector2 justify, float scale, Color color, float outlineWidth, Color outlineColor, float time, float amplitude = 3f, float frequency = 4f)
    {
        if (string.IsNullOrEmpty(text) || atlas == null)
            return;

        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0)
                    continue;

                Vector2 offset = new Vector2(ox * outlineWidth, oy * outlineWidth);
                DrawWavy(text, position + offset, justify, scale, outlineColor, time, amplitude, frequency);
            }
        }

        DrawWavy(text, position, justify, scale, color, time, amplitude, frequency);
    }
}
