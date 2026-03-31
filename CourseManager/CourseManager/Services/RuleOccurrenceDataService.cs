using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class RuleOccurrenceDataService : BaseDataService, IRuleOccurrenceService
{
    public RuleOccurrenceDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task SaveOccurrenceAsync(RuleOccurrence occurrence)
    {
        using var context = _dbFactory.CreateDbContext();
        context.RuleOccurrences.Add(occurrence);
        await context.SaveChangesAsync();
    }

    public async Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleOccurrences
            .Where(o => o.CourseId == courseId && o.Timestamp >= DateTime.Today)
            .ToListAsync();
    }

    public async Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleOccurrences
            .Where(o => o.PersonId == personId)
            .OrderByDescending(o => o.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end)
    {
        var endOfDay = end.Date.AddDays(1).AddTicks(-1);
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleOccurrences
            .Where(o => o.PersonId == personId &&
                        o.Timestamp >= start.Date && o.Timestamp <= endOfDay)
            .OrderByDescending(o => o.Timestamp)
            .ToListAsync();
    }

    public async Task<List<RuleOccurrence>> GetOccurrencesByCourseAsync(Guid courseId, DateTime start, DateTime end)
    {
        var endOfDay = end.Date.AddDays(1).AddTicks(-1);
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleOccurrences
            .Where(o => o.CourseId == courseId &&
                        o.Timestamp >= start.Date && o.Timestamp <= endOfDay)
            .OrderBy(o => o.Timestamp)
            .ToListAsync();
    }
}