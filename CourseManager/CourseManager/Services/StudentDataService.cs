using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class StudentDataService : BaseDataService, IStudentService
{
    public StudentDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<List<Person>> GetStudentsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Students.OrderBy(s => s.LastName).ToListAsync();
    }

    public async Task SaveStudentAsync(Person student)
    {
        Console.WriteLine($"[TRACE] SaveStudentAsync aufgerufen für: {student.FirstName} {student.LastName}");
        using var context = _dbFactory.CreateDbContext();
        var dbStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        if (dbStudent == null)
        {
            bool exists = await context.Students.AnyAsync(s =>
                s.FirstName.ToLower() == student.FirstName.ToLower() &&
                s.LastName.ToLower() == student.LastName.ToLower() &&
                s.BirthDate.Date == student.BirthDate.Date);
            if (exists) throw new InvalidOperationException("Diese Person existiert bereits.");
            if (student.Id == Guid.Empty) student.Id = Guid.NewGuid();
            student.SchoolId = context.GetSchoolId();
            context.Students.Add(student);
        }
        else
        {
            Console.WriteLine($"[TRACE] Aktualisiere Student: {student.FirstName} {student.LastName}");
            student.SchoolId = context.GetSchoolId();
            context.Entry(dbStudent).CurrentValues.SetValues(student);
        }
        await context.SaveChangesAsync();
        NotifyChanged();
    }

    public async Task DeleteStudentAsync(Guid id)
    {
        using var context = _dbFactory.CreateDbContext();
        var student = await context.Students.FindAsync(id);
        if (student != null)
        {
            context.Students.Remove(student);
            await context.SaveChangesAsync();
            NotifyChanged();
        }
    }
}