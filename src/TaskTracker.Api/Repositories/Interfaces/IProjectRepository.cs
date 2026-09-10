using TaskTracker.Api.Models.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetUserProjectsAsync(int userId);
    Task<Project?> GetByIdAsync(int id);
    Task<ProjectMember?> GetMembershipAsync(int projectId, int userId);
    Task AddAsync(Project project);
    Task AddMemberAsync(ProjectMember member);
    Task RemoveMemberAsync(ProjectMember member);
    Task DeleteAsync(Project project);
    Task SaveChangesAsync();
}
