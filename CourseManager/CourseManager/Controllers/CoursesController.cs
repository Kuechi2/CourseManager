using Microsoft.AspNetCore.Mvc;
using CourseManager.Data;

namespace CourseManager.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> Get()
    {
        var courses = await _service.GetCoursesWithParticipantsAsync();
        return Ok(courses);
    }
    [HttpGet("{id:guid}")] // URL: api/courses/GUID-HIER
    public async Task<ActionResult<Course>> Get(Guid id)
    {
        var course = await _service.GetCourseWithParticipantsAsync(id);
        if (course == null) return NotFound();
        return Ok(course);
    }
    [HttpPost] // URL: api/courses
    public async Task<ActionResult> Post([FromBody] Course course)
    {
        if (course == null) return BadRequest();
        Console.WriteLine($"Empfange Kurs: {course.Title} mit {course.Enrollments?.Count ?? 0} Teilnehmern");
        await _service.SaveCourseAsync(course);
        return Ok();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(Guid id)
    {
        var course = await _service.GetCourseByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }
    [HttpDelete("{id:guid}")] // URL: DELETE api/courses/GUID
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteCourseAsync(id);
        if (!success) return NotFound();
        return Ok();
    }
}