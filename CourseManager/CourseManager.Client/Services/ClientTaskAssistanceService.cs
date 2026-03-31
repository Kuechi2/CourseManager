using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientTaskAssistanceService : ITaskAssistanceService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientTaskAssistanceService(HttpClient http) => _http = http;

    public async Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment)
    {
        var response = await _http.PostAsJsonAsync("api/assignments", assignment);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<CourseAssignment>())!;

        var errorMsg = await response.Content.ReadAsStringAsync();
        throw new Exception($"CreateAssignment fehlgeschlagen: {errorMsg}");
    }

    public async Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId)
        => await _http.GetFromJsonAsync<List<CourseAssignment>>($"api/assignments/course/{courseId}") ?? new();

    public async Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId)
    {
        var response = await _http.PostAsJsonAsync("api/assignments/toggle",
            new { AssignmentId = assignmentId, ParticipantId = courseParticipantId });
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId)
    {
        var response = await _http.PostAsJsonAsync("api/assignments/markall",
            new { AssignmentId = assignmentId, CourseId = courseId });
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Fehler beim Markieren aller Teilnehmer als erledigt.");
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        var response = await _http.DeleteAsync($"api/assignments/{assignmentId}");
        response.EnsureSuccessStatusCode();
    }
}