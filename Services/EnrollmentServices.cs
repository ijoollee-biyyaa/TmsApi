using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Dtos;
namespace TmsApi.Services;
public interface IEnrollmentService
{
   Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
}

public class EnrollmentService(TmsDbContext dbContext, ILogger<EnrollmentService> logger) : IEnrollmentService
{
 
public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };
        dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Enrolled student {StudentId} into course {CourseId}", request.StudentId, courseId);
        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }
   


 
}

