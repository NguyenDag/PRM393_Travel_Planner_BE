using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;

public class UserRepository(Prm393TravelPlannerContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public Task<bool> EmailExistsAsync(string email)
        => db.Users.AnyAsync(u => u.Email == email.ToLower());

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
