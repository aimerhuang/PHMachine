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
    /// ApplyIndex.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = new MainPageViewModels();
        }

        public MainPageViewModels ViewModel => this.DataContext as MainPageViewModels;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.OwnerViewModel.IsXuwen && QJTConfig.QJTModel.IsTodayPH)
            {
                Button.Content = "点我填表";
            }

            if (ViewModel.OwnerViewModel.IsShenZhen)
            {
                Button.Content = "请刷身份证";
                Button2.Content = "请扫码取号";
            }
            else
            {
                Button.Content = "点我取号";
                Button.Margin = new Thickness(0,0,0,0);
                Button2.Visibility = Visibility.Collapsed;
            }

        }
       
    }
}
