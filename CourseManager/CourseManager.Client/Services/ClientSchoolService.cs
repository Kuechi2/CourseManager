using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientSchoolService : ISchoolService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientSchoolService(HttpClient http) => _http = http;

    public async Task<List<School>> GetAllSchoolsGlobalAsync()
        => await _http.GetFromJsonAsync<List<School>>("api/schools/allschools") ?? new();

    public async Task<bool> IsSchoolNameTakenAsync(string name)
    {
        var schools = await GetAllSchoolsGlobalAsync();
        return schools.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<School> CreateSchoolAsync(Guid teacherId, string name, string address)
    {
        var payload = new { TeacherId = teacherId, Name = name, Address = address };
        var response = await _http.PostAsJsonAsync("api/schools/create", payload);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<School>())!;

        var errorMsg = await response.Content.ReadAsStringAsync();
        throw new Exception($"Der Server meldet: {errorMsg}");
    }

    public async Task<School> GetSchoolWithIdAsync(Guid schoolId)
    {
        var result = await _http.GetFromJsonAsync<School>($"api/schools/schools/{schoolId}");
        return result ?? throw new Exception($"Schule mit Id {schoolId} nicht gefunden!");
    }
}