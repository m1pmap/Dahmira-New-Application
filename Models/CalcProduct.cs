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
        public int Num { get; set; } = 0; //Номер
        public string Manufacturer {  get; set; } = string.Empty; //Производитель
        public string ProductName { get; set; } = string.Empty; //Наименование товара
        public string Article { get; set; } = string.Empty; //Артикул
        public string Unit { get; set; } = string.Empty; //Единица измерения
        public byte[] Photo { get; set; } = null; //Фото
        public double RealCost { get; set; } = double.NaN; //Цена товара (реальная)
        public double Cost { get; set; } = double.NaN; //Цена товара (может изменяться в зависимости от страны)
        public int Count { get; set; } = 0; //Количество
        public double TotalCost { get; set; } = double.NaN; //финальная цена
        public int ID { get; set; } = 0; 
        public int ID_Art { get; set; } = 0;
        public string Note { get; set; } = string.Empty; //Примечания
    }
}
