using System.ComponentModel.DataAnnotations;
using TaskManagement.API.Models;
using TaskManagement.API.DTOs;

namespace TaskManagement.API.Helpers.Validators;

public static class WorkItemValidators
{
    public static ValidationResult? ValidateCreateWorkItemDto(CreateWorkItemDto dto, ValidationContext context)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors.Add("Title is required");

        if (dto.Title?.Length > 200)
            errors.Add("Title cannot exceed 200 characters");

        if (dto.Description?.Length > 2000)
            errors.Add("Description cannot exceed 2000 characters");

        if (string.IsNullOrWhiteSpace(dto.ProjectId))
            errors.Add("ProjectId is required");

        if (dto.DueDate.HasValue && dto.DueDate.Value <= DateTime.UtcNow)
            errors.Add("Due date must be in the future");

        //if (dto.EstimatedHours.HasValue && dto.EstimatedHours.Value <= 0)
        //    errors.Add("Estimated hours must be greater than 0");

        if (!Enum.IsDefined(typeof(WorkItemPriority), dto.Priority))
            errors.Add("Invalid priority value");

        if (errors.Any())
            return new ValidationResult(string.Join("; ", errors));

        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateUpdateWorkItemDto(UpdateWorkItemDto dto, ValidationContext context)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors.Add("Title is required");

        if (dto.Title?.Length > 200)
            errors.Add("Title cannot exceed 200 characters");

        if (dto.Description?.Length > 2000)
            errors.Add("Description cannot exceed 2000 characters");

        if (dto.DueDate.HasValue && dto.DueDate.Value <= DateTime.UtcNow)
            errors.Add("Due date must be in the future");

        if (dto.EstimatedHours.HasValue && dto.EstimatedHours.Value <= 0)
            errors.Add("Estimated hours must be greater than 0");

        if (dto.ActualHours.HasValue && dto.ActualHours.Value < 0)
            errors.Add("Actual hours cannot be negative");

        //if (!Enum.IsDefined(typeof(WorkItemStatus), dto.Status))
        //    errors.Add("Invalid status value");

        if (!Enum.IsDefined(typeof(WorkItemPriority), dto.Priority))
            errors.Add("Invalid priority value");

        if (errors.Any())
            return new ValidationResult(string.Join("; ", errors));

        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateWorkItemStatusTransition(WorkItemStatus currentStatus, WorkItemStatus newStatus, ValidationContext context)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<WorkItemStatus, WorkItemStatus[]>
        {
            { WorkItemStatus.ToDo, new[] { WorkItemStatus.InProgress, WorkItemStatus.Cancelled } },
            { WorkItemStatus.InProgress, new[] { WorkItemStatus.Done, WorkItemStatus.ToDo, WorkItemStatus.Cancelled } },
            { WorkItemStatus.Done, new[] { WorkItemStatus.InProgress } }, // Allow reopening
            { WorkItemStatus.Cancelled, new[] { WorkItemStatus.ToDo } } // Allow reactivation
        };

        if (currentStatus == newStatus)
            return ValidationResult.Success;

        if (validTransitions.ContainsKey(currentStatus) && 
            validTransitions[currentStatus].Contains(newStatus))
            return ValidationResult.Success;

        return new ValidationResult($"Invalid status transition from {currentStatus} to {newStatus}");
    }

    public static bool IsValidWorkItemStatus(WorkItemStatus status)
    {
        return Enum.IsDefined(typeof(WorkItemStatus), status);
    }

    public static bool IsValidWorkItemPriority(WorkItemPriority priority)
    {
        return Enum.IsDefined(typeof(WorkItemPriority), priority);
    }
}
