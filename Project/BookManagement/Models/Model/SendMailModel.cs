namespace BookManagement.Models.Model
{
    public class SendMailModel
    {
        public string From { get; set; }

        public string To { get; set; }

        public List<string> Cc { get; set; }

        public string Subject { get; set; }

        public string EmailContent { get; set; }
    }
}
