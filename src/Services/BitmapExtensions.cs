using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace EQTool.Services
{
    public static class BitmapExtensions
    {
        public static BitmapImage ConvertToBitmapImage(this System.Drawing.Bitmap src)
        {
            using (var memory = new MemoryStream())
            {
                src.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
