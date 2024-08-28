using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dahmira.Services
{
    internal class NullToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) //Конвертер нулевого значения в пустую строку
        {
            // Проверяем, является ли значение null
            if (value == null || 
               (value is double && double.IsNaN((double)value)) || 
               (value is int && (int)value == 0))
            {
                return "";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Конвертация обратно не требуется
            return value;
        }
    }
}
