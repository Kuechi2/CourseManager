public class RuleOccurrence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Wer?
    public Guid PersonId { get; set; }

    // Was? (Anzeigetext – mit Schülername, für Chronik/History)
    public string RuleName { get; set; } = string.Empty;

    // Welche Regel? (Regelname ohne Schülerbezug – für Statistiken)
    public string RuleTitle { get; set; } = string.Empty;

    public int Points { get; set; }
    public double BiasPoints { get; set; } = 0;

    // Verknüpfung für Statistiken
    public Guid? CourseId { get; set; }
}