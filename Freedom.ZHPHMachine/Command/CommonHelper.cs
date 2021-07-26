using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Freedom.BLL;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Hardware;
using Freedom.Models;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.CrjDataModels;
using Freedom.Models.DataBaseModels;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.View;
using Freedom.ZHPHMachine.ViewModels;
using MachineCommandService;


namespace Freedom.ZHPHMachine.Command
{
    public class CommonHelper : ViewModelBase
    {
        #region 字段
        private readonly MainWindowViewModels OwnerViewModel = MainWindowViewModels.Instance;


        /// <summary>
        /// BLL层查全国对象
        /// </summary>
        private CrjPreapplyManager crjManager;

        public static Dev_AlarmInfo DevAlarm = new Dev_AlarmInfo();//设备故障信息
        //public BookingModel BookingBaseInfo = new BookingModel();//预约model
        private List<DictionaryType> sqTypes = new List<DictionaryType>();//申请类别
        /// <summary>
        /// 身份证信息
        /// </summary>
        IdCardInfo info = new IdCardInfo();
        /// <summary>
        /// 外网预约信息
        /// </summary>
        public List<PH_YYSQXX_TB> wsYysqxx = new List<PH_YYSQXX_TB>();

        /// <summary>
        /// 是否有在办业务提醒（太极）
        /// </summary>
        public bool _IsTipsMessage = true;
        /// <summary>
        /// 是否有完结业务提醒
        /// </summary>
        public bool _IsWjywMessage = false;
        /// <summary>
        /// 有预约和派号信息
        /// </summary>
        public PH_YYSQXX_TB CallNo = new PH_YYSQXX_TB();
        /// <summary>
        /// 是否是国家公务人员 0：否 1：是
        /// </summary> 
        private bool _isOfficial = false;
        /// <summary>
        /// 是否是受控对象 0：否 1：是
        /// </summary>
        private bool _isCheckSpecil = false;

        private List<DictionaryType> _dictionaryType;

        /// <summary>
        /// 人事主管单位
        /// </summary>
        private string _RSZGDW;
        /// <summary>
        /// 是否新疆重点人员
        /// </summary>
        public bool _issfxjzdry = false;
        /// <summary>
        /// 是否重点地区人员
        /// </summary>
        public bool _issfzddq = false;

        /// <summary>
        /// 户籍信息
        /// </summary>
        public BasicInfo Basicinfo = null;
        /// <summary>
        /// 是否作废重取
        /// </summary>
        public bool IsAgain = false;
        #endregion

        /// <summary>
        /// 身份效验方法
        /// </summary>
        /// <param name="info">身份信息</param>
        /// <param name="isAuto">是否手输</param>
        public void IdentityVerification(IdCardInfo info, bool isAuto)
        {
            try
            {
                //OwnerViewModel.ContentPageSetting("BlankPage");
                Log.Instance.WriteInfo("\n*********************** 结束【刷身份证】身份证识读界面*********************** ");
                Log.Instance.WriteInfo(" \n*********************** 进入【身份核查】【" +
                                       CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + " " +
                                       CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "】页面*********************** ");
                OwnerViewModel.IsShowHiddenLoadingWait("正在核查您的身份信息，请稍候......");
                new Thread(delegate ()
                {
                    string strTraget = string.Empty;

                    if (!GoNext(info, isAuto, out string msg))
                    {
                        OwnerViewModel?.MessageTips(msg, (() =>
                        {
                            DoExitFunction(null);
                        }));
                    }
                    //作废重取 提示
                    if (!_IsTipsMessage)
                    {
                        OwnerViewModel?.MessageTips("系统查询到您存在有效排队号，若需要重复取号，上一个号码自动作废！如需取号请点击【作废重取】按钮。", (() =>
                        {
                            Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                            DoExitFunction(null);
                        }), () =>
                        {
                            Log.Instance.WriteInfo("点击【作废重取】按钮，开始过号操作");
                            OverNo(CallNo);
                            _IsTipsMessage = false;
                            IsAgain = true;
                            GoNext(info, isAuto, out string msg1);
                        });

                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("身份核查发生错误！" + ex);
            }

        }

        /// <summary>
        /// 下一步（重写 ）
        /// </summary>
        /// <param name="info">身份证信息</param>
        /// <param name="isAuto">刷卡或手输</param>
        /// <param name="msg">提示消息/页面跳转地址</param>
        /// <returns></returns>
        private bool GoNext(IdCardInfo info, bool isAuto, out string msg)
        {

            msg = string.Empty;
            bool IsFlag = false;
            try
            {
                ClearCache();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //Log.Instance.WriteInfo(" =============开始 身份核查方法============");
                //Log.Instance.WriteInfo(isAuto ? "刷身份证进入" : "手输身份证号码");
                //关闭页面定时器
                thCountdown?.Abort();
                //修改设备受理中
                bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(OwnerViewModel._devid, EnumTypeSTATUS.ACCEPTTING);
                Log.Instance.WriteInfo(blnResult
                    ? $"修改设备状态【{EnumType.GetEnumDescription(EnumTypeSTATUS.ACCEPTTING)}】：成功"
                    : $"修改设备状态【{EnumType.GetEnumDescription(EnumTypeSTATUS.ACCEPTTING)}】：失败");

                #region 1、查询预约信息

                Log.Instance.WriteInfo("\n======1、开始查询平台【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "预约信息======");

                //查询当前大厅当天此人预约信息
                //非陆河地区查询预约信息直接查询全国接口
                var result = ZHPHMachineWSHelper.ZHPHInstance.S_YYSQXX(info?.IDCardNo, DateTime.Now);

                if (BookingBaseInfo.IsEmpty())
                {
                    Log.Instance.WriteInfo("查询预约基础信息发生错误！");
                    msg = "查询预约信息发生错误！请重试！";
                    return false;
                }
                //是否有预约信息
                if (result != null && result.IsSucceed && (result.ReturnValue == null || result.ReturnValue?.Count() <= 0))
                {
                    Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "今日未查询到预约信息!");
                    msg = "Booking/BookingTarget";
                    BookingBaseInfo.BookingSource = 1;

                    //查找全国预约信息
                    if (QJTConfig.QJTModel.IsCheckYYXX_ZZZP)
                    {
                        CheckYyxx(info);
                    }
                }
                else
                {
                    BookingBaseInfo.BookingSource = 0;
                    Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info.FullName?.ToString()) + "今日存在有效预约信息" + result?.ReturnValue?.Length + "条！");
                    if (result?.ReturnValue != null)
                    {
                        var yYsqxxTb = (PH_YYSQXX_TB)result?.ReturnValue?.FirstOrDefault();

                        var yySqList = (List<PH_YYSQXX_TB>)result?.ReturnValue?.ToList();
                        if (yYsqxxTb?.PH_SENDLIST != null)
                        {
                            if (TjConfig.TjModel.IsConnectionTj && _IsTipsMessage)//
                            {
                                if (yYsqxxTb.PH_SENDLIST[0]?.CLZT.ToInt() == 0 || yYsqxxTb.PH_SENDLIST[0]?.CLZT.ToInt() == 1 || yYsqxxTb.PH_SENDLIST[0]?.CLZT.ToInt() == 2 || yYsqxxTb.PH_SENDLIST[0]?.CLZT.ToInt() == 4)
                                {
                                    Log.Instance.WriteInfo("查询到您有排队数据");
                                    if (yYsqxxTb?.PH_SENDLIST?.FirstOrDefault()?.CALLUSER?.IsNotEmpty() == true)
                                        CallNo = yYsqxxTb;
                                    _IsTipsMessage = false;
                                    return true;
                                }
                                else
                                {
                                    Log.Instance.WriteInfo("未查询到您有排队数据");
                                }

                            }
                        }
                        else
                        {
                            Log.Instance.WriteInfo("预约信息返回为空！！！");
                        }

                        if (yySqList?.Count > 0)//&& DjzConfig.DjzModel.IsConnectionDjz)
                        {
                            foreach (var item in yySqList)
                            {
                                if (item?.CLBZ == "2")
                                {
                                    Log.Instance.WriteInfo("查询到您有未完结业务：" + item?.SQLB);
                                    OwnerViewModel.zbywList.Add(item.SQLB);
                                    _IsWjywMessage = true;
                                }
                            }
                        }

                    }
                }
                Log.Instance.WriteInfo("\n======1、结束查询平台【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "预约信息======");
                //sw.Stop();
                //Log.Instance.WriteInfo("======查询预约时间用时：" + sw.ElapsedMilliseconds.ToString() + "======");
                #endregion

                #region 2、核查人口库
                //sw.Restart();
                Log.Instance.WriteInfo("\n======2、开始核查【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo) + "】人口信息========");

                //Log.Instance.WriteInfo(isAuto ? "刷身份证进入" : "手输身份证号码，开始构造身份证信息");
                //if (!isAuto)
                //{
                //    SaveSfzInfo();
                //}
                Log.Instance.WriteInfo("IsConnectionDjz：" + DjzConfig.DjzModel?.IsConnectionDjz + " IsConnectionTj：" + TjConfig.TjModel?.IsConnectionTj + "======");
                #region 启用航信
                if (DjzConfig.DjzModel.IsConnectionDjz)
                {
                    //连接大集中查询人口数据
                    if (!CheckInformationDjz(info))
                    {
                        msg = "全国人口库服务器响应失败，请重试！";
                        return false;
                    }
                    //广东非本省户口提示不能办理
                    if (OwnerViewModel?.IsXuwen == true && OwnerViewModel?.IsGuangDongRegister == false)
                    {
                        msg = "您非广东户籍，不符合自助办理条件，请移步人工窗口办理！";
                        return false;
                    }

                    if (OwnerViewModel?.IsLocalRegister == false)
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        Log.Instance.WriteInfo("非本省户籍，不符合自助办理条件，派人工号标识为True");
                    }

                    //if (OwnerViewModel?.IsWuHan == true && OwnerViewModel?.IsHuBeiRegister == true)
                    //{
                    //    OwnerViewModel.isVALIDXCYY = "1";
                    //    Log.Instance.WriteInfo("非湖北本省户籍，不符合自助办理条件，派人工号标识为True");
                    //}

                    #region 核查国家工作人员和控制对象

                    //是否开启核查国家工作人员
                    if (QJTConfig.QJTModel.IsOfficial)
                    {

                        //判断是否是国家公务人员
                        _isOfficial = QueryOfficialAsync(info, out msg);
                        //_isOfficial = true;
                        //北京区域不做判断，在后面区域选择中接口返回判断
                        if (_isOfficial)
                        {
                            if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                                OwnerViewModel.IsOfficial += "、国家工作人员";
                            else
                                OwnerViewModel.IsOfficial += "普通群众（国家工作人员）";

                            OwnerViewModel.IsManual = true;
                            if (OwnerViewModel?.IsShanXi == true || OwnerViewModel?.IsXuwen == true || OwnerViewModel?.IsWuHan == true)
                                OwnerViewModel.isVALIDXCYY = "1";
                            if (OwnerViewModel?.IsLeiZhou == true)
                            {
                                msg = TipMsgResource.GJGWRYTipMsg;
                                return false;
                            }
                            Log.Instance.WriteInfo("核查到国家工作人员！" + "人工号判断为True");
                        }

                    }

                    //是否开启核查控制对象
                    if (QJTConfig.QJTModel.IsCheckSpecil)
                    {
                        //判断是否是控制对象
                        _isCheckSpecil = CheckSpecialList(info.IDCardNo);
                        //北京区域不做判断，在后面区域选择中接口返回判断
                        if (_isCheckSpecil)
                        {
                            if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                                OwnerViewModel.IsOfficial += "、（1001）";
                            else
                                OwnerViewModel.IsOfficial += "普通群众 (1001)";

                            OwnerViewModel.IsManual = true;

                            msg = OwnerViewModel?.IsShanXi == true ?
                                "请前往人工拍照区域进行拍照" : TipMsgResource.SKDXTipMsg;
                            if (OwnerViewModel?.IsXuwen == true || OwnerViewModel?.IsWuHan == true || OwnerViewModel?.IsShanXi == true)
                            {
                                OwnerViewModel.isVALIDXCYY = "1";
                                Log.Instance.WriteInfo("核查到控制对象！人工号判断为True");
                            }
                            else
                            {
                                Log.Instance.WriteInfo("核查到控制对象，请前往人工区办理！");
                                return false;
                            }
                        }

                    }

                    sw.Stop();
                    Log.Instance.WriteInfo("\n===查询全国人口库用时时长：" + sw.ElapsedMilliseconds.ToString() + "===");

                    #endregion
                }
                #endregion

                #region 启用太极
                if (TjConfig.TjModel.IsConnectionTj && !DjzConfig.DjzModel.IsConnectionDjz)
                {
                    var taiJiResult = ConnectingTaiJi(info, out msg);
                    if (!taiJiResult)
                    {
                        return false;
                    }
                }
                #endregion

                if (string.IsNullOrEmpty(info.pNational) && !QJTConfig.QJTModel.IsCheckInformation)
                {
                    info.pNational = "01";
                }

                //_isOfficial = true;
                //info.IsOfficial = _isOfficial;
                //if (QJTConfig.QJTModel.IsOfficial)
                //{
                //    Log.Instance.WriteInfo("开启核验【国家公务人员】选项，核查结果：");
                //}
                //Log.Instance.WriteInfo(info.IsOfficial ? "核查身份【国家公务人员】" : "核查身份【非国家公务人员】");
                //info.IsCheckSpecil = _isCheckSpecil;
                //if (QJTConfig.QJTModel.IsCheckSpecil)
                //{
                //    Log.Instance.WriteInfo("开启核验【控制对象】选项，核查结果：");
                //}
                //Log.Instance.WriteInfo(info.IsCheckSpecil ? "核查身份【是控制对象】" : "核查身份【非控制对象】");

                //拼音
                Hz2Py.GetPinYin(info);
                BookingBaseInfo.CardInfo = info;

                sw.Stop();
                //Log.Instance.WriteInfo("查询全国人口库用时时长：" + sw.ElapsedMilliseconds.ToString() + "");
                Log.Instance.WriteInfo("\n======2、结束核查【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo) + "】人口信息========");
                #endregion

                #region 3、判断取号方式（16周岁以下、65周岁以上）
                sw.Restart();
                bool ages = Get_NomalAge(info, out msg);
                if (!ages)
                    return false;

                //sw.Stop();
                //Log.Instance.WriteInfo("===判断年龄是否可以取号用时时长：" + sw.ElapsedMilliseconds.ToString() + "===");
                #endregion

                #region  4、查询制证照片，是否存在当天有效制证照片
                //sw.Restart();
                Log.Instance.WriteInfo("\n======3、开始获取 【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "】的制证照片======");
                GetZzzpFunc(info);
                //sw.Stop();
                //Log.Instance.WriteInfo("===查询制证照片用时时长：" + sw.ElapsedMilliseconds.ToString() + "===");
                #endregion

                #region 进入下一个操作页面

                //Log.Instance.WriteInfo("\n======4、进入下一个页面跳转判断======");

                //当天有预约信息
                if (result?.ReturnValue != null && result.IsSucceed && result.ReturnValue?.Count() > 0)
                {
                    //有预约信息 给预约信息默认选中
                    var yytypes = new List<DictionaryType>();
                    //所有业务  
                    var carTypes = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString())?.OrderBy(t => t.Code)?.ToList();
                    //预约信息
                    var str = result?.ReturnValue?.Select(t => t?.SQLB)?.Distinct()?.ToList();
                    foreach (var item in str)
                    {
                        var yytype = carTypes?.Where(t => t?.Code == item)?.ToList()?.FirstOrDefault();
                        yytypes?.Add(yytype);
                    }
                    BookingBaseInfo.SelectCardTypes = yytypes;

                    IEnumerable<string> strlst = new List<string>();

                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        PH_YYSQXX_TB yYSQXX_TB = (PH_YYSQXX_TB)result?.ReturnValue[0];

                        strlst = result?.ReturnValue?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t?.SQLB)))?.Distinct();
                        #region 预约信息赋值

                        if (OwnerViewModel._hasYYXX.Count > 0 && BookingBaseInfo.QWD.IsEmpty() && BookingBaseInfo.HasJob.IsEmpty())
                        {
                            SetYyxx(yYSQXX_TB);
                        }
                        else if (OwnerViewModel._hasYYXX?.Count == 0 && BookingBaseInfo.QWD.IsEmpty() && BookingBaseInfo.HasJob.IsEmpty())
                        {
                            SetYyxx(yYSQXX_TB);
                        }
                        #endregion

                    }
                    else
                    {
                        #region 非太极预约数据赋值

                        PH_YYSQXX_TB yYSQXX_TB = (PH_YYSQXX_TB)result.ReturnValue?.FirstOrDefault();

                        //赋值预约数据
                        SetYyxx(yYSQXX_TB);

                        //拼接已预约的业务类型
                        strlst = result?.ReturnValue?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.SQLB)))?.Distinct();

