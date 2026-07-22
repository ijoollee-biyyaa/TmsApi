using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentController : ControllerBase
{

    private readonly IStudentsService _studentService;
    private readonly ILogger<StudentController> _logger;
    public StudentController(IStudentsService studentsService, ILogger<StudentController> logger)
    {
        _studentService = studentsService;
        _logger = logger;
    }


    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointName("GetStudentById")]
    [EndpointSummary("Get a student by ID")]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var students = await _studentService.GetByIdAsync(id, ct);
        return students is not null ? Ok(students) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointName("GetStudents")]
    [EndpointSummary("Get a list of all students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _studentService.GetStudentsAsync(request, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointName("CreateStudent")]
    [EndpointSummary("Create a new student")]
    public async Task<IActionResult> CreateStudent(CreateStudentRequest request, CancellationToken ct)
    {
        if (await _studentService.RegistrationNumberExistsAsync(request.RegistrationNumber, ct))
            return Conflict(new ProblemDetails
            {
                Title = "Student Number already exists",
                Detail = $"A student with Registration Number '{request.RegistrationNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });

        var result = await _studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetStudentById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}", Name =nameof(UpdateStudentAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointName("UpdateStudent")]
    [EndpointSummary("Update a student")]
    public async Task<IActionResult> UpdateStudentAsync(int id , CreateStudentRequest request, CancellationToken ct)
    {
        if (await _studentService.RegistrationNumberExistsUpdatesAsync(id, request.RegistrationNumber, ct))
        
            return Conflict(new ProblemDetails
            {
                Title = "Student Registration Number Already Exist",
                Detail =$"Student with Registration Number {request.RegistrationNumber} is already exist!",
                Status = StatusCodes.Status409Conflict,
            });

        
        var students = await _studentService.UpdateStudentASync(id, request, ct);
        return students is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteStudentAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointName("DeleteStudent")]
    [EndpointSummary("Delete a student")]
    public async Task<IActionResult> DeleteStudentAsync(int id, CancellationToken ct)
    {
        var deleted = await _studentService.DeleteStudentAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
    // [HttpGet("n1-demo")]
    // public async Task<IActionResult> DemonstrateN1(CancellationToken cancellationToken)
    // {
    //     await _studentService.DemonstrateN1Async(cancellationToken);
    //     return Ok("Check SQL log for N+1 queries");
    // }


    // [HttpGet("enrollment-report")]
    // public async Task<IActionResult> GetEnrollmentReport(CancellationToken cancellationToken)
    // {
    //     var report = await _studentService.GetEnrollmentReportAsync(cancellationToken);
    //     return Ok(report);
    // }

    //     [HttpGet]
    //     public async Task<IActionResult> GetAll()
    //     {
    //         var students = await _studentService.GetAllAsync();
    //         return Ok(students);
    //     }

    //     [HttpGet("paged")]
    //     public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    //     {
    //         var students = await _studentService.GetPagedAsync(page, pageSize, cancellationToken);
    //         return Ok(students);
    //     }


    //     [HttpGet("{id}")]
    //     public async Task<IActionResult> GetById(int id)
    //     {

    //         var students = await _studentService.GetById(id);
    //         return students != null
    //             ? Ok(students)
    //             : NotFound();
    //     }

    //     [HttpPost]
    //     public async Task<IActionResult> Create([FromBody] StudentCreateRequest student)
    //     {
    //         var studRequest = new Student
    //         {
    //             Name = student.Name,
    //             RegistrationNumber = student.RegistrationNumber,
    //             GPA = student.GPA,
    //             IsActive = true
    //         };
    //         var createdStudent = await _studentService.CreateAsync(studRequest);
    //         if (createdStudent is null)
    //         {
    //             return BadRequest(new { Message = "Invalid Request data: name or registration number is required" });
    //         }
    //         return CreatedAtAction(nameof(GetById), new { id = createdStudent.Id }, createdStudent);
    //     }

    // [HttpPut("{id}")]
    // public async Task<IActionResult> Update(int id, [FromBody] StudentCreateRequest request, CancellationToken cancellationToken)
    // {
    //     var studentRequest = new Student
    //     {
    //         Name = request.Name,
    //         RegistrationNumber = request.RegistrationNumber,
    //         GPA = request.GPA,
    //         IsActive = request.IsActive
    //     };

    //     var updated = await _studentService.UpdateAsync(id, studentRequest, cancellationToken);
    //     return updated is null ? NotFound() : Ok(updated);
    // }

    //     [HttpDelete("{id}")]
    //     public async Task<IActionResult> Delete(int id)
    //     {
    //         var deleted = await _studentService.DeleteAsync(id);
    //         return deleted ? NoContent() : NotFound();
    //     }

    //     [HttpPost("enrollments/archive")]
    // public async Task<IActionResult> BulkArchive([FromQuery] DateTime cutoff, CancellationToken cancellationToken)
    // {
    //     await _studentService.BulkArchiveEnrollmentsAsync(cutoff, cancellationToken);
    //     return Ok("Enrollments archived successfully");
    // }
    //     public record StudentCreateRequest(string RegistrationNumber, string Name, decimal GPA, bool IsActive);
}