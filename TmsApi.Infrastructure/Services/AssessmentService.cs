using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Interfaces;

namespace TmsApi.Infrastructure.Services;


public class AssessmentService(TmsDbContext dbContext, ILogger<AssessmentService> logger) : IAssessmentService
{
  public async Task<PagedResponse<AssessmentResponseDto>> GetAssessmentsAsync(
        int courseId, PagedRequest request, CancellationToken ct)
    {
        // Step 1: scope to this course, no-tracking
        IQueryable<Assessment> query = dbContext.Assessments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId);

        // Step 2: search filter on title
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => EF.Functions.ILike(a.Title, $"%{request.Search}%"));

        // Step 3: count before paging
        var totalCount = await query.CountAsync(ct);

        // Step 4: whitelist ordering
        query = request.OrderBy switch
        {
            "MaxScore" => request.Descending
                ? query.OrderByDescending(a => a.MaxScore)
                : query.OrderBy(a => a.MaxScore),
            "Weight" => request.Descending
                ? query.OrderByDescending(a => a.Weight)
                : query.OrderBy(a => a.Weight),
            _ => request.Descending
                ? query.OrderByDescending(a => a.Title)
                : query.OrderBy(a => a.Title)
        };

        // Step 5: page + project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AssessmentResponseDto(a.Id, a.Title, a.MaxScore, a.Weight, a.CourseId))
            .ToListAsync(ct);

        // Step 6: wrap
        return new PagedResponse<AssessmentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AssessmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
       return await dbContext.Assessments
       .AsNoTracking()
       .Where(a=>a.CourseId == courseId && a.Id == id)
       .Select(a=> new AssessmentResponseDto(a.Id, a.Title, a.MaxScore, a.Weight, a.CourseId))
         .FirstOrDefaultAsync(ct);
    }


    public async Task<AssessmentResponseDto> CreateAsync(int courseId, CreateAssessmentRequest request, CancellationToken ct)
    {
        var assessment = new Assessment
        {
           Title = request.Title,  
           MaxScore = request.MaxScore,
           Weight = request.Weight,
           CourseId = courseId   
        };
        dbContext.Assessments.Add(assessment);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Created assessment {AssessmentId}-{Title} for course {CourseId}", assessment.Id, assessment.Title, courseId);
        return new AssessmentResponseDto(assessment.Id, assessment.Title, assessment.MaxScore, assessment.Weight, assessment.CourseId);

    }

    public async Task<bool> DeleteAssessmentAsync(int courseId, int id, CancellationToken ct)
    {
       var assessment = await dbContext.Assessments.FirstOrDefaultAsync(a=>a.CourseId == courseId && a.Id == id, ct);
       if(assessment is null) return false;

       dbContext.Assessments.Remove(assessment);
       await dbContext.SaveChangesAsync(ct);
       logger.LogInformation("Deleted assessment {AssessmentId}-{Title} for course {CourseId}", assessment.Id, assessment.Title, courseId);
       return true;
    }

 public async Task<bool?> UpdateAssessmentAsync(int courseId, int id, CreateAssessmentRequest request, CancellationToken ct)
    {
        var assessment = await dbContext.Assessments.FirstOrDefaultAsync(a=>a.CourseId == courseId && a.Id == id, ct);
        if(assessment is null) return null;

        assessment.Title = request.Title;
        assessment.MaxScore = request.MaxScore;
        assessment.Weight = request.Weight;

        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Updated assessment {AssessmentId}-{Title} for course {CourseId}", assessment.Id, assessment.Title, courseId);
        return true;
    }


    public async Task<bool> TitleExistsAsync(int courseId, string title, CancellationToken ct)
    {
        return await dbContext.Assessments.AsNoTracking().AnyAsync(a=>a.CourseId == courseId && a.Title == title, ct);
    }

    public async Task<bool> TitleExistsForUpdateAsync(int courseId, int id, string title, CancellationToken ct)
    {
        return await dbContext.Assessments.AsNoTracking().AnyAsync(a=>a.CourseId == courseId && a.Id != id && a.Title == title, ct);
    }

   
}

