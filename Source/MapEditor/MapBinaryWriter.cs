using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Writes a <see cref="BinaryPacker.Element"/> tree to a Celeste .bin file.
/// This mirrors the vanilla BinaryPacker.ToBinary format exactly so the
/// output can be loaded by <see cref="MapData"/> without any extra tooling.
/// </summary>
public static class MapBinaryWriter
{
    private const string InnerTextKey = "innerText";

    /// <summary>
    /// Write a BinaryPacker element tree to a .bin file.
    /// </summary>
    public static void Write(string filename, string packageName, BinaryPacker.Element root)
    {
        if (root == null)
            throw new ArgumentNullException(nameof(root));

        Directory.CreateDirectory(Path.GetDirectoryName(filename));

        // 1. Build string lookup table (same DFS order as vanilla)
        var lookup = new Dictionary<string, short>();
        short counter = 0;
        CollectStrings(root, lookup, ref counter);
        AddLookupValue(InnerTextKey, lookup, ref counter);

        using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fs);

        // 2. Header
        writer.Write("CELESTE MAP");
        writer.Write(packageName);
        writer.Write((short)lookup.Count);
        foreach (var kvp in lookup.OrderBy(kv => kv.Value))
        {
            writer.Write(kvp.Key);
        }

        // 3. Element tree
        WriteElement(writer, root, lookup);
        writer.Flush();
    }

    #region String Table

    private static void CollectStrings(BinaryPacker.Element element, Dictionary<string, short> lookup, ref short counter)
    {
        AddLookupValue(element.Name, lookup, ref counter);

        if (element.Attributes != null)
        {
            foreach (var attr in element.Attributes)
            {
                AddLookupValue(attr.Key, lookup, ref counter);
                if (attr.Value is string s)
                    AddLookupValue(s, lookup, ref counter);
            }
        }

        if (element.Children != null)
        {
            foreach (var child in element.Children)
                CollectStrings(child, lookup, ref counter);
        }
    }

    private static void AddLookupValue(string name, Dictionary<string, short> lookup, ref short counter)
    {
        if (string.IsNullOrEmpty(name) || lookup.ContainsKey(name))
            return;

        lookup.Add(name, counter);
        counter++;
    }

    #endregion

    #region Element Serialization

    private static void WriteElement(BinaryWriter writer, BinaryPacker.Element element, Dictionary<string, short> lookup)
    {
        int childCount = element.Children?.Count ?? 0;

        // Count attributes (vanilla ignores "_eid")
        int attrCount = 0;
        var attrList = new List<KeyValuePair<string, object>>();
        if (element.Attributes != null)
        {
            foreach (var attr in element.Attributes)
            {
                if (attr.Key == "_eid")
                    continue;
                attrCount++;
                attrList.Add(attr);
            }
        }

        // innerText handling: if element has innerText attribute and no real children,
        // it was originally stored as inner text in XML. In BinaryPacker.Element form,
        // inner text is carried via the "innerText" attribute.
        bool hasInnerTextAttr = element.Attributes != null && element.Attributes.ContainsKey(InnerTextKey);
        bool hasInnerText = hasInnerTextAttr && childCount == 0;
        if (hasInnerText)
            attrCount++; // we write it separately below

        // Name index
        writer.Write(lookup[element.Name]);

        // Attribute count
        writer.Write((byte)attrCount);

        // Write regular attributes
        foreach (var attr in attrList)
        {
            if (attr.Key == InnerTextKey)
                continue; // handled below

            writer.Write(lookup[attr.Key]);
            byte type = GetValueType(attr.Value, out object parsed);
            writer.Write(type);
            WriteValue(writer, type, parsed, lookup);
        }

        // Write innerText as the last attribute if present
        if (hasInnerText)
        {
            string text = element.Attributes[InnerTextKey]?.ToString() ?? "";
            writer.Write(lookup[InnerTextKey]);

            if (element.Name == "solids" || element.Name == "bg")
            {
                byte[] rle = RunLengthEncoding.Encode(text);
                writer.Write((byte)7);
                writer.Write((short)rle.Length);
                writer.Write(rle);
            }
            else
            {
                writer.Write((byte)6);
                writer.Write(text);
            }
        }

        // Children count
        writer.Write((short)childCount);

        if (element.Children != null)
        {
            foreach (var child in element.Children)
                WriteElement(writer, child, lookup);
        }
    }

    /// <summary>
    /// Determine the vanilla type code for a value.
    /// 0=bool, 1=byte, 2=short, 3=int, 4=float, 5=stringRef
    /// </summary>
    private static byte GetValueType(object value, out object parsed)
    {
        if (value is bool b)
        {
            parsed = b;
            return 0;
        }

        if (value is byte by)
        {
            parsed = by;
            return 1;
        }

        if (value is short s)
        {
            parsed = s;
            return 2;
        }

        if (value is int i)
        {
            parsed = i;
            return 3;
        }

        if (value is float f)
        {
            parsed = f;
            return 4;
        }

        // Try parsing strings into smaller numeric types when possible,
        // exactly like vanilla ParseValue does.
        if (value is string str)
        {
            if (bool.TryParse(str, out bool parsedBool))
            {
                parsed = parsedBool;
                return 0;
            }
            if (byte.TryParse(str, out byte parsedByte))
            {
                parsed = parsedByte;
                return 1;
            }
            if (short.TryParse(str, out short parsedShort))
            {
                parsed = parsedShort;
                return 2;
            }
            if (int.TryParse(str, out int parsedInt))
            {
                parsed = parsedInt;
                return 3;
            }
            if (float.TryParse(str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                parsed = parsedFloat;
                return 4;
            }

            parsed = str;
            return 5; // string reference (must be in lookup table)
        }

        // Fallback: convert to string and reference
        parsed = value?.ToString() ?? "";
        return 5;
    }

    private static void WriteValue(BinaryWriter writer, byte type, object value, Dictionary<string, short> lookup)
    {
        switch (type)
        {
            case 0:
                writer.Write((bool)value);
                break;
            case 1:
                writer.Write((byte)value);
                break;
            case 2:
                writer.Write((short)value);
                break;
            case 3:
                writer.Write((int)value);
                break;
            case 4:
                writer.Write((float)value);
                break;
            case 5:
                writer.Write(lookup[(string)value]);
                break;
            case 6:
                writer.Write((string)value);
                break;
            case 7:
                // RLE data: should already be byte[] from WriteElement
                if (value is byte[] rle)
                {
                    writer.Write((short)rle.Length);
                    writer.Write(rle);
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported attribute type {type}");
        }
    }

    #endregion
}
