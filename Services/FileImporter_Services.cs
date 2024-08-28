using Dahmira.Interfaces;
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
        private string ftpFilePath = "/countries/countriesTest.json";
        void IFileImporter.ExportToExcel(MainWindow window)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                var package = new ExcelPackage();
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

                //Установка стилей для Header 
                ExcelRange titleRange = worksheet.Cells[1, 1, 1, lastColumnIndex];
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.Color);

                //Установка стилей для всего рабочего пространства
                ExcelRange Rng = worksheet.Cells[1, 1, window.calcItems.Count + 2, lastColumnIndex];
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
                for (int i = 0; i < window.calcItems.Count; i++)
                {
                    CalcProduct item = window.calcItems[i];

                    if (item.Photo == null) //Если фото равно нулю (Раздел)
                    {
                        //Стили для раздела
                        ExcelRange chapterRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                        chapterRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        chapterRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelChapterColor.Color);
                        chapterRange.Merge = true;
                        worksheet.Cells[i + 2, 1].Value = item.Manufacturer;
                        continue;
                    }

                    //Установка стилей всех данных
                    ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                    dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    dataRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelDataColor.Color);

                    //Установка стилей для примечаний
                    ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                    notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    notesRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNotesColor.Color);

                    //Установка стилей для номера
                    ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                    numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    numberRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNumberColor.Color);

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
                        photoRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelPhotoBackgroundColor.Color);

                        ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                        BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);

                        //Ширина и высота в зависимости от выбранных параметров в настройках
                        worksheet.Column(6).Width = (window.settings.MaxExcelPhotoWidth + 10) / 7;
                        worksheet.Rows[i + 2].Height = (window.settings.MaxExcelPhotoHeight + 10) / 1.33;

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
                            excelImage.SetSize(window.settings.MaxExcelPhotoWidth, window.settings.MaxExcelPhotoHeight);
                        }
                    }

                    //Добавление остальных данных в ячейки
                    worksheet.Cells[i + 2, lastColumnIndex - 3].Value = item.Cost;
                    worksheet.Cells[i + 2, lastColumnIndex - 2].Value = item.Count;
                    worksheet.Cells[i + 2, lastColumnIndex - 1].Value = item.TotalCost;
                    worksheet.Cells[i + 2, lastColumnIndex].Value = item.Note;
                }

                worksheet.Cells[window.calcItems.Count + 2, lastColumnIndex - 1].Value = window.settings.TotalCostValue + " " + window.fullCost.Content;

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Сохранить Excel документ",
                    InitialDirectory = window.settings.ExcelFolderPath
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    worksheet.Protection.IsProtected = false;
                    worksheet.Protection.AllowSelectLockedCells = false;
                    package.SaveAs(new FileInfo(filePath));
                }
            }
            catch (Exception exp) 
            {
                MessageBox.Show(exp.Message);
            }
        }

        void IFileImporter.ExportToExcelAsNewSheet(MainWindow window)
        {
            //try
            //{
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Добавить в Excel документ",
                    InitialDirectory = window.settings.ExcelFolderPath
                };

                if (openFileDialog.ShowDialog() == true)
                {
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
                        titleRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelTitleColor.Color);

                        //Установка стилей для всего рабочего пространства
                        ExcelRange Rng = worksheet.Cells[1, 1, window.calcItems.Count + 2, lastColumnIndex];
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
                        for (int i = 0; i < window.calcItems.Count; i++)
                        {
                            CalcProduct item = window.calcItems[i];

                            if (item.Photo == null) //Если фото равно нулю (Раздел)
                            {
                                //Стили для раздела
                                ExcelRange chapterRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                                chapterRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                chapterRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelChapterColor.Color);
                                chapterRange.Merge = true;
                                worksheet.Cells[i + 2, 1].Value = item.Manufacturer;
                                continue;
                            }

                            //Установка стилей всех данных
                            ExcelRange dataRange = worksheet.Cells[i + 2, 1, i + 2, lastColumnIndex];
                            dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            dataRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelDataColor.Color);

                            //Установка стилей для примечаний
                            ExcelRange notesRange = worksheet.Cells[i + 2, lastColumnIndex];
                            notesRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            notesRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNotesColor.Color);

                            //Установка стилей для номера
                            ExcelRange numberRange = worksheet.Cells[i + 2, 1];
                            numberRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            numberRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelNumberColor.Color);

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
                                photoRange.Style.Fill.BackgroundColor.SetColor(window.settings.ExcelPhotoBackgroundColor.Color);

                                ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();

                                BitmapImage bitmapImage = (BitmapImage)converter.Convert(item.Photo, typeof(BitmapImage), null, CultureInfo.CurrentCulture);

                                //Ширина и высота в зависимости от выбранных параметров в настройках
                                worksheet.Column(6).Width = (window.settings.MaxExcelPhotoWidth + 10) / 7;
                                worksheet.Rows[i + 2].Height = (window.settings.MaxExcelPhotoHeight + 10) / 1.33;

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
                                    excelImage.SetSize(window.settings.MaxExcelPhotoWidth, window.settings.MaxExcelPhotoHeight);
                                }
                            }

                            //Добавление остальных данных в ячейки
                            worksheet.Cells[i + 2, lastColumnIndex - 3].Value = item.Cost;
                            worksheet.Cells[i + 2, lastColumnIndex - 2].Value = item.Count;
                            worksheet.Cells[i + 2, lastColumnIndex - 1].Value = item.TotalCost;
                            worksheet.Cells[i + 2, lastColumnIndex].Value = item.Note;
                        }

                        worksheet.Cells[window.calcItems.Count + 2, lastColumnIndex - 1].Value = window.settings.TotalCostValue + " " + window.fullCost.Content;

                        package.Save();
                    }
                }
            //}
            //catch(Exception exp) 
            //{
            //    MessageBox.Show(exp.Message);
            //}
        }

        void IFileImporter.ExportToPDF(bool isImporting)
        {
            throw new NotImplementedException();
        }

        void IFileImporter.ExportSettingsOnFile(MainWindow window)
        {
            try
            {
                string filePath = "settings.json";
                string jsonString = File.ReadAllText(filePath);
                if (jsonString != string.Empty)
                {
                    window.settings = JsonSerializer.Deserialize<SettingsParameters>(jsonString);
                }
            }
            catch (Exception exp) 
            {
                MessageBox.Show(exp.Message);
            }
        }

        void IFileImporter.ImportSettingsFromFile(MainWindow window)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(window.settings);
                string filePath = "settings.json";
                File.WriteAllText(filePath, jsonString);
            }
            catch(Exception exp ) 
            {
                MessageBox.Show(exp.Message);
            }
        }

        void IFileImporter.ImportCountriesFromFTP()
        {
            try
            {
                string localJsonString = File.ReadAllText("countries.json");
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
                    CountryManager.Instance.countries = JsonSerializer.Deserialize<ObservableCollection<Country>>(ftpJsonString);
                }
            }
            catch(Exception ex ) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ExportCountriesToFTP()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(CountryManager.Instance.countries);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url_praise + ftpFilePath);
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void IFileImporter.ExportCalcToFile(MainWindow window)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            string jsonString = JsonSerializer.Serialize(window.calcItems, options);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Json Files (*.json)|*.json",
                Title = "Сохранить json файл",
                InitialDirectory = window.settings.CalcFolderPath
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                File.WriteAllText(filePath, jsonString);
            }
        }

        void IFileImporter.ImportCalcFromFile(MainWindow window)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Json Files (*.json)|*.json",
                Title = "Сохранить json файл",
                InitialDirectory = window.settings.CalcFolderPath
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
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
            }
        }
    }
}
