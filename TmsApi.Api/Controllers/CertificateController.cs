using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

/// <summary>
/// Manages certificates issued to students for completed courses.
/// </summary>
[ApiController]
[Route("api/students/{studentId:int}/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(
    IStudentsService studentService,
    ICourseService courseService,
    ICertificateService certificateService) : ControllerBase
{
    /// <summary>
    /// Returns a paginated list of all certificates (active and revoked) held by a student.
    /// </summary>
    /// <param name="studentId">The student's ID.</param>
    /// <param name="request">Paging, search, and ordering options.</param>
    [HttpGet(Name = "ListStudentCertificates")]
    [ProducesResponseType(typeof(PagedResponse<CertificateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List certificates for a student")]
    [EndpointDescription("Returns every certificate the student has ever been issued, including revoked ones. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> GetCertificates(
        int studentId, [FromQuery] PagedRequest request, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(studentId, ct);
        if (student is null) return NotFound();

        return Ok(await certificateService.GetCertificatesAsync(studentId, request, ct));
    }

    /// <summary>
    /// Returns a single certificate by ID, scoped to its owning student.
    /// </summary>
    /// <param name="studentId">The student's ID.</param>
    /// <param name="id">The certificate's ID.</param>
    [HttpGet("{id:int}", Name = nameof(GetCertificate))]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one certificate for a student")]
    [EndpointDescription("Returns 404 if the certificate does not exist, or does not belong to the specified student.")]
    public async Task<IActionResult> GetCertificate(int studentId, int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetCertificateByIdAsync(studentId, id, ct);
        return certificate is not null ? Ok(certificate) : NotFound();
    }

    /// <summary>
    /// Issues a new certificate to a student for a completed course.
    /// </summary>
    /// <param name="studentId">The student's ID.</param>
    /// <param name="request">The course the certificate is being issued for.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Issue a certificate to a student")]
    [EndpointDescription("Returns 404 if the student or course does not exist. Returns 409 if the student already holds an active (non-revoked) certificate for this course.")]
    public async Task<IActionResult> IssueCertificate(
        int studentId, CreateCertificateRequest request, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(studentId, ct);
        if (student is null) return NotFound();

        var course = await courseService.GetByIdAsync(request.CourseId, ct);
        if (course is null) return NotFound();

        if (await certificateService.ExistsForCourseAsync(studentId, request.CourseId, ct))
            return Conflict(new ProblemDetails
            {
                Title = "Certificate already issued",
                Detail = $"Student {studentId} already holds an active certificate for course '{course.Title}'.",
                Status = StatusCodes.Status409Conflict
            });

        var certificate = await certificateService.CreateCertificateAsync(studentId, request, ct);
        return CreatedAtAction(nameof(GetCertificate), new { studentId, id = certificate.Id }, certificate);
    }

    /// <summary>
    /// Revokes a previously issued certificate, preserving it in the student's history.
    /// </summary>
    /// <param name="studentId">The student's ID.</param>
    /// <param name="id">The certificate's ID.</param>
    /// <param name="request">The reason for revocation.</param>
    [HttpPatch("{id:int}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Revoke a certificate")]
    [EndpointDescription("Marks the certificate as revoked without deleting it. Returns 404 if not found, 409 if it was already revoked.")]
    public async Task<IActionResult> RevokeCertificate(
        int studentId, int id, RevokeCertificateRequest request, CancellationToken ct)
    {
        var result = await certificateService.RevokeCertificateAsync(studentId, id, request, ct);

        return result switch
        {
            null => NotFound(),
            false => Conflict(new ProblemDetails
            {
                Title = "Certificate already revoked",
                Detail = $"Certificate {id} was already revoked.",
                Status = StatusCodes.Status409Conflict
            }),
            true => NoContent()
        };
    }
}