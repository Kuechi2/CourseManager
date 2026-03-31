using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseManager.Data;

public abstract class BaseDataService
{
    protected readonly IDbContextFactory<AppDataContext> _dbFactory;
    public event Action? OnChanged;

    protected Guid CurrentTeacherId =>
        Guid.Parse(_httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException());

    private readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseDataService(
        IDbContextFactory<AppDataContext> dbFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbFactory = dbFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    protected void NotifyChanged() => OnChanged?.Invoke();
}