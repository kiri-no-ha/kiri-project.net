using MailKit.Net.Smtp;
using MailKit.Security;
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
        // === Читаем настройки SMTP ===
        var host = _config["Smtp:Host"];
        var portStr = _config["Smtp:Port"];
        var user = _config["Smtp:Username"];
        var pass = _config["Smtp:Password"];

        // === Логируем, что реально пришло из конфига ===
        Console.WriteLine(">>> ========================================");
        Console.WriteLine($">>> SMTP Host: {host ?? "<NULL>"}");
        Console.WriteLine($">>> SMTP Port: {portStr ?? "<NULL>"}");
        Console.WriteLine($">>> SMTP User: {user ?? "<NULL>"}");
        Console.WriteLine($">>> SMTP Pass: {(string.IsNullOrEmpty(pass) ? "<NULL>" : "***")}");
        Console.WriteLine($">>> КОД ДЛЯ {toEmail}: {code}");
        Console.WriteLine(">>> ========================================");

        // === Если SMTP не настроен — просто оставляем код в консоли ===
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine(">>> SMTP не настроен (host/user/pass пусты).");
            Console.WriteLine(">>> Код выведен в консоль — введи его вручную в форму.");
            return;
        }

        var port = int.TryParse(portStr, out var p) ? p : 587;

        // === Собираем письмо ===
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Game Auth", user));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your access code";
        message.Body = new TextPart("plain")
        {
            Text = $"Your code: {code}"
        };

        // === Отправляем, ловим ошибки ===
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine($">>> Письмо успешно отправлено на {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> ОШИБКА ОТПРАВКИ: {ex.Message}");
            Console.WriteLine($">>> Код на случай, если письмо не дошло: {code}");
        }
    }
}