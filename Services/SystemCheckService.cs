using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WebApplicationASP01.App;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro diagnostické ověření konektivity k PostgreSQL databázi a stavu IMemoryCache.
/// </summary>
public class SystemCheckService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _env;
    private readonly ILogger<SystemCheckService> _logger;
    private readonly LinkService _linkService;

    public SystemCheckService(
        IServiceProvider serviceProvider,
        IHostEnvironment env,
        ILogger<SystemCheckService> logger,
        LinkService linkService)
    {
        _serviceProvider = serviceProvider;
        _env = env;
        _logger = logger;
        _linkService = linkService;
    }

    /// <summary>
    /// Provede kompletní asynchronní test konektivity pro PostgreSQL a stav úložiště odkazů.
    /// </summary>
    public async Task<SystemCheckResponse> PerformCheckAsync()
    {
        var totalSw = Stopwatch.StartNew();

        var postgresTask = CheckPostgresAsync();
        var linkStorageTask = CheckLinkStorageAsync();

        await Task.WhenAll(postgresTask, linkStorageTask);

        var pgResult = await postgresTask;
        var linkResult = await linkStorageTask;

        totalSw.Stop();

        var overallStatus = pgResult.IsConnected ? "Healthy" : "Unhealthy";

        return new SystemCheckResponse
        {
            OverallStatus = overallStatus,
            Timestamp = DateTimeOffset.UtcNow,
            TotalCheckDurationMs = Math.Round(totalSw.Elapsed.TotalMilliseconds, 2),
            Postgres = pgResult,
            Cache = linkResult,
            Environment = new AppEnvironmentInfo
            {
                Framework = ".NET 10.0 (C# 13)",
                EnvironmentName = _env.EnvironmentName,
                OsPlatform = System.Environment.OSVersion.ToString(),
                ServerTimeUtc = DateTimeOffset.UtcNow,
                MemoryUsageMb = (Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0).ToString("0.00") + " MB",
                CpuTime = Process.GetCurrentProcess().TotalProcessorTime.ToString(@"hh\:mm\:ss"),
                Uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss"),
                RateLimitingEnabled = true
            }
        };
    }

    private async Task<ServiceCheckResult> CheckPostgresAsync()
    {
        var sw = Stopwatch.StartNew();
        var result = new ServiceCheckResult
        {
            Name = "PostgreSQL",
            Type = "Relational Database (EF Core / Npgsql)"
        };

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var canConnect = await dbContext.Database.CanConnectAsync(cts.Token);

            if (canConnect)
            {
                // Dotaz na verzi PostgreSQL
                string? version = null;
                try
                {
                    var conn = dbContext.Database.GetDbConnection();
                    await dbContext.Database.OpenConnectionAsync(cts.Token);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT version();";
                    var rawVersion = await cmd.ExecuteScalarAsync(cts.Token);
                    version = rawVersion?.ToString();
                    await dbContext.Database.CloseConnectionAsync();
                }
                catch { }

                sw.Stop();
                result.IsConnected = true;
                result.Status = "Online";
                result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);

                var dbConn = dbContext.Database.GetDbConnection();
                result.Details["Database"] = dbConn.Database;
                result.Details["DataSource"] = dbConn.DataSource;
                if (!string.IsNullOrEmpty(version))
                {
                    result.Details["ServerVersion"] = version.Split(',')[0].Trim();
                }
            }
            else
            {
                sw.Stop();
                result.IsConnected = false;
                result.Status = "Offline";
                result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
                result.ErrorMessage = "PostgreSQL server neodpovídá na pokus o připojení (CanConnect returned false).";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.IsConnected = false;
            result.Status = "Offline";
            result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Chyba při diagnostické kontrole PostgreSQL.");
        }

        return result;
    }

    private async Task<ServiceCheckResult> CheckLinkStorageAsync()
    {
        var sw = Stopwatch.StartNew();
        var result = new ServiceCheckResult
        {
            Name = "LinkStorage (PostgreSQL)",
            Type = "Relational Storage via EF Core (shared_links table)"
        };

        try
        {
            var status = await _linkService.GetStatusAsync();
            sw.Stop();

            result.IsConnected = true;
            result.Status = "Online";
            result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            result.Details["StorageType"] = status.StorageType;
            result.Details["CachedItems"] = status.Count.ToString();
            result.Details["MaxLimit"] = status.MaxLimit.ToString();
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.IsConnected = false;
            result.Status = "Error";
            result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Chyba při kontrole LinkStorage.");
        }

        return result;
    }
}
