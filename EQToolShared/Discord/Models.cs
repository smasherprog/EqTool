using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public class embedFields
    {
        public int? Price
        {
            get
            {
                if (name == "No Price Listed" || name.StartsWith("00") || name.Contains("00000000000000000"))
                {
                    return null;
                }

                _ = float.TryParse(new string(name.Where(a => char.IsDigit(a) || a == '.').ToArray()), out var p);
                if (name.EndsWith("pp"))
                {
                    if (p <= 0)
                    {
                        return null;
                    }
                    else if (p > 1000000)
                    {
                        return null;
                    }
                    return (int)p;
                }
                else if (name.EndsWith("k"))

                {
                    var r = (int)(p * 1000);
                    if (r <= 0)
                    {
                        return null;
                    }
                    else if (r > 1000000)
                    {
                        return null;
                    }
                    return r;
                }
                return (int)p;
            }
        }

        public string ItemName => string.IsNullOrWhiteSpace(value) || !value.Contains("[") || !value.Contains("]")
                    ? string.Empty
                    : value.Substring(1, value.IndexOf("]")).Trim(']').Trim('[').Trim();

        public string name { get; set; }
        public string value { get; set; }
    }
    public class AuthorMessage
    {
        public string name { get; set; }
    }

    public class MessageEmbed
    {
        public AuctionType AuctionType => author.name.StartsWith("[ WTB ]") ? AuctionType.WTB : (author.name.StartsWith("[ WTS ]") ? AuctionType.WTS : AuctionType.BOTH);
        public string AuctionPerson => author.name.Substring(author.name.LastIndexOf("   ") + 3).Trim();
        public AuthorMessage author { get; set; }
        public DateTimeOffset timestamp { get; set; }
        public List<embedFields> fields { get; set; }
    }

    public class Message
    {
        public long id { get; set; }
        public List<MessageEmbed> embeds { get; set; }
    }

}
