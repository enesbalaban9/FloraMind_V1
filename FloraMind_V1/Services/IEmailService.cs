using MailKit.Net.Smtp;
using MimeKit;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("FloraMind", "floramind.help@gmail.com"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using (var client = new SmtpClient())
        {
            // Gmail için: "smtp.gmail.com", 587, false
            // Outlook için: "smtp.office365.com", 587, false
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

            // DİKKAT: Buradaki şifre Gmail şifreniz değil, "Uygulama Şifresi" olmalıdır.
            await client.AuthenticateAsync("floramind.help@gmail.com", "biwszztlichamgqm");

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }

}