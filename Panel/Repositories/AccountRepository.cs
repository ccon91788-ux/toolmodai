using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Panel.Models;
using Panel.Helpers;

namespace Panel.Repositories;

public class AccountRepository
{
    private string ConnectionString => DatabaseHelper.GetConnectionString();

    public IEnumerable<Account> GetAllAccounts()
    {
        var accounts = new List<Account>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("SELECT * FROM Accounts", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            accounts.Add(MapAccount(reader));
        }
        return accounts;
    }

    public int AddAccount(Account account)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        
        string sql = @"INSERT INTO Accounts (Username, Password, Server, Status, CharacterName, IsSelected) 
                       VALUES (@Username, @Password, @Server, @Status, @CharacterName, @IsSelected); 
                       SELECT last_insert_rowid();";
        
        using (var command = new SqliteCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Username", account.Username);
            command.Parameters.AddWithValue("@Password", CryptoHelper.EncryptPassword(account.Password));
            command.Parameters.AddWithValue("@Server", account.Server);
            command.Parameters.AddWithValue("@Status", account.Status);
            command.Parameters.AddWithValue("@CharacterName", account.CharacterName);
            command.Parameters.AddWithValue("@IsSelected", account.IsSelected ? 1 : 0);
            account.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        // Tạo thêm dòng trong Setting
        string sqlSetting = "INSERT INTO AccountSettings (AccountId) VALUES (@AccountId)";
        using (var commandSetting = new SqliteCommand(sqlSetting, connection))
        {
            commandSetting.Parameters.AddWithValue("@AccountId", account.Id);
            commandSetting.ExecuteNonQuery();
        }

        return account.Id;
    }

    public void UpdateAccount(Account account)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        string sql = "UPDATE Accounts SET Username = @Username, Password = @Password, Server = @Server WHERE Id = @Id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", account.Username);
        command.Parameters.AddWithValue("@Password", CryptoHelper.EncryptPassword(account.Password));
        command.Parameters.AddWithValue("@Server", account.Server);
        command.Parameters.AddWithValue("@Id", account.Id);
        command.ExecuteNonQuery();
    }

    public void UpdateAccountServer(int accountId, string server)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        const string sql = "UPDATE Accounts SET Server = @Server WHERE Id = @AccountId";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Server", server);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.ExecuteNonQuery();
    }

    public void UpdateAccountSelection(int accountId, bool isSelected)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        const string sql = "UPDATE Accounts SET IsSelected = @IsSelected WHERE Id = @AccountId";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@IsSelected", isSelected ? 1 : 0);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.ExecuteNonQuery();
    }

    public Account? GetAccountById(int accountId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        string sql = "SELECT * FROM Accounts WHERE Id = @AccountId";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@AccountId", accountId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAccount(reader);
        }
        return null;
    }

    public AccountSettings? GetAccountSettings(int accountId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        string sql = "SELECT * FROM AccountSettings WHERE AccountId = @AccountId";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@AccountId", accountId);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAccountSettings(reader);
        }
        return null;
    }

    public void UpdateAccountSettings(AccountSettings settings)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        string sql = @"INSERT INTO AccountSettings (
                           AccountId,
                           SettingsJson,
                           TrainSettings,
                           BossSettings,
                           GobackSettings,
                           GeneralSettings
                       )
                       VALUES (
                           @AccountId,
                           @SettingsJson,
                           @TrainSettings,
                           @BossSettings,
                           @GobackSettings,
                           @GeneralSettings
                       )
                       ON CONFLICT(AccountId) DO UPDATE SET
                           SettingsJson = excluded.SettingsJson,
                           TrainSettings = excluded.TrainSettings,
                           BossSettings = excluded.BossSettings,
                           GobackSettings = excluded.GobackSettings,
                           GeneralSettings = excluded.GeneralSettings";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@AccountId", settings.AccountId);
        command.Parameters.AddWithValue("@SettingsJson", settings.SettingsJson);
        command.Parameters.AddWithValue("@TrainSettings", settings.TrainSettings);
        command.Parameters.AddWithValue("@BossSettings", settings.BossSettings);
        command.Parameters.AddWithValue("@GobackSettings", settings.GobackSettings);
        command.Parameters.AddWithValue("@GeneralSettings", settings.GeneralSettings);
        command.ExecuteNonQuery();
    }

    public void UpdateStatus(int accountId, string status, string characterName)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        // Không lưu Status vào SQL nữa, chỉ lưu CharacterName
        string sql = @"UPDATE Accounts SET CharacterName = @CharacterName 
                       WHERE Id = @AccountId";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@CharacterName", characterName);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.ExecuteNonQuery();
    }

    public void DeleteAccount(int accountId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM Accounts WHERE Id = @AccountId", connection);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.ExecuteNonQuery();
    }

    private static Account MapAccount(SqliteDataReader reader)
    {
        return new Account
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Password = CryptoHelper.DecryptPassword(reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password"))),
            Server = reader.GetString(reader.GetOrdinal("Server")),
            Status = "0. OFFLINE", // Luôn mặc định là OFFLINE khi load từ DB
            CharacterName = reader.GetString(reader.GetOrdinal("CharacterName")),
            IsSelected = reader.GetInt32(reader.GetOrdinal("IsSelected")) == 1
        };
    }

    private static AccountSettings MapAccountSettings(SqliteDataReader reader)
    {
        return new AccountSettings
        {
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            SettingsJson = reader.GetString(reader.GetOrdinal("SettingsJson")),
            TrainSettings = reader.GetString(reader.GetOrdinal("TrainSettings")),
            BossSettings = reader.GetString(reader.GetOrdinal("BossSettings")),
            GobackSettings = reader.GetString(reader.GetOrdinal("GobackSettings")),
            GeneralSettings = reader.GetString(reader.GetOrdinal("GeneralSettings"))
        };
    }
}

