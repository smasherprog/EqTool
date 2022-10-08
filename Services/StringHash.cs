using System.Security.Cryptography;
using System.Text;

namespace EqTool.Services
{
    public static class StringHash
    {
        public static string sha256_hash(string value)
        {
            StringBuilder Sb = new();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                {
                    _ = Sb.Append(b.ToString("x2"));
                }
            }

            return Sb.ToString();
        }
    }
}
