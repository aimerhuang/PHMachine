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
using Freedom.Models.ZHPHMachine;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// NumberKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class SelectOpera : Window
    {
        public SelectOpera(OperaType type, string key, string code = null, int status = 1)
        {
            InitializeComponent();
            this.DataContext = new SelectOperaViewModels(type, key, code, status);

        }

        public SelectOperaViewModels ViewModel => this.DataContext as SelectOperaViewModels;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        //private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ViewModel.Item = lst.SelectedValue; 
        //}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
