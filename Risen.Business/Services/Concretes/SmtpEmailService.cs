using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.Business.Services.Abstracts;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Risen.Business.Services.Concretes
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _opt;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailOptions> opt, ILogger<SmtpEmailService> logger)
        {
            _opt = opt.Value;
            _logger = logger;
        }
        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            using var msg = new MailMessage();
            msg.From = new MailAddress(_opt.From);
            msg.To.Add(new MailAddress(to));
            msg.Subject = subject;
            msg.Body = htmlBody;
            msg.IsBodyHtml = true;

            using var smtp = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_opt.UserName))
            {
                smtp.Credentials = new NetworkCredential(_opt.UserName, _opt.Password);
            }

            try
            {
                // SmtpClient doesn't support CancellationToken; call SendMailAsync and ignore ct.
                await smtp.SendMailAsync(msg);
                _logger.LogInformation("Sent email to {To} subject {Subject}", to, subject);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        // Backwards-compatible overload
        public async Task SendAsync(string to, string subject, string body)
        {
            await SendAsync(to, subject, body, CancellationToken.None);
        }
    }
}
