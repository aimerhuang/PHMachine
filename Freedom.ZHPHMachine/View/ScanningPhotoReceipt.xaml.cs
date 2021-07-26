using Freedom.Config;
using Freedom.ZHPHMachine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Freedom.Common;
using WpfAnimatedGif;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// ScanningPhotoReceiptPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScanningPhotoReceipt : Page
    {
        [DllImport("user32.dll")]
        private static extern bool PostMessage(int hhwnd, uint msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        private static uint WM_INPUTLANGCHANGEREQUEST = 0X0050;
        private static int HWND_BROADCAST = 0xffff;
        private static string en_US = "00000409";
        private static string en_ZH = "00000804";
        private static uint KLF_ACTIVATE = 1;

        public ScanningPhotoReceipt()
        {
            InitializeComponent();
            this.DataContext = new ScanningPhotoReceiptViewModels(this);
            txt.Focus();

            //不显示下一页
            ViewModel.PreviousPageShow = ViewModel.OwnerViewModel?.IsShenZhen == true ? Visibility.Collapsed : Visibility.Visible;
            PZButton2.Visibility = Visibility.Collapsed;
            TextBlock.Visibility = Visibility.Collapsed;
            if (ViewModel.OwnerViewModel?.IsShenZhen == true || ViewModel.OwnerViewModel?.IsXuwen == true || QJTConfig.QJTModel.IsTodayPH)// 
            {
                PZButton.Visibility = Visibility.Hidden;
                PZButton2.Visibility = Visibility.Hidden;
                TextBlock.Visibility = Visibility.Visible;
            }

            if (ViewModel.BookingBaseInfo.BookingSource == 1 && ViewModel.OwnerViewModel?.IsShenZhen == true)// 
            {
                YyBlock.Visibility = Visibility.Hidden;
            }

        }
        public ScanningPhotoReceiptViewModels ViewModel => this.DataContext as ScanningPhotoReceiptViewModels;

        string msg = string.Empty;
        private void Page_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r" || e.Text == "\r\n")
            {
                Log.Instance.WriteInfo("换行");
                ViewModel.QueryRKZPInfo(msg);
                msg = string.Empty;
            }
            msg += e.Text;

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (QJTConfig.QJTModel.BarCodeType == Freedom.Models.BarCodeTYpe.USB)
            {
                Application.Current.MainWindow.PreviewTextInput += Page_PreviewTextInput;
                PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, LoadKeyboardLayout(en_US, KLF_ACTIVATE));
            }
            txt.Focus();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (QJTConfig.QJTModel.BarCodeType == Freedom.Models.BarCodeTYpe.USB)
            {
                Application.Current.MainWindow.PreviewTextInput -= Page_PreviewTextInput;
                PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, LoadKeyboardLayout(en_ZH, KLF_ACTIVATE));
            }
            if (this.GifImage != null)
            {
                var controller = ImageBehavior.GetAnimationController(GifImage);
                controller?.Dispose();
            }
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                Log.Instance.WriteInfo("回车");
                ViewModel.QueryRKZPInfo(txt.Text);
            }

            txt.Focus();
        }

        private void PZButton_KeyDown(object sender, KeyEventArgs e)
        {
            Log.Instance.WriteInfo("点击【前往拍照区拍照】按钮");

            //txt.Focus();
        }

        private void PZButton2_KeyDown(object sender, KeyEventArgs e)
        {
            Log.Instance.WriteInfo("点击【前往人工区拍照】按钮");
        }
    }
}
