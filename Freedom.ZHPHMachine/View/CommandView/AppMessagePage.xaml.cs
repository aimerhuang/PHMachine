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
using Freedom.ZHPHMachine.ViewModels.CommandViewModels;

namespace Freedom.ZHPHMachine.View.CommandView
{
    /// <summary>
    /// AppMessagePage.xaml 的交互逻辑
    /// </summary>
    public partial class AppMessagePage : Page
    {
        public AppMessagePage()
        {
            InitializeComponent();
            //this.DataContext = new AppMessageViewModels();
            this.DataContext = AppMessageViewModels.Instance;
            //启用计时器
            
            //ViewModel.ContentPage = this;
            //ViewModel.NextPageShow = Visibility.Visible;
        }
        public AppMessageViewModels ViewModel => this.DataContext as AppMessageViewModels;


    }
}
