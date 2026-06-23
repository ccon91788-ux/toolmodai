using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Panel.Models;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(AccountSettingsRoot))]
[JsonSerializable(typeof(GeneralSettings))]
[JsonSerializable(typeof(TrainFeatureSettings))]
[JsonSerializable(typeof(AutoUpZinSettings))]
[JsonSerializable(typeof(AutoUpZinTo700kSettings))]
[JsonSerializable(typeof(BossFeatureSettings))]
[JsonSerializable(typeof(GobackFeatureSettings))]
[JsonSerializable(typeof(ItemSettings))]
[JsonSerializable(typeof(SupportSettings))]
[JsonSerializable(typeof(TrainProSlot))]
[JsonSerializable(typeof(MapTemplate))]
[JsonSerializable(typeof(List<MapTemplate>))]
[JsonSerializable(typeof(KilisFeatureSettings))]
[JsonSerializable(typeof(BossVegetaCityFeatureSettings))]
[JsonSerializable(typeof(BuffNamekFeatureSettings))]
[JsonSerializable(typeof(DailyQuestFeatureSettings))]
[JsonSerializable(typeof(AttendanceFeatureSettings))]
[JsonSerializable(typeof(AutoAmuletSettings))]
[JsonSerializable(typeof(DailyMetrics))]
[JsonSerializable(typeof(ReducePowerFeatureSettings))]
internal partial class PanelJsonContext : JsonSerializerContext
{
}
