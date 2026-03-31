using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class TeacherDataService : BaseDataService, ITeacherService
{
    public TeacherDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<List<TeacherDto>> GetTeachersAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        var rawTeachers = await context.Users.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        return rawTeachers.Select(t => new TeacherDto
        {
            Id = t.Id, FirstName = t.FirstName, LastName = t.LastName,
            ShortName = t.ShortName, Email = t.Email,
            ActiveSchoolId = t.ActiveSchoolId, IsAdmin = t.IsAdmin,
            IsApproved = t.IsApproved, PointsBias = t.PointsBias
        }).ToList();
    }

    public async Task AddTeacher(TeacherDto dto)
    {
        using var context = _dbFactory.CreateDbContext();
        var existingTeacher = await context.Users
            .FirstOrDefaultAsync(t => t.Id == dto.Id || t.Email == dto.Email);

        if (existingTeacher != null)
        {
            existingTeacher.FirstName = dto.FirstName;
            existingTeacher.LastName = dto.LastName;
            existingTeacher.ShortName = dto.ShortName ?? "KEIN";
            existingTeacher.Email = dto.Email;
            existingTeacher.PointsBias = dto.PointsBias;
            existingTeacher.NormalizedEmail = dto.Email?.ToUpper();
            existingTeacher.UserName = dto.Email ?? dto.LastName;
            existingTeacher.IsAdmin = dto.IsAdmin;
            existingTeacher.IsApproved = dto.IsApproved;
            existingTeacher.NormalizedUserName = existingTeacher.UserName?.ToUpper();
            existingTeacher.ActiveSchoolId = dto.ActiveSchoolId;
            existingTeacher.SchoolId = dto.ActiveSchoolId;
            context.Users.Update(existingTeacher);
        }
        else
        {
            context.Users.Add(new Teacher
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                FirstName = dto.FirstName, LastName = dto.LastName,
                ShortName = dto.ShortName ?? "KEIN",
                Email = dto.Email, NormalizedEmail = dto.Email?.ToUpper(),
                UserName = dto.Email ?? dto.LastName,
                IsAdmin = false, IsApproved = false, PointsBias = dto.PointsBias,
                NormalizedUserName = (dto.Email ?? dto.LastName).ToUpper()
            });
        }

        await context.SaveChangesAsync();
        NotifyChanged();
    }
}