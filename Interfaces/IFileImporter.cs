using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Interfaces
{
    public interface IFileImporter
    {
        public void ExportToExcel(MainWindow window);
        public void ExportToExcelAsNewSheet(MainWindow window);
        public void ExportToPDF(bool isImporting);
        public void ExportSettingsOnFile(MainWindow window);
        public void ImportSettingsFromFile(MainWindow window);
        public void ExportCountriesToFTP();
        public void ImportCountriesFromFTP();
        public void ExportCalcToFile(MainWindow window);
        public void ImportCalcFromFile(MainWindow window);
    }
}
