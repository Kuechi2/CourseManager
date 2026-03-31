using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientRuleOccurrenceService : IRuleOccurrenceService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientRuleOccurrenceService(HttpClient http) => _http = http;

    public async Task SaveOccurrenceAsync(RuleOccurrence occurrence)
        => await _http.PostAsJsonAsync("api/occurrences", occurrence);

    public async Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId)
        => await _http.GetFromJsonAsync<List<RuleOccurrence>>($"api/courses/{courseId}/occurrences/today") ?? new();

    public async Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count)
        => await _http.GetFromJsonAsync<List<RuleOccurrence>>($"api/persons/{personId}/occurrences/last/{count}") ?? new();

    public async Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end)
    {
        var url = $"api/persons/{personId}/occurrences?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}";
        try
        {
            return await _http.GetFromJsonAsync<List<RuleOccurrence>>(url) ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Historie: {ex.Message}");
            return new();
        }
    }

    public async Task<List<RuleOccurrence>> GetOccurrencesByCourseAsync(Guid courseId, DateTime start, DateTime end)
    {
        var url = $"api/courses/{courseId}/occurrences/range?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}";
        try
        {
            return await _http.GetFromJsonAsync<List<RuleOccurrence>>(url) ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Kursstatistik: {ex.Message}");
            return new();
        }
    }
}