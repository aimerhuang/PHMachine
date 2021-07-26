using Freedom.Common.HsZhPjh.Enums;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.ViewModels;
using MachineCommandService;
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
    //class CardTypeToVisibilityConverter
    //{
    //}
    /// <summary>
    /// BoolToVisibilityConverter 的交互逻辑
    /// </summary>
    public class CardTypeToVisibilityConverter : ViewModelBase, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var CardTypes = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()
                ?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
            Visibility show = Visibility.Visible;

            if (value != null)
            {
                if (BookingBaseInfo.SelectCardTypes == null || !BookingBaseInfo.SelectCardTypes.Contains(parameter))
                {
                    show = Visibility.Collapsed;
                }
                else
                {
                    show = Visibility.Visible;
                }

                //初始化状态
                if (BookingBaseInfo?.BookingInfo != null && BookingBaseInfo.BookingInfo.Count > 0)
                {
                    //有预约信息且在使用状态中
                    var HZBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                         t.SQLB.Equals(((int)EnumTypeSQLB.HZ).ToString()) && t.IsUse);
                    var TWBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                         t.SQLB.Equals(((int)EnumTypeSQLB.TWN).ToString()) && t.IsUse);
                    var GABookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                         t.SQLB.Equals(((int)EnumTypeSQLB.HKGMAC).ToString()) && t.IsUse);
                    if (HZBookingInfo != null)
                    {
                        var a = HZBookingInfo.BZLB;
                    }
                }


            }

            return show;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var a = BookingBaseInfo.SelectCardTypes;

            throw new NotImplementedException();
        }
    }
}
