using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;

public class RefreshTokenRepository(Prm393TravelPlannerContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        => db.RefreshTokens
             .Include(r => r.User)
             .FirstOrDefaultAsync(r => r.Token == tokenHash);

    public async Task AddAsync(RefreshToken token)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllByUserAsync(Guid userId)
    {
        await db.RefreshTokens
            .Where(r => r.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        db.RefreshTokens.Remove(token);
        await db.SaveChangesAsync();
    }

    public async Task DeleteExpiredAsync()
    {
        await db.RefreshTokens
            .Where(r => r.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
