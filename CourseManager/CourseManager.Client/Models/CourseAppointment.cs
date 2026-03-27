using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CourseManager.Data
{
    public class CourseAppointment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CourseId { get; set; }

        [JsonIgnore]
        public Course? Course { get; set; }

        public DayOfWeek Day { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}