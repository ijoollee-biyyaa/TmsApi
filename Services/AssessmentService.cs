using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;



public interface IAssessmentService
{
    public Task<IReadOnlyList<Assessment>> GetAllAsync();
    public Task<Assessment?> GetById(string id);
    public Task<Assessment> SubmitQuizAsync(string StudentId, string CourseCode, Quiz quiz);
    public Task<Assessment> SubmitLabAsync(string StudentId, string CourseCode, LabAssignment labAssignment);
    Task<bool> DeleteAsync(string id);
}

public class AssessmentService : IAssessmentService {

private readonly AssessmentStore _assessment;
private readonly ILogger<AssessmentService> _logger;
private readonly IStudentsService _student;
private readonly ICourseService _course;
public AssessmentService(AssessmentStore assessment , ILogger<AssessmentService> logger, IStudentsService student, ICourseService course )
{

    _assessment = assessment;
    _student = student;
    _course = course;
    _logger = logger;
}

public async Task<IReadOnlyList<Assessment>> GetAllAsync()
    {
        IReadOnlyList<Assessment> assessment = _assessment.AssessmentList.Values.ToList();
       return await Task.FromResult(assessment);
      
    }

    public Task<Assessment?> GetById(string id)
    {
        _assessment.AssessmentList.TryGetValue(id, out var assessment);
        if (assessment is null)
         _logger.LogWarning("Assessment {assessmentId} not found",id);
         return  Task.FromResult(assessment);
        
    }
    public async Task<Assessment> SubmitQuizAsync(string studentId, string courseCode, Quiz quiz)
    {
         var student = await _student.GetById(studentId);
         if(student is null)
        {
            _logger.LogWarning("Quiz Submission failed - student {studentId} is not found",studentId);
            throw new TmsDatabaseException($"student {studentId} is not found");
        }
        var course = await _course.GetById(courseCode);
         if(course is null)
        {
            _logger.LogWarning("Quiz Submission failed - Course {CourseCode} is not found",courseCode);
            throw new TmsDatabaseException($"course {courseCode} is not found");
        }

        var grade = quiz.CalculateGrade();

      var newAssessment = new Assessment
{
    Id         = Guid.NewGuid().ToString("N")[..8],
    CourseCode = course.Code,
    StudentId  = student.Id,       
    Type       = "Quiz",
    CreatedAt  = DateTime.UtcNow,   
    Grade      = grade           
};
_assessment.AssessmentList[newAssessment.Id] = newAssessment;
_logger.LogInformation("Quiz  {assessmentId} Created Succesfully",newAssessment.Id);
 return newAssessment;
    }


 public async Task<Assessment> SubmitLabAsync(string studentId, string courseCode, LabAssignment labAssignment)
    {
         var student = await _student.GetById(studentId);
         if(student is null)
        {
            _logger.LogWarning("LabAssessment Submission failed - student {studentId} is not found",studentId);
            throw new TmsDatabaseException("student {studentId} is not found");
        }
        var course = await _course.GetById(courseCode);
         if(course is null)
        {
            _logger.LogWarning("LabAssessment Submission failed - Course {CourseCode} is not found",courseCode);
            throw new TmsDatabaseException("course {courseCode} is not found");
        }

        var grade = labAssignment.CalculateGrade();

      var newAssessment = new Assessment
{
    Id         = Guid.NewGuid().ToString("N")[..8],
    CourseCode = course.Code,
    StudentId  = student.Id,       
    Type       = "Lab",
    CreatedAt  = DateTime.UtcNow,   
    Grade      = grade           
};
_assessment.AssessmentList[newAssessment.Id] = newAssessment;
_logger.LogInformation("Lab Assessment {assessmentId} Created Succesfully",newAssessment.Id);
 return newAssessment;
    }



public Task<bool> DeleteAsync(string id)
    {
       var removed =  _assessment.AssessmentList.Remove(id);
       if (removed)
     _logger.LogInformation("Assessment {assessmentId} is Deleted Succesfully",id);
       else
       _logger.LogWarning("Assessment {assessmentId} is not found", id);
     return Task.FromResult(removed);

    }
}