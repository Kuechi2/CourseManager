using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientTeacherService : ITeacherService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientTeacherService(HttpClient http) => _http = http;

    public async Task<List<TeacherDto>> GetTeachersAsync()
        => await _http.GetFromJsonAsync<List<TeacherDto>>("api/teachers") ?? new();

    public async Task AddTeacher(TeacherDto teacherDto)
        => await _http.PostAsJsonAsync("api/teachers", teacherDto);
}