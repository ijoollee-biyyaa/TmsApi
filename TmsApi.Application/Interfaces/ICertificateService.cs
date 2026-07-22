using TmsApi.Application.DTOs;
namespace TmsApi.Application.Interfaces;
public interface ICertificateService
{
    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(
        int studentId, PagedRequest request, CancellationToken ct);

    Task<CertificateResponseDto?> GetCertificateByIdAsync(
        int studentId, int id, CancellationToken ct);

    Task<bool> ExistsForCourseAsync(
        int studentId, int courseId, CancellationToken ct);

    Task<CertificateResponseDto> CreateCertificateAsync(
        int studentId, CreateCertificateRequest request, CancellationToken ct);

    Task<bool?> RevokeCertificateAsync(
        int studentId, int id, RevokeCertificateRequest request, CancellationToken ct);
}
