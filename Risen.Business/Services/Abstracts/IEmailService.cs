using System.Threading;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
        Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
    }
}
