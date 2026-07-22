using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public class EnrollmentRepository(TmsDbContext dbContext) : IEnrollmentRepository
{
    public Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken ct) =>
        dbContext.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId, ct);

    public Task AddAsync(Enrollment enrollment, CancellationToken ct)
    {
        dbContext.Enrollments.Add(enrollment);
        return dbContext.SaveChangesAsync(ct);
    }

      public Task<List<Enrollment>> GetByStudentIdWithCourseAsync(int studentId, CancellationToken ct) =>
        dbContext.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
}
