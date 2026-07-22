using TmsApi.Domain.Entities;
using System.ComponentModel.DataAnnotations;
namespace TmsApi.Application.DTOs;
public record CreateCertificateRequest
{
    
    [Required]
    public required int CourseId { get; init; }

  
}