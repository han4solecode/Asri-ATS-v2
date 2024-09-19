namespace AsriATS.Application.Options
{
    public class MailOptions
    {
        public const string MailSettings = "MailSettings";

        public string EmailId { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string Host { get; set; } = String.Empty;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
}