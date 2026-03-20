public class EvaluationService
{
    public List<StudentScore> AnalyzeSession(List<RuleOccurrence> data)
    {
        if (data == null || !data.Any()) return new();

        // 1. Gruppieren nach Schüler
        var totals = data.GroupBy(o => o.PersonId)
            .Select(g => new {
                Id = g.Key,
                Sum = (double)g.Sum(o => o.Points),
                Count = g.Count()
            }).ToList();

        double avg = totals.Average(t => t.Sum);

        // Standardabweichung berechnen für den Bias-Check (Z-Score)
        double stdDev = Math.Sqrt(totals.Average(t => Math.Pow(t.Sum - avg, 2)));

        return totals.Select(t => new StudentScore
        {
            PersonId = t.Id,
            RawPoints = (int)t.Sum,
            // Bias-Logik: Wie weit weicht er vom Klassendurchschnitt ab?
            // Wenn stdDev 0 ist, sind alle gleich -> Score 0
            BiasScore = stdDev > 0.1 ? (t.Sum - avg) / stdDev : 0
        }).ToList();
    }
}

public class StudentScore
{
    public Guid PersonId { get; set; }
    public int RawPoints { get; set; }
    public double BiasScore { get; set; } // Z-Score (meist zwischen -3 und +3)
}
public static class EvaluationLogic
{
    public static Dictionary<Guid, double> CalculateBiasScores(List<RuleOccurrence> occurrences, double average)
    {
        if (!occurrences.Any()) return new();
        var studentSums = occurrences
            .GroupBy(o => o.PersonId)
            .ToDictionary(g => g.Key, g => (double)g.Sum(o => o.Points));
        return studentSums.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value / (average!=0?average:1));
    }
}
public static class ProgressiveEvaluationLogic
{
    public static double CalculateContinuityScore(List<RuleOccurrence> occurrences)
    {
        if (occurrences == null || !occurrences.Any()) return 0;

        double weightedScore = 0;
        double totalWeight = 0;

        for (int i = 0; i < occurrences.Count; i++)
        {
            // Gewicht berechnen: Das neueste (i=0) bekommt 10, das älteste (i=9) bekommt 1.
            // Formel: (Maximale Anzahl - aktueller Index)
            double weight = (occurrences.Count - i);

            weightedScore += occurrences[i].Points * weight;
            totalWeight += weight;
        }

        // Optional: Den Score wieder normalisieren oder als "Bias" belassen
        // Wenn du es auf die ursprüngliche Größenordnung bringen willst:
        // return weightedScore / totalWeight * occurrences.Count; 

        return weightedScore;
    }
}