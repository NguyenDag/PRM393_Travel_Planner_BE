using System.Security.Claims;
using PRM393_Travel_Planner_BE.Models;

public interface IJwtService
{
    (string token, DateTime expiry) GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateExpiredToken(string token);
}
