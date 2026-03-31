using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientStudentService : IStudentService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientStudentService(HttpClient http) => _http = http;

    public async Task<List<Person>> GetStudentsAsync()
        => await _http.GetFromJsonAsync<List<Person>>("api/students") ?? new();

    public async Task SaveStudentAsync(Person student)
    {
        Console.WriteLine("Speichere Schüler: " + student.FirstName + " " + student.LastName);
        var response = await _http.PostAsJsonAsync("api/students", student);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception(errorContent);
        }
    }

    public async Task DeleteStudentAsync(Guid id)
    {
        await _http.DeleteAsync($"api/students/{id}");
    }
}