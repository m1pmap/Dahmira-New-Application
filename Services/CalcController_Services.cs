using Dahmira.DAL.Model;
using Dahmira.Interfaces;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Dahmira.Services
{
    internal class CalcController_Services : ICalcController
    {
        public void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems) //Обновление расчётки
        {
            ICalcController CalcController = new CalcController_Services();
            double fullCost = 0;
            int chapterCount = 0;
            bool isNowAddToDependencies = false;
            CalcProduct selectFordependency = null;

            for (int i = 0; i < calcItems.Count - 1; i++) //Перебор всех элементов
            {
                CalcProduct item = calcItems[i];
                double count = Convert.ToDouble(item.Count);
                if(item.isDependency == true)
                {
                    count = 0;
                }

                //Перечисление всех зависимостей и получение результата
                foreach (var dep in item.dependencies)
                {
                    CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ID == dep.ProductId);
                    if(foundProduct != null)
                    {
                        switch (dep.SelectedType)
                        {
                            case "*":
                                {
                                    count += Convert.ToDouble(foundProduct.Count) * dep.Multiplier;
                                    break;
                                }
                            case "+":
                                {
                                    count += Convert.ToDouble(foundProduct.Count) + dep.Multiplier;
                                    break;
                                }
                            case "-":
                            {
                                count += Convert.ToDouble(foundProduct.Count) - dep.Multiplier;
                                break;
                            }
                            case "/":
                            {
                                count += Convert.ToDouble(foundProduct.Count) / dep.Multiplier;
                                break;
                            }
                        }
                    }
                }

                item.Count = Math.Round(count, 2).ToString();

                item.TotalCost = Math.Round(item.Cost * Convert.ToDouble(item.Count), 2); //Обновление итоговой цены
                if (item.Num != i + 1) //Если номер идёт не по порядку
                {
                    if (item.Num == 0) //Если это раздел
                    {
                        chapterCount++;
                    }
                    else
                    {
                        item.Num = i + 1 - chapterCount;
                    }
                }
                else
                {
                    item.Num = i + 1 - chapterCount;
                }

                if(item.TotalCost > 0) //Если цена положительная
                {
                    fullCost += item.TotalCost;
                }

                if (item.ProductName == string.Empty && item.ID == 0) //Если это раздел, то изменяем ему фон на желтый
                {
                    item.RowColor = ColorToHex(Colors.LightBlue);
                    item.RowForegroundColor = ColorToHex(Colors.Black);
                }
                else
                {
                    if (item.RowColor == ColorToHex(Colors.Coral))
                    {
                        item.RowColor = ColorToHex(Colors.Coral);
                        item.RowForegroundColor = ColorToHex(Colors.White);
                    }
                    else
                    {
                        if (item.RowColor == ColorToHex(Colors.OrangeRed)) //Если элемента нет в бд, то оставляем цвет красным
                        {
                            item.RowColor = ColorToHex(Colors.OrangeRed);
                            item.RowForegroundColor = ColorToHex(Colors.White);
                        }
                        else
                        {
                            if (item.RowColor == ColorToHex(Colors.CornflowerBlue)) //Если цвет Синий, то оставляем цвет таким же
                            {
                                item.RowColor = ColorToHex(Colors.CornflowerBlue);
                                item.RowForegroundColor = ColorToHex(Colors.White);
                                selectFordependency = item;
                                isNowAddToDependencies = true;
                            }
                            else //Иначе делаем все поля прозрачными с серым цветом
                            {
                                item.RowColor = ColorToHex(Colors.Transparent);
                                item.RowForegroundColor = ColorToHex(Colors.Black);
                            }
                        }
                    }
                }
                
            }

            var selectedItem = (CalcProduct)CalcGrid.SelectedItem; //Получаем выбранный элемент
            if(selectedItem != null)
            {
                if(isNowAddToDependencies) //Если сейчас идёт добавление
                {
                    selectedItem = selectFordependency; //Меняем выбранный элемент на то, в который идёт добавление
                }
                foreach (var dependency in selectedItem.dependencies) //Отображение всех зависимостей
                {
                    CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ID == dependency.ProductId);
                    if(foundProduct != null)
                    {
                        foundProduct.RowColor = ColorToHex(Colors.MediumSeaGreen);
                        foundProduct.RowForegroundColor = ColorToHex(Colors.White);
                    }
                }
            }


            calcItems[calcItems.Count - 1].TotalCost = Math.Round(fullCost, 2);
            CalcGrid.CommitEdit();
            CalcGrid.Items.Refresh();
        }
        public bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, MainWindow window, string count = "1", string position = "Last") //Добавление в расчётку товара
        {
            try
            {
                if(window.calcItems.Count > 1)
                {
                    Material selectedDBItem = (Material)DBGrid.SelectedItem; //Текущий выбранный элемент в БД
                    int selectedCalcItemIndex = CalcGrid.SelectedIndex; //Индекс текущего выбранного элемента в расчётке

                    int maxId = 0;

                    foreach(var item in window.calcItems)
                    {
                        if(maxId < item.ID)
                        {
                            maxId = item.ID;
                        }
                    }

                    //Создание нового элемента расчётки
                    CalcProduct newCalcProductItem = new()
                    {
                        ID = maxId + 1,
                        Num = window.calcItems.Count + 1,
                        Manufacturer = selectedDBItem.Manufacturer,
                        ProductName = selectedDBItem.ProductName,
                        Article = selectedDBItem.Article,
                        Unit = selectedDBItem.Unit,
                        Photo = selectedDBItem.Photo,
                        RealCost = Math.Round(selectedDBItem.Cost, 2),
                        Cost = Math.Round(selectedDBItem.Cost, 2),
                        Count = count,
                        TotalCost = Math.Round(selectedDBItem.Cost, 2),
                        Note = ""
                    };

                    switch (position) //В зависимости от выбранной позиции для добавления
                    {
                        case "Last": //В конец
                            {
                                window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1 } прайса добавлена под {window.calcItems.Count - 1} в расчёте.";
                                window.calcItems.Insert(window.calcItems.Count - 1, newCalcProductItem);
                                window.WarningFlashing("Добавлено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);
                                break;
                            }
                        case "UnderSelect": //Под выбранным
                            {

                                if (CalcGrid.SelectedItem == null)
                                {
                                    window.PriceInfo_label.Content = $"Строка прайса не добавлена в расчёт. Для начала выберите строку в расчёте.";
                                    break;
                                }
                                window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1} прайса добавлена под {CalcGrid.SelectedIndex + 1} в расчёте.";
                                if (CalcGrid.SelectedIndex != window.calcItems.Count - 1)
                                {
                                    window.calcItems.Insert(selectedCalcItemIndex + 1, newCalcProductItem);
                                }
                                else
                                {
                                    window.calcItems.Insert(selectedCalcItemIndex, newCalcProductItem);
                                }
                                window.WarningFlashing("Добавлено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);
                                break;
                            }
                        case "Replace": //Заменить
                            {
                                if (CalcGrid.SelectedItem == null)
                                {
                                    window.PriceInfo_label.Content = $"Строка не заменена. Для начала выберите строку в расчёте.";
                                    break;
                                }
                                if (CalcGrid.SelectedIndex != window.calcItems.Count - 1)
                                {
                                    window.PriceInfo_label.Content = $"Строка {DBGrid.SelectedIndex + 1} прайса заменила {CalcGrid.SelectedIndex + 1} в расчёте.";
                                    window.calcItems[selectedCalcItemIndex] = newCalcProductItem;
                                    window.WarningFlashing("Добавлено!", window.WarningBorder, window.WarningLabel, Colors.MediumSeaGreen, 1);
                                }
                                break;
                            }
                    }

                    Refresh(CalcGrid, window.calcItems); //Обновление
                    window.isCalcSaved = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }            
        }

        public void ObjectFlashing(Border target, Color initialColor, Color flashingColor, double interval) //Анимация мигания выбранной кнопки и выбранными цветами
        {
            // Создаем анимацию
            var storyboard = new Storyboard();

            // Создаем анимацию цвета фона
            ColorAnimation colorAnimation = new ColorAnimation
            {
                From = initialColor,
                To = flashingColor,
                Duration = new Duration(TimeSpan.FromMilliseconds(750)),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(interval) // Количество миганий
            };

            // Применяем анимацию к фону кнопки
            Storyboard.SetTarget(colorAnimation, target);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Background.Color"));

            // Добавляем анимацию в storyboard
            storyboard.Children.Add(colorAnimation);

            // Запускаем анимацию
            storyboard.Begin();
        }

        public string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public Color HexToColor(string hex)
        {
            // Удаляем символ '#' если он есть
            hex = hex.Replace("#", "");

            // Если длина строки 6, добавляем 2 символа для альфа-канала (полностью непрозрачный)
            if (hex.Length == 6)
            {
                hex = "FF" + hex;
            }

            // Преобразуем HEX в ARGB
            byte a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        void ICalcController.UpdateCellStyle(DataGrid dataGrid, Brush backgroundColor, Brush foregroundColor)
        {
            Style oldCellStyle = dataGrid.CellStyle;

            // Создаем новый стиль, наследуя старый
            Style newCellStyle = new Style(typeof(DataGridCell), oldCellStyle);

            // Добавляем триггер для выделенных ячеек
            Trigger selectedTrigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };

            // Устанавливаем фон и цвет текста для выделенной ячейки
            selectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, backgroundColor));
            selectedTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, foregroundColor));

            // Добавляем триггер в новый стиль
            newCellStyle.Triggers.Add(selectedTrigger);

            // Применяем новый стиль к DataGrid
            dataGrid.CellStyle = newCellStyle;
        }

        public bool ArePhotosEqual(byte[] photo1, byte[] photo2) //Равны ли 2 фото, представленные в массиве байтов
        {
            if (photo1 == null && photo2 == null) //Если оба фото не указаны 
            {
                return true;
            }

            if (photo1 == null || photo2 == null) //Одно из фото не указано
            {
                return false;
            }

            if (photo1.Length != photo2.Length) //Если у фото разная длина
            {
                return false;
            }

            for (int i = 0; i < photo1.Length; i++) 
            {
                if (photo1[i] != photo2[i]) //Если у фото разное содержимое
                {
                    return false;
                }
            }

            return true;
        }

        void ICalcController.CheckingDifferencesWithDB(DataGrid CalcDataGrid, MainWindow window) //Проверка идентичности данных в БД и расчетке
        {
            bool isDifferent = false; //Указвает на присутствие отличий в фото
            //bool isRemovedOnDB = false; //Указвает на то, есть ли элементы, которых нет в бд

            ClearBackgroundsColors(window);

            foreach (var item in window.calcItems)
            {
                if (item.ProductName != string.Empty) //Если не раздел
                {
                    Material dbMaterial = window.dbItems.FirstOrDefault(i => i.Article == item.Article); //Проверяем наличие этого элемента в БД
                    if (dbMaterial != null) //Если элемент есть
                    {
                        if (!ArePhotosEqual(item.Photo, dbMaterial.Photo)) //Если фото не равны
                        {
                            item.Photo = dbMaterial.Photo;
                            isDifferent = true;
                        }
                        if(item.ProductName != dbMaterial.ProductName || item.Manufacturer != dbMaterial.Manufacturer)
                        {
                            item.RowColor = ColorToHex(Colors.LightGray);
                            item.RowForegroundColor = ColorToHex(Colors.White);

                            CalcDataGrid.Items.Refresh();
                            MessageBoxResult result = MessageBox.Show("В расчётке:" +
                                                                      "\nПроизводитель: " + item.Manufacturer + 
                                                                      "\nНаименование: " + item.ProductName + 
                                                                      "\n\nВ Базе Данных:\nПроизводитель: " + dbMaterial.Manufacturer + 
                                                                      "\nНаименование: " + dbMaterial.ProductName + 
                                                                      "\n\nЗаменить в расчётке эти данные или оставить как есть?\nВ любом случае он будет подсвечен оранжевым",
                                                                      "Несоответствие с Базой Данных", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if(result == MessageBoxResult.Yes) 
                            {
                                item.Manufacturer = dbMaterial.Manufacturer;
                                item.ProductName = dbMaterial.ProductName;
                            }
                            isDifferent = true;
                        }
                        else //Если производитель и наименование равно
                        {
                            item.RowColor = ColorToHex(Colors.Transparent);
                            item.RowForegroundColor = ColorToHex(Colors.Black);
                        }
                    }
                    else //Если элемента нет
                    {
                        item.RowColor = ColorToHex(Colors.OrangeRed);
                        item.RowForegroundColor = ColorToHex(Colors.White);
                        CalcDataGrid.Items.Refresh();
                        MessageBox.Show("Артикула нет в прайсе!" +
                                        "\nОтсутствующий артикул подсвечен красным." +
                                        "\nНомер элемента: " + item.Num.ToString() +
                                        "\nПроизводитель: " + item.Manufacturer + 
                                        "\nНаименование: " + item.ProductName + 
                                        "\nАртикул " + item.Article, "Товар отсутствует в Базе Данных", MessageBoxButton.OK, MessageBoxImage.Error);;
                        isDifferent = true;
                    }
                }
                CalcDataGrid.Items.Refresh();
                window.CalcInfo_label.Content = "Нарушено соответствие с Базой Данных.";
            }

            if (!isDifferent) 
            {
                MessageBox.Show("Соответствие с Базой Данных не нарушена", "", MessageBoxButton.OK, MessageBoxImage.Information);
                window.CalcInfo_label.Content = "Соответствие с Базой Данных не нарушена.";
            }
        }

        public void Calculation(MainWindow window)
        {
            foreach (var item in window.calcItems)
            {
                if (item.ProductName != string.Empty) //Если не раздел
                {
                    if(window.dbItems.Any(i => i.Article == item.Article))
                    {
                        Material material = window.dbItems.First(i => i.Article == item.Article);
                        if (material.Cost != item.RealCost)
                        {
                            item.RealCost = material.Cost;
                        }
                    }
                }
            }
        }

        public void ClearBackgroundsColors(MainWindow window)
        {
            foreach(var item in window.calcItems)
            {
                if(item.RowColor != ColorToHex(Colors.Transparent) && item.RowColor != ColorToHex(Colors.LightBlue))
                {
                    item.RowColor = ColorToHex(Colors.Transparent);
                    item.RowForegroundColor = ColorToHex(Colors.Black);
                }
            }
        }
    }
}
