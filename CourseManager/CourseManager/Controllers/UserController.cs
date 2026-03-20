using CourseManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")] // Route: api/user
[Authorize]
public class UserController(IUserProvider serverUserProvider) : ControllerBase
{
    [HttpGet("id")] // api/user/id
    public async Task<ActionResult<Guid>> GetId()
        => Ok(await serverUserProvider.GetCurrentUserIdAsync());

    [HttpGet("teacher")] // api/user/teacher
    public async Task<ActionResult<TeacherDto>> GetTeacher()
    {
        var teacher = await serverUserProvider.GetCurrentTeacherAsync();
        if (teacher == null) return NotFound();
        return Ok(teacher);
    }

    [HttpGet("schoolid")] // api/user/schoolid
    public async Task<ActionResult<Guid>> GetSchoolId()
        => Ok(await serverUserProvider.GetCurrentSchoolIdAsync());
}