﻿using Dahmira.Interfaces;
using Dahmira.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Win32;
using Dahmira.Pages;
using System.Text.Json;
using System.Windows;
using System.Net;
using System.IO.Packaging;
using System.Text.Json.Serialization;

namespace Dahmira.Services
{
    public class FileImporter_Services : IFileImporter
    {
        private string url_praise = "ftp://31.177.95.187";
        private string ftpUsername = "dahmira1_admin";
        private string ftpPassword = "zI2Hghfnslob";
        void IFileImporter.ExportToExcel(MainWindow window) //Экспорт расчётки в Excel
        {
            try
            {
                bool isSaved  = false;

                ExcelPackage.LicenseContext = LicenseContext.Commercial; //Вид лицензии

                var package = new ExcelPackage(); //Создание нового документа
                var worksheet = package.Workbook.Worksheets.Add("Лист1");
                int lastColumnIndex = 10;

                if (window.settings.IsInsertExcelPicture == false) //Если фото не добавляется, то количество столбцов меньше
                {
                    lastColumnIndex = 9;
                }

                // Записываем заголовки столбцов
                for (int i = 0; i < lastColumnIndex; i++)
                {
                    if (i >= 5 && window.settings.IsInsertExcelPicture == false) //Если картинка не добавляется
                    {
                        worksheet.Cells[1, i + 1].Value = window.CalcDataGrid.Columns[i + 1].Header;
                    }
                    else
                    {
                        worksheet.Cells[1, i + 1].Value = window.CalcDataGrid.Columns[i].Header;
                    }

                }
                worksheet.Cells[1, lastColumnIndex].Value = "Примечания";
                worksheet.Cells[1, lastColumnIndex - 1].Value = "Сумма без НДС";
                worksheet.Cells[1, lastColumnIndex - 3].Value = "Цена без НДС";

                //Установка стилей для Header 
                ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.GetColor());

                //Установка стилей для всего рабочего пространства
                ExcelRange Rng = worksheet.Cells[1, 1, window.calcItems.Count + 2 - 1, lastColumnIndex];
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
                for (int i = 0; i < window.calcItems.Count - 1; i++)
                {
                    CalcProduct item = window.calcItems[i];

                    if (item.Photo == null) //Если фото равно нулю (Раздел)
                    {
                        //Стили для раздела
                        ExcelRange chapterRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                        chapterRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        chapterRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelChapterColor.GetColor());
                        chapterRange.Merge = true;
                        worksheet.Cells[i + 2, 1].Value = item.Manufacturer;
                        continue;
                    }

                    //Установка стилей всех данных
                    ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                    dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dataRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelDataColor.GetColor());

                    //Установка стилей для примечаний
                    ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                    notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    notesRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNotesColor.GetColor());

