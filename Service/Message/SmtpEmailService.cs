using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Common;

namespace Domain.Message
{
    public class SmtpEmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            using var client = new SmtpClient(SmtpSettings.Host, SmtpSettings.Port)
            {
                Credentials = new NetworkCredential(SmtpSettings.Username, SmtpSettings.Password),
                EnableSsl = SmtpSettings.EnableSsl,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(SmtpSettings.FromEmail, SmtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
