using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;

[ApiController]
[Route("api/students")]
public class StudentController : ControllerBase
{
    
    private readonly IStudentsService _studentService;
     private readonly ILogger<StudentController> _logger;   
    public StudentController(IStudentsService studentsService, ILogger<StudentController> logger)
    {
        _studentService = studentsService;
        _logger = logger;
    }
   

   [HttpGet]
   public async Task<IActionResult> GetAll()
    {
        var students = await _studentService.GetAllAsync();
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        
    var students = await _studentService.GetById(id);
        return students != null
            ? Ok(students)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Student student)
    {
        var createdStudent = await _studentService.CreateAsync(student);
        return CreatedAtAction(nameof(GetById), new { id = createdStudent.Id }, createdStudent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted  = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}