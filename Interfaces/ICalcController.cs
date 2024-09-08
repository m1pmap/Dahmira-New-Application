using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dahmira.Interfaces
{
    public interface ICalcController
    {
        void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems, Label fullCost_label); //Обновление расчётки
        bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, MainWindow window, Label fullCost_label, double count = 1, string position = "Last"); //Добавление в расчётку товара
        void ObjectFlashing(Button target, Color initialColor, Color flashingColor); //Анимация мигания выбранной кнопки и выбранными цветами
        string ColorToHex(Color color); //Конвертация цвета в hex
        void UpdateCellStyle(DataGrid dataGrid, Brush backgroundColor, Brush foregroundColor); //Изменение стиля для DataGrid
        bool ArePhotosEqual(byte[] photo1, byte[] photo2);
        bool CheckingDifferencesWithDB(DataGrid CalcDataGrid, MainWindow window);
    }
}
