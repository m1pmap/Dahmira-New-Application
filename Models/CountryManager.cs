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
        public ObservableCollection<Country> countries { get; set; }

        private CountryManager()
        {
            allManufacturers = new ObservableCollection<Manufacturer> { };
            countries = new ObservableCollection<Country>
            {
                new Country { name = "1x1", coefficient = 1, discount = 1, manufacturers = new ObservableCollection<Manufacturer> { new Manufacturer { name = "AcoFunki" }, new Manufacturer { name = "Azud" } } },
                new Country { name = "Армения", coefficient = 2, discount = 0.1M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Beerepoot" }, new Manufacturer { name = "Codaf" } } },
                new Country { name = "Россия", coefficient = 2, discount = 0.2M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "Беларусь", coefficient = 1, discount = 0.5M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "Украина", coefficient = 2, discount = 1.1M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "2x1", coefficient = 2, discount = 1, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "Польша", coefficient = 2, discount = 0.3M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "Казахстан", coefficient = 2, discount = 1, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
                new Country { name = "EXW Wroclaw Poland", coefficient = 1.4M, discount = 1.2M, manufacturers = new ObservableCollection<Manufacturer> {new Manufacturer { name = "Daltec" } } },
            };
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
