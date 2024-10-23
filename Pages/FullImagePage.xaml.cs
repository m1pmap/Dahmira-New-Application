using Dahmira.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using static System.Resources.ResXFileRef;

namespace Dahmira.Pages
{
    /// <summary>
    /// Логика взаимодействия для FullImagePage.xaml
    /// </summary>
    public partial class FullImagePage : Window
    {
        private ByteArrayToImageSourceConverter_Services converter = new ByteArrayToImageSourceConverter_Services();
        public FullImagePage(byte[] image)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            fullImage.Source = (BitmapImage)converter.Convert(image, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
        }

        private void fullImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("");
        }
    }
}
