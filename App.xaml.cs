using System.Configuration;
using System.Data;
using System.Windows;

namespace Dahmira
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Тепрь запускаем главное окно ручками
            MainWindow mainWindow = new MainWindow();

            //Проверяем запущена ли программа через файл
            if (e.Args.Length > 0)
            {
                //Если да, забираем путь к файлу
                string filePath = e.Args[0];

                //Подписываемся на событие Loaded, форма полностью прогрузилась
                mainWindow.Loaded += (s, ev) =>
                {
                    //Вызываем метод после того, как окно будет загружено
                    mainWindow.openCalcTest(filePath);
                };
            }
            //Отображаем главное окно
            mainWindow.Show();
        }
    }

}
