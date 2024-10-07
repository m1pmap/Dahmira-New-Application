using Dahmira.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для FastSearch.xaml
    /// </summary>
    public partial class FastSearch : Window
    {
        int lastIndex = 0;

        bool isManufacturerLast = true;
        bool isProductNameLast = false;
        bool isArticleLast = false;
        bool isUnitLast = false;

        MainWindow window;

        public FastSearch(MainWindow w)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window = w;
             // Начинаем с следующего элемента
        }

        private void Search_button_Click(object sender, RoutedEventArgs e)
        {
            Search_button.Content = "Перейти к следующему";
            string pattern = Regex.Escape(searchingText.Text.ToString());

            // Проверяем наличие текста для поиска
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return;
            }

            // Перебор по столбцам
            if (isManufacturerLast)
            {
                if (SearchInColumn(item => item.Manufacturer, pattern, 0))
                    return;

                // Если не найдено, переходим к следующему столбцу
                isManufacturerLast = false;
                lastIndex = 0; // Сбрасываем индекс для следующего столбца
                isProductNameLast = true;
            }

            if (isProductNameLast)
            {
                if (SearchInColumn(item => item.ProductName, pattern, 1))
                    return;

                // Если не найдено, переходим к следующему столбцу
                isProductNameLast = false;
                lastIndex = 0; // Сбрасываем индекс для следующего столбца
                isArticleLast = true;
            }

            if (isArticleLast)
            {
                if (SearchInColumn(item => item.Article, pattern, 2))
                    return;

                // Если не найдено, переходим к следующему столбцу
                isArticleLast = false;
                lastIndex = 0; // Сбрасываем индекс для следующего столбца
                isUnitLast = true;
            }

            if (isUnitLast)
            {
                if (SearchInColumn(item => item.Unit, pattern, 3))
                    return;

                // Если не найдено, начинаем сначала
                isUnitLast = false; // Завершили поиск во всех столбцах
                lastIndex = 0; // Сбрасываем индекс для следующего цикла
                isManufacturerLast = true; // Начинаем снова с первого столбца

                // Если все столбцы проверены и ничего не найдено
                Search_button.Content = "Начать поиск сначала";
            }
        }

        // Метод для поиска в заданном столбце
        private bool SearchInColumn(Func<Material, string> selector, string pattern, int columnIndex)
        {
            for (int i = lastIndex; i < window.dbItems.Count; i++)
            {
                Material item = window.dbItems[i];
                FocusManager.SetFocusedElement(window, null);

                if (Regex.IsMatch(selector(item), pattern, RegexOptions.IgnoreCase))
                {
                    window.dataBaseGrid.SelectedItem = item;
                    lastIndex = i + 1;

                    // Обновляем макет
                    window.dataBaseGrid.UpdateLayout();

                    var row = window.dataBaseGrid.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;

                    if (row != null)
                    {
                        // Получаем нужный столбец
                        var cell = window.dataBaseGrid.Columns[columnIndex].GetCellContent(row).Parent as DataGridCell;
                        if (cell != null)
                        {
                            // Устанавливаем фокус на ячейку
                            FocusManager.SetFocusedElement(window.dataBaseGrid, cell);
                            cell.Focus();
                        }
                    }

                    // Устанавливаем фокус на PriceInfo_label
                    window.PriceInfo_label.Focus();
                    return true;
                }
            }

            return false;
        }

        private void searchingText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                lastIndex = window.dataBaseGrid.SelectedIndex + 1;
                var currentCell = window.dataBaseGrid.CurrentCell;
                int columnIndex = currentCell.Column.DisplayIndex;
                if (columnIndex == 1)
                {
                    isManufacturerLast = false;
                    isProductNameLast = true;
                    isArticleLast = false;
                    isUnitLast = false;
                }
                else if (columnIndex == 2)
                {
                    isManufacturerLast = false;
                    isProductNameLast = false;
                    isArticleLast = true;
                    isUnitLast = false;
                }
                else if (columnIndex == 3)
                {
                    isManufacturerLast = false;
                    isProductNameLast = false;
                    isArticleLast = false;
                    isUnitLast = true;
                }
                else
                {
                    isManufacturerLast = true;
                    isProductNameLast = false;
                    isArticleLast = false;
                    isUnitLast = false;
                }
            }
            else
            {
                isManufacturerLast = true;
                isProductNameLast = false;
                isArticleLast = false;
                isUnitLast = false;

                lastIndex = 0;
            }

            Search_button.Content = "Поиск";
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                lastIndex = window.dataBaseGrid.SelectedIndex + 1;
                var currentCell = window.dataBaseGrid.CurrentCell;
                int columnIndex = currentCell.Column.DisplayIndex;
                if(columnIndex == 1)
                {
                    isManufacturerLast = false;
                    isProductNameLast = true;
                    isArticleLast = false;
                    isUnitLast = false;
                }
                else if (columnIndex == 2)
                {
                    isManufacturerLast = false;
                    isProductNameLast = false;
                    isArticleLast = true;
                    isUnitLast = false;
                }
                else if (columnIndex == 3)
                {
                    isManufacturerLast = false;
                    isProductNameLast = false;
                    isArticleLast = false;
                    isUnitLast = true;
                }
                else
                {
                    isManufacturerLast = true;
                    isProductNameLast = false;
                    isArticleLast = false;
                    isUnitLast = false;
                }
            }
            
            if(checkBox.IsChecked == false)
            {
                isManufacturerLast = true;
                isProductNameLast = false;
                isArticleLast = false;
                isUnitLast = false;

                lastIndex = 0;
            }
        }
    }
}
