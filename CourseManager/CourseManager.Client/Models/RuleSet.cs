using System.ComponentModel.DataAnnotations;
namespace CourseManager.Data
{
    public class RuleSet
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; } = "<unbenannt>";
        public List<Rule> Rules { get; set; } = new List<Rule>();
        public Guid SchoolId { get; set; }
        public override string ToString()
        {
            return $"{Title} (ID: {Id}, Rules: {Rules.Count})";
        }
        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }
    }
}
