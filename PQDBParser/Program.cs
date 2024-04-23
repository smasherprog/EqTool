using EQToolShared.PQModels;
using EQToolShared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PQDBParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filepath = AppDomain.CurrentDomain.BaseDirectory;
            var dbfile = System.IO.Directory.GetFiles(filepath, "*.sql").FirstOrDefault();

            var data = File.ReadAllText(dbfile);
            var r = new PQData
            {
                Items = GetAll<item>(data, "INSERT INTO `items` VALUES "),
                npc_Types = GetAll<npc_type>(data, "INSERT INTO `npc_types` VALUES "),
                loottable_Entries = GetAll<loottable_entry>(data, "INSERT INTO `loottable_entries` VALUES "),
                lootdrop_Entries = GetAll<lootdrop_entry>(data, "INSERT INTO `lootdrop_entries` VALUES ")
            };
            var checkformanualmaps = System.IO.Directory.GetCurrentDirectory();
            BinarySerializer.WriteToBinaryFile(checkformanualmaps + "/outdata.bin", r);

        }
        static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }

        static T ApplyValuesFromStrings<T>(List<string> array, PropertyInfo[] props) where T : class, new()
        {
            var r = new T();
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var value = array[i].Trim('\'');
                //Debug.WriteLine($"{prop.Name}-{value}");
                if (value == "0000-00-00 00:00:00" || value == "NULL")
                {
                    continue;
                }
                var propvalue = ChangeType(value, prop.PropertyType);
                prop.SetValue(r, propvalue);
            }
            return r;
        }

        static List<string> SplitIgnoringQuotes(string input, char separator)
        {
            List<string> parts = new List<string>();
            bool insideQuotes = false;
            int start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && input[i - 1] == '\\' && input[i] == '\'')
                {
                    continue;
                }
                else if (input[i] == '\'')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (input[i] == separator && !insideQuotes)
                {
                    parts.Add(input.Substring(start, i - start));
                    start = i + 1;
                }
            }

            parts.Add(input.Substring(start));

            return parts;
        }

        static List<T> GetAll<T>(string datafile, string inserttofind) where T : class, new()
        {
            var ret = new List<T>();
            var insertindex = datafile.IndexOf(inserttofind);
            var props = typeof(T).GetProperties();
            do
            {
                var startindextocopy = insertindex + inserttofind.Length;
                var endindex = datafile.IndexOf(";", startindextocopy);
                var substr = datafile.Substring(startindextocopy, endindex - startindextocopy).Trim().TrimStart('(').TrimEnd(')');
                var splits = substr.Split(new string[] { "),(" }, StringSplitOptions.None);

                foreach (var item in splits)
                {
                    var innersplits = SplitIgnoringQuotes(item, ',');
                    ret.Add(ApplyValuesFromStrings<T>(innersplits, props));
                }
                insertindex = datafile.IndexOf(inserttofind, endindex);
            } while (insertindex != -1);
            return ret;
        }

    }
}
