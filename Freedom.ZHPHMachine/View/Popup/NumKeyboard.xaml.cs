using Freedom.WinAPI;
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
using System.Windows.Shapes;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// NumberKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class NumKeyboard : Window
    {
        public NumKeyboard(KeyboardType type, string title)
        {
            InitializeComponent();
            this.DataContext = new NumberKeyboardViewModels(type, title);
            if (type == KeyboardType.TelePhone)
            {
                txt.MaxLength = 11;
            }
            txt.Focus(); 
        }

        public NumberKeyboardViewModels ViewModel => this.DataContext as NumberKeyboardViewModels;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_TouchDown(object sender, TouchEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewModel.TipsMsg = "";
            if (sender is Button)
            {
                var content = (sender as Button).Content?.ToString();
                if (content.Length == 1)
                {
                    ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                    int intAsciiCode = (int)asciiEncoding.GetBytes(content)[0];
                    Win32API.AddKeyBoardINput((byte)intAsciiCode);
                }
            }

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TipsMsg = "";
            Win32API.AddKeyBoardINput(0x08);
        }

        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TipsMsg = "";
            string content = txt.Text;
            if (content.Length > 0)
            {
                txt.Text = string.Empty;
            }
        }

        private void Txt_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Content != null)
            {
                txt.SelectionStart = ViewModel.Content.Length;
            }
        }
    }
}
