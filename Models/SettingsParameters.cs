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
        public bool? IsNotificationsWithSound {get; set;} = false; //Уведомления со звуком
        public double CheckingIntervalFromMail { get; set; } = 1.1; //Интервал проверки сообщений с mail
        public string? PriceFolderPath { get; set; } = "D:\\"; //Папка для сохранения прайса

        public bool IsAdministrator { get; set; } = false;

        //Менеджер цен
        //...

        //Вывод данных
        //Excel
        public ColorItem ExcelTitleColor { get; set; } = new ColorItem ("Зелёный", Color.MediumSeaGreen); //Цвет заголовка
        public ColorItem ExcelChapterColor { get; set; } = new ColorItem ("Жёлтый", Color.LightYellow); //Цвет раздела
        public ColorItem ExcelDataColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); // Цвет данных
        public ColorItem ExcelPhotoBackgroundColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет за фото
        public ColorItem ExcelNotesColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет примечаний
        public ColorItem ExcelNumberColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет номеров
        public bool? IsInsertExcelPicture { get; set; } = true; //Добавляется ли картинка в Excel
        public int ExcelPhotoWidth { get; set; } = 100; //Ширина картинки в Excel
        public int ExcelPhotoHeight { get; set; } = 100; //Высота картинки в Excel
        public string FullCostType { get; set; } = "ИТОГО:"; //Тип полной стоимости

        //PDF
        public ColorItem PdfHeaderColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет заголовка
        public ColorItem PdfChapterColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет раздела
        public ColorItem PdfResultsColor { get; set; } = new ColorItem ("Прозрачный", Color.Transparent); //Цвет результатов

        //Пути сохранения
        public string ExcelFolderPath { get; set; } = "D:\\"; //Папка для сохранения Excel
        public string PDFFolderPath { get; set; } = "D:\\"; //Папка для сохранения PDF
        public string CalcFolderPath { get; set; } = "D:\\"; //Папка для сохранения расчётки
    }
}
