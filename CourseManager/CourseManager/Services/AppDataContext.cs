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
    public DbSet<CourseAppointment> CourseAppointments { get; set; }
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
        // 1. Wenn ein Schüler (Person) gelöscht wird -> Lösche seine Regel-Vorkommnisse
        builder.Entity<RuleOccurrence>()
            .HasOne<Person>() // Falls RuleOccurrence eine Navigation zu Person hat
            .WithMany()       // Falls Person eine Liste von RuleOccurrences hat, hier .WithMany(p => p.RuleOccurrences)
            .HasForeignKey(ro => ro.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Wenn ein Schüler gelöscht wird -> Lösche seine Kursteilnahmen
        builder.Entity<CourseParticipant>()
            .HasOne(cp => cp.Person)  // Navigation-Property referenzieren!
            .WithMany()
            .HasForeignKey(cp => cp.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Bestehende Kaskade für Assignments (hast du schon drin)
        builder.Entity<CourseAssignment>()
            .HasMany(a => a.StatusEntries)
            .WithOne()
            .HasForeignKey(s => s.CourseAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Falls StudentAssignmentStatus an einem Schüler hängt:
        builder.Entity<StudentAssignmentStatus>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(s => s.CourseParticipantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Person>()
            .HasOne<Teacher>() // Kein Lambda, da keine Navigations-Property in Person vorhanden
            .WithMany()
            .HasForeignKey(p => p.CreatedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict); // WICHTIG: Verhindert versehentliches Löschen des Lehrers
    }
}