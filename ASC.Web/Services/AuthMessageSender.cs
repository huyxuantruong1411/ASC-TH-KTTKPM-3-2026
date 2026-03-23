using System.Threading.Tasks;
using ASC.Web.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security; // Thêm thư viện này cho StartTls

namespace ASC.Web.Services
{
    // Fix: Cho phép class này đại diện cho cả IEmailSender của Identity UI
    public class AuthMessageSender : ASC.Web.Services.IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, ISmsSender
    {
        private IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("ASC Admin", _settings.Value.SMTPAccount));
                emailMessage.To.Add(new MailboxAddress("User", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
                {
                    // Bỏ qua lỗi chứng chỉ SSL khi test ở localhost
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Ép ứng dụng văng lỗi ra màn hình đỏ để biết chính xác sai ở đâu
                throw new Exception($"[LỖI GỬI MAIL]: {ex.Message} | Server: {_settings.Value?.SMTPServer} | Account: {_settings.Value?.SMTPAccount}");
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            return Task.FromResult(0);
        }
    }
}