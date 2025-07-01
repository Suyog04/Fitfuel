using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _apiKey = _configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid:ApiKey not found in configuration");
        _fromEmail = _configuration["SendGrid:FromEmail"] ?? throw new ArgumentNullException("SendGrid:FromEmail not found in configuration");
        _fromName = _configuration["SendGrid:FromName"] ?? "FitFuel"; // Optional default name
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

            _logger.LogInformation("SendGrid email sent to {ToEmail} with status {StatusCode}", toEmail, response.StatusCode);

            if ((int)response.StatusCode >= 400)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogWarning("SendGrid returned an error: {ResponseBody}", responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {ToEmail}", toEmail);
            throw;
        }
    }
}
