using TaskTracker.Api.Exceptions;
using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Models.Entities;
using TaskTracker.Api.Models.Enums;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<PagedResult<TaskItemDto>> GetProjectTasksAsync(int projectId, TaskQueryParameters query, int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var (items, totalCount) = await _taskRepository.GetProjectTasksAsync(projectId, query);

        return new PagedResult<TaskItemDto>
        {
            Items = items.Select(ToDto),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskItemDto> GetByIdAsync(int taskId, int currentUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureProjectMemberAsync(task.ProjectId, currentUserId);
        return ToDto(task);
    }

    public async Task<TaskItemDto> CreateAsync(int projectId, CreateTaskDto dto, int currentUserId)
    {
        await EnsureProjectMemberAsync(projectId, currentUserId);

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Status = TaskStatusEnum.Todo,
            ProjectId = projectId,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);

        var created = await GetTaskOrThrowAsync(task.Id);
        return ToDto(created);
    }

    public async Task<TaskItemDto> UpdateAsync(int taskId, UpdateTaskDto dto, int currentUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureProjectMemberAsync(task.ProjectId, currentUserId);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.SaveChangesAsync();
        return ToDto(task);
    }

    public async Task<TaskItemDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int currentUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureProjectMemberAsync(task.ProjectId, currentUserId);

        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.SaveChangesAsync();
        return ToDto(task);
    }

    public async Task<TaskItemDto> AssignAsync(int taskId, AssignTaskDto dto, int currentUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureProjectMemberAsync(task.ProjectId, currentUserId);

        if (dto.AssignedToUserId.HasValue)
        {
            var assigneeMembership = await _projectRepository.GetMembershipAsync(task.ProjectId, dto.AssignedToUserId.Value);
            if (assigneeMembership is null)
            {
                throw new InvalidOperationException("You can only assign the task to a member of this project.");
            }
        }

        task.AssignedToUserId = dto.AssignedToUserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.SaveChangesAsync();

        var updated = await GetTaskOrThrowAsync(taskId);
        return ToDto(updated);
    }

    public async Task DeleteAsync(int taskId, int currentUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        var project = await EnsureProjectMemberAsync(task.ProjectId, currentUserId);

        var isCreator = task.CreatedByUserId == currentUserId;
        var isOwner = project.OwnerId == currentUserId;

        if (!isCreator && !isOwner)
        {
            throw new ForbiddenException("Only the task's creator or the project owner can delete this task.");
        }

        await _taskRepository.DeleteAsync(task);
    }

    private async Task<TaskItem> GetTaskOrThrowAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }
        return task;
    }

    private async Task<Project> EnsureProjectMemberAsync(int projectId, int userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        var membership = await _projectRepository.GetMembershipAsync(projectId, userId);
        if (membership is null)
        {
            throw new ForbiddenException("You are not a member of this project.");
        }

        return project;
    }

    private static TaskItemDto ToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        ProjectId = task.ProjectId,
        AssignedToUserId = task.AssignedToUserId,
        AssignedToUsername = task.AssignedToUser?.Username,
        CreatedByUserId = task.CreatedByUserId,
        CreatedByUsername = task.CreatedByUser.Username,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
