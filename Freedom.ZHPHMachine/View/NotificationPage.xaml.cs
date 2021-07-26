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
using System.Windows.Threading;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// NotificationPage.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationPage : Page
    {
        public NotificationPage()
        {
            InitializeComponent();
            this.DataContext = new NotificationPageViewModels();
            
        }

        public NotificationPageViewModels ViewModel => this.DataContext as NotificationPageViewModels;

        private void Block_OnLoaded(object sender, RoutedEventArgs e)
        {
            Block.Text = ViewModel.Text;
        }
    }
}
