using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Dependency
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } //Название товара
        public double Multiplier { get; set; } //Множитель
        public string SelectedType { get; set; } // Это будет значение, выбранное в ComboBox
    }
}
