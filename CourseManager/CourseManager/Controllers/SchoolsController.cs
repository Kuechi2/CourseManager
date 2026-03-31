using CourseManager.Client.Pages;
using CourseManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class SchoolsController : ControllerBase
{
    private readonly AppDataContext _context;
    private readonly ISchoolService _service;
    public SchoolsController(AppDataContext context, ISchoolService service)
    {
        _context = context;
        _service = service;
    }

    // GET: api/schools/allschools
    [HttpGet("allschools")]
    public async Task<ActionResult<IEnumerable<School>>> GetAllSchools()
    {
        // WICHTIG: .IgnoreQueryFilters() schaltet den Tenant-Filter aus,
        // damit auch Lehrer ohne Schule alle Optionen sehen können.
        var schools = await _context.Schools
            .IgnoreQueryFilters()
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(schools);
    }
    [HttpGet("schools/{schoolId}")]
    public async Task<ActionResult<School>> GetSchoolWithId(Guid schoolId)
    {
        var school = await _service.GetSchoolWithIdAsync(schoolId);
        if (school == null)
        {
            return NotFound($"Schule mit Id {schoolId} nicht gefunden!");
        }
        return Ok(school);
    }
    // NEU: Endpunkt zum Gründen einer Schule
    [HttpPost("create")]
    public async Task<ActionResult<School>> Create([FromBody] SchoolCreateDto dto)
    {
        // Hier rufen wir deine neue Lord-Logik im Service auf
        var newSchool = await _service.CreateSchoolAsync(dto.TeacherId, dto.Name, dto.Address);
        return Ok(newSchool);
    }
    public record SchoolCreateDto(Guid TeacherId, string Name, string Address);
}