using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

public interface IWorkItemRepository : IBaseRepository<WorkItem>
{
    Task<IEnumerable<WorkItem>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<WorkItem>> GetByAssigneeIdAsync(string assigneeId);
    Task<IEnumerable<WorkItem>> GetByStatusAsync(WorkItemStatus status);
    Task<IEnumerable<WorkItem>> GetByPriorityAsync(WorkItemPriority priority);
    Task<IEnumerable<WorkItem>> GetOverdueWorkItemsAsync();
    Task<IEnumerable<WorkItem>> GetWorkItemsDueSoonAsync(int days = 3);
    Task<IEnumerable<WorkItem>> GetByCreatedByIdAsync(string createdById);
    Task<IEnumerable<WorkItem>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<WorkItem>> GetByTagsAsync(List<string> tags);
    Task<IEnumerable<WorkItem>> SearchWorkItemsAsync(string searchTerm);
    Task<long> GetWorkItemCountByProjectAsync(string projectId);
    Task<long> GetWorkItemCountByStatusAsync(WorkItemStatus status);
    Task<long> GetOverdueWorkItemCountAsync();
    Task<Dictionary<WorkItemStatus, long>> GetWorkItemCountsByStatusAsync();
    Task<Dictionary<WorkItemPriority, long>> GetWorkItemCountsByPriorityAsync();
}
