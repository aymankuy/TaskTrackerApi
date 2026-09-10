using TaskTracker.Api.Models.Enums;

namespace TaskTracker.Api.Models.Dtos;

public class TaskQueryParameters
{
    public TaskStatusEnum? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public int? AssignedUserId { get; set; }
    public string? SortBy { get; set; }

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 20 : value;
    }
}
