using PRM393_Travel_Planner_BE.Models;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task RevokeAllByUserAsync(Guid userId);
    Task RevokeAsync(RefreshToken token);
    Task DeleteExpiredAsync();
}
