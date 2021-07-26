using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Freedom.Config;
using Freedom.ZHPHMachine.ViewModels;
using Freedom.ZHPHMachine.ViewModels.Popup;

namespace Freedom.ZHPHMachine.View.Popup
{
    /// <summary>
    /// SelectDic.xaml 的交互逻辑
    /// </summary>
    public partial class SelectDic : Window
    {
        public SelectDic(OperaType type, string key, string code = null, int status = 1)
        {
            InitializeComponent();
            this.DataContext = new SelectDicViewModels(type, key, code, status);
            //ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            //修改选择预约时间时触发的事件
            ((INotifyCollectionChanged)lst.Items).CollectionChanged +=
                lst_CollectionChanged;
        }
        public SelectDicViewModels ViewModel => this.DataContext as SelectDicViewModels;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Lst_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var count = ViewModel.ItemsInfo?.Count;
            if (count == 1)
            {
                lst.SelectedIndex = 0;
            }
        }
        private void lst_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                var count = ViewModel.ItemsInfo?.Count;
                if (count == 1)
                {
                    lst.SelectedIndex = 0;
                }
            }

        }

        private void Lst_OnLoaded(object sender, RoutedEventArgs e)
        {
            var count = ViewModel.ItemsInfo?.Count;
            if (count == 1)
            {
                lst.SelectedIndex = 0;
            }
        }
    }
}
