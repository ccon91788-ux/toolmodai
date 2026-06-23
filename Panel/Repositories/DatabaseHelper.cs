using Microsoft.Data.Sqlite;

namespace Panel.Repositories;

public static class DatabaseHelper
{
    private static string ConnectionString;

    static DatabaseHelper()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "ToolDragonBoy");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        ConnectionString = $"Data Source={Path.Combine(folder, "panel.db")}";
    }

    public static string GetConnectionString() => ConnectionString;

    public static void InitializeDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL DEFAULT '',
                Server TEXT NOT NULL,
                Status TEXT NOT NULL,
                CharacterName TEXT NOT NULL,
                IsSelected INTEGER NOT NULL DEFAULT 0
            );
        ");

        // Migration: thêm cột Password nếu DB cũ chưa có
        try { ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN Password TEXT NOT NULL DEFAULT ''"); } catch { }

        // Migration: thêm cột IsSelected để lưu trạng thái tick
        try { ExecuteNonQuery(connection, "ALTER TABLE Accounts ADD COLUMN IsSelected INTEGER NOT NULL DEFAULT 0"); } catch { }

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS AccountSettings (
                AccountId INTEGER PRIMARY KEY,
                SettingsJson TEXT NOT NULL DEFAULT '{}',
                TrainSettings TEXT NOT NULL DEFAULT '{}',
                BossSettings TEXT NOT NULL DEFAULT '{}',
                GobackSettings TEXT NOT NULL DEFAULT '{}',
                GeneralSettings TEXT NOT NULL DEFAULT '{}',
                FOREIGN KEY(AccountId) REFERENCES Accounts(Id) ON DELETE CASCADE
            );
        ");

        // Migration: add new settings columns for older DBs.
        try { ExecuteNonQuery(connection, "ALTER TABLE AccountSettings ADD COLUMN SettingsJson TEXT NOT NULL DEFAULT '{}'"); } catch { }
        try { ExecuteNonQuery(connection, "ALTER TABLE AccountSettings ADD COLUMN GeneralSettings TEXT NOT NULL DEFAULT '{}'"); } catch { }

        ExecuteNonQuery(connection, @"
            CREATE TABLE IF NOT EXISTS AppConfig (
                Id INTEGER PRIMARY KEY,
                ConfigJson TEXT NOT NULL DEFAULT '{}'
            );
        ");
        try { ExecuteNonQuery(connection, "ALTER TABLE AppConfig ADD COLUMN LicenseKey TEXT NOT NULL DEFAULT ''"); } catch { }
        ExecuteNonQuery(connection, "INSERT OR IGNORE INTO AppConfig (Id, ConfigJson) VALUES (1, '{}')");

    }


    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}

