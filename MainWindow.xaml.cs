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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System.Text;

namespace Dahmira
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IProductImageUpdating ImageUpdater = new ProductImageUpdating_Services();//Для работы с загрузкой/удалением/сохранением картинки в файл/буфер
        private ICalcController CalcController = new CalcController_Services();//Для работы с обновлением/добавлением в расчётку
        private ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services(); //???Для Конвертации изображения в массив байтов и обратно???
        public SettingsParameters settings = new SettingsParameters();
        private int ExcelFileCount = 1;

        int oldCurrentProductIndex = 0; //Прошлый выбранный элемент в dataBaseGrid

        private ObservableCollection<TestData> items; //Элементы в БД
        private ObservableCollection<CalcProduct> calcItems = new ObservableCollection<CalcProduct>(); //Элементы в БД

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
                    new TestData { Manufacturer = "Ermaf", ProductName = "Комплект замены стойки GP14", Article = "N70300135", Unit = "шт.", Cost = 35.54 },
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

                //Добавление ItemSource компонентам
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
            try
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
            catch
            {
                MessageBox.Show("Формат введённых данных неверен");
            }
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
            SimpleSettings simpleSettings = new SimpleSettings(settings, this);
            simpleSettings.ShowDialog();
            fullCostType.Content = settings.TotalCostValue;
        }

        private void priceCalcButton_Click(object sender, RoutedEventArgs e) //Переход на прайс и расчётку
        {
            if (CalcDataGrid_Grid.Visibility == Visibility.Hidden) //Если открыта расчётка
            {
                priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                CulcGrid_Grid.Visibility = Visibility.Visible;
                CalcDataGrid_Grid.Visibility = Visibility.Visible;

                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Hidden;
                DataBaseGrid_Grid.Visibility = Visibility.Hidden;

                TotalCostRow_row.Height = new GridLength(39, GridUnitType.Pixel);
                dataBaseBorder_border.CornerRadius = new CornerRadius(15, 15, 0, 0);
            }
            else //Если открыта прайс
            {
                priceCalcButton.Content = "ПРАЙС->РАСЧЁТ";

                searchGrid.Visibility = Visibility.Visible;
                DataBaseGrid_Grid.Visibility = Visibility.Visible;

                CulcGrid_Grid.Visibility = Visibility.Hidden;
                CalcDataGrid_Grid.Visibility = Visibility.Hidden;

                TotalCostRow_row.Height = new GridLength(0, GridUnitType.Pixel);
                dataBaseBorder_border.CornerRadius = new CornerRadius(15, 15, 15, 15);
            }
        }

        private void dataBaseGrid_MouseDoubleClick(object sender, EventArgs e) //Добавление в расчётку при двойном нажатии на элемент 
        {
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, calcItems, fullCost);
            if (!isAddedWell)
            {
                MessageBox.Show("Для начала добавьте раздел!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                CalcController.ObjectFlashing(priceCalcButton, Colors.LightGray, Colors.White);
            }
        }

        private void CalcdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //При смене текущего выделенного элемента расчётки
        {
            try
            {
                //Отображении информации о текущем выделенном элементе
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Получение текущего выделенного элемента
                if (selectedItem != null)
                {
                    if (selectedItem.ProductName == null) //Если нажат раздел
                    {
                        CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative)); //Обнуление картинки, так как у раздела не может быть картинки
                    }
                    else
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
            }
            catch { }
        }

        private void CalcDeleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выбранного товара из расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                calcItems.Remove(selectedItem);

                CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
            }
            catch { }
        }

        private void CalcDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование ячейки
        {
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;

            if (selectedItem != null)
            {
                selectedItem.TotalCost = selectedItem.Cost * selectedItem.Count;
                CalcDataGrid.Dispatcher.BeginInvoke(new Action(() => { CalcController.Refresh(CalcDataGrid, calcItems, fullCost); }));
            }
        }

        private void allCountries_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Изменение ценников в зависимости от местных поставщиков выбранной страны
        {
            var selectedCountry = (Country)allCountries_comboBox.SelectedItem;

            foreach (var item in calcItems) //Перебор всех элементов
            {
                if (item.ProductName != null) //Если не раздел
                {
                    item.Cost = Math.Round(item.RealCost * selectedCountry.coefficient, 2); //Коэф страны * цену

                    foreach (var countryManufacturer in selectedCountry.manufacturers)
                    {
                        if (countryManufacturer.name == item.Manufacturer) //Если это местный поставщик выбранной страны
                        {
                            double discount = item.Cost * selectedCountry.discount; //Скидка
                            item.Cost -= discount; //Цена со скидкой
                        }
                    }
                }
            }

            CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
        }

        private void CalcRefresh_button_Click(object sender, RoutedEventArgs e) //Обновление расчётки
        {
            CalcController.Refresh(CalcDataGrid, calcItems, fullCost); //Обновление
        }

        private void CalcUploadFromFile_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла в элемент расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    bool imageIsEdit = ImageUpdater.UploadImageFromFile(CalcProductImage); //Загрузка картинки
                    //Если сейчас выбран раздел
                    if (selectedItem.ProductName == null)
                    {
                        return;
                    }

                    if (imageIsEdit) //Если картинку загрузили
                    {
                        //Изменение картинки в dataBaseGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                    }
                }
            }
            catch { }
        }

        private void CalcDeleteImage_Click(object sender, RoutedEventArgs e) //Удаление картинки элемента расчётки
        {
            try
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    ImageUpdater.DeleteImage(CalcProductImage);

                    //Если сейчас выбран раздел
                    if (selectedItem.ProductName == null)
                    {
                        return;
                    }

                    int index = CalcDataGrid.SelectedIndex;
                    calcItems[index].Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                    CalcDataGrid.Items.Refresh();
                }
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
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(CalcProductImage); //Загрузка картинки

                    //Если сейчас выбран раздел
                    if (selectedItem.ProductName == null)
                    {
                        return;
                    }

                    if (imageIsEdit) //Если картинку загрузили
                    {
                        //Изменение картинки в dataBaseGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                    }
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
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text)) //Если количество не указано
            {
                MessageBox.Show("Количество не указано!", "Ошибка");
                return;
            }
            int count = Convert.ToInt32(CountProductToAdd_textBox.Text); //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, calcItems, fullCost, count); //Добавление
            if (!isAddedWell)
            {
                MessageBox.Show("Для начала добавьте Раздел!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                CalcController.ObjectFlashing(priceCalcButton, Colors.LightGray, Colors.White);
            }
        }

        private void AddToCalcUnderSelectedRow_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text)) //Если количество не указано
            {
                MessageBox.Show("Количество не указано!", "Ошибка");
                return;
            }
            int count = Convert.ToInt32(CountProductToAdd_textBox.Text); //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, calcItems, fullCost, count, "UnderSelect"); //Добавление
            if (!isAddedWell)
            {
                MessageBox.Show("Для начала добавьте Раздел!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                CalcController.ObjectFlashing(priceCalcButton, Colors.LightGray, Colors.White);
            }
        }

        private void ReplaceCalc_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text)) //Если количество не указано
            {
                MessageBox.Show("Количество не указано!", "Ошибка");
                return;
            }
            int count = Convert.ToInt32(CountProductToAdd_textBox.Text); //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, calcItems, fullCost, count, "Replace"); //Замена
            if (!isAddedWell)
            {
                MessageBox.Show("Для начала добавьте Раздел!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                CalcController.ObjectFlashing(priceCalcButton, Colors.LightGray, Colors.White);
            }
        }

        private void CalcChapter_button_Click(object sender, RoutedEventArgs e) //Создание раздела
        {
            int selectedIndex = CalcDataGrid.SelectedIndex; //Индекс текущего выделенного элемента
            string chapterName = chapterName_textBox.Text;

            //Создание раздела
            CalcProduct chapter = new CalcProduct
            {
                Manufacturer = chapterName,
                Cost = double.NaN,
                TotalCost = double.NaN
            };

            //Добавление
            calcItems.Insert(selectedIndex + 1, chapter);
        }

        private void CalcToExcel_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Лист1");
                int lastColumnIndex = 10;

                if (settings.IsInsertExcelPicture == false) //Если фото не добавляется, то количество столбцов меньше
                {
                    lastColumnIndex = 9;
                }

                // Записываем заголовки столбцов
                for (int i = 0; i < lastColumnIndex; i++)
                {
                    if (i >= 5 && settings.IsInsertExcelPicture == false) //Если картинка не добавляется
                    {
                        worksheet.Cells[1, i + 1].Value = CalcDataGrid.Columns[i + 1].Header;
                    }
                    else
                    {
                        worksheet.Cells[1, i + 1].Value = CalcDataGrid.Columns[i].Header;
                    }
                    
                }
                worksheet.Cells[1, lastColumnIndex].Value = "Примечания";

                //Установка стилей для Header 
                ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelTitleColor.Color);

                //Установка стилей для всего рабочего пространства
                ExcelRange Rng = worksheet.Cells[1, 1, calcItems.Count + 2, lastColumnIndex];
                Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                Rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                Rng.Style.WrapText = true;

                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                Rng.Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                Rng.Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                //Установка ширины столбцов 
                worksheet.Column(1).Width = 4.29;
                worksheet.Column(2).Width = 27.14;
                worksheet.Column(3).Width = 50.86;
                worksheet.Column(4).Width = 22.14;
                worksheet.Column(5).Width = 15.43;
                worksheet.Column(lastColumnIndex - 3).Width = 10.14;
                worksheet.Column(lastColumnIndex - 2).Width = 12.14;
                worksheet.Column(lastColumnIndex - 1).Width = 24.14;
                worksheet.Column(lastColumnIndex).Width = 18.43;

                // Записываем данные из DataGrid
                for (int i = 0; i < calcItems.Count; i++)
                {
                    CalcProduct item = calcItems[i];

                    if (item.Photo == null) //Если фото равно нулю (Раздел)
                    {
                        worksheet.Cells[i + 2, 2].Value = item.Manufacturer; //Имя раздела
                        //Стили для раздела
                        ExcelRange chapterRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                        chapterRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        chapterRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelChapterColor.Color);
                        continue;
                    }

                    //Установка стилей всех данных
                    ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                    dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dataRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelDataColor.Color);

                    //Установка стилей для примечаний
                    ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                    notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    notesRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelNotesColor.Color);

                    //Установка стилей для номера
                    ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                    numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    numberRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelNumberColor.Color);

                    //Добавление данных в ячейки
                    worksheet.Cells[i + 2, 1].Value = item.Num;
                    worksheet.Cells[i + 2, 2].Value = item.Manufacturer;
                    worksheet.Cells[i + 2, 3].Value = item.ProductName;
                    worksheet.Cells[i + 2, 4].Value = item.Article;
                    worksheet.Cells[i + 2, 5].Value = item.Unit;

                    if(settings.IsInsertExcelPicture == true) //Если картинку надо добавить
                    {
                        //Установка стилей для фона фото 
                        ExcelRange photoRange = worksheet.Cells[i + 2, 6];
                        photoRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        photoRange.Style.Fill.BackgroundColor.SetColor(settings.ExcelPhotoBackgroundColor.Color);

                        BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);

                        //Ширина и высота в зависимости от выбранных параметров в настройках
                        worksheet.Column(6).Width = (settings.MaxExcelPhotoWidth + 10) / 7;
                        worksheet.Rows[i + 2].Height = (settings.MaxExcelPhotoHeight + 10) / 1.33;

                        //MemoryStream для создания временного файла для дальнейшей конвертации в FileInfo
                        using (var memoryStream = new MemoryStream())
                        {
                            //Кодек для сохранения изображения
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                            encoder.Save(memoryStream);

                            //Сброс позиции потока в начало
                            memoryStream.Position = 0;

                            //Добавление изображения в Excel
                            var excelImage = worksheet.Drawings.AddPicture(i.ToString(), memoryStream);
                            excelImage.SetPosition(i + 1, 3, 5, 3);
                            excelImage.SetSize(settings.MaxExcelPhotoWidth, settings.MaxExcelPhotoHeight);

                            
                        }
                    }

                    //Добавление остальных данных в ячейки
                    worksheet.Cells[i + 2, lastColumnIndex - 3].Value = item.Cost;
                    worksheet.Cells[i + 2, lastColumnIndex - 2].Value = item.Count;
                    worksheet.Cells[i + 2, lastColumnIndex - 1].Value = item.TotalCost;
                    worksheet.Cells[i + 2, lastColumnIndex].Value = item.Note;
                }

                worksheet.Cells[calcItems.Count + 2, lastColumnIndex - 1].Value = settings.TotalCostValue + " " + fullCost.Content;

                //Сохранение в Excel файл по указанному в Настройках пути
                DialogPage dialog = new DialogPage("excel");
                dialog.ShowDialog();
                if (dialog.Result != string.Empty)
                {
                    worksheet.Protection.IsProtected = false;
                    worksheet.Protection.AllowSelectLockedCells = false;
                    package.SaveAs(new FileInfo(settings.ExcelFolderPath + dialog.Result + ".xlsx"));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
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
