using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Panel.Models;
using Panel.Sockets;

namespace Panel.Services;

public class ReducePowerCoordinator
{
    private sealed class NamekState
    {
        public int AccountId { get; set; }
        public string Server { get; set; } = string.Empty;
        public long CdRemainMs { get; set; }
        public DateTime LastUpdateUtc { get; set; }
    }

    private enum RescueJobStatus
    {
        WaitingDispatch,
        WaitingAck,
        WaitingAlive
    }

    private sealed class RescueJob
    {
        public string JobId { get; set; } = string.Empty;
        public int DeadAccountId { get; set; }
        public string DeadCharName { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;

        public int MapId { get; set; }
        public int ZoneId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastDeadAtUtc { get; set; }

        public RescueJobStatus Status { get; set; }
        public int CurrentHelperAccountId { get; set; }
        public DateTime AckDeadlineUtc { get; set; }
        public DateTime AliveDeadlineUtc { get; set; }

        public DateTime LastRoundResetUtc { get; set; }
        public HashSet<int> AttemptedHelpers { get; } = new();
    }

    private readonly PanelSocketServer _socketServer;
    private readonly Repositories.AccountRepository _accountRepo;
    private readonly AccountSettingsService _settingsService;

    private readonly ConcurrentDictionary<int, NamekState> _namekStates = new();
    private readonly ConcurrentDictionary<int, RescueJob> _jobs = new();
    private readonly object _sync = new();
    private readonly System.Threading.Timer _timer;

    private static readonly TimeSpan HelperStateStale = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan AliveTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan JobTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan NextRoundDelay = TimeSpan.FromSeconds(3);

    public ReducePowerCoordinator(
        PanelSocketServer socketServer,
        Repositories.AccountRepository accountRepo,
        AccountSettingsService settingsService)
    {
        _socketServer = socketServer;
        _accountRepo = accountRepo;
        _settingsService = settingsService;

        _socketServer.OnBuffNamekStateReceived += OnBuffNamekStateReceived;
        _socketServer.OnReducePowerDeadReceived += OnReducePowerDeadReceived;
        _socketServer.OnReducePowerAliveReceived += OnReducePowerAliveReceived;
        _socketServer.OnReducePowerHelpAckReceived += OnReducePowerHelpAckReceived;
        _socketServer.OnClientConnectionChanged += OnClientConnectionChanged;

        _timer = new System.Threading.Timer(_ => Tick(), null, 1000, 500);
    }

    private void OnClientConnectionChanged(int accountId, bool isConnected)
    {
        if (!isConnected)
        {
            _namekStates.TryRemove(accountId, out _);
        }
    }

    private void OnBuffNamekStateReceived(
        int accountId,
        int mapId,
        int zoneId,
        int x,
        int y,
        int skillId,
        long cdTotalMs,
        long cdRemainMs,
        long lastCastMs,
        string state,
        string targetName)
    {
        var acc = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == accountId);
        if (acc == null)
        {
            _namekStates.TryRemove(accountId, out _);
            return;
        }

        var settings = _settingsService.Load(accountId);
        bool isHelper = settings.BuffNamek != null
                        && settings.BuffNamek.Enabled
                        && settings.BuffNamek.BuffTargetMode == 2;
        if (!isHelper)
        {
            _namekStates.TryRemove(accountId, out _);
            return;
        }

        _namekStates[accountId] = new NamekState
        {
            AccountId = accountId,
            Server = acc.Server,
            CdRemainMs = cdRemainMs,
            LastUpdateUtc = DateTime.UtcNow
        };
    }

    private void OnReducePowerDeadReceived(int accountId, int mapId, int zoneId, int x, int y)
    {
        var deadAcc = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == accountId);
        if (deadAcc == null) return;

