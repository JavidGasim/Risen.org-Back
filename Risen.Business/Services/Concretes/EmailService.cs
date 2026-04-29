using Microsoft.Extensions.Options;
using Risen.Business.Services.Abstracts;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task SendAsync(string to, string subject, string body)
        {
            await SendAsync(to, subject, body, CancellationToken.None);
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            using var mail = new MailMessage(_settings.Email, to, subject, body)
            {
                IsBodyHtml = true
            };

            // SmtpClient.SendMailAsync does not accept CancellationToken directly
            await client.SendMailAsync(mail);
        }
    }
}
