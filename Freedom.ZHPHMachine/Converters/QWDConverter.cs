using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Freedom.ZHPHMachine.Converters
{
    public class QWDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()) && parameter != null && !string.IsNullOrWhiteSpace(parameter.ToString()))
            {
                return value.ToString().Equals(parameter.ToString());
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
