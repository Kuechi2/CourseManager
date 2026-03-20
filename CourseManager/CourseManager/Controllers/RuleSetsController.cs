using CourseManager.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RuleSetsController : ControllerBase
{
    private readonly IStudentService _service;

    public RuleSetsController(IStudentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<RuleSet>>> GetRuleSets()
    {
        return await _service.GetRuleSetsAsync();
    }

    [HttpPost]
    public async Task<IActionResult> SaveRuleSet(RuleSet ruleSet)
    {
        foreach (var r in ruleSet.Rules) Console.WriteLine($"Speichere Regel: {r.Id}");
        await _service.SaveRuleSetAsync(ruleSet);
        return Ok();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<RuleSet>> GetRuleSet(Guid id)
    {
        var set = await _service.GetRuleSetByIdAsync(id);
        return set == null ? NotFound() : Ok(set);
    }
}