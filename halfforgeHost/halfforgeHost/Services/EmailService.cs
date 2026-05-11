using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace halfforgeHost.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string userEmail, string subject, string body)
        {
            using var client = new SmtpClient();
            string username = _configuration["Email:Username"];
            string password = _configuration["Email:Password"];
            try
            {
                await client.ConnectAsync(
                    "smtps.aruba.it",
                    465,
                    SecureSocketOptions.SslOnConnect
                );

                await client.AuthenticateAsync(
                    username,
                    password
                );

                // ==========================
                // EMAIL CHE ARRIVA AD HALFFORGE
                // ==========================

                var adminMessage = new MimeMessage();

                adminMessage.From.Add(new MailboxAddress("Halfforge", username));
                adminMessage.To.Add(MailboxAddress.Parse(username));

                adminMessage.ReplyTo.Add(MailboxAddress.Parse(userEmail));

                adminMessage.Subject = $"Nuovo contatto: {subject}";

                var adminBuilder = new BodyBuilder
                {
                    TextBody =
                    $@"Nuovo messaggio dal sito
                    Email: {userEmail}
                    Messaggio: {body}"
                };

                adminMessage.Body = adminBuilder.ToMessageBody();

                await client.SendAsync(adminMessage);

                // ==========================
                // EMAIL DI CONFERMA ALL'UTENTE
                // ==========================

                var userMessage = new MimeMessage();

                userMessage.From.Add(new MailboxAddress("Halfforge", username));
                userMessage.To.Add(MailboxAddress.Parse(userEmail));

                userMessage.Subject = "Abbiamo ricevuto la tua richiesta";

                var userBuilder = new BodyBuilder
                {
                    HtmlBody = @"
                    <p>Ciao,</p>
                    <p>Abbiamo ricevuto correttamente la tua richiesta.</p>
                    <p>
                        Il nostro team la esaminerà e ti ricontatteremo il prima possibile se necessario.
                        Grazie per aver contattato Halfforge.</p>
                     <p>
                    — Halfforge<br>
                    <a href='https://halfforge.com'>halfforge.com</a> </p>"
                };

                userMessage.Body = userBuilder.ToMessageBody();
                await client.SendAsync(userMessage);
                await client.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email: {ex}");
                throw;
            }
        }
    }
}
