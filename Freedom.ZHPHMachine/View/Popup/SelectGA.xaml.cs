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
    /// SelectGA.xaml 的交互逻辑
    /// </summary>
    public partial class SelectGA : Window
    {
        public SelectGA()
        {
            InitializeComponent();
            this.DataContext = new SelectGAViewModels();

        }

        public SelectGAViewModels ViewModel => this.DataContext as SelectGAViewModels;
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
