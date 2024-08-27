using Dahmira.Interfaces;
using Microsoft.Win32;
using System.Windows.Controls;
using System.IO;

namespace Dahmira.Services
{
    public class FolderPath_Services : IFolderPath
    {
        void IFolderPath.SelectedFolderPathToTextBox(TextBox textBox)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите папку",
                Filter = "Папки|*.*", // Устанавливаем фильтр для папок
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Папка" // Устанавливаем имя файла по умолчанию
            };

            // Открываем диалог и проверяем результат
            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем выбранный путь
                string selectedPath = Path.GetDirectoryName(openFileDialog.FileName);

                // Записываем путь в TextBox
                textBox.Text = selectedPath;
            }
        }
    }
}
