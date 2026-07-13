using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink);

    Task SendDoctorApprovalAsync(string toEmail, string toName);
    Task SendDoctorRejectionAsync(string toEmail, string toName, string? reason = null);
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
}
