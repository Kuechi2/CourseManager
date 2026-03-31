using CourseManager.Data;

public interface IStudentService
{
    public event Action? OnChanged;
    Task<List<Person>> GetStudentsAsync();
    Task SaveStudentAsync(Person student);
    Task DeleteStudentAsync(Guid Id);  
}
public interface IRuleSetService
{
    public event Action? OnChanged;
    Task<List<RuleSet>> GetRuleSetsAsync();
    Task SaveRuleSetAsync(RuleSet ruleSet);
    Task<RuleSet?> GetRuleSetByIdAsync(Guid id);
}
public interface ICourseService
{
    public event Action? OnChanged;
    Task<List<Course>> GetCoursesAsync();
    Task<Course?> GetCourseWithParticipantsAsync(Guid id);
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task SaveCourseAsync(Course course);
    Task<List<Course>> GetCoursesWithParticipantsAsync();
    Task<bool> DeleteCourseAsync(Guid id);

}
public interface IRuleOccurrenceService

{
    public event Action? OnChanged;
    Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId);
    Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count);
    Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end);
    Task<List<RuleOccurrence>> GetOccurrencesByCourseAsync(Guid courseId, DateTime start, DateTime end);
    Task SaveOccurrenceAsync(RuleOccurrence occurrence);
}
public interface ITeacherService
{
    public event Action? OnChanged;
    Task<List<TeacherDto>> GetTeachersAsync();
    Task AddTeacher(TeacherDto teacherDto);
}
public interface ISchoolService
{
    public event Action? OnChanged;
    Task<List<School>> GetAllSchoolsGlobalAsync();
    Task<bool> IsSchoolNameTakenAsync(string name);
    Task<School> CreateSchoolAsync(Guid TeacherId, string name, string address);
    Task<School> GetSchoolWithIdAsync(Guid SchoolId);
}
public interface ITaskAssistanceService
{
    public event Action? OnChanged;
    Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment);
    Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId);
    Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId);
    Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId);
    Task DeleteAssignmentAsync(Guid assignmentId);
}