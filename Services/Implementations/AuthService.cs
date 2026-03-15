using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using PRM393_Travel_Planner_BE.Models;
using TravelApp.API.Application.DTOs.Auth;
using TravelApp.API.Application.Interfaces;

namespace TravelApp.API.Application.Services;

public class AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository tokenRepo,
    IJwtService jwt,
    IEmailService emailService,
    IMemoryCache cache,
    IConfiguration config) : IAuthService
{
    private readonly int _refreshDays =
        int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "30");

    // ── OTP Settings ──────────────────────────────────────────────────────────
    private const int OtpLength       = 6;
    private const int OtpExpiryMinutes = 5;
    private const string OtpCachePrefix      = "otp_";
    private const string ResetTokenCachePrefix = "reset_";

    // ── Register ──────────────────────────────────────────────────────────────
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        if (await userRepo.EmailExistsAsync(req.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = req.FullName,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await userRepo.AddAsync(user);
        return await BuildAuthResponseAsync(user);
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await userRepo.GetByEmailAsync(req.Email)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        return await BuildAuthResponseAsync(user);
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest req)
    {
        var tokenHash = HashToken(req.RefreshToken);
        var stored = await tokenRepo.GetByTokenHashAsync(tokenHash)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        if (stored.ExpiresAt < DateTime.UtcNow)
        {
            await tokenRepo.RevokeAsync(stored);
            throw new UnauthorizedAccessException("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
        }

        await tokenRepo.RevokeAsync(stored);
        return await BuildAuthResponseAsync(stored.User);
    }

    // ── Revoke single token ───────────────────────────────────────────────────
    public async Task RevokeTokenAsync(string refreshToken, Guid userId)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await tokenRepo.GetByTokenHashAsync(tokenHash)
            ?? throw new KeyNotFoundException("Token không tồn tại.");

        if (stored.UserId != userId)
            throw new UnauthorizedAccessException();

        await tokenRepo.RevokeAsync(stored);
    }

    // ── Revoke all (logout all devices) ──────────────────────────────────────
    public Task RevokeAllTokensAsync(Guid userId)
        => tokenRepo.RevokeAllByUserAsync(userId);

    // ── Change Password ───────────────────────────────────────────────────────
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await userRepo.UpdateAsync(user);
        await tokenRepo.RevokeAllByUserAsync(userId);
    }

    // ── Get Profile ───────────────────────────────────────────────────────────
    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");
        return MapToDto(user);
    }

    // ── Send OTP ──────────────────────────────────────────────────────────────
    public async Task SendOtpAsync(SendOtpRequest req)
    {
        // Kiểm tra email có tồn tại trong hệ thống không
        var user = await userRepo.GetByEmailAsync(req.Email)
            ?? throw new KeyNotFoundException("Email này chưa được đăng ký.");

        // Tạo OTP 6 chữ số
        var otp = GenerateOtp();

        // Lưu vào cache với TTL 5 phút
        var cacheKey = OtpCachePrefix + req.Email.ToLower();
        cache.Set(cacheKey, otp, TimeSpan.FromMinutes(OtpExpiryMinutes));

        // Gửi email
        await emailService.SendOtpAsync(req.Email, otp);
    }

    // ── Verify OTP ────────────────────────────────────────────────────────────
    public Task<string> VerifyOtpAsync(VerifyOtpRequest req)
    {
        var cacheKey = OtpCachePrefix + req.Email.ToLower();

        if (!cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != req.Otp)
            throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        // Xóa OTP đã dùng
        cache.Remove(cacheKey);

        // Tạo reset token (random, có TTL)
        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var resetKey = ResetTokenCachePrefix + resetToken;
        cache.Set(resetKey, req.Email.ToLower(), TimeSpan.FromMinutes(15));

        return Task.FromResult(resetToken);
    }

    // ── Reset Password ────────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordRequest req)
    {
        var resetKey = ResetTokenCachePrefix + req.ResetToken;

        if (!cache.TryGetValue(resetKey, out string? email) || string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

        var user = await userRepo.GetByEmailAsync(email)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await userRepo.UpdateAsync(user);

        // Xóa reset token và thu hồi tất cả session
        cache.Remove(resetKey);
        await tokenRepo.RevokeAllByUserAsync(user.Id);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var (accessToken, expiry) = jwt.GenerateAccessToken(user);
        var rawRefresh = jwt.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = HashToken(rawRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays),
            CreatedAt = DateTime.UtcNow,
        };

        await tokenRepo.AddAsync(refreshToken);
        return new AuthResponse(accessToken, rawRefresh, expiry, MapToDto(user));
    }

    private static string GenerateOtp()
    {
        // Tạo số ngẫu nhiên 6 chữ số dùng cryptographic RNG
        var bytes = RandomNumberGenerator.GetBytes(4);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return number.ToString("D6");
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static UserDto MapToDto(User u)
        => new(u.Id, u.FullName, u.Email, u.AvatarUrl, u.CreatedAt ?? DateTime.Now);
}
