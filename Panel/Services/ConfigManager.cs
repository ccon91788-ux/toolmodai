using System.Text.Json;
using Microsoft.Data.Sqlite;
using Panel.Repositories;
using Panel.Models;

namespace Panel.Services;

public static class ConfigManager
{
    public static AppConfig Load()
    {
        try
        {
            using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
            connection.Open();
            const string query = "SELECT ConfigJson FROM AppConfig WHERE Id = 1";

            string? jsonFromDb;
            using (var command = new SqliteCommand(query, connection))
            {
                jsonFromDb = command.ExecuteScalar()?.ToString();
            }

            if (!string.IsNullOrWhiteSpace(jsonFromDb) && jsonFromDb != "{}")
            {
                return JsonSerializer.Deserialize(jsonFromDb, PanelJsonContext.Default.AppConfig) ?? new AppConfig();
            }
        }
        catch
        {
        }

        return new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        try
        {
            string json = JsonSerializer.Serialize(config, PanelJsonContext.Default.AppConfig);
            using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
            connection.Open();

            using var command = new SqliteCommand("UPDATE AppConfig SET ConfigJson = @Json WHERE Id = 1", connection);
            command.Parameters.AddWithValue("@Json", json);
            command.ExecuteNonQuery();
        }
        catch
        {
        }
    }
}
