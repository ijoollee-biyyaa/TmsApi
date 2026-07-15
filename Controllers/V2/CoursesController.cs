using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Tags("Courses")]
[Produces("application/json")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination (V2 envelope)")]
    [EndpointDescription("Returns courses wrapped in the data/meta/links envelope introduced in V2.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        var page = result.Page;
        var pageSize = result.PageSize;

        return Ok(new
        {
            data = result.Items,
            meta = new
            {
                result.TotalCount,
                page,
                pageSize,
                result.TotalPages,
                result.HasNext,
                result.HasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
                next = result.HasNext ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}" : null,
                prev = result.HasPrevious ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}" : null,
                enroll = "/api/v2/enrollments"
            }
        });
    }
}