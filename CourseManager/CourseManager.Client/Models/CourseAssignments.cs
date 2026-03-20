using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CourseManager.Data
{
    public class CourseAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid SchoolId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public List<StudentAssignmentStatus> StatusEntries { get; set; } = new();
    }

    public class StudentAssignmentStatus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseAssignmentId { get; set; }
        public Guid CourseParticipantId { get; set; } // Verweis auf den Schüler im Kurs
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}