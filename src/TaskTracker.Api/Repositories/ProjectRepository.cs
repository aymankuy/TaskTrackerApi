using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models.Entities;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(int userId)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProjectMember?> GetMembershipAsync(int projectId, int userId)
    {
        return await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(ProjectMember member)
    {
        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(ProjectMember member)
    {
        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
