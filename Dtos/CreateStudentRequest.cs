namespace TmsApi.Dtos;

using System.ComponentModel.DataAnnotations;

public record CreateStudentRequest
{
    [Required]
    [RegularExpression(@"^TMS-\d{4}-\d{4}$", ErrorMessage = "Registration number must follow the pattern TMS-YYYY-0000")]
    public required string RegistrationNumber { get; init; }

    [Required, MaxLength(20)]
    public required string Name { get; init; }

    [Range(0, 4.0)]
    public decimal GPA { get; init; }
}

