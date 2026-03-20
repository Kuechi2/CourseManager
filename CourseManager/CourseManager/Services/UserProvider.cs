using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CourseManager.Data;
using Microsoft.EntityFrameworkCore;
public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private TeacherDto? _cachedTeacher;

    public UserProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdString, out var guid))
        {
            return guid;
        }
        return Guid.Empty;
    }

    public async Task<TeacherDto> GetCurrentTeacherAsync()
    {
        if (_cachedTeacher != null) return _cachedTeacher;
        var userId = await GetCurrentUserIdAsync();
        if (userId == Guid.Empty) return null!;
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDataContext>();
        var teacher = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == userId);

        if (teacher != null)
        {
            _cachedTeacher = new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                ShortName = teacher.ShortName,
                Email = teacher.Email,
                PointsBias = teacher.PointsBias,
                ActiveSchoolId = teacher.ActiveSchoolId, // Das ist das Goldstück!
                IsApproved = teacher.IsApproved,
                IsAdmin = teacher.IsAdmin,
            };
        }

        return _cachedTeacher!;
    }

    public async Task<Guid> GetCurrentSchoolIdAsync()
    {
        var teacher = await GetCurrentTeacherAsync();
        return teacher?.ActiveSchoolId ?? Guid.Empty;
    }
}