using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Celeste.Mod.DZ.Security
{
    /// <summary>
    /// Protects plain-text assets by converting them to an encrypted, compressed binary format.
    /// Uses GZip compression followed by XOR encryption (see <see cref="SecurityConstants"/>).
    /// Format is compatible with <see cref="AssetPackerTool"/> — both share the same DMZD header.
    /// </summary>
    public static class AssetProtector
    {
        /// <summary>
        /// Encrypts, compresses, and packs a text asset into binary format.
        /// </summary>
        public static byte[] PackAsset(string content, string assetName)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            byte[] compressed = GzipCompress(data);
            byte[] encrypted = SecurityConstants.XorEncrypt(compressed);
            byte[] nameBytes = Encoding.ASCII.GetBytes(assetName);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(SecurityConstants.Magic);
            writer.Write(SecurityConstants.FormatVersion);
            writer.Write(data.Length);          // original UTF-8 byte count (before compress+encrypt)
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            writer.Write(encrypted.Length);
            writer.Write(encrypted);
            return ms.ToArray();
        }

        /// <summary>
        /// Unpacks, decrypts, and decompresses a binary asset back to text.
        /// </summary>
        public static string UnpackAsset(byte[] packedData)
        {
            using var ms = new MemoryStream(packedData);
            using var reader = new BinaryReader(ms);

            int magic = reader.ReadInt32();
            if (magic != SecurityConstants.Magic)
                throw new InvalidDataException($"Invalid asset magic: 0x{magic:X8}");

            int version = reader.ReadInt32();
            if (version != SecurityConstants.FormatVersion)
                throw new InvalidDataException($"Unsupported asset version: {version}");

            int originalSize = reader.ReadInt32();
            int nameLength = reader.ReadInt32();
            reader.ReadBytes(nameLength); // skip asset name

            int dataLength = reader.ReadInt32();
            byte[] encrypted = reader.ReadBytes(dataLength);
            byte[] compressed = SecurityConstants.XorEncrypt(encrypted);
            byte[] data = GzipDecompress(compressed, originalSize);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Verifies raw asset bytes against an expected SHA-256 hash.
        /// Returns <c>true</c> when <paramref name="expectedHash"/> is null or empty (no check required).
        /// </summary>
        public static bool VerifyAssetIntegrity(byte[] data, string expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash))
                return true;
            return ComputeHash(data).Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes the SHA-256 hash of <paramref name="data"/> as a lowercase hex string.
        /// </summary>
        public static string ComputeHash(byte[] data)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static byte[] GzipCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gz = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
                gz.Write(data, 0, data.Length);
            return output.ToArray();
        }

        private static byte[] GzipDecompress(byte[] data, int expectedSize)
        {
            using var input = new MemoryStream(data);
            using var gz = new GZipStream(input, CompressionMode.Decompress);
            var output = new byte[expectedSize];
            int read = 0;
            while (read < expectedSize)
            {
                int n = gz.Read(output, read, expectedSize - read);
                if (n == 0) break;
                read += n;
            }
            return output;
        }
    }
}
