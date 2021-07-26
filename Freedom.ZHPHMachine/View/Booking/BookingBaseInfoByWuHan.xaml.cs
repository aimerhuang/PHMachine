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
using System.Windows.Shapes;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.Themes.Control;
using Freedom.ZHPHMachine.ViewModels.Booking;
using MachineCommandService;

namespace Freedom.ZHPHMachine.View.Booking
{
    /// <summary>
    /// BookingBaseInfoByWuHan.xaml 的交互逻辑
    /// </summary>
    public partial class BookingBaseInfoByWuHan : Page
    {

        DateTime sysTime;
        public BookingBaseInfoByWuHan()
        {
            InitializeComponent();

            this.DataContext = new BookingBaseInfoByWuHanViewModel();
            sysTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());

            //启用计时器
            ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            ViewModel.ContentPage = this;
            ViewModel.TipMessage = "填写基本信息";

            if (ViewModel.OwnerViewModel.BookingBaseInfo.BookingSource == 0)
            {
                //设置背景色
            }

            //MyImage.Source = ViewModel.OwnerViewModel.DjzPhoto;
            //var a = ViewModel.OwnerViewModel.DjzPhoto;
            //北京不显示上一页
            if (ViewModel?.OwnerViewModel?.IsWuHan == true || ViewModel.OwnerViewModel?.IsTakePH_No == true)
            {
                ViewModel.NextPageShow = Visibility.Visible;
            }
            else
            {

                ViewModel.NextPageShow = ViewModel.PreviousPageShow = Visibility.Visible;

            }

        }

        public BookingBaseInfoByWuHanViewModel ViewModel => this.DataContext as BookingBaseInfoByWuHanViewModel;

        /// <summary>
        /// 判断有效期是否大于今天
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool GetVaildDate(string datetime)
        {
            if (string.IsNullOrEmpty(datetime))
                return false;
            Log.Instance.WriteInfo("当前时间：" + sysTime.ToString("yyyyMMdd").ToInt() + "证件有效期至：" + datetime.ToInt());
            return datetime.ToInt() < sysTime.ToString("yyyyMMdd").ToInt();

        }

        /// <summary>
        /// 显示和隐藏状态
        /// </summary>
        private void IsEnable()
        {
            //var a = ViewModel.GABookingInfo.REMARK;
            if (string.IsNullOrEmpty(ViewModel.HZBookingInfo?.XCZJHM))
            {
                HZHM.Text = "无证件信息";
                HZZJHM.Text = "";
                HZHM.Foreground = Brushes.Red;
                HZYXQ.Text = "";
                HZSXYY.Text = "";
                //HZSXYY.Text = "";
            }
            else if (ViewModel.HZBookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.HZBookingInfo?.XCZJYXQZ))
            {
                HZZJHM.Text = "失效";
                HZZJHM.Foreground = Brushes.Red;
                HZSXYY.Text = ViewModel.HZBookingInfo?.ZJSXYY;//失效原因
                HZSXYY.Foreground = Brushes.Red;
                HZYXQ.Text = "";

            }
            else
            {
                HZHM.Text = ViewModel.HZBookingInfo?.XCZJHM;
                HZHM.Foreground = Brushes.Black;
                HZZJHM.Text = "有效";
                HZZJHM.Foreground = Brushes.Green;
                //HZYXQ.Text = "有效期至：";
                HZYXQ.Text = "";
                HZSXYY.Text = "";
            }

