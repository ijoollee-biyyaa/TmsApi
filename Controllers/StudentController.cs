using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
using TmsApi.Services;

[ApiController]
[Route("api/students")]
public class StudentController : ControllerBase
{

    private readonly IStudentsService _studentService;
    private readonly ILogger<StudentController> _logger;
    public StudentController(IStudentsService studentsService, ILogger<StudentController> logger)
    {
        _studentService = studentsService;
        _logger = logger;
    }



[HttpGet("n1-demo")]
public async Task<IActionResult> DemonstrateN1(CancellationToken cancellationToken)
{
    await _studentService.DemonstrateN1Async(cancellationToken);
    return Ok("Check SQL log for N+1 queries");
}


[HttpGet("enrollment-report")]
public async Task<IActionResult> GetEnrollmentReport(CancellationToken cancellationToken)
{
    var report = await _studentService.GetEnrollmentReportAsync(cancellationToken);
    return Ok(report);
}

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _studentService.GetAllAsync();
        return Ok(students);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var students = await _studentService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(students);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

        var students = await _studentService.GetById(id);
        return students != null
            ? Ok(students)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudentCreateRequest student)
    {
        var studRequest = new Student
        {
            Name = student.Name,
            RegistrationNumber = student.RegistrationNumber,
            GPA = student.GPA,
            IsActive = true
        };
        var createdStudent = await _studentService.CreateAsync(studRequest);
        if (createdStudent is null)
        {
            return BadRequest(new { Message = "Invalid Request data: name or registration number is required" });
        }
        return CreatedAtAction(nameof(GetById), new { id = createdStudent.Id }, createdStudent);
    }

[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] StudentCreateRequest request, CancellationToken cancellationToken)
{
    var studentRequest = new Student
    {
        Name = request.Name,
        RegistrationNumber = request.RegistrationNumber,
        GPA = request.GPA,
        IsActive = request.IsActive
    };
    
    var updated = await _studentService.UpdateAsync(id, studentRequest, cancellationToken);
    return updated is null ? NotFound() : Ok(updated);
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("enrollments/archive")]
public async Task<IActionResult> BulkArchive([FromQuery] DateTime cutoff, CancellationToken cancellationToken)
{
    await _studentService.BulkArchiveEnrollmentsAsync(cutoff, cancellationToken);
    return Ok("Enrollments archived successfully");
}
    public record StudentCreateRequest(string RegistrationNumber, string Name, decimal GPA, bool IsActive);
}
