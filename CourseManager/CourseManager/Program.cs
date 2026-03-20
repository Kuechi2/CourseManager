using CourseManager.Client.Pages;
using CourseManager.Components;
using CourseManager.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

// 1. VERBINDUNGSSTRING
var connectionString = "Data Source=geometry.db";

// 2. DATENBANK-KONFIGURATION
// Wir registrieren die Factory explizit als SCOPED, damit sie mit Identity harmoniert
builder.Services.AddDbContextFactory<AppDataContext>(
    options => options.UseSqlite(connectionString),
    ServiceLifetime.Scoped); // <-- DAS ist der entscheidende Retter!

// Identity braucht den normalen Context (ebenfalls Scoped)
builder.Services.AddDbContext<AppDataContext>(
    options => options.UseSqlite(connectionString));

// 3. IDENTITY (Unverändert, aber wichtig für den Context)
builder.Services.AddIdentity<Teacher, IdentityRole<Guid>>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDataContext>()
.AddDefaultTokenProviders();

// 4. BACKGROUND SERVICES
//builder.Services.AddScoped<SchoolStatsService>();
//builder.Services.AddHostedService<DailyMaintenanceBackgroundService>();
// 3. ANWENDUNGS-SERVICES
// In der Program.cs des SERVER-Projekts
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAccountService, TeacherService>(); 
builder.Services.AddScoped<IUserProvider, UserProvider>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddCascadingAuthenticationState(); // Wichtig für Blazor & Identity
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
// Den TenantService als Scoped registrieren, damit er pro Benutzer-Session existiert

var app = builder.Build();
app.MapPost("/auth/login", async (
    [FromForm] string email,
    [FromForm] string password,
    SignInManager<Teacher> signInManager) =>
{
    var result = await signInManager.PasswordSignInAsync(email, password, true, false);
    if (result.Succeeded)
    {
        return Results.Redirect("/");
    }
    
    return Results.Redirect("/login?error=true");
}).DisableAntiforgery();
app.MapPost("/auth/logout", async (SignInManager<Teacher> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).DisableAntiforgery();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDataContext>>();
        await DbInitializer.SeedData(factory);
        Console.WriteLine("--- Datenbank-Seeding erfolgreich abgeschlossen ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--- Kritischer Fehler beim Seeding: {ex.Message} ---");
        // Falls hier ein Fehler kommt, liegt es oft an einer korrupten .db Datei
    }
}
app.MapPost("/auth/register", async (
    [FromForm] string firstName,
    [FromForm] string lastName,
    [FromForm] string shortName,
    [FromForm] string email,
    [FromForm] string password,
    UserManager<Teacher> userManager,
    SignInManager<Teacher> signInManager) =>
{
    var newUser = new Teacher
    {
        UserName = email,
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        ShortName = shortName,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(newUser, password);

    if (result.Succeeded)
    {
        // Nach der Registrierung direkt einloggen (wie beim Login)
        await signInManager.SignInAsync(newUser, isPersistent: true);
        return Results.Redirect("/");
    }

    // Bei Fehlern zurück zur Seite mit Fehlermeldung
    var errorQuery = string.Join(",", result.Errors.Select(e => e.Description));
    return Results.Redirect($"/newuser?error={Uri.EscapeDataString(errorQuery)}");
}).DisableAntiforgery();
// 5. MIDDLEWARE PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CourseManager.Client._Imports).Assembly);

app.MapControllers();

app.Run();