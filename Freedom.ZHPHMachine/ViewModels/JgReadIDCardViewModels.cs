using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Freedom.Models.DataBaseModels;
using Freedom.Models.HsZhPjhJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.ZHPHMachine.Command;

namespace Freedom.ZHPHMachine.ViewModels
{
    using Freedom.BLL;
    using Freedom.Common;
    using Freedom.Common.HsZhPjh.Enums;
    using Freedom.Config;
    using Freedom.Controls.Foundation;
    using Freedom.Hardware;
    using Freedom.Models;
    using Freedom.Models.CrjCreateJsonModels;
    using Freedom.Models.CrjDataModels;
    using Freedom.Models.ZHPHMachine;
    using Freedom.WinAPI;
    using Freedom.ZHPHMachine.Common;
    using Freedom.ZHPHMachine.View;
    using MachineCommandService;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    /// <summary>
    /// 身份信息核查部分
    /// 重写2021年5月24日11:35:51
    /// </summary>
    public partial class JgReadIDCardViewModels : ViewModelBase
    {
        #region 构造方法
        public JgReadIDCardViewModels(Page page)
        {
            this.ContentPage = page;
            _configManager = ServiceRegistry.Instance.Get<ElementManager>();
            TipMessage = "请将身份证贴近屏幕右下方感应区域进行操作";
        }
        #endregion

        #region 字段和属性 

        private ElementManager _configManager;
        private bool isNext = false;//是否读卡成功
        public static Dev_AlarmInfo DevAlarm = new Dev_AlarmInfo();//设备故障信息
        private Command.CommonHelper _common = new Command.CommonHelper();
        /// <summary>
        /// 是否新疆重点人员
        /// </summary>
        public bool _issfxjzdry = false;
        /// <summary>
        /// 是否重点地区人员
        /// </summary>
        public bool _issfzddq = false;
        /// <summary>
        /// BLL层查全国对象
        /// </summary>
        private CrjPreapplyManager crjManager;
        /// <summary>
        /// 读取身份证线程
        /// </summary>
        private Thread ReadIDCardThread = null;
        /// <summary>
        /// 核查身份信息线程
        /// </summary>
        private Thread QueryReadIDCardThread = null;
        //string msg = string.Empty;
        //private PH_ZZZP_TBBLL zzzp;
        /// <summary>
        /// 是否是国家公务人员 0：否 1：是
        /// </summary> 
        private bool _isOfficial = false;
        /// <summary>
        /// 人事主管单位
        /// </summary>
        private string _RSZGDW;
        /// <summary>
        /// 是否是受控对象 0：否 1：是
        /// </summary>
        private bool _isCheckSpecil = false;
        /// <summary>
        /// 是否有在办提醒
        /// </summary>
        public bool _IsTipsMessage = true;
        /// <summary>
        /// 是否有完结业务提醒
        /// </summary>
        public bool _IsWjywMessage = false;
        /// <summary>
        /// 锁对象
        /// </summary>
        private object objLock = new object();
        public List<string> YyTypeList = new List<string>();
        private string iDCardNo;
        public bool bln = false;
        private string ModelType;
        private List<DictionaryType> DictionaryType;//字典类型
        private List<DictionaryType> sqTypes;//申请类型
        /// <summary>
        /// 外网预约信息
        /// </summary>
        public List<PH_YYSQXX_TB> wsYysqxx = new List<PH_YYSQXX_TB>();
        /// <summary>
        /// 有预约和派号信息
        /// </summary>
        public PH_YYSQXX_TB CallNo = new PH_YYSQXX_TB();
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IDCardNo
        {
            get { return iDCardNo; }
            set { iDCardNo = value; RaisePropertyChanged("IDCardNo"); }
        }

        private string tipMsg;

        /// <summary>
        /// 错误提示
        /// </summary>
        public string TipMsg
        {
            get { return tipMsg; }
            set { tipMsg = value; RaisePropertyChanged("TipMsg"); }
        }

