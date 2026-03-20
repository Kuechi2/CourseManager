using System.ComponentModel.DataAnnotations;

namespace CourseManager.Data;

public class School
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; }
    public string Email { get; set; } = "";
    public string AccessCode { get; set; } = ""; // Dein "Schul-Passwort"
    public double GlobalRuleAverage { get; set; } = 0; // Neuer Wert für den Durchschnitt aller Regeln in der Schule
}