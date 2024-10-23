using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Services;
using OfficeOpenXml.Drawing.Style.Coloring;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dahmira.DAL;
using Microsoft.Win32;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для SimpleSettings.xaml
    /// </summary>
    public partial class SimpleSettings : Window
    {
        private ObservableCollection<ColorItem> colors { get; set; }

        private IFolderPath pathFolderController = new FolderPath_Services();
        private SettingsParameters settings;
        private MainWindow mainWindow;
        public SimpleSettings(SettingsParameters s, MainWindow window)
        {
            InitializeComponent();

            mainWindow = window;
            //Заполнение данными CountryDataGrid
            CountryDataGrid.ItemsSource = CountryManager.Instance.priceManager.countries;    
            colors = new ObservableCollection<ColorItem>
            {
                new ColorItem("Красный", System.Drawing.Color.Red),
                new ColorItem ("Зелёный", System.Drawing.Color.MediumSeaGreen),
                new ColorItem ("Синий", System.Drawing.Color.Blue),
                new ColorItem ("Жёлтый", System.Drawing.Color.LightYellow),
                new ColorItem ("Прозрачный", System.Drawing.Color.Transparent)
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

            settings = s; //Получение настроек

            lastDataUpdate.Content = "ПОСЛЕДНЕЕ ОБНОВЛЕНИЕ: " + CountryManager.Instance.priceManager.lastUpdated.ToString();

            //Установка значений настроек
            //Общие настройки
            theme_comboBox.SelectedIndex = settings.Theme;
            IsNotificationsWithSound_checkBox.IsChecked = settings.IsNotificationsWithSound;
            CheckingIntervalFromMail_textBox.Text = settings.CheckingIntervalFromMail.ToString();
            PriceFolderPath_textBox.Text = settings.PriceFolderPath;

            if (settings.IsAdministrator)
            {
                AdministratingStatus_label.Content = "вы Администратор";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                hideShowButtonColumn.Width = new GridLength(30, GridUnitType.Pixel);
            }
            else
            {
                AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
            }

            //Вывод данных в Excel
            ComboBoxItemCompare(ExcelTitleColors_comboBox, settings.ExcelTitleColor);
            ComboBoxItemCompare(ExcelChapterColors_comboBox, settings.ExcelChapterColor);
            ComboBoxItemCompare(ExcelDataColors_comboBox, settings.ExcelDataColor);
            ComboBoxItemCompare(ExcelPhotoBackgroundColors_comboBox, settings.ExcelPhotoBackgroundColor);
            ComboBoxItemCompare(ExcelNotesColors_comboBox, settings.ExcelNotesColor);
            ComboBoxItemCompare(ExcelNumberColors_comboBox, settings.ExcelNumberColor);
            IsInsertExcelPicture_textBox.IsChecked = settings.IsInsertExcelPicture;
            ExcelImageWidth.Text = settings.ExcelPhotoWidth.ToString();
            ExcelImageHeight.Text = settings.ExcelPhotoHeight.ToString();
            FullCostType_comboBox.SelectedItem = FullCostType_comboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == settings.FullCostType);

            //Вывод данных в PDF
            ComboBoxItemCompare(pdfHeaderColors_comboBox, settings.PdfHeaderColor);
            ComboBoxItemCompare(pdfChapterColors_comboBox, settings.PdfChapterColor);
            ComboBoxItemCompare(pdfResultsColors_comboBox, settings.PdfResultsColor);

            //Пути сохранения
            ExcelImportFolderPath_textBox.Text = settings.ExcelFolderPath;
            PDFImportFolderPath_textBox.Text = settings.PDFFolderPath;
            CalcImportFolderPath_textBox.Text = settings.CalcFolderPath;
        }

        private void ComboBoxItemCompare(ComboBox comboBox, ColorItem item)
        {
            comboBox.SelectedItem = comboBox.Items
                .Cast<ColorItem>()
                .FirstOrDefault(i => i.Name == item.Name);
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
            try
            {
                Country selectedCountry = (Country)CountryDataGrid.SelectedItem;
                Manufacturer selectedWithoutCountryManufacturer = (Manufacturer)WithoutCountryManufacturersDataGrid.SelectedItem;
                //Добавление поставщика в местные поставщики выбранной страны
                selectedCountry.manufacturers.Add(selectedWithoutCountryManufacturer);
                //Удаление из поставщиков, которые не имеют страны
                var ManufacturerWithoutCountryItemSource = WithoutCountryManufacturersDataGrid.ItemsSource as ObservableCollection<Manufacturer>;
                ManufacturerWithoutCountryItemSource.Remove(selectedWithoutCountryManufacturer);
            }
            catch { }
        }

        private void ManufacturerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(settings.IsAdministrator)
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
            catch { }
        }

        private void CountryDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование текущей ячейки
        {
            try
            {
                Country selectedItem = (Country)CountryDataGrid.SelectedItem;

                if (selectedItem != null)
                {
                    if (selectedItem.discount > 100)
                    {
                        selectedItem.discount = 100;
                    }
                    if (selectedItem.discount < 0)
                    {
                        selectedItem.discount = 0;
                    }
                    if (selectedItem.coefficient == 1)
                    {
                        selectedItem.discount = 0;
                    }

                    //Обновляем DataGrid после завершения текущего цикла событий
                    CountryDataGrid.CommitEdit();
                    CountryDataGrid.Dispatcher.BeginInvoke(new Action(() => { CountryDataGrid.Items.Refresh(); }));
                }
            }
            catch { }
        }

        private void AdministratorPasswordCheck_button_Click(object sender, RoutedEventArgs e) //Проверка введённого пароля администратора
        {
            if (AdministratorPassword_passwordBox.Password == "Administrator2024") //Если пароль верный
            {
                AdministratingStatus_label.Content = "вы Администратор";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                hideShowButtonColumn.Width = new GridLength(30, GridUnitType.Pixel);
                withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                settings.IsAdministrator = true;
                CountryDataGrid.IsReadOnly = false;
                MyTimer timer = new MyTimer(1800, TimerAction); // Создаем таймер на 10 секунд
                timer.Start(); // Запускаем таймер
            }
            else
            {
                MessageBox.Show("Пароль введён неверно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                withoutCountryColumn.Width = new GridLength(0, GridUnitType.Pixel);
                settings.IsAdministrator = false;
                CountryDataGrid.IsReadOnly = true;
            }
        }

        private void TimerAction()
        {
            this.Dispatcher.Invoke(() =>
            {
                AdministratingStatus_label.Content = "вы не обладаете правами Администратора";
                AdministratingStatus_label.Foreground = new SolidColorBrush(Colors.OrangeRed);
                hideShowButtonColumn.Width = new GridLength(0, GridUnitType.Star);
                settings.IsAdministrator = false;
                CountryDataGrid.IsReadOnly = true;
            });
        }

        private void AddPriceFolderPath_button_Click(object sender, RoutedEventArgs e)  //Добавление пути к прайсу
        {
            pathFolderController.SelectedFolderPathToTextBox(PriceFolderPath_textBox);
        }

        private void DeletePriceFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к прайсу
        {
            PriceFolderPath_textBox.Text = "D:\\";
        }

        private void ExcelImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Добавление пути к Excel
        {
            pathFolderController.SelectedFolderPathToTextBox(ExcelImportFolderPath_textBox);
        }

        private void DeleteExcelImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к Excel
        {
            ExcelImportFolderPath_textBox.Text = "D:\\";
        }

        private void PDFImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Добавление пути к PDF
        {
            pathFolderController.SelectedFolderPathToTextBox(PDFImportFolderPath_textBox);
        }

        private void DeletePDFImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к PDF
        {
            PDFImportFolderPath_textBox.Text = "D:\\";
        }

        private void CalcImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Добавление пути к расчётке
        {
            pathFolderController.SelectedFolderPathToTextBox(CalcImportFolderPath_textBox);
        }

        private void DeleteCalcImportFolderPath_button_Click(object sender, RoutedEventArgs e) //Удаление пути к расчётке
        {
            CalcImportFolderPath_textBox.Text = "D:\\";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Общие настройки
            settings.Theme = theme_comboBox.SelectedIndex;
            settings.IsNotificationsWithSound = IsNotificationsWithSound_checkBox.IsChecked;
            settings.CheckingIntervalFromMail = Convert.ToDouble(CheckingIntervalFromMail_textBox.Text);
            settings.PriceFolderPath = PriceFolderPath_textBox.Text;

            //Вывод данных в Excel
            settings.ExcelTitleColor = (ColorItem)ExcelTitleColors_comboBox.SelectedItem;
            settings.ExcelChapterColor = (ColorItem)ExcelChapterColors_comboBox.SelectedItem;
            settings.ExcelDataColor = (ColorItem)ExcelDataColors_comboBox.SelectedItem;
            settings.ExcelNotesColor = (ColorItem)ExcelNotesColors_comboBox.SelectedItem;
            settings.ExcelPhotoBackgroundColor = (ColorItem)ExcelPhotoBackgroundColors_comboBox.SelectedItem;
            settings.ExcelNumberColor = (ColorItem)ExcelNumberColors_comboBox.SelectedItem;
            settings.IsInsertExcelPicture = IsInsertExcelPicture_textBox.IsChecked;
            settings.ExcelPhotoWidth = Convert.ToInt32(ExcelImageWidth.Text);
            settings.ExcelPhotoHeight = Convert.ToInt32(ExcelImageHeight.Text);
            settings.FullCostType = FullCostType_comboBox.Text;

            //Вывод данных в PDF
            settings.PdfHeaderColor = (ColorItem)pdfHeaderColors_comboBox.SelectedItem;
            settings.PdfChapterColor = (ColorItem)pdfChapterColors_comboBox.SelectedItem;
            settings.PdfResultsColor = (ColorItem)pdfResultsColors_comboBox.SelectedItem;

            //Пути сохранения
            settings.ExcelFolderPath = ExcelImportFolderPath_textBox.Text;
            settings.PDFFolderPath = PDFImportFolderPath_textBox.Text;
            settings.CalcFolderPath = CalcImportFolderPath_textBox.Text;

            mainWindow.settings = settings;
        }

        private void SaveCountries_button_Click(object sender, RoutedEventArgs e)
        {
            CountryManager.Instance.priceManager.lastUpdated = DateTime.Now;
            IFileImporter fileImporter = new FileImporter_Services();
            fileImporter.ExportCountriesToFTP();
            lastDataUpdate.Content = "ПОСЛЕДНЕЕ ОБНОВЛЕНИЕ: " + CountryManager.Instance.priceManager.lastUpdated.ToString();
        }

        private void CountryDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Коэффициент" || e.Column.Header.ToString() == "Скидка, %")
            {
                string newTex = ((TextBox)e.EditingElement).Text;


                if (string.IsNullOrWhiteSpace(newTex)) //Если текст пустой
                {
                    ((TextBox)e.EditingElement).Text = "0";
                }
                else
                {
                    bool oneSymbol = false; //Флаг для отслеживания наличия точки
                    string validNumber = ""; //Строка для хранения валидного числа

                    for (int i = 0; i < newTex.Length; i++)
                    {
                        char currentChar = newTex[i];

                        if (char.IsDigit(currentChar)) //Если символ цифра
                        {
                            validNumber += currentChar; //Добавление символа
                        }
                        else if (currentChar == '.' && !oneSymbol) //Если символ точка и её ещё не было
                        {
                            validNumber += currentChar; //Добавление точки
                            oneSymbol = true;
                        }
                    }

                    // Если число пустое или состоит только из точки
                    if (string.IsNullOrEmpty(validNumber) || validNumber == ".")
                    {
                        ((TextBox)e.EditingElement).Text = "0";
                    }
                    else
                    {
                        ((TextBox)e.EditingElement).Text = validNumber;
                    }
                }
            }
        }

        private void HandleDeleteAction()
        {
            Country selectedItem = (Country)CountryDataGrid.SelectedItem;
            if (selectedItem != null) 
            {
                CountryManager.Instance.priceManager.countries.Remove(selectedItem);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                //Проверяем, что строка не последняя
                if (CountryDataGrid.SelectedIndex < (CountryDataGrid.Items.Count - 1))
                {
                    if (MyTabControl.SelectedItem is TabItem selectedTab)
                    {
                        if (selectedTab.Header.ToString() == "Менеджер цен")
                        {
                            if (!settings.IsAdministrator)
                            {
                                e.Handled = true;
                            }
                            else
                            {
                                HandleDeleteAction();
                            }
                        }
                    }
                }
            }
        }
    }
}
