using Microsoft.AspNetCore.Mvc;

[ApiController]
// Wir lassen die Basis-Route weg oder setzen sie sehr allgemein
[Route("api")]
public class OccurrencesController : ControllerBase
{
    private readonly IStudentService _service;

    public OccurrencesController(IStudentService service)
    {
        _service = service;
    }

    // Dieser Pfad matcht jetzt exakt deine Client-Anfrage
    [HttpGet("courses/{courseId}/occurrences/today")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetToday(Guid courseId)
    {
        var list = await _service.GetTodayOccurrencesAsync(courseId);
        return Ok(list);
    }
    [HttpGet("persons/{personId}/occurrences/last/{count}")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetLastN(Guid personId, int count)
    {
        // Jetzt wird die richtige ID an den Service gereicht
        var list = await _service.GetLastOccurrencesAsync(personId, count);
        return Ok(list);
    }

    // Dein bestehender POST-Endpunkt
    [HttpPost("occurrences")]
    public async Task<IActionResult> Post([FromBody] RuleOccurrence occurrence)
    {
        await _service.SaveOccurrenceAsync(occurrence);
        return Ok();
    }
    [HttpGet("persons/{personId}/occurrences")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetByDate(
    Guid personId,
    [FromQuery] DateTime start,
    [FromQuery] DateTime end)
    {
        // Der Service nutzt jetzt deinen sauberen Factory-Ansatz
        var list = await _service.GetOccurrencesByDateAsync(personId, start, end);
        return Ok(list);
    }
}