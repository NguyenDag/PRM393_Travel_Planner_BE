using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TravelApp.API.Application.Interfaces;

namespace TravelApp.API.Application.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var smtpHost     = config["Email:SmtpHost"]!;
        var smtpPort     = int.Parse(config["Email:SmtpPort"] ?? "587");
        var smtpUser     = config["Email:SmtpUser"]!;
        var smtpPass     = config["Email:SmtpPass"]!;
        var fromName     = config["Email:FromName"] ?? "Travel Planner";
        var fromAddress  = config["Email:FromAddress"] ?? smtpUser;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Mã xác thực OTP của bạn";

        message.Body = new TextPart("html")
        {
            Text = $"""
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

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
