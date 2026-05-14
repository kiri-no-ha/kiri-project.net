using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendCodeAsync(string toEmail, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Game Auth", "noreply@example.com"));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your access code";
        message.Body = new TextPart("plain")
        {
            Text = $"Your code: {code}. It expires in 5 minutes."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), true);
        await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}