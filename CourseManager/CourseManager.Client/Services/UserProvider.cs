using CourseManager.Data;
using System.Net.Http.Json;

public class ClientUserProvider(HttpClient http) : IUserProvider
{
    public async Task<Guid> GetCurrentUserIdAsync()
        => await http.GetFromJsonAsync<Guid>("api/user/id");

    public async Task<TeacherDto> GetCurrentTeacherAsync()
        => await http.GetFromJsonAsync<TeacherDto>("api/user/teacher");

    public async Task<Guid> GetCurrentSchoolIdAsync()
        => await http.GetFromJsonAsync<Guid>("api/user/schoolid");
}