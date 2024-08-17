using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dahmira.Services
{
    public class ByteArrayToImageSourceConverter_Services : IValueConverter
    {
        //Конвертация массива байтов в картинку
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    return image;
                }
            }
            return null;
        }

        //Конвертация Картинки как компонента в массив байтов
        public byte[] ConvertFromComponentImageToByteArray(Image image)
        {
            byte[] imageBytes;
            BitmapSource bitmapSource = (BitmapSource)image.Source;

            using (var memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            return imageBytes;
        }

        //Конвертация картинки из файла в массив байтов
        public byte[] ConvertFromFileImageToByteArray(string fileName)
        {
            string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "resources", "images", fileName);
            return File.ReadAllBytes(projectPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
