using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;
namespace TmsApi.Services;

public interface ICourseService
{
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool?> UpdateCourseAsync(int id, CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistUpdateAsync(int id, string code, CancellationToken ct);
    Task<bool> DeleteCourseAsync(int id, CancellationToken ct);
}

public class CourseService(TmsDbContext dbContext, ILogger<CourseService> logger) : ICourseService
{

   public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        dbContext.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);


 
   public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
{
    // Step 1: Start with no-tracking queryable
    IQueryable<Course> query = dbContext.Courses.AsNoTracking();

    // Step 2: Apply search filter if provided
    if (!string.IsNullOrWhiteSpace(request.Search))
        query = query.Where(c =>
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));

    // Step 3: Count BEFORE paging — total matching rows
    var totalCount = await query.CountAsync(ct);

    // Step 4: Apply ordering (whitelist only)
    query = request.OrderBy switch
    {
        "Code" => request.Descending
            ? query.OrderByDescending(c => c.Code)
            : query.OrderBy(c => c.Code),
        "MaxCapacity" => request.Descending
            ? query.OrderByDescending(c => c.MaxCapacity)
            : query.OrderBy(c => c.MaxCapacity),
        _ => request.Descending
            ? query.OrderByDescending(c => c.Title)
            : query.OrderBy(c => c.Title)
    };

    // Step 5: Skip/Take then project inside IQueryable chain
    var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c => new CourseResponseDto(
            c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
        .ToListAsync(ct);

    // Step 6: Return paged response
    return new PagedResponse<CourseResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}


    public async Task<bool?> UpdateCourseAsync(int id, CreateCourseRequest request, CancellationToken ct)
    {
   var courses = await dbContext.Courses.FirstOrDefaultAsync(c=> c.Id == id);
        if(courses is null) return null;

        courses.Title = request.Title;
        courses.Code = request.Code;
        courses.MaxCapacity = request.MaxCapacity;

        await dbContext.SaveChangesAsync(ct);
       return true;
    }

    public async Task<bool> CodeExistUpdateAsync(int id, string code, CancellationToken ct) =>
    await dbContext.Courses.AnyAsync(c=>c.Code == code && c.Id != id, ct);

    public async Task<bool> DeleteCourseAsync(int id, CancellationToken ct)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if(course is null) return false;

        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
