using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface IWorkItemLogRepository : IBaseRepository<WorkItemLog>
{
    Task<IEnumerable<WorkItemLog>> GetByWorkItemIdAsync(string workItemId);
    Task<IEnumerable<WorkItemLog>> GetByUserIdAsync(string userId);
    Task<IEnumerable<WorkItemLog>> GetByActionAsync(WorkItemLogAction action);
    Task<IEnumerable<WorkItemLog>> GetRecentLogsAsync(int count = 50);
    Task<IEnumerable<WorkItemLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
