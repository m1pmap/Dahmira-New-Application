using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Interfaces
{
    public interface IFileImporter
    {
        public void ExportToExcel(MainWindow window); //Экспорт расчётки в Excel
        public void ExportToExcelAsNewSheet(MainWindow window); //Экспорт расчётки в Excel в качестве нового листа
        public void ExportToPDF(bool isImporting);  //Экспорт в PDF
        public void ExportSettingsOnFile(MainWindow window); //Экспорт настроек в файл
        public void ImportSettingsFromFile(MainWindow window); //Импорт настроек из файла
        public void ExportCountriesToFTP(); //Экспорт стран на фтп сервера
        public void ImportCountriesFromFTP(); //Импорт стран с фтп сервера 
        public void ExportCalcToFile(MainWindow window); //Экспорт расчётки в файл
        public void ImportCalcFromFile(MainWindow window); //Испорт расчётки из файла
        public void ImportDBFromFTP(MainWindow window); //Получение БД с сервера


        public void ImportCalcFromFile_StartDUH(string path, MainWindow window); //Испорт расчётки при запске dah файла
        void ExportCalcToTemlates(MainWindow window, string patch); //Экспорт расчётки в шаблон
        ObservableCollection<CalcProduct> Get_JsonList(string path, MainWindow window);//Возвращаем массив данных после десериализации из json


        //Такое себе решеиме, нужно подумать будет
        public List<string> GetFileListFromFtp();
        public async Task DownloadFileAsync(string ftpServerUrl, string localFilePath) { }
    }
}
