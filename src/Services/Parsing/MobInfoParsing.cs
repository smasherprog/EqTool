using EQTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public static class MobInfoParsing
    {
        public static List<TestUriViewModel> ParseKnownLoot(List<string> splits)
        {
            return Parse("known_loot", splits);
        }

        private static List<TestUriViewModel> Parse(string name, List<string> splits)
        {
            var ret = new List<TestUriViewModel>();
            var specials = StripHTML(GetValue(name, splits)).Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            var specialcopy = new List<string>();
            foreach (var item in specials)
            {
                var s = item.Split(',');
                specialcopy.AddRange(s.Select(x => x.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)));
            }

            foreach (var item in specialcopy.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                if (item.ToLower().Trim().Contains("casts:"))
                {
                    continue;
                }
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
                            indexofend = item.IndexOf("(Faction)");
                            if (indexofend != -1)
                            {
                                var model = new TestUriViewModel();
                                var diff = indexofend - (indexof + 2);
                                model.Name = item.Substring(indexof + 2, diff).Trim();
                                model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                                indexofend = item.IndexOf("]]");
                                if (indexofend != -1)
                                {
                                    model.Name += item.Substring(indexofend + 2);
                                }
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

        public static List<TestUriViewModel> ParseSpecials(List<string> splits)
        {
            return Parse("special", splits);
        }

        public static List<TestUriViewModel> ParseFactions(List<string> splits)
        {
            return Parse("factions", splits);
        }
        public static List<TestUriViewModel> ParseOpposingFactions(List<string> splits)
        {
            return Parse("opposing_factions", splits);
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
