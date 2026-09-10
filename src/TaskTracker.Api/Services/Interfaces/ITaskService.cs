using TaskTracker.Api.Models.Dtos;

namespace TaskTracker.Api.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskItemDto>> GetProjectTasksAsync(int projectId, TaskQueryParameters query, int currentUserId);
    Task<TaskItemDto> GetByIdAsync(int taskId, int currentUserId);
    Task<TaskItemDto> CreateAsync(int projectId, CreateTaskDto dto, int currentUserId);
    Task<TaskItemDto> UpdateAsync(int taskId, UpdateTaskDto dto, int currentUserId);
    Task<TaskItemDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int currentUserId);
    Task<TaskItemDto> AssignAsync(int taskId, AssignTaskDto dto, int currentUserId);
    Task DeleteAsync(int taskId, int currentUserId);
}
