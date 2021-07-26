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
using Freedom.ZHPHMachine.View;

namespace Freedom.ZHPHMachine.Themes.Control
{
    /// <summary>
    /// MultiCheckBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiCheckBox : UserControl
    {
        public MultiCheckBox()
        {
            InitializeComponent();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                SelectedItems = listBox.SelectedItems;

            
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
           DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(MultiCheckBox));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedValuePathProperty =
         DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(MultiCheckBox));

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
          DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiCheckBox), new PropertyMetadata(null, (sender, e) =>
          {
              var ctrl = sender as MultiCheckBox;
              ctrl?.Listviewrefresh((IEnumerable)e.NewValue);
          }));

        private void Listviewrefresh(IEnumerable value)
        {
            listBox.ItemsSource = value;
            if (SelectedItems != null && ItemsSource != null)
            {
                foreach (var selectItem in SelectedItems)
                {
                    var selectValue = selectItem.GetType().GetProperty(SelectedValuePath).GetValue(selectItem, null);
                    foreach (var item in ItemsSource)
                    {
                        var value1 = item.GetType().GetProperty(SelectedValuePath).GetValue(item, null);
                        if (value1.Equals(selectValue))
                        {
                            listBox.SelectedItems.Add(item);
                            break;
                        }
                    }
                }
            }  
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
         DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiCheckBox));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }
       
        private void listBox_Loaded(object sender, RoutedEventArgs e)
        {
            listBox.DisplayMemberPath = DisplayMemberPath;

            if (SelectedItems != null && ItemsSource != null)
            {
                foreach (var selectItem in SelectedItems)
                {
                    var selectValue = selectItem.GetType().GetProperty(SelectedValuePath).GetValue(selectItem, null);
                    foreach (var item in ItemsSource)
                    {
                        var value1 = item.GetType().GetProperty(SelectedValuePath).GetValue(item, null);
                        if (value1.Equals(selectValue))
                        {
                            listBox.SelectedItems.Add(item);
                            break;
                        }
                    }
                }
            }
        }

        
    }
}
