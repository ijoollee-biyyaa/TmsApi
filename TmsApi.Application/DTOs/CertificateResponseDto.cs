
namespace TmsApi.Application.DTOs;

public record CertificateResponseDto
(
    int Id,
    string SerialNumber,
    DateTime IssuedAt,
     bool IsRevoked,
    DateTime? RevokedAt,
    string? RevokedReason,
    int StudentId,
    int CourseId,
    string StudentName,
    string CourseTitle
);
