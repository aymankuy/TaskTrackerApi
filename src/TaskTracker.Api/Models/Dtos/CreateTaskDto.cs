using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Models.Dtos;

public class CreateTaskDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }
}
