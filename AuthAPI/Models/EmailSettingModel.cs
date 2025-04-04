namespace AuthAPI.Models
{
    public class EmailSettingModel
    {
        public string SecretKey { get; set; } = default!;
        public string From { get; set; } = default!;
        public string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
        public bool EnablSsl { get; set; }
    }
}
