using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Freedom.ZHPHMachine.Themes.Control
{
    /// <summary>
    /// MultiCheckBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiRadioButton : UserControl
    {
        public MultiRadioButton()
        {
            InitializeComponent();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedValue == null) { return; }
            if (listBox.IsLoaded)
            {
                SelectedValue = listBox.SelectedValue;
            } 
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
           DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(MultiRadioButton));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedValuePathProperty =
         DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(MultiRadioButton));

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
          DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiRadioButton), new PropertyMetadata(null,
              (sender, e) =>
              {
                  var ctrl = sender as MultiRadioButton;
                  ctrl?.Listviewrefresh((IEnumerable)e.NewValue);
              }));

        private void Listviewrefresh(IEnumerable value)
        {
            listBox.ItemsSource = value;
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
         DependencyProperty.Register("SelectedValue", typeof(object), typeof(MultiRadioButton), new FrameworkPropertyMetadata( 
             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback((d,e)=> {
                 var ctrl = d as MultiRadioButton;
                 ctrl?.SelectItem(e.NewValue);
             })));
        private void SelectItem(object value)
        {
            this.listBox.SelectedValue = value;
        } 
     

        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set
            {
                SetValue(SelectedValueProperty, value);
            }
        }

        //private bool isload = false;
        private void listBox_Loaded(object sender, RoutedEventArgs e)
        {
            //if (isload) return;
            
            //listBox.ItemsSource = ItemsSource;
            listBox.DisplayMemberPath = DisplayMemberPath;
            listBox.SelectedValuePath = SelectedValuePath;
            //isload = true;
            //listBox.SelectedIndex = 0;
            //if (SelectedValue != null)
            //{
            //    listBox.SelectedValue = SelectedValue;
            //}
            //else
            //{
            //    listBox.SelectedIndex = 0;
            //}
        }
    }
}
