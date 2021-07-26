using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Freedom.Common;
using MachineCommandService;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// BookingTarget.xaml 的交互逻辑
    /// </summary>
    public partial class BookingInfo : Page
    {
        public BookingInfo()
        {
            InitializeComponent();
            this.DataContext = new BookingInfoViewModels();


            sysTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());

            //启用计时器
            ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            ViewModel.ContentPage = this;
            ViewModel.StateInitializationAsync();
            ViewModel.TipMessage = "填写申请信息";
            ViewModel.NextPageShow = Visibility.Visible;
            ViewModel.PreviousPageShow = Visibility.Visible;
        }

        DateTime sysTime;
        public BookingInfoViewModels ViewModel => this.DataContext as BookingInfoViewModels;

        private void RadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.gABookingInfo == null) { return; }

            if (ViewModel.gABookingInfo.SQOp == 0) raall.IsChecked = true;
            else if (ViewModel.gABookingInfo.SQOp == 1) raqz.IsChecked = true;
            else ratxz.IsChecked = true;

        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            int index = 0;
            if (ViewModel.hZBookingInfo != null) index = 0;
            else if (ViewModel.tWBookingInfo != null) index = 2;
            else if (ViewModel.gABookingInfo != null) index = 1;
            tbc.SelectedIndex = index;
        }

        private void RadioButton_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (ViewModel.gABookingInfo == null) { return; }
            if (ViewModel.gABookingInfo.GAQSXB == "1")
            {
                ra1.IsChecked = true;
            }
            else if (ViewModel.gABookingInfo.GAQSXB == "2")
            {
                ra2.IsChecked = true;
            }
        }

        private void Ra1_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as RadioButton)?.Content?.ToString();
            if (content == "男")
            {
                ViewModel.GABookingInfo.GAQSXB = "1";
            }
            else if (content == "女")
            {
                ViewModel.GABookingInfo.GAQSXB = "2";
            }
        }

        private void Raallt_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.tWBookingInfo == null) { return; }

            if (ViewModel.tWBookingInfo.SQOp == 0) raallt.IsChecked = true;
            else if (ViewModel.tWBookingInfo.SQOp == 1) raqzt.IsChecked = true;
            else ratxzt.IsChecked = true;
        }

        /// <summary>
        /// 港澳签注选择为持证申请签注时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GATxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GATxt.Text.Trim()) && ViewModel.GABookingInfo.ApplyType?.Code == "92")
            {
                raall.IsChecked = false;
                raqz.IsChecked = true;

            }
            else
            {
                raall.IsChecked = true;
                raqz.IsChecked = false;
            }
        }

        /// <summary>
        /// 台湾签注选择为持证申请签注时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TWTxt_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrEmpty(TWTxt.Text.Trim()) && ViewModel.TWBookingInfo.ApplyType?.Code == "92")
            {
                raallt.IsChecked = false;
                raqzt.IsChecked = true;

            }
            else
            {
                raallt.IsChecked = true;
                raqzt.IsChecked = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.HZBookingInfo != null || ViewModel?.GABookingInfo != null || ViewModel?.TWBookingInfo != null)
            {
                //证件状态显示
                IsEnable();
            }
        }

        /// <summary>
        /// 证件状态显示
        /// </summary>
        private void IsEnable()
        {
            if (string.IsNullOrEmpty(ViewModel.HZBookingInfo?.XCZJHM))
            {

                HZZJZT.Text = "无证件信息";
                HZZJZT.Foreground = Brushes.Red;

            }
            else if (ViewModel.HZBookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.HZBookingInfo?.XCZJYXQZ))
            {
                HZZJZT.Text = "失效";
                HZZJZT.Foreground = Brushes.Red;

            }
            else
            {
                HZZJZT.Text = "有效";
                HZZJZT.Foreground = Brushes.Green;
            }

            if (string.IsNullOrEmpty(ViewModel.GABookingInfo?.XCZJHM))
            {
                GAZJZT.Text = "无证件信息";
                GAZJZT.Foreground = Brushes.Red;
            }
            else if (ViewModel.GABookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.GABookingInfo?.XCZJYXQZ))
            {
                GAZJZT.Text = "失效";
                GAZJZT.Foreground = Brushes.Red;
            }
            else
            {
                GAZJZT.Text = "有效";
                GAZJZT.Foreground = Brushes.Green;
            }

            if (string.IsNullOrEmpty(ViewModel.TWBookingInfo?.XCZJHM))
            {
                TWZJZT.Text = "无证件信息";
                TWZJZT.Foreground = Brushes.Red;
            }
            else if (ViewModel.TWBookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.TWBookingInfo?.XCZJYXQZ))
            {
                TWZJZT.Text = "失效";
                TWZJZT.Foreground = Brushes.Red;
            }
            else
            {
                TWZJZT.Text = "有效";
                TWZJZT.Foreground = Brushes.Green;
            }
        }

        /// <summary>
        /// 判断有效期是否大于今天
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool GetVaildDate(string datetime)
        {
            if (string.IsNullOrEmpty(datetime))
                return false;
            Log.Instance.WriteInfo("当前时间：" + sysTime.ToString("yyyyMMdd").ToInt() + "证件有效期至：" + datetime.ToInt());
            return datetime.ToInt() < sysTime.ToString("yyyyMMdd").ToInt();
            //DateTime dt = DateTime.ParseExact(datetime, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            //return DateTime.Compare(sysTime, dt) > 0;
        }



    }
}
