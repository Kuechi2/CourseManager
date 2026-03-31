using CourseManager.Data;
using System.Net.Http.Json;

namespace CourseManager.Data;

public class ClientRuleSetService : IRuleSetService
{
    public event Action? OnChanged;
    private readonly HttpClient _http;
    public ClientRuleSetService(HttpClient http) => _http = http;

    public async Task<List<RuleSet>> GetRuleSetsAsync()
        => await _http.GetFromJsonAsync<List<RuleSet>>("api/RuleSets") ?? new();

    public async Task SaveRuleSetAsync(RuleSet ruleSet)
        => await _http.PostAsJsonAsync("api/RuleSets", ruleSet);

    public async Task<RuleSet?> GetRuleSetByIdAsync(Guid id)
        => await _http.GetFromJsonAsync<RuleSet>($"api/RuleSets/{id}");
}