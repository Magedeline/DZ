using System;
using System.IO;
using System.Text;

namespace Celeste.Mod.KIRBY_CELESTE.Security
{
    /// <summary>
    /// Build-time tool for packing text assets into binary format.
    /// Run this during CI/CD to convert Dialog/*.txt before packaging.
    /// </summary>
    public static class AssetPackerTool
    {
        private static readonly byte[] Key = new byte[] { 0xDE, 0x50, 0x10, 0x5A, 0x4E, 0xA5, 0xCE, 0x1E };

        /// <summary>
        /// Packs a text file to binary format.
        /// Usage: AssetPackerTool.PackFile("Dialog/English.txt", "Dialog/English.bin")
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
            byte[] encrypted = XorEncrypt(data, Key);
            
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(0x444D5A44); // Magic: "DMZD" (Desolo Zantas)
            writer.Write(1); // Version
            writer.Write(data.Length); // Original size
            
            // Asset name
            byte[] nameBytes = Encoding.ASCII.GetBytes(assetName);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            
            // Encrypted data
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
            if (magic != 0x444D5A44)
                throw new InvalidDataException($"Invalid magic: 0x{magic:X8}");
            
            int version = reader.ReadInt32();
            if (version != 1)
                throw new InvalidDataException($"Unsupported version: {version}");
            
            int originalSize = reader.ReadInt32();
            
            int nameLength = reader.ReadInt32();
            reader.ReadBytes(nameLength); // Skip name
            
            int dataLength = reader.ReadInt32();
            byte[] encrypted = reader.ReadBytes(dataLength);
            
            byte[] decrypted = XorEncrypt(encrypted, Key);
            return Encoding.UTF8.GetString(decrypted);
        }

        private static byte[] XorEncrypt(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
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
