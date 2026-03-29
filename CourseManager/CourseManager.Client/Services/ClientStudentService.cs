using CourseManager.Data; // Hier liegen deine Models Person & Course
using System.Net.Http;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientStudentService : IStudentService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientStudentService(HttpClient http) => _http = http;

    public async Task<Course?> GetCourseWithParticipantsAsync(Guid id)
        => await _http.GetFromJsonAsync<Course>($"api/courses/{id}");

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
            else
            {
                Console.WriteLine("Senden erfolgreich!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client-seitiger Fehler beim Senden: {ex.Message}");
        }
    }

    public async Task<List<Person>> GetStudentsAsync()
        => await _http.GetFromJsonAsync<List<Person>>("api/students") ?? new();
    public async Task<List<Course>> GetCoursesWithParticipantsAsync()
    {
        var courses = await _http.GetFromJsonAsync<List<Course>>("api/courses");
        return courses ?? new List<Course>();
    }

    public async Task<List<TeacherDto>> GetTeachersAsync()
    {
        // Hier jetzt TeacherDto nutzen!
        return await _http.GetFromJsonAsync<List<TeacherDto>>("api/teachers") ?? new();
    }

    public async Task AddTeacher(TeacherDto teacherDto)
    {
        await _http.PostAsJsonAsync("api/teachers", teacherDto);
    }
    public async Task<List<RuleSet>> GetRuleSetsAsync()
    {
        return await _http.GetFromJsonAsync<List<RuleSet>>("api/RuleSets") ?? new();
    }

    public async Task SaveRuleSetAsync(RuleSet ruleSet)
    {
        await _http.PostAsJsonAsync("api/RuleSets", ruleSet);
    }
    public async Task<Course?> GetCourseByIdAsync(Guid id) =>
    await _http.GetFromJsonAsync<Course>($"api/Courses/{id}");

    public async Task<RuleSet?> GetRuleSetByIdAsync(Guid id) =>
        await _http.GetFromJsonAsync<RuleSet>($"api/RuleSets/{id}");

    public async Task SaveOccurrenceAsync(RuleOccurrence occurrence)
    {
        await _http.PostAsJsonAsync("api/occurrences", occurrence);
    }
    public async Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId)
    {
        return await _http.GetFromJsonAsync<List<RuleOccurrence>>($"api/courses/{courseId}/occurrences/today")
               ?? new List<RuleOccurrence>();
    }
    public async Task SaveStudentAsync(Person student)
    {
        Console.WriteLine("Speichere Schüler: " + student.FirstName + " " + student.LastName);
        var response = await _http.PostAsJsonAsync($"api/students", student);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception(errorContent);
        }
    }

    public async Task DeleteStudentAsync(Guid Id)
    {
        await _http.DeleteAsync($"api/students/{Id}");
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        var courses = await _http.GetFromJsonAsync<List<Course>>("api/courses");
        return courses ?? new List<Course>();
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/courses/{id}");
        return response.IsSuccessStatusCode;
    }
    public async Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end)
    {
        // Wir formatieren das Datum zu yyyy-MM-dd, damit die API es sauber parsen kann
        var startStr = start.ToString("yyyy-MM-dd");
        var endStr = end.ToString("yyyy-MM-dd");

        var url = $"api/persons/{personId}/occurrences?start={startStr}&end={endStr}";

        try
        {
            var result = await _http.GetFromJsonAsync<List<RuleOccurrence>>(url);
            return result ?? new List<RuleOccurrence>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Historie: {ex.Message}");
            return new List<RuleOccurrence>();
        }
    }
    public async Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count)
    {
        // Wir rufen den Pfad auf, den wir im Controller definiert haben
        return await _http.GetFromJsonAsync<List<RuleOccurrence>>($"api/persons/{personId}/occurrences/last/{count}")
               ?? new List<RuleOccurrence>();
    }

    public async Task<List<School>> GetAllSchoolsGlobalAsync()
    {
        return await _http.GetFromJsonAsync<List<School>>("api/schools/allschools")
           ?? new List<School>(); // Fallback, falls null kommt
    }
    public async Task<bool> IsSchoolNameTakenAsync(string name)
    {
        // Wir fragen alle Schulen ab und schauen, ob der Name schon existiert
        var schools = await GetAllSchoolsGlobalAsync();
        return schools.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<School> CreateSchoolAsync(Guid teacherId, string name, string address)
    {
        // Das Paket für den Server schnüren
        var payload = new { TeacherId = teacherId, Name = name, Address = address };

        // Ab die Post zum Controller
        var response = await _http.PostAsJsonAsync("api/schools/create", payload);

        if (response.IsSuccessStatusCode)
        {
            var createdSchool = await response.Content.ReadFromJsonAsync<School>();
            return createdSchool!;
        }
        else
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new Exception($"Der Server meldet: {errorMsg}");
        }
    }

    public Task<School> GetSchoolWithIdAsync(Guid SchoolId)
    {
        return _http.GetFromJsonAsync<School>($"api/schools/schools/{SchoolId}")
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.Result == null)
                {
                    throw new Exception($"Schule mit Id {SchoolId} nicht gefunden!");
                }
                return task.Result;
            });
    }
    public async Task ToggleAssignment(Guid assignmentId, Guid participantId)
    {
        var response = await _http.PostAsJsonAsync("api/assignments/toggle",
            new { AssignmentId = assignmentId, ParticipantId = participantId });
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId)
    {
        var AssignemntList = await _http.GetFromJsonAsync<List<CourseAssignment>>($"api/assignments/course/{courseId}");
        if(AssignemntList == null)
        {
            return new List<CourseAssignment>();
        }
        else
        {
            return AssignemntList;
        }
    }

    public async Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment)
    {
        var response=  await _http.PostAsJsonAsync("api/assignments", assignment);
        if (response.IsSuccessStatusCode)
        {
            var createdCourseAssignment = await response.Content.ReadFromJsonAsync<CourseAssignment>();
            return createdCourseAssignment!;
        }
        else
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            throw new Exception($"CreateAssignment fehlgeschlagen: {errorMsg}");
        }
    }

    public Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId)
    {
        return ToggleAssignment(assignmentId, courseParticipantId);
    }

    public Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId)
    {
        return _http.PostAsJsonAsync("api/assignments/markall", new { AssignmentId = assignmentId, CourseId = courseId })
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    throw new Exception($"Fehler beim Markieren aller Teilnehmer als erledigt: {task.Exception?.Message}");
                }
            });
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/assignments/{assignmentId}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception($"Fehler beim Löschen der Aufgabe ({assignmentId}): {ex.Message}");
        }
    }

    public async Task<List<RuleOccurrence>> GetOccurrencesByCourseAsync(Guid courseId, DateTime start, DateTime end)
    {
        var startStr = start.ToString("yyyy-MM-dd");
        var endStr = end.ToString("yyyy-MM-dd");
        try
        {
            return await _http.GetFromJsonAsync<List<RuleOccurrence>>(
                $"api/courses/{courseId}/occurrences/range?start={startStr}&end={endStr}") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Kursstatistik: {ex.Message}");
            return new();
        }
    }

}