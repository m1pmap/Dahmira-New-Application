using Dahmira.Interfaces;
using Dahmira.Models;
using Dahmira.Pages;
using Dahmira.Services;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace Dahmira
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IProductImageUpdating ImageUpdater = new ProductImageUpdating_Services();//Для работы с загрузкой/удалением/сохранением картинки в файл/буфер
        ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services(); //???Для Конвертации изображения в массив байтов и обратно???

        int oldCurrentProductIndex = 0; //Прошлый выбранный элемент в dataBaseGrid

        ObservableCollection<TestData> items; //Элементы в БД
        ObservableCollection<CalcProduct> calcItems = new ObservableCollection<CalcProduct>(); //Элементы в БД

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized; // Разворачиваем окно на весь экран

            try
            {
                //Пробное наполнение dataBaseGrid 
                items = new ObservableCollection<TestData>()
                {
                    new TestData { Manufacturer = "AcoFunki", ProductName = "ПЛАСТИКОВАЯ ПЛАНКА 60X40 СМ ДЛЯ СВИНОМАТОК, 50% ОТКРЫТИЕ ПЛАСТИКОВЫЙ ПОЛ", Unit = "шт.", Article = "9102", Cost = 12.25 },
                    new TestData { Manufacturer = "Azud", ProductName = "Filter 2", Article = "DF 1", Unit = "шт.", Cost = 50.00 },
                    new TestData { Manufacturer = "Beerepoot", ProductName = "Привод 0,75кВт,консоль 2м, со вспомог. двигателем для поворотного колеса, для системы 290м, шт.", Unit = "шт.", Article = "60-84166-200", Cost = 5209.20 },
                    new TestData { Manufacturer = "Codaf", ProductName = "Лебедка электрическая с 2 катушками, крепежной пластиной и микропереключателем", Unit = "шт.", Article = "9102", Cost = 12.25 },
                    new TestData { Manufacturer = "Daltec", ProductName = "Привод STD 38/29, макс 0,75кВт", Article = "038101", Unit = "шт.", Cost = 652.24 },
                    new TestData { Manufacturer = "Ermaf", ProductName = "Комплект замены стойки GP14", Article = "N70300135", Unit = "шт.", Cost = 35.74 },
                    new TestData { Manufacturer = "Fancom", ProductName = "Птицеводческий компьютер для напольного содержания, 8 зон, шт.", Unit = "шт.", Article = "F38", Cost = 3835.95 },
                    new TestData { Manufacturer = "Gasolec", ProductName = "Источник постоянного тока 48В,  (DC), макс. 320Вт, шт. ", Article = "PLP32048", Unit = "шт.", Cost = 86.90 },
                    new TestData { Manufacturer = "HS", ProductName = "Pad colling price for KIT L=1000, H is different, NOTE!!! typical length = 3 m", Article = "Cl.FR1m", Unit = "метр", Cost = 20.00 },
                    new TestData { Manufacturer = "Jomapeks", ProductName = "Металлический бункер с дном из нержавеющей стали. 120 кг и трансмиссия без микровыключателя", Article = "010 325", Unit = "шт.", Cost = 93.00 },
                    new TestData { Manufacturer = "Jomapeks", ProductName = "Металлический бункер с дном из нержавеющей стали. 120 кг и трансмиссия без микровыключателя", Article = "010 325", Unit = "шт.", Cost = 93.00 }
                };

                dataBaseGrid.ItemsSource = items;

                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString(); //Отображение количества товаров

                //Пробное получение всех производителей
                List<string> firstColumnValues = dataBaseGrid.ItemsSource
                                                .Cast<TestData>() // Приводим к вашему типу данных
                                                .Select(item => item.Manufacturer) // Получаем значение из первого столбца (например, Manufacturer)
                                                .Distinct() // Убираем дубликаты
                                                .ToList();
                foreach (string item in firstColumnValues)
                {
                    CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = item });
                }

                Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;

                ProductName_comboBox.ItemsSource = items;
                Article_comboBox.ItemsSource = items;
                Unit_comboBox.ItemsSource = items;
                Cost_comboBox.ItemsSource = items;

                allCountries_comboBox.ItemsSource = CountryManager.Instance.countries;

                CalcDataGrid.ItemsSource = calcItems;

            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }

        private void addGrid_Button_Click(object sender, RoutedEventArgs e) //Смена функционала меню
        {
            searchGrid.Visibility = Visibility.Visible;
            addGrid.Visibility = Visibility.Hidden;
        }

        private void searchGrid_Button_Click(object sender, RoutedEventArgs e)
        {
            addGrid.Visibility = Visibility.Visible;
            searchGrid.Visibility = Visibility.Hidden;
        }

        private void productNum_textBox_TextChanged(object sender, TextChangedEventArgs e) //Динамическое увеличение и уменьшение ширины productNum_textBox
        {
            //Ширина текста
            var formattedText = new FormattedText(productNum_textBox.Text, System.Globalization.CultureInfo.CurrentCulture, productNum_textBox.FlowDirection,
                new Typeface(productNum_textBox.FontFamily, productNum_textBox.FontStyle, productNum_textBox.FontWeight, productNum_textBox.FontStretch),
                productNum_textBox.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(productNum_textBox).PixelsPerDip);

            //Ширина TextBox в зависимости от ширины текста
            productNum_textBox.Width = formattedText.Width + 4; //Небольшой отступ
        }

        private void uploadFromFile_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromFile(productImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (TestData)dataBaseGrid.Items[dataBaseGrid.SelectedIndex];
                    selectedItem.Photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    dataBaseGrid.Items.Refresh();
                }
            }
            catch { }
            //...
        }

        private void deletePhoto_button_Click(object sender, RoutedEventArgs e) //Удаление картинки
        {
            ImageUpdater.DeleteImage(productImage);
            var selectedItem = (TestData)dataBaseGrid.Items[dataBaseGrid.SelectedIndex];
            selectedItem.Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
            dataBaseGrid.Items.Refresh();
            //...
        }

        private void uploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(productImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (TestData)dataBaseGrid.Items[dataBaseGrid.SelectedIndex];
                    selectedItem.Photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    dataBaseGrid.Items.Refresh();
                }
            }
            catch { }
            //...
        }

        private void downloadToClipboard_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер
        {
            ImageUpdater.DownloadImageToClipboard(productImage);
        }

        private void downloadToFile_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл
        {
            ImageUpdater.DownloadImageToFile(productImage);
        }

        private void uploadFromFileAdd_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла (Меню добавления)
        {
            ImageUpdater.UploadImageFromFile(addedProductImage);
        }

        private void uploadFromClipboardAdd_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера обмена (Меню добавления)
        {
            ImageUpdater.UploadImageFromClipboard(addedProductImage);
        }

        private void deleteAdd_button_Click(object sender, RoutedEventArgs e) //Удаление картинки (Меню добавления)
        {
            ImageUpdater.DeleteImage(addedProductImage);
        }

        public static string GetImageHash(byte[] imageBytes)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(imageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private void dataBaseGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) //При изменении выделенной строки
        {
            //Отображение индекса выделенной строки
            productNum_textBox.Text = (dataBaseGrid.SelectedIndex + 1).ToString();

            //Отображении информации о текущем выделенном элементе
            TestData selectedItem = (TestData)dataBaseGrid.SelectedItem; //Получение текущего выделенного элемента
            if (selectedItem != null)
            {
                ManufacturerInformation_textBox.Text = selectedItem.Manufacturer;
                ProductNameInformation_textBox.Text = selectedItem.ProductName;
                ArticleInformation_textBox.Text = selectedItem.Article;
                UnitInformation_textBox.Text = selectedItem.Unit;
                CostInformation_textBox.Text = selectedItem.Cost.ToString();

                var fileImageBytes = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                if (BitConverter.ToString(fileImageBytes) == BitConverter.ToString(selectedItem.Photo)) //Если нет фотографии
                {
                    productImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                }
                else
                {
                    // Вызов метода Convert для преобразования массива байтов в BitmapImage
                    var converter = new ByteArrayToImageSourceConverter_Services();
                    productImage.Source = (BitmapImage)converter.Convert(selectedItem.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                }
            }

            //Отображение меню с поиском и информацией в случае, если она была скрыта
            if (searchGrid.Visibility == Visibility.Hidden)
            {
                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Visible;
            }

            oldCurrentProductIndex = dataBaseGrid.SelectedIndex;
        }

        private void productNum_textBox_KeyDown(object sender, KeyEventArgs e) //Переход к строке, введённой в textBox
        {
            try
            {
                if (e.Key == Key.Enter) //Если нажат Enter
                {
                    int index = Convert.ToInt32(productNum_textBox.Text) - 1;

                    if (index > dataBaseGrid.Items.Count) //Если индекс выше допустимого
                    {
                        throw new Exception("Выход за предел количества элементов");
                    }

                    dataBaseGrid.SelectedIndex = index;
                    oldCurrentProductIndex = index;

                    dataBaseGrid.Focus();
                }
            }
            catch
            {
                dataBaseGrid.SelectedIndex = oldCurrentProductIndex;
                productNum_textBox.Text = (oldCurrentProductIndex + 1).ToString();
                dataBaseGrid.Focus();
            }
        }

        private void addToPrice_button_Click(object sender, RoutedEventArgs e) //Добавление в прайс новой строки
        {
            //проверка на незаполненные поля
            if (string.IsNullOrWhiteSpace(newManufacturer_textBox.Text) ||
                string.IsNullOrWhiteSpace(newProductName_textBox.Text) ||
                string.IsNullOrWhiteSpace(newArticle_textBox.Text) ||
                string.IsNullOrWhiteSpace(newUnit_textBox.Text) ||
                string.IsNullOrWhiteSpace(newCost_textBox.Text))
            {
                MessageBox.Show("Не все поля заполнены.", "Ошибка ввода");
                return;
            }

            //Составление нового элемента прайса
            TestData newPrice = new TestData
            {
                Manufacturer = newManufacturer_textBox.Text,
                ProductName = newProductName_textBox.Text,
                Article = newArticle_textBox.Text,
                Unit = newUnit_textBox.Text,
                Cost = Convert.ToDouble(newCost_textBox.Text),
            };

            if (addedProductImage.Source.ToString() != "pack://application:,,,/resources/images/without_picture.png") //Если картинка изменилась
            {
                //Конвертация из Image в массив байтов
                ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                newPrice.Photo = converter.ConvertFromComponentImageToByteArray(addedProductImage);
            }

            //Добавление
            items.Add(newPrice);
            productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();

            //Проверка на нового производителя
            bool isNewManufacturer = !CountryManager.Instance.allManufacturers
                                     .Any(manufacturerItem => manufacturerItem.name == newPrice.Manufacturer);
            if (isNewManufacturer)
            {
                CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = newPrice.Manufacturer });
            }
            //Очистка строк и картинки от прошлого добавленного элемента
            newManufacturer_textBox.Clear();
            newProductName_textBox.Clear();
            newArticle_textBox.Clear();
            newUnit_textBox.Clear();
            newCost_textBox.Clear();
            addedProductImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/without_picture.png"));
        }

        private void deleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выделенного элемента прайса
        {
            try
            {
                TestData selectedItem = (TestData)dataBaseGrid.SelectedItem; //Получение текущего выделенного элемента
                items.Remove(selectedItem); //Удаление
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
            catch { }
        }

        private void deleteSelectedManufacturerProducts_button_Click(object sender, RoutedEventArgs e) //Удаление всех товаров выделенного производителя
        {
            try
            {
                TestData selectedItem = (TestData)dataBaseGrid.SelectedItem;
                //запрос (Если среди производителей бд есть те, что равны с выделенным)
                IEnumerable<TestData> dataForRemove = dataBaseGrid.Items.OfType<TestData>()
                                                                      .Where(item => item.Manufacturer == selectedItem.Manufacturer);

                foreach (var item in dataForRemove.Cast<TestData>().ToArray())
                {
                    items.Remove(item);
                }
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
            catch { }
        }

        private void Information_textBox_LostFocus(object sender, RoutedEventArgs e) //Обновление информации в выбранном элементе dataBaseGrid при потере фокуса на TextBox
        {
            var selectedItem = (TestData)dataBaseGrid.SelectedItem;

            selectedItem.Manufacturer = ManufacturerInformation_textBox.Text;
            selectedItem.ProductName = ProductNameInformation_textBox.Text;
            selectedItem.Article = ArticleInformation_textBox.Text;
            selectedItem.Unit = UnitInformation_textBox.Text;
            selectedItem.Cost = Convert.ToDouble(CostInformation_textBox.Text);

            dataBaseGrid.Items.Refresh();
        }

        private void simpleSettings_menuItem_Click(object sender, RoutedEventArgs e) //Открытие настроек
        {
            SimpleSettings simpleSettings = new SimpleSettings();
            simpleSettings.ShowDialog();
        }

        private void priceCalcButton_Click(object sender, RoutedEventArgs e) //Переход на прайс и расчётку
        {
            if(CalcDataGrid_Grid.Visibility == Visibility.Hidden) //Если открыт прайс
            {
                priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                CulcGrid_Grid.Visibility = Visibility.Visible;
                CalcDataGrid_Grid.Visibility = Visibility.Visible;

                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Hidden;
                DataBaseGrid_Grid.Visibility = Visibility.Hidden;
            }
            else //Если открыта расчётка
            {
                priceCalcButton.Content = "ПРАЙС->РАСЧЁТ";

                searchGrid.Visibility = Visibility.Visible;
                DataBaseGrid_Grid.Visibility = Visibility.Visible;

                CulcGrid_Grid.Visibility = Visibility.Hidden;
                CalcDataGrid_Grid.Visibility = Visibility.Hidden;
            }
        }

        private void dataBaseGrid_MouseDoubleClick(object sender, EventArgs e) //Добавление в расчётку при двойном нажатии на элемент 
        {
            TestData selectedItem = (TestData)dataBaseGrid.SelectedItem;

            CalcProduct newCalcProductItem = new CalcProduct
            {
                Num = calcItems.Count + 1,
                Manufacturer = selectedItem.Manufacturer,
                ProductName = selectedItem.ProductName,
                Article = selectedItem.Article,
                Unit = selectedItem.Unit,
                Photo = selectedItem.Photo,
                RealCost = selectedItem.Cost,
                Cost = selectedItem.Cost,
                Count = 1,
                TotalCost = selectedItem.Cost,
                ID = 0,
                ID_Art = 0,
                Note = ""
            };

            calcItems.Add(newCalcProductItem);
        }

        private void CalcdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //При смене текущего выделенного элемента расчётки
        {

            //Отображении информации о текущем выделенном элементе
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Получение текущего выделенного элемента
            if (selectedItem != null)
            {

                var fileImageBytes = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                if (BitConverter.ToString(fileImageBytes) == BitConverter.ToString(selectedItem.Photo)) //Если нет фотографии
                {
                    CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                }
                else
                {
                    // Вызов метода Convert для преобразования массива байтов в BitmapImage
                    var converter = new ByteArrayToImageSourceConverter_Services();
                    CalcProductImage.Source = (BitmapImage)converter.Convert(selectedItem.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                }
            }
        }

        private void CalcDeleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выбранного товара из расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                calcItems.Remove(selectedItem);
            }
            catch { }
        }

        private void CalcDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование ячейки
        {
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
            
            if (selectedItem != null)
            {
                selectedItem.TotalCost = selectedItem.Cost * selectedItem.Count;
                CalcDataGrid.Dispatcher.BeginInvoke(new Action(() => { CalcDataGrid.Items.Refresh(); }));
            }
        }

        private void allCountries_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCountry = (Country)allCountries_comboBox.SelectedItem;

            foreach(var item in calcItems)
            {
                item.Cost = item.RealCost * selectedCountry.coefficient;
                
                foreach(var countryManufacturer in selectedCountry.manufacturers)
                {
                    if(countryManufacturer.name == item.Manufacturer)
                    {
                        double discount = item.Cost * selectedCountry.discount;
                        item.Cost -= discount;
                    }
                }
            }

            CalcRefresh_button_Click(CalcRefresh_button, e);
        }

        private void CalcRefresh_button_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < calcItems.Count; i++)
            {
                CalcProduct item = calcItems[i];
                item.TotalCost = item.Cost * item.Count;
                if(item.Num != i + 1)
                {
                    item.Num = i + 1;
                }
            }

            CalcDataGrid.Items.Refresh();
        }

        private void CalcUploadFromFile_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла в элемент расчётки
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromFile(CalcProductImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    //Изменение картинки в dataBaseGrid
                    int index = CalcDataGrid.SelectedIndex;
                    calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                    CalcDataGrid.Items.Refresh();
                }
            }
            catch { }
        }

        private void CalcDeleteImage_Click(object sender, RoutedEventArgs e) //Удаление картинки элемента расчётки
        {
            try
            {
                ImageUpdater.DeleteImage(CalcProductImage);
                int index = CalcDataGrid.SelectedIndex;
                calcItems[index].Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                CalcDataGrid.Items.Refresh();
            }
            catch { }
        }

        private void CalcDownloadToFile_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл из элемента расчётки
        {
            ImageUpdater.DownloadImageToFile(CalcProductImage);
        }

        private void CalcUploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера в элемент расчётки
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(CalcProductImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    //Изменение картинки в dataBaseGrid
                    int index = CalcDataGrid.SelectedIndex;
                    calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                    CalcDataGrid.Items.Refresh();
                }
            }
            catch { }
        }

        private void CalcDownloadToClipboard_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер из элемента расчётки
        {
            ImageUpdater.DownloadImageToClipboard(CalcProductImage);
        }

        private void AddToCalc_button_Click(object sender, RoutedEventArgs e) //Добавление выделенного элемента DAtaBaseGrid в расчётку
        {
            dataBaseGrid_MouseDoubleClick(CalcDataGrid, e);
        }
    }

    public class TestData
    {
        public string Manufacturer { get; set; }
        public string ProductName { get; set; }
        public string Article { get; set; }
        public string Unit { get; set; }
        public byte[] Photo { get; set; }
        public double Cost { get; set; }

        public TestData()
        {
            if (Photo == null) //Если картинка не указана
            {
                ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
                Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
            }
        }
    }
}
