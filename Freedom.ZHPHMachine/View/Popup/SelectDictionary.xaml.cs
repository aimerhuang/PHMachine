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
using Freedom.ZHPHMachine.ViewModels;

namespace Freedom.ZHPHMachine.View.Popup
{
    /// <summary>
    /// SelectDictionary.xaml 的交互逻辑
    /// </summary>
    public partial class SelectDictionary : Window
    {
        public SelectDictionary(OperaType type, string key, string code = null, int status = 1)
        {
            InitializeComponent();
            //ViewModel.ItemsInfo.
            this.DataContext = new SelectDictionaryViewModels(type, key, code, status);
        }

        public SelectDictionaryViewModels ViewModel => this.DataContext as SelectDictionaryViewModels;

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
        }
    }
}
