using MailKit.Net.Smtp;
using MimeKit;

namespace FloraMind_V1.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                // Buradaki "FloraMind" ismi maillerde gönderici adı olarak görünür
                emailMessage.From.Add(new MailboxAddress("FloraMind", "floramind.help@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Buraya aldığın 16 haneli kodu boşluksuz yapıştır
                    await client.AuthenticateAsync("floramind.help@gmail.com", "biwszztlichamgqm");

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
           
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MAİL HATASI: " + ex.Message);
                throw;
            }
        }
    }
}
