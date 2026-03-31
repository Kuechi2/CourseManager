using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CourseManager.Data;

public class AppDataContextFactory : IDesignTimeDbContextFactory<AppDataContext>
{
    public AppDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();
        optionsBuilder.UseSqlite("Data Source=geometry.db");

        // Stub-Implementierung für Design-Time (keine echte Tenant-Logik nötig)
        return new AppDataContext(optionsBuilder.Options, new DesignTimeTenantService());
    }
}

// Dummy-Service damit der Konstruktor zufrieden ist
internal class DesignTimeTenantService : ITenantService
{
    public Guid? GetCurrentUserId() => null;
}