using System.Text.Json;
using Panel.Models;
using Panel.Repositories;

namespace Panel.Services;

public class AccountSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AccountRepository _accountRepository;

    public AccountSettingsService(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public AccountSettingsRoot Load(int accountId)
    {
        var row = _accountRepository.GetAccountSettings(accountId);
        if (row == null)
        {
            var empty = new AccountSettingsRoot();
            Save(accountId, empty);
            return empty;
        }

        var rootFromJson = TryDeserialize(row.SettingsJson, PanelJsonContext.Default.AccountSettingsRoot);
        if (rootFromJson != null)
            return rootFromJson;

        // Backward compatibility: map old per-module JSON into one root object.
        var migrated = new AccountSettingsRoot
        {
            General = TryDeserialize(row.GeneralSettings, PanelJsonContext.Default.GeneralSettings) ?? new GeneralSettings(),
            Train = TryDeserialize(row.TrainSettings, PanelJsonContext.Default.TrainFeatureSettings) ?? new TrainFeatureSettings(),
            Boss = TryDeserialize(row.BossSettings, PanelJsonContext.Default.BossFeatureSettings) ?? new BossFeatureSettings(),
            Goback = TryDeserialize(row.GobackSettings, PanelJsonContext.Default.GobackFeatureSettings) ?? new GobackFeatureSettings()
        };

        Save(accountId, migrated);
        return migrated;
    }

    public void Save(int accountId, AccountSettingsRoot root)
    {
        var row = new AccountSettings
        {
            AccountId = accountId,
            SettingsJson = JsonSerializer.Serialize(root, PanelJsonContext.Default.AccountSettingsRoot),
            GeneralSettings = JsonSerializer.Serialize(root.General, PanelJsonContext.Default.GeneralSettings),
            TrainSettings = JsonSerializer.Serialize(root.Train, PanelJsonContext.Default.TrainFeatureSettings),
            BossSettings = JsonSerializer.Serialize(root.Boss, PanelJsonContext.Default.BossFeatureSettings),
            GobackSettings = JsonSerializer.Serialize(root.Goback, PanelJsonContext.Default.GobackFeatureSettings)
        };

        _accountRepository.UpdateAccountSettings(row);
    }

    private static T? TryDeserialize<T>(string? json, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize(json, typeInfo);
        }
        catch
        {
            return null;
        }
    }
}

