using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

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
        public double Count { get; set; } = double.NaN; //Количество
        public double TotalCost { get; set; } = double.NaN; //финальная цена
        public int ID { get; set; } = 0; 
        public int ID_Art { get; set; } = 0;
        public string Note { get; set; } = string.Empty; //Примечания
        public string RowColor { get; set; } = "#FFFFFF";
        public string RowForegroundColor { get; set; } = "#808080";
        public bool isInDB { get; set; } = true;

        public bool isDependency { get; set; } = false; //Есть ли зависимость у этого товара
        public ObservableCollection<Dependency> dependencies { get; set; } = new ObservableCollection<Dependency>(); //Зависимости

        public CalcProduct Clone()
        {
            return new CalcProduct { Num = this.Num,
                                     Manufacturer = this.Manufacturer,
                                     ProductName = this.ProductName,
                                     Article = this.Article,
                                     Unit = this.Unit,
                                     Photo = this.Photo,
                                     RealCost = this.RealCost,
                                     Cost = this.Cost,
                                     Count = this.Count,
                                     TotalCost = this.TotalCost,
                                     ID = this.ID,
                                     ID_Art = this.ID_Art,
                                     Note = this.Note,
                                     RowColor = this.RowColor,
                                     RowForegroundColor = this.RowForegroundColor,
                                     isDependency = this.isDependency,
                                     dependencies = this.dependencies, 
            };
        }
    }
}
