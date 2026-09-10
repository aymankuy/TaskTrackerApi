using TaskTracker.Api.Models.Dtos;

namespace TaskTracker.Api.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int currentUserId);
    Task<ProjectDto> GetByIdAsync(int projectId, int currentUserId);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, int currentUserId);
    Task<ProjectDto> UpdateAsync(int projectId, UpdateProjectDto dto, int currentUserId);
    Task DeleteAsync(int projectId, int currentUserId);
    Task AddMemberAsync(int projectId, AddProjectMemberDto dto, int currentUserId);
    Task RemoveMemberAsync(int projectId, int userIdToRemove, int currentUserId);
}
