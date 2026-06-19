using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
public interface IStudentsService
{
    Task<Student?> GetById(int id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<Student> CreateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}

public class StudentsService(TmsDbContext dbContext, ILogger<StudentsService> logger) : IStudentsService
{
    
    
    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> students =  dbContext.Students.ToList();
        return await Task.FromResult(students);
        
    }
    
    public  async Task<Student?> GetById(int id)
    {
       var stud = await dbContext.Students.FindAsync(id);
        if (stud is null)
        logger.LogWarning("Student {StudentId} not found", id);
        return  stud;
    }
    
      public async Task<Student> CreateAsync(Student student)
    {
        var existingStudent = await dbContext.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == student.RegistrationNumber);

        if (existingStudent is not null)
        {
            logger.LogWarning(
                "Duplicate registration attempt {RegistrationNumber} — already exists as student {StudentId}",
                student.RegistrationNumber, existingStudent.Id);
            return existingStudent;
        }

        var newStudent = new Student
        {
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            GPA = student.GPA,
            IsActive = true
        };

        dbContext.Students.Add(newStudent);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created student {StudentId} — {StudentName}", newStudent.Id, newStudent.Name);

        return newStudent;
    }

    
       public async Task<bool> DeleteAsync(int id)
    {
      var studentId = await dbContext.Students.FindAsync(id);
        if(studentId is null)
        {
            logger.LogWarning("Student {StudentId} not found", id);
            return false;
        }
        dbContext.Students.Remove(studentId);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Deleted student {StudentId}", id);
        return true;
    }

}