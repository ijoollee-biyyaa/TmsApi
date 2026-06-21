using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Services;
public interface IEnrollmentService
{
    Task<Enrollment?> EnrollAsync(int studentId, int courseId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<IReadOnlyList<Enrollment>> GetAllAsync();
    Task<IList<Enrollment>>GetPagedResult(int page, int pageSize, CancellationToken  cancellationToken);
    Task<bool> DeleteAsync(int id);
}

public class EnrollmentService(TmsDbContext dbContext, ILogger<EnrollmentService> logger) : IEnrollmentService
{
 
   public async Task<IList<Enrollment>> GetPagedResult(int page, int pageSize, CancellationToken cancellationToken)
    {
        var enrollement = await dbContext.Enrollments
        .OrderByDescending(e=>e.EnrolledAt)
        .Skip((1-page) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
        return enrollement;
    }
    public async Task<Enrollment?> EnrollAsync(int studentId, int courseId)
    {
        
        var studentExists = await dbContext.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists)
        {
            logger.LogWarning("Enrollment failed: student {StudentId} not found", studentId);
            return null;
        }
          var courseExists = await dbContext.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
        {
            logger.LogWarning("Enrollment failed: course {CourseId} not found", courseId);
            return null;
        }
  var existing = await dbContext.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

   if (existing is not null)
        {
            logger.LogWarning(
                "Duplicate enrollment attempt: student {StudentId} already in course {CourseId} (record {EnrollmentId})",
                studentId, courseId, existing.Id);
            return existing;
        }
       
      
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId
        };

       dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseId} — record {EnrollmentId}",
            studentId, courseId, enrollment.Id);

        return enrollment;
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {

       var enrollment = await dbContext.Enrollments.FindAsync(id);
        if (enrollment is null)
        {
            logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        return enrollment;

    }

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync()
    {
       
       
        return await dbContext.Enrollments.ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        
        var removed = await dbContext.Enrollments.FindAsync(id);
        if (removed is null)
        {
             logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
             return false;
        }
        dbContext.Enrollments.Remove(removed);
        await dbContext.SaveChangesAsync();

           logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }

 
}

