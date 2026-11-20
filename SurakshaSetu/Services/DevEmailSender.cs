using Microsoft.AspNetCore.Identity.UI.Services;

namespace SurakshaSetu.Services
{
    /// <summary>
    /// Development-only email sender: logs emails to the console instead of sending them.
    /// Replace with a real SMTP/SendGrid sender for production.
    /// </summary>
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;

        public DevEmailSender(ILogger<DevEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Strip HTML tags to get the plain reset link
            var plainText = System.Text.RegularExpressions.Regex.Replace(htmlMessage, "<.*?>", "");

            _logger.LogWarning(
                "\n\n========== DEV EMAIL ==========\n" +
                "TO:      {Email}\n" +
                "SUBJECT: {Subject}\n" +
                "BODY:    {Body}\n" +
                "================================\n",
                email, subject, plainText.Trim());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n========== DEV EMAIL ==========");
            Console.WriteLine($"TO:      {email}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"BODY:    {plainText.Trim()}");
            Console.WriteLine("================================\n");
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}

