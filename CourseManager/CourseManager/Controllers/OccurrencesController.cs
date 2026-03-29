using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class OccurrencesController : ControllerBase
{
    private readonly IStudentService _service;
    public OccurrencesController(IStudentService service) => _service = service;

    [HttpGet("courses/{courseId}/occurrences/today")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetToday(Guid courseId)
        => Ok(await _service.GetTodayOccurrencesAsync(courseId));

    [HttpGet("courses/{courseId}/occurrences/range")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetByCourseRange(
        Guid courseId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        => Ok(await _service.GetOccurrencesByCourseAsync(courseId, start, end));

    [HttpGet("persons/{personId}/occurrences/last/{count}")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetLastN(Guid personId, int count)
        => Ok(await _service.GetLastOccurrencesAsync(personId, count));

    [HttpPost("occurrences")]
    public async Task<IActionResult> Post([FromBody] RuleOccurrence occurrence)
    {
        await _service.SaveOccurrenceAsync(occurrence);
        return Ok();
    }

    [HttpGet("persons/{personId}/occurrences")]
    public async Task<ActionResult<List<RuleOccurrence>>> GetByDate(
        Guid personId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        => Ok(await _service.GetOccurrencesByDateAsync(personId, start, end));
}