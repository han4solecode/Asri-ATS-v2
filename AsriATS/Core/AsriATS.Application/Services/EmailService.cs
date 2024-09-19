using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AsriATS.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailOptions _mailOptions;

        public EmailService(IOptions<MailOptions> options)
        {
            _mailOptions = options.Value;
        }

        public async Task<bool> SendEmailAsync(EmailDataDto emailData)
        {
            var emailMessage = CreateEmailMessage(emailData);
            var result = await Send(emailMessage);

            return result;
        }

        private MimeMessage CreateEmailMessage(EmailDataDto emailData)
        {
            var emailMessage = new MimeMessage();
            var emailFrom = new MailboxAddress(_mailOptions.Name, _mailOptions.EmailId);

            emailMessage.From.Add(emailFrom);

            // Multiple email receiver logic
            if (emailData.EmailToIds != null && emailData.EmailToIds.Count != 0)
            {
                foreach (var to in emailData.EmailToIds)
                {
                    var emailTos = new MailboxAddress(to ,to);
                    emailMessage.To.Add(emailTos);
                }
            }

            // Multiple email cc logic
            if (emailData.EmailCCIds != null && emailData.EmailCCIds.Count != 0)
            {
                foreach (var cc in emailData.EmailCCIds)
                {
                    var emailCc = new MailboxAddress(cc ,cc);
                    emailMessage.Cc.Add(emailCc);
                }
            }

            emailMessage.Subject = emailData.EmailSubject;

            var emailBodyBuilder = new BodyBuilder
            {
                HtmlBody = emailData.EmailBody
            };

            // Email attachment logic
            // This logic is used to set a server generated file as an email attachment
            if (emailData.AttachmentFiles != null && emailData.AttachmentFiles.Count != 0)
            {
                foreach (var file in emailData.AttachmentFiles)
                {
                    // pass file to emailBodyBuilder
                    emailBodyBuilder.Attachments.Add(file);
                }
            }

            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            return emailMessage;
        }

        private async Task<bool> Send(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_mailOptions.Host, _mailOptions.Port, _mailOptions.UseSSL);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_mailOptions.UserName, _mailOptions.Password);
                await client.SendAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                var a = ex;
                return false;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}