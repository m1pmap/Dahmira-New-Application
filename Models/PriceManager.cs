using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class PriceManager
    {
        public ObservableCollection<Country> countries { get; set; }
        public DateTime lastUpdated { get; set; }
    }
}
