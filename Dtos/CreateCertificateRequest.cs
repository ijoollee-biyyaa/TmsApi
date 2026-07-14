using TmsApi.Entities;
using System.ComponentModel.DataAnnotations;
namespace TmsApi.Dtos;
public record CreateCertificateRequest
{
    
    [Required]
    public required int CourseId { get; init; }

  
}