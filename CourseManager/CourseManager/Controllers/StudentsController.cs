using Microsoft.AspNetCore.Mvc;
using CourseManager.Data;

namespace CourseManager.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // Dies macht die URL "api/students" erreichbar
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Person>>> Get()
    {
        var students = await _studentService.GetStudentsAsync();
        return Ok(students);
    }
    [HttpPost] 
    public async Task<ActionResult> Post([FromBody] Person student)
    {
        if (student == null)
        {
            return BadRequest("Schüler-Daten waren leer.");
        }
        try
        {
            await _studentService.SaveStudentAsync(student);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ein unerwarteter Fehler ist aufgetreten.");
        }
    }
    [HttpDelete("{id}")] // Erwartet api/students/DEINE-GUID
    public async Task<ActionResult> Delete(Guid id)
    {
        await _studentService.DeleteStudentAsync(id);
        return Ok();
    }
}