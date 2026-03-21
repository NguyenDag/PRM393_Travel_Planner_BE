using TravelApp.API.Application.DTOs.Auth;

namespace TravelApp.API.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task RevokeTokenAsync(string refreshToken, Guid userId);
    Task RevokeAllTokensAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserDto> GetProfileAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

    // ── OTP / Forgot Password ─────────────────────────────────────
    /// <summary>Tạo OTP, lưu cache, gửi email.</summary>
    Task SendOtpAsync(SendOtpRequest request);

    /// <summary>Xác minh OTP → trả về reset token.</summary>
    Task<string> VerifyOtpAsync(VerifyOtpRequest request);

    /// <summary>Đặt lại mật khẩu bằng reset token.</summary>
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
