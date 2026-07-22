
using TmsApi.Application.DTOs;
namespace TmsApi.Application.Interfaces;
public interface IAssessmentService
{
    Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(
        int courseId, PagedRequest request, CancellationToken ct);

    Task<AssessmentResponseDto?> GetByIdAsync(
        int courseId, int id, CancellationToken ct);

    Task<AssessmentResponseDto> CreateAsync(
        int courseId, CreateAssessmentRequest request, CancellationToken ct);

    Task<bool> TitleExistsAsync(
        int courseId, string title, CancellationToken ct);

    Task<bool> TitleExistsForUpdateAsync(
        int courseId, int id, string title, CancellationToken ct);

    Task<bool?> UpdateAssessmentAsync(
        int courseId, int id, CreateAssessmentRequest request, CancellationToken ct);

    Task<bool> DeleteAssessmentAsync(
        int courseId, int id, CancellationToken ct);
}