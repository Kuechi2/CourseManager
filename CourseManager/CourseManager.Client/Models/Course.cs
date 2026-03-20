using System.ComponentModel.DataAnnotations;

namespace CourseManager.Data
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty; 
        public List<CourseParticipant> Enrollments { get; set; } = new();
        public List<SeatLayout> SeatLayouts { get; set; } = new();
        public Teacher? Teacher { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? RuleSetId { get; set; }
        public RuleSet? RuleSet { get; set; }
        public string? RoomNumber { get; set; }
        public DayOfWeek? Day { get; set; }
        public TimeOnly? StartTime { get; set; }
        public Guid SchoolId { get; set; }

        public TimeOnly? EndTime { get; set; }
        public Course() { }

        public Course(string title)
        {
            Title = title;
        }

        public override string ToString()
        {
            return $"{Title} (Participants: {Enrollments?.Count ?? 0})";
        }

        public void AddParticipant(Person person)
        {
            if (!Enrollments.Any(e => e.PersonId == person.Id))
            {
                var enrollment = new CourseParticipant
                {
                    Id = Guid.NewGuid(),
                    CourseId = this.Id,
                    PersonId = person.Id,
                    Person = person,
                    Course = this
                    // PosX/Y SIND JETZT WEG!
                };

                Enrollments.Add(enrollment);
            }
        }
    }
}