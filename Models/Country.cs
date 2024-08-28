using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Country
    {
        public string name { get; set; } //Название страны
        public double coefficient { get; set; } //Коэфф
        public double discount { get; set; } //Скидка
        public ObservableCollection<Manufacturer> manufacturers { get; set; } //Местные поставщики

        public Country()
        {
            manufacturers = new ObservableCollection<Manufacturer>();
        }
    }
}
