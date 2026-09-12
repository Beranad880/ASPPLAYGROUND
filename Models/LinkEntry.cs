using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplicationASP01.Models;

/// <summary>
/// Model reprezentující uložený text nebo URL odkaz sdílený přes PostgreSQL.
/// </summary>
[Table("shared_links")]
public class LinkEntry
{
    [Key]
    [Column("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [MaxLength(4000)]
    [Column("content")]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("is_url")]
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
/// Stavové informace o úložišti PostgreSQL.
/// </summary>
public class LinkServiceStatus
{
    [JsonPropertyName("storageType")]
    public string StorageType { get; set; } = "PostgreSQL";

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("maxLimit")]
    public int MaxLimit { get; set; } = 50;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
