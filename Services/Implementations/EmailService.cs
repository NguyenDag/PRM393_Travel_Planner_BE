using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PRM393_Travel_Planner_BE.Models;
using TravelApp.API.Application.Interfaces;

namespace TravelApp.API.Application.Services;

public class EmailService(IOptions<EmailSettings> emailOptions) : IEmailService
{
    private readonly EmailSettings _email = emailOptions.Value;

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_email.FromAddress, _email.FromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = "Mã xác thực OTP của bạn";
        message.IsBodyHtml = true;
        message.Body = $"""
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
            """;

        using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort);
        client.Credentials = new NetworkCredential(_email.SmtpUser, _email.SmtpPass);
        client.EnableSsl = true;

        await client.SendMailAsync(message);
    }
}