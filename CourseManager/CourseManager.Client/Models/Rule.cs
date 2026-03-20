using System.ComponentModel.DataAnnotations;

public enum RuleType { Standard, Partner, Timer }

public class Rule
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid RuleSetId { get; set; }
    // Platzhalter-Idee: "Der Schüler {name} hat sich toll gemeldet."
    public string ExplanationString { get; set; } = "";
    private int _points;
    public int Points
    {
        get => _points;
        set => _points = Math.Clamp(value, -10, 10);
    }
    public RuleType Type { get; set; } = RuleType.Standard;
    // Für Timer-Regeln: Dauer in Minuten
    public int? DefaultDurationMinutes { get; set; }
}