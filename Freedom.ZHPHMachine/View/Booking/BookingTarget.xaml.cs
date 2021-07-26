using Freedom.Config;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// BookingTarget.xaml 的交互逻辑
    /// </summary>
    public partial class BookingTarget : Page
    {
        public BookingTarget()
        {
            InitializeComponent();

            this.DataContext = new BookingTargetViewModels();
            //启用计时器
            ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
            //ViewModel.OwnerViewModel.TimeOutNew();
            ViewModel.ContentPage = this;
            ViewModel.NextPageShow = Visibility.Visible;
            //修改选择预约时间时触发的事件
            ((INotifyCollectionChanged)lst.Items).CollectionChanged +=
                lst_CollectionChanged;
        }

        public BookingTargetViewModels ViewModel => this.DataContext as BookingTargetViewModels;

        private void lst_Loaded(object sender, RoutedEventArgs e)
        {
            var count = ViewModel.BookingTagetInfo?.Count;
            if (count == 1)
            {
                lst.SelectedIndex = 0;
            }
            
        }

        private void Lst_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var count = ViewModel.BookingTagetInfo?.Count;
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
                var count = ViewModel.BookingTagetInfo?.Count;
                if (count == 1)
                {
                    lst.SelectedIndex = 0;
                }
            }

        }
    }
}
