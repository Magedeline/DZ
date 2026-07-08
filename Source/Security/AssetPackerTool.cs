using System;
using System.IO;
using System.Text;

namespace Celeste.Mod.DZ.Security
{
    /// <summary>
    /// Shared constants and primitives used by all Security classes.
    /// Binary asset format (version 1):
    ///   [4]  magic        = 0x444D5A44 ("DMZD")
    ///   [4]  version      = 1
    ///   [4]  originalSize = UTF-8 byte count before encryption
    ///   [4]  nameLength
    ///   [n]  assetName    = ASCII bytes
    ///   [4]  dataLength
    ///   [n]  data         = XOR-encrypted (+ optional GZip) bytes
    /// </summary>
    internal static class SecurityConstants
    {
        internal const int Magic = 0x444D5A44;
        internal const int FormatVersion = 1;
        internal static readonly byte[] XorKey = { 0xDE, 0x50, 0x10, 0x5A, 0x4E, 0xA5, 0xCE, 0x1E };

        internal static byte[] XorEncrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ XorKey[i % XorKey.Length]);
            return result;
        }
    }

    /// <summary>
    /// Build-time tool for packing text assets into binary format.
    /// Usage: AssetPackerTool.PackFile("Dialog/English.txt", "Dialog/English.bin")
    /// </summary>
    public static class AssetPackerTool
    {
        /// <summary>
        /// Packs a text file on disk to binary format.
        /// </summary>
        public static void PackFile(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException($"Input file not found: {inputPath}");

            string content = File.ReadAllText(inputPath, Encoding.UTF8);
            byte[] packed = PackContent(content, Path.GetFileName(inputPath));
            File.WriteAllBytes(outputPath, packed);
            Console.WriteLine($"[AssetPacker] Packed: {inputPath} -> {outputPath} ({packed.Length} bytes)");
        }

        /// <summary>
        /// Packs string content to binary format.
        /// </summary>
        public static byte[] PackContent(string content, string assetName)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            byte[] encrypted = SecurityConstants.XorEncrypt(data);
            byte[] nameBytes = Encoding.ASCII.GetBytes(assetName);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(SecurityConstants.Magic);
            writer.Write(SecurityConstants.FormatVersion);
            writer.Write(data.Length);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            writer.Write(encrypted.Length);
            writer.Write(encrypted);
            return ms.ToArray();
        }

        /// <summary>
        /// Unpacks binary content back to string.
        /// </summary>
        public static string UnpackContent(byte[] packedData)
        {
            using var ms = new MemoryStream(packedData);
            using var reader = new BinaryReader(ms);

            int magic = reader.ReadInt32();
            if (magic != SecurityConstants.Magic)
                throw new InvalidDataException($"Invalid magic: 0x{magic:X8}");

            int version = reader.ReadInt32();
            if (version != SecurityConstants.FormatVersion)
                throw new InvalidDataException($"Unsupported version: {version}");

            reader.ReadInt32(); // originalSize — not needed for XOR-only decode
            int nameLength = reader.ReadInt32();
            reader.ReadBytes(nameLength); // skip name

            int dataLength = reader.ReadInt32();
            byte[] encrypted = reader.ReadBytes(dataLength);
            return Encoding.UTF8.GetString(SecurityConstants.XorEncrypt(encrypted));
        }

        /// <summary>
        /// CLI entry point for build scripts.
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: AssetPackerTool <input.txt> <output.bin>");
                return;
            }
            PackFile(args[0], args[1]);
        }
    }
}
