using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Repository.Interface;
using System.Net;
using System.Net.Mail;

namespace AuthAPI.Repository.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> SendEmailAsync<T>(T model, string subject, string message) where T : class
        {
            bool status = false;
            try
            {
                var email = model?.GetType().GetProperty("Email")?.GetValue(model, null)?.ToString() ?? string.Empty;
                EmailSettingModel emailSetting = new EmailSettingModel()
                {
                    SecretKey = _configuration.GetValue<string>("AppSettings:SecreatKey") ?? string.Empty,
                    Port = _configuration.GetValue<int>("AppSettings:EmailSettings:Port"),
                    SmtpServer = _configuration.GetValue<string>("AppSettings:EmailSettings:SmtpServer") ?? string.Empty,
                    EnablSSl = Convert.ToBoolean(_configuration["AppSettings:EmailSettings:EnablSSl"]),
                    From = _configuration.GetValue<string>("AppSettings:EmailSettings:From") ?? string.Empty
                };
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(emailSetting.From),
                    Subject = subject,
                    Body = message
                };
                mail.To.Add(email);
                SmtpClient smtpClient = new SmtpClient(emailSetting.SmtpServer)
                {
                    Port = emailSetting.Port,
                    Credentials = new NetworkCredential(emailSetting.From, emailSetting.SecretKey),
                    EnableSsl = emailSetting.EnablSSl
                };
                await smtpClient.SendMailAsync(mail);
                status = true;
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }
    }
}
