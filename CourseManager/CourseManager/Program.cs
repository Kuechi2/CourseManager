using CourseManager.Components;
using CourseManager.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Data Source=geometry.db";

builder.Services.AddDbContextFactory<AppDataContext>(
    options => options.UseSqlite(connectionString), ServiceLifetime.Scoped);
builder.Services.AddDbContext<AppDataContext>(
    options => options.UseSqlite(connectionString));

builder.Services.AddIdentity<Teacher, IdentityRole<Guid>>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDataContext>()
.AddDefaultTokenProviders();

// Services – je Interface eine Klasse
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<IStudentService,        StudentDataService>();
builder.Services.AddScoped<ICourseService,          CourseDataService>();
builder.Services.AddScoped<IRuleSetService,         RuleSetDataService>();
builder.Services.AddScoped<IRuleOccurrenceService,  RuleOccurrenceDataService>();
builder.Services.AddScoped<ITeacherService,         TeacherDataService>();
builder.Services.AddScoped<ISchoolService,          SchoolDataService>();
builder.Services.AddScoped<ITaskAssistanceService,  TaskAssistanceDataService>();

builder.Services.AddScoped<IAccountService, TeacherService>();
builder.Services.AddScoped<IUserProvider, UserProvider>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.MapPost("/auth/login", async (
    [FromForm] string email,
    [FromForm] string password,
    SignInManager<Teacher> signInManager) =>
{
    var result = await signInManager.PasswordSignInAsync(email, password, true, false);
    return result.Succeeded ? Results.Redirect("/") : Results.Redirect("/login?error=true");
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (SignInManager<Teacher> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).DisableAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDataContext>>();
    await DbInitializer.SeedData(factory);
}

// ?? Middleware-Pipeline ???????????????????????????????????????????????????????
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CourseManager.Client._Imports).Assembly);

app.Run();