using Freedom.Config;
using Freedom.ZHPHMachine.ViewModels;
using SFreedom.ZHPHMachine.ViewModels;
using System;
using System.Collections;
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
using Freedom.Common;
using Freedom.Models;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// SystemManagementPage.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordKeyboardPage : Page
    {
        public PasswordKeyboardPage()
        {
            InitializeComponent();
            this.DataContext = new PasswordKeyboardViewModels();
            ViewModel.TipMessage = "输入管理员密码";
            input.Focus();

            //if (!string.IsNullOrWhiteSpace(QJTConfig.QJTModel?.AppExitPassWord))
            //{
            //    password = QJTConfig.QJTModel.AppExitPassWord.Trim();
            //}
        }
        public PasswordKeyboardViewModels ViewModel => this.DataContext as PasswordKeyboardViewModels;

        private string password = "123456";
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            var czPassWord = ViewModel?.OwnerViewModel?.CZPassWord;

            if (!string.IsNullOrEmpty(czPassWord))
            {
                password = czPassWord;
            }
            if (e.Key == Key.Enter)
            {
                if (input.Password == password)
                {
                    ViewModel.DoNext.Execute("SystemManagementPage");
                }
                else
                {
                    input.Password = string.Empty;
                }
            }
        }
    }
}