            if (string.IsNullOrEmpty(ViewModel.GABookingInfo?.XCZJHM))
            {
                GAHM.Text = "无证件信息";
                GAZJHM.Text = "";
                GAHM.Foreground = Brushes.Red;
                GAYXQ.Text = "";
                GASXYY.Text = "";
            }
            else if (ViewModel.GABookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.GABookingInfo?.XCZJYXQZ))
            {
                GAZJHM.Text = "失效";
                GAZJHM.Foreground = Brushes.Red;
                //GAYXQ.Text = "有效期至：";
                GAYXQ.Text = "";
                GASXYY.Text = ViewModel.GABookingInfo?.ZJSXYY;
                GASXYY.Foreground = Brushes.Red;
            }
            else
            {
                GAHM.Text = ViewModel.GABookingInfo?.XCZJHM;
                GAHM.Foreground = Brushes.Black;
                GAZJHM.Text = "有效";
                GAZJHM.Foreground = Brushes.Green;
                //GAYXQ.Text = "有效期至：";
                GAYXQ.Text = "";
                GASXYY.Text = "";
            }

            if (string.IsNullOrEmpty(ViewModel.TWBookingInfo?.XCZJHM))
            {
                TWHM.Text = "无证件信息";
                TWZJHM.Text = "";
                TWHM.Foreground = Brushes.Red;
                TWYXQ.Text = "";
                TWSXYY.Text = "";
            }
            else if (ViewModel.TWBookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.TWBookingInfo?.XCZJYXQZ))
            {
                TWZJHM.Text = "失效";
                TWZJHM.Foreground = Brushes.Red;
                //TWYXQ.Text = "有效期至：";
                TWYXQ.Text = "";
                TWSXYY.Text = ViewModel.TWBookingInfo?.ZJSXYY;
                TWSXYY.Foreground = Brushes.Red;
            }
            else
            {
                TWHM.Text = ViewModel.TWBookingInfo?.XCZJHM;
                TWHM.Foreground = Brushes.Black;
                TWZJHM.Text = "有效";
                TWZJHM.Foreground = Brushes.Green;
                //TWYXQ.Text = "有效期至：";
                TWYXQ.Text = "";
                TWSXYY.Text = "";
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                listBox_Loaded(sender, e);
            if (IsLoaded)
                SelectedItems = ListBox.SelectedItems;

            //选中办证类型创建基础预约数据
            ViewModel.BookingBaseInfo.SelectCardTypes = SelectedItems;
            if (SelectedItems == null || SelectedItems.Count <= 0)
            {
                ViewModel.HZBookingInfo = ViewModel.GABookingInfo = ViewModel.TWBookingInfo = null;
                //BookingGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                //BookingGroup.Visibility = Visibility.Visible;
                //Log.Instance.WriteInfo("初始化办证类型成功！！！" + ViewModel.BookingBaseInfo.SelectCardTypes?.Count);
                if (ViewModel.OwnerViewModel?.IsWuHan == true && ViewModel.BookingBaseInfo.Address == null ||
                    ViewModel.OwnerViewModel?.IsWuHan == true && string.IsNullOrWhiteSpace(ViewModel.BookingBaseInfo?.Address?.Code))
                {
                    ViewModel.BookingBaseInfo.Address = new DictionaryType()
                    {
                        Code = ViewModel.BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                    };
                }

                //申请类型初始化
                string msgstr = "";
                if (ViewModel.BookingBaseInfo.SelectCardTypes.IsNotEmpty())
                    ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(ViewModel.BookingBaseInfo, out msgstr);

                //Log.Instance.WriteInfo("初始化办证类型成功！！！" + ViewModel.BookingBaseInfo.SelectCardTypes?.Count);
                ViewModel.HZBookingInfo = ViewModel.TWBookingInfo = ViewModel.GABookingInfo = null;
                if (ViewModel.BookingBaseInfo?.BookingInfo != null && ViewModel.BookingBaseInfo.BookingInfo.Count > 0)
                {

                    //有预约信息且在使用状态中
                    ViewModel.HZBookingInfo = ViewModel.BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.HZ).ToString()) && t.IsUse);
                    ViewModel.TWBookingInfo = ViewModel.BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.TWN).ToString()) && t.IsUse);
                    ViewModel.GABookingInfo = ViewModel.BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.HKGMAC).ToString()) && t.IsUse);
                    //Log.Instance.WriteInfo("初始化证件状态成功！！！");
                }

                if (ViewModel.HZBookingInfo != null && ViewModel.HZBookingInfo.ApplyType == null)
                {
                    //启动新线程 根据身份证查询护照信息
                    var paperInfo = ViewModel.QueryDocumentInfo(DocumentType.HZ, ViewModel.BookingBaseInfo?.CardInfo.IDCardNo);

                    if (paperInfo != null)
                    {
                        //存在护照信息
                        ViewModel.HZBookingInfo.XCZJZL = paperInfo.zjzl;
                        ViewModel.HZBookingInfo.XCZJHM = paperInfo.zjhm;
                        ViewModel.HZBookingInfo.XCZJQFRQ = paperInfo.qfrq;
                        ViewModel.HZBookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                        ViewModel.HZBookingInfo.DJSY = paperInfo.zjzt;
                        //更新护照加注信息
                        foreach (var item in ViewModel.HZBookingInfo.YYJZBLLIST)
                        {
                            item.ZJHM = paperInfo.zjhm;
                        }
                    }

                    ViewModel.HZBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                    //根据护照信息，给护照签注默认值
                    DataVerificate(EnumTypeSQLB.HZ, ViewModel.HZBookingInfo, out string msg);
                    //ViewModel.IsContinue = true;
                }


                if (ViewModel.GABookingInfo != null && ViewModel.GABookingInfo.ApplyType == null)
                {

                    //查询港澳通行证信息
                    var paperInfo = ViewModel.QueryDocumentInfo(DocumentType.GA, ViewModel.BookingBaseInfo.CardInfo.IDCardNo);
                    if (paperInfo != null)
                    {
                        //存在港澳通行证信息
                        ViewModel.GABookingInfo.XCZJZL = paperInfo.zjzl;
                        ViewModel.GABookingInfo.XCZJHM = paperInfo.zjhm;
                        ViewModel.GABookingInfo.XCZJQFRQ = paperInfo.qfrq;
                        ViewModel.GABookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                        ViewModel.GABookingInfo.DJSY = paperInfo.zjzt;
                    }

                    ViewModel.GABookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                    //根据港澳信息，给港澳签注默认值
                    DataVerificate(EnumTypeSQLB.HKGMAC, ViewModel.GABookingInfo, out string msg);
                    //ViewModel.GAQzType = true;
                }

                if (ViewModel.TWBookingInfo != null && ViewModel.TWBookingInfo.ApplyType == null)
                {
                    //存在台湾预约信息
                    //获取加注信息
                    //var item = TWBookingInfo.YYQZLIST[0];
                    //是否启用太极接口查询

                    //if (ViewModel.OwnerViewModel?.PaperInfos != null && ViewModel.OwnerViewModel?.PaperInfos?.Count > 0)
                    //{
                    //查询台湾证件信息
                    var paperInfo = ViewModel.QueryDocumentInfo(DocumentType.TW, ViewModel.BookingBaseInfo.CardInfo.IDCardNo);
                    if (paperInfo != null)
                    {
                        //存在台湾证件信息
                        ViewModel.TWBookingInfo.XCZJZL = paperInfo.zjzl;
                        ViewModel.TWBookingInfo.XCZJHM = paperInfo.zjhm;
                        ViewModel.TWBookingInfo.XCZJQFRQ = paperInfo.qfrq;
                        ViewModel.TWBookingInfo.XCZJYXQZ = paperInfo.zjyxqz;
                        ViewModel.TWBookingInfo.DJSY = paperInfo.zjzt;
                    }

                    //}
                    ViewModel.TWBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault();

                    //根据台湾证件信息，给台湾签注默认值
                    DataVerificate(EnumTypeSQLB.TWN, ViewModel.TWBookingInfo, out string msg);
                    //ViewModel.TWQzType = true;
                }
            }
            //显示隐藏
            IsEnable();

        }

        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="type">办理类别</param>
        /// <param name="info"></param>
        /// <param name="msg"></param>
        /// <param name="isApply"></param>
        private void DataVerificate(EnumTypeSQLB type, Models.ZHPHMachine.BookingInfo info, out string msg, bool isApply = true)
        {

            msg = "";
            var PaperInfo = ViewModel.OwnerViewModel?.PaperInfos;
            //获取证件有效期
            string zjyxq = info.XCZJYXQZ;
            //获取间隔时间
            int days = DaysInterval(info.XCZJYXQZ);

            if (type == EnumTypeSQLB.TWN && info != null)
            {
                var twnitem = info.YYQZLIST[0];
                //校验签注类型
                if (twnitem.QZType == null)
                {
                    //默认团签注
                    twnitem.QZType = new DictionaryType() { Code = "25", Description = "赴台团队旅游" };

                }
                //校验签注次数
                if (twnitem.QZCount == null)
                {
                    //默认6个月一次
                    twnitem.QZCount = new DictionaryType() { Code = "TWN6M1T", Description = "6个月一次" };
                }

            }
            if (type == EnumTypeSQLB.HKGMAC && info != null)
            {
                var hkitem = info.YYQZLIST[0];
                var macitem = info.YYQZLIST[1];

                if (hkitem.QZType == null)
                {
                    //广东省非全国异地办证默认个人旅游签注
                    if (info?.YDBZBS != "2" && ViewModel.BookingBaseInfo?.Address?.Code?.Substring(0, 2).Equals("44") == true)
                    {
                        //默认个签注
                        hkitem.QZType = new DictionaryType() { Code = "1B", Description = "个人旅游签注" };
                    }
                    else
                    {
                        //默认团签注
                        hkitem.QZType = new DictionaryType() { Code = "12", Description = "团队旅游签注" };
                    }

                    if (raqz.IsChecked == true)
                    {
                        //默认其他签注
                        hkitem.QZType = new DictionaryType() { Code = "19", Description = "其他签注" };
                    }


                }
                if (macitem.QZType == null)
                {
                    //广东省非全国异地办证默认个人旅游签注
                    if (info?.YDBZBS != "2" && ViewModel.BookingBaseInfo?.Address?.Code?.Substring(0, 2).Equals("44") == true)
                    {
                        //默认个签注
                        macitem.QZType = new DictionaryType() { Code = "1B", Description = "个人旅游签注" };
                    }
                    else
                    {
                        //默认团签注
                        macitem.QZType = new DictionaryType() { Code = "12", Description = "团队旅游签注" };
                    }

                    if (raqz.IsChecked == true)
                    {
                        //默认其他签注
                        macitem.QZType = new DictionaryType() { Code = "19", Description = "其他签注" };
                    }
                }
                if (hkitem.QZCount == null)
                {

                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365)
                    {
                        //证件有效期小于1年默认3个月一次
                        hkitem.QZCount = new DictionaryType() { Code = "HKG3M1T", Description = "3个月一次" };
                    }
                    else
                    {
                        //广东省非全国异地办证且深圳户口证件有效期大于一年默认1年多次
                        if (info?.YDBZBS != "2" && ViewModel.BookingBaseInfo?.Address?.Code?.Substring(0, 4).Equals("4403") == true)
                        {
                            hkitem.QZCount = new DictionaryType() { Code = "HKG1Y9T", Description = "1年多次" };
                        }
                        else
                        {
                            hkitem.QZCount = new DictionaryType() { Code = "HKG1Y1T", Description = "1年一次" };
                        }
                    }
                }
                if (macitem.QZCount == null)
                {
                    if (!string.IsNullOrWhiteSpace(zjyxq) && days < 365)
                    {
                        macitem.QZCount = new DictionaryType() { Code = "MAC3M1T", Description = "3个月一次" };
                    }
                    else
                    {
                        macitem.QZCount = new DictionaryType() { Code = "MAC1Y1T", Description = "1年一次" };
                    }
                }
            }

        }



        private void listBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitStatus();

            //var a = ViewModel.BookingBaseInfo?.SelectCardTypes;
            if (ViewModel.BookingBaseInfo?.SelectCardTypes == null || ViewModel.BookingBaseInfo?.SelectCardTypes.Count <= 0)
            {
                ViewModel.HZBookingInfo = ViewModel.GABookingInfo = ViewModel.TWBookingInfo = null;
                //BookingGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                //BookingGroup.Visibility = Visibility.Visible;
                SelectedItems = ViewModel.BookingBaseInfo?.SelectCardTypes;
                foreach (var i in SelectedItems)
                {
                    if (!ListBox.SelectedItems.Contains(i))
                    {
                        ListBox.SelectedItems.Add(i);
                    }

                }
            }

            if (SelectedItems != null && ItemsSource != null)
            {
                foreach (var selectItem in SelectedItems)
                {
                    var selectValue = selectItem.GetType().GetProperty(SelectedValuePath)?.GetValue(selectItem, null);
                    foreach (var item in ItemsSource)
                    {
                        var value1 = item.GetType().GetProperty(SelectedValuePath)?.GetValue(item, null);
                        if (value1 != null && value1.Equals(selectValue))
                        {
                            ListBox.SelectedItems.Add(item);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取接口所有申请类别
        /// </summary>
        /// <returns>申请类别集合</returns>
        private List<DictionaryType> GetApplyTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
            //护照申请类别
            var hzApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryHZ") ?? new List<DictionaryType>();
            //港澳申请类别
            var gaApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryGA") ?? new List<DictionaryType>();
            //台湾申请类别
            var twApplyTypes = config.Get<List<DictionaryType>>("ApplyCategoryTWN") ?? new List<DictionaryType>();
            var lstAll = hzApplyTypes.Union(gaApplyTypes).Union(twApplyTypes)?.GroupBy(t => new { t.Code, t.Description })
                .Select(t => new DictionaryType()
                {
                    Description = t.Key.Description,
                    Code = t.Key.Code
                }).ToList();
            return lstAll;
        }

        /// <summary>
        /// 获取时间与当前时间间隔
        /// </summary>
        /// <param name="zjyxq"></param>
        /// <returns></returns>
        private int DaysInterval(string zjyxq)
        {
            if (string.IsNullOrWhiteSpace(zjyxq)) { return 0; }
            //证件有效日期
            DateTime dt = DateTime.ParseExact(zjyxq, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            //当前日期
            DateTime currentdt = DateTime.Now;

            return dt.Subtract(currentdt).Days;
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
        /// 初始化状态
        /// </summary>
        private void InitStatus()
        {

            if (Xb.Text != null)
            {
                if (!Command.ValidationHelper.IsChinaChar(Xb.Text))
                    Xb.Text = Xb.Text == "1" ? "男" : "女";
            }

            if (ViewModel.BookingBaseInfo.BookingSource == 1)
            {
                YYSJ_txt.Text = "";
                YYTime.Text = "";
            }

            if (Mz.Text != null)
            {
                if (!Command.ValidationHelper.IsChinaChar(Mz.Text))
                    Mz.Text = GetDictionaryTypes().FirstOrDefault(t => t.Code == Mz.Text)?.Description;
            }


            if (string.IsNullOrWhiteSpace(ViewModel.TipsMsg))
            {
                ViewModel.TipsMsg = "普通群众";
            }


        }

        private void RadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            raall.IsChecked = true;
            raall2.IsChecked = true;
        }

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(BookingBaseInfoByWuHan));

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BookingBaseInfoByWuHan), new PropertyMetadata(null, (sender, e) =>
            {
                var ctrl = sender as BookingBaseInfoByWuHan;
                ctrl?.Listviewrefresh((IEnumerable)e.NewValue);
            }));

        private void Listviewrefresh(IEnumerable value)
        {
            ListBox.ItemsSource = value;
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
                            ListBox.SelectedItems.Add(item);
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
    }
}
