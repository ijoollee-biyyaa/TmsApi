using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Services;
public interface ICourseService
{
    Task<Course?> GetById(int id);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<IList<Course>>GetPagedResult(int page, int pageSize, CancellationToken  cancellationToken);
   
    Task<Course?> CreateAsync(Course course);
    Task<bool> DeleteAsync(int id);
}

public class CourseService(TmsDbContext dbContext , ILogger<CourseService> logger) : ICourseService
{
    
    public async Task<IList<Course>> GetPagedResult(int page, int pageSize, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses
        .OrderBy(e=>e.Title)
        .Skip((1-page) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
        return course;
    }

    
    public async Task<IReadOnlyList<Course>> GetAllAsync()
    {
       
        return await dbContext.Courses.ToListAsync();
        
    }
    
    public async  Task<Course?> GetById(int id)
    {
       var course = await dbContext.Courses.FindAsync(id);
        if (course is null)
        logger.LogWarning("Course {CourseCode} not found", id);
        return  course;
    }
    
    public async Task<Course?> CreateAsync (Course course)
    {
       var existingCourse = await dbContext.Courses.FirstOrDefaultAsync(s=>s.Code == course.Code);

       if(existingCourse is not null)
        {
            logger.LogWarning(
                "Duplicate Course attempt {CourseCode}- already exists (record {CourseCode})",
                course.Code, existingCourse.Code);
             return existingCourse;
        }
        if (string.IsNullOrWhiteSpace(course.Title) || string.IsNullOrWhiteSpace(course.Code))
        {
            logger.LogWarning("Course registration failed: Course code or Title is missing");
            return null;
        } 
              
                var newCourse = new Course
                {
                   
                   Code = course.Code,
                   Capacity = course.Capacity,
                   Title = course.Title
                     

                };
            dbContext.Courses.Add(newCourse);
            await dbContext.SaveChangesAsync();
          logger.LogInformation(
            "Created course {CourseId}-{courseCode}",
            newCourse.Code,newCourse.Code);

            return newCourse;
        }

    
       public async Task<bool> DeleteAsync(int id)
    {
           var course = await dbContext.Courses.FindAsync(id);
        if(course is null)
        {
            logger.LogWarning("Course {Coursecode} not found", id);
            return false;
        }
      dbContext.Courses.Remove(course);
      await dbContext.SaveChangesAsync();
        logger.LogInformation("Deleted course {CourseCode}", id);
        return true;
    }


}
