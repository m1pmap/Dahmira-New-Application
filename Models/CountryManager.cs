using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    internal class CountryManager
    {
        private static CountryManager instance;
        public ObservableCollection<Manufacturer> allManufacturers { get; set; }

        public PriceManager priceManager = new PriceManager();
        private CountryManager()
        {
            allManufacturers = new ObservableCollection<Manufacturer> { };
            priceManager.countries = new ObservableCollection<Country> {  };
        }

        public static CountryManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CountryManager();
                }
                return instance;
            }
        }
    }
}
