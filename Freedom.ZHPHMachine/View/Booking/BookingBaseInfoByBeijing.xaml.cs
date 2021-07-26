using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Models;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.Themes.Control;
using Freedom.ZHPHMachine.ViewModels;
using Freedom.ZHPHMachine.ViewModels.Booking;
using MachineCommandService;

namespace Freedom.ZHPHMachine.View.Booking
{
    /// <summary>
    /// BookingBaseInfoByBeijing.xaml 的交互逻辑
    /// </summary>
    public partial class BookingBaseInfoByBeijing : Page
    {
        DateTime sysTime;
        public BookingBaseInfoByBeijing()
        {
            InitializeComponent();


            this.DataContext = new BookingBaseInfoByBeijingViewModel(this);

            ViewModel.OwnerViewModel.IsDqzType = 0;

            sysTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());

            //启用计时器 不应该放在这里写 by wei.chen 会导致跳转操时页 
            //ViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            ViewModel.ContentPage = this;
            ViewModel.TipMessage = "填写基本信息";
            ViewModel.OwnerViewModel.IsBackShow = Visibility.Visible;

            if (ViewModel.OwnerViewModel.BookingBaseInfo.BookingSource == 0)
            {
                //设置背景色
            }

            //MyImage.Source = ViewModel.OwnerViewModel.DjzPhoto;
            //var a = ViewModel.OwnerViewModel.DjzPhoto;
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


        public BookingBaseInfoByBeijingViewModel ViewModel => this.DataContext as BookingBaseInfoByBeijingViewModel;

        /// <summary>
        /// 判断有效期是否小于今天
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool GetVaildDate(string datetime)
        {
            if (string.IsNullOrEmpty(datetime))
                return false;
            //Log.Instance.WriteInfo("当前时间：" + sysTime.ToString("yyyyMMdd").ToInt() + "证件有效期至：" + datetime.ToInt());

            return datetime.ToInt() < sysTime.ToString("yyyyMMdd").ToInt();
            //DateTime dt = DateTime.ParseExact(datetime, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            //return DateTime.Compare(sysTime, dt) > 0;
        }

        /// <summary>
        /// 显示和隐藏状态
        /// </summary>
        private void IsEnable()
        {
            //获取自然月三个月零30天天数
            int daysInMonth = GetDaysInMonth(DateTime.Now, 3) + 30;

            if (ViewModel?.GABookingInfo?.ApplyType == null)
            {
                GACheckBox.IsChecked = false;
            }

            //TWCheckBox.Visibility = string.IsNullOrEmpty(ViewModel?.TWBookingInfo?.XCZJHM) ? Visibility.Collapsed : Visibility.Visible;


            if (ViewModel?.TWBookingInfo?.ApplyType == null)
            {
                TWCheckBox.IsChecked = false;
            }


            if (string.IsNullOrEmpty(ViewModel.HZBookingInfo?.XCZJHM))
            {
                HZZJHM.Text = "无证件信息";
                HZZJHM.Foreground = Brushes.Red;
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
                HZZJHM.Text = "有效";
                HZZJHM.Foreground = Brushes.Green;
                //HZYXQ.Text = "有效期至：";
                HZYXQ.Text = "";
                HZSXYY.Text = "";
            }

            //港澳通行证
            if (string.IsNullOrEmpty(ViewModel.GABookingInfo?.XCZJHM))
            {
                GAZJHM.Text = "无证件信息";
                GAZJHM.Foreground = Brushes.Red;
                GAYXQ.Text = "";
                GACheckBox.Visibility = Visibility.Collapsed;
                //GASXYY.Text = "";
            }
            else if (ViewModel.GABookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.GABookingInfo?.XCZJYXQZ))
            {
                GAZJHM.Text = "失效";
                GAZJHM.Foreground = Brushes.Red;
                GAYXQ.Text = "";
                GACheckBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                GAZJHM.Text = "有效";
                GAZJHM.Foreground = Brushes.Green;
                GAYXQ.Text = "";
                //仅签注按钮显示 条件：
                //1.可办业务存在持证申请签注
                //2.证件状态为有效
                //3.证件号码正确
                //2021-5-11 14:09:07 增加 证件有效期 < 3个月30天 不显示仅签注
                var qzListGa = ViewModel?.OwnerViewModel?.KbywInfos?.Select(t =>
                        t.sqlb == ((int)EnumTypeSQLB.HKGMAC).ToString() &&
                        t.bzlb == ((int)EnumTypeBZLB.BZLB92).ToString())
                    .ToList();

                //获取有效天数
                int days = GetValidDay(DateTime.ParseExact(ViewModel?.GABookingInfo?.XCZJYXQZ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));

                if (qzListGa?.Count > 0
                    && ViewModel.GABookingInfo?.DJSY == "1"
                    && !string.IsNullOrEmpty(ViewModel?.GABookingInfo?.XCZJHM)
                    && days > daysInMonth)
                {
                    GACheckBox.Visibility = Visibility.Visible;
                }
                else
                {
                    GACheckBox.Visibility = Visibility.Collapsed;
                }
            }

