using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Rolled_metal_products.Utility
{
 public class EmailSender : IEmailSender
 {
  private readonly IConfiguration _configuration;

  public SmtpSettings _smtpSettings { get; set; }

  public EmailSender(IConfiguration configuration)
  {
   _configuration = configuration;
   _smtpSettings = _configuration.GetSection("Smtp").Get<SmtpSettings>();
  }

  public Task SendEmailAsync(string email, string subject, string htmlMessage)
  {
   return Execute(email, subject, htmlMessage);
  }

  public async Task Execute(string email, string subject, string body)
  {
   var emailMessage = new MimeMessage();
   emailMessage.From.Add(new MailboxAddress("Sergey", "pizzzabanderasss@yandex.ru"));
   emailMessage.To.Add(new MailboxAddress("DotNetMastery", email));
   emailMessage.Subject = subject;
   var bodyBuilder = new BodyBuilder { HtmlBody = body };
   emailMessage.Body = bodyBuilder.ToMessageBody();
   
   using (var client = new SmtpClient())
   {
    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port,
     SecureSocketOptions.SslOnConnect); //SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
    await client.SendAsync(emailMessage);
    await client.DisconnectAsync(true);
   }
  }
 }
}
