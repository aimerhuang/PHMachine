using SFreedom.ZHPHMachine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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
using WpfAnimatedGif;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// CheckPage.xaml 的交互逻辑
    /// </summary>
    public partial class CheckPage : Page
    {

        public CheckPage()
        {
            InitializeComponent();
            var viewModel = new CheckPageViewModels();
            this.DataContext = viewModel;
            ViewModel.ContentPage = this;
            //不显示上一页
            ViewModel.NextPageShow = ViewModel.PreviousPageShow = Visibility.Collapsed;

        }
        public CheckPageViewModels ViewModel => this.DataContext as CheckPageViewModels;

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.GifImage != null)
            {
                var controller = ImageBehavior.GetAnimationController(GifImage);
                controller?.Dispose();
            }
        }
    }
}
