public interface IStudentsService
{
    Task<Student?> GetById(string id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<Student> CreateAsync(Student student);
    Task<bool> DeleteAsync(string id);
}

public class StudentsService : IStudentsService
{
    private readonly StudentStore _students;
    private readonly ILogger<StudentsService> _logger;

    public StudentsService(StudentStore students, ILogger<StudentsService> logger)
    {
        _students = students;
        _logger = logger;
    }   
    
    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> students =  _students.Students.Values.ToList();
        return await Task.FromResult(students);
        
    }
    
    public   Task<Student?> GetById(string id)
    {
        _students.Students.TryGetValue(id, out var student);
        if (student is null)
        _logger.LogWarning("Stuent {StudentId} not found", id);
        return  Task.FromResult(student);
    }
    
    public  Task<Student> CreateAsync (Student student)
    {
       var existingStudents = _students.Students.Values.FirstOrDefault(s=>s.Id == student.Id && s.Name == student.Name);

       if(existingStudents is not null)
        {
            _logger.LogWarning(
                "Duplicate student attempt {StudentId}- {StudentName}  already exists (record {StudentId})",
                student.Id, existingStudents.Name, existingStudents.Id);
             return Task.FromResult(existingStudents);
        }
                
                var id = Guid.NewGuid().ToString("N")[..8];
                var newStudent = new Student
                {
                    Id = id,
                    Name = student.Name,
                    Age = student.Age,
                    GPA = student.GPA
                };
          _students.Students[id] = newStudent;
          _logger.LogInformation(
            "Created student {StudentId}- {StudentName} record {StudentId}",
            newStudent.Id, newStudent.Name, newStudent.Id);

            return Task.FromResult(newStudent);
        }

    
       public Task<bool> DeleteAsync(string id)
    {
        _students.Students.TryGetValue(id, out var student);
        if(student is null)
        {
            _logger.LogWarning("Student {StudentId} not found", id);
            return Task.FromResult(false);
        }
        _students.Students.Remove(id);
        _logger.LogInformation("Deleted student {StudentId}", id);
        return Task.FromResult(true);
    }

}