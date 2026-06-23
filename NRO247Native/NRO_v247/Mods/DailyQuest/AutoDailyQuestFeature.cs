using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NRO_v247.Mods.Notifications;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods.DailyQuest
{
    public sealed class AutoDailyQuestFeature : IAutoFeature, ICleanupFeature
    {
        public sealed class DailyQuestPanelSettings
        {
            public bool Enabled { get; set; }
            public bool ScheduleEnabled { get; set; }
            public int StartHour { get; set; } = 4;
            public int StartMinute { get; set; }
            public string Difficulty { get; set; } = "siêu khó";
            public bool CancelKillPlayerQuest { get; set; }
            public bool CancelTrainGoldQuest { get; set; }
            public bool CancelTrainMonsterQuest { get; set; }
            public bool TrainMonsterEnabled { get; set; } = true;
            public int TrainMonsterMapId { get; set; } = -1;
            public int TrainMonsterZoneId { get; set; } = -1;
            public string TrainMonsterMobNames { get; set; } = string.Empty;
            public int TrainGoldMapId { get; set; } = -1;
            public bool TrainGoldRequireZone { get; set; }
            public int TrainGoldZoneId { get; set; } = -1;
            public bool UseGoldSuicideMode { get; set; }
            public int TrainGoldSuicideMapId { get; set; }
            public int TrainGoldSuicideZoneId { get; set; } = -1;
            public int KillPlayerMapId { get; set; } = -1;
            public int KillPlayerZoneId { get; set; } = -1;
            public bool KillPlayerOnlyListedTargets { get; set; }
            public string KillPlayerTargetNames { get; set; } = string.Empty;
            public bool AutoFusion { get; set; }
            public int TrainingArmorMode { get; set; }
            public bool UseTdltWhenDoingDailyQuest { get; set; } = true;
            public bool TdltForTrainMonster { get; set; }
            public bool TdltForKillPlayer { get; set; }
            public bool TdltForTrainGold { get; set; }
        }

        public enum AutoState
        {
            Idle,
            NavigateToQuestGiver,
            WaitMapLoaded,
            ConfirmQuestGiver,
            WaitConfirmation,
            WaitForQuestSignal,
            RetryQuestSignal,
            Execute_Navigate,
            Execute_Action,
            Execute_Wait,
            WaitQuestCompleted,
            Complete_Navigate,
            Complete_Wait,
            Finish_Cancel
        }

        public enum QuestType
        {
            None,
            TrainMonster,
            TrainGold,
            SuicideGold,
            KillPlayer,
            Steal
        }

        private readonly DailyQuestPanelSettings _settings = new DailyQuestPanelSettings();
        private readonly TrainCombatController _killPlayerCombatController = new TrainCombatController();
        private readonly TrainRuntimeSettings _killPlayerCombatSettings = new TrainRuntimeSettings();

        // Khi đang chờ hoàn thành quest train và nhường luồng Action cho TrainFeature,
        // DailyQuest vẫn cần được tick để bắt tín hiệu "đã hoàn thành" và quay về trả task.
        public bool IsUtilityTask => State == AutoState.WaitQuestCompleted && ShouldYieldToTrainActionLoop();
        public bool IsActive => _settings.Enabled;
        public bool IsRequested => IsActive && !ShouldYieldToTrainActionLoop();
        public bool IsControllingTrainRuntime
            => IsActive
               && (CurrentQuestType == QuestType.TrainMonster || CurrentQuestType == QuestType.TrainGold)
               && (State == AutoState.Execute_Navigate || State == AutoState.Execute_Action || State == AutoState.WaitQuestCompleted);
        public string CurrentState => IsActive ? $"NVHN: {StateText}" : string.Empty;
        public int Priority => 65;

        public int CompletedToday { get; private set; }
        public int CanceledToday { get; private set; }
        public bool FinishedToday { get; private set; }
        public string RunMode { get; private set; } = string.Empty;
        public string StateText { get; private set; } = Texts.StateOff;

        public AutoState State { get; private set; } = AutoState.Idle;
        public QuestType CurrentQuestType { get; private set; } = QuestType.None;

        private long _delayTimer;
        private string _rawQuestDetail = string.Empty;
        private int _lastScheduleAutoStartDateKey = -1;
        private long _lastKillPlayerZoneChangeMs;
        private long _lastQuestRuntimeApplyMs;
        private long _waitQuestSignalSinceMs;
        private int _questSignalRetryCount;
        private long _waitQuestCompletedSinceMs;

        private int _menuStep;
        
        // Bò Mộng NPC
        private const int QuestGiverMapId = 47;
        private const int QuestGiverNpcId = 17;

        private sealed class MobQuestHint
        {
            public string Name { get; }
            public int MapId { get; }
            public int MobId { get; }

            public MobQuestHint(string name, int mapId, int mobId)
            {
                Name = name;
                MapId = mapId;
                MobId = mobId;
            }
        }

        // Đồng bộ theo hướng Mod Unity: map mob name -> (mapId, mobId) để parse nhiệm vụ ổn định.
        private static readonly MobQuestHint[] QuestMobDatabase =
        {
            new MobQuestHint("Mộc nhân", 14, 0),
            new MobQuestHint("Khủng long", 1, 1),
            new MobQuestHint("Lợn lòi", 8, 2),
            new MobQuestHint("Quỷ đất", 15, 3),
            new MobQuestHint("Khủng long mẹ", 2, 4),
            new MobQuestHint("Lợn lòi mẹ", 9, 5),
            new MobQuestHint("Quỷ đất mẹ", 16, 6),
            new MobQuestHint("Thằn lằn bay", 3, 7),
            new MobQuestHint("Phi long", 11, 8),
            new MobQuestHint("Quỷ bay", 17, 9),
            new MobQuestHint("Thằn lằn mẹ", 4, 10),
            new MobQuestHint("Phi long mẹ", 12, 11),
            new MobQuestHint("Quỷ bay mẹ", 18, 12),
            new MobQuestHint("Ốc mượn hồn", 29, 13),
            new MobQuestHint("Ốc sên", 33, 14),
            new MobQuestHint("Heo Xayda mẹ", 37, 15),
            new MobQuestHint("Heo rừng", 28, 16),
            new MobQuestHint("Heo da xanh", 32, 17),
            new MobQuestHint("Heo Xayda", 36, 18),
            new MobQuestHint("Heo rừng mẹ", 6, 19),
            new MobQuestHint("Heo xanh mẹ", 10, 20),
            new MobQuestHint("Alien", 19, 21),
            new MobQuestHint("Bulon", 30, 22),
            new MobQuestHint("Ukulele", 34, 23),
            new MobQuestHint("Quỷ mập", 38, 24),
            new MobQuestHint("Tambourine", 6, 25),
            new MobQuestHint("Drum", 10, 26),
            new MobQuestHint("Akkuman", 19, 27),
            new MobQuestHint("Không tặc", 29, 31),
            new MobQuestHint("Quỷ đầu to", 33, 32),
            new MobQuestHint("Quỷ địa ngục", 37, 33),
            new MobQuestHint("Nappa", 68, 39),
            new MobQuestHint("Soldier", 70, 40),
            new MobQuestHint("Appule", 71, 41),
            new MobQuestHint("Raspberry", 71, 42),
            new MobQuestHint("Thằn lằn xanh", 72, 43),
            new MobQuestHint("Quỷ đầu nhọn", 64, 44),
            new MobQuestHint("Quỷ đầu vàng", 63, 45),
            new MobQuestHint("Quỷ da tím", 66, 46),
            new MobQuestHint("Quỷ già", 67, 47),
            new MobQuestHint("Cá sấu", 73, 48),
            new MobQuestHint("Dơi da xanh", 67, 49),
            new MobQuestHint("Quỷ chim", 81, 50),
            new MobQuestHint("Lính đầu trọc", 74, 51),
            new MobQuestHint("Lính tai dài", 76, 52),
            new MobQuestHint("Lính vũ trụ", 77, 53),
            new MobQuestHint("Khỉ lông đen", 82, 54),
            new MobQuestHint("Khỉ giáp sắt", 83, 55),
            new MobQuestHint("Khỉ lông đỏ", 79, 56),
            new MobQuestHint("Khỉ lông vàng", 80, 57),
            new MobQuestHint("Xên con cấp 1", 92, 58),
            new MobQuestHint("Xên con cấp 2", 93, 59),
            new MobQuestHint("Xên con cấp 3", 94, 60),
            new MobQuestHint("Xên con cấp 4", 96, 61),
            new MobQuestHint("Xên con cấp 5", 97, 62),
            new MobQuestHint("Xên con cấp 6", 98, 63),
            new MobQuestHint("Xên con cấp 7", 99, 64),
            new MobQuestHint("Xên con cấp 8", 100, 65),
            new MobQuestHint("Tai tím", 106, 66),
            new MobQuestHint("Abo", 107, 67),
            new MobQuestHint("Kado", 109, 68),
            new MobQuestHint("Da xanh", 110, 69),
            new MobQuestHint("Khỉ lông xanh", 155, 78),
            new MobQuestHint("Taburine Đỏ", 155, 79),
            new MobQuestHint("Ếch mặt đỏ", 166, 86),
            new MobQuestHint("Jinai", 166, 87),
            new MobQuestHint("Máy đo sức mạnh", 42, 94)
        };

        private static class Texts
        {
            public const string StateOff = "Đang tắt";
            public const string StateDoneToday = "Đã xong hôm nay";
            public const string StateCompleteAndReturn = "Xong! Về trả task";
            public const string StateParseQuest = "Phân tích nhiệm vụ";
            public const string StateCancelQuest = "Hủy nhiệm vụ";
            public const string StateStart = "Khởi động Auto Bò Mộng";
            public const string StateGoKarin = "Đang về Rừng Karin...";
            public const string StateWaitNpcLoad = "Đợi load Npc Bò Mộng";
            public const string StateOpenNpc = "Mở hội thoại NPC";
            public const string StateWaitNpcResponse = "Chờ phản hồi NPC";
            public const string StateClaimReward = "Nhận thưởng hoàn thành";
            public const string StateViewQuestDetail = "Xem chi tiết nhiệm vụ";
            public const string StateDifficultyChosen = "Đã chọn độ khó, reset menu";
            public const string StateWaitQuestSignal = "Đợi tín hiệu nhiệm vụ";
            public const string StateRetryQuestSignal = "Chưa thấy tín hiệu, thử lại NPC";
            public const string StateReturnReward = "Về Tảo vũ trụ trả task...";
            public const string StateScheduleAutoStart = "Tới giờ NVHN, tự động bật";
            public const string RunModeWaitLogic = "Chờ logic";
            public const string RunModeTrainMonster = "Đánh quái";
            public const string RunModeTrainGold = "Nhặt vàng";
            public const string RunModeKillPlayer = "Đánh người";
            public const string RunModeCancelUnsupported = "Hủy nhiệm vụ (Ăn trộm hoặc lạ)";
            public const string MenuDailyQuest = "nhiệm vụ hàng ngày";
            public const string MenuQuestDetail = "chi tiết nhiệm vụ";
            public const string MenuClaimReward = "nhận thưởng";
            public const string RuleQuestMine = "nhiệm vụ của ngươi";
            public const string RuleQuestCooldown = "thời gian nhận nhiệm vụ";
            public const string RuleQuestGeneric = "nhiệm vụ của";
            public const string RuleOutOfQuest = "đã hết nhiệm vụ";
            public const string RuleComplete = "hoàn thành";
            public const string RuleMonsterLocation = "địa điểm";
            public const string RuleMonsterKillVerb = "hạ";
            public const string RuleGold = "vàng";
            public const string RuleGoldCollect = "nhặt vàng";
            public const string RuleGoldCollectAlt = "lụm vàng";
            public const string RulePlayer = "người";
            public const string RuleQuestLevel = "nhiệm vụ cấp độ";
            public const string RuleKillProgress = "đã hạ được";
        }

        public AutoDailyQuestFeature()
        {
            NotifyCatcher.OnNotifyReceived += HandleNotifyReceived;
        }

        public void Cleanup()
        {
            NotifyCatcher.OnNotifyReceived -= HandleNotifyReceived;
        }

        private void HandleNotifyReceived(NotifyCatcher.NotifyEvent e)
        {
            if (!IsActive) return;

            if (e.Type == NotifyCatcher.NotifyType.SystemNpc
                || e.Type == NotifyCatcher.NotifyType.ChatVip
                || e.Type == NotifyCatcher.NotifyType.GlobalServer)
            {
                string rawText = NormalizeForMatch(e.Message);

                bool isQuestSignal = rawText.Contains(NormalizeForMatch(Texts.RuleQuestMine))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleQuestCooldown))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleQuestGeneric))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleQuestLevel))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleKillProgress))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleMonsterLocation))
                    || rawText.Contains(NormalizeForMatch(Texts.RuleGold))
                    || rawText.Contains(NormalizeForMatch(Texts.RulePlayer));

                if (isQuestSignal)
                {
                    bool alreadyExecuting = State == AutoState.Execute_Navigate
                        || State == AutoState.Execute_Action
                        || State == AutoState.WaitQuestCompleted;
                    if (!alreadyExecuting)
                    {
                        ResetQuestSignalWait();
                        ParseQuestRule(rawText);
                    }
                }
                else if (rawText.Contains(NormalizeForMatch(Texts.RuleOutOfQuest)))
                {
                    ResetQuestSignalWait();
                    FinishedToday = true;
                    StopFromPanel();
                    StateText = Texts.StateDoneToday;
                    State = AutoState.Idle;
                }
                else if (rawText.Contains(NormalizeForMatch(Texts.RuleComplete))
                    && rawText.Contains(NormalizeForMatch(Texts.MenuClaimReward)))
                {
                    ResetQuestSignalWait();
                    SetKillPlayerMode(false);
                    State = AutoState.Complete_Navigate;
                    StateText = Texts.StateCompleteAndReturn;
                    ModBootstrap.TrainFeature?.DisableFromPanel();
                }
            }
        }

        private void ParseQuestRule(string questText)
        {
            SetKillPlayerMode(false);
            _rawQuestDetail = questText;
            StateText = Texts.StateParseQuest;

            List<MobQuestHint> matchedMobHints = FindQuestMobHints(questText);
            bool hasMonsterLocation = questText.Contains(NormalizeForMatch(Texts.RuleMonsterLocation));
            bool hasMonsterKillVerb = questText.Contains(NormalizeForMatch(Texts.RuleMonsterKillVerb))
                || questText.Contains(NormalizeForMatch(Texts.RuleKillProgress));
            bool hasPlayerKeyword = questText.Contains(NormalizeForMatch(Texts.RulePlayer));
            bool hasGoldCollectKeyword = questText.Contains(NormalizeForMatch(Texts.RuleGoldCollect))
                || questText.Contains(NormalizeForMatch(Texts.RuleGoldCollectAlt));
            bool hasGoldKeyword = questText.Contains(NormalizeForMatch(Texts.RuleGold));
            bool hasKnownMonster = matchedMobHints.Count > 0;
            bool hasPlayerProgressPattern = Regex.IsMatch(questText, @"\b\d+\s*/\s*\d+\s*nguoi\b");
            bool isKillPlayerQuest = hasPlayerKeyword && (hasMonsterKillVerb || hasPlayerProgressPattern);

            // Ưu tiên quest đánh người trước để tránh nhầm sang quái/vàng do tên map hoặc mob.
            if (isKillPlayerQuest)
            {
                CurrentQuestType = QuestType.KillPlayer;
                RunMode = Texts.RunModeKillPlayer;
                if (_settings.CancelKillPlayerQuest)
                {
                    CancelQuest();
                    return;
                }
            }
            // Sau đó mới xét quest đánh quái.
            else if (hasKnownMonster || hasMonsterLocation || (hasMonsterKillVerb && !hasPlayerKeyword))
            {
                CurrentQuestType = QuestType.TrainMonster;
                RunMode = Texts.RunModeTrainMonster;
                if (_settings.CancelTrainMonsterQuest)
                {
                    CancelQuest();
                    return;
                }
            }
            else if (hasGoldCollectKeyword || (hasGoldKeyword && !hasMonsterKillVerb && !hasPlayerKeyword))
            {
                CurrentQuestType = _settings.UseGoldSuicideMode ? QuestType.SuicideGold : QuestType.TrainGold;
                RunMode = Texts.RunModeTrainGold;
                if (_settings.CancelTrainGoldQuest)
                {
                    CancelQuest();
                    return;
                }
            }
            else if (hasPlayerKeyword)
            {
                CurrentQuestType = QuestType.KillPlayer;
                RunMode = Texts.RunModeKillPlayer;
                if (_settings.CancelKillPlayerQuest)
                {
                    CancelQuest();
                    return;
                }
            }
            else
            {
                // Mặc định cho ăn trộm hoặc không xác định là hủy
                CurrentQuestType = QuestType.Steal;
                RunMode = Texts.RunModeCancelUnsupported;
                CancelQuest();
                return;
            }

            State = AutoState.Execute_Action;
        }

        private void CancelQuest()
        {
            StateText = Texts.StateCancelQuest;
            State = AutoState.Finish_Cancel;
            _menuStep = 0; // Để nó bắt đầu chu trình mở NPC bấm hủy
        }

        private int GetMapIdFromText(string text)
        {
            List<MobQuestHint> matchedHints = FindQuestMobHints(text);
            if (matchedHints.Count > 0)
            {
                return matchedHints[0].MapId;
            }

            string normalizedText = NormalizeForMatch(text);
            if (TileMap.mapNames == null) return -1;
            for (int i = 0; i < TileMap.mapNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(TileMap.mapNames[i]))
                {
                    if (normalizedText.Contains(NormalizeForMatch(TileMap.mapNames[i])))
                        return i;
                }
            }
            return -1;
        }

        private string GetMobIdsFromText(string text)
        {
            List<MobQuestHint> matchedHints = FindQuestMobHints(text);
            if (matchedHints.Count > 0)
            {
                HashSet<int> ids = new HashSet<int>();
                List<int> ordered = new List<int>();
                for (int i = 0; i < matchedHints.Count; i++)
                {
                    int mobId = matchedHints[i].MobId;
                    if (ids.Add(mobId))
                    {
                        ordered.Add(mobId);
                    }
                }

                return string.Join(",", ordered);
            }

            string normalizedText = NormalizeForMatch(text);
            if (Mob.arrMobTemplate == null) return string.Empty;
            List<int> found = new List<int>();
            for (int i = 0; i < Mob.arrMobTemplate.Length; i++)
            {
                if (Mob.arrMobTemplate[i] != null && !string.IsNullOrEmpty(Mob.arrMobTemplate[i].name))
                {
                    if (normalizedText.Contains(NormalizeForMatch(Mob.arrMobTemplate[i].name)))
                        found.Add(i);
                }
            }
            return string.Join(",", found);
        }

        private List<MobQuestHint> FindQuestMobHints(string text)
        {
            List<MobQuestHint> matched = new List<MobQuestHint>();
            string normalizedText = NormalizeForMatch(text);
            if (string.IsNullOrEmpty(normalizedText)) return matched;

            for (int i = 0; i < QuestMobDatabase.Length; i++)
            {
                MobQuestHint hint = QuestMobDatabase[i];
                if (normalizedText.Contains(NormalizeForMatch(hint.Name)))
                {
                    matched.Add(hint);
                }
            }

            return matched;
        }

        private bool SelectMenuByName(params string[] names)
        {
            // 1. Dạng List Menu dọc (GameCanvas.menu)
            if (GameCanvas.menu != null && GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null)
            {
                for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
                {
                    Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                    if (IsCommandMatch(cmd, names))
                    {
                        GameCanvas.menu.menuSelectedItem = i;
                        GameCanvas.menu.performSelect();
                        GameCanvas.menu.showMenu = false;
                        return true;
                    }
                }
            }

            // 2. Dạng Popup có nút nằm ngang (ChatPopup)
            if (Char.chatPopup != null && TryClickChatPopupCommand(Char.chatPopup, names)) return true;
            if (ChatPopup.currChatPopup != null && TryClickChatPopupCommand(ChatPopup.currChatPopup, names)) return true;
            if (ChatPopup.serverChatPopUp != null && TryClickChatPopupCommand(ChatPopup.serverChatPopUp, names)) return true;

            return false;
        }

        private bool IsCommandMatch(Command cmd, string[] targetNames)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.caption)) return false;
            string caption = cmd.caption.ToLower().Replace("\n", " ").Replace("\r", " ");
            caption = System.Text.RegularExpressions.Regex.Replace(caption, @"\s+", " ").Trim();

            foreach (var n in targetNames)
            {
                string targetName = System.Text.RegularExpressions.Regex.Replace(n.ToLower(), @"\s+", " ").Trim();
                if (caption.Contains(targetName)) return true;
            }
            return false;
        }

        private bool TryClickChatPopupCommand(ChatPopup popup, string[] targetNames)
        {
            if (IsCommandMatch(popup.cmdMsg1, targetNames))
            {
                popup.cmdMsg1.performAction();
                return true;
            }
            if (IsCommandMatch(popup.cmdMsg2, targetNames))
            {
                popup.cmdMsg2.performAction();
                return true;
            }
            if (IsCommandMatch(popup.cmdNextLine, targetNames))
            {
                popup.cmdNextLine.performAction();
                return true;
            }
            return false;
        }

        private bool IsMenuOrPopupOpen()
        {
            if (GameCanvas.menu != null && GameCanvas.menu.showMenu) return true;
            if (Char.chatPopup != null) return true;
            if (ChatPopup.currChatPopup != null) return true;
            if (ChatPopup.serverChatPopUp != null) return true;
            return false;
        }

        public void ApplySettingsFromPanel(DailyQuestPanelSettings settings)
        {
            if (settings == null) return;
            
            bool wasEnabled = _settings.Enabled;
            // Copy logic
            _settings.ScheduleEnabled = settings.ScheduleEnabled;
            _settings.StartHour = settings.StartHour;
            _settings.StartMinute = settings.StartMinute;
            _settings.Difficulty = settings.Difficulty;
            _settings.CancelKillPlayerQuest = settings.CancelKillPlayerQuest;
            _settings.CancelTrainGoldQuest = settings.CancelTrainGoldQuest;
            _settings.CancelTrainMonsterQuest = settings.CancelTrainMonsterQuest;
            _settings.TrainMonsterEnabled = settings.TrainMonsterEnabled;
            _settings.TrainMonsterMapId = settings.TrainMonsterMapId;
            _settings.TrainMonsterZoneId = settings.TrainMonsterZoneId;
            _settings.TrainMonsterMobNames = settings.TrainMonsterMobNames;
            _settings.TrainGoldMapId = settings.TrainGoldMapId;
            _settings.TrainGoldRequireZone = settings.TrainGoldRequireZone;
            _settings.TrainGoldZoneId = settings.TrainGoldZoneId;
            _settings.UseGoldSuicideMode = settings.UseGoldSuicideMode;
            _settings.TrainGoldSuicideMapId = settings.TrainGoldSuicideMapId;
            _settings.TrainGoldSuicideZoneId = settings.TrainGoldSuicideZoneId;
            _settings.KillPlayerMapId = settings.KillPlayerMapId;
            _settings.KillPlayerZoneId = settings.KillPlayerZoneId;
            _settings.KillPlayerOnlyListedTargets = settings.KillPlayerOnlyListedTargets;
            _settings.KillPlayerTargetNames = settings.KillPlayerTargetNames;
            _settings.AutoFusion = settings.AutoFusion;
            _settings.TrainingArmorMode = settings.TrainingArmorMode;
            _settings.UseTdltWhenDoingDailyQuest = settings.UseTdltWhenDoingDailyQuest;
            _settings.TdltForTrainMonster = settings.TdltForTrainMonster;
            _settings.TdltForKillPlayer = settings.TdltForKillPlayer;
            _settings.TdltForTrainGold = settings.TdltForTrainGold;
            _settings.Enabled = settings.Enabled;

            SyncDailyQuestFusionOverride();

            if (wasEnabled && !settings.Enabled)
            {
                StopFromPanel();
            }
            else if (wasEnabled && settings.Enabled)
            {
                // Hot-update ngay khi đang chạy NVHN.
                ApplyQuestRuntimeSettings(mSystem.currentTimeMillis());
            }
            else if (!wasEnabled && settings.Enabled)
            {
                // Lần đầu bật NVHN từ Panel qua ApplySettingsFromPanel.
                StartFromPanel();
            }
        }

        public void StartFromPanel()
        {
            _settings.Enabled = true;
            SyncDailyQuestFusionOverride();
            ResetQuestSignalWait();
            if (FinishedToday)
            {
                StateText = Texts.StateDoneToday;
                State = AutoState.Idle;
                return;
            }

            State = AutoState.NavigateToQuestGiver;
            StateText = Texts.StateStart;
            RunMode = Texts.RunModeWaitLogic;
            CurrentQuestType = QuestType.None;
            _rawQuestDetail = string.Empty;

            // Preflight sync khi bấm Start: nếu player đã làm tay xong quest trước đó
            // thì phải ưu tiên về nhận thưởng ngay, không đi nhận task mới.
            if (IsQuestCompletedRuntimeSignal())
            {
                State = AutoState.Complete_Navigate;
                StateText = Texts.StateCompleteAndReturn;
            }
        }

        public void StopFromPanel()
        {
            // Capture trước khi reset state — chỉ tắt Train nếu NVHN đang kiểm soát nó.
            bool wasControllingTrain = IsControllingTrainRuntime;

            _settings.Enabled = false;
            SyncDailyQuestFusionOverride();
            SetKillPlayerMode(false);
            _killPlayerCombatController.ClearPlayerFocus();
            ResetQuestSignalWait();
            State = AutoState.Idle;
            StateText = Texts.StateOff;
            RunMode = string.Empty;
            CurrentQuestType = QuestType.None;

            // Dừng xmap
            ServiceLocator.Get<IXmapService>()?.StopFromPanel();

            // Tắt Train chỉ khi NVHN đang giữ quyền train — không ảnh hưởng Auto Train thường của Panel.
            // UpZin / Buff NM / Boss / Đậu săn là feature riêng → không bị ảnh hưởng.
            if (wasControllingTrain)
                ModBootstrap.TrainFeature?.DisableFromPanel();
        }

        public void Update()
        {
            TryAutoStartBySchedule();
            if (!IsActive) return;

            long now = mSystem.currentTimeMillis();
            if (now < _delayTimer) return;

            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            switch (State)
            {
                case AutoState.NavigateToQuestGiver:
                    if (IsQuestCompletedRuntimeSignal())
                    {
                        State = AutoState.Complete_Navigate;
                        StateText = Texts.StateCompleteAndReturn;
                        _delayTimer = now + 300;
                        break;
                    }

                    if (TileMap.mapID == QuestGiverMapId)
                    {
                        State = AutoState.WaitMapLoaded;
                        _delayTimer = now + 1000;
                    }
                    else
                    {
                        var xmap = ServiceLocator.Get<IXmapService>();
                        if (xmap != null && !xmap.IsXmaping())
                        {
                            StateText = Texts.StateGoKarin;
                            xmap.StartGoToMapFromPanel(QuestGiverMapId);
                            _delayTimer = now + 2000;
                        }
                    }
                    break;

                case AutoState.WaitMapLoaded:
                    // Đợi load Npc 17
                    if (GameScr.findNPCInMap(QuestGiverNpcId) != null)
                    {
                        State = AutoState.ConfirmQuestGiver;
                        _delayTimer = now + 500;
                    }
                    else
                    {
                        StateText = Texts.StateWaitNpcLoad;
                        _delayTimer = now + 500;
                    }
                    break;

                case AutoState.ConfirmQuestGiver:
                    StateText = Texts.StateOpenNpc;
                    _menuStep = 0;
                    Service.gI().openMenu(QuestGiverNpcId);
                    State = AutoState.WaitConfirmation;
                    _delayTimer = now + 700;
                    break;

                case AutoState.WaitConfirmation:
                    StateText = Texts.StateWaitNpcResponse;
                    if (IsMenuOrPopupOpen())
                    {
                        if (_menuStep == 0)
                        {
                            if (SelectMenuByName(Texts.MenuDailyQuest))
                            {
                                _menuStep++;
                                _delayTimer = now + 500;
                            }
                        }
                        else if (_menuStep == 1)
                        {
                            // Ưu tiên nhận thưởng/chi tiết trước, rồi mới nhận nhiệm vụ mới theo độ khó.
                            if (SelectMenuByName(Texts.MenuClaimReward))
                            {
                                StateText = Texts.StateClaimReward;
                                ResetQuestSignalWait();
                                State = AutoState.WaitForQuestSignal;
                                _delayTimer = now + 1000;
                            }
                            else if (SelectMenuByName(Texts.MenuQuestDetail))
                            {
                                StateText = Texts.StateViewQuestDetail;
                                ResetQuestSignalWait();
                                State = AutoState.WaitForQuestSignal;
                                _delayTimer = now + 1000;
                            }
                            else if (SelectMenuByName(_settings.Difficulty))
                            {
                                _menuStep++;
                                _delayTimer = now + 500;
                            }
                        }
                        else if (_menuStep == 2)
                        {
                            // Sau khi click độ khó, có thể cần mở lại menu để lấy chi tiết nhiệm vụ.
                            StateText = Texts.StateDifficultyChosen;
                            State = AutoState.ConfirmQuestGiver;
                            _delayTimer = now + 2000;
                        }
                    }
                    else if (now > _delayTimer + 5000)
                    {
                        // Timeout
                        State = AutoState.ConfirmQuestGiver;
                    }
                    break;

                case AutoState.WaitForQuestSignal:
                    StateText = Texts.StateWaitQuestSignal;
                    if (TryParseQuestSignalFromUi())
                    {
                        _delayTimer = now + 300;
                        break;
                    }
                    if (_waitQuestSignalSinceMs <= 0)
                    {
                        _waitQuestSignalSinceMs = now;
                    }

                    if (now - _waitQuestSignalSinceMs >= 4500)
                    {
                        State = AutoState.RetryQuestSignal;
                        _delayTimer = now + 200;
                        break;
                    }

                    _delayTimer = now + 500;
                    break;

                case AutoState.RetryQuestSignal:
                    StateText = Texts.StateRetryQuestSignal;
                    _questSignalRetryCount++;
                    State = AutoState.ConfirmQuestGiver;
                    _delayTimer = now + (_questSignalRetryCount < 3 ? 500 : 1200);
                    break;

                case AutoState.Execute_Navigate:
                case AutoState.Execute_Action:
                    StateText = "Đang thực hiện NV";
                    ApplyQuestRuntimeSettings(now);
                    // Sau khi đẩy setting qua Train, chuyển sang chờ hoàn thành nhiệm vụ.
                    // Lúc này DailyQuest sẽ nhả luồng ActionTask để Train được AutoMod chạy.
                    State = AutoState.WaitQuestCompleted;
                    _delayTimer = now + 5000;
                    break;

                case AutoState.WaitQuestCompleted:
                    StateText = "Đang NV";
                    // Re-apply định kỳ — 30s để không reset timer tìm quái của Train mỗi 1.5s.
                    if (now - _lastQuestRuntimeApplyMs >= 30000)
                    {
                        ApplyQuestRuntimeSettings(now);
                    }

                    if (TryHandleQuestCompletedFromRuntimeSignals())
                    {
                        _waitQuestCompletedSinceMs = 0;
                        _delayTimer = now + 300;
                        break;
                    }

                    // Timeout 8 phút: tự về nhận thưởng nếu không thấy tín hiệu hoàn thành.
                    if (_waitQuestCompletedSinceMs <= 0) _waitQuestCompletedSinceMs = now;
                    if (now - _waitQuestCompletedSinceMs >= 480000)
                    {
                        _waitQuestCompletedSinceMs = 0;
                        SetKillPlayerMode(false);
                        State = AutoState.Complete_Navigate;
                        StateText = Texts.StateCompleteAndReturn;
                        ModBootstrap.TrainFeature?.DisableFromPanel();
                        _delayTimer = now + 500;
                        break;
                    }

                    if (CurrentQuestType == QuestType.KillPlayer)
                    {
                        SetKillPlayerMode(true);
                        EnsureKillPlayerMapAndZone(now);
                    }
                    _delayTimer = now + 1000;
                    break;

                case AutoState.Complete_Navigate:
                    // 1. Nếu chết thì hồi sinh trước
                    if (me.meDead)
                    {
                        Service.gI().returnTownFromDead();
                        _delayTimer = now + 500;
                        break;
                    }

                    // 2. Chưa ở map 47 → Xmap đến
                    if (TileMap.mapID != QuestGiverMapId)
                    {
                        var xmap = ServiceLocator.Get<IXmapService>();
                        if (xmap != null && !xmap.IsXmaping())
                        {
                            StateText = Texts.StateReturnReward;
                            xmap.StartGoToMapFromPanel(QuestGiverMapId);
                        }
                        _delayTimer = now + 1500;
                        break;
                    }

                    // 3. Đã ở map 47 → tìm NPC 17, openMenu ngay
                    if (GameScr.findNPCInMap(QuestGiverNpcId) != null)
                    {
                        StateText = "Nhận thưởng NPC...";
                        _menuStep = 0;
                        Service.gI().openMenu(QuestGiverNpcId);
                        State = AutoState.Complete_Wait;
                        _delayTimer = now + 500;
                    }
                    else
                    {
                        StateText = Texts.StateWaitNpcLoad;
                        _delayTimer = now + 800;
                    }
                    break;

                case AutoState.Complete_Wait:
                    // Chờ menu show → tìm "nhiệm vụ hàng ngày" → "nhận thưởng"
                    if (IsMenuOrPopupOpen())
                    {
                        if (_menuStep == 0)
                        {
                            if (SelectMenuByName(Texts.MenuDailyQuest))
                            {
                                _menuStep++;
                                _delayTimer = now + 500;
                            }
                            else
                            {
                                _delayTimer = now + 300;
                            }
                        }
                        else if (_menuStep == 1)
                        {
                            if (SelectMenuByName(Texts.MenuClaimReward))
                            {
                                // Nhận thưởng thành công → reset + nhận NV mới
                                CompletedToday++;
                                ResetQuestSignalWait();
                                CurrentQuestType = QuestType.None;
                                _rawQuestDetail = string.Empty;
                                State = AutoState.NavigateToQuestGiver;
                                StateText = $"Hoàn thành #{CompletedToday} — nhận NV mới";
                                _delayTimer = now + 1500;
                            }
                            else
                            {
                                // Không thấy "nhận thưởng" — retry mở menu
                                _menuStep = 0;
                                State = AutoState.Complete_Navigate;
                                _delayTimer = now + 1500;
                            }
                        }
                    }
                    else if (now > _delayTimer + 5000)
                    {
                        // Timeout menu không mở → retry
                        State = AutoState.Complete_Navigate;
                        _delayTimer = now + 500;
                    }
                    break;

                case AutoState.Finish_Cancel:
                    StateText = Texts.StateCancelQuest;
                    SetKillPlayerMode(false);
                    CanceledToday++;
                    CurrentQuestType = QuestType.None;
                    _rawQuestDetail = string.Empty;
                    State = AutoState.Complete_Navigate;
                    break;
            }
        }
        private void SyncDailyQuestFusionOverride()
        {
            ModBootstrap.PotaraFeature?.SetDailyQuestFusionOverride(_settings.Enabled && _settings.AutoFusion);
        }

        private bool IsAnyTdltScopeSelected()
        {
            return _settings.TdltForTrainMonster || _settings.TdltForKillPlayer || _settings.TdltForTrainGold;
        }

        private bool ShouldUseTdltForQuest(QuestType questType)
        {
            if (!_settings.UseTdltWhenDoingDailyQuest) return false;

            bool hasScopedSelection = IsAnyTdltScopeSelected();
            if (!hasScopedSelection) return true;

            switch (questType)
            {
                case QuestType.TrainMonster:
                    return _settings.TdltForTrainMonster;
                case QuestType.TrainGold:
                case QuestType.SuicideGold:
                    return _settings.TdltForTrainGold;
                case QuestType.KillPlayer:
                    return _settings.TdltForKillPlayer;
                default:
                    return false;
            }
        }

        private static string GetTrainGoldMobIds(int mapId)
        {
            // Bám theo logic Unity cũ:
            // 68 -> mob 39, 77 -> mob 53, map khác để trống => train toàn map.
            if (mapId == 68) return "39";
            if (mapId == 77) return "53";
            return string.Empty;
        }

        private void EnsureKillPlayerMapAndZone(long now)
        {
            int targetMap = _settings.KillPlayerMapId >= 0 ? _settings.KillPlayerMapId : -1;
            int targetZone = _settings.KillPlayerZoneId >= 0 ? _settings.KillPlayerZoneId : -1;

            if (targetMap >= 0 && TileMap.mapID != targetMap)
            {
                ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(targetMap);
                return;
            }

            if (targetZone >= 0 && TileMap.zoneID != targetZone && !Char.ischangingMap && !Controller.isStopReadMessage)
            {
                if (now - _lastKillPlayerZoneChangeMs >= 4500)
                {
                    Service.gI().requestChangeZone(targetZone, -1);
                    _lastKillPlayerZoneChangeMs = now;
                }
            }
        }

        private static void SetKillPlayerMode(bool enabled)
        {
            ModBootstrap.CoDenFeature?.SetDailyQuestOverride(enabled);
        }

        private void ApplyKillPlayerCombatSettings()
        {
            bool useTdlt = ShouldUseTdltForQuest(QuestType.KillPlayer);
            _killPlayerCombatSettings.Apply(
                mapId: -1,
                requireZone: false,
                zoneId: -1,
                useTdlt: useTdlt,
                onlyUsePunch: true,         // skill 0/2/4 tùy class khi đánh người chơi
                avoidSuperMob: false,
                mobTargetType: 0,
                changeLowPlayerZoneIfNoMob: false,
                mobIdsRaw: string.Empty,
                skills: null);
        }

        private void TryAttackKillPlayerTarget()
        {
            if (CurrentQuestType != QuestType.KillPlayer) return;

            int targetMap = _settings.KillPlayerMapId >= 0 ? _settings.KillPlayerMapId : -1;
            int targetZone = _settings.KillPlayerZoneId >= 0 ? _settings.KillPlayerZoneId : -1;
            if (targetMap >= 0 && TileMap.mapID != targetMap) return;
            if (targetZone >= 0 && TileMap.zoneID != targetZone) return;

            Char target = FindKillPlayerTarget();
            if (target == null)
            {
                _killPlayerCombatController.ClearPlayerFocus();
                return;
            }

            ApplyKillPlayerCombatSettings();
            _killPlayerCombatController.UpdateCombatAgainstPlayer(_killPlayerCombatSettings, target);
        }

        private Char FindKillPlayerTarget()
        {
            Char me = Char.myCharz();
            if (me == null || GameScr.vCharInMap == null) return null;

            HashSet<string> targetFilter = BuildKillPlayerTargetFilter();
            bool onlyListedTargets = _settings.KillPlayerOnlyListedTargets && targetFilter.Count > 0;

            Char best = null;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                if (c == null) continue;
                if (c.charID == me.charID) continue;
                if (c.isPet || c.isMiniPet) continue;
                if (c.meDead || c.cHP <= 0 || c.statusMe == 14 || c.statusMe == 5) continue;
                if (string.IsNullOrWhiteSpace(c.cName)) continue;
                if (c.cName.StartsWith("#") || c.cName.StartsWith("$")) continue;
                if (c.cName.Equals("Trọng tài", StringComparison.OrdinalIgnoreCase)) continue;
                if (!me.isMeCanAttackOtherPlayer(c)) continue;

                if (onlyListedTargets)
                {
                    string normalizedName = NormalizePlayerName(c.cName);
                    if (!targetFilter.Contains(normalizedName))
                    {
                        continue;
                    }
                }

                int distance = Res.distance(me.cx, me.cy, c.cx, c.cy);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = c;
                }
            }

            return best;
        }

        private HashSet<string> BuildKillPlayerTargetFilter()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            string raw = _settings.KillPlayerTargetNames;
            if (string.IsNullOrWhiteSpace(raw)) return result;

            string[] parts = raw.Split(new[] { '\r', '\n', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string normalized = NormalizePlayerName(parts[i]);
                if (!string.IsNullOrEmpty(normalized))
                {
                    result.Add(normalized);
                }
            }

            return result;
        }

        private static string NormalizePlayerName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().ToLowerInvariant();
        }

        private void ApplyQuestRuntimeSettings(long now)
        {
            if (!IsActive) return;

            if (CurrentQuestType == QuestType.KillPlayer)
            {
                ModBootstrap.TrainFeature?.DisableFromPanel();
                SetKillPlayerMode(true);
                EnsureKillPlayerMapAndZone(now);
                TryAttackKillPlayerTarget();
                _lastQuestRuntimeApplyMs = now;
                return;
            }

            if (ModBootstrap.TrainFeature == null) return;

            if (CurrentQuestType == QuestType.TrainMonster)
            {
                int targetMap = _settings.TrainMonsterMapId == -1 ? GetMapIdFromText(_rawQuestDetail) : _settings.TrainMonsterMapId;
                string targetMobs = string.IsNullOrEmpty(_settings.TrainMonsterMobNames) ? GetMobIdsFromText(_rawQuestDetail) : _settings.TrainMonsterMobNames;
                bool requireZone = _settings.TrainMonsterZoneId >= 0;
                int zoneId = requireZone ? _settings.TrainMonsterZoneId : -1;

                var tf = ModBootstrap.TrainFeature;
                // Kế thừa Skills chỉ khi Train đang được Panel cấu hình (IsTrain=true)
                // Nếu user chỉ bật NVHN không bật Train → dùng null = tất cả skill tốt nhất.
                bool[] inheritSkills = tf.IsTrain ? tf.Skills : null;
                ModBootstrap.TrainFeature.ApplySettingsFromPanel(
                    targetMap,
                    requireZone, zoneId,
                    ShouldUseTdltForQuest(QuestType.TrainMonster),
                    tf.OnlyUsePunch, inheritSkills, true,
                    0, !requireZone, true,
                    _settings.TrainingArmorMode,
                    false,
                    targetMobs,
                    tf.UseShieldUnderHp, tf.ShieldHpPercent
                );
                _lastQuestRuntimeApplyMs = now;
                return;
            }

            if (CurrentQuestType == QuestType.TrainGold)
            {
                int targetMap = _settings.TrainGoldMapId > 0 ? _settings.TrainGoldMapId : 80;
                bool requireZone = _settings.TrainGoldRequireZone && _settings.TrainGoldZoneId >= 0;
                int zoneId = requireZone ? _settings.TrainGoldZoneId : -1;

                var tf = ModBootstrap.TrainFeature;
                bool[] inheritSkills = tf.IsTrain ? tf.Skills : null;
                ModBootstrap.TrainFeature.ApplySettingsFromPanel(
                    targetMap,
                    requireZone, zoneId,
                    ShouldUseTdltForQuest(QuestType.TrainGold),
                    tf.OnlyUsePunch, inheritSkills, true,
                    0, !requireZone, true,
                    _settings.TrainingArmorMode,
                    false,
                    GetTrainGoldMobIds(targetMap),
                    tf.UseShieldUnderHp, tf.ShieldHpPercent
                );
                _lastQuestRuntimeApplyMs = now;
                return;
            }

            if (CurrentQuestType == QuestType.SuicideGold)
            {
                // Tự sát nhặt vàng: điều hướng đến map/zone chỉ định, không khởi Train.
                int targetMap = _settings.TrainGoldSuicideMapId > 0 ? _settings.TrainGoldSuicideMapId : 80;
                int targetZone = _settings.TrainGoldSuicideZoneId >= 0 ? _settings.TrainGoldSuicideZoneId : -1;

                if (TileMap.mapID != targetMap)
                {
                    ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(targetMap);
                }
                else if (targetZone >= 0 && TileMap.zoneID != targetZone
                    && !Char.ischangingMap && !Controller.isStopReadMessage)
                {
                    if (now - _lastKillPlayerZoneChangeMs >= 4500)
                    {
                        Service.gI().requestChangeZone(targetZone, -1);
                        _lastKillPlayerZoneChangeMs = now;
                    }
                }

                _lastQuestRuntimeApplyMs = now;
            }
        }

        private void ResetQuestSignalWait()
        {
            _waitQuestSignalSinceMs = 0;
            _questSignalRetryCount = 0;
            _waitQuestCompletedSinceMs = 0;
        }

        private bool TryParseQuestSignalFromUi()
        {
            string popupText = NormalizeForMatch(ReadVisibleQuestText());
            if (string.IsNullOrWhiteSpace(popupText)) return false;

            bool isQuestSignal = popupText.Contains(NormalizeForMatch(Texts.RuleQuestMine))
                || popupText.Contains(NormalizeForMatch(Texts.RuleQuestCooldown))
                || popupText.Contains(NormalizeForMatch(Texts.RuleQuestGeneric))
                || popupText.Contains(NormalizeForMatch(Texts.RuleQuestLevel))
                || popupText.Contains(NormalizeForMatch(Texts.RuleKillProgress))
                || popupText.Contains(NormalizeForMatch(Texts.RuleMonsterLocation))
                || popupText.Contains(NormalizeForMatch(Texts.RuleGold))
                || popupText.Contains(NormalizeForMatch(Texts.RulePlayer));

            if (!isQuestSignal) return false;

            ResetQuestSignalWait();
            ParseQuestRule(popupText);
            return true;
        }

        private bool TryHandleQuestCompletedFromRuntimeSignals()
        {
            if (State != AutoState.WaitQuestCompleted) return false;

            if (IsQuestCompletedRuntimeSignal())
            {
                ResetQuestSignalWait();
                SetKillPlayerMode(false);
                State = AutoState.Complete_Navigate;
                StateText = Texts.StateCompleteAndReturn;
                ModBootstrap.TrainFeature?.DisableFromPanel();
                return true;
            }
            return false;
        }

        private bool IsDailyQuestOrderCompleted()
        {
            Char me = Char.myCharz();
            if (me == null || me.taskOrders == null) return false;

            for (int i = 0; i < me.taskOrders.size(); i++)
            {
                TaskOrder order = (TaskOrder)me.taskOrders.elementAt(i);
                if (order == null) continue;
                if (order.taskId != TaskOrder.TASK_DAY) continue;
                if (order.maxCount <= 0) continue;

                if (order.count >= order.maxCount)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsQuestCompletedRuntimeSignal()
        {
            if (IsDailyQuestOrderCompleted()) return true;

            string popupText = NormalizeForMatch(ReadVisibleQuestText());
            if (string.IsNullOrWhiteSpace(popupText)) return false;

            return popupText.Contains(NormalizeForMatch(Texts.RuleComplete))
                || popupText.Contains(NormalizeForMatch(Texts.MenuClaimReward));
        }

        private static string ReadVisibleQuestText()
        {
            var sb = new StringBuilder();

            void appendLines(string[] lines)
            {
                if (lines == null || lines.Length == 0) return;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        if (sb.Length > 0) sb.Append(' ');
                        sb.Append(lines[i]);
                    }
                }
            }

            if (Char.chatPopup != null)
            {
                appendLines(Char.chatPopup.says);
            }

            if (ChatPopup.currChatPopup != null)
            {
                appendLines(ChatPopup.currChatPopup.says);
            }

            if (ChatPopup.serverChatPopUp != null)
            {
                appendLines(ChatPopup.serverChatPopUp.says);
            }

            return sb.ToString();
        }

        private static string NormalizeForMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string lower = text.ToLowerInvariant().Trim();
            string decomposed = lower.Normalize(NormalizationForm.FormD);
            char[] buffer = new char[decomposed.Length];
            int count = 0;
            for (int i = 0; i < decomposed.Length; i++)
            {
                char c = decomposed[i];
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    buffer[count++] = c;
                }
            }

            string noAccent = new string(buffer, 0, count).Normalize(NormalizationForm.FormC);
            noAccent = noAccent.Replace('đ', 'd');
            noAccent = Regex.Replace(noAccent, @"\s+", " ");
            return noAccent;
        }

        private void TryAutoStartBySchedule()
        {
            if (IsActive) return;
            if (!_settings.ScheduleEnabled) return;
            if (FinishedToday) return;

            DateTime now = DateTime.Now;
            int todayKey = now.Year * 10000 + now.Month * 100 + now.Day;
            if (_lastScheduleAutoStartDateKey == todayKey) return;

            bool reachedStartTime = now.Hour > _settings.StartHour
                || (now.Hour == _settings.StartHour && now.Minute >= _settings.StartMinute);
            if (!reachedStartTime) return;

            _lastScheduleAutoStartDateKey = todayKey;
            StartFromPanel();
            StateText = Texts.StateScheduleAutoStart;
        }

        private bool ShouldYieldToTrainActionLoop()
        {
            if (State != AutoState.WaitQuestCompleted) return false;

            bool isTrainQuest = CurrentQuestType == QuestType.TrainMonster || CurrentQuestType == QuestType.TrainGold;
            if (!isTrainQuest) return false;

            return ModBootstrap.TrainFeature != null && ModBootstrap.TrainFeature.IsActive;
        }
    }
}




