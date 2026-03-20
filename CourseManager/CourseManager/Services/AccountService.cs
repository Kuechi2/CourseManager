// Im Server-Projekt
using CourseManager.Data;
using Microsoft.AspNetCore.Identity;

public class TeacherService : IAccountService
{
    private readonly UserManager<Teacher> _userManager;

    public TeacherService(UserManager<Teacher> userManager)
    {
        _userManager = userManager;
    }

    public Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteTeacherAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<TeacherDto>> GetAllTeachersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TeacherDto?> GetCurrentTeacherProfileAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RegisterTeacherAsync(TeacherDto dto, string password)
    {
        var newTeacher = new Teacher
        {
            UserName = dto.Email, // Wichtig: Login-Name
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ShortName = dto.ShortName ?? "Kürzel",
            EmailConfirmed = true // Damit er sich sofort einloggen kann
        };

        // Hier passiert die Magie: Validierung, Hashing, Speichern
        var result = await _userManager.CreateAsync(newTeacher, password);

        return result.Succeeded;
    }
}