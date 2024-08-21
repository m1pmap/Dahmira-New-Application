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
        public ObservableCollection<ColorItem> colors { get; set; }
        public SimpleSettings()
        {
            InitializeComponent();

            //Заполнение данными CountryDataGrid
            CountryDataGrid.ItemsSource = CountryManager.Instance.countries;
            colors = new ObservableCollection<ColorItem>
            {
                new ColorItem { Name = "Красный", Color = Colors.Red },
                new ColorItem { Name = "Зелёный", Color = Colors.Green },
                new ColorItem { Name = "Синий", Color = Colors.Blue },
                new ColorItem { Name = "Жёлтый", Color = Colors.Yellow },
                new ColorItem { Name = "Прозрачный", Color = Colors.Transparent }
            };
            //Цвета для comboBox, отвечающие за настройку Excel
            ExcelTitleColors_comboBox.ItemsSource = colors;
            ExcelChapterColors_comboBox.ItemsSource = colors;
            ExcelDataColors_comboBox.ItemsSource = colors;
            ExcelPhotoBackgroundColors_comboBox.ItemsSource = colors;
            ExcelNotesColors_comboBox.ItemsSource = colors;
            ExcelNumberColors_comboBox.ItemsSource = colors;

            //Цвета для comboBox, отвечающие за настройку pdf
            pdfHeaderColors_comboBox.ItemsSource = colors;
            pdfChapterColors_comboBox.ItemsSource = colors;
            pdfResultsColors_comboBox.ItemsSource = colors;
        }

        private void CountryDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //Отображение поставщиков у выбранной страны
        {
            if(CountryDataGrid.SelectedIndex != CountryDataGrid.Items.Count - 1) 
            {
                Country selectedItem = (Country)CountryDataGrid.SelectedItem;
                if(selectedItem != null) 
                {
                    ManufacturerDataGrid.ItemsSource = selectedItem.manufacturers;

                    List<Manufacturer> list = CountryManager.Instance.allManufacturers.Except(selectedItem.manufacturers).ToList();
                    WithoutCountryManufacturersDataGrid.ItemsSource = new ObservableCollection<Manufacturer>(list);
                }
            }
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

        private void CountryDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование текущей ячейки
        {
            Country selectedItem = (Country)CountryDataGrid.SelectedItem;

            if (selectedItem != null)
            {
                if (selectedItem.discount > 1)
                {
                    selectedItem.discount = 1;
                }
                if (selectedItem.discount < 0)
                {
                    selectedItem.discount = 0;
                }

                // Обновляем DataGrid после завершения текущего цикла событий
                CountryDataGrid.Dispatcher.BeginInvoke(new Action(() =>{CountryDataGrid.Items.Refresh();}));
            }
        }
    }
}
