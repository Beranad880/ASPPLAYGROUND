using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebApplicationASP01.App;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.Services;

/// <summary>
/// Služba pro diagnostické ověření konektivity k PostgreSQL databázi a Redis serveru.
/// </summary>
public class SystemCheckService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IHostEnvironment _env;
    private readonly ILogger<SystemCheckService> _logger;

    public SystemCheckService(
        IServiceProvider serviceProvider,
        IHostEnvironment env,
        ILogger<SystemCheckService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _serviceProvider = serviceProvider;
        _env = env;
        _logger = logger;
        _redis = redis;
    }

    /// <summary>
    /// Provede kompletní asynchronní test konektivity pro PostgreSQL i Redis.
    /// </summary>
    public async Task<SystemCheckResponse> PerformCheckAsync()
    {
        var totalSw = Stopwatch.StartNew();

        var postgresTask = CheckPostgresAsync();
        var redisTask = CheckRedisAsync();

        await Task.WhenAll(postgresTask, redisTask);

        var pgResult = await postgresTask;
        var redisResult = await redisTask;

        totalSw.Stop();

        var overallStatus = (pgResult.IsConnected, redisResult.IsConnected) switch
        {
            (true, true) => "Healthy",
            (true, false) => "Degraded",
            (false, true) => "Degraded",
            (false, false) => "Unhealthy"
        };

        return new SystemCheckResponse
        {
            OverallStatus = overallStatus,
            Timestamp = DateTimeOffset.UtcNow,
            TotalCheckDurationMs = Math.Round(totalSw.Elapsed.TotalMilliseconds, 2),
            Postgres = pgResult,
            Redis = redisResult,
            Environment = new AppEnvironmentInfo
            {
                Framework = ".NET 10.0 (C# 13)",
                EnvironmentName = _env.EnvironmentName,
                OsPlatform = System.Environment.OSVersion.ToString(),
                ServerTimeUtc = DateTimeOffset.UtcNow
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

    private async Task<ServiceCheckResult> CheckRedisAsync()
    {
        var sw = Stopwatch.StartNew();
        var result = new ServiceCheckResult
        {
            Name = "Redis",
            Type = "In-Memory Cache & Key-Value (StackExchange.Redis)"
        };

        try
        {
            if (_redis != null && _redis.IsConnected)
            {
                var db = _redis.GetDatabase();
                var ping = await db.PingAsync();
                sw.Stop();

                var endpoints = _redis.GetEndPoints();
                var endpointStr = endpoints.Length > 0 ? endpoints[0].ToString() : "N/A";

                result.IsConnected = true;
                result.Status = "Online";
                result.LatencyMs = Math.Round(ping.TotalMilliseconds, 2);
                result.Details["Endpoint"] = endpointStr;
                result.Details["ClientName"] = _redis.ClientName ?? "ASPNET_PLAYGROUND";
                result.Details["PingLatency"] = $"{Math.Round(ping.TotalMilliseconds, 2)} ms";
            }
            else
            {
                sw.Stop();
                result.IsConnected = false;
                result.Status = "Offline";
                result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
                result.ErrorMessage = "Redis multiplexer není připojen k žádnému aktivnímu endpointu.";
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.IsConnected = false;
            result.Status = "Offline";
            result.LatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Chyba při diagnostické kontrole Redis.");
        }

        return result;
    }
}
