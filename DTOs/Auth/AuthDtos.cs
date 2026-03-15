using System.ComponentModel.DataAnnotations;

namespace TravelApp.API.Application.DTOs.Auth;

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record RegisterRequest(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, MinLength(6), StringLength(255)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword
);

public record SendOtpRequest(
    [Required, EmailAddress] string Email
);

public record VerifyOtpRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Otp
);

public record ResetPasswordRequest(
    [Required] string ResetToken,
    [Required, MinLength(6)] string NewPassword
);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    UserDto User
);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? AvatarUrl,
    DateTime CreatedAt
);

public record OtpResponse(string Message);
