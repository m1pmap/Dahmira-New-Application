using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dahmira.Interfaces
{
    internal interface IProductImageUpdating
    {
        bool UploadImageFromFile(Image image); //Загрузка картинки из файла
        void DownloadImageToFile(Image image); //Сохранение картинки в файл
        void DeleteImage(Image image); //Удаление картинки

        bool UploadImageFromClipboard(Image image); //Загрузка картинки из буфера обмена
        void DownloadImageToClipboard(Image image); //Сохранение картинки в буфер обмена
    }
}
