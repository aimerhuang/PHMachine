using Freedom.Common;
using Freedom.Config;
using Freedom.ZHPHMachine.Common;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Command;
using MachineCommandService;

namespace Freedom.ZHPHMachine.View
{
    /// <summary>
    /// BookingTarget.xaml 的交互逻辑
    /// </summary>
    public partial class BookingBaseInfo : Page
    {
        public BookingBaseInfo()
        {
            InitializeComponent();

            this.DataContext = new BookingBaseInfoViewModels();

            //启用计时器
            ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            ViewModel.ContentPage = this;
            ViewModel.TipMessage = "填写基本信息";
            //北京不显示上一页
            if (ViewModel?.OwnerViewModel?.IsBeijing == true || ViewModel.OwnerViewModel?.IsTakePH_No == true)
            {
                ViewModel.NextPageShow = Visibility.Visible;
            }
            else
            {

                ViewModel.NextPageShow = ViewModel.PreviousPageShow = Visibility.Visible;

            }
        }

        public BookingBaseInfoViewModels ViewModel => this.DataContext as BookingBaseInfoViewModels;

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            InitStatus();
        }

        /// <summary>
        /// 初始化状态
        /// </summary>
        private void InitStatus()
        {
            if (Names.IsNotEmpty() && IdNo.IsNotEmpty())
            {
                Names.Text = CommandTools.ReplaceWithSpecialChar(Names.Text);
                IdNo.Text = CommandTools.ReplaceWithSpecialChar(IdNo.Text);
            }



            if (!string.IsNullOrEmpty(Xb.Text))
            {
                if (!Command.ValidationHelper.IsChinaChar(Xb.Text))
                    Xb.Text = Xb.Text == "1" ? "男" : "女";
            }



            if (ViewModel.BookingBaseInfo.BookingSource == 1)
            {
                YYSJ_txt.Text = "";
                YYTime.Text = "";
            }

            if (!string.IsNullOrEmpty(Mz.Text))
            {
                if (!Command.ValidationHelper.IsChinaChar(Mz.Text))
                    Mz.Text = GetDictionaryTypes().FirstOrDefault(t => t.Code == Mz.Text)?.Description;
            }
            else
            {
                ViewModel.BookingBaseInfo.CardInfo.pNational = "01";
                Mz.Text = "汉";

            }


            if (string.IsNullOrWhiteSpace(ViewModel.TipsMsg))
            {
                ViewModel.TipsMsg = "普通群众";
            }


        }

        private void Ra1_Checked(object sender, RoutedEventArgs e)
        {
            if (ra1.IsChecked == true) ViewModel.BookingBaseInfo.IsExpress = 0;
            else ViewModel.BookingBaseInfo.IsExpress = 1;

            UpdateStatus();

        }

        private void Ra1_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.BookingBaseInfo == null) { return; }
            if (ViewModel.BookingBaseInfo.IsExpress == 0) ra1.IsChecked = true;
            else ra2.IsChecked = true;

            LoadStatus();

        }

        /// <summary>
        /// 取民族
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetDictionaryTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            List<DictionaryType> lst = new List<DictionaryType>();

            lst = config.Get<List<DictionaryType>>();
            lst = lst?.Where(t => t.KindType == ((int)KindType.MZ).ToString() && t.Status == 1)?.ToList();

            return lst;
        }

        /// <summary>
        /// 动态显示邮寄信息
        /// </summary>
        public void UpdateStatus()
        {
            txtAddress.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            BtnAddress.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            txtName.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            BtnName.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            txtRecipientPhone.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            BtnRecipientPhone.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            txtYzbm.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
            BtnYzbm.Visibility = ViewModel.BookingBaseInfo.IsExpress == 1 ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// 默认邮寄信息
        /// </summary>
        public void LoadStatus()
        {
            ViewModel.BookingBaseInfo.EMSAddress = ViewModel.BookingBaseInfo.CardInfo.Address;
            ViewModel.BookingBaseInfo.HasEMSAddress = ViewModel.BookingBaseInfo.CardInfo.Address;
            //ViewModel.BookingBaseInfo.Address?.Description + 
            //ViewModel.BookingBaseInfo.EMSCode = ViewModel.BookingBaseInfo.CardInfo.hkszd;
            //ViewModel.BookingBaseInfo.HasEMSCode = ViewModel.BookingBaseInfo.CardInfo.hkszd;
            ViewModel.BookingBaseInfo.RecipientName = ViewModel.BookingBaseInfo.CardInfo.FullName;
            ViewModel.BookingBaseInfo.HasRecipientName = ViewModel.BookingBaseInfo.CardInfo.FullName;
            ViewModel.BookingBaseInfo.RecipientTelephone = ViewModel.BookingBaseInfo.Telephone;
            ViewModel.BookingBaseInfo.HasRecipientTelephone = ViewModel.BookingBaseInfo.Telephone;

        }

    }
}
