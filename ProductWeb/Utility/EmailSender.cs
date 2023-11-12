using Microsoft.AspNetCore.Identity.UI.Services;

namespace ProductWeb.Utility
{
    //เชื่อมโยงกับบทบาท
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}
