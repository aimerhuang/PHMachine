using Freedom.Common;
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

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// BookingTarget.xaml 的交互逻辑
    /// </summary>
    public partial class BookingArea : Page
    {
        public BookingArea()
        {
            InitializeComponent();

            this.DataContext = new BookingAreaViewModels();

            //在这里开启计时器引起的重新开之后,未关闭导致的操时提示。已放到Init by 2021年7月9日18:58:26 wei.chen
            //启用计时器
            //ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);

            ViewModel.ContentPage = this;
            ViewModel.TipMessage = ViewModel.OwnerViewModel?.IsBeijing == true || ViewModel.OwnerViewModel?.IsWuHan == true ? "请选择您的办理方式" : "请在屏幕下方取走您的派号单";
            //|| ViewModel.OwnerViewModel?.IsWuHan == true
            ViewModel.NextPageShow = ViewModel.OwnerViewModel?.IsBeijing == true || ViewModel.OwnerViewModel?.IsWuHan == true ? Visibility.Visible : Visibility.Collapsed;
            ViewModel.PreviousPageShow = Visibility.Collapsed;
        }

        public BookingAreaViewModels ViewModel => this.DataContext as BookingAreaViewModels;

        private void lst_Loaded(object sender, RoutedEventArgs e)
        {
            var count = ViewModel.AreaInfo?.Count;
            if (count > 0)
            {
                Lst.SelectedIndex = 0;
            }

        }

        private void Lst_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var count = ViewModel.AreaInfo?.Count;
            if (count > 0)
            {
                //Lst.SelectedIndex = 0;
            }
        }



    }
}
