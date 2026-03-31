using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class RuleSetDataService : BaseDataService, IRuleSetService
{
    public RuleSetDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<List<RuleSet>> GetRuleSetsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleSets.Include(s => s.Rules).ToListAsync();
    }

    public async Task<RuleSet?> GetRuleSetByIdAsync(Guid id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.RuleSets.Include(s => s.Rules).FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task SaveRuleSetAsync(RuleSet incomingSet)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbSet = await context.RuleSets.Include(s => s.Rules)
            .FirstOrDefaultAsync(s => s.Id == incomingSet.Id);

        if (dbSet == null)
        {
            incomingSet.SchoolId = context.GetSchoolId();
            context.RuleSets.Add(incomingSet);
        }
        else
        {
            context.Entry(dbSet).CurrentValues.SetValues(incomingSet);
            foreach (var dbRule in dbSet.Rules.ToList())
                if (!incomingSet.Rules.Any(r => r.Id == dbRule.Id))
                    context.Rules.Remove(dbRule);

            foreach (var incomingRule in incomingSet.Rules)
            {
                var dbRule = dbSet.Rules.FirstOrDefault(r => r.Id == incomingRule.Id);
                if (dbRule == null)
                {
                    incomingRule.RuleSetId = dbSet.Id;
                    context.Entry(incomingRule).State = EntityState.Added;
                    dbSet.Rules.Add(incomingRule);
                }
                else
                {
                    context.Entry(dbRule).CurrentValues.SetValues(incomingRule);
                }
            }
        }

        try
        {
            await context.SaveChangesAsync();
            NotifyChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}