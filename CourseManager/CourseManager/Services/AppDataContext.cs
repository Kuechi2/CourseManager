using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class AppDataContext : IdentityDbContext<Teacher, IdentityRole<Guid>, Guid>
{
    private readonly ITenantService _tenantService;
    private Guid? _cachedSchoolId; // Cache, damit wir nicht bei jedem Query neu abfragen

    public AppDataContext(DbContextOptions<AppDataContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
        // WICHTIG: Hier NICHT mehr direkt zuweisen, sonst Endlosschleife!
    }

    // Lazy-Loading der SchoolId
    public Guid GetSchoolId()
    {
        if (_cachedSchoolId.HasValue)
            return _cachedSchoolId.Value;

        var userId = _tenantService.GetCurrentUserId();
        if (userId == null)
        {
            _cachedSchoolId = Guid.Empty;
            return Guid.Empty;
        }

        // Wir holen jetzt beides: Die ID UND den Status
        var teacherInfo = Set<Teacher>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .Select(u => new { u.ActiveSchoolId, u.IsApproved }) // Projektion auf ein anonymes Objekt
            .FirstOrDefault();

        // Logik: Nur wenn er eine ID hat UND bestätigt wurde
        if (teacherInfo != null && teacherInfo.IsApproved && teacherInfo.ActiveSchoolId != null)
        {
            _cachedSchoolId = teacherInfo.ActiveSchoolId;
            Console.WriteLine($"[TENANT] Zugriff erlaubt: Schule {_cachedSchoolId}");
        }
        else
        {
            _cachedSchoolId = Guid.Empty;
            Console.WriteLine($"[TENANT-INFO] Kein Zugriff (Unapproved oder keine Schule).");
        }

        return _cachedSchoolId.Value;
    }
    // In AppDataContext.cs

    public DbSet<Person> Students { get; set; }
    public DbSet<SeatLayout> SeatLayouts { get; set; }
    public DbSet<CanvasSeatData> Seats { get; set; }
    public DbSet<CourseManager.Data.Course> Courses { get; set; }
    public DbSet<CourseParticipant> CourseParticipants { get; set; }
    public DbSet<RuleSet> RuleSets { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<RuleOccurrence> RuleOccurrences { get; set; }
    public DbSet<CourseAssignment> CourseAssignments { get; set; }
    public DbSet<StudentAssignmentStatus> StudentAssignmentStatuses { get; set; }
    public DbSet<School> Schools { get; set; } // Hier muss sie rein!

    // Falls du die Tabelle unbedingt "Teachers" statt "AspNetUsers" nennen willst:
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Das hier ist lebenswichtig für Identity!

        builder.Entity<Teacher>().ToTable("Teachers");
        builder.Entity<Person>().HasQueryFilter(p => p.SchoolId == GetSchoolId());
        builder.Entity<Course>().HasQueryFilter(c => c.SchoolId == GetSchoolId());
        builder.Entity<RuleSet>().HasQueryFilter(s => s.SchoolId == GetSchoolId());
        builder.Entity<CourseAssignment>().HasQueryFilter(a => a.SchoolId == GetSchoolId());
        builder.Entity<CourseAssignment>()
            .HasMany(a => a.StatusEntries)
            .WithOne()
            .HasForeignKey(s => s.CourseAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}