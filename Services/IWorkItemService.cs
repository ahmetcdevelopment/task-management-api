using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public interface IWorkItemService
{
    Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync();
    Task<WorkItem?> GetWorkItemByIdAsync(string id);
    Task<IEnumerable<WorkItem>> GetWorkItemsByProjectIdAsync(string projectId);
    Task<IEnumerable<WorkItem>> GetWorkItemsByAssigneeIdAsync(string assigneeId);
    Task<IEnumerable<WorkItem>> GetWorkItemsByStatusAsync(WorkItemStatus status);
    Task<IEnumerable<WorkItem>> GetOverdueWorkItemsAsync();
    Task<IEnumerable<WorkItem>> GetWorkItemsDueSoonAsync(int days = 3);
    Task<WorkItem> CreateWorkItemAsync(CreateWorkItemDto createWorkItemDto, string createdById);
    Task<WorkItem?> UpdateWorkItemAsync(string id, UpdateWorkItemDto updateWorkItemDto, string updatedById);
    Task<bool> DeleteWorkItemAsync(string id, string deletedById);
    Task<bool> AssignWorkItemAsync(string workItemId, string assigneeId, string assignedById);
    Task<bool> UpdateWorkItemStatusAsync(string workItemId, WorkItemStatus status, string updatedById);
    Task<bool> AddCommentAsync(string workItemId, string comment, string userId);
    Task<IEnumerable<WorkItemLog>> GetWorkItemLogsAsync(string workItemId);
}
