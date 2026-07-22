using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.DTOs;
public record RevokeCertificateRequest
{
    [Required, MaxLength(500)]
    public required string Reason { get; init; }
}