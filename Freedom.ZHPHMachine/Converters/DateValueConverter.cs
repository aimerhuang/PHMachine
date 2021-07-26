using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Freedom.ZHPHMachine.Converters
{
    public class DateValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                DateTime dt = DateTime.Now;

                if (value is DateTime)
                {
                    dt = (DateTime)value;
                }
                else if (value is string)
                {
                    dt = DateTime.ParseExact(value?.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                }
                if (parameter != null && parameter?.ToString().Length > 0)
                {
                    return dt.ToString(parameter.ToString());
                }
                else
                {
                    return dt.ToString(parameter == null ? "yyyy-MM-dd" : parameter.ToString());
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
