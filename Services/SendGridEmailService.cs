using SendGrid;
using SendGrid.Helpers.Mail;

namespace PetConnect.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("La API Key de SendGrid no está configurada.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("no-reply@petconnect.com", "Equipo de PetConnect");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

            await client.SendEmailAsync(msg);
        }
    }
}