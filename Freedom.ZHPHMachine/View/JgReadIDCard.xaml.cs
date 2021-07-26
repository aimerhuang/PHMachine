using Freedom.Common;
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
using WpfAnimatedGif;

namespace SFreedom.ZHPHMachine.ViewModels
{
    /// <summary>
    /// JgReadIDCard.xaml 的交互逻辑
    /// </summary>
    public partial class JgReadIDCard : Page
    {
        public JgReadIDCard()
        {
            InitializeComponent();
            this.DataContext = new JgReadIDCardViewModels(this);
            inputbox.Focus();
            if (!QJTConfig.QJTModel.IsManualInput)
            {
                GridInput.Visibility = Visibility.Collapsed;
            }
            if (ViewModels.OwnerViewModel.IsBeijing == false)
                ViewModels.PreviousPageShow = Visibility.Visible;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.GifImage != null)
            {
                var controller = ImageBehavior.GetAnimationController(GifImage);
                controller?.Dispose();
            }
        }

        public JgReadIDCardViewModels ViewModels => this.DataContext as JgReadIDCardViewModels;

        private void inputbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputbox.Text.Length == 18)
            {
                //校验格式不需要显示 by wei.chen
                //ViewModels.OwnerViewModel.IsShowHiddenLoadingWait("正在核查您的身份信息，请稍候......");
                ViewModels.AutoDoNextFun(inputbox.Text.Trim());
            }
        }
    }
}
