using CourseManager.Data;
using System.Net.Http.Json;

public class ClientAccountService : IAccountService
{
    private readonly HttpClient _http;

    public ClientAccountService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> RegisterTeacherAsync(TeacherDto dto, string password)
    {
        // Wir schicken ein anonymes Objekt mit DTO und Passwort zum Server
        var response = await _http.PostAsJsonAsync("api/account/register", new { Teacher = dto, Password = password });
        return response.IsSuccessStatusCode;
    }

    // Die anderen Methoden kannst du erstmal mit NotImplemented lassen
    public Task<bool> ChangePasswordAsync(string oldPassword, string newPassword) => throw new NotImplementedException();
    public Task<bool> DeleteTeacherAsync(Guid id) => throw new NotImplementedException();
    public Task<List<TeacherDto>> GetAllTeachersAsync() => throw new NotImplementedException();
    public Task<TeacherDto?> GetCurrentTeacherProfileAsync() => throw new NotImplementedException();
}