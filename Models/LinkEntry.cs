using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationASP01.Models;

/// <summary>
/// Model reprezentující uložený text nebo URL odkaz sdílený přes Redis.
/// </summary>
public class LinkEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("isUrl")]
    public bool IsUrl { get; set; }

    public static bool CheckIsUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        return false;
    }
}

/// <summary>
/// DTO pro vytvoření nového odkazu / textu.
/// </summary>
public class CreateLinkDto
{
    [Required(ErrorMessage = "Text nebo URL nesmí být prázdné.")]
    [StringLength(4000, ErrorMessage = "Délka textu nesmí přesáhnout 4000 znaků.")]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    // Aliasy pro flexibilní JSON payload ({ "text": "..." } nebo { "url": "..." })
    [JsonPropertyName("text")]
    public string? Text
    {
        get => Content;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Content))
            {
                Content = value;
            }
        }
    }

    [JsonPropertyName("url")]
    public string? Url
    {
        get => Content;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Content))
            {
                Content = value;
            }
        }
    }
}

/// <summary>
/// Stavové informace o úložišti a Redis připojení.
/// </summary>
public class LinkServiceStatus
{
    [JsonPropertyName("isRedisConnected")]
    public bool IsRedisConnected { get; set; }

    [JsonPropertyName("storageType")]
    public string StorageType { get; set; } = "Redis";

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("redisKey")]
    public string RedisKey { get; set; } = "shared:links";

    [JsonPropertyName("maxLimit")]
    public int MaxLimit { get; set; } = 50;

    [JsonPropertyName("ttlDays")]
    public int TtlDays { get; set; } = 7;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
