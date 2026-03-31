using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class SchoolDataService : BaseDataService, ISchoolService
{
    public SchoolDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<List<School>> GetAllSchoolsGlobalAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Schools.IgnoreQueryFilters().OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<School> GetSchoolWithIdAsync(Guid schoolId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId)
               ?? throw new Exception($"Schule mit Id {schoolId} nicht gefunden!");
    }

    public async Task<bool> IsSchoolNameTakenAsync(string name)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Schools.IgnoreQueryFilters()
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<School> CreateSchoolAsync(Guid teacherId, string name, string address)
    {
        using var context = _dbFactory.CreateDbContext();
        var school = new School
        {
            Id = Guid.NewGuid(), Name = name, Address = address,
            City = "Unbekannt", Email = "info@schule.de",
            AccessCode = "START2024", GlobalRuleAverage = 0
        };
        context.Schools.Add(school);

        var teacher = await context.Users.FirstOrDefaultAsync(u => u.Id == teacherId);
        if (teacher != null)
        {
            teacher.SchoolId = school.Id;
            teacher.ActiveSchoolId = school.Id;
            teacher.IsApproved = true;
            teacher.IsAdmin = true;
            context.Users.Update(teacher);
        }

        await context.SaveChangesAsync();
        return school;
    }
}