                        //深圳地区提示，只显示外网预约办证类型
                        if (OwnerViewModel?.IsShenZhen == true)
                        {
                            var wsyy = result?.ReturnValue?.Where(t => t.BOOK_TYPE == "0")?.ToList();
                            if (wsyy?.Count > 0)
                            {
                                strlst = wsyy?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.SQLB)))?.Distinct();
                            }
                        }
                        msg = "ScanningPhotoReceipt";
                        BookingBaseInfo.BookingSource = 0;
                        //如果手动输入身份证号码
                        if (string.IsNullOrWhiteSpace(info?.FullName))
                        {
                            info.FullName = yYSQXX_TB?.ZWXM;
                        }
                        #endregion

                    }

                    //太极返回的预约办证类别赋值
                    if (OwnerViewModel._hasYYXX?.Count > 0)
                    {
                        sqTypes = new List<DictionaryType>();
                        var carType = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString())?.OrderBy(t => t.Code)?.ToList();
                        //预约业务列表
                        foreach (var item in OwnerViewModel._hasYYXX)
                        {
                            sqTypes.Add(carType?.Where(t => t.Code == item)?.ToList()[0]);
                        }
                        BookingBaseInfo.SelectCardTypes = sqTypes;
                    }
                    BookingBaseInfo.StrCardTypes = string.Join("、", strlst);
                    
                }
                
                sw.Stop();

                //Log.Instance.WriteInfo("======核查身份用时时长：" + sw.ElapsedMilliseconds.ToString() + "======");
                if (BookingBaseInfo.BookingSource == 1)//当天无预约
                {
                    #region  未查询到预约信息不允许直接取号或其它处理 
                    if (OwnerViewModel?.IsBeijing == true)//北京
                    {
                        if (OwnerViewModel == null)
                        {
                            Log.Instance.WriteInfo("检测到主程序公共类为空，直接跳转基本信息页");
                            //OwnerViewModel.ContentPageSetting("Booking/BookingTarget");
                            msg = "主程序发生数据错误，请重试！";
                            IsFlag = false;
                            return IsFlag;
                        }
                        #region 北京地区
                        //Log.Instance.WriteInfo("======未查询到预约信息，是否北京户籍： " + (OwnerViewModel?.IsBeijingRegister) + "======");
                        //北京地区分配取号信息跳转页面
                        if (OwnerViewModel?.IsTakePH_No == true && OwnerViewModel?.IsBeijingRegister == true)
                        {
                            HaveZBYW(info);
                            //Log.Instance.WriteInfo("======北京户籍没有预约信息可直接进行办理======");
                            OwnerViewModel.MessageTips("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", (() =>
                            {
                                Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                                DoExitFunction(null);
                            }), () =>
                            {
                                Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                                DoNextFunction("Booking/BookingBaseInfoByBeijing");
                            });
                            //OwnerViewModel?.SetFrameMessagePage("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", true, true, "前往", "Booking/BookingTarget");
                            IsFlag = true;
                        }
                        else if (OwnerViewModel?.IsTakePH_No == false && OwnerViewModel?.IsBeijingRegister == true)
                        {
                            //Log.Instance.WriteInfo("======北京户籍，无预约可直接办理======");
                            OwnerViewModel.MessageTips("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", (() =>
                            {
                                if(IsAgain)
                                {
                                    Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                                    DoNextFunction("Booking/BookingBaseInfoByBeijing");
                                }
                                else
                                {
                                    Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                                    DoExitFunction(null);
                                }   
                            }), () =>
                            {
                                Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                                DoNextFunction("Booking/BookingBaseInfoByBeijing");
                            });
                            //OwnerViewModel?.SetFrameMessagePage("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", true, true, "前往", "Booking/BookingTarget");
                            IsFlag = true;

                            //正常跳转播放现场预约语音文件
                            TTS.PlaySound("预约机-提示-现场预约");
                        }
                        else
                        {
                            if (OwnerViewModel?.IsTakePH_No == false && OwnerViewModel?.IsBeijing == true && OwnerViewModel?.IsBeijingRegister == false && !QJTConfig.QJTModel.IsNormalYY)
                            {
                                IsFlag = true;
                                //OwnerViewModel.SetFrameMessagePage("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", false, true, "确定");
                                OwnerViewModel.MessageTips("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                                {
                                    DoExitFunction(null);
                                }));
                                //非北京户籍没有预约信息不能办理
                                TTS.PlaySound("预约机-提示-北京无预约");

                            }
                            else if (QJTConfig.QJTModel.IsNormalYY)
                            {
                                //Log.Instance.WriteInfo("【北京启用现场预约配置】");
                                OwnerViewModel.MessageTips("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", (() =>
                                {
                                    Log.Instance.WriteInfo("点击【返回】按钮，返回主页面");
                                    DoExitFunction(null);
                                }), () =>
                                {
                                    Log.Instance.WriteInfo("点击【前往】按钮");
                                    DoNextFunction("Booking/BookingBaseInfoByBeijing");
                                });
                                //OwnerViewModel?.SetFrameMessagePage("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", true, true, "前往", "Booking/BookingTarget");
                                IsFlag = true;
                            }
                            IsFlag = true;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 非北京地区正常流程跳转现场预约页面
                        //非北京地区正常流程跳转现场预约页面
                        if (OwnerViewModel?.IsTakePH_No == true && OwnerViewModel?.IsBeijing == false)
                        {
                            //Log.Instance.WriteInfo("======无预约信息，直接取号模式跳转选择预约时间页面======");
                            //直接取号模式 
                            DoNextFunction("Booking/BookingTarget");
                            //OwnerViewModel?.SetFrameMessagePage("找不到您今日的有效预约数据！如需现场预约请点击【前往】按钮。", true, true, "前往", "Booking/BookingTarget");
                            IsFlag = true;
                        }
                        else
                        {
                            //Log.Instance.WriteInfo("======无预约,判断是否继续跳转======");

                            //深圳地区现场不派人工号 提示去人工窗口取号
                            if (OwnerViewModel?.IsShenZhen == true)
                            {
                                OwnerViewModel?.MessageTips(info?.FullName + "系统未查询到您有效预约数据，请移步4、5号审核窗口审核资料窗口取号！", (() =>
                                {
                                    DoExitFunction(null);
                                }));
                                //OwnerViewModel?.SetFrameMessagePage("系统未查询到您有效预约数据，请移步4、5号审核窗口审核资料窗口取号！", true, true, "确定");
                                IsFlag = true;
                                //测试模式 开放预约 注释
                                //OwnerViewModel.isVALIDXCYY = "1";
                                //OwnerViewModel?.ActionToGo("", (() =>
                                //{
                                //    Log.Instance.WriteInfo("深圳地区，现场预约，跳过提示，进入选择预约时间页面");
                                //    this.DoNext.Execute("Booking/BookingTarget");
                                //}), null, "取号");
                            }
                            else
                            {
                                //武汉 根据配置开放现场预约
                                if (OwnerViewModel?.IsWuHan == true && !QJTConfig.QJTModel.IsNormalYY)
                                {
                                    //正常跳转播放现场预约语音文件
                                    TTS.PlaySound("预约机-提示-北京无预约");
                                    OwnerViewModel?.MessageTips(info?.FullName + "找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                                    {
                                        DoExitFunction(null);
                                    }));
                                    //OwnerViewModel.SetFrameMessagePage("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", true, true, "确定");
                                    IsFlag = true;
                                }
                                else
                                {
                                    //正常跳转播放现场预约语音文件

                                    OwnerViewModel?.MessageTips("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！如需现场预约请点击【前往】按钮。", (() =>
                                    {
                                        Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                                        DoNextFunction(null);
                                    }), () =>
                                    {
                                        Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                                        DoNextFunction("Booking/BookingTarget");
                                        //DoNext
                                    });
                                    TTS.PlaySound("预约机-提示-现场预约");
                                    IsFlag = true;
                                }

                            }


                            IsFlag = true;
                        }
                        #endregion
                    }
                    #endregion
                }
                else   //已查询到当天的预约信息
                {
                    #region 已查询到当天的预约信息
                    //有预约播放重新预约语音文件
                    TTS.PlaySound("预约机-提示-重新预约");
                    string bookingDate = BookingBaseInfo?.BookingTarget?.BookingDt?.ToString("yyyy-MM-dd");
                    string bookingTime = BookingBaseInfo?.BookingTarget?.Title;

                    Log.Instance.WriteInfo($"{info.FullName}您已预约{bookingDate}{bookingTime} 办理{BookingBaseInfo.StrCardTypes}业务");

                    if (OwnerViewModel?.IsBeijing == false && OwnerViewModel?.IsTakePH_No == true)
                    {
                        Log.Instance.WriteInfo($"======{info.FullName}您已预约{bookingDate} {bookingTime}办理{BookingBaseInfo.StrCardTypes}业务======");
                        DoNextFunction("Booking/BookingTarget");
                        //OwnerViewModel.ContentPageSetting("Booking/BookingTarget");
                        //this.DoNext.Execute("Booking/BookingTarget");
                        IsFlag = true;
                    }

                    if (OwnerViewModel?.IsBeijing == true)
                    {
                        #region 北京地区

                        if (!_IsTipsMessage)
                        {
                            Log.Instance.WriteInfo("点击【作废重取】按钮，过号操作【成功】");
                            //OwnerViewModel.ContentPageSetting("Booking/BookingTarget");
                            DoNextFunction("Booking/BookingBaseInfoByBeijing");
                            return true;
                        }
                        if (OwnerViewModel?.zbywxx != null)
                        {
                            HaveZBYW(info);
                            IsFlag = true;
                        }
                        else
                        {

                            //您已预约{ bookingDate}{ bookingTime}办理{ BookingBaseInfo.StrCardTypes}业务,若您需要重新预约办理新业务,请点击【重新预约】按钮，上一次的预约办理业务将自动失效,若有疑问请咨询现场工作人员。

                            OwnerViewModel?.MessageTips($"{info.FullName}您已预约 {bookingDate}-{bookingTime} 办理[{BookingBaseInfo.StrCardTypes}]业务,请点击【取号】按钮取号。", (() =>
                            {
                                Log.Instance.WriteInfo("点击【返回】按钮，返回主页面");
                                DoExitFunction(null);
                            }), () =>
                            {
                                Log.Instance.WriteInfo("点击【取号】按钮");
                                //this.DoNext.Execute("Booking/BookingTarget");
                                //跳转填写基本资料页面
                                DoNextFunction("Booking/BookingBaseInfoByBeijing");
                                //清空预约信息
                                //BookingBaseInfo?.BookingTarget = null;
                            });
                            IsFlag = true;
                        }
                        #endregion
                    }
                    else
                    {
                        //非北京地区有预约信息正常流程
                        #region 非北京地区有预约信息正常流程
                        //深圳地区外网预约提示
                        if (OwnerViewModel?.IsShenZhen == true && BookingBaseInfo.Book_Type == "0")
                        {
                            Log.Instance.WriteInfo("======外网预约受理类型为：" + result.ReturnValue.FirstOrDefault().SLLX + "======");
                            if (result.ReturnValue.FirstOrDefault().SLLX == 1)
                            {
                                //OwnerViewModel?.MessageTips($"{info.FullName}您已预约{bookingDate}{bookingTime} 办理{BookingBaseInfo.StrCardTypes}业务,请点击【直接取号】按钮进行取号。", (() =>
                                //{
                                //    GetPhNo(result.ReturnValue);
                                //    //跳转填写基本资料页面
                                //    if (wsYysqxx != null && wsYysqxx.Count > 0)
                                //        InsertYyxx(wsYysqxx);
                                //    DoNextFunction("ScanningPhotoReceipt");
                                //    //this.DoNext.Execute("ScanningPhotoReceipt");
                                //}), null, 10, "直接取号");
                                OwnerViewModel?.MessageTips("您不符合自助办理条件，请移步4、5号审核资料窗口取号！", (() =>
                                {
                                    DoExitFunction(null);
                                }));
                                //OwnerViewModel.SetFrameMessagePage("您不符合自助办理条件，请移步4、5号审核资料窗口取号！", true, true, "确定");
                                IsFlag = true;
                            }
                            else
                            {
                                PH_YYSQXX_TB yYSQXX_TB2 = (PH_YYSQXX_TB)result.ReturnValue?.FirstOrDefault();
                                string msgstr =
                                    $"{info.FullName}您已预约{bookingDate}{bookingTime} 办理{BookingBaseInfo.StrCardTypes}业务,请点击【直接取号】按钮进行取号。";
                                if (_IsWjywMessage)
                                {
                                    msgstr = $"{info.FullName}您已存在办结的业务,若您需要重新预约，请点击【重新预约】按钮，已办结业务无法重复办理，若有疑问请咨询现场工作人员。";
                                    OwnerViewModel?.MessageTips(msgstr, (() =>
                                    {
                                        SetYyxx(yYSQXX_TB2);
                                        //DoNextFunc();
                                        //跳转填写基本资料页面
                                        DoNextFunction("Booking/BookingTarget");
                                        //this.DoNext.Execute("Booking/BookingTarget");
                                    }), null, 10, "重新预约");
                                    IsFlag = true;
                                }
                                else
                                {
                                    OwnerViewModel?.MessageTips(msgstr, (() =>
                                    {
                                        GetPhNo(result.ReturnValue);
                                        //跳转填写基本资料页面
                                        if (wsYysqxx != null && wsYysqxx.Count > 0)
                                            InsertYyxx(wsYysqxx);
                                        DoNextFunction("ScanningPhotoReceipt");
                                        //this.DoNext.Execute("ScanningPhotoReceipt");
                                    }), null, 10, "直接取号");
                                    IsFlag = true;
                                }
                            }
                        }
                        else if (OwnerViewModel?.IsShenZhen == true && BookingBaseInfo.Book_Type == "1")
                        {
                            if (result.ReturnValue.FirstOrDefault().SLLX == 1)
                            {

                                OwnerViewModel?.MessageTips("您不符合自助办理条件，请移步4、5号审核资料窗口取号！", (() =>
                                {
                                    DoNextFunction(null);
                                }));
                                IsFlag = true;
                                //OwnerViewModel?.ActionToGo("", (() =>
                                //{
                                //    GetPhNo(result.ReturnValue);
                                //    //BookingBaseInfo.IsGetPHNO = "1";
                                //    Log.Instance.WriteInfo("深圳地区，现场预约，跳过提示，进入选择预约时间页面");
                                //    this.DoNext.Execute("ScanningPhotoReceipt");
                                //}), null, "取号");
                                //IsFlag = true;
                            }
                            else
                            {
                                OwnerViewModel?.MessageTips("您不符合自助办理条件，请移步4、5号审核资料窗口取号！", (() =>
                                {
                                    DoNextFunction(null);
                                }));
                                //OwnerViewModel.SetFrameMessagePage("您不符合自助办理条件，请移步4、5号审核资料窗口取号！", true, true, "确定");
                                //GetPhNo(result.ReturnValue);
                                //OwnerViewModel.isVALIDXCYY = "1";
                                //OwnerViewModel?.ActionToGo("", (() =>
                                //{
                                //    Log.Instance.WriteInfo("深圳地区，现场预约，跳过提示，进入选择预约时间页面");
                                //    this.DoNext.Execute("ScanningPhotoReceipt");
                                //}), null, "取号");
                                IsFlag = true;
                            }

                        }
                        else
                        {
                            //非深圳区域，正常提示
                            string msgstr =
                            $"{info.FullName}您已预约{bookingDate}{bookingTime} 办理{BookingBaseInfo.StrCardTypes}业务,若您需要重新预约办理新业务,请点击【重新预约】按钮，上一次的预约办理业务将自动失效,若有疑问请咨询现场工作人员。";
                            if (OwnerViewModel?.IsXuwen == true && QJTConfig.QJTModel.IsTodayPH)
                            {
                                msgstr = $"{info.FullName}您已预约{bookingDate}{bookingTime} 办理{BookingBaseInfo.StrCardTypes}业务,若您需要重新填表,请点击【重新填表】按钮，上一次的填表数据将自动失效,若有疑问请咨询现场工作人员。";

                            }

                            if (_IsWjywMessage)
                            {
                                msgstr = $"{info.FullName}您已存在办结的业务,若您需要重新预约，请点击【重新预约】按钮，已办结业务无法重复办理，若有疑问请咨询现场工作人员。";
                                OwnerViewModel?.MessageTips(msgstr, (() =>
                                {
                                    RegestFunc();
                                    //跳转填写基本资料页面
                                    DoNextFunction("Booking/BookingTarget");
                                    //this.DoNext.Execute("Booking/BookingTarget");
                                }), null, 10, "重新预约");
                                IsFlag = true;
                            }
                            else
                            {
                                OwnerViewModel?.MessageTips(msgstr, (() =>
                                {
                                    GetPhNo(result.ReturnValue);
                                    //保存外网预约数据
                                    if (wsYysqxx != null && wsYysqxx.Count > 0)
                                        InsertYyxx(wsYysqxx);
                                    //this.DoNext.Execute("ScanningPhotoReceipt");
                                    DoNextFunction("ScanningPhotoReceipt");
                                }), () =>
                                {
                                    RegestFunc();
                                    //跳转填写基本资料页面
                                    DoNextFunction("Booking/BookingTarget");
                                    //清空预约信息

                                }, 10, "直接取号");
                                IsFlag = true;
                            }

                            //IsFlag = true;
                        }

                        #endregion
                        IsFlag = true;
                    }

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                msg = "身份校验报错，请重试！";
                Log.Instance.WriteInfo("======核查身份信息发生异常：" + ex.Message + "======");
                Log.Instance.WriteError("核查身份信息发生异常：：" + ex.Message);
                //OwnerViewModel?.SetFrameMessagePage("身份校验报错，请重试！", true, true, "确定");
                OwnerViewModel?.MessageTips(msg, (() =>
                {
                    DoExitFunction(null);
                }));
            }
            finally
            {
                Log.Instance.WriteInfo(" \n***********************结束【身份核查】【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "】页面***********************");

            }
            return IsFlag;
        }



        /// <summary>
        ///  核查是否国家工作人员
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool QueryOfficialAsync(IdCardInfo info, out string msg)
        {
            bool isOfficial = false;
            msg = string.Empty;

            if (info.Birthday == null)
            {
                var birth = info.IDCardNo.Substring(6, 8);
                info.Birthday = birth;
            }

            var result = crjManager.QueryOfficial(info.IDCardNo, info.FullName, info.Gender, info.Birthday);
            if (int.TryParse(result.ReturnValue?.ToString(), out int returnValue))
            {
                isOfficial = returnValue == 1;
            }
            //上报故障
            if (result.IsException)
            {
                Log.Instance.WriteError("查询全国人口库接口返回异常！");
                Log.Instance.WriteInfo("查询全国人口库接口返回异常！");
                DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                DevAlarm.AlarmInfo = "人口库连接异常";//返回信息
                ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());

                throw new Exception(TipMsgResource.GJGWRYExceptionTipMsg);

            }
            return isOfficial;
        }

        /// <summary>
        /// 核查是否受控对象
        /// </summary>
        /// <param name="idCardNo">身份证号码</param>
        /// <returns></returns>
        private bool CheckSpecialList(string idCardNo)
        {
            bool isCheckSpecil = false;
            var model = new CheckSpecialInfo()
            {
                sfzh = idCardNo,
            };
            var result = crjManager.CheckSpecialList(model);
            if (int.TryParse(result.ReturnValue?.ToString(), out int returnValue))
            {
                isCheckSpecil = returnValue == 1;
            }
            if (result.IsException)
            {
                Log.Instance.WriteError("查询全国人口库接口返回异常！");
                Log.Instance.WriteInfo("查询全国人口库接口返回异常！");
                DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                DevAlarm.AlarmInfo = "人口库连接异常";//返回信息
                ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());
                throw new Exception(TipMsgResource.SKDXExceptionTipMsg);
            }
            return isCheckSpecil;
        }

        /// <summary>
        /// 核查人口信息
        /// </summary>
        /// <param name="model">身份证信息</param>
        /// <param name="info">返回人口信息</param>
        /// <returns></returns>
        private ReturnInfo GetPopulationInfo(IdCardInfo model)
        {
            //查询省内常住人口
            ReturnInfo returnInfo = crjManager.QueryHzInfo(model);
            if (!returnInfo.IsSucceed && returnInfo.ReturnValue == null)
            {
                //查询异地全国人口信息
                returnInfo = crjManager.Qg_queryPeoplenew(model);
            }
            return returnInfo;
        }

        /// <summary>  
        /// 18位身份证号码验证  
        /// </summary>  
        private bool CheckIDCard18(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber.Remove(17), out n) == false
                || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证  
            }
            return true;//符合GB11643-1999标准  
        }


        /// <summary>  
        /// 15位身份证号码验证  
        /// </summary>  
        private bool CheckIDCard15(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = idNumber.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            return true;
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="idCardNo">身份证信息</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool DataVerification(string idCardNo, out string msg)
        {
            msg = "";
            if (string.IsNullOrWhiteSpace(idCardNo))
            {
                msg = TipMsgResource.IDCardNonEmptyTipMsg;
                return false;
            }
            if (idCardNo.Length != 15 && idCardNo.Length != 18)
            {
                msg = TipMsgResource.IDCardErrorTipMsg;
                return false;
            }
            if (idCardNo.Length == 15 && !CheckIDCard15(idCardNo))
            {
                msg = TipMsgResource.IDCardErrorTipMsg;
                return false;
            }
            if (idCardNo.Length == 18 && !CheckIDCard18(idCardNo))
            {
                msg = TipMsgResource.IDCardErrorTipMsg;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取职业
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetDictionaryTypes()
        {
            var config = ServiceRegistry.Instance.Get<ElementManager>();
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
            return ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
        }

        /// <summary>
        /// 保存太极人口照片
        /// </summary>
        /// <param name="base64">照片字符串</param>
        /// <param name="sfzh">身份证为照片名称</param>
        public void SaveImageByTaiji(string base64, string sfzh)
        {
            string imageName = sfzh;

            if (!Directory.Exists(FileHelper.GetLocalPath() + "\\Images"))
            {
                Directory.CreateDirectory(FileHelper.GetLocalPath() + "\\Images");
            }
            string docPath = Path.Combine(FileHelper.GetLocalPath() + "\\Images", imageName + ".jpg");
            //保存路径
            OwnerViewModel.djzPhoto = docPath;
            Log.Instance.WriteInfo(OwnerViewModel?.djzPhoto != null ? "保存人口照片成功！" : "保存人口照片失败！");
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                using (Bitmap bm2 = new Bitmap(ms))
                {
                    bm2.Save(docPath);
                }
            }
        }

        /// <summary>
        /// 保存航信相片接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sfzh"></param>
        public void SaveImageByHx(string path, string sfzh)
        {
            OwnerViewModel.djzPhoto = path;
            Log.Instance.WriteInfo("大集中照片保存路径：" + path);
            //作为人口照片上传到本地服务器

            string photoPath = Path.Combine(FileHelper.GetLocalPath(), path);

            if (File.Exists(photoPath))
            {
                Log.Instance.WriteInfo("获取到大集中人口照片路径，开始上传人口照片：" + path);
                var img = CommandTools.ImgToBase64(photoPath);
                //根据配置是否使用默认照片来上传回执编号
                var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                    sfzh, "",
                    QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                    sfzh + ".png",
                    img, img, null, 1);
                if (zzzpinfo.IsSucceed)
                {
                    Log.Instance.WriteInfo("上传人口照片成功！");
                    //return true;
                }
                else
                {
                    Log.Instance.WriteInfo("上传人口照片失败：" + zzzpinfo.MessageInfo);
                    //msg = zzzpinfo.MessageInfo;
                    //return false;
                }
            }

        }

        /// <summary>
        /// 连大集中查询人口信息
        /// </summary>
        /// <param name="info">身份信息</param>
        /// <returns></returns>
        public bool CheckInformationDjz(IdCardInfo info)
        {
            #region  连大集中查询

            if (QJTConfig.QJTModel.IsCheckInformation)
            {
                //连接大集中 取大集中数据至预约数据
                Log.Instance.WriteInfo("========" + CommandTools.ReplaceWithSpecialChar(info.IDCardNo) + "开始核查人口信息========");
                ReturnInfo resultInfo = GetPopulationInfo(info);

                if (resultInfo?.IsSucceed == true && resultInfo?.ReturnValue != null && resultInfo?.ReturnValue is BasicInfo)
                {
                    var model = resultInfo?.ReturnValue as BasicInfo;
                    //保存人口库户口所在地信息
                    BookingBaseInfo.Address = new DictionaryType()
                    {
                        Code = model?.HkszdCode?.ToString(),
                        Description = model?.Hkszd?.ToString(),
                    };
                    //绑定到基本信息户口所在地输入框
                    BookingBaseInfo.HasAddress = BookingBaseInfo.Address.Description;
                    //保存人口库信息
                    info.FullName = model?.FullName?.ToString();
                    //航信和太极性别返回值不同 需先判断
                    if (model?.Gender?.ToString() != null)
                    {
                        info.Gender = model?.Gender == "男" ? "1" : "2";
                    }

                    info.pNational = model?.NationCode?.ToString();
                    //info.Gender = model?.Gender?.ToString();
                    info.Address = model?.Address?.ToString();
                    info.Birthday = model?.Birthday?.Replace("-", "");

                    BookingBaseInfo.CSDDM = model?.CsdCode;
                    BookingBaseInfo.CSDMC = model?.Csd;
                    Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(model?.CardId?.ToString()) + "查询全国人口库信息：" + CommandTools.ReplaceWithSpecialChar(model?.FullName?.ToString()) + "来自" + model?.Hkszd?.ToString() + "，户口所在地：" + model?.Hkszd?.ToString() + "性别：" + model?.Gender?.ToString() + "民族：" + info.pNational);

                    //保存航信人口照片
                    if (!string.IsNullOrEmpty(model?.ImgUrl))
                        SaveImageByHx(model?.ImgUrl, info?.IDCardNo);

                }
                else if (string.IsNullOrWhiteSpace(info?.FullName))
                {
                    Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(info?.IDCardNo) + "未核查到全国人口库姓名");
                    //msg = OwnerViewModel?.IsShanXi == true ?
                    //    "请前往人工拍照区域进行拍照" : TipMsgResource.IdentityExceptionTipMsg;
                    return false;
                }
                if (resultInfo != null && resultInfo.IsException)
                {
                    Log.Instance.WriteInfo("全国人口库服务器响应失败，请重试！");
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                    DevAlarm.AlarmInfo = resultInfo?.MessageInfo;//返回信息
                    ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());
                }
                Log.Instance.WriteInfo("========" + CommandTools.ReplaceWithSpecialChar(info.IDCardNo) + "结束核查人口信息========");

            }
            return true;
            #endregion
        }
        /// <summary>
        /// 启用太极接口
        /// </summary>
        /// <param name="info">身份信息</param>
        /// <param name="msg">提示信息</param>
        /// <returns>true|false</returns>
        private bool ConnectingTaiJi(IdCardInfo info, out string msg)
        {
            msg = "";
            try
            {
                //是否启用太极接口
                if (TjConfig.TjModel.IsConnectionTj)
                {
                    //清缓存
                    ClearCache();
                    //取时间
                    if (ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate() != null)
                    {
                        OwnerViewModel.BeginTime = DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());
                    }
                    if (OwnerViewModel?.DyLoginRec == null)
                    {
                        if (!DoDyLogin()) //登陆导服系统
                        {
                            msg = "导服登录失败，请重试！";
                            return false;
                        }
                    }

                    return DoDyCheck(info, out msg);//导服核查
                }
            }
            catch (Exception ex)
            {
                msg = "身份核验失败，请重试！";
                Log.Instance.WriteError("启用太极接口查询发生错误：" + ex.Message);
                Log.Instance.WriteInfo("启用太极接口查询发生错误：" + ex.Message);
            }

            return true;

        }

        #region  对接太极登录接口

        /// <summary>
        /// 导服登陆
        /// </summary>
        /// <returns></returns>
        public bool DoDyLogin()
        {
            bool isFlag = false;
            Log.Instance.WriteInfo("开始登陆【太极】导服接口");
            //太极接口登录
            Json_I_DY_login dyLogin = new Json_I_DY_login
            {
                loginName = TjConfig.TjModel.DYUserName,
                password = TjConfig.TjModel.DYUserPwd,
                jqbh = TjConfig.TjModel.TJMachineNo
            };

            //登录返回实体类
            Json_R_DY_login_Rec loginTaiji = new Json_R_DY_login_Rec();

            loginTaiji = new TaiJiHelper().DO_DY_Login(dyLogin);

            OwnerViewModel.DyLoginRec = loginTaiji;
            if (loginTaiji.IsNotEmpty() && loginTaiji.dwid.IsNotEmpty())
            {
                isFlag = true;

            }
            Log.Instance.WriteInfo("【太极】导服接口登录返回" + isFlag + "：单位编号：" + loginTaiji?.dwid + "，单位代码：" + loginTaiji?.dwdm);

            Log.Instance.WriteInfo("结束登录【太极】接口");
            return isFlag;

        }
        #endregion

        /// <summary>
        /// 导服核查
        /// </summary>
        /// <returns></returns>
        public bool DoDyCheck(IdCardInfo info, out string msg)
        {
            bool IsDyCheck = false;
            msg = string.Empty;
            string sfzh = string.Empty;
            string zwxm = string.Empty;
            try
            {

                //登录接口
                var logininfo = OwnerViewModel?.DyLoginRec;
                Log.Instance.WriteInfo("【太极】开始核查【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + "】信息");
                if (logininfo.IsEmpty())
                {
                    IsDyCheck = false;
                    msg = info.IDCardNo?.ToString() + "身份证号不正确，请重试！";
                    return IsDyCheck;
                }
                sfzh = info.IDCardNo;

                if (string.IsNullOrEmpty(logininfo?.dwid) || string.IsNullOrEmpty(logininfo?.yhid))
                {
                    IsDyCheck = false;
                    msg = "导服核验失败，请重试！";
                    return IsDyCheck;
                }

                Json_I_DY_check dymodel = new Json_I_DY_check
                {
                    dwid = logininfo?.dwid,
                    loginName = TjConfig.TjModel.DYUserName,
                    sfzh = info?.IDCardNo,
                    jqbh = TjConfig.TjModel.TJMachineNo
                };
                //Log.Instance.WriteInfo("开始核查...");
                //导服核查接口
                Json_R_DY_check_Rec dyCheck = new TaiJiHelper().Do_DY_Check(dymodel);

                if (dyCheck == null)
                {
                    Log.Instance.WriteInfo("【太极】6.2	导服核查接口查询结果返回空！！！");
                    msg = "系统没有查询到" + sfzh + " 的身份数据，无法进行取号！";

                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                    DevAlarm.AlarmInfo = "导服核查接口查询结果返回空：" + msg;//返回信息
                    //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());
                    return IsDyCheck;
                }

                Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(dyCheck?.rkxxs[0]?.sfzh?.ToString()) + "【太极】导服查询导引结果返回：" + CommandTools.ReplaceWithSpecialChar(dyCheck?.rkxxs[0]?.xm?.ToString()) + "来自" + dyCheck?.rkxxs[0]?.csdmc?.ToString() + "，户口所在地：" + dyCheck?.rkxxs[0]?.hkszdmc?.ToString() + "性别：" + info?.Gender?.ToString() + "，民族：" + dyCheck?.rkxxs[0]?.mzdm + "，生日：" + info.Birthday);

                //导服核查返回结果
                Log.Instance.WriteInfo(dyCheck?.zwx?.ToString() + dyCheck?.zwm?.ToString() + "" + dyCheck?.ywid + "人口信息：" + dyCheck?.rkxxs?.Length + "条，证件信息：" + dyCheck?.zjxxs?.Length + "，预约信息：" + dyCheck?.yyxxs?.Length + "条，可办理业务信息：" + dyCheck?.kbywxx?.Length + "条，在办业务信息：" + dyCheck?.zbywxxs?.Length + "条");


                if (dyCheck?.rkxxs == null || dyCheck?.rkxxs?.Length == 0)
                {
                    Log.Instance.WriteInfo("【太极】6.2	导服核查接口查询rkxxs结果返回空！！！");
                    msg = "系统没有核查到" + sfzh + " 的人口信息，无法进行取号！";
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                    DevAlarm.AlarmInfo = "导服核查接口查询rkxxs结果为空：" + msg;//返回信息
                    //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());
                    return IsDyCheck;
                }

                #region 核查是否符合办理条件

                Log.Instance.WriteInfo("2.1、【太极】核查是否符合办理条件：");
                BookingBaseInfo.Address = new DictionaryType()
                {
                    Code = dyCheck?.rkxxs[0]?.hkszddm?.ToString(),
                    Description = dyCheck?.rkxxs[0]?.hkszdmc?.ToString(),
                };
                Log.Instance.WriteInfo("是否北京户籍：" + OwnerViewModel?.IsBeijingRegister);
                //核查人口信息
                if (dyCheck?.rkxxs?.Length > 0)
                {
                    zwxm = dyCheck.rkxxs[0].xm?.ToString();
                    if (zwxm.IsNotEmpty())
                        info.FullName = zwxm;

                    if (string.IsNullOrWhiteSpace(info.FullName) && OwnerViewModel?.IsBeijing == false)
                    {
                        Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(info?.IDCardNo) + "您的人口库信息姓名不全，不能操作取号，若有疑问，请咨询工作人员！");
                        msg = OwnerViewModel?.IsShanXi == true ?
                            "请前往人工拍照区域进行拍照" : TipMsgResource.IdentityExceptionTipMsg;
                        return IsDyCheck;
                    }

                    if (dyCheck?.rkxxs[0]?.mzdm?.ToString()?.Contains("04") == true ||
                        dyCheck?.rkxxs[0]?.mzdm?.ToString()?.Contains("05") == true)
                    {
                        msg = zwxm + "您不符合自助办理条件，请咨询工作人员！";
                        Log.Instance.WriteInfo("核查到民族为【维吾尔族】或【藏族】！");
                        //return IsDyCheck;
                        OwnerViewModel.IsOfficial += "维藏人员";
                        OwnerViewModel.SFKZDX = "1";
                    }

                    //是否是国家工作人员
                    if (QJTConfig.QJTModel.IsOfficial && dyCheck?.sfgjgzry != null)
                    {
                        //_isOfficial = true;
                        //info.IsOfficial = true;
                        if (dyCheck?.sfgjgzry == "1")
                        {
                            _isOfficial = true;
                            info.IsOfficial = true;
                            //国家工作人员获取人事主管单位 后面区域派号传
                            if (dyCheck.gjgzryxxs.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                                    OwnerViewModel.IsOfficial += "、国家工作人员";
                                else
                                    OwnerViewModel.IsOfficial += "国家工作人员";

                                info.RSZGDW = dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0]?.bsdwmc;
                                _RSZGDW = dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0]?.bsdwmc;
                                Log.Instance.WriteInfo("国家工作人员返回：【是】，人事主管单位：" + dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0]?.bsdwmc + "");
                            }
                            else
                            {
                                Log.Instance.WriteInfo("国家工作人员返回：【否】");
                            }
                            //msg = OwnerViewModel?.IsShanXi == true ?
                            //    "请前往人工拍照区域进行拍照" : dyCheck?.gjgzryxxs[0]?.bsdwmc + TipMsgResource.GJGWRYTipMsg;
                        }

                    }
                    else
                    {
                        _isOfficial = false;
                        info.IsOfficial = false;
                        Log.Instance.WriteInfo("国家工作人员返回：【否】");
                    }

                    //是否是控制对象
                    if (QJTConfig.QJTModel.IsCheckSpecil && dyCheck?.sfkzdx == "1")
                    {
                        info.IsCheckSpecil = true;
                        msg = OwnerViewModel?.IsShanXi == true ?
                            "请前往人工拍照区域进行拍照" : TipMsgResource.SKDXTipMsg;
                        //return IsDyCheck;
                        if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                            OwnerViewModel.IsOfficial += "、受控对象";
                        else
                            OwnerViewModel.IsOfficial += "受控对象";
                        OwnerViewModel.SFKZDX = "1";
                        Log.Instance.WriteInfo("查询控制对象返回：【是】");
                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询控制对象返回：【否】");
                    }

                    //是否是新疆重点人员
                    if (dyCheck?.sfxjzdry == "1")
                    {
                        _issfxjzdry = true;
                        msg = zwxm + "您不符合自助办理条件，请咨询工作人员！";
                        //Log.Instance.WriteInfo("======核查到为新疆重点人员======");
                        //return IsDyCheck;
                        if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                            OwnerViewModel.IsOfficial += "、新疆重点人员";
                        else
                            OwnerViewModel.IsOfficial += "新疆重点人员";

                        Log.Instance.WriteInfo("查询新疆重点人员返回：【是】");
                        OwnerViewModel.SFKZDX = "1";
                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询新疆重点人员返回：【否】");
                    }

                    // //是否是重点地区人员
                    if (dyCheck?.sfzddq == "1")
                    {
                        _issfzddq = true;
                        msg = zwxm + "您不符合自助办理条件，请咨询工作人员！";
                        //Log.Instance.WriteInfo("======核查到为重点地区人员======");
                        //return IsDyCheck;
                        if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                            OwnerViewModel.IsOfficial += "、重点地区人员";
                        else
                            OwnerViewModel.IsOfficial += "重点地区人员";
                        Log.Instance.WriteInfo("查询重点人员返回：【是】");
                        OwnerViewModel.SFKZDX = "1";
                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询重点人员返回：【否】");
                    }

                    //是否是现役军人
                    if (dyCheck?.rkxxs[0]?.byqk == "服现役")
                    {
                        msg = zwxm + "您不符合自助办理条件，请前往人工咨询台咨询！";
                        //Log.Instance.WriteInfo("======核查到为服现役人员======");
                        Log.Instance.WriteInfo("查询服现役返回：【是】");
                        OwnerViewModel.SFKZDX = "1";
                        return false;
                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询服现役返回：【否】");
                    }
                    _isCheckSpecil = false;

                    #endregion
                }

                IsDyCheck = true;

                #region 核查是否有在办业务及不可办理业务

                Log.Instance.WriteInfo("2.2、【太极】核查是否有在办业务及不可办理业务");
                //核查和有预约的ywid是否一致
                //1.有预约 有ywid 取之前ywid 不上传
                //2.无预约 无ywid 赋值新ywid 上传
                if (dyCheck?.zbywxxs != null && dyCheck?.zbywxxs?.Length > 0)
                {
                    //存在在办业务 报警上传
                    OwnerViewModel.zbywxx = dyCheck?.zbywxxs;

                    if (QJTConfig.QJTModel.IsEnableWebDBServer)
                    {
                        //Log.Instance.WriteInfo("在办业务：" + dyCheck?.zbywxxs);
                        DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                        DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000002);
                        DevAlarm.AlarmInfo = info.FullName + "核查到有正在办理且未完结的业务，身份证：" + info.IDCardNo + "，业务id：" + dyCheck?.ywid;
                        //ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000002).ToString());
                    }

                    if (dyCheck?.zbywxxs?.Length >= 3)
                    {
                        IsDyCheck = false;
                        Log.Instance.WriteInfo("在办业务总数：" + dyCheck?.zbywxxs?.Length);
                        msg = info?.FullName + "系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";
                        Log.Instance.WriteInfo(msg);
                        return IsDyCheck;
                    }
                }

                if (!string.IsNullOrEmpty(OwnerViewModel?.YWID) && OwnerViewModel?.YWID == dyCheck?.ywid)
                {
                    //Log.Instance.WriteInfo("======旧导引id：" + OwnerViewModel?.YWID + "，新导引id：" + dyCheck?.ywid + "======");
                    if (dyCheck?.zbywxxs != null)
                    {
                        if (dyCheck?.zbywxxs.Length > 0)
                        {
                            if (QJTConfig.QJTModel.IsEnableWebDBServer)
                            {
                                //OwnerViewModel.zbywxx = dyCheck?.zbywxxs;
                                //核查到有正在办理且未完结的业务，不能派号
                                Log.Instance.WriteInfo(msg);
                                DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                                DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000002);
                                DevAlarm.AlarmInfo = info.IDCardNo + "核查到有正在办理且未完结的业务，身份证：" + info.IDCardNo + "，业务id：" + dyCheck?.ywid;
                                ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000002).ToString());
                                msg = "核查到您有正在办理且未完结的业务，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";
                                return false;
                            }

                        }
                    }
                }
                else
                {
                    OwnerViewModel.YWID = dyCheck?.ywid;
                    //业务id放到未上传列表，上传后删除
                    OwnerViewModel.UnOverYwList.Add(dyCheck.ywid);
                }

                if (dyCheck?.zjxxs.Length > 0)
                {
                    OwnerViewModel.PaperWork = dyCheck?.zjxxs;

                    var OnWork = new List<ZjxxInfo>();
                    var UnWork = new List<ZjxxInfo>();
                    foreach (var item in dyCheck?.zjxxs)
                    {
                        if (item?.zjzt?.Contains("1") == true)
                        {
                            OnWork.Add(item);
                        }
                        else
                        {
                            UnWork.Add(item);
                        }
                    }
                    //var OnWork=dyCheck?.zjxxs?.FirstOrDefault()?.


                    Log.Instance.WriteInfo("核查有效证件条数为：" + OnWork?.Count + "条，失效证件条数为：" + UnWork?.Count + "条");
                }

                //核查太极返回可办业务类型
                //Log.Instance.WriteInfo("========" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo) + "开始核查可办业务========");
                if (dyCheck != null && dyCheck?.kbywxx != null && dyCheck?.kbywxx?.Length > 0)
                {
                    var kbywList = new List<KbywInfo>();
                    string strmsg = "";
                    foreach (var item in dyCheck?.kbywxx)
                    {
                        if (!kbywList?.Contains(item) == true)
                        {
                            kbywList.Add(item);
                            strmsg += item.bzlb + ",";
                        }
                    }

                    Log.Instance.WriteInfo("" + CommandTools.ReplaceWithSpecialChar(sfzh) + CommandTools.ReplaceWithSpecialChar(zwxm) + "核查可办业务为：" + dyCheck?.kbywxx?.Length + "条，可办业务为：" + strmsg + "");
                    OwnerViewModel.KbywInfos = dyCheck.kbywxx;
                    //异地办证人员类型
                    var ydsf = dyCheck?.kbywxx?.Where(t => !string.IsNullOrEmpty(t?.ydrysf))?.ToList();
                    if (ydsf?.Count > 0)
                    {
                        OwnerViewModel.ydrysf = ydsf?.FirstOrDefault()?.ydrysf;
                        if (string.IsNullOrEmpty(OwnerViewModel?.ydrysf))
                        {
                            OwnerViewModel.ydrysf = "15";
                        }
                    }
                }
                else
                {
                    Log.Instance.WriteInfo("========" + sfzh + zwxm + "核查可办业务为空========");
                    msg = info.FullName + " 系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";
                    IsDyCheck = false;
                    DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                    DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000002);
                    DevAlarm.AlarmInfo = msg + " 业务id：" + dyCheck?.ywid;
                    ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000002).ToString());
                    return IsDyCheck;
                }

                #endregion

                IsDyCheck = true;

                #region 处理预约信息
                //是否有预约信息
                Log.Instance.WriteInfo("2.3、【太极】核查外网预约信息");
                if (dyCheck?.yyxxs?.Length > 0 && dyCheck?.yyxxs?.FirstOrDefault()?.yyblyw != null)
                {
                    Log.Instance.WriteInfo(zwxm + "查询到预约信息，办理业务为：" + dyCheck?.yyxxs[0]?.yyblyw?.FirstOrDefault()?.bzlb);
                    BookingBaseInfo.BookingSource = 0;
                    BookingBaseInfo.Book_Type = "0";
                    //已预约的时间段信息
                    //太极无预约时间字段 有预约 北京地区默认当天办理
                    BookingBaseInfo.BookingTarget = new BookingTargetModel
                    {
                        BookingDate = DateTime.Now.ToString("yyyyMMdd"),
                        StartTime = "08:00",
                        EndTime = "18:00"
                    };
                    Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(zwxm) + "预约信息时间：" + BookingBaseInfo?.BookingTarget?.BookingDate + "-" + BookingBaseInfo?.BookingTarget?.StartTime + "-" + BookingBaseInfo?.BookingTarget?.EndTime);
                    //预约电话和紧急联系人 、职业
                    BookingBaseInfo.UrgentName = dyCheck?.yyxxs[0]?.jjlxr;
                    BookingBaseInfo.Telephone = dyCheck?.yyxxs[0]?.brlxdh;
                    BookingBaseInfo.UrgentTelephone = dyCheck?.yyxxs[0]?.jjlxrdh;

                    BookingBaseInfo.QWD = dyCheck?.yyxxs[0]?.hzqwd;
                    BookingBaseInfo.CJSY = dyCheck?.yyxxs[0]?.hzcjsy;
                    //网上预约信息
                    BookingBaseInfo.IsExpress = dyCheck.yyxxs[0].sfxtkzd.ToInt();
                    BookingBaseInfo.RecipientName = dyCheck?.yyxxs[0]?.sjrxm?.ToString();
                    BookingBaseInfo.RecipientTelephone = dyCheck?.yyxxs[0]?.sjrdh?.ToString();
                    BookingBaseInfo.EMSCode = dyCheck?.yyxxs[0]?.sjryzbm?.ToString();
                    BookingBaseInfo.EMSAddress = dyCheck?.yyxxs[0]?.sjrdz?.ToString();
                    OwnerViewModel.Xxly = dyCheck?.yyxxs[0]?.xxly?.ToString();
                    
                    //BookingBaseInfo.HasJob = result?.ReturnValue[0]?.ZY;
                    _dictionaryType = GetDictionaryTypes();
                    var job = _dictionaryType?.FirstOrDefault(t => t?.Code == dyCheck?.yyxxs[0]?.zy);
                    BookingBaseInfo.HasJob = job?.Description;
                    BookingBaseInfo.Job = job;

                    Log.Instance.WriteInfo("紧急联系人：" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentName)
                        + "，本人电话：" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.Telephone)
                        + ",紧急联系人电话：" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentTelephone)
                        + "，职业：" + CommandTools.ReplaceWithSpecialChar(BookingBaseInfo?.HasJob));

                    OwnerViewModel._hasYYXX = new List<string>();
                    string yystr = "";
                    //拼接已预约的业务类型
                    if (dyCheck?.yyxxs[0].yyblyw != null)
                    {
                        OwnerViewModel.YyywInfo = dyCheck?.yyxxs[0]?.yyblyw;
                        foreach (var item in dyCheck?.yyxxs[0]?.yyblyw)
                        {
                            //预约业务名称拼接
                            yystr += dyCheck?.yyxxs?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(item?.sqlb)))?.ToList()[0]?.ToString() + "、";
                            //预约的业务
                            var yyxx = dyCheck?.yyxxs?.Select(t => item?.sqlb)?.ToList()[0]?.ToString();
                            OwnerViewModel._hasYYXX.Add(yyxx);
                        }

                        BookingBaseInfo.StrCardTypes = yystr?.Substring(0, yystr.Length);

                        if (!string.IsNullOrEmpty(BookingBaseInfo.StrCardTypes))
                        {
                            Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(zwxm) + "保存预约类型：" + BookingBaseInfo?.StrCardTypes);
                        }
                        else
                        {
                            Log.Instance.WriteInfo("查询到预约类型为空！！！");
                        }
                    }
                }
                else
                {
                    Log.Instance.WriteInfo(zwxm + "未查询到今天的预约信息！");
                    //BookingBaseInfo.BookingSource = 1;
                    //BookingBaseInfo.Book_Type = "1";
                }

                if (dyCheck?.yyxxs == null)
                {
                    BookingBaseInfo.BookingSource = 1;
                    BookingBaseInfo.Book_Type = "1";
                }
                #endregion

                #region 太极  外地有预约且预约在办的 提示
                if (TjConfig.TjModel.IsConnectionTj)
                {
                    Log.Instance.WriteInfo("2.4【太极】判断和过滤在办业务");

                    if (!OwnerViewModel.IsBeijingRegister && BookingBaseInfo.BookingSource == 0 && OwnerViewModel?._hasYYXX?.Count > 0 && OwnerViewModel?.zbywxx != null && OwnerViewModel?.zbywxx?.Length > 0)
                    {
                        string strTypesDes = "";
                        List<string> zbyw = new List<string>(OwnerViewModel?.zbywxx);
                        string zbywStr = "";
                        foreach (var zbyy in zbyw)
                        {
                            zbywStr += zbyy + "、";
                        }
                        Log.Instance.WriteInfo(" 【太极】已在办业的业务申请类型：" + zbywStr + "");

                        if (zbyw != null && zbyw.Count > 0)
                        {
                            _dictionaryType = new List<DictionaryType>();
                            //所有业务  
                            var carTypes = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString())?.OrderBy(t => t.Code)?.ToList();
                            //预约业务列表 转字典
                            foreach (var item in OwnerViewModel?._hasYYXX)
                            {
                                _dictionaryType?.Add(carTypes?.Where(t => t?.Code == item)?.ToList()?.FirstOrDefault());
                            }
                            //外地的可办业务为预约业务
                            //在办业务列表 转字典
                            List<DictionaryType> copeTypes = new List<DictionaryType>();
                            foreach (var item in zbyw)
                            {
                                //预约业务==在办业务
                                var yyyw = _dictionaryType?.Where(t => t?.Code == item)?.ToList()?.FirstOrDefault();
                                if (yyyw != null)
                                {
                                    copeTypes?.Add(yyyw);
                                }
                            }

                            if (copeTypes != null && copeTypes?.Count > 0 && _dictionaryType?.Count == copeTypes?.Count)
                            {
                                strTypesDes = string.Join("、", copeTypes?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t?.Code))));

                                if (string.IsNullOrWhiteSpace(strTypesDes))
                                {
                                    msg = info.FullName + "系统查询到您预约的业务正在办理，请您再次核对，若有疑问请咨询现场工作人员！";
                                    return false;
                                }
                                else
                                {
                                    msg = info?.FullName + "系统查询到您预约业务【" + strTypesDes + "】与正在办理【" + strTypesDes?.Substring(0, strTypesDes.Length) + "】业务冲突，不能重复办理，请您再次核对，若有疑问请咨询现场工作人员！";
                                    return false;
                                }

                            }

                        }
                    }
                    else
                    {
                        Log.Instance.WriteInfo("【太极】没有可过滤业务");
                    }
                }

                #endregion

                IsDyCheck = true;

                #region 保存太极接口返回的人口信息

                //Log.Instance.WriteInfo("\n=======4、保存太极接口返回的人口信息=========");

                if (QJTConfig.QJTModel.IsCheckInformation)
                {
                    //绑定到基本信息户口所在地输入框
                    BookingBaseInfo.HasAddress = BookingBaseInfo.Address?.Description;

                    //保存人口库信息
                    info.FullName = dyCheck?.rkxxs[0]?.xm?.ToString();
                    //航信和太极性别返回值不同 需先判断
                    if (dyCheck?.rkxxs[0]?.xb?.ToString() != null)
                    {
                        info.Gender = dyCheck?.rkxxs[0]?.xb == "1" ? "1" : "2";
                    }

                    info.pNational = dyCheck?.rkxxs[0]?.mzdm?.ToString();
                    info.Address = dyCheck?.rkxxs[0]?.jtzz?.ToString();
                    info.Birthday = dyCheck?.rkxxs[0]?.csrq?.Replace("-", "");
                    if (string.IsNullOrEmpty(info.Birthday) && !string.IsNullOrEmpty(info?.IDCardNo))
                    {
                        if (info?.IDCardNo?.Length > 8)
                            info.Birthday = info.IDCardNo.Substring(6, 8);
                        Log.Instance.WriteInfo("人口信息返回出生日期为空，截取身份证号！！！");
                    }

                    BookingBaseInfo.CSDDM = dyCheck?.rkxxs[0]?.csddm;
                    BookingBaseInfo.CSDMC = dyCheck?.rkxxs[0]?.csdmc;


                    if (!string.IsNullOrWhiteSpace(dyCheck?.rkxxs[0]?.zp))
                        SaveImageByTaiji(dyCheck?.rkxxs[0]?.zp, dyCheck?.rkxxs[0]?.sfzh);

                    //Log.Instance.WriteInfo("========" + CommandTools.ReplaceWithSpecialChar(sfzh) + CommandTools.ReplaceWithSpecialChar(zwxm) + "结束核查人口信息========");
                }
                #endregion
            }
            catch (Exception ex)
            {
                msg = "核查身份发生异常：请重试！";
                Log.Instance.WriteError(info?.IDCardNo + "调用太极导服核查发生异常：" + ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("太极人口信息保存正常，赋值到本地对象成功。");
                Log.Instance.WriteInfo("【太极】结束核查【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + "】信息");
            }
            return IsDyCheck;
        }


        /// <summary>
        /// 是否有在办业务
        /// </summary>
        /// <param name="info"></param>
        public void HaveZBYW(IdCardInfo info)
        {
            //北京地区判断是否有在办业务
            if (TjConfig.TjModel.IsConnectionTj)
            {
                if (OwnerViewModel?.zbywxx != null)
                {
                    sqTypes = GetSQTypes();
                    string strTypesDes = "";
                    List<string> zbyw = new List<string>(OwnerViewModel?.zbywxx);
                    List<DictionaryType> dicTypes = new List<DictionaryType>();
                    foreach (var item in zbyw)
                    {
                        var yyyw = sqTypes.Where(t => t.Code == item).ToList();
                        if (yyyw.Count > 0)
                        {
                            dicTypes.Add(yyyw.First());
                        }
                    }
                    if (dicTypes.Count > 0)
                    {
                        strTypesDes = string.Join("、",
                            dicTypes.Select(t =>
                                EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.Code))));

                    }

                }
            }
        }

        /// <summary>
        /// 判断年龄是否能取号
        /// </summary>
        /// <param name="info"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Get_NomalAge(IdCardInfo info, out string msg)
        {
            msg = "";
            var strMsg = "";
            //Log.Instance.WriteInfo("\n==========3、判断年龄是否可以取号==========");
            if (!string.IsNullOrEmpty(info?.IDCardNo) && info?.IDCardNo?.Length > 14)
            {
                //取系统时间
                var mistime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                if (string.IsNullOrEmpty(mistime))
                {
                    Log.Instance.WriteInfo("获取系统时间返回为空");
                    mistime = DateTime.Now.ToString();//返回空取本机时间
                }
                //时间转换
                DateTime now = DateTime.Parse(mistime);
                if (!string.IsNullOrEmpty(now.ToString()))
                {
                    //Log.Instance.WriteInfo("当前时间：" + now.ToString("yyyy年MM月dd日 HH:mm:ss"));
                    DateTime birth;
                    DateTime.TryParse(
                        info?.IDCardNo?.Substring(6, 4) +
                        "-" + info?.IDCardNo?.Substring(10, 2) +
                        "-" +
                        info.IDCardNo?.Substring(12, 2), out birth);
                    int age = now.Year - birth.Year; //年龄
                    if (now.Month < birth.Month || (now.Month == birth.Month && now.Day < birth.Day))
                        age--;
                    Log.Instance.WriteInfo("计算出年龄为：" + age);
                    if (age <= 16)
                    {
                        if (OwnerViewModel?.IsBeijing == true)//北京0为人工
                        {
                            strMsg = "16周岁以下未成年申请人，请前往人工窗口办理！区域参数isVALIDXCYY为0";
                            Log.Instance.WriteInfo(strMsg);

                            if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                                OwnerViewModel.IsOfficial += "、小于16周岁";
                            else
                                OwnerViewModel.IsOfficial += "小于16周岁";

                            OwnerViewModel.isVALIDXCYY = "0";
                        }
                        else
                        {
                            OwnerViewModel.isVALIDXCYY = "1";//非北京1为人工
                            strMsg = "16周岁以下未成年申请人，请前往人工拍照区域进行拍照！";
                            Log.Instance.WriteInfo(strMsg + "人工号判断为True");
                            OwnerViewModel.IsManual = true;
                            if (OwnerViewModel.IsShanXi)
                                msg = strMsg;
                        }

                    }

                    if (age > 40 && OwnerViewModel?.IsBeijing == true)
                    {
                        strMsg = "40周岁以上，请前往人工窗口办理！区域参数isVALIDXCYY为0";
                        Log.Instance.WriteInfo(strMsg);

                        if (!string.IsNullOrEmpty(OwnerViewModel.IsOfficial))
                            OwnerViewModel.IsOfficial += "、大于40岁";
                        else
                            OwnerViewModel.IsOfficial += "大于40岁";
                        OwnerViewModel.isVALIDXCYY = "0";
                    }

                    if (age > 65)
                    {
                        if (OwnerViewModel?.IsShanXi == true)
                        {
                            OwnerViewModel.isVALIDXCYY = "1";
                            strMsg = "65周岁以上长者，请前往人工拍照区域进行拍照！";
                            Log.Instance.WriteInfo(strMsg + "人工号判断为True");
                            OwnerViewModel.IsManual = true;
                            if (OwnerViewModel?.IsShanXi == true)
                                msg = strMsg;
                            //return false;
                        }

                    }

                    if (age > 70)
                    {
                        if (OwnerViewModel?.IsWuHan == true)
                        {
                            OwnerViewModel.isVALIDXCYY = "1";
                            strMsg = "70周岁以上长者，请前往人工拍照区域进行拍照！";
                            Log.Instance.WriteInfo(strMsg + "人工号判断为True");
                            OwnerViewModel.IsManual = true;
                            //return false;
                        }

                    }
                }

            }

            return true;
        }

        /// <summary>
        /// 查询全国外网预约数据
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public void CheckYyxx(IdCardInfo info)
        {
            Log.Instance.WriteInfo("============开始查询全国预约信息===============");
            //var yylist = crjManager.GetSDWsyy(info.IDCardNo, DateTime.Now.ToString("yyyyMMdd"), "08:00-18:00",
            //    QJTConfig.QJTModel.QJTDevInfo.TBDWBH, QJTConfig.QJTModel.QJTDevInfo);
            var yylist = crjManager.GetSDWsyy(info.IDCardNo, DateTime.Now.ToString("yyyyMMdd"), "",
                QJTConfig.QJTModel.QJTDevInfo.TBDWBH, QJTConfig.QJTModel.QJTDevInfo);
            Log.Instance.WriteDebug("====查询到全国预约数据，参数：====");
            Log.Instance.WriteDebug(JsonHelper.ToJson(yylist));
            if (yylist != null && yylist?.Count > 0)
            {
                Log.Instance.WriteInfo("查询到全国预约数据：" + yylist.Count + "条");
                wsYysqxx = yylist;
                BookingBaseInfo.Book_Type = "0";
                BookingBaseInfo.BookingSource = 0;
            }
            else
            {
                Log.Instance.WriteInfo(CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "全国库找不到您今日的有效预约数据！");
                BookingBaseInfo.Book_Type = "1";
                BookingBaseInfo.BookingSource = 1;
            }

        }

        /// <summary>
        /// 获取制证照片
        /// </summary>
        /// <param name="info">身份证信息</param>
        public void GetZzzpFunc(IdCardInfo info)
        {
            try
            {
                //Log.Instance.WriteInfo("======开始查询制证照片======");
                //是否存在照片回执单
                var ZZinfo = ZHPHMachineWSHelper.ZHPHInstance.S_ZZZP(info.IDCardNo);

                if (ZZinfo != null)
                {
                    Log.Instance.WriteInfo("获取制证照片返回【成功】");
                    if (ZZinfo.ZIPCONTEXT != null && !string.IsNullOrEmpty(ZZinfo.ZIPCONTEXT))
                    {
                        //获取服务器时间

                        var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                        //获取制证照片时间
                        var ZXDate = ZZinfo.ZXDATE?.ToString();
                        Log.Instance.WriteInfo("制证照片时间为：【" + ZXDate + "】，回执编号：【" + ZZinfo?.HZBH?.ToString() + "】");
                        if (!string.IsNullOrEmpty(ZXDate.ToString()))
                        {
                            //是否是今天的判断
                            if (DateTime.Parse(Now).ToString("yyyyMMdd") == DateTime.Parse(ZXDate).ToString("yyyyMMdd"))
                            {

                                //广东地区根据配置，给当天制证照片
                                if (!QJTConfig.QJTModel.IsCheckNowPic)
                                {
                                    Log.Instance.WriteInfo("查询到制证照片为当天，配置项不使用当天制证照片！");
                                    BookingBaseInfo.ReceiptsNo = "";
                                }
                                else
                                {
                                    Log.Instance.WriteInfo("查询到制证照片为当天，跳过扫描回执界面！");
                                    BookingBaseInfo.ReceiptsNo = ZZinfo.HZBH;

                                }

                            }
                            else if (QJTConfig.QJTModel.IsCheck_Pic)
                            {
                                Log.Instance.WriteInfo("查询到制证照片时间为：" + ZXDate?.ToString() + "");
                                BookingBaseInfo.ReceiptsNo = ZZinfo.HZBH;
                            }
                        }
                        else
                            Log.Instance.WriteInfo("核查到照相时间返回为空");
                    }
                    else
                        Log.Instance.WriteInfo("接口返回制证照片内容为空");
                }
                else
                {
                    Log.Instance.WriteInfo("查询制证照片接口返回：为空！");
                }

            }
            catch (Exception ex)
            {
                throw new Exception("获取制证照片出现异常：" + ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("\n======3、结束获取 【" + CommandTools.ReplaceWithSpecialChar(info?.IDCardNo?.ToString()) + CommandTools.ReplaceWithSpecialChar(info?.FullName?.ToString()) + "】的制证照片======");
            }
        }

        /// <summary>
        /// 赋值预约数据 条件在外面判断
        /// </summary>
        /// <param name="yYSQXX_TB"></param>
        public void SetYyxx(PH_YYSQXX_TB yYSQXX_TB)
        {
            //Log.Instance.WriteInfo("===开始赋值预约数据===");

            if (yYSQXX_TB.WSYYSJ.IsNotEmpty())
            {
                BookingBaseInfo.BookingTarget = new BookingTargetModel
                {
                    BookingDate = yYSQXX_TB.WSYYRQ,
                    StartTime = yYSQXX_TB.WSYYSJ.Split('-')[0],
                    EndTime = yYSQXX_TB.WSYYSJ.Split('-')[1],
                };
            }
            else
            {
                BookingBaseInfo.BookingTarget = new BookingTargetModel
                {
                    BookingDate = yYSQXX_TB.WSYYRQ,
                    StartTime = "08:00",
                    EndTime = "18:00",
                };
            }

            Log.Instance.WriteInfo("预约信息时间：" + BookingBaseInfo?.BookingTarget?.BookingDate + "-" + BookingBaseInfo?.BookingTarget?.StartTime + "-" + BookingBaseInfo?.BookingTarget?.EndTime);

            //预约电话和紧急联系人 、职业
            BookingBaseInfo.UrgentName = yYSQXX_TB?.JJQKLXR?.ToString();
            BookingBaseInfo.Telephone = yYSQXX_TB?.LXDH?.ToString();
            BookingBaseInfo.UrgentTelephone = yYSQXX_TB?.JJQKLXRDH?.ToString();
            var job = _dictionaryType?.FirstOrDefault(t => t.Code == yYSQXX_TB?.ZY?.ToString());
            BookingBaseInfo.HasJob = job?.Description?.ToString();
            BookingBaseInfo.Job = job;

            BookingBaseInfo.CJSY = yYSQXX_TB?.CJSY?.ToString();
            BookingBaseInfo.QWD = yYSQXX_TB?.QWD?.ToString();
            BookingBaseInfo.Remark = yYSQXX_TB?.REMARK?.ToString();
            //网上预约信息
            BookingBaseInfo.IsExpress = yYSQXX_TB.SFXTKZD.ToInt();
            BookingBaseInfo.RecipientName = yYSQXX_TB?.SJR?.ToString();
            BookingBaseInfo.RecipientTelephone = yYSQXX_TB?.SJRLXDH?.ToString();
            BookingBaseInfo.EMSCode = yYSQXX_TB?.YZBM?.ToString();
            BookingBaseInfo.EMSAddress = yYSQXX_TB?.EMSDZ?.ToString();

            BookingBaseInfo.Book_Type = yYSQXX_TB?.BOOK_TYPE?.ToString();

            //Log.Instance.WriteInfo("紧急联系人：" + 
            //                       PrintHelper.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentName, 1, 1) + "，本人电话：" + 
            //                       PrintHelper.ReplaceWithSpecialChar(BookingBaseInfo?.Telephone, 3, 3) + ",紧急联系人电话：" + 
            //                       PrintHelper.ReplaceWithSpecialChar(BookingBaseInfo?.UrgentTelephone, 3, 3) + "，职业：" + 
            //                       BookingBaseInfo?.HasJob);
            Log.Instance.WriteInfo("预约数据赋值到本地【成功】");
        }

        /// <summary>
        /// 直接取号
        /// </summary>
        /// <param name="yysqxx"></param>
        public void GetPhNo(PH_YYSQXX_TB[] yysqxx)
        {
            Log.Instance.WriteInfo("点击【直接取号】按钮，进入照片回执页面");
            BookingBaseInfo.IsGetPHNO = "1";
            if (OwnerViewModel.IsWuHan)
            {
                foreach (var item in yysqxx)
                {
                    //var type = item.BOOK_TYPE;
                    //var phSend = item?.PH_SENDLIST?.Where(t => t.SERVICE_CODE.Contains("B"))?.ToList();
                    if (item.BOOK_TYPE == "0")
                    {
                        OwnerViewModel.isVALIDXCYY = "1";
                        Log.Instance.WriteInfo("武汉查询到人工号标识为1");
                    }

                }

            }

            if (OwnerViewModel?.IsShenZhen == true)
            {
                Log.Instance.WriteInfo("深圳查询受理类型为：" + yysqxx?.FirstOrDefault()?.SLLX + ",预约方式为：" + yysqxx?.FirstOrDefault()?.BOOK_TYPE);

                OwnerViewModel.isVALIDXCYY =
                    yysqxx?.FirstOrDefault()?.SLLX != 1 && yysqxx?.FirstOrDefault()?.BOOK_TYPE == "0" ? "0" : "1";
            }

        }

        /// <summary>
        /// 保存外网预约数据
        /// </summary>
        /// <param name="yylist"></param>
        public void InsertYyxx(List<PH_YYSQXX_TB> yylist)
        {
            Log.Instance.WriteInfo("============直接取号开始保存全国预约信息===============");
            foreach (var item in yylist)
            {
                if (item.YWXM.IsEmpty())
                {
                    item.YWXM = item.YWX + item.YWM;
                }
            }
            var sqlist = ZHPHMachineWSHelper.ZHPHInstance.I_WsYYSQXX(yylist);
            //PH_YYSQXX_TBBLL yyqzxxBll = new PH_YYSQXX_TBBLL(QJTConfig.QJTModel.QJTDevInfo.DEV_ID);
            //var sqlist = yyqzxxBll.SaveYYSQXXInfo(yylist);
            if (sqlist != null && sqlist.Count > 0)
            {
                Log.Instance.WriteInfo("保存全国预约信息 " + yylist?.Count.ToString() + " 条到服务器成功！");
                BookingBaseInfo.BookingSource = 0;
                BookingBaseInfo.Book_Type = "0";
                //result = ZHPHMachineWSHelper.ZHPHInstance.S_YYSQXX(info?.IDCardNo, DateTime.Now);
            }

        }

        /// <summary>
        /// 重新预约
        /// </summary>
        public void RegestFunc()
        {
            Log.Instance.WriteInfo("点击【重新预约】按钮，进入选择预约时间页面");

            //修改 重新预约 不派人工号
            if (OwnerViewModel?.IsXuwen == true && !_isOfficial && !_isCheckSpecil)
            {
                OwnerViewModel.IsManual = false;
                OwnerViewModel.isVALIDXCYY = "0";
                Log.Instance.WriteInfo("重新预约，非国家工作人员，非控制对象，人工号标识为False");
            }
            //深圳重新预约为人工号
            if (OwnerViewModel?.IsShenZhen == true)
            {
                OwnerViewModel.IsManual = true;
                OwnerViewModel.isVALIDXCYY = "1";
                Log.Instance.WriteInfo("深圳地区重新预约，人工号标识为True");
            }

            BookingBaseInfo.Book_Type = "1";
            BookingBaseInfo.BookingSource = 1;
            BookingBaseInfo.IsExpress = 0;
            BookingBaseInfo.RecipientName = null;
            BookingBaseInfo.RecipientTelephone = null;
            BookingBaseInfo.EMSCode = null;
            BookingBaseInfo.EMSAddress = null;
            BookingBaseInfo.BookingTarget = null;

        }

        /// <summary>
        /// 作废重取过号操作
        /// </summary>
        /// <param name="yYSQXX_TB"></param>
        public void OverNo(PH_YYSQXX_TB yYSQXX_TB)
        {
            try
            {
                OwnerViewModel.IsShowHiddenLoadingWait("正在过号，请稍候...");
                if (yYSQXX_TB?.PH_SENDLIST?.FirstOrDefault()?.CALLUSER != null)
                {
                    //Log.Instance.WriteInfo("作废重取，开始过号操作...");
                    var tuple = ZHSLMachineWSHelper.Instance.GetPH_CallNo(yYSQXX_TB?.PH_SENDLIST[0]?.CALLUSER, EnumTypeCallType.PASSED.ToString());
                    if (tuple?.Item1?.IsSucceed == true && tuple.Item2.IsNotEmpty())
                    {
                        Log.Instance.WriteInfo("过号成功，业务：【" + tuple?.Item2.YWBH + "】，号码【" + tuple?.Item2.PH_NUMBER + "】，叫号状态：【" + tuple?.Item2.CLZT + "】");
                    }
                    else
                    {
                        Log.Instance.WriteInfo("过号失败：" + tuple?.Item1?.MessageInfo);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("过号操作发生异常：" + ex);
            }
            finally
            {
                OwnerViewModel.IsShowHiddenLoadingWait();
            }

        }

        /// <summary>
        /// 创建身份证信息写入文件     
        /// </summary>
        private void SaveSfzInfo()
        {
            try
            {
                string strFullName = string.Empty;
                string strGender = string.Empty;
                string strNation = string.Empty;
                string strBirthday = string.Empty;
                string strAddress = string.Empty;
                string strSfzh = string.Empty;
                string strQfjg = string.Empty;
                string strQfyxq = string.Empty;
                string strTemp = string.Empty;

                string wzPath = Path.Combine(FileHelper.GetLocalPath(), "EDCDMM") + @"\wz.txt";
                var dir = Path.GetDirectoryName(wzPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (File.Exists(wzPath))
                    File.Delete(wzPath);
                string zpPath = Path.Combine(FileHelper.GetLocalPath(), "EDCDMM") + @"\zp.bmp";
                if (File.Exists(zpPath))
                    File.Delete(zpPath);

                if (Basicinfo.IsNotEmpty())
                {
                    //Log.Instance.WriteInfo("写身份证信息：" + JsonHelper.ToJson(Basicinfo));
                    if (Basicinfo.FullName.IsNotEmpty())
                        strFullName = Basicinfo.FullName?.Trim();
                    if (Basicinfo.Gender.IsNotEmpty())
                        strGender = Basicinfo.Gender?.Trim();
                    if (Basicinfo.Nation.IsNotEmpty())
                        strNation = Basicinfo.Nation?.Trim();
                    if (Basicinfo.Birthday.IsNotEmpty())
                        strBirthday = Basicinfo.Birthday?.Replace("年", "")?.Replace("月", "")?.Replace("日", "")?.Replace("-", "")?.Trim()?.ToString();
                    if (Basicinfo.Address.IsNotEmpty())
                        strAddress = Basicinfo.Address?.Trim()?.ToString();
                    if (Basicinfo.CardId.IsNotEmpty())
                        strSfzh = Basicinfo.CardId?.Trim();
                    if (Basicinfo.PoliceStation.IsNotEmpty())
                        strQfjg = Basicinfo.PoliceStation?.Trim();
                    if (Basicinfo.CertificatesYXQ.IsNotEmpty() && (Basicinfo.CertificatesYXQ.Length == 17))
                    {
                        strQfyxq = Basicinfo.CertificatesYXQ?.Trim();
                    }

                    string imgPath = Basicinfo?.ImgUrl?.ToString();
                    if (imgPath.IsNotEmpty() && File.Exists(imgPath))
                    {
                        File.Copy(imgPath, zpPath);
                    }
                    FileStream fs = null;
                    if (!File.Exists(wzPath))
                    {
                        fs = new FileStream(wzPath, FileMode.Create, FileAccess.Write);//创建写入文件                
                    }
                    else
                    {
                        fs = new FileStream(wzPath, FileMode.Open, FileAccess.Write);
                    }
                    StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("GB2312"));

                    sw.WriteLine(strFullName);
                    sw.WriteLine(strGender);
                    sw.WriteLine(strNation);
                    sw.WriteLine(strBirthday);
                    sw.WriteLine(strAddress);
                    sw.WriteLine(strSfzh);
                    sw.WriteLine(strQfjg);
                    sw.WriteLine(strQfyxq);
                    sw.Close();
                    fs.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// 下一步
        /// </summary>
        /// <param name="obj"></param>
        public void DoNextFunction(object obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (obj == null || obj.IsEmpty())
                    OwnerViewModel.DoExitFunction(null);
                //OwnerViewModel.ReturnHome();
                if (OwnerViewModel != null)
                {
                    //关闭消息提示
                    OwnerViewModel.IsMessage = false;
                    //OwnerViewModel.Dispose();
                    if (obj != null) OwnerViewModel.DoNextFunction(obj.ToString());
                }
            });
        }

        public override void DoExitFunction(object obj)
        {
            ReadIDCardHelper.Instance.DoCloseIDCard();
            //isNext = true;
            //MainWindowViewModels.Instance.StopTimeOut();
            //MainWindowViewModels.Instance.ReturnHome();
            base.DoExitFunction(obj);
        }
        /// <summary>
        /// 清除上个人办证缓存
        /// </summary>
        public void ClearCache()
        {
            //清空上个人的证件信息
            OwnerViewModel.PaperWork = null;
            //清空上个人在办业务
            OwnerViewModel.zbywxx = null;
            OwnerViewModel.zbywList = new List<string>();
            //清空上个人国家工作人员信息
            OwnerViewModel.IsOfficial = "";
            //清空上个人预约业务
            OwnerViewModel._hasYYXX = new List<string>();
            //清空上个人照片
            OwnerViewModel.djzPhoto = "";
            //清空上个人办证类别
            //OwnerViewModel._HasBZLB = null;
            OwnerViewModel.ydrysf = null;
            //清空人工号标识
            OwnerViewModel.isVALIDXCYY = "0";
            OwnerViewModel.SFKZDX = "0";
            OwnerViewModel.Xxly = "";
            OwnerViewModel.TaijiPhMode = "";
            OwnerViewModel.IsDirectNumber = false;
            OwnerViewModel.RenGong = 0;
        }


    }
}
