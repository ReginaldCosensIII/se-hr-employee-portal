using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SeHrEmployeePortal.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendNewRequestNotificationAsync(string employeeName, string certificationName, string managerName)
    {
        try
        {
            var host = _configuration["EmailSettings:SmtpHost"];
            var portString = _configuration["EmailSettings:SmtpPort"];
            var fromAddress = _configuration["EmailSettings:FromAddress"];
            var adminEmailsString = _configuration["EmailSettings:AdminNotificationEmails"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(adminEmailsString))
            {
                _logger.LogWarning("EmailSettings are not fully configured. Skipping email notification.");
                return;
            }

            int port = int.TryParse(portString, out var p) ? p : 25;

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = "New Certification Request Submitted",
                Body = $"A new certification request has been submitted.\n\nEmployee: {employeeName}\nCertification: {certificationName}\nManager: {managerName}\n\nPlease log in to the HR Certification Portal to review and approve this request.",
                IsBodyHtml = false
            };

            var emails = adminEmailsString.Split(',');
            foreach (var email in emails)
            {
                var trimmedEmail = email.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedEmail))
                {
                    message.To.Add(new MailAddress(trimmedEmail));
                }
            }

            var pickupDirectoryLocation = _configuration["EmailSettings:PickupDirectoryLocation"];

            using var client = new SmtpClient();
            client.UseDefaultCredentials = false;

            if (!string.IsNullOrEmpty(pickupDirectoryLocation))
            {
                System.IO.Directory.CreateDirectory(pickupDirectoryLocation);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = pickupDirectoryLocation;
            }
            else
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = host;
                client.Port = port;
            }

            await client.SendMailAsync(message);
            _logger.LogInformation("New request notification email sent successfully to {Count} admins.", message.To.Count);
        }
        catch (Exception ex)
        {
            // Catching everything here to ensure the email failure doesn't bubble up,
            // though the caller also catches it as a safety measure.
            _logger.LogError(ex, "Failed to send email notification inside SmtpEmailService.");
            throw; // Rethrow to be caught by the caller as requested by the prompt
        }
    }
}
