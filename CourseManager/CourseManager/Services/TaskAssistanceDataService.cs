using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data;

public class TaskAssistanceDataService : BaseDataService, ITaskAssistanceService
{
    public TaskAssistanceDataService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        : base(dbFactory, httpContextAccessor) { }

    public async Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment)
    {
        using var context = _dbFactory.CreateDbContext();
        assignment.SchoolId = context.GetSchoolId();
        if (assignment.CreatedAt == default) assignment.CreatedAt = DateTime.Now;
        context.CourseAssignments.Add(assignment);
        await context.SaveChangesAsync();
        return assignment;
    }

    public async Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.CourseAssignments
            .Where(a => a.CourseId == courseId)
            .Include(a => a.StatusEntries)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId)
    {
        using var context = _dbFactory.CreateDbContext();
        var status = await context.StudentAssignmentStatuses
            .FirstOrDefaultAsync(s => s.CourseAssignmentId == assignmentId
                                   && s.CourseParticipantId == courseParticipantId);
        if (status == null)
        {
            context.StudentAssignmentStatuses.Add(new StudentAssignmentStatus
            {
                CourseAssignmentId = assignmentId,
                CourseParticipantId = courseParticipantId,
                IsCompleted = true, CompletedAt = DateTime.Now
            });
        }
        else
        {
            status.IsCompleted = !status.IsCompleted;
            status.CompletedAt = status.IsCompleted ? DateTime.Now : null;
        }
        await context.SaveChangesAsync();
    }

    public async Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId)
    {
        using var context = _dbFactory.CreateDbContext();
        var participants = await context.CourseParticipants
            .Where(p => p.CourseId == courseId).ToListAsync();

        foreach (var p in participants)
        {
            var status = await context.StudentAssignmentStatuses
                .FirstOrDefaultAsync(s => s.CourseAssignmentId == assignmentId
                                       && s.CourseParticipantId == p.Id);
            if (status == null)
                context.StudentAssignmentStatuses.Add(new StudentAssignmentStatus
                {
                    CourseAssignmentId = assignmentId, CourseParticipantId = p.Id,
                    IsCompleted = true, CompletedAt = DateTime.Now
                });
            else
            {
                status.IsCompleted = true;
                status.CompletedAt = DateTime.Now;
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        using var context = _dbFactory.CreateDbContext();
        var assignment = await context.CourseAssignments
            .Include(a => a.StatusEntries)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment == null) return;
        if (assignment.StatusEntries?.Any() == true)
            context.StudentAssignmentStatuses.RemoveRange(assignment.StatusEntries);
        context.CourseAssignments.Remove(assignment);
        await context.SaveChangesAsync();
    }
}