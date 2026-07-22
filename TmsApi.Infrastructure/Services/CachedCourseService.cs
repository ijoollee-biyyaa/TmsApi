using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService courseService,
    ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request, CancellationToken ct)
    {
        var key = CacheKeys.CoursesPage(
            request.Page,
            request.PageSize,
            request.Search,
            request.OrderBy,
            request.Descending);

        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (courseService, request), // state — avoids closure allocation
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} — fetching from DB", key);
                return await state.courseService.GetCoursesAsync(state.request, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return result;
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var key = CacheKeys.Course(id);
        var dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            (courseService, id),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} — fetching from DB", key);
                return await state.courseService.GetByIdAsync(state.id, token);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
            logger.LogInformation("Cache HIT for {Key}", key);

        return result;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}