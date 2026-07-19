using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Enrollments.Queries;

public class GetStudentScheduleHandler(TmsDbContext dbContext) : IRequestHandler<GetStudentScheduleQuery, ScheduleDto>
{
    public async Task<ScheduleDto> Handle(GetStudentScheduleQuery query, CancellationToken ct)
    {
        var items = await dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == query.StudentId)
            .Select(e => new ScheduleItemDto(e.Course.Code, e.Course.Title, "TBD"))
            .ToListAsync(ct);

        return new ScheduleDto(query.StudentId, items);
    }
}