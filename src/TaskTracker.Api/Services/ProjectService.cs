using TaskTracker.Api.Exceptions;
using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Models.Entities;
using TaskTracker.Api.Models.Enums;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int currentUserId)
    {
        var projects = await _projectRepository.GetUserProjectsAsync(currentUserId);
        return projects.Select(ToDto);
    }

    public async Task<ProjectDto> GetByIdAsync(int projectId, int currentUserId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        await EnsureMemberAsync(projectId, currentUserId);
        return ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, int currentUserId)
    {
        var project = new Project
        {
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);

        var ownerMembership = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = currentUserId,
            Role = ProjectRole.Owner,
            CreatedAt = DateTime.UtcNow
        };
        await _projectRepository.AddMemberAsync(ownerMembership);

        var created = await GetProjectOrThrowAsync(project.Id);
        return ToDto(created);
    }

    public async Task<ProjectDto> UpdateAsync(int projectId, UpdateProjectDto dto, int currentUserId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureOwner(project, currentUserId);

        project.Title = dto.Title;
        project.Description = dto.Description;
        await _projectRepository.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task DeleteAsync(int projectId, int currentUserId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureOwner(project, currentUserId);

        await _projectRepository.DeleteAsync(project);
    }

    public async Task AddMemberAsync(int projectId, AddProjectMemberDto dto, int currentUserId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureOwner(project, currentUserId);

        var userToAdd = await _userRepository.GetByIdAsync(dto.UserId);
        if (userToAdd is null)
        {
            throw new NotFoundException("User not found.");
        }

        var existingMembership = await _projectRepository.GetMembershipAsync(projectId, dto.UserId);
        if (existingMembership is not null)
        {
            throw new InvalidOperationException("User is already a member of this project.");
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId,
            Role = ProjectRole.Member,
            CreatedAt = DateTime.UtcNow
        };
        await _projectRepository.AddMemberAsync(member);
    }

    public async Task RemoveMemberAsync(int projectId, int userIdToRemove, int currentUserId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureOwner(project, currentUserId);

        var membership = await _projectRepository.GetMembershipAsync(projectId, userIdToRemove);
        if (membership is null)
        {
            throw new NotFoundException("This user is not a member of the project.");
        }

        if (membership.Role == ProjectRole.Owner)
        {
            throw new InvalidOperationException("The project owner cannot be removed.");
        }

        await _projectRepository.RemoveMemberAsync(membership);
    }

    private async Task<Project> GetProjectOrThrowAsync(int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }
        return project;
    }

    private async Task EnsureMemberAsync(int projectId, int userId)
    {
        var membership = await _projectRepository.GetMembershipAsync(projectId, userId);
        if (membership is null)
        {
            throw new ForbiddenException("You are not a member of this project.");
        }
    }

    private static void EnsureOwner(Project project, int userId)
    {
        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can perform this action.");
        }
    }

    private static ProjectDto ToDto(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Description = project.Description,
        OwnerId = project.OwnerId,
        OwnerUsername = project.Owner.Username,
        CreatedAt = project.CreatedAt
    };
}
