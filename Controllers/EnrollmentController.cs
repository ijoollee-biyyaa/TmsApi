namespace TmsApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
using TmsApi.Services;
[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments =
            await _enrollmentService.GetAllAsync();

        return Ok(enrollments);
    }


    [HttpGet("paged")]
    public async Task<IActionResult> GetEnrollmentPaged(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)

    {
var enrollment = await _enrollmentService.GetPagedResult(page, pageSize, cancellationToken);
return Ok(enrollment);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record =
            await _enrollmentService.GetByIdAsync(id);

        return record != null
            ? Ok(record)
            : NotFound();
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record = await _enrollmentService.EnrollAsync(request.StudentId, request.CourseId);
        if (record is null)
        {
            return BadRequest(new { Message = "Student or course not found." });
        }

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _enrollmentService.DeleteAsync(id);

        return deleted
            ? NoContent()
            : NotFound();
    }

    public record CreateEnrollmentRequest(
        int StudentId,
        int CourseId);


}