using CourseManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly ITaskAssistanceService _service;

    public AssignmentsController(ITaskAssistanceService service)
    {
        _service = service;
    }

    // Alle Aufgaben eines Kurses laden
    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<List<CourseAssignment>>> GetAssignments(Guid courseId)
    {
        var assignments = await _service.GetAssignmentsForCourseAsync(courseId);
        return Ok(assignments);
    }

    // Neue Aufgabe erstellen
    [HttpPost]
    public async Task<ActionResult<CourseAssignment>> CreateAssignment([FromBody] CourseAssignment assignment)
    {
        // Die SchoolId wird im Service idealerweise automatisch gesetzt, 
        // falls nicht, hier aus dem Teacher-Profil ziehen.
        var result = await _service.CreateAssignmentAsync(assignment);
        return Ok(result);
    }

    // Den Status für einen Schüler umschalten (Toggle)
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleStatus([FromBody] ToggleRequest request)
    {
        await _service.ToggleAssignmentStatusAsync(request.AssignmentId, request.ParticipantId);
        return Ok();
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Ungültige ID");

        try
        {
            await _service.DeleteAssignmentAsync(id);

            return NoContent(); // Alles okay, Objekt gelöscht
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Fehler beim Löschen: {ex.Message}");
        }
    }
}

// Hilfsklasse für den Request
public class ToggleRequest
{
    public Guid AssignmentId { get; set; }
    public Guid ParticipantId { get; set; }
}