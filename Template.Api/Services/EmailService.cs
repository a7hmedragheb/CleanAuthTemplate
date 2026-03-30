using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Template.Api.Services;

public class EmailService : IEmailSender
{
	private readonly MailSettings _mailSettings;
	private readonly ILogger<EmailService> _logger;
	public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
	{
		_mailSettings = mailSettings.Value;
		_logger = logger;
	}
	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var message = new MimeMessage
		{
			Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail),
			Subject = subject
		};

		message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
		message.To.Add(MailboxAddress.Parse(email));

		var builder = new BodyBuilder
		{
			HtmlBody = htmlMessage
		};

		message.Body = builder.ToMessageBody();

		using var smtp = new SmtpClient();
		try
		{
			await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
			await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
			await smtp.SendAsync(message);
			_logger.LogInformation("Email sent successfully to {Email}", email);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending email to {Email}", email);
			throw;
		}
		finally
		{
			await smtp.DisconnectAsync(true);
		}
	}
}