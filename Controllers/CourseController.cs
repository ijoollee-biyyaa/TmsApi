using Microsoft.AspNetCore.Mvc;
using TmsApi.Entities;
[ApiController]
[Route("api/course")]
public class CourseController : ControllerBase
{
    
    private readonly ICourseService _courseService;
     private readonly ILogger<CourseController> _logger;   
    public CourseController(ICourseService courseService, ILogger<CourseController> logger)
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById( int id)
    {
        
    var course = await _courseService.GetById(id);
        return course != null
            ? Ok(course)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Course course)
    {
        var createdCourse = await _courseService.CreateAsync(course);
        return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted  = await _courseService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}