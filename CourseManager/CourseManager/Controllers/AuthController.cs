using CourseManager.Data;
using Microsoft.AspNetCore.Mvc;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromForm] string firstName,
        [FromForm] string lastName,
        [FromForm] string shortName,
        [FromForm] string email,
        [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Redirect("/newuser?error=invalid_input");

        var dto = new TeacherDto
        {
            FirstName = firstName,
            LastName = lastName,
            ShortName = shortName,
            Email = email
        };

        var success = await _accountService.RegisterTeacherAsync(dto, password);

        if (!success)
            return Redirect("/newuser?error=registration_failed");

        return Redirect("/login");
    }
}