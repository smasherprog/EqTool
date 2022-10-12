using EQTool.Services.WLD.Fragments;
using System;
using System.Collections.Generic;

namespace EQTool.Services.WLD
{

    public static class FragmentNameCleaner
    {
        private static readonly Dictionary<Type, string> _prefixes = new Dictionary<Type, string>
        {
            // Materials
            {typeof(MaterialList), "_MP"},
            {typeof(Material), "_MDF"},
            {typeof(Mesh), "_DMSPRITEDEF"},
            {typeof(LegacyMesh), "_DMSPRITEDEF"}
        };

        public static string CleanName(WldFragment fragment, bool toLower = true)
        {
            var cleanedName = fragment.Name;

            if (_prefixes.ContainsKey(fragment.GetType()))
            {
                cleanedName = cleanedName.Replace(_prefixes[fragment.GetType()], string.Empty);
            }

            if (toLower)
            {
                cleanedName = cleanedName.ToLower();
            }

            return cleanedName;
        }
    }
}
