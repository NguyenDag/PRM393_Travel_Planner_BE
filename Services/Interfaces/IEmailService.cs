namespace TravelApp.API.Application.Interfaces;

public interface IEmailService
{
    /// <summary>Gửi mã OTP đến email người dùng.</summary>
    Task SendOtpAsync(string toEmail, string otp);
}
