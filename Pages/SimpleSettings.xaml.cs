using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimpleSettings.xaml
    /// </summary>
    public partial class SimpleSettings : Window
    {
        public SimpleSettings()
        {
            InitializeComponent();

            //Заполнение данными CountryDataGrid
            CountryDataGrid.ItemsSource = CountryManager.Instance.countries;
        }

        private void CountryDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //Отображение поставщиков у выбранной страны
        {
            Country selectedItem = (Country)CountryDataGrid.SelectedItem;
            ManufacturerDataGrid.ItemsSource = selectedItem.manufacturers;

            List<Manufacturer> list = CountryManager.Instance.allManufacturers.Except(selectedItem.manufacturers).ToList();
            WithoutCountryManufacturersDataGrid.ItemsSource = new ObservableCollection<Manufacturer>(list);
        }

        private void showHide_button_Click(object sender, RoutedEventArgs e) //Показ и скрытие дополнительного dataGrid с производителями без страны
        {
            GridLength length = new GridLength(20, GridUnitType.Star);
            if (withoutCountryColumn.Width == length) //Если дополнительный dataGrid показан
            {
                withoutCountryColumn.Width = new GridLength(0, GridUnitType.Star); //Скрытие dataGrid
                showHide_button.RenderTransform = new RotateTransform(360); //Разворот кнопки
                showHide_button.ToolTip = "Развернуть";
            }
            else
            {
                withoutCountryColumn.Width = length; //Разворот dataGrid
                showHide_button.RenderTransform = new RotateTransform(180);
                showHide_button.ToolTip = "Скрыть";
            }
        }

        private void WithoutCountryManufacturersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Country selectedCountry = (Country)CountryDataGrid.SelectedItem;
            Manufacturer selectedWithoutCountryManufacturer = (Manufacturer)WithoutCountryManufacturersDataGrid.SelectedItem;
            //Добавление поставщика в местные поставщики выбранной страны
            selectedCountry.manufacturers.Add(selectedWithoutCountryManufacturer);
            //Удаление из поставщиков, которые не имеют страны
            var ManufacturerWithoutCountryItemSource = WithoutCountryManufacturersDataGrid.ItemsSource as ObservableCollection<Manufacturer>;
            ManufacturerWithoutCountryItemSource.Remove(selectedWithoutCountryManufacturer);
        }

        private void ManufacturerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Country selectedCountry = (Country)CountryDataGrid.SelectedItem;
            Manufacturer selectedManufacturer = (Manufacturer)ManufacturerDataGrid.SelectedItem;
            //Добавление поставщика в те, что не имеют страны
            var ManufacturerWithoutCountryItemSource = WithoutCountryManufacturersDataGrid.ItemsSource as ObservableCollection<Manufacturer>;
            ManufacturerWithoutCountryItemSource.Add(selectedManufacturer);
            //Удаление поставщика из местных поставщиков выбранной страны
            selectedCountry.manufacturers.Remove(selectedManufacturer);
        }
    }
}
