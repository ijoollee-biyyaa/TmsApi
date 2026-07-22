
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Interfaces;
namespace TmsApi.Infrastructure.Services;




public class StudentsService(TmsDbContext dbContext, ILogger<StudentsService> logger) : IStudentsService
{
    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Student> query = dbContext.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{request.Search}%") ||
                EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));

        var totalCount = await query.CountAsync(ct);

        query = request.OrderBy switch
        {
            "Name" => request.Descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "GPA" => request.Descending ? query.OrderByDescending(s => s.GPA) : query.OrderBy(s => s.GPA),
            _ => request.Descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
        };

        var students = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id, s.RegistrationNumber, s.Name, s.GPA))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
            Items = students,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>

       await dbContext.Students.AsNoTracking()
       .Where(s => s.Id == id)
       .Select(s => new StudentResponseDto
       (s.Id,
       s.Name,
       s.RegistrationNumber,
       s.Enrollments.Count())
       ).FirstOrDefaultAsync(ct);



    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var newStudents = new Student
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            GPA = request.GPA,

        };
        dbContext.Students.Add(newStudents);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Created Student {StudentId} ({RegistrationNumber})", newStudents.Id, newStudents.RegistrationNumber);
        return (await GetByIdAsync(newStudents.Id, ct))!;
    }

    public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct)
    {
        var existing = await dbContext.Students.AnyAsync(s => s.RegistrationNumber == registrationNumber, ct);
        return existing;
    }

    public async Task<bool> RegistrationNumberExistsUpdatesAsync(int id, string registrationNumber, CancellationToken ct)
    {
      return await dbContext.Students.AnyAsync(s=>s.RegistrationNumber == registrationNumber && s.Id != id);
    }

    public async Task<bool?> UpdateStudentASync(int id, CreateStudentRequest request, CancellationToken ct)
    {
        var students = await dbContext.Students.FirstOrDefaultAsync(s=>s.Id == id);
        if (students == null) return false;

        students.Name = request.Name;
        students.RegistrationNumber = request.RegistrationNumber;
        students.GPA = request.GPA;

        
         await dbContext.SaveChangesAsync(ct);
         return true;
    }

    public async Task<bool> DeleteStudentAsync(int id, CancellationToken ct)
    {
      var studeent = await dbContext.Students.FindAsync(id);
      if (studeent == null) return false;


      //dbContext.Students.Remove(studeent);
        studeent.IsDeleted = true;
      await dbContext.SaveChangesAsync(ct);
      return true;
    }



}





