using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail(string to, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer is missing");
        var smtpPortStr = _config["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException("SmtpPort is missing");
        var smtpUser = _config["EmailSettings:SmtpUser"] ?? throw new ArgumentNullException("SmtpUser is missing");
        var smtpPassword = _config["EmailSettings:SmtpPassword"] ?? throw new ArgumentNullException("SmtpPassword is missing");


        if (!int.TryParse(smtpPortStr, out int smtpPort))
        {
            throw new FormatException("Invalid SMTP port format");
        }

        var mail = new MailMessage
        {
            From = new MailAddress(smtpUser),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        using (var smtp = new SmtpClient(smtpServer, smtpPort))
        {
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.TargetName = "STARTTLS/smtp.office365.com"; 



            try
            {
                smtp.Send(mail);
                Console.WriteLine("✅ Correo enviado correctamente.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"❌ EMAIL ERROR: {ex.Message}");
            }
        }
    }


}

