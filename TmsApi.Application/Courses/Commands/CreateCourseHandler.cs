using MediatR;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseHandler(ICourseRepository courseRepository, 
    ICachedCourseService cachedCourseService,
    ILogger<CreateCourseHandler> logger)
    : IRequestHandler<CreateCourseCommand, Result<CourseCreated, CourseError>>
{
    public async Task<Result<CourseCreated, CourseError>> Handle(
        CreateCourseCommand command, CancellationToken ct)
    {
        if (await courseRepository.CourseExistAsync(command.Code, ct))
            return Result<CourseCreated, CourseError>.Failure(
                CourseError.CodeAlreadyExists(command.Code));

        var course = new Course
        {
            Code = command.Code,
            Title = command.Title,
            MaxCapacity = command.MaxCapacity
        };

        var created = await courseRepository.AddAsync(course, ct);
        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        logger.LogInformation("Created course {CourseId} ({Code})", created.Id, created.Code);

        return Result<CourseCreated, CourseError>.Success(
            new CourseCreated(created.Id, created.Code, created.Title, created.MaxCapacity));
    }
}