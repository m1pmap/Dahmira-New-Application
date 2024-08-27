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
            double fullCost = 0;
            int chapterCount = 0;
            for (int i = 0; i < calcItems.Count; i++) //Перебор всех элементов
            {
                CalcProduct item = calcItems[i];
                item.TotalCost = Math.Round(item.Cost * item.Count, 2); //Обновление итоговой цены
                if (item.Num != i + 1) //Если номер идёт не по порядку
                {
                    if (item.Num == 0)
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

                if(item.TotalCost > 0)
                {
                    fullCost += item.TotalCost;
                }
            }
            CalcGrid.CommitEdit();
            fullCost_label.Content = fullCost;
            CalcGrid.Items.Refresh();
        }
        public bool AddToCalc(DataGrid DBGrid, DataGrid CalcGrid, ObservableCollection<CalcProduct> calcItems, Label fullCost_label, int count = 1, string position = "Last")
        {
            try
            {
                if(calcItems.Count != 0)
                {
                    TestData selectedDBItem = (TestData)DBGrid.SelectedItem; //Текущий выбранный элемент в БД
                    int selectedCalcItemIndex = CalcGrid.SelectedIndex; //Индекс текущего выбранного элемента в расчётке

                    //Создание нового элемента расчётки
                    CalcProduct newCalcProductItem = new()
                    {
                        Num = calcItems.Count + 1,
                        Manufacturer = selectedDBItem.Manufacturer,
                        ProductName = selectedDBItem.ProductName,
                        Article = selectedDBItem.Article,
                        Unit = selectedDBItem.Unit,
                        Photo = selectedDBItem.Photo,
                        RealCost = selectedDBItem.Cost,
                        Cost = selectedDBItem.Cost,
                        Count = count,
                        TotalCost = selectedDBItem.Cost,
                        Note = ""
                    };

                    switch (position) //В зависимости от выбранной позиции для добавления
                    {
                        case "Last": //В конец
                            {
                                calcItems.Add(newCalcProductItem);
                                break;
                            }
                        case "UnderSelect": //Под выбранным
                            {
                                calcItems.Insert(selectedCalcItemIndex + 1, newCalcProductItem);
                                break;
                            }
                        case "Replace": //Заменить
                            {
                                calcItems[selectedCalcItemIndex] = newCalcProductItem;
                                break;
                            }
                    }

                    Refresh(CalcGrid, calcItems, fullCost_label); //Обновление
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

        public void ObjectFlashing(Button target, Color initialColor, Color flashingColor)
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
    }
}