        lock (_sync)
        {
            DateTime now = DateTime.UtcNow;
            CleanupStaleNamekStates(now);

            if (!_jobs.TryGetValue(accountId, out RescueJob? job))
            {
                job = new RescueJob
                {
                    JobId = Guid.NewGuid().ToString("N"),
                    DeadAccountId = accountId,
                    DeadCharName = deadAcc.CharacterName ?? string.Empty,
                    Server = deadAcc.Server,
                    MapId = mapId,
                    ZoneId = zoneId,
                    X = x,
                    Y = y,
                    CreatedAtUtc = now,
                    LastDeadAtUtc = now,
                    Status = RescueJobStatus.WaitingDispatch,
                    CurrentHelperAccountId = 0,
                    LastRoundResetUtc = now
                };
                _jobs[accountId] = job;
            }
            else
            {
                job.MapId = mapId;
                job.ZoneId = zoneId;
                job.X = x;
                job.Y = y;
                job.DeadCharName = deadAcc.CharacterName ?? job.DeadCharName;
                job.Server = deadAcc.Server;
                job.LastDeadAtUtc = now;
            }

            TryDispatchForJob(job, now);
        }
    }

    private void OnReducePowerAliveReceived(int accountId, int mapId, int zoneId, int x, int y)
    {
        lock (_sync)
        {
            if (_jobs.TryRemove(accountId, out RescueJob? job))
            {
                Log($"[ReducePowerCoordinator] Job {job.JobId} done: {job.DeadCharName} alive at map {mapId} khu {zoneId}.");
            }
        }
    }

    private void OnReducePowerHelpAckReceived(int helperAccountId, int deadAccountId, string jobId, bool ok, long castAtMs, string reason)
    {
        lock (_sync)
        {
            if (!_jobs.TryGetValue(deadAccountId, out RescueJob? job))
            {
                return;
            }

            if (!string.Equals(job.JobId, jobId, StringComparison.Ordinal))
            {
                return;
            }

            if (job.CurrentHelperAccountId != helperAccountId)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            if (!ok)
            {
                job.Status = RescueJobStatus.WaitingDispatch;
                job.CurrentHelperAccountId = 0;
                Log($"[ReducePowerCoordinator] Job {job.JobId}: helper {helperAccountId} fail ({reason}). Try next.");
                TryDispatchForJob(job, now);
                return;
            }

            job.Status = RescueJobStatus.WaitingAlive;
            job.AliveDeadlineUtc = now + AliveTimeout;
            Log($"[ReducePowerCoordinator] Job {job.JobId}: helper {helperAccountId} cast ok, waiting alive.");
        }
    }

    private void Tick()
    {
        lock (_sync)
        {
            DateTime now = DateTime.UtcNow;
            CleanupStaleNamekStates(now);

            foreach (var kv in _jobs.ToList())
            {
                RescueJob job = kv.Value;

                if ((now - job.CreatedAtUtc) > JobTtl)
                {
                    _jobs.TryRemove(job.DeadAccountId, out _);
                    Log($"[ReducePowerCoordinator] Job {job.JobId} expired (ttl). dead={job.DeadCharName}");
                    continue;
                }

                if ((now - job.LastDeadAtUtc) > HelperStateStale)
                {
                    _jobs.TryRemove(job.DeadAccountId, out _);
                    Log($"[ReducePowerCoordinator] Job {job.JobId} removed by dead-timeout fallback.");
                    continue;
                }

                if (job.Status == RescueJobStatus.WaitingAck && now > job.AckDeadlineUtc)
                {
                    Log($"[ReducePowerCoordinator] Job {job.JobId}: helper {job.CurrentHelperAccountId} ack timeout.");
                    job.Status = RescueJobStatus.WaitingDispatch;
                    job.CurrentHelperAccountId = 0;
                    TryDispatchForJob(job, now);
                    continue;
                }

                if (job.Status == RescueJobStatus.WaitingAlive && now > job.AliveDeadlineUtc)
                {
                    Log($"[ReducePowerCoordinator] Job {job.JobId}: alive timeout after helper {job.CurrentHelperAccountId}. Try next.");
                    job.Status = RescueJobStatus.WaitingDispatch;
                    job.CurrentHelperAccountId = 0;
                    TryDispatchForJob(job, now);
                    continue;
                }

                if (job.Status == RescueJobStatus.WaitingDispatch)
                {
                    TryDispatchForJob(job, now);
                }
            }
        }
    }

    private void TryDispatchForJob(RescueJob job, DateTime now)
    {
        if (job.Status != RescueJobStatus.WaitingDispatch)
        {
            return;
        }

        var candidates = _namekStates.Values
            .Where(n => string.Equals(n.Server, job.Server, StringComparison.OrdinalIgnoreCase))
            .Where(n => (now - n.LastUpdateUtc) <= HelperStateStale)
            .Where(n => n.AccountId != job.DeadAccountId)
            .Where(n => !IsHelperBusyInOtherJob(n.AccountId, job.DeadAccountId))
            .ToList();

        if (candidates.Count == 0)
        {
            Log($"[ReducePowerCoordinator] Job {job.JobId}: no helper candidate on server {job.Server}.");
            return;
        }

        var untried = candidates
            .Where(n => !job.AttemptedHelpers.Contains(n.AccountId))
            .OrderBy(n => n.CdRemainMs)
            .ThenBy(n => n.AccountId)
            .ToList();

        if (untried.Count == 0)
        {
            if ((now - job.LastRoundResetUtc) < NextRoundDelay)
            {
                return;
            }

            job.AttemptedHelpers.Clear();
            job.LastRoundResetUtc = now;
            untried = candidates
                .OrderBy(n => n.CdRemainMs)
                .ThenBy(n => n.AccountId)
                .ToList();
            if (untried.Count == 0)
            {
                return;
            }
        }

        NamekState chosen = untried[0];
        string cmd = $"REDUCE_POWER_HELP|{job.JobId}|{job.DeadAccountId}|{job.MapId}|{job.ZoneId}|{job.X}|{job.Y}|{job.DeadCharName}";
        bool sent = _socketServer.SendCommand(chosen.AccountId, cmd);
        if (!sent)
        {
            _namekStates.TryRemove(chosen.AccountId, out _);
            Log($"[ReducePowerCoordinator] Job {job.JobId}: helper {chosen.AccountId} send fail.");
            return;
        }

        job.AttemptedHelpers.Add(chosen.AccountId);
        job.CurrentHelperAccountId = chosen.AccountId;
        job.Status = RescueJobStatus.WaitingAck;
        job.AckDeadlineUtc = now + AckTimeout;

        Log($"[ReducePowerCoordinator] Job {job.JobId}: dispatch helper {chosen.AccountId} to {job.DeadCharName} at {job.MapId}/{job.ZoneId} ({job.X},{job.Y}).");
    }

    private void CleanupStaleNamekStates(DateTime now)
    {
        foreach (var key in _namekStates.Keys.ToList())
        {
            if (_namekStates.TryGetValue(key, out NamekState? state))
            {
                if ((now - state.LastUpdateUtc) > HelperStateStale)
                {
                    _namekStates.TryRemove(key, out _);
                }
            }
        }
    }

    private static void Log(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
    }

    private bool IsHelperBusyInOtherJob(int helperAccountId, int currentDeadAccountId)
    {
        foreach (var kv in _jobs)
        {
            if (kv.Key == currentDeadAccountId) continue;
            RescueJob other = kv.Value;
            if (other.CurrentHelperAccountId != helperAccountId) continue;
            if (other.Status == RescueJobStatus.WaitingAck || other.Status == RescueJobStatus.WaitingAlive)
            {
                return true;
            }
        }
        return false;
    }
}
