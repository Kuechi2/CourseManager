using CourseManager.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _accountService.RegisterTeacherAsync(request.Teacher, request.Password);
        if (result) return Ok();
        return BadRequest();
    }

    public record RegisterRequest(TeacherDto Teacher, string Password);
}