using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Models.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetProjectTasksAsync(int projectId, TaskQueryParameters query);
    Task AddAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task SaveChangesAsync();
}