                    //Установка стилей для номера
                    ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                    numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    numberRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNumberColor.GetColor());

                    //Добавление данных в ячейки
                    worksheet.Cells[i + 2, 1].Value = item.Num;
                    worksheet.Cells[i + 2, 2].Value = item.Manufacturer;
                    worksheet.Cells[i + 2, 3].Value = item.ProductName;
                    worksheet.Cells[i + 2, 4].Value = item.Article;
                    worksheet.Cells[i + 2, 5].Value = item.Unit;

                    if (window.settings.IsInsertExcelPicture == true) //Если картинку надо добавить
                    {
                        //Установка стилей для фона фото 
                        ExcelRange photoRange = worksheet.Cells[i + 2, 6];
                        photoRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        photoRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelPhotoBackgroundColor.GetColor());

                        ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                        BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);

                        //Ширина и высота в зависимости от выбранных параметров в настройках
                        worksheet.Column(6).Width = (window.settings.ExcelPhotoWidth + 10) / 7;
                        worksheet.Rows[i + 2].Height = (window.settings.ExcelPhotoHeight + 10) / 1.33;

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
                            excelImage.SetSize(window.settings.ExcelPhotoWidth, window.settings.ExcelPhotoHeight);
                        }
                    }

                    //Добавление остальных данных в ячейки
                    worksheet.Cells[i + 2, lastColumnIndex - 3].Value = item.Cost;
                    worksheet.Cells[i + 2, lastColumnIndex - 2].Value = item.Count;
                    worksheet.Cells[i + 2, lastColumnIndex - 1].Value = item.TotalCost;
                    worksheet.Cells[i + 2, lastColumnIndex].Value = item.Note;
                }

                worksheet.Cells[window.calcItems.Count + 2 - 1, lastColumnIndex - 1].Value = window.settings.FullCostType + " " + window.calcItems[window.calcItems.Count - 1].TotalCost;

                ExcelRange countryRng = worksheet.Cells[window.calcItems.Count + 4 - 1, 1, window.calcItems.Count + 4 - 1, lastColumnIndex];
                countryRng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                countryRng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                countryRng.Style.WrapText = true;

                countryRng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                countryRng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                countryRng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                countryRng.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                countryRng.Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                countryRng.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                countryRng.Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                countryRng.Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                countryRng.Merge = true;
                Country selectedCountry = (Country)window.allCountries_comboBox.SelectedItem;
                worksheet.Cells[window.calcItems.Count + 4 - 1, 1].Value = $"Цена для: {selectedCountry.name}";

                //Диалоговое окно для сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Сохранить Excel документ",
                    InitialDirectory = window.settings.ExcelFolderPath
                };
                //Сохранение по выбранному пути
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    worksheet.Protection.IsProtected = false;
                    worksheet.Protection.AllowSelectLockedCells = false;
                    package.SaveAs(new FileInfo(filePath));
                    isSaved = true;
                }

                if (isSaved)
                {
                    window.CalcInfo_label.Content = "Расчёт успешно сохранён в Excel.";
                }
                else
                {
                    window.CalcInfo_label.Content = "Расчёт не сохранён в Excel.";
                }
            }
            catch (Exception exp) 
            {
                MessageBox.Show(exp.Message);
                window.CalcInfo_label.Content = "Расчёт не сохранён в Excel.";
            }
        }

        void IFileImporter.ExportToExcelAsNewSheet(MainWindow window) //Экспорт расчётки в Excel в качестве нового листа
        {
            try
            {
                bool isSaved = false;

                ExcelPackage.LicenseContext = LicenseContext.Commercial; //Лицензия
                //Диалоговое окно открытия файла
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Добавить в Excel документ",
                    InitialDirectory = window.settings.ExcelFolderPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    //Диалоговое окно для того чтобы узнать имя нового листа Excel
                    DialogPage dialogPage = new DialogPage();
                    dialogPage.ShowDialog();

                    if (dialogPage.Result != string.Empty)
                    {
                        string filePath = openFileDialog.FileName;
                        var package = new ExcelPackage(new FileInfo(filePath));
                        var worksheet = package.Workbook.Worksheets.Add(dialogPage.Result);

                        int lastColumnIndex = 10;

                        if (window.settings.IsInsertExcelPicture == false) //Если фото не добавляется, то количество столбцов меньше
                        {
                            lastColumnIndex = 9;
                        }

                        // Записываем заголовки столбцов
                        for (int i = 0; i < lastColumnIndex; i++)
                        {
                            if (i >= 5 && window.settings.IsInsertExcelPicture == false) //Если картинка не добавляется
                            {
                                worksheet.Cells[1, i + 1].Value = window.CalcDataGrid.Columns[i + 1].Header;
                            }
                            else
                            {
                                worksheet.Cells[1, i + 1].Value = window.CalcDataGrid.Columns[i].Header;
                            }

                        }
                        worksheet.Cells[1, lastColumnIndex].Value = "Примечания";

                        //Установка стилей для Header 
                        ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                        titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        titleRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.GetColor());

                        //Установка стилей для всего рабочего пространства
                        ExcelRange Rng = worksheet.Cells[1, 1, window.calcItems.Count + 2 - 1, lastColumnIndex];
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
                        for (int i = 0; i < window.calcItems.Count - 1; i++)
                        {
                            CalcProduct item = window.calcItems[i];

                            if (item.Photo == null) //Если фото равно нулю (Раздел)
                            {
                                //Стили для раздела
                                ExcelRange chapterRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                                chapterRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                chapterRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelChapterColor.GetColor());
                                chapterRange.Merge = true;
                                worksheet.Cells[i + 2, 1].Value = item.Manufacturer;
                                continue;
                            }

                            //Установка стилей всех данных
                            ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                            dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            dataRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelDataColor.GetColor());

                            //Установка стилей для примечаний
                            ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                            notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            notesRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNotesColor.GetColor());

                            //Установка стилей для номера
                            ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                            numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            numberRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNumberColor.GetColor());

                            //Добавление данных в ячейки
                            worksheet.Cells[i + 2, 1].Value = item.Num;
                            worksheet.Cells[i + 2, 2].Value = item.Manufacturer;
                            worksheet.Cells[i + 2, 3].Value = item.ProductName;
                            worksheet.Cells[i + 2, 4].Value = item.Article;
                            worksheet.Cells[i + 2, 5].Value = item.Unit;

                            if (window.settings.IsInsertExcelPicture == true) //Если картинку надо добавить
                            {
                                //Установка стилей для фона фото 
                                ExcelRange photoRange = worksheet.Cells[i + 2, 6];
                                photoRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                photoRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelPhotoBackgroundColor.GetColor());

                                ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                                BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);

                                //Ширина и высота в зависимости от выбранных параметров в настройках
                                worksheet.Column(6).Width = (window.settings.ExcelPhotoWidth + 10) / 7;
                                worksheet.Rows[i + 2].Height = (window.settings.ExcelPhotoHeight + 10) / 1.33;

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
                                    excelImage.SetSize(window.settings.ExcelPhotoWidth, window.settings.ExcelPhotoHeight);
                                }
                            }

                            //Добавление остальных данных в ячейки
                            worksheet.Cells[i + 2, lastColumnIndex - 3].Value = item.Cost;
                            worksheet.Cells[i + 2, lastColumnIndex - 2].Value = item.Count;
                            worksheet.Cells[i + 2, lastColumnIndex - 1].Value = item.TotalCost;
                            worksheet.Cells[i + 2, lastColumnIndex].Value = item.Note;
                        }

                        worksheet.Cells[window.calcItems.Count + 2 - 1, lastColumnIndex - 1].Value = window.settings.FullCostType + " " + window.calcItems[window.calcItems.Count - 1].TotalCost;

                        package.Save();

                        isSaved = true;
                    }
                }

                if(isSaved)
                {
                    window.CalcInfo_label.Content = "Расчётка успешно добавлена новым листом в существующий Excel файл.";
                }
                else
                {
                    window.CalcInfo_label.Content = "Расчёт не сохранён в существующий Excel.";
                }
            }
            catch(Exception exp) 
            {
                MessageBox.Show(exp.Message);
                window.CalcInfo_label.Content = "Расчёт не сохранён в существующий Excel.";
            }
        }

        void IFileImporter.ExportToPDF(bool isImporting) //Экспорт в PDF
        {
            throw new NotImplementedException();
        }

        void IFileImporter.ImportSettingsFromFile(MainWindow window) //Импорт настроек из файл
        {
            try
            {
                string filePath = "settings.json";
                string jsonString = File.ReadAllText(filePath);
                if (jsonString != string.Empty)
                {
                    window.settings = JsonSerializer.Deserialize<SettingsParameters>(jsonString);
                }

                foreach (var columnInfo in window.settings.CalcColumnInfos)
                {
                    var column = window.CalcDataGrid.Columns.First(c => c.Header.ToString() == columnInfo.Header);
                    if (column != null)
                    {
                        column.DisplayIndex = columnInfo.DisplayIndex;
                    }
                }

                foreach (var columnInfo in window.settings.DBColumnInfos)
                {
                    var column = window.dataBaseGrid.Columns.First(c => c.Header.ToString() == columnInfo.Header);
                    if (column != null)
                    {
                        column.DisplayIndex = columnInfo.DisplayIndex;
                    }
                }

                window.settings.IsAdministrator = false;
            }
            catch (Exception exp) 
            {
                MessageBox.Show(exp.Message);
            }
        }

        void IFileImporter.ExportSettingsOnFile(MainWindow window) //Экспорт настроек в файла
        {
            try
            {
                window.settings.CalcColumnInfos.Clear();
                window.settings.DBColumnInfos.Clear();

                foreach (var column in window.CalcDataGrid.Columns)
                {
                    window.settings.CalcColumnInfos.Add(new ColumnInfo
                    {
                        Header = column.Header.ToString(),
                        DisplayIndex = column.DisplayIndex,
                    });
                }

                foreach (var column in window.dataBaseGrid.Columns)
                {
                    window.settings.DBColumnInfos.Add(new ColumnInfo
                    {
                        Header = column.Header.ToString(),
                        DisplayIndex = column.DisplayIndex,
                    });
                }

                string jsonString = JsonSerializer.Serialize(window.settings);
                string filePath = "settings.json";
                File.WriteAllText(filePath, jsonString);
            }
            catch(Exception exp ) 
            {
                MessageBox.Show(exp.Message);
            }
        }

        void IFileImporter.ImportCountriesFromFTP() //Импорт стран с фтп сервера
        {
            string ftpFilePath = "/countries/countriesTest.json";
            string localJsonString = string.Empty;
            if (File.Exists("countries.json"))
            {
                localJsonString = File.ReadAllText("countries.json");
            }
            else
            {
                File.Create("countries.json");
            }

            try
            {
                string ftpJsonString = string.Empty;
                
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + ftpFilePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.UseBinary = true;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    // Читаем содержимое потока и конвертируем его в строку
                    ftpJsonString = reader.ReadToEnd();
                }

                if (localJsonString != ftpJsonString)
                {
                    MessageBox.Show("Менеджер цен не совпадал, но был обновлён");
                    CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(ftpJsonString);
                    File.WriteAllText("countries.json", ftpJsonString);
                }
                else
                {
                    CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(localJsonString);
                }
            }
            catch(Exception ex ) 
            {
                MessageBox.Show(ex.Message);
                CountryManager.Instance.priceManager = JsonSerializer.Deserialize<PriceManager>(localJsonString);
            }
        }

        void IFileImporter.ExportCountriesToFTP() //Экспорт стран на фтп сервера
        {
            try
            {
                string ftpFilePath = "/countries/countriesTest.json";
                string jsonString = JsonSerializer.Serialize(CountryManager.Instance.priceManager);
                File.WriteAllText("countries.json", jsonString);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + ftpFilePath);
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                }

                MessageBox.Show("Менеджер цен обновлён");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ExportCalcToFile(MainWindow window) //Экспорт расчётки в файл
        {
            if(window.calcItems.Count > 0)
            {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                };

                string jsonString = JsonSerializer.Serialize(window.calcItems, options);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Json Files (*.DAH)|*.DAH",
                    Title = "Сохранить json файл",
                    InitialDirectory = window.settings.CalcFolderPath
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(filePath)}";
                    File.WriteAllText(filePath, jsonString);
                }
            }
        }

        void IFileImporter.ImportCalcFromFile(MainWindow window) //Испорт расчётки из файла
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Json Files (*.DAH)|*.DAH",
                Title = "Открыть json файл",
                InitialDirectory = window.settings.CalcFolderPath
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(filePath)}";
                string jsonString = File.ReadAllText(filePath);
                window.calcItems.Clear();
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
                window.calcItems.Clear();
                foreach (var item in newItems)
                {
                    window.calcItems.Add(item);
                }
                window.allCountries_comboBox.SelectedIndex = 0;
                window.isCalculationNeed = true;
                window.MovingLabel.Visibility = Visibility.Visible;
            }
        }

        void IFileImporter.ImportCalcFromFile_StartDUH(string path, MainWindow window) //Испорт расчётки при запске dah файла
        {


            string filePath = path;
            window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(filePath)}";
            string jsonString = File.ReadAllText(filePath);
            window.calcItems.Clear();
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);

            var in78 = 1;

            window.calcItems.Clear();
            foreach (var item in newItems)
            {
                window.calcItems.Add(item);
            }
            window.allCountries_comboBox.SelectedIndex = 0;

        }

        void IFileImporter.ImportDBFromFTP(MainWindow window)
        {
            string ftpFilePath = "/data_price_test/Dahmira_TestDb.mdf";
            string ftpFilePathLdf = "/data_price_test/Dahmira_TestDb_log.ldf";
            string localFilePath = window.settings.PriceFolderPath + "\\Dahmira_TestDb.mdf";
            string localFilePathLdf = window.settings.PriceFolderPath + "\\Dahmira_TestDb_log.ldf";
            try
            {
                // Создаем запрос для скачивания файла
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + ftpFilePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.UseBinary = true;

                // Получаем ответ от сервера
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    // Копируем поток данных из ответа в локальный файл
                    responseStream.CopyTo(fileStream);
                }

                // Создаем запрос для скачивания файла
                FtpWebRequest requestLdf = (FtpWebRequest)WebRequest.Create(url_praise + ftpFilePathLdf);
                requestLdf.Method = WebRequestMethods.Ftp.DownloadFile;
                requestLdf.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                requestLdf.UseBinary = true;

                // Получаем ответ от сервера
                using (FtpWebResponse response = (FtpWebResponse)requestLdf.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(localFilePathLdf, FileMode.Create))
                {
                    // Копируем поток данных из ответа в локальный файл
                    responseStream.CopyTo(fileStream);
                }

                MessageBox.Show("Файл успешно загружен");
                window.Title = $"Dahmira       {localFilePath}";
                window.settings.Price = localFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        //Экспорт расчётки в шаблон
        void IFileImporter.ExportCalcToTemlates(MainWindow window, string patch)
        {
            if (window.calcItems.Count > 0)
            {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                };

                string jsonString = JsonSerializer.Serialize(window.calcItems, options);

                window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(patch)}";
                File.WriteAllText(patch, jsonString);
            }
        }


        //Возвращаем массив данных после десериализации из json
        ObservableCollection<CalcProduct> IFileImporter.Get_JsonList(string path, MainWindow window)
        {


            string filePath = path;
            window.CalcPath_label.Content = $"Имя файла расчёта: {Path.GetFileName(filePath)}";
            string jsonString = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var newItems = JsonSerializer.Deserialize<ObservableCollection<CalcProduct>>(jsonString, options);
            return newItems;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //           Загрузка Шаблонов
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Получение списка файлов на FTP-сервере
        List<string> IFileImporter.GetFileListFromFtp()
        {
            List<string> files = new List<string>();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + "/data_price_test/Шаблоны/");
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);


            try
            {
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.EndsWith("dah", StringComparison.OrdinalIgnoreCase))
                        {
                            files.Add(line);
                        }
                    }
                }
            }
            catch
            {
                //LB_info.Content = "FTP не доступен!";
                //LB_info.Foreground = new SolidColorBrush(Colors.DarkGoldenrod);
                MessageBox.Show("FTP сервер временно не доступен! Попробуйте позже!");

                //DownloadProgressBar.Visibility = Visibility.Hidden;
            }
            return files;
        }

        // Асинхронное скачивание файла с FTP-сервера
        async Task IFileImporter.DownloadFileAsync(string ftpServerUrl, string localFilePath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + "/data_price_test/Шаблоны/" + ftpServerUrl);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream localFileStream = new FileStream(localFilePath, FileMode.Create))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await localFileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }



    }
}
