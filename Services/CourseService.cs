public interface ICourseService
{
    Task<Course?> GetById(string code);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<Course> CreateAsync(Course course);
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    private readonly CourseStore _course;
    private readonly ILogger<CourseService> _logger;

    public CourseService(CourseStore course, ILogger<CourseService> logger)
    {
        _course = course;
        _logger = logger;
    }   
    
    public async Task<IReadOnlyList<Course>> GetAllAsync()
    {
        IReadOnlyList<Course> course =  _course.Courses.Values.ToList();
        return await Task.FromResult(course);
        
    }
    
    public  Task<Course?> GetById(string code)
    {
        _course.Courses.TryGetValue(code, out var course);
        if (course is null)
        _logger.LogWarning("Course {CourseCode} not found", code);
        return  Task.FromResult(course);
    }
    
    public  Task<Course> CreateAsync (Course course)
    {
       var existingCourse = _course.Courses.Values.FirstOrDefault(s=>s.Code == course.Code);

       if(existingCourse is not null)
        {
            _logger.LogWarning(
                "Duplicate Course attempt {CourseCode}- already exists (record {CourseCode})",
                course.Code, existingCourse.Code);
             return Task.FromResult(existingCourse);
        }
                
                var code = Guid.NewGuid().ToString("N")[..8];
                var newCourse = new Course
                {
                   
                   Code = code,
                   Capacity = course.Capacity,
                   EnrolledCount = course.EnrolledCount  ,
                   Title = course.Title
                     

                };
          _course.Courses[code] = newCourse;
          _logger.LogInformation(
            "Created course {CourseId}",
            newCourse.Code);

            return Task.FromResult(newCourse);
        }

    
       public Task<bool> DeleteAsync(string code)
    {
        _course.Courses.TryGetValue(code, out var course);
        if(course is null)
        {
            _logger.LogWarning("Course {Coursecode} not found", code);
            return Task.FromResult(false);
        }
        _course.Courses.Remove(code);
        _logger.LogInformation("Deleted course {CourseCode}", code);
        return Task.FromResult(true);
    }

}