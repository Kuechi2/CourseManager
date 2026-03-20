using CourseManager.Data;

public interface IUserProvider
{
    Task<Guid> GetCurrentUserIdAsync();
    Task<TeacherDto> GetCurrentTeacherAsync();
    Task<Guid> GetCurrentSchoolIdAsync();
}