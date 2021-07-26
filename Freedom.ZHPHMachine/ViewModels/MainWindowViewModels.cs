using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Freedom.BLL;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.ZHPHMachine.ViewModels.CommandViewModels;

namespace Freedom.ZHPHMachine.ViewModels
{
    using Freedom.Controls.Foundation;
    using System.Windows.Media.Imaging;
    using System.Windows;
    using System.IO;
    using Freedom.Common;
    using System.Threading;
    using Freedom.Config;
    using Freedom.Models;
    using Freedom.Models.DataBaseModels;
    using Freedom.Hardware;
    using MachineCommandService;
    using Freedom.Common.HsZhPjh.Enums;
    using Freedom.ZHPHMachine.Common;
    using Freedom.Models.ZHPHMachine;
    using Freedom.ZHPHMachine.Command;
    using Freedom.Controls;

    public partial class MainWindowViewModels : ViewModelBase
    {

        #region 字段
        private static MainWindowViewModels _Instance;
        /// <summary>
        /// 单例
        /// </summary>
        public static MainWindowViewModels Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MainWindowViewModels();
                return _Instance;
            }
        }
        private ZHPHMachineWSHelper dbHelper = ZHPHMachineWSHelper.ZHPHInstance;
        private ElementManager config = ServiceRegistry.Instance.Get<ElementManager>();
        public static Dev_AlarmInfo DevAlarm = new Dev_AlarmInfo();//设备故障信息
        public string Dev_ID = QJTConfig.QJTModel.QJTDevInfo?.Dev_ID.ToString();
        public static string dbUrl = DBConfig.DBModel.WebServiceUrl;
        public List<DictionaryTypeByPinYin> DictionaryTypeByPinYin;
        private CrjPreapplyManager crjManager = new CrjPreapplyManager();
        public static Socket Client;
        //public bool IsNext = false;

        //毫秒
        private int RefreshDevStatusSeconds = 20000;
        #endregion

        #region 属性 

        public decimal _devid = 0;
        public DEVICEINFO Deviceinfo = new DEVICEINFO();
        private System.Windows.Threading.DispatcherTimer ShowTimer = null;
        private System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("zh-CN");

        private BitmapImage winBrshImage = null;
        /// <summary>
        /// 背景图片
        /// </summary>
        public BitmapImage WinBrshImage
        {
            get
            {
                return winBrshImage;
            }
            set { winBrshImage = value; base.RaisePropertyChanged("WinBrshImage"); }
        }

        private BitmapImage messageTipsImage = null;
        /// <summary>
        /// 消息提示背景图片
        /// </summary>
        public BitmapImage MessageTipsImage
        {
            get
            {
                return messageTipsImage;
            }
            set { messageTipsImage = value; base.RaisePropertyChanged("MessageTipsImage"); }
        }

        public string CZPassword;
        /// <summary>
        /// 管理员密码
        /// </summary>
        public string CZPassWord
        {
            get { return CZPassword; }
            set
            {
                CZPassword = value;
                RaisePropertyChanged("CZPassWord");
            }
        }

        /// <summary>
        /// （平台）证件信息
        /// </summary>
        public List<PaperInfo> PaperInfos = new List<PaperInfo>();

        /// <summary>
        /// （太极查询）证件信息
        /// </summary>
        public ZjxxInfo[] PaperWork { get; set; }

        /// <summary>
        /// 可办业务信息
        /// </summary>
        public KbywInfo[] KbywInfos { get; set; }

        /// <summary>
        /// 在办业务信息
        /// </summary>
        public string[] zbywxx { get; set; }

        /// <summary>
        /// 太极登录业务编号
        /// </summary>
        public string YWID { get; set; }

        /// <summary>
        /// 派人工号标识
        /// </summary>
        public bool IsManual { get; set; } = false;

        /// <summary>
        /// 太极登录输出
        /// </summary>
        public Json_R_DY_login_Rec DyLoginRec { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 未上传业务列表
        /// </summary>
        public List<string> UnOverYwList { get; set; }

        /// <summary>
        /// 是否是国家工作人员
        /// </summary>
        public string IsOfficial { get; set; }

        /// <summary>
        /// 预约业务类型拼接
        /// </summary>
        public List<string> _hasYYXX = new List<string>();

        /// <summary>
        /// 预约业务办证类型
        /// </summary>
        public Dictionary<int, string> _HasBZLB = new Dictionary<int, string>();

        /// <summary>
        /// 是否现场预约（区域分配）
        /// </summary>
        public string isVALIDXCYY { get; set; } = "0";

        /// <summary>
        /// 是否控制对象（区域分配）
        /// </summary>
        public string SFKZDX { get; set; }
        /// <summary>
        /// 异地人员身份
        /// </summary>
        public string ydrysf { get; set; }

        /// <summary>
        /// 人口照片路径
        /// </summary>
        public string djzPhoto { get; set; }

        /// <summary>
        /// 信息来源
        /// </summary>
        public string Xxly { get; set; }

        /// <summary>
        /// 太极取号方式（0：一桌式，1：一站式）
        /// </summary>
        public string TaijiPhMode { get; set; }

        /// <summary>
        /// 扫描到的身份证号码
        /// </summary>
        public string ReceiptCardNo { get; set; }

        /// <summary>
        /// 读卡信息
        /// </summary>
        public IdCardInfo CardInfo { get; set; }

        /// <summary>
        /// 单签注标识
        /// </summary>
        public int IsDqzType { get; set; } = 0;

        /// <summary>
        /// 跳过回执界面标识
        /// </summary>
        public bool IsDirectNumber { get; set; } = false;

        /// <summary>
        /// 在办业务信息（clbz==2）
        /// </summary>
        public List<string> zbywList { get; set; }

        /// <summary>
        /// 太极预约信息
        /// </summary>
        public YyywInfo[] YyywInfo { get; set; }

        /// <summary>
        /// 北京前往人工区标识
        /// </summary>
        public int RenGong { get; set; } = 0;

        private bool isBusy;

        /// <summary>
        /// 是否加载等待
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        private string loadWatingMsg;

        /// <summary>
        /// 等待提示文字
        /// </summary>
        public string LoadWatingMsg
        {
            get { return loadWatingMsg; }
            set { loadWatingMsg = value; base.RaisePropertyChanged("LoadWatingMsg"); }
        }

        private Uri mainTraget;

        /// <summary>
        /// 跳转界面
        /// </summary>
        public Uri MainTraget
        {
            get { return mainTraget; }
            set
            {
                mainTraget = value;
                if (value != null)
                {
                    base.RaisePropertyChanged("MainTraget");
                }
            }
        }

        private string title = "智慧预约取号机";
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title"); }
        }

        private string homeCurrDate;

        /// <summary>
        /// 日期
        /// </summary>
        public string HomeCurrDate
        {
            get { return homeCurrDate; }
            set { homeCurrDate = value; base.RaisePropertyChanged("HomeCurrDate"); }
        }

        private string homeCurrTime;

        /// <summary>
        /// 时间
        /// </summary>
        public string HomeCurrTime
        {
            get { return homeCurrTime; }
            set { homeCurrTime = value; base.RaisePropertyChanged("HomeCurrTime"); }
        }

        private string homeCurrWeek;

        /// <summary>
        /// 星期
        /// </summary>
        public string HomeCurrWeek
        {
            get { return homeCurrWeek; }
            set { homeCurrWeek = value; base.RaisePropertyChanged("HomeCurrWeek"); }
        }

        private bool isMessage;

        /// <summary>
        /// 是否显示提示消息
        /// </summary>
        public bool IsMessage
        {
            get { return isMessage; }
            set
            {
                //设置主页计时器
                if (isMessage != value && contentViewModel != null)
                {
                    contentViewModel.IsStop = value;
                }

                isMessage = value;
                RaisePropertyChanged("IsMessage");
            }
        }

        private string messageTipsContent;

        /// <summary>
        /// 消息提示内容
        /// </summary>
        public string MessageTipsContent
        {
            get { return messageTipsContent; }
            set { messageTipsContent = value; RaisePropertyChanged("MessageTipsContent"); }
        }

        /// <summary>
        /// 关闭温馨提示回调
        /// </summary>
        public Action CloseeReminderCallBack = null;

        /// <summary>
        /// 关闭温馨提示命令
        /// </summary>
        public RelayCommand CloseReminderCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        //点非返回按钮时，关闭返回默认的倒计时 by 2021年7月15日12:14:54 wei.chen
                        //this.StopTimeOut();
                        IsStop = true;
                        this.TimeOutCallBack -= this.TimeOutCallBackExcuted;

                        this.CloseeReminderCallBack?.Invoke();
                        this.IsMessage = false;
                        this.OnDispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.WriteInfo("----ex:"+ex.Message);
                    }
                });
            }
        }


        public Action GOOnCallBack = null;

        public RelayCommand GOOnCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //点非返回按钮时，关闭返回默认的倒计时 by 2021年7月15日12:14:54 wei.chen
                    //this.StopTimeOut();
                    IsStop = true;
                    this.TimeOutCallBack -= this.TimeOutCallBackExcuted;

                    this.GOOnCallBack?.Invoke();
                    this.IsMessage = false;
                    this.OnDispose();
                });
            }
        }

        public Action GOBack = null;

        public RelayCommand GoBackCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.GOBack?.Invoke();
                    this.IsMessage = false;
                    this.OnDispose();
                });
            }
        }


        private string gOContent = "前往";

        public string GOContent
        {
            get { return gOContent; }
            set { gOContent = value; RaisePropertyChanged("GOContent"); }
        }


        private string goBackContent = "关闭";
        public string GoBackContent
        {
            get { return goBackContent; }
            set { goBackContent = value; RaisePropertyChanged("GoBackContent"); }
        }

        private Visibility isBackShow = Visibility.Collapsed;
        public Visibility IsBackShow
        {
            get { return isBackShow; }
            set { isBackShow = value; RaisePropertyChanged("IsBackShow"); }
        }


        private Visibility isGoOnShow = Visibility.Collapsed;
        public Visibility IsGoOnShow
        {
            get { return isGoOnShow; }
            set { isGoOnShow = value; RaisePropertyChanged("IsGoOnShow"); }
        }

        private string tipMessage;

        /// <summary>
        /// 内容页面提示
        /// </summary>
        public new string TipMessage
        {
            get { return tipMessage; }
            set { tipMessage = value; RaisePropertyChanged("TipMessage"); }
        }

        public ViewModelBase contentViewModel;
        /// <summary>
        /// Frame内容对象
        /// </summary>
        public ViewModelBase ContentViewModel
        {
            get { return contentViewModel; }
            set
            {
                contentViewModel = value;
                RaisePropertyChanged("ContentViewModel");
            }
        }

        private Visibility homeShow = Visibility.Visible;
        /// <summary>
        /// 是否显示首页
        /// </summary>
        public Visibility HomeShow
        {
            get { return homeShow; }
            set { homeShow = value; RaisePropertyChanged("HomeShow"); }
        }

        private Visibility previousPageShow = Visibility.Collapsed;
        /// <summary>
        /// 上一页是否显示
        /// </summary>
        public new Visibility PreviousPageShow
        {
            get { return previousPageShow; }
            set { previousPageShow = value; RaisePropertyChanged("PreviousPageShow"); }
        }

        private Visibility nextPageShow = Visibility.Collapsed;
        /// <summary>
        /// 下一页是否显示
        /// </summary>
        public new Visibility NextPageShow
        {
            get { return nextPageShow; }
            set { nextPageShow = value; RaisePropertyChanged("NextPageShow"); }
        }

        /// <summary>
        /// 是否北京地区
        /// </summary>
        public bool IsBeijing
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 2 &&
                    DWCode.Substring(0, 2).Equals("11"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否陕西地区（西安、安康）
        /// </summary>
        public bool IsShanXi
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 2 &&
                    DWCode.Substring(0, 2).Equals("61"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否广东地区
        /// </summary>
        public bool IsGuangDong
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 2 &&
                    DWCode.Substring(0, 2).Equals("44"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否是汕尾市陆河县
        /// </summary>
        public bool IsLuHe
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 4).Equals("4415"))
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 是否是湛江地区（徐闻、遂溪）
        /// </summary>
        public bool IsXuwen
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 4).Equals("4408"))//4408
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否是武汉地区
        /// </summary>
        public bool IsWuHan
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 4).Equals("4201"))//4408
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否深圳地区
        /// </summary>
        public bool IsShenZhen
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 4).Equals("4403"))//4408
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否雷州市
        /// </summary>
        public bool IsLeiZhou
        {
            get
            {
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) &&
                    DWCode.Length > 4 &&
                    DWCode.Substring(0, 6).Equals("440882"))//4408
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否为本省户籍
        /// </summary>
        /// <param name="qycode"></param>
        /// <returns></returns>
        public bool IsLocalRegister
        {
            get
            {
                string code = BookingBaseInfo?.Address?.Code;
                string idcardNo = BookingBaseInfo?.CardInfo?.IDCardNo;
                string DWCode = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH;
                if (!string.IsNullOrWhiteSpace(DWCode) && DWCode.Length > 2)
                {
                    if (!string.IsNullOrWhiteSpace(code) && code.Length > 2)
                    {
                        return code.Substring(0, 2).Equals(DWCode.Substring(0, 2));
                    }
                    else if (!string.IsNullOrWhiteSpace(idcardNo) && idcardNo.Length > 2)
                    {
                        return idcardNo.Substring(0, 2).Equals(DWCode.Substring(0, 2));
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 是否直接取号
        /// （默认填写基本信息）
        /// </summary>
        public bool IsTakePH_No
        {
            get
            {
                string phMode = QJTConfig.QJTModel?.PH_Mode?.ToString();
                if (phMode == "0")
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否北京户籍
        /// </summary>
        public bool IsBeijingRegister
        {
            get
            {
                string code = BookingBaseInfo?.Address?.Code;
                string idcardNo = BookingBaseInfo?.CardInfo?.IDCardNo;
                if (!string.IsNullOrWhiteSpace(code) && code.Length > 2)
                {
                    return code.Substring(0, 2).Equals("11");
                }
                else if (!string.IsNullOrWhiteSpace(idcardNo) && idcardNo.Length > 2)
                {
                    return idcardNo.Substring(0, 2).Equals("11");
                }
                return false;
            }
        }


        /// <summary>
        /// 是否广东户籍
        /// </summary>
        public bool IsGuangDongRegister
        {
            get
            {
                string code = BookingBaseInfo?.Address?.Code;
                string idcardNo = BookingBaseInfo?.CardInfo?.IDCardNo;
                if (!string.IsNullOrWhiteSpace(code) && code.Length > 2)
                {
                    return code.Substring(0, 2).Equals("44");
                }
                else if (!string.IsNullOrWhiteSpace(idcardNo) && idcardNo.Length > 2)
                {
                    return idcardNo.Substring(0, 2).Equals("44");
                }
                return false;
            }
        }

        /// <summary>
        /// 是否广东户籍
        /// </summary>
        public bool IsHuBeiRegister
        {
            get
            {
                string code = BookingBaseInfo?.Address?.Code;
                string idcardNo = BookingBaseInfo?.CardInfo?.IDCardNo;
                if (!string.IsNullOrWhiteSpace(code) && code.Length > 2)
                {
                    return code.Substring(0, 2).Equals("42");
                }
                else if (!string.IsNullOrWhiteSpace(idcardNo) && idcardNo.Length > 2)
                {
                    return idcardNo.Substring(0, 2).Equals("42");
                }
                return false;
            }
        }

        #region 基本信息

        private string deviceName;

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName
        {
            get { return deviceName; }
            set { deviceName = value; RaisePropertyChanged("DeviceName"); }
        }

        private string advisoryTelephone;

        /// <summary>
        /// 技术支持电话
        /// </summary>
        public string AdvisoryTelephone
        {
            get { return advisoryTelephone; }
            set { advisoryTelephone = value; RaisePropertyChanged("AdvisoryTelephone"); }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public string VersionNumber
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        #endregion


        #endregion

        #region 方法

        /// <summary>
        ///  内容页设置
        /// </summary>
        /// <param name="strPageName">切换页面</param>
        /// <param name="isAllow">是否允许切换</param>
        public void ContentPageSetting(string strPageName, bool isAllow = true)
        {
            //App.Current.Dispatcher.Invoke(new Action(() =>
            //{
            if (isAllow)
            {
                Thread.Sleep(10);
                this.MainTraget = new Uri(string.Format("/Freedom.ZHPHMachine;component/View/{0}.xaml", strPageName), UriKind.RelativeOrAbsolute);
            }

            //MainTraget = new Uri(string.Format("/Freedom_ZHSLMachine;component/View/{0}.xaml", pageName), UriKind.RelativeOrAbsolute);
            //}));
        }

        /// <summary>
        /// 提示消息
        /// </summary>
        /// <param name="strMessage">消息</param>
        public void IsShowHiddenLoadingWait(string strMessage = "")
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.IsBusy = !string.IsNullOrEmpty(strMessage) ? true : false;
                this.LoadWatingMsg = strMessage;
            });
        }

        #region 返回首页
        /// <summary>
        /// 返回首页
        /// </summary>
        public void ReturnHome()
        {
            new Thread(delegate ()
            {
                try
                {
                    //this.IsHome = true;
                    //this.thCommon?.Abort();
                    Log.Instance.WriteInfo(" ================== 返回首页 ================== ");
                    //CloseDialog();
                    //CommonHelper.Instance.ClearCashData();
                    //CacheClearTimely();
                    //CountdownVisibility = Visibility.Collapsed;
                    //ZHGT_LedHelper.Instance.OpenLed(QJTConfig.QJTModel.ZhgtLedToIDCard, 0);
                    //ZHGT_LedHelper.Instance.OpenLed(QJTConfig.QJTModel.ZhgtLedToDown, 0);
                    GC.Collect();
                    GC.SuppressFinalize(this);
                    DoExitFunction("MainPage");
                    //switch (CurrDevStatus)
                    //{
                    //    case EnumTypeSTATUS.OFFLINE:
                    //    case EnumTypeSTATUS.ERROR:
                    //    case EnumTypeSTATUS.PASUE:
                    //    case EnumTypeSTATUS.BLOCK:
                    //        IsShowHiddenLoadingWait("");
                    //        //切换到暂停页面
                    //        SetFrameTraget("NotificationPage");
                    //        if (NotificationEventAction != null)
                    //        {
                    //            NotificationEventAction();
                    //            NotificationEventAction = null;
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteError("返回首页发生错误：" + ex);
                }
                finally
                {
                    //ClearMemory();
                }
            }).Start();
        }
        #endregion

        /// <summary>
        /// 取字典的前往地
        /// </summary>
        /// <returns>前往地集合</returns>
        public List<DictionaryTypeByPinYin> GetDictionaryTypeByPinYin()
        {
            return DictionaryTypeByPinYin;
        }

        /// <summary>
        /// 消息提示
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="action"></param>
        /// <param name="second"></param>
        /// <param name="closeContent"></param>
        public void MessageTips(string content, Action action = null, Action goOnaction = null, int second = 10, string closeContent = "返回")
        {
            Log.Instance.WriteInfo("消息提示:" + content);
            //this.TipMessage = "消息提示";
            this.IsShowHiddenLoadingWait("");
            this.IsMessage = true;
            MessageTipsContent = content;
            this.OpenTimeOut(second);
            this.CloseeReminderCallBack = action;
            this.GOOnCallBack = goOnaction;
            this.IsGoOnShow = goOnaction == null ? Visibility.Collapsed : Visibility.Visible;
            var items = content.Split(new char[2] { '【', '】' });
            GOContent = items?.Count() >= 3 ? items[1] : "前往";
            GoBackContent = closeContent;
        }


        /// <summary>
        /// 弹出过的窗口
        /// </summary>
        private List<Window> DialogWindows = new List<Window>();

        /// <summary>
        /// 弹窗提示窗
        /// </summary>
        /// <param name="record">是否记录</param>
        public ReturnInfo ShowMsgReturnDialog(string MsgInfo = "", int TimeOut = 10, bool IsClose = false, bool record = true, Visibility cancelBtnVisibility = Visibility.Collapsed)
        {
            ReturnInfo result = null;
            ShowMessageWindow window = new ShowMessageWindow(MsgInfo, IsClose, TimeOut, cancelBtnVisibility);
            if (record) DialogWindows.Add(window);
            window.MessageInfo = MsgInfo;
            result = window.ShowDialog() == true ? ReturnInfo.CreateSuccess("") : ReturnInfo.CreateFailed("");
            result.ReturnValue = window.IsTimeOutClose;
            return result;
        }

        public void ActionToGo(string content, Action action = null, Action goOnaction = null, string closeContent = "返回")
        {
            OwnerViewModel?.IsShowHiddenLoadingWait(TipMsgResource.IDCardQuueryTipMsg);
            //Log.Instance.WriteInfo(content);
            //this.TipMessage = "消息提示";
            this.IsMessage = false;
            MessageTipsContent = content;
            this.OpenTimeOut2(1);
            this.CloseeReminderCallBack = action;
            this.GOOnCallBack = goOnaction;
            this.IsGoOnShow = goOnaction == null ? Visibility.Collapsed : Visibility.Visible;
            var items = content.Split(new char[2] { '【', '】' });
            GOContent = items?.Count() >= 3 ? items[1] : "前往";
            GoBackContent = closeContent;
        }

        /// <summary>
        /// 跳转到提示消息界面
        /// </summary>
        /// <param name="MessageInfo"></param>
        /// <param name="IsColse"></param>
        /// <param name="BtnVisibility"></param>
        /// <param name="BtnContent"></param>
        public void SetFrameMessagePage(string MessageInfo, bool IsColse = false, bool BtnVisibility = true, string BtnContent = "确定", string FrameTraget = "")
        {
            this.IsShowHiddenLoadingWait("");


            AppMessageViewModels.Instance.IsClose = IsColse;
            AppMessageViewModels.Instance.FrameTraget = FrameTraget;
            AppMessageViewModels.Instance.ButtonVisibility = BtnVisibility ? Visibility.Visible : Visibility.Collapsed;
            AppMessageViewModels.Instance.ButtonContent = BtnContent;
            AppMessageViewModels.Instance.AppMessageInfo = MessageInfo;
            Log.Instance.WriteInfo(MessageInfo);
            //this.ContentPageSetting("CommandView/AppMessagePage");
            DoNextFunction("CommandView/AppMessagePage");
        }

        public void TimeOut()
        {
            this.OpenTimeOut();
        }

        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        private bool DataInit(out string msg)
        {
            bool blnResult = false;
            msg = string.Empty;
            try
            {
                #region 本地配置文件参数加载 及获取设备信息
                Log.Instance.WriteInfo("***本地配置文件参数加载 及获取设备信息***");
                var rinfo = QJTConfig.BindParameters();
                if (rinfo == null || !rinfo.IsSucceed)
                {
                    msg = "加载配置参数错误,系统退出...";
                    return false;
                }
                AdvisoryTelephone = QJTConfig.QJTModel?.Zxdh;
                //查询设备信息
                string ip = Net.GetLanIp();
                if (!string.IsNullOrWhiteSpace(QJTConfig.QJTModel?.QJTDevInfo?.DEV_IP))
                {
                    ip = QJTConfig.QJTModel?.QJTDevInfo?.DEV_IP;
                }
                Log.Instance.WriteInfo("本机IP：" + ip);

                if (!CheckServeStatus())
                {
                    msg = "本地网络出错，请联系管理员！";
                    Log.Instance.WriteInfo(msg);
                    Thread.Sleep(1000);
                    IsShowHiddenLoadingWait(msg);
                }
                //检查连接接口是否正常 
                int ick = 10;
                while (ick > 0)
                {
                    if (dbHelper.S_Sysdate().IsEmpty())
                    {
                        msg = (ick + 1) + " 次，连不上应用服务器，正在重新连接,请稍候...";
                        //重连10次应用服务器还是操时连接不上视为应用服务器故障
                        Thread.Sleep(1000);
                        IsShowHiddenLoadingWait(msg);
                    }
                    else
                    {
                        ick = 0;
                        msg = "应用服务器正常连接...";
                    }
                    Log.Instance.WriteInfo(msg);
                    ick--;
                }

                //获取设备信息
                var deviceInfo = dbHelper.S_DeviceInfo(ip, out msg);
                Deviceinfo = deviceInfo;
                _devid = QJTConfig.QJTModel.QJTDevInfo.DEV_ID;  //设备ID

                //获取设备信息失败
                if (deviceInfo.IsEmpty() || deviceInfo?.DEV_ID.ToDecimal(0) == 0)
                {
                    msg = "此设备未授权，请联系管理员！";
                    Log.Instance.WriteInfo(msg);
                    Thread.Sleep(1000);
                    IsShowHiddenLoadingWait(msg);
                    return false;
                }
                else
                {
                    deviceInfo.DeptInfo.DeptCode = deviceInfo.TBDWBH;
                    deviceInfo.DeptInfo.DeptName = QJTConfig.QJTModel?.QJTDevInfo?.DeptInfo?.DeptName;
                    deviceInfo.Dev_Name = deviceInfo.DEV_NAME;
                    deviceInfo.Dev_IP = deviceInfo.DEV_IP;
                    QJTConfig.QJTModel.QJTDevInfo = deviceInfo;
                    DeviceName = QJTConfig.QJTModel.QJTDevInfo.Dev_Name;
                    msg = deviceInfo.Dev_IP + "-" + deviceInfo.Dev_Name + " 此设备认证成功！";
                    Log.Instance.WriteInfo(msg);
                }

                //设置设备状态
                blnResult = dbHelper.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.ONLINE);
                Log.Instance.WriteInfo("设备id：" + QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString());

                if (!blnResult)
                {
                    msg = "服务认证失败，请重试！";
                    Log.Instance.WriteInfo(msg);
                    return false;
                }
                //保存配置信息
                Log.Instance.WriteInfo("更新配置文件：" + QJTConfig.SaveDjzConfig(QJTConfig.QJTModel));
                //Log.Instance.WriteInfo("当前版本：" + version);
                //设备ID
                _devid = QJTConfig.QJTModel.QJTDevInfo.DEV_ID;
                
                //设备ID
                string deviceId = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString();
                #endregion

                #region 完善更新设备当前最新软件 并检测升级
                DEVICEINFO info = new DEVICEINFO();
                info = OwnerViewModel.Deviceinfo;
                info.DEV_APPCURRVERSION = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                info.DEV_ADDRESS = System.Windows.Forms.Application.StartupPath;
                info.HX_SERIALNUMBER = DjzConfig.DjzModel.DzjZCXLH;
                info.DJZZH = DjzConfig.DjzModel.DjzUserName;
                Log.Instance.WriteInfo("完善更新设备版本号：" + info.DEV_APPCURRVERSION + "信息：" + ZHWSBase.Instance.UpdateDevStatusInfo(info));

                string processName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();  //更新程序
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); //程序版本

                if (QJTConfig.QJTModel.IsEnableWebDBServer)
                {
                    AutoUpdate.CheckAutoUpdate(_devid.ToString(), version, processName);
                    AutoUpdate.CheckAutoUpdate(deviceId, version, processName);
                    AutoUpdate.CheckAutoUpdate(_devid.ToString(), version, processName);//调用自动升级+启动监控工具+备份日志到服务器
                }
                #endregion

                Thread.Sleep(200);

                #region 第一次启动检查打印机、读卡器、条码枪是否正常

                Log.Instance.WriteInfo("***第一次启动检查打印机、读卡器、条码枪是否正常***");
                //检查打印机状态
                if (!PrintHelper.CheckPrinter(out msg))
                {
                    //自动上报打印机故障
                    Log.Instance.WriteInfo(msg);
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault20;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_10000015);
                    DevAlarm.AlarmInfo = msg;
                    ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_10000015).ToString());
                    return false;
                }

                //初始化身份证身份证阅读器
                if (!ReadIDCardHelper.Instance.DoReadIDCardInit().IsSucceed)
                {
                    //自动上报身份证阅读器故障
                    msg = TipMsgResource.IDCardInitializationTipMsg;
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault20;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_10000012);
                    DevAlarm.AlarmInfo = msg;
                    var returnInfo = ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_10000012).ToString());

                    Log.Instance.WriteInfo(msg);
                    return false;
                }

                //初始化扫描抢
                if (!BarCodeScanner.Instance.OpenBarCodeDev().IsSucceed)
                {
                    msg = TipMsgResource.ScanningReceiptTipMsg;
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault20;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_10000013);
                    DevAlarm.AlarmInfo = msg;
                    ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_10000013).ToString());

                    Log.Instance.WriteInfo(msg);
                    return false;
                }
                else
                {
                    BarCodeScanner.Instance.CloseBarCodeDev();
                }

                #endregion

                #region 加载服务器字典数据到本地
                LoadXTZDData();
                #endregion

                #region  记录设备监控日志
                Log.Instance.WriteInfo("***记录设备监控日志***");

                DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
                {
                    DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                    CZ_PASSWORD = CZPassWord,
                    DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                    DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                    LOG_TYPE = "INFO",
                    LOG_STATUS = Log_Status.ONLINE.ToString(),
                    QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                    START_USER = "QHJ",
                    START_DATE = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate().ToDate(),
                    LOG_MESSAGE = "初始化程序成功！",
                    REMARK = ""
                };
                //推送设备软件使用记录
                dbHelper.I_Dev_Watch_Log(watchLog);

                //陕西人工号标识
                IsManual = false;
                //导服核查取消标识
                UnOverYwList = new List<string>();
                //太极上传开机设备状态和运行状态
                if (TjConfig.TjModel.IsConnectionTj)
                {
                    UploadMachineCondition();
                    UploadRunningCondition();
                }

                #endregion

                DeleteLog();
            }
            catch (Exception ex)
            {
                msg = "初始化失败";
                Log.Instance.WriteInfo(msg);
                Log.Instance.WriteError(ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("End:" + msg);
            }
            return blnResult;
        }

        /// <summary>
        /// 开始加载服务器【字典数据】到本地
        /// </summary>
        public void LoadXTZDData()
        {
            Log.Instance.WriteInfo("***开始加载服务器【字典数据】到本地***");
            string msg = string.Empty;
            try
            {
                Task[] task = new Task[]{
                    //查询省市区
                    dbHelper.S_ProvinceList(),
                    //查询申请类别
                    dbHelper.S_XTZD(KindType.ApplyType), 
                    //查询办证类别(护照)
                    dbHelper.S_XTZD(KindType.ApplyCategory, 1),
                     //查询办证类别(台湾)
                    dbHelper.S_XTZD(KindType.ApplyCategory, 3),
                     //查询办证类别(港澳)
                    dbHelper.S_XTZD(KindType.ApplyCategory, 2),
                    //查询签注种类(台湾)
                    dbHelper.S_XTZD(KindType.QZType, 2),
                    //查询签注种类(港澳)WinBrshImage
                    dbHelper.S_XTZD(KindType.QZType, 1),
                    //查询签注次数 
                    dbHelper.S_QZZLList(),
                    //获取系统参数配置列表
                    dbHelper.S_SysParaSettingList(),
                    //查询与申请人关系
                    dbHelper.S_XTZD(KindType.RelationType),
                    //查询字典职业
                    dbHelper.S_XTZD(KindType.Job),
                    //查询出境事由
                    dbHelper.S_XTZD(KindType.DepartureReason),
                    //查询前往地
                    dbHelper.S_XTZD(KindType.Destination),
                    //民族
                    dbHelper.S_XTZD(KindType.MZ)
            };

                Log.Instance.WriteInfo("下载字典任务：" + task.Length + " 数。");
                Task.WaitAll(task);
                config.Set<List<DictionaryType>>(new List<DictionaryType>());
                for (int i = 0; i < task.Length; i++)
                {
                    var item = task[i];

                    if (item is Task<List<ProvinceList>>)
                    {
                        config.Set<List<ProvinceList>>((item as Task<List<ProvinceList>>).Result);
                    }
                    else if (item is Task<List<DictionaryType>>)
                    {
                        List<DictionaryType> result = (item as Task<List<DictionaryType>>).Result;
                        switch (i)
                        {
                            case 2:
                                //保存申请类型(护照)
                                config.Set<List<DictionaryType>>(result, "ApplyCategoryHZ");
                                break;
                            case 3:
                                //保存申请类型(台湾)
                                config.Set<List<DictionaryType>>(result, "ApplyCategoryTWN");
                                break;
                            case 4:
                                //保存申请类型(香港澳门)
                                config.Set<List<DictionaryType>>(result, "ApplyCategoryGA");
                                break;
                            default:
                                config.Get<List<DictionaryType>>()?.AddRange(result);
                                break;
                        }
                    }
                    else if (item is Task<List<QZZLList>>)
                    {
                        config.Set<List<QZZLList>>((item as Task<List<QZZLList>>).Result);
                    }
                    else if (item is Task<List<PH_SYSPARASETTING_TB>>)
                    {
                        var lst = (item as Task<List<PH_SYSPARASETTING_TB>>).Result;
                        //获取打印配置信息
                        var model = lst?.FirstOrDefault(t => t.NAME == "PRINT_BILL_Title");
                        if (model != null)
                        {
                            Log.Instance.WriteInfo("获取系统参数成功！开始下载派号打印模板...");
                            PARA_VALUE param = JsonHelper.ConvertToObject<PARA_VALUE>(model.PARA_VALUE);
                            //单位名称
                            QJTConfig.QJTModel.QJTDevInfo.DeptInfo.DeptName = param?.BillDwName;
                            //从服务器下载派号打印模板
                            DownLoadWord(param);
                        }
                        else
                        {
                            Log.Instance.WriteInfo("获取系统打印回执参数失败！");
                        }

                        //获取时间配置信息
                        model = lst?.FirstOrDefault(t => t.NAME == "HowLongSS");
                        if (model != null)
                        {
                            Time_PZ param = JsonHelper.ConvertToObject<Time_PZ>(model.PARA_VALUE);
                            QJTConfig.QJTModel.TOutSeconds = param.TOutSeconds;
                            QJTConfig.QJTModel.TOutForWrittingSeconds = param.TOutForWrittingSeconds;
                            this.RefreshDevStatusSeconds = param.RefreshDevStatusSeconds * 1000;//转换为毫秒
                        }

                        if (lst?.FirstOrDefault(t => t.NAME == "CZPasswordList") != null)
                        {
                            //配置项中取操作密码
                            CZPassWord = JsonHelper.ConvertToObject<CZPasswrodLISTPARA_VALUE>(lst?.FirstOrDefault(t => t.NAME == "CZPasswordList")?.PARA_VALUE)?.QH_pwd;
                            Log.Instance.WriteInfo("管理员密码为：" + CZPassWord);
                            config.Set<List<PH_SYSPARASETTING_TB>>(lst);
                        }

                        //string str= @"{"data":"{\"dwdm\":\"110111060100\",\"dwid\":\"BA1D16BAAAA84982BD6E844C730A0E58\",\"dwmc\":\"房山出入境\",\"dwxzqh\":\"110111\",\"yhid\":\"9c8b442f3a8d423d85a74345e1593c3e\",\"yhmc\":\"高景明\"}","moreInfo":null,"stateCode":"KTJ0000","stateDesc":"处理成功","success":"1"}
                        //";
                    }
                }

                DictionaryTypeByPinYin = dbHelper.S_XTZDByPinyin(KindType.Destination);
                Log.Instance.WriteInfo("获取拼音字典数据：" + DictionaryTypeByPinYin?.Count + "条！");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("加载服务器【字典数据】到本地发生异常：" + ex.Message);
                Log.Instance.WriteError("加载服务器【字典数据】到本地发生异常：" + ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("***结束加载服务器【字典数据】到本地***");
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="obj"></param>
        public override void DoInitFunction(object obj)
        {
            try
            {
                //修改程序名称
                if (!string.IsNullOrWhiteSpace(QJTConfig.QJTModel?.AppTitle))
                {
                    title = QJTConfig.QJTModel?.AppTitle;
                }
                ShowTimer = new System.Windows.Threading.DispatcherTimer();
                ShowTimer.Tick += new EventHandler(ShowTimer_Tick);
                ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                ShowTimer.Start();

                //设置背景图   
                var bgpath = Path.Combine(FileHelper.GetLocalPath(), @"ApplicationData/Skin/bg.jpg");
                WinBrshImage = new BitmapImage(new Uri(bgpath, UriKind.RelativeOrAbsolute));

                //隐藏任务栏
                //Hidetask();
                //thNetWork = new Thread(CheckServeStatus);
                //thNetWork.IsBackground = true;
                //thNetWork.Start();
                //初始化数据信息
                this.thCommon = new Thread(delegate ()
                 {
                     var rinfo = new ReturnInfo();
                     IsShowHiddenLoadingWait("系统正在初始化,请稍候...");
                     string msg = string.Empty;
                     Process processes = Process.GetCurrentProcess();
                     Log.Instance.WriteInfo("===================开始启动主程序【" + processes.ProcessName + "】====================");

                     if (!DataInit(out msg))
                     {
                         IsShowHiddenLoadingWait(msg);
                         Thread.Sleep(3000);
                         this.ExitProgram();
                         return;
                     }
                     Log.Instance.WriteInfo($"【{QJTConfig.QJTModel.QJTDevInfo.DEV_NAME}】【{QJTConfig.QJTModel.QJTDevInfo.DEV_NO}】【{EnumType.GetEnumDescription(EnumTypeSTATUS.ONLINE)}】<初始化成功>");
                     this.IsShowHiddenLoadingWait();
                     this.ContentPageSetting("MainPage", this.ContentPage == null);
                 });
                thCommon.IsBackground = true;
                thCommon.Start();

                //监控设备状态
                //if (OwnerViewModel?.IsBeijing != true)
                //{
                //    var thDeviceInfo = new Thread(() =>
                //    {
                //        string ip = QJTConfig.QJTModel?.QJTDevInfo?.Dev_IP;
                //        //获取设备IP
                //        if (string.IsNullOrWhiteSpace(ip))
                //        {
                //            ip = Net.GetLanIp();
                //            //continue;
                //        }
                //        while (true)
                //        {
                //            try
                //            {
                //                //定时刷新
                //                Thread.Sleep(RefreshDevStatusSeconds);

                //                //获取设备状态
                //                var result = dbHelper.S_DeviceStatus(ip);
                //                if (result != null)
                //                {
                //                    //更新最新设备状态 
                //                    if (QJTConfig.QJTModel != null && QJTConfig.QJTModel.QJTDevInfo != null
                //                    && QJTConfig.QJTModel.QJTDevInfo.DEVICESTATUS != result.DEVICESTATUS)
                //                    {
                //                        //判断当前是否在系统页，否则切换至首页
                //                        App.Current.Dispatcher.Invoke(new Action(() =>
                //                        {
                //                            QJTConfig.QJTModel.QJTDevInfo.DEVICESTATUS = result.DEVICESTATUS;

                //                            if (ContentPage?.Title?.Contains("系统") == false)
                //                            {
                //                                //预防首页暂停页面不切换
                //                                if (ContentPage?.Title?.Contains("首页") == true && result.DEVICESTATUS == (EnumTypeSTATUS.PASUE).ToString())
                //                                {
                //                                    DoNextFunction("NotificationPage");
                //                                }
                //                                else
                //                                {
                //                                    //返回首页(首页中根据设备状态判断时候进入提示暂停页)难怪
                //                                    DoExitFunction(null);
                //                                }
                //                            }
                //                        }));
                //                    }
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                Log.Instance.WriteError($"设备状态获取异常[{ex.Message}]");
                //            }

                //        }
                //        }
                //    });
                //    thDeviceInfo.IsBackground = true;
                //    thDeviceInfo.Start();
                //}
                //});
            }
            catch (Exception e)
            {
                Log.Instance.WriteInfo("初始化程序发生异常：" + e.Message);
                Log.Instance.WriteError("初始化程序发生异常：" + e.Message);
                DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
                {
                    DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                    CZ_PASSWORD = CZPassWord,
                    DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                    DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                    LOG_TYPE = "ERROR",
                    LOG_STATUS = Log_Status.ERROR.ToString(),
                    QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                    START_USER = "QHJ",
                    STOP_DATE = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate().ToDate(),
                    LOG_MESSAGE = "初始化程序出错：" + e.Message
                };
                //推送设备软件使用记录
                dbHelper.I_Dev_Watch_Log(watchLog);
                this.ExitProgram();
            }
        }

        /// <summary>
        /// 太极上传设备状态
        /// </summary>
        /// <returns></returns>
        public static bool UploadMachineCondition()
        {
            var nowTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate()).ToString("yyyyMMddHHmmss");
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_MachineCondition sbMonitor = new Json_I_SB_Monitor_MachineCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    sbzt = ((int)DM_SBZT.ZT11).ToString(),
                    sbsj = nowTime
                };
                var result = new TaiJiHelper().Do_SB_Monitor_MachineCondition(sbMonitor);
                Log.Instance.WriteInfo(result ? "【太极】上传设备状态返回【成功】" : "【太极】上传设备状态至太极返回【失败】");
            }
            return true;
        }

        /// <summary>
        /// 上传太极设备运行状态
        /// </summary>
        /// <returns></returns>
        public bool UploadRunningCondition()
        {
            var nowTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate()).ToString("yyyyMMddHHmmss");
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_RunningCondition sbMonitor = new Json_I_SB_Monitor_RunningCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    yxzt = "00",
                    sbsj = nowTime
                };
                var result = new TaiJiHelper().Do_SB_Monitor_RunningCondition(sbMonitor);
                Log.Instance.WriteInfo(result ? "【太极】上传设备运行状态返回【成功】" : "【太极】上传设备状态至太极返回【失败】");
            }
            return true;
        }


        private void ShowTimer_Tick(object sender, EventArgs e)
        {
            HomeCurrDate = DateTime.Now.ToString("yyyy年MM月dd日");
            HomeCurrTime = DateTime.Now.ToString("HH:mm:ss");
            HomeCurrWeek = cultureInfo.DateTimeFormat.GetAbbreviatedDayName(DateTime.Now.DayOfWeek);
        }

        /// <summary>
        /// 倒计时回调
        /// </summary>
        public override void TimeOutCallBackExcuted()
        {
            this.CloseReminderCommand?.Execute(null);
        }

        /// <summary>
        /// 从ftp下载word打印模板
        /// </summary>
        /// <param name="param">系统参数</param>
        public void DownLoadWord(PARA_VALUE param)
        {
            Log.Instance.WriteInfo("获取系统打印回执参数返回【成功】，参数【PHTemplatesName】的值为：" + param?.PHTemplatesName);
            //派号单模板名称
            //QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark = param?.PHTemplatesName;
            if (!string.IsNullOrEmpty(param?.PHTemplatesName))
            {
                int len = param.PHTemplatesName.LastIndexOf("/", StringComparison.Ordinal);
                var substring = param.PHTemplatesName.Substring(len + 1,
                    param.PHTemplatesName.Length - len - 1);
                QJTConfig.QJTModel.QJTDevInfo.DeptInfo.Remark = substring;
                //取本身路径
                string docPath = Path.Combine(FileHelper.GetLocalPath(), substring);
                Log.Instance.WriteInfo("获取下载模板文件路径：" + docPath);

                //DownLoadFile(param.PHTemplatesName, docPath);
            }

        }

        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件名称</param>
        /// <param name="downLocallFilePath">下载文件路径</param>
        public static void DownLoadFile(String url, String downLocallFilePath)
        {
            try
            {
                Log.Instance.WriteInfo("=====开始下载=====");

                if (File.Exists(downLocallFilePath))
                {
                    Log.Instance.WriteInfo("下载新文件前，发现本地已存在旧文件：" + downLocallFilePath);
                    File.Delete(downLocallFilePath);
                }
                // string filePath = Application.StartupPath + @"\" + FileName;
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                FileStream outputStream = new FileStream(downLocallFilePath, FileMode.Create);
                Stream httpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = httpStream.Read(buffer, 0, bufferSize);

                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = httpStream.Read(buffer, 0, bufferSize);
                }
                httpStream.Close();
                outputStream.Close();
                response.Close();
                Log.Instance.WriteInfo("成功下载模板文件【" + url + "】到本地：" + downLocallFilePath);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("下载模板文件【" + url + "】到本地发生异常：" + ex.Message);

            }
            finally
            {
                Log.Instance.WriteInfo("=====结束下载=====");
            }
        }

        /// <summary>
        /// 检查是否有网络连接
        /// </summary>
        /// <returns></returns>
        public bool CheckServeStatus()
        {
            bool isLocalAreaConnected = NetworkInterface.GetIsNetworkAvailable();
            if (!isLocalAreaConnected)
            {
                Log.Instance.WriteInfo("网络故障：无网络连接");
                DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault20;
                DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000003);
                DevAlarm.AlarmInfo = "网络故障：无网络连接";
                //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000003).ToString());
                return false;
            }
            return true;
        }

        private Visibility _CountdownVisibility = Visibility.Collapsed;
        public Visibility CountdownVisibility { get => _CountdownVisibility; set { _CountdownVisibility = value; base.RaisePropertyChanged("CountdownVisibility"); } }

        private int _CountdownValue;
        public int CountdownValue { get => _CountdownValue; set { _CountdownValue = value; base.RaisePropertyChanged("CountdownValue"); } }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeOutNumber = 60;
        private Thread timeoutThread = null;
        /// <summary>
        /// 页面超时回调事件
        /// </summary>
        public event Action TimeOutEventAction = null;

        public virtual void TimeOutNew(int timeoutnumber = 0, bool IsReturnHome = true, Visibility isVisibility = Visibility.Visible)
        {
            if (timeoutnumber <= 0)
                timeoutnumber = QJTConfig.QJTModel.TOutSeconds;
            if (CountdownVisibility != isVisibility)
                CountdownVisibility = isVisibility;
            CountdownValue = timeoutnumber;
            TimeOutNumber = timeoutnumber;
            timeoutThread = new Thread(new ParameterizedThreadStart(TimeOutFunction));
            timeoutThread.IsBackground = true;
            timeoutThread.Start(IsReturnHome);

        }
        private void TimeOutFunction(object IsReturnHome)
        {
            while (true)
            {
                TimeOutNumber--;
                this.CountdownValue = TimeOutNumber;
                if (TimeOutNumber == 0)
                {
                    if (TimeOutEventAction != null)
                    {
                        TimeOutEventAction();
                        TimeOutEventAction = null;
                    }
                    timeoutThread = null;
                    if (IsReturnHome.ToBool())
                    {
                        StopTimeOut();
                        TimeOutCallBackExcuted();
                        return;
                    }
                    //DoExitFunction(null);

                }
                Thread.Sleep(1000);
            }
        }

        public void StopTimeOut(bool nflag = true)
        {
            try
            {
                //加了这句，卡死更多了，先注释掉 by wei.chen
                //base.StopTimeOut(nflag);
                if (timeoutThread != null)
                {
                    if (nflag)
                    {
                        if (TimeOutEventAction != null)
                            TimeOutEventAction = null;
                    }
                    if (timeoutThread != null)
                    {
                        timeoutThread.Abort(0);
                        timeoutThread = null;
                    }
                    this.CountdownVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("停止操时发生异常：" + ex.Message);
                Log.Instance.WriteError("停止操时发生异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        private static void DeleteLog()
        {

            string logs = "Log\\" + DateTime.Now.ToString("yyyy年MM月");

            string docPath = Path.Combine(FileHelper.GetLocalPath(), logs);

            DelLogFile(docPath, 5);
            //if (File.Exists(docPath))
            //{
            //    DirectoryInfo dyInfo = new DirectoryInfo(docPath);
            //    //获取文件夹下所有的文件
            //    foreach (FileInfo feInfo in dyInfo.GetFiles())
            //    {
            //        //判断文件日期是否小于今天，是则删除
            //        if (feInfo.CreationTime < DateTime.Today)
            //            feInfo.Delete();
            //    }
            //}

        }
        /// <summary>
        /// 定时清理日志文件(30天后)
        /// </summary>
        public static void DelLogFile(string Logdir, int saveDay)
        {
            try
            {
                if (Directory.Exists(Logdir))
                {
                    DateTime nowTime = DateTime.Now;
                    int delIndex = 0;
                    //string Logdir = FileHelper.GetLocalPath() + @"\Log";
                    DirectoryInfo root = new DirectoryInfo(Logdir);
                    DirectoryInfo[] dics = root.GetDirectories();//获取文件夹

                    FileAttributes attr = File.GetAttributes(Logdir);
                    if (attr == FileAttributes.Directory)//判断是不是文件夹
                    {
                        foreach (DirectoryInfo item in dics)//遍历文件夹
                        {
                            TimeSpan t = nowTime - item.CreationTime;
                            int day = t.Days;
                            if (day > saveDay)   //保存的时间 ；  单位：天
                            {
                                Directory.Delete(item.FullName, true);
                                delIndex++;
                            }
                            if (Directory.Exists(item.FullName))
                            {
                                FileInfo[] files = item.GetFiles();
                                foreach (FileInfo file in files)//遍历文件
                                {
                                    t = nowTime - file.CreationTime;
                                    day = t.Days;
                                    if (day > saveDay)
                                    {
                                        File.Delete(file.FullName);
                                        delIndex++;
                                    }
                                }
                            }
                        }
                        //Log.Instance.WriteDebug("定时清理超时【" + saveDay + "】天前日志文件：" + delIndex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("定时清理超时【" + saveDay + "】天前日志文件发生异常：" + ex.Message);
            }
        }


        private const int SW_HIDE = 0; //隐藏任务栏
        private const int SW_RESTORE = 9;//显示任务栏

        [DllImport("user32.dll")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);


        /// <summary>
        /// 隐藏任务栏
        /// </summary>
        public static void Hidetask()
        {
            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_HIDE);
        }

        /// <summary>
        /// 显示任务栏
        /// </summary>
        public void Showtask()
        {
            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_RESTORE);
        }


        #endregion

        #region 重写方法
        /// <summary>
        /// 将当前Frame显示页面关闭
        /// </summary>
        /// <param name="obj"></param>
        public override void DoExitFunction(object obj)
        {
            try
            {
                if (obj.IsNotEmpty())
                    Log.Instance.WriteInfo("点击【退出办理】" + obj?.ToString());
                ReadIDCardHelper.Instance.DoCloseIDCard();
                //isNext = true;
                contentViewModel?.DoExit.Execute(true);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("DoExitFunction发生异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 跳转到下一页
        /// </summary>
        /// <param name="obj"></param>
        public override void DoNextFunction(object obj)
        {
            //执行子页面跳转
            if (obj == null)
                contentViewModel?.DoNext.Execute(obj);
            else
                //执行首页页面跳转
                base.DoNextFunction(obj);
        }

        /// <summary>
        /// 跳转到上一页
        /// </summary>
        /// <param name="obj"></param>
        public override void DoBackFunction(object obj)
        {
            contentViewModel?.DoBack.Execute(this.ContentPage);
        }


        #endregion
    }
}