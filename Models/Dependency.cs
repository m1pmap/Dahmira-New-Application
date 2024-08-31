using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Dependency
    {
        public string ProductName { get; set; }
        public double Multiplier { get; set; }
        public string SelectedType { get; set; } // Это будет значение, выбранное в ComboBox
    }
}
