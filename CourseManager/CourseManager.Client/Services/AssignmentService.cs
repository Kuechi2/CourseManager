using CourseManager.Data;

public class AssignmentService
{
    private readonly ITaskAssistanceService _service;
    public Guid CourseId { get; private set; }
    public List<CourseAssignment> Assignments { get; private set; } = new();
    private CourseAssignment? _selectedAssignment;
    public CourseAssignment? SelectedAssignment
    {
        get => _selectedAssignment;
        set
        {
            _selectedAssignment = value;
            OnChange?.Invoke(); // Das ist das Signal an die UI!
        }
    }
    public event Action? OnChange;

    public AssignmentService(ITaskAssistanceService service) => _service = service; //Injection!

    public bool IsCompleted(Guid? participantId) =>
        SelectedAssignment?.StatusEntries.Any(s => s.CourseParticipantId == participantId && s.IsCompleted) ?? false;

    public IEnumerable<CourseAssignment> GetOpenAssignments(Guid? participantId) =>
        Assignments.Where(a => !a.StatusEntries.Any(s => s.CourseParticipantId == participantId && s.IsCompleted));

    public async Task Refresh(Guid? courseId = null)
    {
        if (courseId.HasValue)
        {
            CourseId = courseId.Value;
        }
        if (CourseId == Guid.Empty) return;
        Assignments = await _service.GetAssignmentsForCourseAsync(CourseId);
        OnChange?.Invoke();
    }
    public async Task ToggleStatus(Guid? participantId)
    {
        if (SelectedAssignment == null || participantId == null) return;
        await _service.ToggleAssignmentStatusAsync(SelectedAssignment.Id, participantId.Value);
        await Refresh();
        SelectedAssignment = Assignments.FirstOrDefault(a => a.Id == SelectedAssignment.Id);
    }
    public async Task CreateAssignment(string title, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title) || CourseId == Guid.Empty) return;

        var newAss = new CourseAssignment
        {
            Title = title,
            CourseId = this.CourseId, // Wir nutzen die ID aus dem Cache!
            DueDate = dueDate ?? DateTime.Now.AddDays(7)
        };

        await _service.CreateAssignmentAsync(newAss);
        await Refresh();
        SelectedAssignment = Assignments.OrderByDescending(a => a.Id).FirstOrDefault();
    }
    public async Task DeleteAssignment(Guid assignmentId)
    {
        // 1. In DB löschen
        await _service.DeleteAssignmentAsync(assignmentId);

        // 2. Falls die gelöschte Aufgabe gerade ausgewählt war, Selektion aufheben
        if (SelectedAssignment?.Id == assignmentId)
        {
            _selectedAssignment = null;
        }

        // 3. Liste aktualisieren und UI benachrichtigen
        await Refresh();
    }
    public void SelectAssignment(Guid? id)
    {
        SelectedAssignment = Assignments.FirstOrDefault(a => a.Id == id);
    }
}