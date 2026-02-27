using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace BackendendApi.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;

            // ★ 新增：啟動時立即記錄 SendGrid ApiKey 的前 10 個字元（避免完整 key 外洩）
            var apiKey = _config["SendGrid:ApiKey"];
            var keyPreview = string.IsNullOrEmpty(apiKey)
                ? "null 或空字串"
                : apiKey.Substring(0, Math.Min(10, apiKey.Length)) + "...";

            _logger.LogInformation(
                "EmailSender 初始化完成 - SendGrid ApiKey 前綴: {KeyPreview}",
                keyPreview
            );
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API Key 未設定");
                throw new InvalidOperationException("SendGrid API Key 未設定");
            }

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(
                _config["SendGrid:SenderEmail"] ?? "yzhu33729@gmail.com",
                _config["SendGrid:SenderName"] ?? "AI 智慧家庭管家");

            var to = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("SendGrid 郵件發送成功至 {Email}", email);
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid 發送失敗，狀態碼: {StatusCode}，錯誤: {Error}", response.StatusCode, errorBody);
                    throw new Exception($"SendGrid 發送失敗：{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendGrid 寄信例外發生");
                throw;
            }
        }
    }
}