using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

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

public class CertificateService(TmsDbContext dbContext, ILogger<CertificateService> logger) : ICertificateService
{
     public async Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(int studentId, PagedRequest request, CancellationToken ct)
    {
        IQueryable<Certificate> query = dbContext.Certificates
        .AsNoTracking().Where(c => c.StudentId == studentId);

        if(!string.IsNullOrWhiteSpace(request.Search))
        query = query.Where(c=> EF.Functions.ILike(c.SerialNumber, $"%{request.Search}%"));

        var totalCount = await query.CountAsync(ct);

        query = request.OrderBy switch
        {
            "IssuedAt" => request.Descending
                ? query.OrderByDescending(c => c.IssuedAt)
                : query.OrderBy(c => c.IssuedAt),
            _ => request.Descending
                ? query.OrderByDescending(c => c.SerialNumber)
                : query.OrderBy(c => c.SerialNumber)
        };

        var certificates = await query
        .Skip((request.Page -1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c=> new CertificateResponseDto(
            c.Id,
            c.SerialNumber,
            c.IssuedAt,
            c.IsRevoked,
             c.RevokedAt,
            c.RevokedReason,
            c.StudentId,
            c.CourseId,
            c.Student.Name,
          c.Course.Title
        )).ToListAsync(ct);

        return new PagedResponse<CertificateResponseDto>
        {
             Items = certificates,
             TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
        };
    }

 public async Task<CertificateResponseDto?> GetCertificateByIdAsync(int studentId, int id, CancellationToken ct)
    {
        return await dbContext.Certificates
        .AsNoTracking()
        .Where(c => c.StudentId == studentId && c.Id == id)
        .Select(c => new CertificateResponseDto(
            c.Id,
            c.SerialNumber,
            c.IssuedAt,
            c.IsRevoked,
             c.RevokedAt,
            c.RevokedReason,
            c.StudentId,
            c.CourseId,
            c.Student.Name,
          c.Course.Title
        )).FirstOrDefaultAsync(ct);
    }

    
    public async Task<CertificateResponseDto> CreateCertificateAsync(int studentId, CreateCertificateRequest request, CancellationToken ct)
    {
        var SerialNumber = $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        
        var certificate = new Certificate
        {
            SerialNumber = SerialNumber,
            StudentId = studentId,
            CourseId = request.CourseId
        };
        dbContext.Certificates.Add(certificate);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Created certificate {CertificateId}-{SerialNumber} for student {StudentId} and course {CourseId}", certificate.Id, certificate.SerialNumber, studentId, request.CourseId);
      return (await GetCertificateByIdAsync(studentId, certificate.Id, ct))!;
    }

    public async Task<bool> ExistsForCourseAsync(int studentId, int courseId, CancellationToken ct)
    {
        return await dbContext.Certificates
        .AsNoTracking()
        .AnyAsync(c => c.StudentId == studentId && c.CourseId == courseId && !c.IsRevoked, ct);
    }

      

    public async Task<bool?> RevokeCertificateAsync(int studentId, int id, RevokeCertificateRequest request, CancellationToken ct)
    {
        var certificate = await dbContext.Certificates
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.Id == id, ct);

        if (certificate is null) return null;
        if(certificate.IsRevoked) return false;
        certificate.IsRevoked = true;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevokedReason = request.Reason;

        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Revoked certificate {CertificateId}-{SerialNumber} for student {StudentId}", certificate.Id, certificate.SerialNumber, studentId);
        return true;
    }
}