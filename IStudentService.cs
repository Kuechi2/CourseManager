using CourseManager.Data;

public interface IStudentService
{
    public event Action? OnChanged;
    Task<List<Course>> GetCoursesAsync();
    Task<Course?> GetCourseWithParticipantsAsync(Guid id);
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task SaveCourseAsync(Course course);
    Task<List<Course>> GetCoursesWithParticipantsAsync();
    Task<bool> DeleteCourseAsync(Guid id);
    Task<List<Person>> GetStudentsAsync();
    Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId);
    Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count);
    Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end);
    Task<List<RuleOccurrence>> GetOccurrencesByCourseAsync(Guid courseId, DateTime start, DateTime end);
    Task<List<TeacherDto>> GetTeachersAsync();
    Task SaveStudentAsync(Person student);
    Task DeleteStudentAsync(Guid Id);
    Task AddTeacher(TeacherDto teacherDto);
    Task<List<RuleSet>> GetRuleSetsAsync();
    Task SaveRuleSetAsync(RuleSet ruleSet);
    Task<RuleSet?> GetRuleSetByIdAsync(Guid id);
    Task SaveOccurrenceAsync(RuleOccurrence occurrence);
    Task<List<School>> GetAllSchoolsGlobalAsync();
    Task<bool> IsSchoolNameTakenAsync(string name);
    Task<School> CreateSchoolAsync(Guid TeacherId, string name, string address);
    Task<School> GetSchoolWithIdAsync(Guid SchoolId);
    Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment);
    Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId);
    Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId);
    Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId);
    Task DeleteAssignmentAsync(Guid assignmentId);
}