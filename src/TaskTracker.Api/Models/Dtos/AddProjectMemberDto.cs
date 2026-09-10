using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.Models.Dtos;

public class AddProjectMemberDto
{
    [Required]
    public int UserId { get; set; }
}
