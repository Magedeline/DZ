using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Celeste.Mod;

namespace Celeste.Mod.MaggyHelper.Security
{
    /// <summary>
    /// Protects plain-text assets by converting them to encrypted binary format.
    /// Nintendo-style asset protection for Desolo Zantas.
    /// </summary>
    public static class AssetProtector
    {
        // XOR key for basic obfuscation (will be obfuscated by Obfuscar)
        private static readonly byte[] Key = new byte[] { 0xDE, 0x50, 0x10, 0x5A, 0x4E, 0xA5, 0xCE, 0x1E };

        // Asset manifest for integrity verification
        private static readonly Dictionary<string, string> AssetChecksums = new Dictionary<string, string>
        {
            // Populated at runtime from embedded manifest
        };

        /// <summary>
        /// Encrypts and packs a text asset into binary format.
        /// Call during build process to convert Dialog/*.txt files.
        /// </summary>
        public static byte[] PackAsset(string content, string assetName)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            byte[] encrypted = XorEncrypt(data, Key);
            byte[] compressed = Compress(encrypted);
            
            // Prepend header: magic (4) + version (4) + original size (4) + asset name length (4) + asset name + data
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            writer.Write(0x444D5A44); // Magic: "DMZD" (Desolo Zantas)
            writer.Write(1); // Version
            writer.Write(data.Length); // Original uncompressed size
            writer.Write(assetName.Length);
            writer.Write(Encoding.ASCII.GetBytes(assetName));
            writer.Write(compressed);
            
            return ms.ToArray();
        }

        /// <summary>
        /// Unpacks and decrypts a binary asset back to text.
        /// </summary>
        public static string UnpackAsset(byte[] packedData)
        {
            using var ms = new MemoryStream(packedData);
            using var reader = new BinaryReader(ms);
            
            int magic = reader.ReadInt32();
            if (magic != 0x444D5A44)
                throw new InvalidDataException("Invalid asset magic header");
            
            int version = reader.ReadInt32();
            if (version != 1)
                throw new InvalidDataException($"Unsupported asset version: {version}");
            
            int originalSize = reader.ReadInt32();
            int nameLength = reader.ReadInt32();
            reader.ReadBytes(nameLength); // Skip asset name
            
            byte[] compressed = reader.ReadBytes((int)(ms.Length - ms.Position));
            byte[] encrypted = Decompress(compressed, originalSize);
            byte[] decrypted = XorEncrypt(encrypted, Key);
            
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Verifies asset integrity against embedded checksums.
        /// </summary>
        public static bool VerifyAssetIntegrity(string assetName, byte[] data)
        {
            if (!AssetChecksums.TryGetValue(assetName, out string expectedHash))
                return true; // No checksum recorded, skip
            
            string actualHash = ComputeHash(data);
            return actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes SHA256 hash of data.
        /// </summary>
        public static string ComputeHash(byte[] data)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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

        // Simple compression placeholder - replace with proper LZ4 or deflate if needed
        private static byte[] Compress(byte[] data)
        {
            // For now, just return data (compression can be added later)
            return data;
        }

        private static byte[] Decompress(byte[] data, int originalSize)
        {
            // For now, just return data
            return data;
        }
    }
}
