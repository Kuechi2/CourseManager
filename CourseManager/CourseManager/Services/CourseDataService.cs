using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class CourseDataService : BaseDataService, ICourseService
{
    public CourseDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<List<Course>> GetCoursesAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Courses.Where(c => c.TeacherId == CurrentTeacherId).ToListAsync();
    }

    public async Task<List<Course>> GetCoursesWithParticipantsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Courses
            .Where(c => c.TeacherId == CurrentTeacherId)
            .Include(c => c.Enrollments!).ThenInclude(e => e.Person)
            .Include(c => c.SeatLayouts!).ThenInclude(l => l.Seats!)
                .ThenInclude(s => s.Participant).ThenInclude(p => p!.Person)
            .Include(c => c.Appointments)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseWithParticipantsAsync(Guid id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Courses
            .Include(c => c.Enrollments!).ThenInclude(e => e.Person)
            .Include(c => c.SeatLayouts!).ThenInclude(l => l.Seats!)
                .ThenInclude(s => s.Participant).ThenInclude(p => p!.Person)
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Courses
            .Include(c => c.Enrollments).ThenInclude(e => e.Person)
            .Include(c => c.SeatLayouts!).ThenInclude(l => l.Seats!)
                .ThenInclude(s => s.Participant).ThenInclude(p => p!.Person)
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task SaveCourseAsync(Course course)
    {
        Console.WriteLine($"[TRACE] Start SaveCourseAsync für Kurs: {course.Title}");
        using var context = _dbFactory.CreateDbContext();

        var dbCourse = await context.Courses
            .Include(c => c.Enrollments)
            .Include(c => c.SeatLayouts).ThenInclude(l => l.Seats)
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == course.Id);

        if (dbCourse == null)
        {
            Console.WriteLine("[TRACE] Kurs neu -> Add");
            context.Courses.Add(course);
        }
        else
        {
            context.Entry(dbCourse).CurrentValues.SetValues(course);
            SyncEnrollments(context, dbCourse, course);
            SyncSeatLayouts(context, dbCourse, course);
            SyncAppointments(context, dbCourse, course);
        }

        try
        {
            var affectedRows = await context.SaveChangesAsync();
            Console.WriteLine($"[TRACE] Erfolg! Betroffene Zeilen: {affectedRows}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine("!!! CONCURRENCY FEHLER !!!");
            foreach (var entry in ex.Entries)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();
                Console.WriteLine(databaseValues == null
                    ? $"[ERROR] {entry.Entity.GetType().Name} existiert nicht in DB!"
                    : $"[ERROR] {entry.Entity.GetType().Name} hat veralteten RowVersion!");
            }
            throw;
        }
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        using var context = _dbFactory.CreateDbContext();
        var c = await context.Courses.FindAsync(id);
        if (c == null) return false;
        context.Courses.Remove(c);
        await context.SaveChangesAsync();
        return true;
    }

    // ── private Sync-Helpers ──────────────────────────────────────────────────

    private void SyncEnrollments(AppDataContext context, Course dbCourse, Course uiCourse)
    {
        var uiPersonIds = uiCourse.Enrollments?.Select(e => e.PersonId).ToHashSet() ?? new();
        foreach (var rem in dbCourse.Enrollments.Where(e => !uiPersonIds.Contains(e.PersonId)).ToList())
            context.Remove(rem);

        foreach (var uiEnroll in uiCourse.Enrollments ?? new())
        {
            var dbEnroll = dbCourse.Enrollments.FirstOrDefault(e => e.PersonId == uiEnroll.PersonId);
            if (dbEnroll == null)
            {
                uiEnroll.CourseId = dbCourse.Id;
                uiEnroll.Person = null;
                dbCourse.Enrollments.Add(uiEnroll);
            }
            else
            {
                dbEnroll.PosX = uiEnroll.PosX;
                dbEnroll.PosY = uiEnroll.PosY;
            }
        }
    }

    private void SyncSeatLayouts(AppDataContext context, Course dbCourse, Course uiCourse)
    {
        foreach (var dbLayout in dbCourse.SeatLayouts.ToList())
            if (!uiCourse.SeatLayouts.Any(l => l.Id == dbLayout.Id))
                context.SeatLayouts.Remove(dbLayout);

        foreach (var uiLayout in uiCourse.SeatLayouts)
        {
            var dbLayout = dbCourse.SeatLayouts.FirstOrDefault(l => l.Id == uiLayout.Id);
            if (dbLayout == null)
            {
                uiLayout.CourseId = dbCourse.Id;
                dbCourse.SeatLayouts.Add(uiLayout);
                context.Entry(uiLayout).State = EntityState.Added;
                foreach (var seat in uiLayout.Seats ?? new())
                    context.Entry(seat).State = EntityState.Added;
            }
            else
            {
                context.Entry(dbLayout).CurrentValues.SetValues(uiLayout);
                SyncSeats(context, dbLayout, uiLayout);
            }
        }
    }

    private void SyncSeats(AppDataContext context, SeatLayout dbLayout, SeatLayout uiLayout)
    {
        foreach (var dbSeat in dbLayout.Seats.ToList())
            if (!uiLayout.Seats.Any(s => s.Id == dbSeat.Id))
                context.Seats.Remove(dbSeat);

        foreach (var uiSeat in uiLayout.Seats)
        {
            var dbSeat = dbLayout.Seats.FirstOrDefault(s => s.Id == uiSeat.Id);
            if (dbSeat == null)
            {
                uiSeat.SeatLayoutId = dbLayout.Id;
                dbLayout.Seats.Add(uiSeat);
                context.Entry(uiSeat).State = EntityState.Added;
            }
            else
            {
                dbSeat.PosX = uiSeat.PosX;
                dbSeat.PosY = uiSeat.PosY;
                dbSeat.CourseParticipantId = uiSeat.CourseParticipantId;
            }
        }
    }

    private void SyncAppointments(AppDataContext context, Course dbCourse, Course uiCourse)
    {
        foreach (var dbApp in dbCourse.Appointments.ToList())
            if (!uiCourse.Appointments.Any(a => a.Id == dbApp.Id))
                context.CourseAppointments.Remove(dbApp);

        foreach (var uiApp in uiCourse.Appointments)
        {
            var dbApp = dbCourse.Appointments.FirstOrDefault(a => a.Id == uiApp.Id);
            if (dbApp == null)
            {
                uiApp.CourseId = dbCourse.Id;
                dbCourse.Appointments.Add(uiApp);
                context.Entry(uiApp).State = EntityState.Added;
            }
            else
            {
                context.Entry(dbApp).CurrentValues.SetValues(uiApp);
            }
        }
    }
}