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
        void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems, Label fullCost_label);
        bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, Label fullCost_label, ObservableCollection<CalcProduct> calcItems, int count = 1, string position = "Last");
        void ObjectFlashing(Button target, Color initialColor, Color flashingColor);
    }
}
