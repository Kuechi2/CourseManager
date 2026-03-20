using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // Dieser Namespace ist wichtig!
namespace CourseManager.Data;

public class Teacher: IdentityUser<Guid>
{
    [Key]
    // public Guid Id { get; set; } wird jetzt von Identity bereitgestellt!!!
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string ShortName { get; set; } = ""; // z.B. "KÜCHOLORE"
    public Guid? ActiveSchoolId { get; set; }

    // Navigation Property: Ein Lehrer hat viele Kurse
    public List<Course> Courses { get; set; } = new();
    // Hier fügen wir schonmal die Schul-Verknüpfung ein
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    public bool IsAdmin { get; set; } = false; // Standardmäßig kein Admin
    public bool IsApproved { get; set; } = false; // Standardmäßig kein Approve
    public double PointsBias { get; set; } = 0;

    public TeacherDto ToDto()
    {
        return new TeacherDto
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName,
            ShortName = this.ShortName,
            Email = this.Email,
            PointsBias = this.PointsBias,
            IsAdmin = this.IsAdmin,
            IsApproved = this.IsApproved,
            ActiveSchoolId = this.ActiveSchoolId
        };
    }
}
/*
 * Data Transfer Object (DTO) für Teacher
 */
public class TeacherDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? ShortName { get; set; }
    public string? Email { get; set; }
    public double PointsBias { get; set; }
    public bool IsAdmin { get; set; } 
    public bool IsApproved { get; set; } 
    public Guid? ActiveSchoolId { get; set; }

}