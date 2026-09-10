using TaskTracker.Api.Models.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
    Task AddAsync(User user);
}
