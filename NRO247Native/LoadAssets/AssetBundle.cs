using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using SkiaSharp;

namespace LoadAssets
{
    public static class AssetBundle
    {
        // Cache byte[] và SKBitmap
        private static readonly Dictionary<string, byte[]> byteCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, SKBitmap> bitmapCache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Load toàn bộ bundle từ file assets.dat
        /// </summary>
        /// <param name="bundlePath">Đường dẫn file assets.dat - copy file assets.dat vào bin để debug</param>
        public static void LoadBundle(string bundlePath)
        {
            if (!File.Exists(bundlePath))
                throw new FileNotFoundException("Bundle not found", bundlePath);

            using var fs = File.OpenRead(bundlePath);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                using var entryStream = entry.Open();
                using var ms = new MemoryStream();
                entryStream.CopyTo(ms);
                byteCache[entry.FullName] = ms.ToArray();
            }

            Console.WriteLine($"Bundle loaded: {byteCache.Count} files");
        }

        /// <summary>
        /// Lấy byte[] từ bundle
        /// </summary>
        public static byte[] GetBytes(string path)
        {
            if (byteCache.TryGetValue(path, out var data))
                return data;

            throw new Exception("File not found in bundle: " + path);
        }

        /// <summary>
        /// Lấy SKBitmap từ PNG/JPG trong bundle
        /// </summary>
        public static SKBitmap GetBitmap(string path)
        {
            if (bitmapCache.TryGetValue(path, out var value))
            {
                return value;
            }
            byte[] bytes = GetBytes(path);
            value = SKBitmap.Decode(bytes) ?? throw new Exception("Failed to decode bitmap: " + path);
            bitmapCache[path] = value;
            return value;
        }

        /// <summary>
        /// Giải phóng toàn bộ cache
        /// </summary>
        public static void DisposeAll()
        {
            foreach (var bmp in bitmapCache.Values)
                bmp?.Dispose();

            bitmapCache.Clear();
            byteCache.Clear();
        }
    }
}
