using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class CalcProduct
    {
        public int Num { get; set; } = 0;
        public string Manufacturer {  get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public byte[] Photo { get; set; } = null;
        public double RealCost { get; set; } = double.NaN;
        public double Cost { get; set; } = double.NaN;
        public int Count { get; set; } = 0;
        public double TotalCost { get; set; } = double.NaN;
        public int ID { get; set; } = 0;
        public int ID_Art { get; set; } = 0;
        public string Note { get; set; } = string.Empty;

    }
}
