using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/course")]
public class CourseController : ControllerBase
{
    
    private readonly ICourseService _courseService;
     private readonly ILogger<StudentController> _logger;   
    public CourseController(ICourseService courseService, ILogger<StudentController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }
   

   [HttpGet]
   public async Task<IActionResult> GetAll()
    {
        var course = await _courseService.GetAllAsync();
        return Ok(course);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetById( string code)
    {
        
    var course = await _courseService.GetById(code);
        return course != null
            ? Ok(course)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Course course)
    {
        var createdCourse = await _courseService.CreateAsync(course);
        return CreatedAtAction(nameof(GetById), new { code = createdCourse.Code }, createdCourse);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete([FromRoute] string code)
    {
        var deleted  = await _courseService.DeleteAsync(code);
        return deleted ? NoContent() : NotFound();
    }
}