using Dahmira.DAL.Model;
using Dahmira.Interfaces;
using Dahmira.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Dahmira.Services
{
    internal class CalcController_Services : ICalcController
    {
        public void Refresh(DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems, Label fullCost_label) //Обновление расчётки
        {
            ICalcController CalcController = new CalcController_Services();
            double fullCost = 0;
            int chapterCount = 0;
            bool isNowAddToDependencies = false;
            CalcProduct selectFordependency = null;

            for (int i = 0; i < calcItems.Count; i++) //Перебор всех элементов
            {
                double count = 0;
                CalcProduct item = calcItems[i];

                if(item.dependencies.Count == 0) 
                {
                    item.isDependency = false;
                    count = item.Count;
                }
                else
                {
                    //Перечисление всех зависимостей и получение результата
                    foreach (var dep in item.dependencies)
                    {
                        CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ProductName == dep.ProductName);
                        if(foundProduct != null)
                        {
                            switch (dep.SelectedType)
                            {
                                case "*":
                                    {
                                        count += foundProduct.Count * dep.Multiplier;
                                        break;
                                    }
                                case "+":
                                    {
                                        count += foundProduct.Count + dep.Multiplier;
                                        break;
                                    }
                                case "-":
                                    {
                                        count += foundProduct.Count - dep.Multiplier;
                                        break;
                                    }
                                case "/":
                                    {
                                        count += foundProduct.Count / dep.Multiplier;
                                        break;
                                    }
                            }
                        }
                    }
                }

                item.Count = Math.Round(count, 2);

                item.TotalCost = Math.Round(item.Cost * item.Count, 2); //Обновление итоговой цены
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

                if (item.ProductName == string.Empty) //Если это раздел, то изменяем ему фон на желтый
                {
                    item.RowColor = ColorToHex(Colors.LightYellow);
                    item.RowForegroundColor = ColorToHex(Colors.Gray);
                }
                else
                {
                    if (!item.isInDB) //Если элемента нет в бд, то оставляем цвет красным
                    {
                        item.RowColor = ColorToHex(Colors.OrangeRed);
                        item.RowForegroundColor = ColorToHex(Colors.White);
                    }
                    else
                    {
                        if (item.RowColor == ColorToHex(Colors.MediumSeaGreen)) //Если цвет зелёный, то оставляем цвет таким же
                        {
                            item.RowColor = ColorToHex(Colors.MediumSeaGreen);
                            item.RowForegroundColor = ColorToHex(Colors.White);
                            selectFordependency = item;
                            isNowAddToDependencies = true;
                        }
                        else //Иначе делаем все поля прозрачными с серым цветом
                        {
                            item.RowColor = ColorToHex(Colors.Transparent);
                            item.RowForegroundColor = ColorToHex(Colors.Gray);
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
                    CalcProduct foundProduct = calcItems.FirstOrDefault(p => p.ProductName == dependency.ProductName);
                    if(foundProduct != null)
                    {
                        foundProduct.RowColor = ColorToHex(Colors.LightGreen);
                        foundProduct.RowForegroundColor = ColorToHex(Colors.White);
                    }
                }
            }

            CalcGrid.CommitEdit();
            fullCost_label.Content = Math.Round(fullCost, 2);
            CalcGrid.Items.Refresh();
        }
        public bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, MainWindow window, Label fullCost_label, double count = 1, string position = "Last") //Добавление в расчётку товара
        {
            try
            {
                if(window.calcItems.Count != 0)
                {
                    Material selectedDBItem = (Material)DBGrid.SelectedItem; //Текущий выбранный элемент в БД
                    int selectedCalcItemIndex = CalcGrid.SelectedIndex; //Индекс текущего выбранного элемента в расчётке

                    //Проверка на то, добавлен ли уже этот товар в расчётку
                    foreach (var item in window.calcItems)
                    {
                        if (item.Article == selectedDBItem.Article)
                        {
                            MessageBox.Show("Этот товар уже добавлен в расчётку, поэтому его количество увеличено на " + count, "", MessageBoxButton.OK, MessageBoxImage.Information);
                            item.Count += count;
                            Refresh(CalcGrid, window.calcItems, fullCost_label);
                            return true;
                        }
                    }

                    //Создание нового элемента расчётки
                    CalcProduct newCalcProductItem = new()
                    {
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
                                window.calcItems.Add(newCalcProductItem);
                                break;
                            }
                        case "UnderSelect": //Под выбранным
                            {
                                window.calcItems.Insert(selectedCalcItemIndex + 1, newCalcProductItem);
                                break;
                            }
                        case "Replace": //Заменить
                            {
                                window.calcItems[selectedCalcItemIndex] = newCalcProductItem;
                                break;
                            }
                    }

                    Refresh(CalcGrid, window.calcItems, fullCost_label); //Обновление
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
                return true;
            }            
        }

        public void ObjectFlashing(Button target, Color initialColor, Color flashingColor) //Анимация мигания выбранной кнопки и выбранными цветами
        {
            // Создаем анимацию
            var storyboard = new Storyboard();

            // Создаем анимацию цвета фона
            ColorAnimation colorAnimation = new ColorAnimation
            {
                From = initialColor,
                To = flashingColor,
                Duration = new Duration(TimeSpan.FromMilliseconds(350)),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3) // Количество миганий
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

        bool ICalcController.CheckingDifferencesWithDB(DataGrid CalcDataGrid, MainWindow window) //Проверка идентичности данных в БД и расчетке
        {
            bool isDifferent = false; //Указвает на присутствие отличий в фото
            bool isRemovedOnDB = false; //Указвает на то, есть ли элементы, которых нет в бд

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
                            item.RowColor = ColorToHex(Colors.Coral);
                            item.RowForegroundColor = ColorToHex(Colors.White);
                            isDifferent = true;
                        }
                        else //Если фото равны
                        {
                            item.isInDB = true;
                            item.RowColor = ColorToHex(Colors.Transparent);
                            item.RowForegroundColor = ColorToHex(Colors.Gray);
                        }
                    }
                    else //Если элемента нет
                    {
                        item.isInDB = false;
                        item.RowColor = ColorToHex(Colors.OrangeRed);
                        item.RowForegroundColor = ColorToHex(Colors.White);
                        isRemovedOnDB = true;
                    }
                }
            }

            CalcDataGrid.Items.Refresh();

            if (isRemovedOnDB)
            {
                if(isDifferent) 
                {
                    MessageBox.Show("Красным выделены товары, которых нет в Базе Данных\nОранжевым - те, что имели несоответствие по фото с Базой Данных, но уже исправлены", "Несоответствие с Базой Данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Выделены товары, которых нет в Базе Данных", "Несоответствие с Базой Данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
            if (isDifferent)
            {
                MessageBox.Show("Выделеные товары, имеющие несоответствие по фото с Базой Данных, но уже исправлены", "Несоответствие с Базой Данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Соответствие с Базой Данных не нарушена", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return true;
        }
    }
}
