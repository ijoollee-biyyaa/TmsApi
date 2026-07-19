using System.ComponentModel.DataAnnotations;

public record RevokeCertificateRequest
{
    [Required, MaxLength(500)]
    public required string Reason { get; init; }
}