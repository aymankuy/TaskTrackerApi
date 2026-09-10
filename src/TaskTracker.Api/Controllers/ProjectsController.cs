using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var projects = await _projectService.GetUserProjectsAsync(GetCurrentUserId());
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var project = await _projectService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
    {
        var project = await _projectService.UpdateAsync(id, dto, GetCurrentUserId());
        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _projectService.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, AddProjectMemberDto dto)
    {
        await _projectService.AddMemberAsync(id, dto, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        await _projectService.RemoveMemberAsync(id, userId, GetCurrentUserId());
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirst("userId")?.Value;
        return int.Parse(value!);
    }
}
