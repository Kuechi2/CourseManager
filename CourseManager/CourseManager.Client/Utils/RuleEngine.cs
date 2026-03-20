namespace CourseManager.Data // Oder dein Shared-Namespace
{
    public static class RuleEngine
    {
        public static string FormatExplanation(Rule rule, Person p1, Person? p2 = null)
        {
            if (string.IsNullOrWhiteSpace(rule.ExplanationString)) return "";

            string text = rule.ExplanationString;

            // Standard-Platzhalter
            text = text.Replace("{name}", $"{p1.FirstName} {p1.LastName}");
            text = text.Replace("{firstname}", p1.FirstName);

            // NEU: Timer-Platzhalter
            if (rule.Type == RuleType.Timer)
            {
                // Falls Minuten gesetzt sind, ersetzen, sonst "0" oder "?"
                var mins = rule.DefaultDurationMinutes?.ToString() ?? "0";
                text = text.Replace("{minutes}", mins);
                text = text.Replace("{dauer}", mins);
            }

            // Partner-Logik
            if (p2 != null)
            {
                text = text.Replace("{name2}", $"{p2.FirstName} {p2.LastName}");
                text = text.Replace("{firstname2}", p2.FirstName);
            }

            return text;
        }
    }
}