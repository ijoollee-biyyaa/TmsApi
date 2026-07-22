using MediatR;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Enrollments.Commands;

public class EnrollStudentHandler(IEnrollmentRepository enrollmentRepo,
    ICourseRepository courseRepo, ILogger<EnrollStudentHandler> logger)
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, EnrollmentError>>
{
    public async Task<Result<EnrollmentCreated, EnrollmentError>> Handle(
        EnrollStudentCommand command, CancellationToken ct)
    {
        var course = await courseRepo.GetByCodeAsync(command.CourseCode, ct);

        if (course is null)
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseNotFound(command.CourseCode));

        if (course.Enrollments.Count >= course.MaxCapacity)
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseFull(course.Title, course.MaxCapacity));

        var alreadyEnrolled = await enrollmentRepo.ExistsAsync(command.StudentId, course.Id, ct);

        if (alreadyEnrolled)
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.AlreadyEnrolled(command.StudentId, command.CourseCode));

        var enrollment = new Enrollment
        {
            StudentId = command.StudentId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        };

        await enrollmentRepo.AddAsync(enrollment, ct);

        logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseCode}", command.StudentId, command.CourseCode);

        return Result<EnrollmentCreated, EnrollmentError>.Success(
            new EnrollmentCreated(enrollment.Id, enrollment.StudentId, course.Code));
    }
}