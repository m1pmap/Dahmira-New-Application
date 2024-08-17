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
        public decimal coefficient { get; set; }
        public decimal discount { get; set; }
        public ObservableCollection<Manufacturer> manufacturers { get; set; }
    }
}
