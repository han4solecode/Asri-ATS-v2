using AsriATS.Application.DTOs.Email;

namespace AsriATS.Application.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailDataDto emailData);
    }
}