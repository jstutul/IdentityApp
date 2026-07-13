using API.DTOs.Account;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace API.Services
{
    public class EmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(EmailSendDto emailsend)
        {
            MailjetClient client = new MailjetClient(configuration["MailJet:ApiKey"], configuration["MailJet:SecrectKey"]);
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(configuration["Email:From"], configuration["Email:ApplicationName"]))
                .WithSubject(emailsend.Subject)
                .WithHtmlPart(emailsend.Body)
                .WithTo(new SendContact(emailsend.To))
                .Build();
            var response  = await client.SendTransactionalEmailAsync(email);
            if(response.Messages != null)
            {
                if (response.Messages[0].Status == "success")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
