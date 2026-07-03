using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class BrevoSettings
    {
        public string SmtpHost { get; set; } 
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = default!;
        public string SenderName { get; set; }
        public string SmtpUser { get; set; }
        public string ApiKey { get; set; }    // Brevo SMTP API Key (not the master key)
    }
}
