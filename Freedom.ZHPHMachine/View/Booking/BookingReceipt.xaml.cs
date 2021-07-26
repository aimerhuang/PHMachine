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
using Freedom.Config;
using Freedom.ZHPHMachine.ViewModels.Booking;

namespace Freedom.ZHPHMachine.View.Booking
{
    /// <summary>
    /// BookingReceipt.xaml 的交互逻辑
    /// </summary>
    public partial class BookingReceipt : Page
    {
        public BookingReceipt()
        {
            InitializeComponent();

            this.DataContext = new BookingReceiptViewModels(this);
            //启用计时器
            ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
            ViewModel.ContentPage = this;
            //ViewModel.TipMessage=""
            ViewModel.PreviousPageShow = Visibility.Visible;
            ViewModel.NextPageShow = Visibility.Collapsed;
        }

        public BookingReceiptViewModels ViewModel => this.DataContext as BookingReceiptViewModels;

    }
}
