using CourseManager.Client.Models; // Namespace für PersistentAuthenticationStateProvider
using CourseManager.Data; // Dein Namespace zum Interface/Service
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// HIER REGISTRIEREN:
builder.Services.AddScoped<IStudentService, ClientStudentService>();
builder.Services.AddScoped<IAccountService, ClientAccountService>();
builder.Services.AddScoped<IUserProvider, ClientUserProvider>();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Services.AddScoped<IUserProvider, ClientUserProvider>();
// 2. Den Host bauen
var host = builder.Build();

// 3. App starten
await host.RunAsync();