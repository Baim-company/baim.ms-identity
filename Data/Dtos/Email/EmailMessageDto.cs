using MimeKit;


namespace Identity.API.Data.Dtos.Email;

public class EmailMessageDto
{
    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }

    public string HtmlFilePath { get; set; }

    public EmailMessageDto(IEnumerable<string> to, string subject, string content)
    {
        To = new List<MailboxAddress>();
        To.AddRange(to.Select(x => new MailboxAddress("email", x)));
        Subject = subject;
        Content = content;
    }
    public EmailMessageDto(IEnumerable<string> to, string subject)
    {
        To = new List<MailboxAddress>();
        To.AddRange(to.Select(x => new MailboxAddress("email", x)));
        Subject = subject;
    }
}