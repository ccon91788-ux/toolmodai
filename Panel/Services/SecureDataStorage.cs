using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Panel.Repositories;

namespace Panel.Services;

/// <summary>
/// Lưu license_key vào SQLite (như cũ) + file license.key cùng thư mục exe (backup bền vững).
/// File license.key KHÔNG BAO GIỜ bị xóa → mở lại Panel sau khi bị kick vẫn tự đọc key.
/// </summary>
public static class SecureDataStorage
{
    // ── Đường dẫn file license.key nằm cùng thư mục exe ──────────────────
    private static readonly string _licenseFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "license.key");

    // ── License Key ────────────────────────────────────────────────────────

    /// <summary>
    /// Lưu key vào cả SQLite lẫn file license.key (backup bền vững).
    /// </summary>
    public static void SaveKey(string licenseKey)
    {
        // 1. Lưu vào SQLite (giữ tương thích cũ)
        try
        {
            string connStr = DatabaseHelper.GetConnectionString();
            if (!string.IsNullOrEmpty(connStr))
            {
                using var connection = new SqliteConnection(connStr);
                connection.Open();
                using var command = new SqliteCommand("UPDATE AppConfig SET LicenseKey = @key WHERE Id = 1", connection);
                command.Parameters.AddWithValue("@key", licenseKey);
                command.ExecuteNonQuery();
            }
        }
        catch { /* Bỏ qua, không crash */ }

        // 2. Lưu ra file license.key — KHÔNG BAO GIỜ BỊ XÓA
        SaveKeyToFile(licenseKey);
    }

    /// <summary>
    /// Đọc key: ưu tiên SQLite, nếu rỗng thì fallback đọc file license.key.
    /// </summary>
    public static string LoadKey()
    {
        // 1. Thử đọc SQLite trước
        string fromDb = LoadKeyFromDb();
        if (!string.IsNullOrEmpty(fromDb))
            return fromDb;

        // 2. Fallback: đọc từ file license.key (bền vững, không bị xóa)
        string fromFile = LoadKeyFromFile();
        if (!string.IsNullOrEmpty(fromFile))
        {
            // Đồng bộ ngược lại vào SQLite để lần sau đọc nhanh
            try
            {
                string connStr = DatabaseHelper.GetConnectionString();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using var connection = new SqliteConnection(connStr);
                    connection.Open();
                    using var command = new SqliteCommand("UPDATE AppConfig SET LicenseKey = @key WHERE Id = 1", connection);
                    command.Parameters.AddWithValue("@key", fromFile);
                    command.ExecuteNonQuery();
                }
            }
            catch { }
            return fromFile;
        }

        return string.Empty;
    }

    /// <summary>
    /// Xóa key trong SQLite (cho heartbeat lockout), NHƯNG KHÔNG XÓA file license.key.
    /// → Lần mở Panel tiếp theo vẫn đọc được key từ file.
    /// </summary>
    public static void DeleteKey()
    {
        try 
        {
            string connStr = DatabaseHelper.GetConnectionString();
            if (string.IsNullOrEmpty(connStr)) return;

            using var connection = new SqliteConnection(connStr);
            connection.Open();
            using var command = new SqliteCommand("UPDATE AppConfig SET LicenseKey = '' WHERE Id = 1", connection);
            command.ExecuteNonQuery();
        }
        catch { }

        // ⚠️ KHÔNG xóa file license.key — đây là điểm khác biệt quan trọng
    }

    // ── File license.key helpers ──────────────────────────────────────────

    private static void SaveKeyToFile(string licenseKey)
    {
        try
        {
            File.WriteAllText(_licenseFilePath, licenseKey.Trim());
        }
        catch { /* Bỏ qua nếu không ghi được */ }
    }

    private static string LoadKeyFromFile()
    {
        try
        {
            if (File.Exists(_licenseFilePath))
                return File.ReadAllText(_licenseFilePath).Trim();
        }
        catch { }
        return string.Empty;
    }

    private static string LoadKeyFromDb()
    {
        try
        {
            string connStr = DatabaseHelper.GetConnectionString();
            if (string.IsNullOrEmpty(connStr)) return string.Empty;

            using var connection = new SqliteConnection(connStr);
            connection.Open();
            using var command = new SqliteCommand("SELECT LicenseKey FROM AppConfig WHERE Id = 1", connection);
            var result = command.ExecuteScalar();
            return result?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    // ── Backward compat: HeartbeatService dùng JWT Token ──────────────────
    // JWT Token được lưu in-memory sau login, không persist sang file nữa
    private static string _sessionToken = string.Empty;

    public static void SaveToken(string jwtToken)  => _sessionToken = jwtToken ?? string.Empty;
    public static string LoadToken()                => _sessionToken;
    public static void DeleteToken()               => _sessionToken = string.Empty;
}
