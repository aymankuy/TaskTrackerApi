using TaskTracker.Api.Models.Enums;
namespace TaskTracker.Api.Models.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusEnum Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}