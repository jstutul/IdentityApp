namespace API.DTOs.Account
{
    public class EmailSendDto
    {
        public EmailSendDto(string to,string body,string subject)
        {
            To = to;
            Subject= subject;
            Body = body;
        }
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}
