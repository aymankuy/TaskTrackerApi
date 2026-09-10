using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Models.Dtos;

public class UpdateTaskStatusDto
{
    [Required]
    public TaskStatusEnum Status { get; set; }
}
