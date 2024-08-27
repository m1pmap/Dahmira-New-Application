using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Dahmira.Models
{
    public class SettingsParameters
    {
        //Общие настройки
        public int Theme { get; set; } = 0;//0 - светлая, 1 - темная
        public bool? IsNotificationsWithSound {get; set;} = false;
        public double CheckingIntervalFromMail { get; set; } = 1.1;
        public string? PriceFolderPath { get; set; } = "D:\\";

        public bool IsAdministrator { get; set; } = false;

        //Менеджер цен
        //...

        //Вывод данных
        //Excel
        public ColorItem ExcelTitleColor { get; set; } = new ColorItem { Name = "Зелёный", Color = Color.MediumSeaGreen};
        public ColorItem ExcelChapterColor { get; set; } = new ColorItem { Name = "Жёлтый", Color = Color.LightYellow };
        public ColorItem ExcelDataColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public ColorItem ExcelPhotoBackgroundColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public ColorItem ExcelNotesColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public ColorItem ExcelNumberColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public bool? IsInsertExcelPicture { get; set; } = true;
        public int MaxExcelPhotoWidth { get; set; } = 100;
        public int MaxExcelPhotoHeight { get; set; } = 100;
        public string TotalCostValue { get; set; } = "ИТОГО:";

        //PDF
        public ColorItem PdfHeaderColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public ColorItem PdfChapterColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };
        public ColorItem PdfResultsColor { get; set; } = new ColorItem { Name = "Прозрачный", Color = Color.Transparent };

        //Пути сохранения
        public string ExcelFolderPath { get; set; } = "D:\\";
        public string PDFFolderPath { get; set; } = "D:\\";
        public string CalcFolderPath { get; set; } = "D:\\";
    }
}
