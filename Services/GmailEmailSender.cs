using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Threading.Tasks;

namespace BackendApi.Services
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GmailEmailSender> _logger;

        public GmailEmailSender(IConfiguration config, ILogger<GmailEmailSender> logger)
        {
            _config = config;
            _logger = logger;

            var appPassword = _config["Gmail:AppPassword"];
            if (string.IsNullOrEmpty(appPassword))
            {
                throw new InvalidOperationException("Gmail 應用程式密碼未設定");
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var senderEmail = _config["Gmail:SenderEmail"] ?? "householdmaiden@gmail.com";
            var senderName = _config["Gmail:SenderName"] ?? "AI 智慧家庭管家";
            var appPassword = _config["Gmail:AppPassword"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, appPassword);
                await client.SendAsync(message);
                _logger.LogInformation("Gmail 郵件發送成功至 {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gmail 寄信失敗");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}