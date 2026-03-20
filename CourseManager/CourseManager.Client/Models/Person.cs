using System.ComponentModel.DataAnnotations;
namespace CourseManager.Data
{
    public class Person
    {
        [Key] // Sagt der Datenbank: Das hier ist der Chef-Schlüssel
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Der Name ist Pflicht.")]
        [StringLength(30, ErrorMessage = "Name zu lang.")]
        public string FirstName { get; set; } = string.Empty;
        public enum EGender { Divers, Mädchen, Junge }

        // In der Person.cs
        public EGender Gender { get; set; } = EGender.Divers;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } = DateTime.Today;
        public Guid SchoolId { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}