using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dahmira.Interfaces
{
    public interface ICalcController
    {
        void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems); //Обновление расчётки
        bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, MainWindow window, string count = "1", string position = "Last"); //Добавление в расчётку товара
        void ObjectFlashing(Border target, Color initialColor, Color flashingColor, double interval); //Анимация мигания выбранной кнопки и выбранными цветами
        string ColorToHex(Color color); //Конвертация цвета в hex
        Color HexToColor(string hex);
        void UpdateCellStyle(DataGrid dataGrid, Brush backgroundColor, Brush foregroundColor); //Изменение стиля для DataGrid
        bool ArePhotosEqual(byte[] photo1, byte[] photo2);
        bool CheckingDifferencesWithDB(DataGrid CalcDataGrid, MainWindow window);
        void Calculation(MainWindow window);
        void ClearBackgroundsColors(MainWindow window);
    }
}
