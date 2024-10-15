using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace EQtoolsTests
{
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

    [TestClass]
    public class DiscordTests
    {
        [TestMethod]
        public void embedtest()
        {
            var obj = new embedFields
            {
                name = "3.5k"
            };
            Assert.AreEqual(obj.Price, 3500);
        }
    }
}
