using TaskTracker.Api.Models.Enums;


namespace TaskTracker.Api.Models.Entities;

public class ProjectMember
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public ProjectRole Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
