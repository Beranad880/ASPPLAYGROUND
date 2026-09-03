using System.Text.Json.Serialization;

namespace WebApplicationASP01.Models;

public class SystemCheckResponse
{
    [JsonPropertyName("overallStatus")]
    public string OverallStatus { get; set; } = "Healthy"; // "Healthy", "Degraded", "Unhealthy"

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("totalCheckDurationMs")]
    public double TotalCheckDurationMs { get; set; }

    [JsonPropertyName("postgres")]
    public ServiceCheckResult Postgres { get; set; } = new();

    [JsonPropertyName("redis")]
    public ServiceCheckResult Redis { get; set; } = new();

    [JsonPropertyName("environment")]
    public AppEnvironmentInfo Environment { get; set; } = new();
}

public class ServiceCheckResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("isConnected")]
    public bool IsConnected { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Offline"; // "Online" | "Offline"

    [JsonPropertyName("latencyMs")]
    public double LatencyMs { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("details")]
    public Dictionary<string, string?> Details { get; set; } = new();
}

public class AppEnvironmentInfo
{
    [JsonPropertyName("framework")]
    public string Framework { get; set; } = ".NET 10.0";

    [JsonPropertyName("environmentName")]
    public string EnvironmentName { get; set; } = "Production";

    [JsonPropertyName("osPlatform")]
    public string OsPlatform { get; set; } = System.Environment.OSVersion.ToString();

    [JsonPropertyName("serverTimeUtc")]
    public DateTimeOffset ServerTimeUtc { get; set; } = DateTimeOffset.UtcNow;
}
