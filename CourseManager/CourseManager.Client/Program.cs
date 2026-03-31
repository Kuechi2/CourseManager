using CourseManager.Client.Models;
using CourseManager.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IStudentService,       ClientStudentService>();
builder.Services.AddScoped<ICourseService,         ClientCourseService>();
builder.Services.AddScoped<IRuleSetService,        ClientRuleSetService>();
builder.Services.AddScoped<IRuleOccurrenceService, ClientRuleOccurrenceService>();
builder.Services.AddScoped<ITeacherService,        ClientTeacherService>();
builder.Services.AddScoped<ISchoolService,         ClientSchoolService>();
builder.Services.AddScoped<ITaskAssistanceService, ClientTaskAssistanceService>();

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

var host = builder.Build();
await host.RunAsync();