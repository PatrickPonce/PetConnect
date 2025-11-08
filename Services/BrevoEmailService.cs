// Archivo: Services/BrevoEmailService.cs

using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using PetConnect.Services;
using Task = System.Threading.Tasks.Task;

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public BrevoEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _configuration["Brevo:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("La API Key de Brevo no está configurada.");
        }

        // --- INICIO DE LA LÍNEA CORREGIDA ---
        // Usamos el nombre completo para evitar el conflicto de nombres
        var config = new brevo_csharp.Client.Configuration();
        // --- FIN DE LA LÍNEA CORREGIDA ---

        config.ApiKey.Add("api-key", apiKey);

        var apiInstance = new TransactionalEmailsApi(config);

        var senderEmail = "min.sunhee.nwn@gmail.com"; // <-- ¡RECUERDA CAMBIAR ESTO SI ES NECESARIO!
        var senderName = "Equipo de Purr & Paws";
        var sender = new SendSmtpEmailSender(senderName, senderEmail);

        var to = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) };

        var sendSmtpEmail = new SendSmtpEmail(
            sender: sender,
            to: to,
            htmlContent: htmlContent,
            subject: subject
        );

        await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }
}