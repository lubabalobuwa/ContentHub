using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
