using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using PRM393_Travel_Planner_BE.Models;
using TravelApp.API.Application.Interfaces;

namespace TravelApp.API.Application.Services;

public class EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _email = emailOptions.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_email.FromName, _email.FromAddress));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Mã xác thực OTP của bạn";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: auto;">
                <h2 style="color: #4F7FFF;">Du Xuân Planner</h2>
                <p>Xin chào,</p>
                <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
                <div style="font-size: 36px; font-weight: bold; letter-spacing: 8px;
                            text-align: center; padding: 20px; background: #f0f4ff;
                            border-radius: 8px; color: #4F7FFF;">
                    {otp}
                </div>
                <p style="margin-top: 20px; color: #666;">
                    Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này với ai.
                </p>
                <hr style="border: none; border-top: 1px solid #eee;" />
                <p style="font-size: 12px; color: #aaa;">
                    Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
                </p>
            </div>
            """
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        // Cài đặt Timeout (15s) tránh lỗi hang vô tận gây request timeout (499)
        client.Timeout = 15000;

        try
        {
            _logger.LogInformation("Đang thử kết nối SMTP tới {Host}:{Port}...", _email.SmtpHost, _email.SmtpPort);
            
            await client.ConnectAsync(_email.SmtpHost, _email.SmtpPort, SecureSocketOptions.StartTls);
            _logger.LogInformation("Đã kết nối SMTP thành công. Đang xác thực với tài khoản {User}...", _email.SmtpUser);
            
            await client.AuthenticateAsync(_email.SmtpUser, _email.SmtpPass);
            _logger.LogInformation("Xác thực SMTP thành công. Đang gửi email...");
            
            await client.SendAsync(message);
            _logger.LogInformation("Gửi email thành công tới {ToEmail}!", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LỖI KHI GỬI EMAIL: {Message}", ex.Message);
            // Throw lên để Middleware trả về 500 hoặc 400 cho Frontend thay vì hang
            throw new InvalidOperationException($"Lỗi hệ thống khi gửi email: {ex.Message}", ex);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}