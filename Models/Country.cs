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
        public string name { get; set; }
        public double coefficient { get; set; }
        public double discount { get; set; }
        public ObservableCollection<Manufacturer> manufacturers { get; set; }

        public Country()
        {
            manufacturers = new ObservableCollection<Manufacturer>();
        }
    }
}
