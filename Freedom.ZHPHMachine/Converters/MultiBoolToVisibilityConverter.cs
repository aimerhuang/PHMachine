using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Freedom.ZHPHMachine.Converters
{
    public class MultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length <= 0) return Visibility.Collapsed;
            foreach (var item in values)
            {
                if (item != null && item is Visibility)
                {
                    if ((Visibility)item == Visibility.Collapsed)
                    {
                        return Visibility.Collapsed;
                    }
                }
                else if (item != null && item is bool)
                {
                    if ((bool)item == false)
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
