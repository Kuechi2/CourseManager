using CourseManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _service;
    public TeachersController(ITeacherService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<TeacherDto>>> Get()
    {
        // Der Service holt die echten Teacher-Objekte aus der DB
        var teachers = await _service.GetTeachersAsync();

        // NUR HIER im Controller mappen wir auf das DTO für das JSON-Format
        var dtos = teachers.Select(t => new TeacherDto
        {
            Id = t.Id,
            FirstName = t.FirstName,
            LastName = t.LastName,
            ShortName = t.ShortName,
            Email = t.Email,
            ActiveSchoolId = t.ActiveSchoolId,
            IsAdmin = t.IsAdmin,
            IsApproved = t.IsApproved,
            PointsBias = t.PointsBias
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TeacherDto teacherDto)
    {
        // Wenn der Client einen neuen Lehrer schickt, nutzt der Service wieder die echte Logik
        await _service.AddTeacher(teacherDto);
        return Ok();
    }
}
