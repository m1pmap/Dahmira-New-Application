using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Interfaces
{
    public interface IFileImporter
    {
        public void ImportToExcel(MainWindow window);
        public void ImportToExcelAsNewSheet(MainWindow window);
        public void ImportToPDF(bool isImporting);
    }
}
