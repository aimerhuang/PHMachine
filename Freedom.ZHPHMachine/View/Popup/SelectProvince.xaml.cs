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
    public partial class SelectProvince : Window
    {
        public SelectProvince(string title, string code)
        {
            InitializeComponent();
            this.DataContext = new SelectProvinceViewModels(title);
            ViewModel.DateInit(code);
            
        }

        public SelectProvinceViewModels ViewModel => this.DataContext as SelectProvinceViewModels;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Result.Code.Length == 6)
            {
                ViewModel.HasAddress = ViewModel.Result.Description;
                DialogResult = true;
            } 
        } 

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            lstProvince.SelectedValue = null;
        }
    }
}
