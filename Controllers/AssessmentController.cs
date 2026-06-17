using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/assessments")]
public class AssessmentController(IAssessmentService assessmentService) : ControllerBase
{
   
   [HttpGet]
   public async Task<IActionResult> GetAllAsync()
    {
        var assessment = assessmentService.GetAllAsync();
        return Ok(assessment);
    }

     [HttpGet("{id}")]
  public async Task<IActionResult> GetById(string id)
    {
        var assessment = assessmentService.GetById(id);
        return assessment is not null ? Ok(assessment) : NotFound();
    }

[HttpPost("quiz")]
public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest     req)
    {
       
        var quiz = new Quiz
        {
          
           Title = req.Title,
            CorrectAnswers = req.CorrectAnswers,
             TotalQuestions = req.TotalQuestions  
        };
        var results = await assessmentService.SubmitQuizAsync(req.StudentId, req.CourseCode, quiz);
        return CreatedAtAction(nameof(GetById), new{id = results.Id },results);
        
    }


[HttpPost("Lab")]
public async Task<IActionResult> SubmitLab([FromBody] SubmitLabRequest req)
    {
       
        var lab = new LabAssignment
        {
          
           Title = req.Title,
           CodeQualityScore = req.CodeQualityScore,
           FunctionalityScore = req.FunctionalityScore  
        };
        var results = await assessmentService.SubmitLabAsync(req.StudentId, req.CourseCode, lab);
        return CreatedAtAction(nameof(GetById), new{id = results.Id },results);
        
    }

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
    {
       
       var removed =  await assessmentService.DeleteAsync(id);
       return removed ? NoContent(): NotFound();
        
    }
    
public record SubmitQuizRequest(
    string StudentId, string CourseCode, string Title, int CorrectAnswers, int TotalQuestions);

public record SubmitLabRequest(
    string StudentId, string CourseCode, string Title, decimal FunctionalityScore, decimal CodeQualityScore);
}