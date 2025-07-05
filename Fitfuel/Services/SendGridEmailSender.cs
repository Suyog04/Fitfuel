using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent);
}

public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(ILogger<SendGridEmailSender> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // ✅ Load from environment variables (e.g. .env or actual env)
        _apiKey = Environment.GetEnvironmentVariable("SendGrid__ApiKey")
            ?? throw new ArgumentNullException("SendGrid__ApiKey not found in environment");

        _fromEmail = Environment.GetEnvironmentVariable("SendGrid__FromEmail")
            ?? throw new ArgumentNullException("SendGrid__FromEmail not found in environment");

        _fromName = Environment.GetEnvironmentVariable("SendGrid__FromName") ?? "FitFuel";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("📧 Email sent to {ToEmail} with status {StatusCode}", toEmail, response.StatusCode);

            if ((int)response.StatusCode >= 400)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogWarning("⚠️ SendGrid returned an error: {ResponseBody}", responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception occurred while sending email to {ToEmail}", toEmail);
            throw;
        }
    }
}
