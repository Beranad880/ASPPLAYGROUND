using System.ComponentModel.DataAnnotations;

namespace WebApplicationASP01.App;

public class Person
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Jmeno { get; set; } = string.Empty;

    [Required]
    public DateOnly DatumNarozeni { get; set; }

    [Required]
    [MaxLength(250)]
    public string TrvalaAdresa { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Telefon { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class CreatePersonDto
{
    [Required]
    public string Jmeno { get; set; } = string.Empty;

    [Required]
    public DateOnly DatumNarozeni { get; set; }

    [Required]
    public string TrvalaAdresa { get; set; } = string.Empty;

    [Required]
    public string Telefon { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class UpdatePersonDto
{
    [Required]
    public string Jmeno { get; set; } = string.Empty;

    [Required]
    public DateOnly DatumNarozeni { get; set; }

    [Required]
    public string TrvalaAdresa { get; set; } = string.Empty;

    [Required]
    public string Telefon { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
