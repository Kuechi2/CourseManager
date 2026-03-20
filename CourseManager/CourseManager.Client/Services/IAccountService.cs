using CourseManager.Data;

public interface IAccountService
{
    // Für den Admin (Lord K.)
    Task<List<TeacherDto>> GetAllTeachersAsync();
    Task<bool> RegisterTeacherAsync(TeacherDto teacher, string initialPassword);
    Task<bool> DeleteTeacherAsync(Guid id);

    // Für den Lehrer selbst
    Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
    Task<TeacherDto?> GetCurrentTeacherProfileAsync();
}