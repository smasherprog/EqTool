namespace EQToolShared.Discord
{

    public class LoginRequest
    {
        public string captcha_key { get; set; } = string.Empty;
        public string gift_code_sku_id { get; set; } = string.Empty;
        public string login { get; set; } = string.Empty;
        public string login_source { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public bool undelete { get; set; } = false;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
    }

    public class Message
    {
        public long id { get; set; }
        public DateTimeOffset timestamp { get; set; }
        public string Text => string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim(new char[] { '\n', '`' });
        public string content { get; set; } = string.Empty;
    }
}