            //台湾证
            if (string.IsNullOrEmpty(ViewModel.TWBookingInfo?.XCZJHM))
            {
                TWZJHM.Text = "无证件信息";
                TWZJHM.Foreground = Brushes.Red;
                TWYXQ.Text = "";
                TWCheckBox.Visibility = Visibility.Collapsed;
            }
            else if (ViewModel.TWBookingInfo?.DJSY == "0" || GetVaildDate(ViewModel.TWBookingInfo?.XCZJYXQZ))
            {
                TWZJHM.Text = "失效";
                TWZJHM.Foreground = Brushes.Red;
                TWYXQ.Text = "";
                TWCheckBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                TWZJHM.Text = "有效";
                TWZJHM.Foreground = Brushes.Green;
                TWYXQ.Text = "";
                //仅签注按钮显示 条件：
                //1.可办业务存在持证申请签注
                //2.证件状态为有效
                //3.证件号码正确
                //2021-5-11 14:09:07 增加 证件有效期 < 3个月30天 不显示仅签注
                var qzListTw = ViewModel?.OwnerViewModel?.KbywInfos?.Select(t =>
                        t.sqlb == ((int)EnumTypeSQLB.TWN).ToString() &&
                        t.bzlb == ((int)EnumTypeBZLB.BZLB91).ToString())
                    .ToList();

                //获取有效天数
                int days = GetValidDay(DateTime.ParseExact(ViewModel?.TWBookingInfo?.XCZJYXQZ, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));

                if (qzListTw?.Count > 0
                    && ViewModel.TWBookingInfo?.DJSY == "1"
                    && !string.IsNullOrEmpty(ViewModel?.TWBookingInfo?.XCZJHM)
                    && days > daysInMonth)
                {
                    TWCheckBox.Visibility = Visibility.Visible;
                }
                else
                {
                    TWCheckBox.Visibility = Visibility.Collapsed;
                }
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
                if (ViewModel.OwnerViewModel?.IsBeijing == true && ViewModel.BookingBaseInfo.Address == null ||
                    ViewModel.OwnerViewModel?.IsBeijing == true && string.IsNullOrWhiteSpace(ViewModel.BookingBaseInfo?.Address?.Code))
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
                    //是否启用太极接口查询
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.HZ);
                        if (paperWorkTask != null)
                        {
                            ViewModel.HZBookingInfo.XCZJZL = paperWorkTask.zjzl;
                            ViewModel.HZBookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //HZBookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            ViewModel.HZBookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            ViewModel.HZBookingInfo.DJSY = paperWorkTask?.zjzt;
                            ViewModel.HZBookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            ViewModel.HZBookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;
                            //更新护照加注信息
                            foreach (var item in ViewModel.HZBookingInfo.YYJZBLLIST)
                            {
                                item.ZJHM = paperWorkTask.zjhm;
                            }
                        }
                        else
                        {
                            if (ViewModel.OwnerViewModel?.PaperInfos != null && ViewModel.OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var HZPaperInfo = ViewModel.OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.HZ.GetHashCode().ToString()).ToList();
                                if (HZPaperInfo != null && HZPaperInfo.Count > 0)
                                {
                                    ViewModel.HZBookingInfo.XCZJYXQZ = HZPaperInfo.FirstOrDefault()?.zjyxqz;
                                    ViewModel.HZBookingInfo.XCZJHM = HZPaperInfo.FirstOrDefault()?.zjhm;
                                    ViewModel.HZBookingInfo.XCZJZL = HZPaperInfo.FirstOrDefault()?.zjzl;
                                    ViewModel.HZBookingInfo.DJSY = HZPaperInfo.FirstOrDefault()?.zjzt;
                                    foreach (var i in ViewModel.HZBookingInfo.YYJZBLLIST)
                                    {
                                        i.ZJHM = HZPaperInfo.FirstOrDefault()?.zjhm;
                                    }
                                }

                            }
                        }
                    }

                    //默认第一个可办业务办证类别
                    if (ViewModel.OwnerViewModel?.KbywInfos != null && ViewModel.OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Hz = ViewModel.OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.HZ).ToString()).ToList();
                        if (Hz.Count > 0)
                        {
                            string Bzlb = string.Empty;
                            //外网预约存在预约信息
                            //1.预约办证类别重新赋值  
                            //2.预约加注信息赋值
                            if (ViewModel.BookingBaseInfo.Book_Type == "0" &&
                                ViewModel.OwnerViewModel.YyywInfo != null &&
                                ViewModel.OwnerViewModel.YyywInfo?.Length > 0)
                            {
                                foreach (var item in ViewModel.OwnerViewModel.YyywInfo)
                                {
                                    if (item.sqlb == ((int)EnumTypeSQLB.HZ).ToString())
                                    {
                                        Bzlb = item?.bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb);
                                        //预约信息不在可办业务范围里，默认可以办业务第一条
                                        if (Hz.Where(t => t.bzlb.Contains(Bzlb))?.ToList().Count == 0)
                                        {
                                            Bzlb = Hz.FirstOrDefault().bzlb;
                                            Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                        }
                                        foreach (var i in ViewModel.HZBookingInfo?.YYJZBLLIST)
                                        {
                                            i.YWBH = ViewModel.HZBookingInfo?.YWBH;
                                            i.BGJZZL = item?.jzxxs?.FirstOrDefault()?.jzzl;
                                            i.BGJZXM = item?.jzxxs?.FirstOrDefault()?.jznr;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Bzlb = Hz.FirstOrDefault().bzlb;
                            }

                            ViewModel.HZBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);
                        }

                    }
                }

                if (ViewModel.GABookingInfo != null && ViewModel.GABookingInfo.ApplyType == null)
                {
                    //存在港澳信息
                    //获取港澳加注信息
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.GA);
                        if (paperWorkTask != null)
                        {
                            ViewModel.GABookingInfo.XCZJZL = paperWorkTask.zjzl;
                            ViewModel.GABookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //GABookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            ViewModel.GABookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            ViewModel.GABookingInfo.DJSY = paperWorkTask.zjzt;
                            ViewModel.GABookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            ViewModel.GABookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;
                        }
                        else
                        {
                            if (ViewModel.OwnerViewModel?.PaperInfos != null && ViewModel.OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var GAPaperInfo = ViewModel.OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.GA.GetHashCode().ToString()).ToList();
                                if (GAPaperInfo != null && GAPaperInfo.Count > 0)
                                {
                                    ViewModel.GABookingInfo.XCZJYXQZ = GAPaperInfo.FirstOrDefault().zjyxqz;
                                    ViewModel.GABookingInfo.XCZJHM = GAPaperInfo.FirstOrDefault().zjhm;
                                    ViewModel.GABookingInfo.XCZJZL = GAPaperInfo.FirstOrDefault().zjzl;
                                    ViewModel.GABookingInfo.DJSY = GAPaperInfo.FirstOrDefault().zjzt;
                                }

                            }
                        }
                    }
                    //无预约默认第一个可办业务办证类别
                    if (ViewModel.OwnerViewModel?.KbywInfos != null && ViewModel.OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Ga = ViewModel.OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.HKGMAC).ToString()).ToList();
                        if (Ga.Count > 0)
                        {
                            string Bzlb = string.Empty;
                            //外网预约存在预约信息
                            //1.预约办证类别重新赋值  
                            //2.预约加注信息赋值
                            if (ViewModel.BookingBaseInfo.Book_Type == "0" &&
                                ViewModel.OwnerViewModel.YyywInfo != null &&
                                ViewModel.OwnerViewModel.YyywInfo?.Length > 0)
                            {
                                foreach (var item in ViewModel.OwnerViewModel.YyywInfo)
                                {
                                    if (item.sqlb == ((int)EnumTypeSQLB.HKGMAC).ToString())
                                    {
                                        if (item.qzxxs.Length == 1 && ViewModel.GABookingInfo?.YYQZLIST.Count > 1)
                                        {
                                            ViewModel.GABookingInfo?.YYQZLIST.Remove(ViewModel.GABookingInfo?.YYQZLIST.Where(t => t.XH == 2).ToList().FirstOrDefault());
                                        }
                                        Bzlb = item?.bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb);

                                        //预约信息不在可办业务范围里，默认可以办业务第一条
                                        if (Ga.Where(t => t.bzlb.Contains(Bzlb))?.ToList().Count == 0)
                                        {
                                            Bzlb = Ga.FirstOrDefault(t => t.bzlb.Contains("31"))?.bzlb;
                                            //Bzlb = Ga.FirstOrDefault(t => t.bzlb == ((int)EnumTypeBZLB.BZLB31).ToString()).bzlb;
                                            if (Bzlb.IsEmpty())
                                                Bzlb = Ga.FirstOrDefault().bzlb;
                                            Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                        }
                                        if (item.qzxxs.Length > 0)
                                        {
                                            foreach (var t in item.qzxxs)
                                            {
                                                if (t.yxq.IsEmpty())
                                                    t.yxq = "Y03";

                                                foreach (var i in ViewModel.GABookingInfo?.YYQZLIST)
                                                {
                                                    if (i.QWD == t.qwd)
                                                    {
                                                        i.YWBH = ViewModel.GABookingInfo?.YWBH;
                                                        i.QZZL = t?.qzzl.IsEmpty() == true ? "1B" : t?.qzzl;
                                                        i.QZType = GetQZTypes(t?.qzzl).FirstOrDefault();
                                                        i.QWD = t?.qwd;
                                                        i.QZYXCS = t?.yxcs;
                                                        i.QZYXQ = ViewModel.ConverYxq(t?.yxq);//太极有效期转换
                                                        i.QZYXQDW = ViewModel.GetYxqDw(t?.yxq);//年；月；周；日
                                                        //i.QZType = GetQZTypes(item?.qzxxs?.FirstOrDefault()?.qzzl).FirstOrDefault();
                                                        //i.QWD = item?.qzxxs?.FirstOrDefault()?.qwd;
                                                        //i.QZYXCS = item?.qzxxs?.FirstOrDefault()?.yxcs;
                                                        //i.QZZL = item?.qzxxs?.FirstOrDefault()?.qzzl;
                                                        //i.QZYXQ = ConverYxq(item?.qzxxs?.FirstOrDefault()?.yxq);//太极有效期转换
                                                        //i.QZYXQDW = GetYxqDw(item?.qzxxs?.FirstOrDefault()?.yxq);//年；月；周；日
                                                        i.ZJHM = ViewModel.GABookingInfo.XCZJHM;
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            ViewModel.GABookingInfo.YYQZLIST = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Bzlb = Ga.Where(t => t.bzlb.Contains("31"))?.ToList()?.Count > 0
                                    ? Ga.FirstOrDefault(t => t.bzlb == ((int)EnumTypeBZLB.BZLB31).ToString()).bzlb
                                    : Ga.FirstOrDefault().bzlb;

                            }

                            ViewModel.GABookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);
                        }

                    }

                }

                if (ViewModel.TWBookingInfo != null && ViewModel.TWBookingInfo.ApplyType == null)
                {
                    //存在台湾预约信息
                    //获取加注信息

                    //是否启用太极接口查询
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.TW);
                        if (paperWorkTask != null)
                        {
                            ViewModel.TWBookingInfo.XCZJZL = paperWorkTask.zjzl;
                            ViewModel.TWBookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //TWBookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            ViewModel.TWBookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            ViewModel.TWBookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            ViewModel.TWBookingInfo.DJSY = paperWorkTask?.zjzt;
                            ViewModel.TWBookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;

                            //更新护照加注信息
                            foreach (var items in ViewModel.TWBookingInfo.YYJZBLLIST)
                            {
                                items.ZJHM = paperWorkTask.zjhm;
                            }
                        }
                        else
                        {
                            if (ViewModel.OwnerViewModel?.PaperInfos != null && ViewModel.OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var TWPaperInfo = ViewModel.OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.TW.GetHashCode().ToString()).ToList();
                                if (TWPaperInfo != null && TWPaperInfo.Count > 0)
                                {
                                    ViewModel.TWBookingInfo.XCZJYXQZ = TWPaperInfo.FirstOrDefault().zjyxqz;
                                    ViewModel.TWBookingInfo.XCZJHM = TWPaperInfo.FirstOrDefault().zjhm;
                                    ViewModel.TWBookingInfo.XCZJZL = TWPaperInfo.FirstOrDefault().zjzl;
                                    ViewModel.TWBookingInfo.DJSY = TWPaperInfo.FirstOrDefault().zjzt;
                                    foreach (var i in ViewModel.TWBookingInfo.YYJZBLLIST)
                                    {
                                        i.ZJHM = TWPaperInfo.FirstOrDefault().zjhm;
                                    }
                                }

                            }
                        }
                    }

                    //无预约默认第一个可办业务办证类别
                    if (ViewModel.OwnerViewModel?.KbywInfos != null && ViewModel.OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Hz = ViewModel.OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.TWN).ToString()).ToList();
                        if (Hz.Count > 0)
                        {
                            string Bzlb = string.Empty;
                            //外网预约存在预约信息
                            //1.预约办证类别重新赋值  
                            //2.预约加注信息赋值
                            if (ViewModel.BookingBaseInfo.Book_Type == "0" &&
                                ViewModel.OwnerViewModel.YyywInfo != null &&
                                ViewModel.OwnerViewModel.YyywInfo?.Length > 0)
                            {
                                foreach (var item in ViewModel.OwnerViewModel.YyywInfo)
                                {
                                    if (item.sqlb == ((int)EnumTypeSQLB.TWN).ToString())
                                    {
                                        Bzlb = item?.bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb);
                                        //预约信息不在可办业务范围里，默认可以办业务换发
                                        if (Hz.Where(t => t.bzlb.Contains(Bzlb))?.ToList().Count == 0)
                                        {
                                            Bzlb = Hz.FirstOrDefault(t => t.bzlb.Contains("31"))?.bzlb;
                                            //Bzlb = Hz.FirstOrDefault(t => t.bzlb == ((int)EnumTypeBZLB.BZLB31).ToString()).bzlb;
                                            if (Bzlb.IsEmpty())
                                                Bzlb = Hz.FirstOrDefault().bzlb;
                                            Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                        }
                                        if (item.qzxxs.Length > 0)
                                        {
                                            if (item?.qzxxs?.FirstOrDefault()?.yxq?.IsEmpty() == true)
                                                item.qzxxs.FirstOrDefault().yxq = "Y06";//无有效期默认6月一次

                                            foreach (var i in ViewModel.TWBookingInfo?.YYQZLIST)
                                            {
                                                i.YWBH = ViewModel?.TWBookingInfo?.YWBH;
                                                i.QZZL = item?.qzxxs?.FirstOrDefault()?.qzzl?.IsEmpty() == true ? "25" : item?.qzxxs?.FirstOrDefault()?.qzzl;//台湾无签注默认团签
                                                i.QZType = GetQZTypes(item?.qzxxs?.FirstOrDefault()?.qzzl).FirstOrDefault();
                                                i.QWD = item?.qzxxs?.FirstOrDefault()?.qwd;
                                                i.QZYXCS = item?.qzxxs?.FirstOrDefault()?.yxcs;
                                                i.QZYXQ = ViewModel.ConverYxq(item?.qzxxs?.FirstOrDefault()?.yxq);//太极有效期转换
                                                i.QZYXQDW = ViewModel.GetYxqDw(item?.qzxxs?.FirstOrDefault()?.yxq);
                                                i.ZJHM = ViewModel.TWBookingInfo.XCZJHM;
                                            }
                                        }
                                        else
                                        {
                                            ViewModel.TWBookingInfo.YYQZLIST = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Bzlb = Hz.Where(t => t.bzlb.Contains("31"))?.ToList()?.Count > 0
                                    ? Hz.FirstOrDefault(t => t.bzlb == ((int)EnumTypeBZLB.BZLB31).ToString()).bzlb
                                    : Hz.FirstOrDefault().bzlb;
                            }

                            ViewModel.TWBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);
                        }

                    }

                }

            }
            //显示隐藏
            IsEnable();

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
        /// 查询太极证件信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ZjxxInfo QueryPaperWorkTask(DocumentType type)
        {
            //Log.Instance.WriteInfo("开始查询太极证件信息...");
            ZjxxInfo model = null;
            try
            {
                if (!TjConfig.TjModel.IsConnectionTj)
                {
                    return null;
                }

                if (ViewModel.OwnerViewModel?.PaperWork != null && ViewModel.OwnerViewModel?.PaperWork.Length > 0)
                {
                    var paperWork = ViewModel.OwnerViewModel?.PaperWork.Where(t => t.zjzl == type.GetHashCode().ToString())
                        .ToList();
                    if (paperWork.Count > 0)
                    {
                        model = ViewModel.OwnerViewModel.PaperWork.Where(t => t.zjzl == type.GetHashCode().ToString())
                            ?.OrderByDescending(t => t.zjyxqz.FirstOrDefault()).ToList()[0];
                    }

                }

            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("太极接口查询证件信息异常");
                Log.Instance.WriteError($"[太极接口查询证件信息异常]{ex.Message}");
            }

            return model;
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
        /// 取签注类型
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetQZTypes(string qztype)
        {
            return ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()
                ?.Where(t => t.KindType == ((int)KindType.QZType).ToString() && t.Code == qztype).OrderBy(t => t.Code).ToList();
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

        /// <summary>
        /// 返回自然月天数(比如当前月三个月)
        /// </summary>
        /// <param name="dt">时间</param>
        /// <param name="count">次数</param>
        /// <returns>天数</returns>
        private int GetDaysInMonth(DateTime dt, int count = 1)
        {
            int days = DateTime.DaysInMonth(dt.Year, dt.Month);
            for (int i = 1; i < count; i++)
            {
                dt = dt.AddMonths(1);
                days += DateTime.DaysInMonth(dt.Year, dt.Month);
            }
            return days;
        }

        /// <summary>
        /// 获取证件有效天数
        /// </summary>
        /// <param name="dt">证件有效期</param>
        /// <returns>天数（不小于0）</returns>
        private int GetValidDay(DateTime dt)
        {
            DateTime oldDate = dt;
            DateTime newDate = DateTime.Now;
            TimeSpan ts = oldDate - newDate;
            int differenceInDays = ts.Days;
            if (differenceInDays <= 0)
                return 0;
            return differenceInDays;
        }


        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(BookingBaseInfoByBeijing));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(BookingBaseInfoByBeijing));

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BookingBaseInfoByBeijing), new PropertyMetadata(null, (sender, e) =>
            {
                var ctrl = sender as BookingBaseInfoByBeijing;
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

        private void TWCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.OwnerViewModel.IsDqzType += 1;
            Log.Instance.WriteInfo("选中【台湾】仅签注按钮");
        }

        private void TWCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.OwnerViewModel.IsDqzType -= 1;
            Log.Instance.WriteInfo("取消选择【台湾】仅签注按钮");
        }

        private void GACheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.OwnerViewModel.IsDqzType += 1;
            Log.Instance.WriteInfo("选中【港澳】仅签注按钮");
        }

        private void GACheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.OwnerViewModel.IsDqzType -= 1;
            Log.Instance.WriteInfo("取消选择【港澳】仅签注按钮");
        }
    }
}
