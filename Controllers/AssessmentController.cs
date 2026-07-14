using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

/// <summary>
/// Manages assessments (graded components such as exams or assignments) belonging to a course.
/// </summary>
[ApiController]
[Route("api/courses/{courseId:int}/assessments")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController(
    ICourseService courseService,
    IAssessmentService assessmentService) : ControllerBase
{
    /// <summary>
    /// Returns a paginated list of assessments for the specified course.
    /// </summary>
    /// <param name="courseId">The parent course's ID.</param>
    /// <param name="request">Paging, search, and ordering options.</param>
    [HttpGet(Name = "ListCourseAssessments")]
    [ProducesResponseType(typeof(PagedResponse<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List assessments for a course")]
    [EndpointDescription("Returns a paginated list of assessments belonging to the specified course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetAssessments(
        int courseId, [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        return Ok(await assessmentService.GetAssessmentsAsync(courseId, request, ct));
    }

    /// <summary>
    /// Returns a single assessment by ID, scoped to its parent course.
    /// </summary>
    /// <param name="courseId">The parent course's ID.</param>
    /// <param name="id">The assessment's ID.</param>
    [HttpGet("{id:int}", Name = nameof(GetAssessment))]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one assessment for a course")]
    [EndpointDescription("Returns 404 if the assessment does not exist, or does not belong to the specified course.")]
    public async Task<IActionResult> GetAssessment(int courseId, int id, CancellationToken ct)
    {
        var assessment = await assessmentService.GetByIdAsync(courseId, id, ct);
        return assessment is not null ? Ok(assessment) : NotFound();
    }

    /// <summary>
    /// Creates a new assessment under the specified course.
    /// </summary>
    /// <param name="courseId">The parent course's ID.</param>
    /// <param name="request">The assessment's title, max score, and weight.</param>
    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create an assessment for a course")]
    [EndpointDescription("Returns 404 if the course does not exist. Returns 409 if an assessment with the same title already exists for this course.")]
    public async Task<IActionResult> CreateAssessment(
        int courseId, CreateAssessmentRequest request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        if (await assessmentService.TitleExistsAsync(courseId, request.Title, ct))
            return Conflict(new ProblemDetails
            {
                Title = "Assessment title already exists",
                Detail = $"Course '{course.Title}' already has an assessment titled '{request.Title}'.",
                Status = StatusCodes.Status409Conflict
            });

        var assessment = await assessmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetAssessment), new { courseId, id = assessment.Id }, assessment);
    }

    /// <summary>
    /// Updates an existing assessment's title, max score, and weight.
    /// </summary>
    /// <param name="courseId">The parent course's ID.</param>
    /// <param name="id">The assessment's ID.</param>
    /// <param name="request">The updated title, max score, and weight.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an assessment")]
    [EndpointDescription("Returns 404 if the course or assessment does not exist. Returns 409 if another assessment in this course already uses the requested title.")]
    public async Task<IActionResult> UpdateAssessment(
        int courseId, int id, CreateAssessmentRequest request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        if (await assessmentService.TitleExistsForUpdateAsync(courseId, id, request.Title, ct))
            return Conflict(new ProblemDetails
            {
                Title = "Assessment title already exists",
                Detail = $"Course '{course.Title}' already has another assessment titled '{request.Title}'.",
                Status = StatusCodes.Status409Conflict
            });

        var updated = await assessmentService.UpdateAssessmentAsync(courseId, id, request, ct);
        return updated is null ? NotFound() : NoContent();
    }

    /// <summary>
    /// Deletes an assessment from a course.
    /// </summary>
    /// <param name="courseId">The parent course's ID.</param>
    /// <param name="id">The assessment's ID.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete an assessment")]
    [EndpointDescription("Permanently deletes the specified assessment. Returns 404 if it does not exist.")]
    public async Task<IActionResult> DeleteAssessment(int courseId, int id, CancellationToken ct)
    {
        var deleted = await assessmentService.DeleteAssessmentAsync(courseId, id, ct);
        return deleted ? NoContent() : NotFound();
    }
}