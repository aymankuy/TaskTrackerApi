using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.Models.Dtos;

public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}
