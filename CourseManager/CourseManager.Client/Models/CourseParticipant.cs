using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CourseManager.Data
{
    public class CourseParticipant
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CourseId { get; set; }
        [JsonIgnore]
        public Course? Course { get; set; }

        public Guid PersonId { get; set; }       // FK-Eigenschaft
        public Person Person { get; set; } = null!; // Navigations-Property
        // DEIN AUSGLEICH: Hier landen später die Koordinaten für den Sitzplan
        public int PosX { get; set; }
        public int PosY { get; set; }
    }
    public class SeatLayout
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public string Name { get; set; } = "Standard";
        public bool IsActive { get; set; }

        // Die physischen Plätze in diesem Layout
        public List<CanvasSeatData> Seats { get; set; } = new();//Hier entstehen die Plätze, die wir im Canvas zeichnen
    }

    public class CanvasSeatData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SeatLayoutId { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }

        // NULL = Leerer Tisch / Platzhalter
        public Guid? CourseParticipantId { get; set; }
        [JsonIgnore]
        public CourseParticipant? Participant { get; set; }
    }
}