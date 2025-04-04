using AuthAPI.Models;

namespace AuthAPI.Repository.Interface
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync<T>(T model, string subject, string message) where T: class;
    }
}
