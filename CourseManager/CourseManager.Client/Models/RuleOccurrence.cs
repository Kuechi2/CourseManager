public class RuleOccurrence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    // Wer?
    public Guid PersonId { get; set; }

    // Was? (Wir speichern den Text fest ein, falls die Regel später gelöscht wird)
    public string RuleName { get; set; } = string.Empty;
    public int Points { get; set; }
    public double BiasPoints { get; set; } = 0;

    // Verknüpfung für Statistiken
    public Guid? CourseId { get; set; }
}