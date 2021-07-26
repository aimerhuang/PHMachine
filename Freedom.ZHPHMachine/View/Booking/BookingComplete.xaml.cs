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
using Freedom.Config;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// BookingComplete.xaml 的交互逻辑
    /// </summary>
    public partial class BookingComplete : Page
    {
        public BookingComplete()
        {
            InitializeComponent();
            this.DataContext = new BookingCompleteViewModels();
            ViewModel.ContentPage = this;
            ViewModel.TipMessage = "预约完成！！！";


            if (ViewModel.OwnerViewModel?.IsXuwen == true && QJTConfig.QJTModel.IsTodayPH)
            {
                ViewModel.TipMessage = "填表完成！！！";
            }
        }
        public BookingCompleteViewModels ViewModel => this.DataContext as BookingCompleteViewModels;
    }
}
