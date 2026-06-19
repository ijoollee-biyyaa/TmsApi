using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
public interface ICourseService
{
    Task<Course?> GetById(int code);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<Course> CreateAsync(Course course);
    Task<bool> DeleteAsync(int code);
}

public class CourseService(TmsDbContext dbContext , ILogger<CourseService> logger) : ICourseService
{
    
  
    
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
    
    public async Task<Course> CreateAsync (Course course)
    {
       var existingCourse = await dbContext.Courses.FirstOrDefaultAsync(s=>s.Code == course.Code);

       if(existingCourse is not null)
        {
            logger.LogWarning(
                "Duplicate Course attempt {CourseCode}- already exists (record {CourseCode})",
                course.Code, existingCourse.Code);
             return existingCourse;
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