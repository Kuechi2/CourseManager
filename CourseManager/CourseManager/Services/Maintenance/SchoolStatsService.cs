// Datei: Services/Maintenance/SchoolStatsService.cs
using CourseManager.Data;
using Microsoft.EntityFrameworkCore;

public class SchoolStatsService
{
    private readonly IDbContextFactory<AppDataContext> _dbFactory;

    public SchoolStatsService(IDbContextFactory<AppDataContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task UpdateGlobalAveragesAsync()
    {
        using var context = _dbFactory.CreateDbContext();

        // Berechnung der Averages pro Schule basierend auf Teacher.PointsBias
        var biasData = await context.Users
            .Where(t => t.SchoolId != null)
            .GroupBy(t => t.SchoolId)
            .Select(g => new { SchoolId = g.Key, Avg = g.Average(t => t.PointsBias) })
            .ToListAsync();

        var schoolIds = biasData.Select(d => d.SchoolId).ToList();
        var schools = await context.Schools.Where(s => schoolIds.Contains(s.Id)).ToListAsync();

        foreach (var school in schools)
        {
            school.GlobalRuleAverage = biasData.First(d => d.SchoolId == school.Id).Avg;
        }

        await context.SaveChangesAsync();
    }
}