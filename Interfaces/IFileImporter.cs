using System;
using System.Collections.Generic;
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
    }
}
