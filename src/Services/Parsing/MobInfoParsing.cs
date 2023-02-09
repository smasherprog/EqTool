using EQTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public static class MobInfoParsing
    {
        public static List<TestUriViewModel> ParseKnwonLoot(List<string> splits)
        {
            var ret = new List<TestUriViewModel>();
            var specials = StripHTML(GetValue("known_loot", splits)).Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in specials.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                var indexof = item.IndexOf("{{");
                if (indexof != -1)
                {
                    var indexofend = item.IndexOf("}}");
                    if (indexofend != -1)
                    {
                        var model = new TestUriViewModel();
                        var diff = indexofend - (indexof + 3);
                        model.Name = item.Substring(indexof + 3, diff).Trim();
                        model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                        ret.Add(model);
                    }
                }
                else
                {
                    indexof = item.IndexOf("[[");
                    if (indexof != -1)
                    {
                        var indexofend = item.IndexOf("|Spell:");
                        if (indexofend != -1)
                        {
                            var model = new TestUriViewModel();
                            var diff = indexofend - (indexof + 2);
                            model.Name = item.Substring(indexof + 2, diff).Trim();
                            model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                            ret.Add(model);
                        }
                        else
                        {
                            indexofend = item.IndexOf("]]");
                            if (indexofend != -1)
                            {
                                var model = new TestUriViewModel();
                                var diff = indexofend - (indexof + 2);
                                model.Name = item.Substring(indexof + 2, diff).Trim();
                                model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                                model.Name += "  " + item.Substring(indexofend + 2).Trim();
                                ret.Add(model);
                            }
                        }
                    }
                    else
                    {
                        ret.Add(new TestUriViewModel
                        {
                            Url = string.Empty,
                            Name = item
                        });
                    }
                }
            }

            return ret;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        private static string GetValue(string propname, List<string> lines)
        {
            var ret = lines.FirstOrDefault(a => a.StartsWith(propname));
            if (string.IsNullOrWhiteSpace(ret))
            {
                return string.Empty;
            }
            var index = ret.IndexOf('=');
            return index != -1 ? ret.Substring(index + 1).Trim() : string.Empty;
        }
    }
}
