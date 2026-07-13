using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportingController(TmsDbContext context) : ControllerBase
{
    [HttpGet("activegpa-count")]
    public async Task<IActionResult> ActiveGpaCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { Count = count });
    }

    [HttpGet("coursesenrollment")]
    public async Task<IActionResult> CoursesEnrollment()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("averageper-course")]
    public async Task<IActionResult> AveragePerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("zeroenrollment")]
    public async Task<IActionResult> ZeroEnrollment()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("zeroenrollmentlj")]
    public async Task<IActionResult> ZeroEnrollmentLeftJoin()
    {
        var list = await context.Students
            .LeftJoin(context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
    }
//top 5 course
    [HttpGet("top5-courses")]
    public async Task<IActionResult> GetTop5(CancellationToken cancelationToken)
    {
        var list = await context.Enrollments
        .GroupBy(s=> s.Course.Title)
        .Select(g=> new
        {
            Course = g.Key,
            EnrollmentCount = g.Count()
        }).OrderByDescending(x=>x.EnrollmentCount)
        .Take(5)
        .ToListAsync();
        return Ok(list);
    }
  
}