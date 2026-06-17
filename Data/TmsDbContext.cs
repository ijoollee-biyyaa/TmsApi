using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<TmsApi.Entities.Student>();
    public DbSet<TmsApi.Entities.Course> Courses => Set<TmsApi.Entities.Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}