        /// <summary>
        /// 手动输入身份证号码
        /// </summary>
        public ICommand NumberKeyboard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupNumberKeyboard(KeyboardType.IDCard, "", out string str))
                    {
                        _common.IdentityVerification(new IdCardInfo() { IDCardNo = IDCardNo }, false);
                    }
                });
            }
        }

        public ICommand KeyboardCommand
        {
            get
            {
                return new RelayCommand<string>((val) =>
                {
                    TipMsg = string.Empty;
                    switch (val)
                    {
                        case "确定":
                            if (!_common.DataVerification(IDCardNo, out string msg))
                            {
                                TipMsg = msg;
                                return;
                            }
                            DoNextFunction(null);
                            break;
                        case "删除":
                            Win32API.AddKeyBoardINput(0x08);
                            break;
                        case "清空":
                            IDCardNo = string.Empty;
                            break;
                        default:
                            if (val.Length == 1)
                            {
                                ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                                int intAsciiCode = (int)asciiEncoding.GetBytes(val)[0];
                                Win32API.AddKeyBoardINput((byte)intAsciiCode);
                            }
                            break;
                    }
                    //获得焦点 by 2021年7月15日12:37:48
                    if ((this.ContentPage.FindName("inputbox") as TextBox).IsNotEmpty())
                        (this.ContentPage.FindName("inputbox") as TextBox).Focus();
                });
            }
        }
        #endregion

        #region 重写方法
        public override void DoInitFunction(object obj)
        {
            Log.Instance.WriteInfo("\n*********************** 进入【刷身份证】身份证识读界面*********************** ");
            InitLoad();

            //首次不跳出循环
            isNext = false;
            //刷身份证语音
            TTS.PlaySound("预约机-页面-刷身份证");
            //注释：初始化没线程，这里的代码产生冗余 by 2021年7月8日09:48:11 wei.chen
            //if (ReadIDCardThread != null && ReadIDCardThread.IsAlive)
            //{
            //    isNext = true;
            //    ReadIDCardThread.Join();
            //    //ReadIDCardThread.Abort();
            //    //ReadIDCardThread = null;
            //}

            if (OwnerViewModel?.IsBeijing == true)
            {
                OwnerViewModel.HomeShow = Visibility.Collapsed;
                //没作用注释掉 by wei.chen 
                //StopTimeOut();
            }
            else
            {
                //启用计时器
                this.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
                //启用计时器没有关闭，会导致在其它页面也调用关闭，停掉计时器 by2021年7月9日14:32:56  wei.chen
                //CloseReadCard();
            }
            //if (QJTConfig.QJTModel.IsConnReadIDcardDev)
            //{

            //初始化身份证阅读器
            Log.Instance.WriteInfo(!InitHardware() ? "初始化身份证阅读器失败！" : "开始初始化身份信息：成功");

            //测试手输屏蔽自动读卡 by wei.chen
            ReadIDCardThread = new Thread(new ThreadStart(ReadIDCard));
            ReadIDCardThread.IsBackground = true;
            ReadIDCardThread.Start();

            //}
            //base.DoInitFunction(obj);
        }

        protected override void OnDispose()
        {
            OwnerViewModel.HomeShow = Visibility.Visible;

            try
            {
                if (ReadIDCardThread != null && ReadIDCardThread.IsAlive)
                {
                    isNext = true;
                    //解决上一步卡死问题 by wei.chen
                    ReadIDCardThread.Abort();
                    ReadIDCardThread.Join();

                    //Thread.Sleep(500);
                    //ReadIDCardThread.Abort(0);
                    //ReadIDCardThread = null;
                }
                if (QueryReadIDCardThread != null && QueryReadIDCardThread.IsAlive)
                {
                    QueryReadIDCardThread.Abort(0);
                    QueryReadIDCardThread.Join();
                    QueryReadIDCardThread = null;
                }

                //页面停止倒计时 by 2021年7月6日17:25:05 wei.chen
                if (!IsStop)
                    StopTimeOut();
                ReadIDCardHelper.Instance.DoCloseIDCard();

                CommonHelper.OnDispose();
                TTS.StopSound();
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                Log.Instance.WriteInfo("释放进程发生异常：" + ex.Message);
                Log.Instance.WriteError("释放进程发生异常：" + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("释放【刷身份证】身份证识读界发生异常：" + ex.Message);
                Log.Instance.WriteError("释放【刷身份证】身份证识读界发生异常：" + ex.Message);
            }
            base.OnDispose();
        }

        public override void DoNextFunction(object obj)
        {

            try
            {
                //关闭读卡线程
                isNext = true;

                if (sw.IsRunning)
                    sw.Stop();

                if (!_common.DataVerification(iDCardNo, out string msg))
                {
                    TipMsg = msg;
                    
                    if (!OwnerViewModel.IsBeijing)
                    {
                        isNext = false;//重新启动读卡线程
                        Log.Instance.WriteInfo("格式校验不通过停留当前页面……");
                        return;
                    }
                    else
                    {
                        OwnerViewModel?.MessageTips(TipMsg, () =>
                        {
                            iDCardNo = "";

                            //初始化身份证阅读器
                            Log.Instance.WriteInfo(!InitHardware() ? "初始化身份证阅读器失败！" : "【重新】初始化身份信息：成功");
                            Thread.Sleep(500);

                            isNext = false;//重新启动读卡线程
                            if (ReadIDCardThread != null && ReadIDCardThread.IsAlive)
                            {
                                //解决上一步卡死问题 by wei.chen
                                ReadIDCardThread.Abort();
                                ReadIDCardThread.Join();
                            }
                            ReadIDCardThread = new Thread(new ThreadStart(ReadIDCard));
                            ReadIDCardThread.IsBackground = true;
                            ReadIDCardThread.Start();

                        });

                    }
                    return;
                }
                else
                {
                    if (iDCardNo != null && iDCardNo.IsNotEmpty())
                    {
                        if (isNext == false)
                            isNext = true;

                        //关闭身份证读卡器 by 2021年7月13日15:44:42 wei.chen 
                        ReadIDCardHelper.Instance.DoCloseIDCard();
                        _common.IdentityVerification(new IdCardInfo() { IDCardNo = iDCardNo }, false);

                    }
                    else
                    {
                        OwnerViewModel?.MessageTips("请输入您正确的身份证号码！", () =>
                        {
                            this.DoExitFunction("MainPage");
                        });
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("核验身份信息发生错误：" + ex);
            }
            finally
            {

                //增加提示消息隐藏+获得输入框焦点 by wei.chen
                OwnerViewModel.IsShowHiddenLoadingWait();
                if ((this.ContentPage.FindName("inputbox") as TextBox).IsNotEmpty())
                    (this.ContentPage.FindName("inputbox") as TextBox).Focus();
            }
        }
        #endregion

        #region 自定义方法

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public void InitLoad()
        {
            //this.ContentPage = page;
            _configManager = ServiceRegistry.Instance.Get<ElementManager>();
            TipMessage = "请将身份证贴近屏幕右下方感应区域进行操作";
            //初始化数据
            _configManager.Set<BookingModel>(new BookingModel());
            //（0：已预约 1：无预约）
            ModelType = ServiceRegistry.Instance.Get<ElementManager>().Get<string>("ModelType");
            //TypeMsg = ModelType == "0" ? "已预约" : "无预约";
            //取职业字典所有数据
            DictionaryType = GetDictionaryTypes();
            sqTypes = GetSQTypes();
            if (crjManager == null) { crjManager = new CrjPreapplyManager(); }

            //判断太极是否存在未上传业务，存在则取消之前业务id
            if (!TjConfig.TjModel.IsConnectionTj && OwnerViewModel?.UnOverYwList == null || OwnerViewModel?.UnOverYwList.Count <= 0) return;

            var ywlist = OwnerViewModel?.UnOverYwList?.Count;
            Log.Instance.WriteInfo("核查到未上传导引业务id：" + OwnerViewModel?.UnOverYwList?.FirstOrDefault() + "条数：" + ywlist);

            OwnerViewModel.TaijiPhMode = BookingBaseInfo?.AreaCode?.PARENT_CODE != null ? BookingBaseInfo?.AreaCode?.PARENT_CODE.ToInt().ToString() : QJTConfig.QJTModel.TaijiPHMode;
            foreach (var item in OwnerViewModel?.UnOverYwList)
            {
                Json_I_DY_upload dyUpload = new Json_I_DY_upload
                {
                    ywid = item,
                    dyzt = "1",
                    ydrysf = OwnerViewModel.ydrysf ?? "",
                    dyqy = OwnerViewModel.TaijiPhMode,// OwnerViewModel.TaijiPhMode == "0" ? ((int)TaiJiHelper.dyqy.OneZhan).ToString() : ((int)TaiJiHelper.dyqy.OneZhuo).ToString(),
                    pdlb = OwnerViewModel.TaijiPhMode == "0" ? ((int)TaiJiHelper.pdlb.OneZhan).ToString() : ((int)TaiJiHelper.pdlb.OneZhuo).ToString(),
                    pdh = "",
                    jqbh = TjConfig.TjModel.TJMachineNo
                };
                var result = new TaiJiHelper().Do_DY_Upload(dyUpload);
                Log.Instance.WriteInfo("取消导引业务id：" + item + "【成功】");
                if (ywlist > 0)
                {
                    ywlist--;
                }
            }

            if (ywlist == 0)
            {
                OwnerViewModel.UnOverYwList = new List<string>();
                Log.Instance.WriteInfo("取消所有未上传导引返回：【成功】");
            }



        }

        /// <summary>
        /// 取职业
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetDictionaryTypes()
        {
            var config = _configManager;
            List<DictionaryType> lst = new List<DictionaryType>();

            lst = config.Get<List<DictionaryType>>();
            lst = lst?.Where(t => t.KindType == ((int)KindType.Job).ToString() && t.Status == 1)?.ToList();

            return lst;
        }

        /// <summary>
        /// 取申请类型
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetSQTypes()
        {
            return _configManager?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
        }

        /// <summary>
        /// 读卡倒计时关闭读卡器
        /// </summary>
        /// <param name="second">秒</param>
        public void CloseReadCard()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = QJTConfig.QJTModel.TOutSeconds * 1000;//执行间隔时间,单位为毫秒; 
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(CloseReadCardThread);
        }
        public void CloseReadCardThread(object source, System.Timers.ElapsedEventArgs e)
        {
            Log.Instance.WriteInfo("倒计时结束，开始关闭身份证阅读器");
            if (ReadIDCardHelper.Instance.DoCloseIDCard().IsSucceed)
            {
                Log.Instance.WriteInfo("关闭身份证阅读器成功！");
            }

            isNext = true;//退出读卡循环
            //ReadIDCardThread.Abort(0);
            //ReadIDCardThread.Join();
        }

        /// <summary>
        /// 读取身份证号码信息
        /// </summary>
        private void ReadIDCard()
        {
            try
            {
                Log.Instance.WriteInfo(isNext + "开始读卡：执行身份证读卡循环，方法名：ReadIDCardFun ");

                while (!isNext)
                {
                    IdCardInfo model = null;

                    ////读卡初始化校验
                    //if (!Verification())
                    //    break;

                    //开始读卡
                    ReturnInfo rinfo = ReadIDCardHelper.Instance.DoReadIDCardInfo(out model, "GetImage");
                    if (rinfo.IsEmpty())
                    {
                        Log.Instance.WriteInfo("感应读卡异常：null");
                        Log.Instance.WriteError("感应读卡异常：null");

                        //增加间隔500秒寻卡 增加continue by 2021年7月13日13:56:22 wei.chen
                        Thread.Sleep(500);
                        continue;
                    }
                    //增加model判断为成功
                    if (rinfo.IsSucceed && model.IsNotEmpty())
                    {
                        isNext = true;//退出读卡循环
                        Log.Instance.WriteInfo("读卡成功：【" + CommandTools.ReplaceWithSpecialChar(model.FullName) + "】【" + CommandTools.ReplaceWithSpecialChar(model.IDCardNo) + "】");

                        //页面停止倒计时
                        StopTimeOut();
                        Log.Instance.WriteInfo("倒计时结束，开始关闭身份证阅读器");
                        if (ReadIDCardHelper.Instance.DoCloseIDCard().IsSucceed)
                        {
                            Log.Instance.WriteInfo("关闭身份证阅读器成功！");
                        }

                        //挪到释放统一做 by 2021年7月6日18:11:05 wei.chen
                        //ReadIDCardHelper.Instance.DoCloseIDCard();

                        _common.IdentityVerification(model, true);
                        //不需要跳出 通过 isNext控制了 by 2021年7月6日17:17:27 wei.chen
                        //break;
                    }
                    else
                    {
                        //InitHardware();
                        //Log.Instance.WriteInfo("读卡失败，返回失败信息：" + rinfo.MessageInfo);
                        if (rinfo.ReturnValue.IsNotEmpty() && rinfo.ReturnValue?.ToString() == "4")
                        {
                            Log.Instance.WriteInfo("读卡失败，读卡返回【失败】，返回首页重新加载。");
                            //OwnerViewModel?.MessageTips("读卡失败，请重试！", () =>
                            //{

                            //改掉方式，重新进入读卡，退出后预约数据会被清空为null了会引起“查询预约基础信息发生错误！”。 by 2021年7月12日17:22:35 wei.chen
                            // base.DoExitFunction(null);
                            model = null;
                            iDCardNo = "";
                            //初始化身份证阅读器
                            Log.Instance.WriteInfo(!InitHardware() ? "初始化身份证阅读器失败！" : "【重新】初始化身份信息：成功");
                            Thread.Sleep(500);
                            isNext = false;//重新启动读卡线程

                            //});
                        }
                    }

                    //增加间隔500秒寻卡 by 2021年7月13日13:56:22 wei.chen
                    Thread.Sleep(500);
                }

            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("身份证循环读取出现异常：" + ex);
                Log.Instance.WriteInfo("身份证循环读取出现异常：" + ex);
                //base.DoExitFunction(null);
            }
            finally
            {
                Thread.Sleep(500);
                //Log.Instance.WriteInfo(" ================== 结束【刷身份证】================== ");
            }

        }

        /// <summary>
        /// 读卡初始化校验
        /// </summary>
        /// <returns></returns>
        public bool Verification()
        {
            if (this.IsEmpty())
            {
                Log.Instance.WriteInfo("this = null, 读卡信息为空，退出读卡循环");
                return false;
            }
            if (isNext)
            {
                Log.Instance.WriteInfo("isNext = true, 下一步标识为true，退出读卡循环");
                return false;
            }

            if (ReadIDCardThread.IsEmpty() || !ReadIDCardThread.IsAlive)
            {
                Log.Instance.WriteInfo("读卡线程 = null 或者线程 IsAlive = false, 退出读卡循环");
                return false;
            }
            //if (!OwnerViewModel.CheckServeStatus())
            //{
            //    ReadIDCardHelper.Instance.DoCloseIDCard();
            //    Log.Instance.WriteInfo("检测网络未连接，退出读卡循环");
            //    return false;
            //    //DoNextFunction("NotificationPage");
            //}
            return true;
        }

        /// <summary>
        /// 初始化智慧大厅硬件
        /// </summary>
        public bool InitHardware()
        {
            ReadIDCardHelper.Instance.DoCloseIDCard();
            int iTimes = 0;
            while (!ReadIDCardHelper.Instance.DoReadIDCardInit().IsSucceed)
            {
                if (++iTimes >= 3)
                {
                    ZHPHMachineWSHelper.ZHPHInstance.WriteDeviceAlarm(EnumTypeALARMTYPEID.Fault20,
                        EnumTypeALARMCODE.ALARMCODE_10000012, "初始化身份证读卡器失败");
                    break;
                }
            }
            //Log.Instance.WriteInfo("初始化身份证读卡器成功，开始读卡操作...");
            return true;
        }

        /// <summary>
        /// 身份证18位自动 下一步操作
        /// </summary>
        /// <param name="idCardInfo"></param>
        public void AutoDoNextFun(string idCardInfo)
        {
            //OwnerViewModel.IsShowHiddenLoadingWait("正在核查您的身份信息，请稍候......");
            iDCardNo = idCardInfo;
            //关闭读卡线程
            isNext = true;
            Log.Instance.WriteInfo("手输身份证，IsNext为true，关闭读卡器");

            //放在核查成功之后再关闭 by 2021年7月13日15:43:26 wei.chen
            //ReadIDCardHelper.Instance.DoCloseIDCard();

            //ReadIDCardThread.Abort(0);
            //ReadIDCardThread.Join();
            DoNextFunction(null);
        }
        #endregion

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public override void DoExitFunction(object obj)
        {
            if (sw.IsRunning)
                sw.Restart();
            else
                sw.Start();

            ReadIDCardHelper.Instance.DoCloseIDCard();
            isNext = true;
            //MainWindowViewModels.Instance.StopTimeOut();
            //MainWindowViewModels.Instance.ReturnHome();

            sw.Stop();
            base.DoExitFunction(obj);
        }

    }
}
