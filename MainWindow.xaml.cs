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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlServerOpenJsonExpression;
using ColumnInfo = Dahmira.Models.ColumnInfo;

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

        private List<Material> materialForDBAdding = new List<Material>();
        private List<Material> materialForDBUpdating = new List<Material>();
        private List<Material> materialForDBDeleting = new List<Material>();

        public bool isCalculationNeed = true;
        bool isDependencySelected = false;

        public MainWindow()
        {
            InitializeComponent();

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            //Указываем в реестре, что формат dah открывается через программу Dahmira + указываем путь программы для дальнейшего запуска файла в реестре
            string extension = "dah"; // Укажите ваше расширение
            string progId = "Dahmira";
            string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileAssociationHelper.RegisterFileAssociation(extension, progId, applicationPath);

            fileImporter.ImportSettingsFromFile(this);

            calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0 });

            this.Title = $"Dahmira       {settings.Price}";
            this.WindowState = WindowState.Maximized; // Разворачиваем окно на весь экран

            fileImporter.ImportCountriesFromFTP();

            ConnectionString_Global.Value = settings.Price;

            allCountries_comboBox.ItemsSource = CountryManager.Instance.priceManager.countries;
            CalcDataGrid.ItemsSource = calcItems;
            DependencyDataGrid.ItemsSource = dependencies;
            isCalcSaved = true;
            if (!File.Exists(ConnectionString_Global.Value))
            {
                MessageBox.Show("Указанная База Данных не была найдена. Приложение будет открыто без Базы Данных");
            }
            else
            {

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
            }
            DataContext = this;

            //Создание анимации для уведомления об отсутствии расчёта
            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = 0, // Начальная позиция (текущая позиция)
                To = 150, // Конечная позиция (1000 пикселей вправо)
                Duration = new Duration(TimeSpan.FromSeconds(5)), // Длительность анимации
                AutoReverse = true, // Включаем автоматическое обратное движение
                RepeatBehavior = RepeatBehavior.Forever // Повторяем бесконечно
            };

            labelTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
        }
        private void addGrid_Button_Click(object sender, RoutedEventArgs e) //Смена функционала меню
        {
            addGrid.Visibility = Visibility.Visible;
            searchGrid.Visibility = Visibility.Hidden;
            searchGrid_Button.Background = new SolidColorBrush(Colors.LightGray);
            addGrid_Button.Background = new SolidColorBrush(Colors.MediumSeaGreen);
        }

        private void searchGrid_Button_Click(object sender, RoutedEventArgs e)
        {
            searchGrid.Visibility = Visibility.Visible;
            addGrid.Visibility = Visibility.Hidden;
            searchGrid_Button.Background = new SolidColorBrush(Colors.Coral);
            addGrid_Button.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void uploadFromFile_button_Click(object sender, RoutedEventArgs e) //Загрузка картинки из файла
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromFile(productImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    PriceInfo_label.Content = "Картинка загружена из файла.";
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (Material)dataBaseGrid.SelectedItem;
                    byte[] photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    selectedItem.Photo = photo;
                    if(!materialForDBUpdating.Any(i => i == selectedItem))
                    {
                        materialForDBUpdating.Add(selectedItem);
                    }
                    //Изменение фото в расчётке
                    List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
                    if(foundedCalcProducts != null) 
                    {
                        foreach (CalcProduct product in foundedCalcProducts)
                        {
                            product.Photo = photo;
                        }
                        CalcDataGrid.Items.Refresh();
                    }
                    
                    dataBaseGrid.Items.Refresh();
                }
                else
                {
                    PriceInfo_label.Content = "Картинка не была загружена из файла.";
                }
            }
            catch { }
        }

        private void deletePhoto_button_Click(object sender, RoutedEventArgs e) //Удаление картинки
        {
            ImageUpdater.DeleteImage(productImage);
            var selectedItem = (Material)dataBaseGrid.SelectedItem;
            byte[] photo = converter.ConvertFromFileImageToByteArray("without_image_database.png");
            selectedItem.Photo = photo;
            if (!materialForDBUpdating.Any(i => i == selectedItem))
            {
                materialForDBUpdating.Add(selectedItem);
            }
            //Изменение фото в расчётке
            List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
            if (foundedCalcProducts != null)
            {
                foreach (CalcProduct product in foundedCalcProducts)
                {
                    product.Photo = photo;
                }
                CalcDataGrid.Items.Refresh();
            }
            dataBaseGrid.Items.Refresh();
            PriceInfo_label.Content = "Картинка удалена.";
        }

        private void uploadFromClipboard_Click(object sender, RoutedEventArgs e) //Загрузка картинки из буфера
        {
            try
            {
                bool imageIsEdit = ImageUpdater.UploadImageFromClipboard(productImage); //Загрузка картинки

                if (imageIsEdit) //Если картинку загрузили
                {
                    PriceInfo_label.Content = "Картинка загружена из буфера.";
                    //Изменение картинки в dataBaseGrid
                    var selectedItem = (Material)dataBaseGrid.SelectedItem;
                    byte[] photo = converter.ConvertFromComponentImageToByteArray(productImage);
                    selectedItem.Photo = photo;
                    if (!materialForDBUpdating.Any(i => i == selectedItem))
                    {
                        materialForDBUpdating.Add(selectedItem);
                    }
                    //Изменение фото в расчётке
                    List<CalcProduct> foundedCalcProducts = calcItems.Where(i => i.Article == selectedItem.Article).ToList();
                    if (foundedCalcProducts != null)
                    {
                        foreach (CalcProduct product in foundedCalcProducts)
                        {
                            product.Photo = photo;
                        }
                        CalcDataGrid.Items.Refresh();
                    }

                    dataBaseGrid.Items.Refresh();
                }
                else
                {
                    PriceInfo_label.Content = "Картинка не была загружена из буфера.";
                    WarningFlashing("В буфера нет картинки!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                }
            }
            catch { }
        }

        private void downloadToClipboard_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер
        {
            PriceInfo_label.Content = "Картинка загружена в буфер.";
            ImageUpdater.DownloadImageToClipboard(productImage);
        }

        private void downloadToFile_button_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл
        {
            bool imageIsDownload = ImageUpdater.DownloadImageToFile(productImage);
            if(imageIsDownload) 
            {
                PriceInfo_label.Content = "Картинка загружена в файл.";
            }
            else
            {
                PriceInfo_label.Content = "Картинка не была загружена в файл.";
            }
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
                    WarningFlashing("Не все поля заполнены!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь заполнил не все поля.";
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
                if (!materialForDBAdding.Any(i => i == newMaterial))
                {
                    materialForDBAdding.Add(newMaterial);
                }
                dbItems.Add(newMaterial);
                PriceInfo_label.Content = "Добавлена новая позиция в прайс. Порядковый номер: " + dataBaseGrid.Items.Count.ToString();
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
                WarningFlashing("Формат введённых данных неверен!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь ввёл неверный формат данных.";
            }
        }

        private void deleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выделенного элемента прайса
        {
            var res = MessageBox.Show("Вы точно хотите удалить выбранные элементы?", "", MessageBoxButton.YesNo);
            if(res == MessageBoxResult.Yes) 
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
                        if (!materialForDBDeleting.Any(i => i == item))
                        {
                            materialForDBDeleting.Add(item);
                        }
                        PriceInfo_label.Content = "Выбранные товары удалены.";
                    }
                    dataBaseGrid.Items.Refresh();
                }
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
        }

        private void deleteSelectedManufacturerProducts_button_Click(object sender, RoutedEventArgs e) //Удаление всех товаров выделенного производителя
        {
            try
            {
                var res = MessageBox.Show("Вы точно хотите удалить выбранные элементы?", "", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Material selectedItem = (Material)dataBaseGrid.SelectedItem;
                    //запрос (Если среди производителей бд есть те, что равны с выделенным)
                    IEnumerable<Material> dataForRemove = dataBaseGrid.Items.OfType<Material>()
                                                                          .Where(item => item.Manufacturer == selectedItem.Manufacturer);

                    foreach (var item in dataForRemove.Cast<Material>().ToArray())
                    {
                        dbItems.Remove(item);
                        if (!materialForDBDeleting.Any(i => i == item))
                        {
                            materialForDBDeleting.Add(item);
                        }
                        PriceInfo_label.Content = "Выбранный производитель удалён.";
                    }
                    productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
                }
            }
            catch { }
        }

        private void Information_textBox_LostFocus(object sender, RoutedEventArgs e) //Обновление информации в выбранном элементе dataBaseGrid при потере фокуса на TextBox
        {
            try
            {
                var selectedItem = (Material)dataBaseGrid.SelectedItem;

                selectedItem.Manufacturer = ManufacturerInformation_textBox.Text;
                selectedItem.ProductName = ProductNameInformation_textBox.Text;
                selectedItem.Article = ArticleInformation_textBox.Text;
                selectedItem.Unit = UnitInformation_textBox.Text;
                selectedItem.Cost = float.Parse(CostInformation_textBox.Text);

                //repository.UpdateMaterial(selectedItem);
                if (!materialForDBUpdating.Any(i => i == selectedItem))
                {
                    materialForDBUpdating.Add(selectedItem);
                }
                dataBaseGrid.Items.Refresh();
            } catch { }
        }

        private void simpleSettings_menuItem_Click(object sender, RoutedEventArgs e) //Открытие настроек
        {
            SimpleSettings simpleSettings = new SimpleSettings(settings, this);
            simpleSettings.ShowDialog();
            calcItems[calcItems.Count - 1].Count = settings.FullCostType;
            CalcDataGrid.Items.Refresh();
        }

        private void priceCalcButton_Click(object sender, RoutedEventArgs e) //Переход на прайс и расчётку
        {
            if (CalcDataGrid_Grid.Visibility == Visibility.Hidden) //Если открыт прайс
            {
                if(DependencyDataGrid.SelectedItem != null) 
                {
                    isDependencySelected = true;
                }
                priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

                CulcGrid_Grid.Visibility = Visibility.Visible;
                CalcDataGrid_Grid.Visibility = Visibility.Visible;

                addGrid.Visibility = Visibility.Hidden;
                searchGrid.Visibility = Visibility.Hidden;
                DataBaseGrid_Grid.Visibility = Visibility.Hidden;

                priceCalcButton.Background = new SolidColorBrush(Colors.LightGreen);
                addGrid_Button.Visibility = Visibility.Hidden;
                searchGrid_Button.Visibility = Visibility.Hidden;

                isCalcOpened = true;
            }
            else //Если открыта расчётка
            {
                isDependencySelected = false;

                priceCalcButton.Content = "ПРАЙС->РАСЧЁТ";

                searchGrid.Visibility = Visibility.Visible;
                DataBaseGrid_Grid.Visibility = Visibility.Visible;

                CulcGrid_Grid.Visibility = Visibility.Hidden;
                CalcDataGrid_Grid.Visibility = Visibility.Hidden;

                priceCalcButton.Background = new SolidColorBrush(Colors.LightPink);
                addGrid_Button.Visibility = Visibility.Visible;
                searchGrid_Button.Visibility = Visibility.Visible;
                searchGrid_Button.Background = new SolidColorBrush(Colors.Coral);
                addGrid_Button.Background = new SolidColorBrush(Colors.LightGray);

                isCalcOpened = false;
            }
        }

        private void dataBaseGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) //Добавление в расчётку при двойном нажатии на элемент 
        {
            if(e.ChangedButton == MouseButton.Right)
            {
                bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this/*, fullCost*/);
                if (!isAddedWell)
                {
                    WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                    PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
                }
            }
        }
        private CancellationTokenSource _cancellationTokenSource;

        public async void WarningFlashing(string content, System.Windows.Controls.Border border, Label label, Color color, double interval)
        {
            // Отменяем предыдущую задачу, если она существует
            _cancellationTokenSource?.Cancel();

            // Создаем новый токен отмены
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            label.Content = content;
            CalcController.ObjectFlashing(border, Colors.Transparent, color, interval);

            try
            {
                await Task.Delay(Convert.ToInt32(700 * interval * 3), token); // Передаем токен в Task.Delay
                border.Background = new SolidColorBrush(Colors.Transparent);
            }
            catch (TaskCanceledException){ }
        }

        private void CalcdataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) //При смене текущего выделенного элемента расчётки
        {

            isDependencySelected = false;
            if (CalcDataGrid.SelectedIndex != calcItems.Count - 1)
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Получение текущего выделенного элемента
                if (selectedItem != null)
                {
                    if (!isAddtoDependency)
                    {
                        if (selectedItem.isDependency == true)
                        {
                            DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                            DependencyImage.Visibility = Visibility.Hidden;
                            DependencyDataGrid.Visibility = Visibility.Visible;
                            DependencyButtons.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            DependencyImage.Visibility = Visibility.Visible;
                            DependencyDataGrid.Visibility = Visibility.Hidden;
                            DependencyButtons.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        if (!selectItemForDependencies.dependencies.Any(i => i.ProductId == selectedItem.ID))
                        {
                            if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ID != 0) //Если элемент выбран и это не текущий для добавления
                            {
                                if (selectItemForDependencies.dependencies.Count == 0) //Если до этого не было зависимостей, то появляется отметка зависимости
                                {
                                    selectItemForDependencies.isDependency = true;
                                }
                                //Добавление
                                selectItemForDependencies.dependencies.Add(new Dependency { ProductId = selectedItem.ID, ProductName = selectedItem.ProductName, SelectedType = "*", Multiplier = 1 });
                                isCalcSaved = false;
                            }
                        }
                    }

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
                    }
                }
            }
           CalcController.Refresh(CalcDataGrid, calcItems);
        }

        private void CalcDeleteSelectedProduct_button_Click(object sender, RoutedEventArgs e) //Удаление выбранного товара из расчётки
        {
            if(CalcDataGrid.SelectedIndex != calcItems.Count - 1) 
            {
                var items = CalcDataGrid.SelectedItems;
                //Создаем список для хранения выделенных элементов нужного типа
                List<CalcProduct> itemsForRemove = new List<CalcProduct>();

                // Перебираем выделенные элементы и добавляем их в список
                foreach (var item in items)
                {
                    CalcProduct product = (CalcProduct)item;
                    if (product.Manufacturer != string.Empty)
                    {
                        itemsForRemove.Add(product); // Добавляем в список
                    }
                }

                //Проверка на то, есть ли зависимость
                foreach (var calcItem in calcItems)
                {
                    foreach(var dependency in calcItem.dependencies) 
                    {
                        CalcProduct item = itemsForRemove.FirstOrDefault(i => i.ID == dependency.ProductId);
                        if (item != null)
                        {
                            MessageBoxResult res = MessageBox.Show($"Вы уверены что хотите удалить товар, находящийся в зависимости:\nНомер: {item.Num}\nПроизводитель: {item.Manufacturer}\nНаименование: {item.ProductName}\nАртикул: {item.Article}", "Товар находится в зависимости", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if(res == MessageBoxResult.No)
                            {
                                itemsForRemove.Remove(item);
                            }
                        }
                    }
                }

                if (itemsForRemove != null)
                {
                    if(itemsForRemove.Count > 0) 
                    {
                        foreach (var item in itemsForRemove)
                        {
                            calcItems.Remove(item);
                            foreach (var calcItem in calcItems) //Удаление этого элемента во всех зависимостях
                            {
                                Dependency removedDependency = calcItem.dependencies.FirstOrDefault(d => d.ProductId == item.ID);
                                calcItem.dependencies.Remove(removedDependency);
                            }
                        }
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        isCalcSaved = false;

                        DependencyImage.Visibility = Visibility.Visible;
                        DependencyDataGrid.Visibility = Visibility.Hidden;
                        DependencyButtons.Visibility = Visibility.Hidden;

                        CalcInfo_label.Content = "Выбранные товары удалены.";
                    }
                    else
                    {
                        CalcInfo_label.Content = "Выбранные товары не удалены. Возможно пользователь не выбрал товар.";
                    }
                }
            }
        }

        private void CalcDataGrid_CurrentCellChanged(object sender, EventArgs e) //Когда заканчивается редактирование ячейки
        {
            if (CalcDataGrid.SelectedIndex != calcItems.Count - 1)
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if (selectedItem != null)
                {
                    selectedItem.TotalCost = selectedItem.Cost * Convert.ToDouble(selectedItem.Count);
                   CalcDataGrid.Dispatcher.BeginInvoke(new Action(() => { CalcController.Refresh(CalcDataGrid, calcItems); }));
                }
            }
        }

        private void allCountries_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Изменение ценников в зависимости от местных поставщиков выбранной страны
        {
            if(!isCalculationNeed)
            {
                MovingLabel.Visibility = Visibility.Visible;
                isCalculationNeed = true;
            }
        }

        private void CalcRefresh_button_Click(object sender, RoutedEventArgs e) //Обновление расчётки
        {
            CalcController.Refresh(CalcDataGrid, calcItems); //Обновление
            CalcController.CheckingDifferencesWithDB(CalcDataGrid, this);
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
                        CalcInfo_label.Content = "Картинка загружена из файла.";
                        //Изменение картинки в calcDataGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                        isCalcSaved = false;
                    }
                    else
                    {
                        CalcInfo_label.Content = "Картинка не была загружена из файла.";
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
                    CalcInfo_label.Content = "Картинка удалена.";
                }
            }
            catch { }
        }

        private void CalcDownloadToFile_Click(object sender, RoutedEventArgs e) //Сохранение картинки в файл из элемента расчётки
        {
            bool isImageDownload = ImageUpdater.DownloadImageToFile(CalcProductImage);
            if (isImageDownload) 
            {
                CalcInfo_label.Content = "Картинка загружена в файла.";
            }
            else
            {
                CalcInfo_label.Content = "Картинка не была загружена в файла.";
            }
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
                        CalcInfo_label.Content = "Картинка загружена из буфера.";
                        //Изменение картинки в dataBaseGrid
                        int index = CalcDataGrid.SelectedIndex;
                        calcItems[index].Photo = converter.ConvertFromComponentImageToByteArray(CalcProductImage);
                        CalcDataGrid.Items.Refresh();
                        isCalcSaved = false;
                    }
                    else
                    {
                        CalcInfo_label.Content = "Картинка не была загружена из буфера. В буфере нет картинки.";
                    }
                }
            }
            catch { }
        }

        private void CalcDownloadToClipboard_Click(object sender, RoutedEventArgs e) //Сохранение картинки в буфер из элемента расчётки
        {
            CalcInfo_label.Content = "Картинка загружена в буфер.";
            ImageUpdater.DownloadImageToClipboard(CalcProductImage);
        }

        private void AddToCalc_button_Click(object sender, RoutedEventArgs e) //Добавление выделенного элемента DAtaBaseGrid в расчётку
        {
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
            {
                WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не указал количество.";
                return;
            }
            string count = CountProductToAdd_textBox.Text; //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count); //Добавление
            if (!isAddedWell)
            {
                WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
            }
        }

        private void AddToCalcUnderSelectedRow_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
            {
                WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не указал количество.";
                return;
            }
            string count = CountProductToAdd_textBox.Text; //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count, "UnderSelect"); //Добавление
            if (!isAddedWell)
            {
                WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
            }
        }

        private void ReplaceCalc_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CountProductToAdd_textBox.Text) || Convert.ToInt32(CountProductToAdd_textBox.Text) == 0) //Если количество не указано
            {
                WarningFlashing("Количество не указано!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не указал количество.";
                return;
            }
            string count = CountProductToAdd_textBox.Text; //Получение количества
            bool isAddedWell = CalcController.AddToCalc(dataBaseGrid, CalcDataGrid, this, count, "Replace"); //Замена
            if (!isAddedWell)
            {
                WarningFlashing("Для началa создайте раздел!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Новая позиция в прайс не добавлена. Пользователь не добавил раздел.";
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
                RowColor = CalcController.ColorToHex(Colors.LightBlue),
                RowForegroundColor = CalcController.ColorToHex(Colors.Black)
            };

            //Добавление
            if (CalcDataGrid.SelectedItem == null)
            {
                selectedIndex = 0;
                CalcInfo_label.Content = "Раздел добавлен в начало.";
                calcItems.Insert(selectedIndex, chapter);
                return;
            }

            if (CalcDataGrid.SelectedIndex != calcItems.Count - 1) 
            {
                selectedIndex++;
                CalcInfo_label.Content = $"Раздел добавлен под строкой {selectedIndex}.";
            }
            else
            {
                CalcInfo_label.Content = "Раздел добавлен в конец.";
            }

            calcItems.Insert(selectedIndex, chapter);

            isCalcSaved = false;
        }

        private void CalcToExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel
        {
            if (isCalculationNeed)
            {
                WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                CalcInfo_label.Content = "Расчёт не сохранён в Excel. Необходимо произвести расчёт.";
            }
            else
            {
                fileImporter.ExportToExcel(this);
            }
        }

        private void CalcToNewSheetExcel_button_Click(object sender, RoutedEventArgs e) //Экспорт в Excel как новый лист
        {
            if (isCalculationNeed)
            {
                WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                CalcInfo_label.Content = "Расчёт не добавлен в Excel новым листом. Необходимо произвести расчёт.";
            }
            else
            {
                fileImporter.ExportToExcelAsNewSheet(this);
            }
        }

        private void saveCaalc_menuItem_Click(object sender, RoutedEventArgs e) //Сохранение расчётки
        {
            if (isCalculationNeed)
            {
                WarningFlashing("Для начала произведите расчёт", CalcWarningBorder, CalcWarningLabel, Colors.OrangeRed, 2.5);
                CalcInfo_label.Content = "Расчёт не сохранён. Необходимо произвести расчёт.";
            }
            else
            {
                CalcDataGrid.SelectedItem = null;
                fileImporter.ExportCalcToFile(this);
                isCalcSaved = true;
                isCalculationNeed = false;
            }
        }

        private void openCalc_menuItem_Click(object sender, RoutedEventArgs e) //Открытие расчётки из файла
        {
            CalcDataGrid.SelectedItem = null;

           
            try
            {
                if (isCalcSaved == false) //Если расчётка не сохранена
                {
                    MessageBoxResult res = MessageBox.Show("Не желаете сохранить эту расчётку?", "", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    isCalcSaved = true;
                    if (res == MessageBoxResult.Yes)
                    {
                        saveCaalc_menuItem_Click(sender, e);
                        if (!isCalculationNeed)
                        {
                            CalcDataGrid.SelectedItem = null;
                        }
                        return;
                    }
                }
                fileImporter.ImportCalcFromFile(this);
                isCalcSaved = true;
                DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей
                CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                CalcController.ClearBackgroundsColors(this);
                calcItems[calcItems.Count - 1].Count = settings.FullCostType;
                CalcController.Refresh(CalcDataGrid, calcItems);
                isCalculationNeed = true;
            }
            catch //Если файл невероно формата
            {
                MessageBox.Show($"Запущен файл не соответсвует нужному типу или поврежден.");
                calcItems.Clear();
                isCalcSaved = true;
                DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей
                CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                CalcController.ClearBackgroundsColors(this);
                calcItems.Add(new CalcProduct());
                calcItems[0].Count = settings.FullCostType;
                CalcController.Refresh(CalcDataGrid, calcItems);
                isCalculationNeed = true;
            }
        }


        public void openCalcTest(string path) //Открытие расчётки из файла dah (двойной клик по нему)
        {
            //Открываем сразу расчетку
            if (DependencyDataGrid.SelectedItem != null)
            {
                isDependencySelected = true;
            }
            priceCalcButton.Content = "РАСЧЁТ->ПРАЙС";

            CulcGrid_Grid.Visibility = Visibility.Visible;
            CalcDataGrid_Grid.Visibility = Visibility.Visible;

            addGrid.Visibility = Visibility.Hidden;
            searchGrid.Visibility = Visibility.Hidden;
            DataBaseGrid_Grid.Visibility = Visibility.Hidden;

            priceCalcButton.Background = new SolidColorBrush(Colors.LightGreen);
            addGrid_Button.Visibility = Visibility.Hidden;
            searchGrid_Button.Visibility = Visibility.Hidden;

            isCalcOpened = true;

            //Проверяем на невреный файл или поврежденный
            try
            {
                //Альтернативная десериализацтя JSON
                fileImporter.ImportCalcFromFile_StartDUH(path, this);
                isCalcSaved = true;
                DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей
                CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                CalcController.ClearBackgroundsColors(this);
                calcItems[calcItems.Count - 1].Count = settings.FullCostType;

                CalcController.Refresh(CalcDataGrid, calcItems);
                isCalculationNeed = false;
            }
            catch
            {
                MessageBox.Show($"Запущен файл: {path} - не соответсвует нужному типу или поврежден.");
                calcItems.Clear();
                isCalcSaved = true;
                DependencyDataGrid.ItemsSource = dependencies; //Обнуление зависимостей
                CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
                CalcController.ClearBackgroundsColors(this);
                calcItems.Add(new CalcProduct());
                calcItems[0].Count = settings.FullCostType;
                CalcController.Refresh(CalcDataGrid, calcItems);
                isCalculationNeed = true;
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
                    saveCaalc_menuItem_Click(sender, e);
                    if (!isCalculationNeed)
                    {
                        CalcDataGrid.SelectedItem = null;
                    }
                    return;
                }
            }
            calcItems.Clear();
            calcItems.Add(new CalcProduct { Count = settings.FullCostType, TotalCost = 0 });
            DependencyDataGrid.ItemsSource = dependencies;
            CalcProductImage.Source = new BitmapImage(new Uri("resources/images/without_picture.png", UriKind.Relative));
            CalcPath_label.Content = "Имя файла расчёта: - ";
            isCalculationNeed = false;
        }

        private void deleteDependency_button_Click(object sender, RoutedEventArgs e)
        {
            CalcProduct selectedCalc = (CalcProduct)CalcDataGrid.SelectedItem; //Текущий элемент

            if (isAddtoDependency)
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
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    isDependencySelected = false;
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

        private void CalcDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (CalcDataGrid.SelectedIndex != calcItems.Count - 1)
            //{
            //    if (isAddtoDependency) //Если сейчас идёт добавление
            //    {
            //        CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Текущий выбранный элемент
            //        if (!selectItemForDependencies.dependencies.Any(i => i.ProductId == selectedItem.ID))
            //        {
            //            if (selectedItem != null && selectItemForDependencies != selectedItem && selectedItem.ID != 0) //Если элемент выбран и это не текущий для добавления
            //            {
            //                if (selectItemForDependencies.dependencies.Count == 0) //Если до этого не было зависимостей, то появляется отметка зависимости
            //                {
            //                    selectItemForDependencies.isDependency = true;
            //                }
            //                //Добавление
            //                selectItemForDependencies.dependencies.Add(new Dependency {ProductId = selectedItem.ID, ProductName = selectedItem.ProductName, SelectedType = "*", Multiplier = 1 });
            //                isCalcSaved = false;
            //            }
            //        }
            //    }
            //}

            //if (e.ChangedButton == MouseButton.Right)
            //{
            //    CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; // Текущий выбранный элемент

            //    if (CurrentItem != null && selectedItem != null && selectedItem.ID != CurrentItem.ID &&
            //        selectedItem.ProductName != string.Empty && !CurrentItem.dependencies.Any(i => i.ProductId == selectedItem.ID))
            //    {
            //        if (!CurrentItem.isDependency)
            //        {
            //            CurrentItem.isDependency = true;
            //            DependencyImage.Visibility = Visibility.Hidden;
            //            DependencyDataGrid.Visibility = Visibility.Visible;
            //            DependencyButtons.Visibility = Visibility.Visible;
            //        }

            //        CurrentItem.dependencies.Add(new Dependency
            //        {
            //            ProductId = selectedItem.ID,
            //            ProductName = selectedItem.ProductName,
            //            SelectedType = "*",
            //            Multiplier = 1
            //        });

            //        await Task.Delay(75);
            //        // Устанавливаем новый выбранный элемент
            //        CalcDataGrid.SelectedItem = CurrentItem;
            //    }
            //}
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

        private int IsCellEditing(DataGrid dataGrid)
        {
            if (dataGrid.CurrentCell.Column == null)
            {
                return 2;
            }
            if (dataGrid.CurrentCell != null)
            {
                var cell = dataGrid.CurrentCell.Column.GetCellContent(dataGrid.CurrentCell.Item).Parent as DataGridCell;
                bool value = (bool)cell.Tag;
                if(value) 
                {
                    return 0;
                }                    
            }
            return 1;
        }

        private void CalcDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var items = CalcDataGrid.SelectedItems;

            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            };

            if (e.Key == Key.Delete && !chapterName_textBox.IsFocused) // Проверяем, нажата ли клавиша Delete
            {
                if (isDependencySelected)
                {
                    deleteDependency_button_Click(sender, e);
                    e.Handled = true;
                }
                else
                {
                    if (isCalcOpened && IsCellEditing(CalcDataGrid) == 1)
                    {
                        CalcDeleteSelectedProduct_button_Click(sender, e);
                        e.Handled = true;

                    }

                    if (!isCalcOpened && IsCellEditing(dataBaseGrid) == 1)
                    {
                        deleteSelectedProduct_button_Click(sender, e);
                        e.Handled = true;
                    }

                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C && IsCellEditing(CalcDataGrid) == 1)
                {
                    CopyCalc_Click(this, e);
                    //e.Handled = true;
                }
                else if (e.Key == Key.X && IsCellEditing(CalcDataGrid) == 1)
                {
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
                    CalcController.Refresh(CalcDataGrid, calcItems);
                    //e.Handled = true; // Указываем, что событие обработано
                }
                else if (e.Key == Key.V && IsCellEditing(CalcDataGrid) == 1)
                {
                    //Извлекаем из буфера обмена
                    if (Clipboard.ContainsText())
                    {
                        try
                        {
                            if (isCalcOpened) //Если открыта расчётка то вставляем в расчётку
                            {
                                PasteCalc_Click(sender, e);
                                //e.Handled = true;
                            }
                            else //Если открыта не расчётка, то вставляем в основную бд, создав при этом новый элемент
                            {
                                string json = Clipboard.GetText();
                                var itemsToPaste = JsonSerializer.Deserialize<List<CalcProduct>>(json, options);
                                foreach (var item in itemsToPaste)
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
                                        if(!materialForDBAdding.Any(i => i == newMaterial))
                                        {
                                            materialForDBAdding.Add(newMaterial);
                                        }
                                        dbItems.Add(newMaterial);
                                        dataBaseGrid.Items.Refresh();
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                else if (e.Key == Key.Tab)
                {
                    priceCalcButton_Click(sender, e);
                }
                else if(e.Key == Key.A)
                {
                    if(isCalcOpened && CalcDataGrid.IsFocused)
                    {
                        CalcDataGrid.SelectAll();
                    }

                    if(!isCalcOpened && dataBaseGrid.IsFocused)
                    {
                        dataBaseGrid.SelectAll();
                    }
                }
                else if( e.Key == Key.F && !isCalcOpened) 
                {
                    e.Handled = true;
                    FastSearch fastSearch = new FastSearch(this);
                    Keyboard.ClearFocus();

                    // Перемещаем фокус на другой элемент (например, на окно)
                    fastSearch.ShowDialog();
                }
            }
        }

        private void ExportCalcToPrice_button_Click(object sender, RoutedEventArgs e)
        {
            bool isAdded = false;
            var items = CalcDataGrid.SelectedItems;

            List<CalcProduct> selectedItems = new List<CalcProduct>();

            // Перебираем выделенные элементы и добавляем их в список
            foreach (var item in items)
            {
                if (item is CalcProduct product) // Проверяем, является ли элемент CalcProduct
                {
                    selectedItems.Add(product); // Добавляем в список
                }
            }

            foreach (var item in selectedItems)
            {
                if (!dbItems.Any(i => i.Article == item.Article) && item.ProductName != string.Empty)
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

                    dbItems.Add(newMaterial);
                    if (!materialForDBAdding.Any(i => i == newMaterial))
                    {
                        materialForDBAdding.Add(newMaterial);
                    }
                    dataBaseGrid.Items.Refresh();
                    isAdded = true;
                }
            }

            if(isAdded) 
            {
                CalcInfo_label.Content = "Выбранные строки успешно перенесены в прайс.";
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
            else
            {
                CalcInfo_label.Content = "Выбранные строки не были перенесены в прайс.";
            }
        }

        private void saveDBChanges_button_Click(object sender, RoutedEventArgs e)
        {
            if(materialForDBAdding.Count > 0 || materialForDBUpdating.Count > 0 || materialForDBDeleting.Count > 0)
            {
                foreach (var item in materialForDBAdding)
                {
                    repository.Add_Material(item);
                }
                foreach (var item in materialForDBUpdating)
                {
                    repository.UpdateMaterial(item);
                }
                foreach (var item in materialForDBDeleting)
                {
                    repository.DeleteMaterial(item);
                }

                materialForDBAdding.Clear();
                materialForDBUpdating.Clear();
                materialForDBDeleting.Clear();

                PriceInfo_label.Content = "Изменения в прайс внесены успешно.";
            }
            else
            {
                WarningFlashing("Для начала внесите изменения!", WarningBorder, WarningLabel, Colors.OrangeRed, 2.5);
                PriceInfo_label.Content = "Изменения не были внесены в прайс, так как изменений нет.";
            }
        }

        private void dataBaseGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            Material selectedItem = (Material)dataBaseGrid.SelectedItem;
            if(!materialForDBUpdating.Any(i => i == selectedItem))
            {
                materialForDBUpdating.Add(selectedItem);
            }
        }

        private void dataBaseGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Material selectedItem = (Material)dataBaseGrid.SelectedItem;
            if (!materialForDBUpdating.Any(i => i == selectedItem))
            {
                materialForDBUpdating.Add(selectedItem);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (materialForDBAdding.Count != 0 || materialForDBDeleting.Count != 0 || materialForDBUpdating.Count != 0)
            {
                MessageBoxResult res = MessageBox.Show("Вы уверены, что хотите выйти без сохранения Базы Данных?", "База Данных не сохранена", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            fileImporter.ExportSettingsOnFile(this);
        }

        private void Calc_button_Click(object sender, RoutedEventArgs e)
        {
            CalcController.Calculation(this);

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
                            double discount = item.Cost * selectedCountry.discount / 100; //Скидка
                            item.Cost = Math.Round(item.Cost - discount, 2); //Цена со скидкой
                        }
                    }
                }
            }

            MovingLabel.Visibility = Visibility.Hidden;
            CalcController.Refresh(CalcDataGrid, calcItems);
            isCalculationNeed = false;
        }

        private void AddDependency_button_Click(object sender, RoutedEventArgs e)
        {
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
            if(selectedItem != null)
            {
                if (selectedItem.ID != 0)
                {
                    DependencyImage.Visibility = Visibility.Hidden;
                    DependencyDataGrid.Visibility = Visibility.Visible;
                    DependencyButtons.Visibility = Visibility.Visible;
                    DependencyDataGrid.ItemsSource = selectedItem.dependencies;
                    selectedItem.isDependency = true;
                    CalcController.Refresh(CalcDataGrid, calcItems);
                }
            }
        }

        private void DeleteDependency_button_Click_1(object sender, RoutedEventArgs e)
        {
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                if (selectedItem.ID != 0)
                {
                    if (isAddtoDependency)
                    {
                        startStopAddingDependency_button_Click(sender, e);
                        DependencyImage.Visibility = Visibility.Visible;
                        DependencyDataGrid.Visibility = Visibility.Hidden;
                        DependencyButtons.Visibility = Visibility.Hidden;
                        selectItemForDependencies.isDependency = false;
                        selectItemForDependencies.dependencies.Clear();
                    }
                    else
                    {
                        DependencyImage.Visibility = Visibility.Visible;
                        DependencyDataGrid.Visibility = Visibility.Hidden;
                        DependencyButtons.Visibility = Visibility.Hidden;
                        selectedItem.isDependency = false;
                        selectedItem.dependencies.Clear();
                    }
                    CalcController.Refresh(CalcDataGrid, calcItems);
                }
            }
        }

        private void CopyCalc_Click(object sender, RoutedEventArgs e)
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
            CalcInfo_label.Content = "Выбранные элементы скопированы в буфер.";
            isCalcSaved = false;
            //e.Handled = true; // Указываем, что событие обработано
        }

        private void PasteCalc_Click(object sender, RoutedEventArgs e)
        {
            CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
            if(selectedItem != null) 
            {
                if(selectedItem.ID != 0 && selectedItem.Manufacturer != string.Empty)
                {
                    var options = new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    };

                    string json = Clipboard.GetText();
                    var itemsToPaste = JsonSerializer.Deserialize<List<CalcProduct>>(json, options);

                    if (itemsToPaste != null)
                    {
                        for (int i = 0; i < itemsToPaste.Count; i++)
                        {
                            int MaxId = calcItems.Max(i => i.ID);
                            itemsToPaste[i].ID = MaxId + 1;
                            calcItems.Insert(CalcDataGrid.SelectedIndex + 1, itemsToPaste[i]);
                        }
                        CalcController.Refresh(CalcDataGrid, calcItems);
                        CalcInfo_label.Content = "Элементы вставлены из буфера.";
                        isCalcSaved = false;
                        //e.Handled = true; // Указываем, что событие обработано
                    }
                }
            }
        }

        //private void CalcDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        // Получаем элемент по позиции курсора
        //        var mousePosition = e.GetPosition(CalcDataGrid);
        //        var hitTestResult = VisualTreeHelper.HitTest(CalcDataGrid, mousePosition);

        //        if (hitTestResult != null)
        //        {
        //            var cell = hitTestResult.VisualHit;

        //            // Ищем ячейку DataGrid
        //            while (cell != null && !(cell is DataGridCell))
        //            {
        //                cell = VisualTreeHelper.GetParent(cell);
        //            }

        //            if (cell is DataGridCell dataGridCell)
        //            {
        //                // Получаем контекст данных ячейки
        //                CalcProduct selectedItem = (CalcProduct)dataGridCell.DataContext;

        //                // Проверяем, является ли это двойным кликом
        //                if (e.ClickCount == 2)
        //                {
        //                    // Начинаем редактирование ячейки
        //                    // Используем BeginEdit() для корректной работы с фокусом
        //                    CalcDataGrid.BeginEdit();
        //                    return; // Выходим из метода, чтобы не выполнять выделение
        //                }

        //                // Если Ctrl удерживается, добавляем или убираем элемент из выделенных
        //                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        //                {
        //                    if (CalcDataGrid.SelectedItems.Contains(selectedItem))
        //                    {
        //                        CalcDataGrid.SelectedItems.Remove(selectedItem);
        //                    }
        //                    else
        //                    {
        //                        CalcDataGrid.SelectedItems.Add(selectedItem);
        //                    }
        //                }
        //                else
        //                {
        //                    // Если Ctrl не удерживается, сбрасываем выделение и выделяем только текущий элемент
        //                    CalcDataGrid.SelectedItems.Clear();
        //                    CalcDataGrid.SelectedItems.Add(selectedItem);
        //                }

        //                CurrentItem = selectedItem;
        //            }
        //        }
        //    }
        //}

        private void startStopAddingDependency_button_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddtoDependency) //Если добавление начинается сейчас
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem; //Выбранный элемент
                if (selectedItem != null)
                {
                    if (selectedItem.ID == 0) //Если раздел
                    {
                        return;
                    }
                    CalcController.UpdateCellStyle(CalcDataGrid, Brushes.MediumSeaGreen, Brushes.White); //Теперь при выборе цвет становится салатовым
                    CalcDataGrid.SelectedItem = null; //Выделенный элемент убирается
                    selectedItem.RowColor = CalcController.ColorToHex(Colors.CornflowerBlue); //Выбранный элемент становится зелёным, чтобы было видно какому элементу добавляются зависимости
                    selectedItem.RowForegroundColor = CalcController.ColorToHex(Colors.White); //Цвет текста у выделенного элемента
                    CalcDataGrid.Items.Refresh();
                    selectItemForDependencies = selectedItem; //Запоминаем выделенный элемент
                    isAddtoDependency = true;
                    DependencyDataGrid.ItemsSource = selectedItem.dependencies;

                    if(selectItemForDependencies.dependencies.Count > 0) 
                    {
                        foreach (var dependency in selectItemForDependencies.dependencies) //Отображение всех зависимостей
                        {
                            CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ID == dependency.ProductId);
                            if (foundProduct != null)
                            {
                                foundProduct.RowColor = CalcController.ColorToHex(Colors.MediumSeaGreen);
                                foundProduct.RowForegroundColor = CalcController.ColorToHex(Colors.White);
                            }
                        }
                    }


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
                CalcController.UpdateCellStyle(CalcDataGrid, Brushes.CornflowerBlue, Brushes.White);
                selectItemForDependencies.RowColor = CalcController.ColorToHex(Colors.Transparent);
                selectItemForDependencies.RowForegroundColor = CalcController.ColorToHex(Colors.Gray);
                CalcDataGrid.SelectedItem = selectItemForDependencies;
                CalcController.Refresh(CalcDataGrid, calcItems);
                startStopAddingDependency_button.Background = Brushes.MediumSeaGreen;
                startStopAddingDependency_image.Source = new BitmapImage(new Uri("resources/images/play.png", UriKind.Relative));
                startStopAddingDependency_image.ToolTip = "Начать добавление зависимостей";
            }

        }

        private void DependencyDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            isDependencySelected = true;
        }

        private void CalcDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Цена" || e.Column.Header.ToString() == "Количество")
            {
                CalcProduct selectedItem = (CalcProduct)CalcDataGrid.SelectedItem;
                if(selectedItem != null) 
                {
                    if(selectedItem.isDependency)
                    {
                        var result = MessageBox.Show("Вы точно хотите установить значение количества для строки с зависимостью. Это действие удалит зависимость", "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if(result == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    selectedItem.isDependency = false;
                    selectedItem.dependencies.Clear();

                    string newTex = ((TextBox)e.EditingElement).Text;

                    if (newTex == "0")
                    {
                        MessageBox.Show("Невозмонжно установить 0 в качестве количества");
                        ((TextBox)e.EditingElement).Text = "1";
                    }
                    else if (string.IsNullOrWhiteSpace(newTex)) //Если текст пустой
                    {
                        ((TextBox)e.EditingElement).Text = "1";
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
                            ((TextBox)e.EditingElement).Text = "1";
                        }
                        else
                        {
                            ((TextBox)e.EditingElement).Text = validNumber;
                        }
                    }
                }
            }            
        }

        private void ExportAllCalcToPrice_button_Click(object sender, RoutedEventArgs e)
        {
            bool isAdded = false;

            List<CalcProduct> items = calcItems.ToList();

            foreach (var item in items)
            {
                if (!dbItems.Any(i => i.Article == item.Article) && item.ProductName != string.Empty)
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

                    dbItems.Add(newMaterial);
                    if (!materialForDBAdding.Any(i => i == newMaterial))
                    {
                        materialForDBAdding.Add(newMaterial);
                        isAdded = true;
                    }
                    dataBaseGrid.Items.Refresh();
                }
            }

            if (isAdded)
            {
                CalcInfo_label.Content = "Все строки успешно перенесены в прайс.";
                productsCount_label.Content = "из " + dataBaseGrid.Items.Count.ToString();
            }
            else
            {
                CalcInfo_label.Content = "Все строки не были перенесены в прайс.";
            }
        }
    }
}
