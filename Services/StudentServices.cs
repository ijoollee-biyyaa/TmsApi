using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Services;

public interface IStudentsService
{
    Task<Student?> GetById(int id);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task<List<Student>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Student?> CreateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task DemonstrateN1Async(CancellationToken cancellationToken);
    Task<List<object>> GetEnrollmentReportAsync(CancellationToken cancellationToken);
    Task<Student?> UpdateAsync(int id, Student request, CancellationToken cancellationToken);
    Task BulkArchiveEnrollmentsAsync(DateTime cutoff, CancellationToken cancellationToken);
}


public class StudentsService(TmsDbContext dbContext, ILogger<StudentsService> logger) : IStudentsService
{




    public async Task DemonstrateN1Async(CancellationToken cancellationToken)
    {
        var students = await dbContext.Students.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var s in students)
        {
            var count = await dbContext.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);
            logger.LogInformation("{StudentName}: {Count} enrollments", s.Name, count);
        }
    }

public async Task<List<object>> GetEnrollmentReportAsync(CancellationToken cancellationToken)
{
    var report = await dbContext.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        })
        .ToListAsync(cancellationToken);

    foreach (var r in report)
        logger.LogInformation("{StudentName}: {Count} enrollments", r.Name, r.EnrollmentCount);

    return report.Cast<object>().ToList();
}

    public async Task<List<Student>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {

        var students = await dbContext.Students
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
        return students ?? new List<Student>();
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> students = dbContext.Students.ToList();
        return await Task.FromResult(students);

    }



    public async Task<Student?> GetById(int id)
    {
        var stud = await dbContext.Students.FindAsync(id);
        if (stud is null)
            logger.LogWarning("Student {StudentId} not found", id);
        return stud;
    }

    public async Task<Student?> CreateAsync(Student student)
    {
        var existingStudent = await dbContext.Students.FirstOrDefaultAsync(s => s.RegistrationNumber == student.RegistrationNumber);

        if (existingStudent is not null)
        {
            logger.LogWarning(
                "Duplicate registration attempt {RegistrationNumber} — already exists as student {StudentId}",
                student.RegistrationNumber, existingStudent.Id);
            return existingStudent;
        }
        if (string.IsNullOrWhiteSpace(student.Name) || string.IsNullOrWhiteSpace(student.RegistrationNumber))
        {
            logger.LogWarning("student registration rejected: you missed student name or registration number");
            return null;
        }
        var newStudent = new Student
        {
            RegistrationNumber = student.RegistrationNumber,
            Name = student.Name,
            GPA = student.GPA,
            IsActive = true
        };

        dbContext.Students.Add(newStudent);
        dbContext.Entry(newStudent).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created student {StudentId} — {StudentName}", newStudent.Id, newStudent.Name);

        return newStudent;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var studentId = await dbContext.Students.FindAsync(id);
        if (studentId is null)
        {
            logger.LogWarning("Student {StudentId} not found", id);
            return false;
        }
       // dbContext.Students.Remove(studentId);
       studentId.IsDeleted = true;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Soft Deleted student {StudentId}", id);
        return true;
    }

    public async Task<Student?> UpdateAsync(int id, Student request, CancellationToken cancellationToken)
{
    var student = await dbContext.Students.FindAsync(id);
    if (student is null) return null;

    student.Name = request.Name;
    student.GPA = request.GPA;
    
    dbContext.Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
    
    await dbContext.SaveChangesAsync(cancellationToken);
    return student;
}

public async Task BulkArchiveEnrollmentsAsync(DateTime cutoff, CancellationToken cancellationToken)
{
    await dbContext.Enrollments
        .Where(e => e.EnrolledAt < cutoff && !e.IsArchived)
        .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true), cancellationToken);
}

}
