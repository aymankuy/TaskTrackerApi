using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Models.Dtos;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> GetProjectTasks(int projectId, [FromQuery] TaskQueryParameters query)
    {
        var result = await _taskService.GetProjectTasksAsync(projectId, query, GetCurrentUserId());
        return Ok(result);
    }

    [HttpPost("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> Create(int projectId, CreateTaskDto dto)
    {
        var task = await _taskService.CreateAsync(projectId, dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpGet("api/tasks/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(task);
    }

    [HttpPut("api/tasks/{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
    {
        var task = await _taskService.UpdateAsync(id, dto, GetCurrentUserId());
        return Ok(task);
    }

    [HttpPatch("api/tasks/{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusDto dto)
    {
        var task = await _taskService.UpdateStatusAsync(id, dto, GetCurrentUserId());
        return Ok(task);
    }

    [HttpPatch("api/tasks/{id}/assign")]
    public async Task<IActionResult> Assign(int id, AssignTaskDto dto)
    {
        var task = await _taskService.AssignAsync(id, dto, GetCurrentUserId());
        return Ok(task);
    }

    [HttpDelete("api/tasks/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirst("userId")?.Value;
        return int.Parse(value!);
    }
}
