using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);
    Task<List<Enrollment>> GetByStudentIdWithCourseAsync(int studentId, CancellationToken ct);
}