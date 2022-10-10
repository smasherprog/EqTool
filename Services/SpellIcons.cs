using EQTool.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TGASharpLib;

namespace EQTool.Services
{
    public static class SpellIcons
    {
        public static List<SpellIcon> GetSpellIcons()
        {
            var ret = new List<SpellIcon>();
            var directory = new DirectoryInfo(FindEq.BestGuessRootEqPath + "/uifiles/default/");
            var spellimages = directory.GetFiles()
                .Where(a => a.Name.StartsWith("spells0") && a.Name.EndsWith(".tga"))
                .ToList();
            foreach (var item in spellimages)
            {
                var img = TGA.FromFile(item.FullName);
                var numberonly = new string(item.Name.Where(a => char.IsNumber(a)).ToArray());
                var index = int.Parse(numberonly);
                var i = new SpellIcon
                {
                    Icon = img.ToBitmap().ToBitmapImage(),
                    SpellIndex = index,
                };
                ret.Add(i);
            }

            return ret;
        }

        private static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
