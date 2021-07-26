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
    /// <summary>
    /// 保存空间的显示与隐藏
    /// 修改为不保存空间的隐藏
    /// </summary>
    public class InSpaceBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility show = Visibility.Visible;

            if (value != null && !string.IsNullOrWhiteSpace(value?.ToString()))
            {
                if (bool.TryParse(value.ToString(), out bool blShow))
                {

                    // 判断是否设置了参数
                    if (null != parameter)
                    {
                        string strInvert = parameter.ToString();

                        // 设置了需要取反
                        if (strInvert.ToLower() == "invert")
                        {
                            if (blShow == false)
                            {
                                show = Visibility.Visible;
                            }
                            else
                            {
                                show = Visibility.Collapsed;
                            }
                        }
                    }
                    else if (blShow == false)
                    {
                        show = Visibility.Collapsed;
                    }
                }
                else if (value is Visibility && parameter.ToString().ToLower() == "invert")
                {
                    if ((Visibility)value == Visibility.Visible)
                    {
                        show = Visibility.Collapsed;
                    }

                }
                else
                {
                    show = Visibility.Visible;
                }
            }
            else
            {
                show = Visibility.Collapsed;
            }

            return show;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
