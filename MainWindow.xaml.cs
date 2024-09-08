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
using System.Text.Json;
using Dahmira.DAL;
using Dahmira.DAL.Repository;
using Dahmira.DAL.Model;
using System.Windows.Media.Media3D;
using Material = Dahmira.DAL.Model.Material;
using System.Runtime.Intrinsics.Arm;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Text.Json.Serialization;

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
        private IFileImporter fileImporter = new FileImporter_Services(); //Для импорта изображений
        public SettingsParameters settings = new SettingsParameters(); //Настройки приложения
        public bool isCalcSaved = true; //Указывает на то сохранена ли сейчас расчётка
        public ObservableCollection<Dependency> dependencies = new ObservableCollection<Dependency>(); //Зависимости для товара

        Material_Repository repository = new Material_Repository(); //Для работы с БД
        public List<string> ComboBoxValues { get; set; } = new List<string> { "*", "+", "-", "/" };

        int oldCurrentProductIndex = 0; //Прошлый выбранный элемент в dataBaseGrid

        public ObservableCollection<Material> dbItems; //Элементы в БД
        public ObservableCollection<CalcProduct> calcItems = new ObservableCollection<CalcProduct>(); //Элементы в расчётке

        private bool isAddtoDependency = false; //Указывает на то идёт ли сейчас добавление в расчётку
        private CalcProduct selectItemForDependencies; //Текущий выбранный элемент, в который идёт добавление зависимостей
        private bool isCalcOpened = false;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized; // Разворачиваем окно на весь экран
            fileImporter.ImportSettingsFromFile(this);
            fullCostType.Content = settings.FullCostType;
            fileImporter.ImportCountriesFromFTP();
            ConnectionString_Global.Value = settings.Price;
            if (!File.Exists(ConnectionString_Global.Value))
            {
                MessageBox.Show("Указанная База Данных не была найдена. Приложение будет открыто без Базы Данных");
            }
            else
            {
                CalcDataGrid.ItemsSource = calcItems;
                DependencyDataGrid.ItemsSource = dependencies;

                dbItems = repository.Get_AllMaterials();
                dataBaseGrid.ItemsSource = dbItems;

                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString(); //Отображение количества товаров

                //Пробное получение всех производителей
                List<string> firstColumnValues = dataBaseGrid.ItemsSource
                                                 .Cast<Material>() // Приводим к вашему типу данных
                                                 .Select(item => item.Manufacturer) // Получаем значение из первого столбца (например, Manufacturer)
                                                 .Distinct() // Убираем дубликаты
                                                 .ToList();
                foreach (string item in firstColumnValues)
                {
                    CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = item });
                }

                //Добавление ItemSource компонентам
                Manufacturer_comboBox.ItemsSource = CountryManager.Instance.allManufacturers;
                ProductName_comboBox.ItemsSource = dbItems;
                Article_comboBox.ItemsSource = dbItems;
                Cost_comboBox.ItemsSource = dbItems;

                allCountries_comboBox.ItemsSource = CountryManager.Instance.countries;
                isCalcSaved = true;

                DataContext = this;
            }
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
                    var selectedItem = (Material)dataBaseGrid.SelectedItem;
                    selectedItem.Photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    repository.UpdateMaterial(selectedItem);
                    dataBaseGrid.Items.Refresh();
                }
            }
            catch { }
            //...
        }

        private void deletePhoto_button_Click(object sender, RoutedEventArgs e) //Удаление картинки
        {
            ImageUpdater.DeleteImage(productImage);
            var selectedItem = (Material)dataBaseGrid.SelectedItem;
            selectedItem.Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
            repository.UpdateMaterial(selectedItem);
            dataBaseGrid.Items.Refresh();
        }

        private void uploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(productImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (Material)dataBaseGrid.SelectedItem;
                    selectedItem.Photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    repository.UpdateMaterial(selectedItem);
                    dataBaseGrid.Items.Refresh();
                }
            }
            catch { }
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
            Material selectedItem = (Material)dataBaseGrid.SelectedItem; //Получение текущего выделенного элемента
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
                Material newMaterial = new Material
                {
                    Manufacturer = newManufacturer_textBox.Text,
                    ProductName = newProductName_textBox.Text,
                    Article = newArticle_textBox.Text,
                    Unit = newUnit_textBox.Text,
                    Cost = float.Parse(newCost_textBox.Text),
                };

                if (addedProductImage.Source.ToString() != "pack://application:,,,/resources/images/without_picture.png") //Если картинка изменилась
                {
                    //Конвертация из Image в массив байтов
                    ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                    newMaterial.Photo = converter.ConvertFromComponentImageToByteArray(addedProductImage);
                }
                if(newMaterial.Photo == null)
                {
                    newMaterial.Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                }

                //Добавление
                repository.Add_Material(newMaterial);
                dbItems.Add(newMaterial);
                //dbItems = repository.Get_AllMaterials();
                //dataBaseGrid.ItemsSource = dbItems;
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();

                //Проверка на нового производителя
                bool isNewManufacturer = !CountryManager.Instance.allManufacturers
                                         .Any(manufacturerItem => manufacturerItem.name == newMaterial.Manufacturer);
                if (isNewManufacturer)
                {
                    CountryManager.Instance.allManufacturers.Add(new Manufacturer { name = newMaterial.Manufacturer });
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
            var items = dataBaseGrid.SelectedItems;
            //Создаем список для хранения выделенных элементов нужного типа
            List<Material> selectedItems = new List<Material>();

            // Перебираем выделенные элементы и добавляем их в список
            foreach (var item in items)
            {
                if (item is Material product) // Проверяем, является ли элемент CalcProduct
                {
                    selectedItems.Add(product); // Добавляем в список
                }
            }
            if (selectedItems != null)
            {
                foreach (var item in selectedItems)
                {
                    dbItems.Remove(item);
                    repository.DeleteMaterial(item);
                }
                dataBaseGrid.Items.Refresh();
            }
        }

        private void deleteSelectedManufacturerProducts_button_Click(object sender, RoutedEventArgs e) //Удаление всех товаров выделенного производителя
        {
            try
            {
                Material selectedItem = (Material)dataBaseGrid.SelectedItem;
                //запрос (Если среди производителей бд есть те, что равны с выделенным)
                IEnumerable<Material> dataForRemove = dataBaseGrid.Items.OfType<Material>()
                                                                      .Where(item => item.Manufacturer == selectedItem.Manufacturer);

                foreach (var item in dataForRemove.Cast<Material>().ToArray())
                {
                    dbItems.Remove(item);
                    repository.DeleteMaterial(item);
                }
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
            catch { }
        }

        private void Information_textBox_LostFocus(object sender, RoutedEventArgs e) //Обновление информации в выбранном элементе dataBaseGrid при потере фокуса на TextBox
        {
            var selectedItem = (Material)dataBaseGrid.SelectedItem;

            selectedItem.Manufacturer = ManufacturerInformation_textBox.Text;
            selectedItem.ProductName = ProductNameInformation_textBox.Text;
            selectedItem.Article = ArticleInformation_textBox.Text;
            selectedItem.Unit = UnitInformation_textBox.Text;
            selectedItem.Cost = float.Parse(CostInformation_textBox.Text);

            repository.UpdateMaterial(selectedItem);
            dataBaseGrid.Items.Refresh();
        }

        private void simpleSettings_menuItem_Click(object sender, RoutedEventArgs e) //Открытие настроек
        {
            SimpleSettings simpleSettings = new SimpleSettings(settings, this);
            simpleSettings.ShowDialog();
            fullCostType.Content = settings.FullCostType;
        }

        private void priceCalcButton_Click(object sender, RoutedEventArgs e) //Переход на прайс и расчётку
        {
            if (CalcDataGrid_Grid.Visibility == Visibility.Hidden) //Если открыт прайс
            {
                priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                CulcGrid_Grid.Visibility = Visibility.Visible;
                CalcDataGrid_Grid.Visibility = Visibility.Visible;

                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Hidden;
                DataBaseGrid_Grid.Visibility = Visibility.Hidden;

                TotalCostRow_row.Height = new GridLength(39, GridUnitType.Pixel);
                dataBaseBorder_border.CornerRadius = new CornerRadius(15, 15, 0, 0);

                isCalcOpened = true;
            }
            else //Если открыть расчётка
            {
                priceCalcButton.Content = "ПРАЙС->РАСЧЁТ";

                searchGrid.Visibility = Visibility.Visible;
                DataBaseGrid_Grid.Visibility = Visibility.Visible;

                CulcGrid_Grid.Visibility = Visibility.Hidden;
                CalcDataGrid_Grid.Visibility = Visibility.Hidden;

                TotalCostRow_row.Height = new GridLength(0, GridUnitType.Pixel);
                dataBaseBorder_border.CornerRadius = new CornerRadius(15, 15, 15, 15);

                isCalcOpened = false;
            }
        }

        private void dataBaseGrid_MouseDoubleClick(object sender, EventArgs e) //Добавление в расчётку при двойном нажатии на элемент 
        {
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, fullCost);
            if (!isAddedWell)
            {
                MessageBox.Show("Для начала добавьте раздел!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                CalcController.ObjectFlashing(priceCalcButton, Colors.LightGray, Colors.White);
            }
        }

        private void CalcdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //При смене текущего выделенного элемента расчётки
        {
            //Отображении информации о текущем выделенном элементе
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Получение текущего выделенного элемента
            if (selectedItem != null)
            {
                if (selectedItem.ProductName == string.Empty) //Если нажат раздел
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

                    if (!isAddtoDependency)
                    {
                        DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                    }
                    CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                }
            }
        }

        private void CalcDeleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выбранного товара из расчётки
        {

            var items = CalcDataGrid.SelectedItems;
            //Создаем список для хранения выделенных элементов нужного типа
            List<CalcProduct> selectedItems = new List<CalcProduct>();

            // Перебираем выделенные элементы и добавляем их в список
            foreach (var item in items)
            {
                if (item is CalcProduct product) // Проверяем, является ли элемент CalcProduct
                {
                    selectedItems.Add(product); // Добавляем в список
                }
            }

            if (selectedItems != null)
            {
                foreach(var item in selectedItems)
                {
                    calcItems.Remove(item);
                }
                CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                isCalcSaved = false;
                CalcDataGrid.Focus();
            }
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
            isCalcSaved = false;
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
                if (selectedItem != null && selectedItem.ProductName != string.Empty)
                {
                    bool imageIsEdit = ImageUpdater.UploadImageFromFile(CalcProductImage); //Загрузка картинки

                    if (imageIsEdit) //Если картинку загрузили
                    {
                        //Изменение картинки в calcDataGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                        isCalcSaved = false;
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
                if (selectedItem != null && selectedItem.ProductName != string.Empty)
                {
                    ImageUpdater.DeleteImage(CalcProductImage);

                    int index = CalcDataGrid.SelectedIndex;
                    calcItems[index].Photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
                    CalcDataGrid.Items.Refresh();
                    isCalcSaved = false;
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
                if (selectedItem != null && selectedItem.ProductName != string.Empty)
                {
                    bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(CalcProductImage); //Загрузка картинки

                    if (imageIsEdit) //Если картинку загрузили
                    {
                        //Изменение картинки в dataBaseGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                        isCalcSaved = false;
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
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, fullCost, count); //Добавление
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
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, fullCost, count, "UnderSelect"); //Добавление
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
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, fullCost, count, "Replace"); //Замена
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
                TotalCost = double.NaN,
                RowColor = CalcController.ColorToHex(Colors.LightYellow)
            };

            //Добавление
            calcItems.Insert(selectedIndex + 1, chapter);
            isCalcSaved = false;
        }

        private void CalcToExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel
        {
            fileImporter.ExportToExcel(this);
        }

        private void CalcToNewSheetExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel как новый лист
        {
            fileImporter.ExportToExcelAsNewSheet(this);
        }

        private void Window_Closed(object sender, EventArgs e) //При закрытии приложения сохраняются настройки
        {
            fileImporter.ExportSettingsOnFile(this);
        }

        private void saveCaalc_menuItem_Click(object sender, RoutedEventArgs e) //Сохранение расчётки
        {
            CalcDataGrid.SelectedItem = null;
            fileImporter.ExportCalcToFile(this);
            isCalcSaved = true;
        }

        private void openCalc_menuItem_Click(object sender, RoutedEventArgs e) //Открытие расчётки из файла
        {
            if (isCalcSaved == false) //Если расчётка не сохранена
            {
                MessageBoxResult res = MessageBox.Show("Не желаете сохранить эту расчётку?", "", MessageBoxButton.YesNo, MessageBoxImage.Information);
                isCalcSaved = true;
                if (res == MessageBoxResult.Yes)
                {
                    CalcDataGrid.SelectedItem = null;
                    saveCaalc_menuItem_Click(sender, e);
                    return;
                }
            }
            fileImporter.ImportCalcFromFile(this);
            isCalcSaved = true;
            DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей
            CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
            CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
            if (!CalcController.CheckingDifferencesWithDB(CalcDataGrid, this))
            {
                CheckDidderencies_button.Background = new SolidColorBrush(Colors.Coral);
                CheckDidderencies_image.Source = new BitmapImage(new Uri("resources/images/CheckDidderencies_false.png", UriKind.Relative));
            }
        }

        private void newCalc_menuItem_Click(object sender, RoutedEventArgs e) //Создание новой расчётки
        {
            if(isCalcSaved == false) //Если расчётка не сохранена
            {
                MessageBoxResult res = MessageBox.Show("Не желаете сохранить эту расчётку?", "", MessageBoxButton.YesNo, MessageBoxImage.Information);
                isCalcSaved = true;
                if (res == MessageBoxResult.Yes) 
                {
                    CalcDataGrid.SelectedItem = null;
                    saveCaalc_menuItem_Click(sender, e);
                    return;
                }
            }
            calcItems.Clear();
            DependencyDataGrid.ItemsSource = dependencies;
            CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
            editedFileName.Content = "-";
            fullCost.Content = "0";
        }

        private void startStopAddingDependency_button_Click(object sender, RoutedEventArgs e)
        {
            if(!isAddtoDependency) //Если добавление начинается сейчас
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Выбранный элемент
                if (selectedItem != null)
                {
                    if(selectedItem.ProductName == string.Empty) //Если раздел
                    {
                        return;
                    }    
                    CalcController.UpdateCellStyle(CalcDataGrid, Brushes.LightGreen, Brushes.White); //Теперь при выборе цвет становится салатовым
                    CalcDataGrid.SelectedItem = null; //Выделенный элемент убирается
                    selectedItem.RowColor = CalcController.ColorToHex(Colors.MediumSeaGreen); //Выбранный элемент становится зелёным, чтобы было видно какому элементу добавляются зависимости
                    selectedItem.RowForegroundColor = CalcController.ColorToHex(Colors.White); //Цвет текста у выделенного элемента
                    CalcDataGrid.Items.Refresh();
                    selectItemForDependencies = selectedItem; //Запоминаем выделенный элемент
                    isAddtoDependency = true;
                    DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                    //Изменение стиля кнопки
                    startStopAddingDependency_button.Background = Brushes.Coral;
                    startStopAddingDependency_image.Source = new BitmapImage(new Uri("resources/images/stop.png", UriKind.Relative));
                    startStopAddingDependency_image.ToolTip = "Прекратить добавление зависимостей";
                }
            }
            else
            {
                //Возвращение всего на свои места при повторном нажатии кнопки
                isAddtoDependency = false;
                CalcController.UpdateCellStyle(CalcDataGrid, Brushes.MediumSeaGreen, Brushes.White);
                selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.Transparent);
                selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.Gray);
                CalcDataGrid.SelectedItem = selectItemForDependencies;
                CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                startStopAddingDependency_button.Background = Brushes.MediumSeaGreen;
                startStopAddingDependency_image.Source = new BitmapImage(new Uri("resources/images/play.png", UriKind.Relative));
                startStopAddingDependency_image.ToolTip = "Начать добавление зависимостей";
            }

        }

        private void deleteDependency_button_Click(object sender, RoutedEventArgs e)
        {
            CalcProduct selectedCalc = (CalcProduct)CalcDataGrid.SelectedItem; //Текущий элемент

            if (isAddtoDependency) //Если сейчас идёт добавление, то меняем выделенный элемент на тот, в который сейчас идёт добавление зависимостей
            {
                selectedCalc = selectItemForDependencies;
            }

            if (selectedCalc != null)
            {
                Dependency selectDependency = (Dependency)DependencyDataGrid.SelectedItem; //Текущая выбранная зависимость
                if (selectDependency != null)
                {
                    //Удаление
                    selectedCalc.dependencies.Remove(selectDependency);
                    if (selectedCalc.dependencies.Count == 0)
                    {
                        selectedCalc.isDependency = false;
                    }
                    CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                }
            }
        }

        private void Manufacturer_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manufacturer manufacturerItem = (Manufacturer)Manufacturer_comboBox.SelectedItem;
            if(manufacturerItem != null) 
            {
                var selectedItems = dbItems.Where(item => item.Manufacturer == manufacturerItem.name).ToList();

                ProductName_comboBox.SelectedItem = selectedItems[0];
                Article_comboBox.SelectedItem = selectedItems[0];
                Cost_comboBox.SelectedItem = selectedItems[0];

                ProductName_comboBox.ItemsSource = selectedItems;
                Article_comboBox.ItemsSource = selectedItems;
                Cost_comboBox.ItemsSource = selectedItems;

                dataBaseGrid.SelectedItem = selectedItems[0];
            }
        }

        private void ProductName_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Material selectedItem = (Material)ProductName_comboBox.SelectedItem;
            if(selectedItem != null) 
            {
                var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                Manufacturer_comboBox.SelectedItem = selectManufacturer;
                Article_comboBox.SelectedItem = selectedItem;
                Cost_comboBox.SelectedItem= selectedItem;

                dataBaseGrid.SelectedItem = selectedItem;
            }
        }

        private void Article_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Material selectedItem = (Material)Article_comboBox.SelectedItem;
            if (selectedItem != null)
            {
                var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                Manufacturer_comboBox.SelectedItem = selectManufacturer;
                ProductName_comboBox.SelectedItem = selectedItem;
                Cost_comboBox.SelectedItem = selectedItem;

                dataBaseGrid.SelectedItem = selectedItem;
            }
        }

        private void Cost_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Material selectedItem = (Material)Cost_comboBox.SelectedItem;
            if (selectedItem != null)
            {
                var selectManufacturer = CountryManager.Instance.allManufacturers.First(item => item.name == selectedItem.Manufacturer);
                Manufacturer_comboBox.SelectedItem = selectManufacturer;
                ProductName_comboBox.SelectedItem = selectedItem;
                Article_comboBox.SelectedItem = selectedItem;

                dataBaseGrid.SelectedItem = selectedItem;
            }
        }

        private void CalcDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) //Выбор элемента для добавления в зависимость
        {
            if(isAddtoDependency) //Если сейчас идёт добавление
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Текущий выбранный элемент
                if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ProductName != string.Empty) //Если элемент выбран и это не текущий для добавления
                {
                    if (selectItemForDependencies.dependencies.Count == 0) //Если до этого не было зависимостей, то появляется отметка зависимости
                    {
                        selectItemForDependencies.isDependency = true;
                    }
                    //Добавление
                    selectItemForDependencies.dependencies.Add(new Dependency { ProductName = selectedItem.ProductName, SelectedType = "*", Multiplier = 1 });
                    CalcDataGrid.Items.Refresh();
                    CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                    isCalcSaved = false;
                }
            }
        }

        private void uploadDataBase_menuItem_Click(object sender, RoutedEventArgs e) //Загрузка выбранной БД с пк
        {
            //Открытие диалогового окна
            OpenFileDialog file = new OpenFileDialog();

            //Параметры для открытия
            file.Title = "Путь к локальной DB";
            file.InitialDirectory = settings.PriceFolderPath;
            file.Filter = "MDF File|*.mdf";
            file.RestoreDirectory = true;

            if (file.ShowDialog() == true) //Если файл выбран
            {
                ConnectionString_Global.Value = file.FileName;
                settings.Price = file.FileName;

                //Запускаем новое экземпляр приложения
                string exePath= AppDomain.CurrentDomain.BaseDirectory + "Dahmira.exe";
                Process.Start(exePath);

                //Закрываем текущее приложение
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void downloadDataBaseFromFtp_menuItem_Click(object sender, RoutedEventArgs e) //Загрузка БД с сервера
        {
            ConnectionString_Global.Value = settings.PriceFolderPath + "Dahmira_TestDb.mdf";
            settings.Price = settings.PriceFolderPath + "Dahmira_TestDb.mdf";
            fileImporter.ImportDBFromFTP(this);

            //Запускаем новое экземпляр приложения
            string exePath = AppDomain.CurrentDomain.BaseDirectory + "Dahmira.exe";
            Process.Start(exePath);

            //Закрываем текущее приложение
            System.Windows.Application.Current.Shutdown();
        }

        private void CalcDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var items = CalcDataGrid.SelectedItems;

            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            };
            // Создаем список для хранения выделенных элементов нужного типа
            List<CalcProduct> selectedItems = new List<CalcProduct>();

            // Перебираем выделенные элементы и добавляем их в список
            foreach (var item in items)
            {
                if (item is CalcProduct product) // Проверяем, является ли элемент CalcProduct
                {
                    selectedItems.Add(product); // Добавляем в список
                }
            }

            if (items != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (e.Key == Key.C)
                    {
                        // Сериализация выделенных элементов в JSON
                        var itemsToCopy = new List<CalcProduct>();
                        foreach (var item in selectedItems)
                        {
                            if (item is CalcProduct product)
                            {
                                itemsToCopy.Add(product.Clone());
                            }
                        }
                        string json = JsonSerializer.Serialize(itemsToCopy, options);
                        Clipboard.SetText(json); // Сохраняем в буфер обмена
                        e.Handled = true; // Указываем, что событие обработано
                    }
                    else if (e.Key == Key.X)
                    {
                        // Копируем и удаляем выделенные элементы
                        var itemsToCopy = new List<CalcProduct>();
                        foreach (var item in selectedItems)
                        {
                            if (item is CalcProduct product)
                            {
                                itemsToCopy.Add(product.Clone());
                                calcItems.Remove(item);
                            }
                        }
                        string json = JsonSerializer.Serialize(itemsToCopy, options);
                        Clipboard.SetText(json); // Сохраняем в буфер обмена
                        CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                        e.Handled = true; // Указываем, что событие обработано
                    }
                    else if (e.Key == Key.V)
                    {
                        //Извлекаем из буфера обмена
                        if (Clipboard.ContainsText())
                        {
                            try
                            {
                                string json = Clipboard.GetText();
                                var itemsToPaste = JsonSerializer.Deserialize<List<CalcProduct>>(json, options);

                                if (isCalcOpened) //Если открыта расчётка то вставляем в расчётку
                                {
                                    if (itemsToPaste != null)
                                    {
                                        for (int i = 0; i < itemsToPaste.Count; i++)
                                        {
                                            calcItems.Insert(CalcDataGrid.SelectedIndex + 1, itemsToPaste[i]);
                                        }
                                        CalcController.Refresh(CalcDataGrid, calcItems, fullCost);
                                        e.Handled = true; // Указываем, что событие обработано
                                    }
                                }
                                else //Если открыта не расчётка, то вставляем в основную бд, создав при этом новый элемент
                                {
                                    List<Material> materialsToPaste = new List<Material>();
                                    foreach(var item in itemsToPaste)
                                    {
                                        if(!dbItems.Any(i => i.Article == item.Article) && item.ProductName != string.Empty)
                                        {
                                            Material newMaterial = new Material
                                            {
                                                Manufacturer = item.Manufacturer,
                                                ProductName = item.ProductName,
                                                Unit = item.Unit,
                                                Article = item.Article,
                                                Photo = item.Photo,
                                                Cost = (float)item.RealCost
                                            };

                                            repository.Add_Material(newMaterial);
                                            dbItems.Add(newMaterial);
                                            dataBaseGrid.Items.Refresh();
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void CheckDidderencies_button_Click(object sender, RoutedEventArgs e)
        {
            if(!CalcController.CheckingDifferencesWithDB(CalcDataGrid, this)) //Если есть несоответствия
            {
                CheckDidderencies_button.Background = new SolidColorBrush(Colors.Coral);
                CheckDidderencies_image.Source = new BitmapImage(new Uri("resources/images/CheckDidderencies_false.png", UriKind.Relative));
            }
            else //Если соответствия есть
            {
                CheckDidderencies_button.Background = new SolidColorBrush(Colors.MediumSeaGreen);
                CheckDidderencies_image.Source = new BitmapImage(new Uri("resources/images/CheckDidderencies_true.png", UriKind.Relative));
            }
        }
    }
}
