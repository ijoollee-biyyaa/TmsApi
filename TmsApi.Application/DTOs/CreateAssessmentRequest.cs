using System.ComponentModel.DataAnnotations;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.DTOs;

public record CreateAssessmentRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Range(0.01,1000)]
    public decimal MaxScore { get; init; }

    [Range(0, 1)]
    public decimal Weight { get; init; }

  
}