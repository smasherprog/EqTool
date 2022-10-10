using System.Security.Cryptography;
using System.Text;

namespace EQTool.Services
{
    public static class StringHash
    {
        public static string sha256_hash(string value)
        {
            var Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(value));

                foreach (var b in result)
                {
                    _ = Sb.Append(b.ToString("x2"));
                }
            }

            return Sb.ToString();
        }
    }
}
