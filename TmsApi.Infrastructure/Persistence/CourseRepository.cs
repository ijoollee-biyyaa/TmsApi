using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public class CourseRepository(TmsDbContext dbContext) : ICourseRepository
{
    public Task<Course?> GetByCodeAsync(string code, CancellationToken ct) =>
        dbContext.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Code == code, ct);

    public Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Course> AddAsync(Course course, CancellationToken ct)
    {
         dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(ct);
        return course;


    }

    public async Task<bool> CourseExistAsync(string code, CancellationToken ct)
    {
        return await dbContext.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    }
}