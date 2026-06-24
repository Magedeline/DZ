using Microsoft.Xna.Framework;

namespace Celeste;

/// <summary>Serializable Vector3 data for YAML deserialization</summary>
internal class Vector3Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Vector3Data() { }
    public Vector3Data(float x, float y, float z) { X = x; Y = y; Z = z; }

    public Vector3 ToVector3() => new Vector3(X, Y, Z);
    public static Vector3Data FromVector3(Vector3 v) => new Vector3Data(v.X, v.Y, v.Z);
}

/// <summary>Serializable Vector2 data for YAML deserialization</summary>
internal class Vector2Data
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2Data() { }
    public Vector2Data(float x, float y) { X = x; Y = y; }

    public Vector2 ToVector2() => new Vector2(X, Y);
    public static Vector2Data FromVector2(Vector2 v) => new Vector2Data(v.X, v.Y);
}

/// <summary>Serializable Vector4 data for YAML deserialization</summary>
internal class Vector4Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }

    public Vector4Data() { }
    public Vector4Data(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }

    public Vector4 ToVector4() => new Vector4(X, Y, Z, W);
    public static Vector4Data FromVector4(Vector4 v) => new Vector4Data(v.X, v.Y, v.Z, v.W);
}

/// <summary>Serializable Color data for YAML deserialization</summary>
internal class ColorData
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public ColorData() { }
    public ColorData(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }

    public Color ToColor() => new Color(R, G, B, A);
    public static ColorData FromColor(Color c) => new ColorData(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
}
