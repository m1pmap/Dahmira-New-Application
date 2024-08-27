using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для DialogPage.xaml
    /// </summary>
    public partial class DialogPage : Window
    {
        public string Result { get; private set; } = string.Empty;
        public DialogPage()
        {
            InitializeComponent();
        }

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            Result = FileName.Text; //Получаем текст из TextBox
            DialogResult = true; //Устанавливаем результат диалога
            Close(); //Закрываем окно
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; //Устанавливаем результат диалога
            Close(); //Закрываем окно
        }
    }
}
