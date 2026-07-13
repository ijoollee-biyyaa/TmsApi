using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{



   [HttpGet("{id:int}", Name = nameof(GetCourseById))]
   [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
   [EndpointSummary("Get course by ID")]
  [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
{
    var course = await courseService.GetByIdAsync(id, ct);
    if (course is null) return NotFound();

    var links = new List<LinkDto>
    {
        new(linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "self", "GET"),
        new(linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "update", "PUT"),
        new(linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "delete", "DELETE"),
        new(linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id })!, "enrollments", "GET"),
        
    };

    if (course.EnrollmentCount < course.MaxCapacity)
        links.Add(new(linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id })!, "enroll", "POST"));

    var detailDto = new CourseDetailDto
    {
        Id = course.Id,
        Code = course.Code,
        Title = course.Title,
        MaxCapacity = course.MaxCapacity,
        EnrollmentCount = course.EnrollmentCount,
        Links = links
    };

    return Ok(detailDto);
}

[HttpGet]
[ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
[EndpointSummary("List courses with pagination")]
[EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
public async Task<IActionResult> GetCourses(
    [FromQuery] PagedRequest request, CancellationToken ct)
{
    var result = await courseService.GetCoursesAsync(request, ct);
    return Ok(result);
}

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a new course with the provided details. Returns 409 if a course with the same code already exists.")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });

        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);

    }

[HttpPut("{id:int}", Name = nameof(UpdateCourse))]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Update a course")]
    public async Task<IActionResult> UpdateCourse(int id, CreateCourseRequest request, CancellationToken ct)
    {
        if(await courseService.CodeExistUpdateAsync(id, request.Code, ct))
        return Conflict(new ProblemDetails
        {
           Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
        });
        var course = await courseService.UpdateCourseAsync(id, request,ct);
        return course is false ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteCourse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes the specified course. Returns 404 if the course does not exist")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var deleted = await  courseService.DeleteCourseAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

}