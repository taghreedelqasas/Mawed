using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Maw3ed.BLL;  

public class EmailService : IEmailService
{
    private readonly BrevoSettings _settings;

    public EmailService(IOptions<BrevoSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Confirm your Maw3ed account";

        message.Body = new TextPart("html")
        {
            Text = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                        <h2>Welcome to Maw3ed 👋</h2>
                        <p>Hi {toName},</p>
                        <p>Please confirm your email address by clicking the button below:</p>
                        <a href='{confirmationLink}'
                           style='display:inline-block; padding:12px 24px; background:#2563EB;
                                  color:#fff; text-decoration:none; border-radius:6px;'>
                            Confirm Email
                        </a>
                        <p style='margin-top:16px; color:#6B7280; font-size:13px;'>
                            If you didn't create an account, you can safely ignore this email.
                        </p>
                    </div>"
        };

        using (var client = new SmtpClient())
        {

            // Brevo requires STARTTLS on port 587.
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);

            // Brevo SMTP auth: login = your Brevo account email, password = SMTP API Key.
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.ApiKey);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }
    }

    //Approval for doctor accounts

    public async Task SendDoctorApprovalAsync(string toEmail, string toName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Your Maw3ed account has been approved 🎉";

        message.Body = new TextPart("html")
        {
            Text = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                        <h2>Congratulations, Dr. {toName}! 🎉</h2>
                        <p>Your account on <strong>Maw3ed</strong> has been reviewed and approved by our admin team.</p>
                        <p>You can now log in and start managing your appointments.</p>
                        <p style='margin-top:16px; color:#6B7280; font-size:13px;'>
                            If you have any questions, feel free to contact our support team.
                        </p>
                    </div>"
        };
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.ApiKey);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }

    }

    //ResetPassword Email

    public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Reset your Maw3ed password";

        message.Body = new TextPart("html")
        {
            Text = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                        <h2>Password Reset Request 🔑</h2>
                        <p>Hi {toName},</p>
                        <p>We received a request to reset your password. Click the button below to set a new one:</p>
                        <a href='{resetLink}'
                           style='display:inline-block; padding:12px 24px; background:#DC2626;
                                  color:#fff; text-decoration:none; border-radius:6px;'>
                            Reset Password
                        </a>
                        <p style='margin-top:16px; color:#6B7280; font-size:13px;'>
                            This link will expire in 1 hour. If you didn't request a password reset, 
                            you can safely ignore this email.
                        </p>
                    </div>"
        };
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.ApiKey);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }
    }
    public async Task SendDoctorRejectionAsync(string toEmail, string toName, string? reason = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Update on your Maw3ed account request";

        var reasonHtml = string.IsNullOrWhiteSpace(reason)
            ? ""
            : $"<p><strong>Reason:</strong> {reason}</p>";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <h2>Application Update</h2>
                    <p>Hi Dr. {toName},</p>
                    <p>After reviewing your application, we're unable to approve your account on <strong>Maw3ed</strong> at this time.</p>
                    {reasonHtml}
                    <p style='margin-top:16px; color:#6B7280; font-size:13px;'>
                        If you believe this was a mistake or would like more information, please contact our support team.
                    </p>
                </div>"
        };
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.ApiKey);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }
    }
}
