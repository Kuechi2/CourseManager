using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientCourseService : ICourseService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientCourseService(HttpClient http) => _http = http;

    public async Task<List<Course>> GetCoursesAsync()
        => await _http.GetFromJsonAsync<List<Course>>("api/courses") ?? new();

    public async Task<List<Course>> GetCoursesWithParticipantsAsync()
        => await _http.GetFromJsonAsync<List<Course>>("api/courses") ?? new();

    public async Task<Course?> GetCourseWithParticipantsAsync(Guid id)
        => await _http.GetFromJsonAsync<Course>($"api/courses/{id}");

    public async Task<Course?> GetCourseByIdAsync(Guid id)
        => await _http.GetFromJsonAsync<Course>($"api/Courses/{id}");

    public async Task SaveCourseAsync(Course course)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/courses", course);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server Fehler: {response.StatusCode} - {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client-seitiger Fehler beim Senden: {ex.Message}");
        }
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/courses/{id}");
        return response.IsSuccessStatusCode;
    }
}