using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);
    Task<Course?> GetByIdAsync(int id, CancellationToken ct);

    Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct);

    Task<Course> AddAsync(Course course, CancellationToken ct);
    Task<bool> CourseExistAsync(string code, CancellationToken ct);
}