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
        public int Num { get; set; }
        public string Manufacturer {  get; set; }
        public string ProductName { get; set; }
        public string Article { get; set; }
        public string Unit {  get; set; }
        public byte[] Photo { get; set; }
        public double RealCost { get; set; }
        public double Cost { get; set; }
        public int Count { get; set; }
        public double TotalCost { get; set; }
        public int ID { get; set; }
        public int ID_Art {  get; set; }
        public string Note {  get; set; }

    }
}
