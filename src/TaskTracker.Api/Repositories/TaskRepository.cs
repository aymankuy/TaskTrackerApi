using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Models.Entities;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetProjectTasksAsync(int projectId, TaskQueryParameters query)
    {
        var tasksQuery = _context.TaskItems
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.ProjectId == projectId);

        if (query.Status.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority.Value);
        }

        if (query.AssignedUserId.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.AssignedToUserId == query.AssignedUserId.Value);
        }

        tasksQuery = query.SortBy?.ToLowerInvariant() switch
        {
            "duedate" => tasksQuery.OrderBy(t => t.DueDate),
            "priority" => tasksQuery.OrderByDescending(t => t.Priority),
            "createdat" => tasksQuery.OrderByDescending(t => t.CreatedAt),
            _ => tasksQuery.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await tasksQuery.CountAsync();

        var items = await tasksQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(TaskItem task)
    {
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
