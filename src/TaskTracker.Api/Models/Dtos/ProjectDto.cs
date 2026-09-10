namespace TaskTracker.Api.Models.Dtos;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
