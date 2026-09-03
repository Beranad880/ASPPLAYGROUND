using System.ComponentModel.DataAnnotations;

namespace WebApplicationASP01.Models;

public class CreateNoteDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

public class UpdateNoteDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}
