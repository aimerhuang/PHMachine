using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Freedom.Models.DataBaseModels;
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

    public partial class JgReadIDCardViewModels : ViewModelBase
    {
        #region 构造方法
        public JgReadIDCardViewModels(Page page)
        {
            this.ContentPage = page;
            //初始化数据
            ServiceRegistry.Instance.Get<ElementManager>().Set<BookingModel>(new BookingModel());
            //（0：已预约 1：无预约）
            ModelType = ServiceRegistry.Instance.Get<ElementManager>().Get<string>("ModelType");
            //TypeMsg = ModelType == "0" ? "已预约" : "无预约";
            //取职业字典所有数据
            DictionaryType = GetDictionaryTypes();
            sqTypes = GetSQTypes();
            if (crjManager == null) { crjManager = new CrjPreapplyManager(); }
            TipMessage = "请将身份证贴近屏幕右下方感应区域进行操作";
        }
        #endregion

        #region 重写方法
        public override void DoInitFunction(object obj)
        {
            Log.Instance.WriteInfo("进入身份证识读界面");
            TTS.PlaySound("预约机-页面-刷身份证");
            //判断是否存在未上传业务，存在则取消之前业务id
            if (OwnerViewModel?.UnOverYwList != null && OwnerViewModel?.UnOverYwList.Count > 0)
            {
                Log.Instance.WriteInfo("核查到未上传导引业务id：" + OwnerViewModel?.UnOverYwList[0] + "条数：" + OwnerViewModel?.UnOverYwList.Count);
                foreach (var item in OwnerViewModel?.UnOverYwList)
                {
                    Json_I_DY_upload dyUpload = new Json_I_DY_upload
                    {
                        ywid = item,
                        dyzt = "1",
                        jzz = "1",
                        dyqy = QJTConfig.QJTModel.TaijiPHMode == "0" ? "0" : "1",
                        pdlb = QJTConfig.QJTModel.TaijiPHMode == "0" ? "104" : "101",
                        pdh = ""
                    };
                    var result = new TaiJiHelper().Do_DY_Upload(dyUpload);
                    Log.Instance.WriteInfo("取消导引业务id：" + item + "成功！");

                }

            }
            ReadIDCardThread = new Thread(new ThreadStart(ReadIDCard));
            ReadIDCardThread.IsBackground = true;
            ReadIDCardThread.Start();

            if (OwnerViewModel?.IsBeijing == true)
            {
                OwnerViewModel.HomeShow = Visibility.Collapsed;
            }
            else
            {
                //启用计时器
                this.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
            }
            base.DoInitFunction(obj);
        }

        protected override void OnDispose()
        {
            OwnerViewModel.HomeShow = Visibility.Visible;

            if (ReadIDCardThread != null)
            {
                ReadIDCardThread.Abort(0);
                ReadIDCardThread = null;
            }
            if (QueryReadIDCardThread != null)
            {
                QueryReadIDCardThread.Abort(0);
                QueryReadIDCardThread = null;
            }
            CommonHelper.OnDispose();
            TTS.StopSound();
            base.OnDispose();
        }

        #endregion

        #region 属性
        /// <summary>
        /// BLL层查全国对象
        /// </summary>
        private CrjPreapplyManager crjManager;
        /// <summary>
        /// 读取身份证线程
        /// </summary>
        private Thread ReadIDCardThread = null;

        private Thread QueryReadIDCardThread = null;
        string msg = string.Empty;

        private PH_ZZZP_TBBLL zzzp;
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

        private string iDCardNo;
        public bool bln = false;
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
                        NextStepOperaAsync(new IdCardInfo() { IDCardNo = str });
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
                            if (!DataVerification(IDCardNo, out string msg))
                            {
                                TipMsg = msg;
                                return;
                            }
                            NextStepOperaAsync(new IdCardInfo() { IDCardNo = iDCardNo });
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
                });
            }
        }
        #endregion

        #region 字段
        //业务类型 0已预约 1无预约
        private string ModelType;
        private string TypeMsg;
        private List<DictionaryType> DictionaryType;
        private List<DictionaryType> sqTypes;
        public static Dev_AlarmInfo DevAlarm = new Dev_AlarmInfo();//设备故障信息
        //public List<string> _hasYYXX = new List<string>();//预约业务类型拼接
        /// <summary>
        /// 是否新疆重点人员
        /// </summary>
        public bool _issfxjzdry = false;
        /// <summary>
        /// 是否重点地区人员
        /// </summary>
        public bool _issfzddq = false;
        #endregion

        #region 方法
        /// <summary>
        /// 读取身份证号码信息
        /// </summary>
        private void ReadIDCard()
        {
            try
            {
                Log.Instance.WriteInfo("===========开始读取身份证===========");
                if (!OwnerViewModel.CheckServeStatus())
                {
                    DoNextFunction("NotificationPage");
                }
                //Log.Instance.WriteInfo("选择业务类型：" + TypeMsg);
                //初始化身份证阅读器
                if (!ReadIDCardHelper.Instance.DoReadIDCardInit().IsSucceed)
                {
                    OwnerViewModel?.MessageTips(TipMsgResource.IDCardInitializationTipMsg, () =>
                    {
                        Log.Instance.WriteInfo("初始化身份证阅读器失败！");
                        this.DoExit.Execute(null);
                    });
                    return;
                }


                while (!bln)
                {
                    //读取身份证号码信息
                    ReturnInfo info = ReadIDCardHelper.Instance.DoReadIDCardInfo(out IdCardInfo model);
                    if (info.IsSucceed)
                    {
                        bln = true;
                        Log.Instance.WriteInfo("核对身份证信息：" + model?.FullName + "，" + model?.IDCardNo);
                        if (!Freedom.Config.QJTConfig.QJTModel.IsConnReadIDcardDev && Freedom.Config.QJTConfig.QJTModel.CJG_IsOpenCCMSCode)
                            model.IDCardNo = Freedom.Config.QJTConfig.QJTModel.CJG_CCMSCode;
                        Log.Instance.WriteInfo("===========结束读取身份证===========");
                        NextStepOperaAsync(model);
                        break;
                    }
                    //Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("初始化身份证阅读器出现异常：" + ex);
                Log.Instance.WriteInfo("初始化身份证阅读器出现异常：" + ex);
            }

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
        /// 获取身份证信息操作
        /// </summary>
        /// <param name="info">身份证信息</param>
        /// <param name="msg">提示消息/页面跳转地址</param>
        /// <returns></returns>
        private bool SaveIDCardNoAsync(IdCardInfo info, out string msg)
        {
            try
            {
                //关闭页面定时器
                this.thCountdown?.Abort();
                //修改设备受理中
                bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.ACCEPTTING);
                if (blnResult)
                {
                    Log.Instance.WriteInfo($"修改设备状态[{EnumType.GetEnumDescription(EnumTypeSTATUS.ACCEPTTING)}]成功");
                }

                //查询当前大厅当天此人预约信息
                //非陆河地区查询预约信息直接查询全国接口
                var result = ZHPHMachineWSHelper.ZHPHInstance.S_YYSQXX(info.IDCardNo, DateTime.Now);
                msg = "身份校验报错";

                //是否有预约信息
                if ((result != null && result.IsSucceed && (result.ReturnValue == null || result.ReturnValue?.Count() <= 0)))// || ModelType == "1")
                {
                    Log.Instance.WriteInfo(info?.IDCardNo + "今日未查询到预约信息!");
                    msg = "Booking/BookingTarget";
                    BookingBaseInfo.BookingSource = 1;

                    //陆河地区查找预约信息
                    if (OwnerViewModel.IsLuHe && QJTConfig.QJTModel.IsCheckYYXX_ZZZP)
                    {
                        Log.Instance.WriteInfo("广东地区开始查询全国预约信息......");
                        var yylist = crjManager.GetSDWsyy(info.IDCardNo, DateTime.Now.ToString("yyyyMMdd"), "08:00-18:00",
                            QJTConfig.QJTModel.QJTDevInfo.TBDWBH, QJTConfig.QJTModel.QJTDevInfo);

                        if (yylist != null && yylist.Count > 0)
                        {
                            PH_YYSQXX_TBBLL yyqzxxBll = new PH_YYSQXX_TBBLL(QJTConfig.QJTModel.QJTDevInfo.DEV_ID);
                            var sqlist = yyqzxxBll.SaveYYSQXXInfo(yylist);
                            if (sqlist.IsSucceed)
                            {
                                Log.Instance.WriteInfo(info?.IDCardNo + "保存全国预约信息到服务器成功！");
                                BookingBaseInfo.BookingSource = 0;
                            }
                            else
                            {
                                Log.Instance.WriteInfo(info?.IDCardNo + "保存全国预约信息到服务器失败！");
                            }
                        }
                        else
                        {
                            Log.Instance.WriteInfo(info?.IDCardNo + "全国库找不到您今日的有效预约数据！");
                        }
                    }

                }
                else
                {
                    BookingBaseInfo.BookingSource = 0;
                    Log.Instance.WriteInfo("今日存在有效预约信息" + result?.ReturnValue?.Length + "条！");
                    //有预约 有业务id 太极能拿号
                    if (!string.IsNullOrEmpty(result?.ReturnValue[0].YWID))
                    {
                        OwnerViewModel.YWID = result?.ReturnValue[0].YWID;
                    }

                }

                if (!string.IsNullOrEmpty(info.Gender))
                {
                    //测试模式性别
                    var gender = info.Gender == "男" ? "1" : "2";
                    info.Gender = gender;
                }

                if (string.IsNullOrEmpty(info.pNational) && !QJTConfig.QJTModel.IsCheckInformation)
                {
                    info.pNational = "01";
                }

                //连接大集中并且打开核查人口信息
                if (DjzConfig.DjzModel.IsConnectionDjz)
                {
                    #region  连大集中查询

                    if (QJTConfig.QJTModel.IsCheckInformation)
                    {
                        //连接大集中 取大集中数据至预约数据
                        Log.Instance.WriteInfo("========" + info.IDCardNo + "开始核查人口信息========");
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

                            info.Gender = model?.Gender?.ToString();
                            info.Address = model?.Address?.ToString();
                            info.Birthday = model?.Birthday?.Replace("-", "");
                            Log.Instance.WriteInfo(model?.CardId?.ToString() + "查询全国人口库信息：" + model?.FullName?.ToString() + "来自" + model?.Hkszd?.ToString() + "，户口所在地：" + model?.Hkszd?.ToString() + "性别：" + info.Gender?.ToString());
                        }
                        else if (string.IsNullOrWhiteSpace(info.FullName))
                        {
                            Log.Instance.WriteInfo(info.IDCardNo + "未核查到全国人口库姓名");
                            msg = OwnerViewModel?.IsShanXi == true ?
                                "请前往人工拍照区域进行拍照" : TipMsgResource.IdentityExceptionTipMsg;
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
                        Log.Instance.WriteInfo("========" + info.IDCardNo + "结束核查人口信息========");
                        //return true;
                    }

                    #endregion

                    #region 核查国家工作人员和控制对象

                    //是否开启核查国家工作人员
                    if (QJTConfig.QJTModel.IsOfficial)
                    {

                        //判断是否是国家公务人员
                        _isOfficial = QueryOfficialAsync(info, out msg);
                        //北京区域不做判断，在后面区域选择中接口返回判断
                        if (_isOfficial)
                        {
                            //msg = OwnerViewModel?.IsShanXi == true ?
                            //    "请前往人工拍照区域进行拍照" : TipMsgResource.GJGWRYTipMsg;
                            //return false;
                            OwnerViewModel.IsManual = true;
                            Log.Instance.WriteInfo("核查到国家工作人员！");
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
                            msg = OwnerViewModel?.IsShanXi == true ?
                                "请前往人工拍照区域进行拍照" : TipMsgResource.SKDXTipMsg;
                            return false;
                        }

                    }
                    #endregion
                }

                #region 判断太极导服核查接口
                if (TjConfig.TjModel.IsConnectionTj)
                {
                    var taiJiResult = ConnectingTaiJi(info, out msg);
                    if (!taiJiResult)
                    {
                        return false;
                    }
                }
                #endregion

                #region 16周岁以下 或 65周岁以上

                bool ages = Get_NomalAge(info, out msg);
                if (!ages)
                    return false;

                //16周岁以下 65周岁以上不能办理
                //if (OwnerViewModel.IsShanXi && !string.IsNullOrEmpty(info.IDCardNo))
                //{
                //    DateTime now = DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());
                //    if (!string.IsNullOrEmpty(now.ToString()))
                //    {
                //        Log.Instance.WriteInfo("当前时间：" + now.ToString("yyyy年MM月dd日 HH:mm:ss"));
                //        DateTime birth;
                //        DateTime.TryParse(info.IDCardNo.Substring(6, 4) + "-" + info.IDCardNo.Substring(10, 2) + "-" +
                //                          info.IDCardNo.Substring(12, 2), out birth);
                //        int age = now.Year - birth.Year; //年龄
                //        if (now.Month < birth.Month || (now.Month == birth.Month && now.Day < birth.Day))
                //            age--;
                //        Log.Instance.WriteInfo("计算出年龄为：" + age);
                //        if (age < 16)
                //        {
                //            Log.Instance.WriteInfo("16周岁以下未成年申请人，请前往人工拍照区域进行拍照！");
                //            //OwnerViewModel.IsManual = true;
                //            msg = "16周岁以下未成年申请人，请前往人工拍照区域进行拍照！";
                //            return false;

                //            //Log.Instance.WriteInfo("16周岁以下和65周岁以上，请前往人工窗口办理！");
                //        }

                //        if (age > 65)
                //        {
                //            Log.Instance.WriteInfo("65周岁以上长者，请前往人工拍照区域进行拍照！");
                //            //OwnerViewModel.IsManual = true;
                //            msg = "65周岁以上长者，请前往人工拍照区域进行拍照！";
                //            return false;

                //        }
                //    }

                //}

                if (OwnerViewModel.IsShanXi && string.IsNullOrEmpty(info.Birthday))
                {
                    Log.Instance.WriteInfo("核查到出生日期为空，无法计算年龄！");
                }

                #endregion

                #region 判断是否存在当天制证照片

                Log.Instance.WriteInfo("=====开始查询是否存在制证照片=====");
                try
                {
                    //是否存在照片回执单
                    var ZZinfo = ZHPHMachineWSHelper.ZHPHInstance.S_ZZZP(info.IDCardNo);

                    if (ZZinfo != null)
                    {
                        Log.Instance.WriteInfo("获取制证照片返回成功");
                        if (ZZinfo.ZIPCONTEXT != null && !string.IsNullOrEmpty(ZZinfo.ZIPCONTEXT))
                        {
                            //获取服务器时间
                            var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                            //获取制证照片时间
                            var ZXDate = ZZinfo.ZXDATE?.ToString();
                            Log.Instance.WriteInfo("照相时间：" + ZXDate);
                            if (!string.IsNullOrEmpty(ZXDate.ToString()))
                            {
                                //是否是今天的判断
                                if (DateTime.Parse(Now).ToString("yyyyMMdd") == DateTime.Parse(ZXDate).ToString("yyyyMMdd"))
                                {
                                    Log.Instance.WriteInfo("查询到制证照片为当天，跳过扫描回执界面！");
                                    BookingBaseInfo.ReceiptsNo = ZZinfo.HZBH;
                                }
                                else if (QJTConfig.QJTModel.IsCheck_Pic)
                                {
                                    Log.Instance.WriteInfo("查询到制证照片时间为：" + ZXDate?.ToString());
                                    BookingBaseInfo.ReceiptsNo = ZZinfo.HZBH;
                                }
                            }
                            else
                                Log.Instance.WriteInfo("核查到照相时间返回为空");
                        }
                        else
                            Log.Instance.WriteInfo("核查到照片内容为空");
                    }
                    else
                    {
                        Log.Instance.WriteInfo("未查询到制证照片！");
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("获取制证照片出现异常：" + ex.Message);
                }
                Log.Instance.WriteInfo("=====结束查询制证照片=====");


                #endregion


                if (string.IsNullOrWhiteSpace(info.FullName))
                {
                    msg = OwnerViewModel?.IsShanXi == true ?
                        "请前往人工拍照区域进行拍照" : TipMsgResource.IdentityErrorTipMsg;
                    Log.Instance.WriteInfo(info.IDCardNo + "身份信息姓名为空！！！");
                    return false;
                }

                //北京地区查询区域分配
                if (OwnerViewModel?.IsBeijing == true)
                {
                    BookingBaseInfo.CardInfo = info;
                    bool isOnSite = result?.ReturnValue == null || result?.ReturnValue?.Count() <= 0;
                    ZHPHMachineWSHelper.ZHPHInstance.S_PlanPHSERVICETYPE(info.IDCardNo, OwnerViewModel.IsBeijingRegister,
                   isOnSite, _isOfficial, _RSZGDW, _isCheckSpecil);
                    //北京预约的办证类型 按预约的赋值


                }
                //陕西地区查询区域分配
                if (OwnerViewModel?.IsShanXi == true)
                {
                    var lst = ZHPHMachineWSHelper.ZHPHInstance.GetPHServiceType(QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(), "", QJTConfig.QJTModel.IS_NEED_SELF);
                    Log.Instance.WriteInfo("派号业务总条数：" + lst.Count);
                    Log.Instance.WriteInfo("获取派号业务成功：" + lst[0]?.SERVICE_CODE);
                }


                //当天有预约信息
                if (result?.ReturnValue != null && result.IsSucceed && result.ReturnValue?.Count() > 0)
                {
                    //点击已有预约办理
                    msg = "ScanningPhotoReceipt";
                    BookingBaseInfo.BookingSource = 0;
                    //已预约的时间段信息
                    BookingBaseInfo.BookingTarget = new BookingTargetModel
                    {
                        BookingDate = result.ReturnValue[0].WSYYRQ,
                        StartTime = result.ReturnValue[0].WSYYSJ.Split('-')[0],
                        EndTime = result.ReturnValue[0].WSYYSJ.Split('-')[1],
                    };
                    Log.Instance.WriteInfo("预约信息时间：" + BookingBaseInfo?.BookingTarget?.BookingDate + "-" + BookingBaseInfo?.BookingTarget?.StartTime + "-" + BookingBaseInfo?.BookingTarget?.EndTime);
                    //户口所在地信息
                    //BookingBaseInfo.Address = new DictionaryType
                    //{
                    //Code = result.ReturnValue[0]?.HKSZD,
                    //Description = result.ReturnValue[0]?.JTZZ,
                    //};
                    //如果手动输入身份证号码
                    if (string.IsNullOrWhiteSpace(info.FullName))
                    {
                        info.FullName = result?.ReturnValue[0]?.ZWXM;
                    }
                    //预约电话和紧急联系人 、职业
                    BookingBaseInfo.UrgentName = result?.ReturnValue[0]?.JJQKLXR;
                    BookingBaseInfo.Telephone = result?.ReturnValue[0]?.LXDH;
                    BookingBaseInfo.UrgentTelephone = result?.ReturnValue[0]?.JJQKLXRDH;
                    //BookingBaseInfo.HasJob = result?.ReturnValue[0]?.ZY;
                    var job = DictionaryType.FirstOrDefault(t => t.Code == result?.ReturnValue[0]?.ZY);
                    BookingBaseInfo.HasJob = job?.Description;
                    BookingBaseInfo.Job = job;

                    Log.Instance.WriteInfo("紧急联系人：" + BookingBaseInfo?.UrgentName + "，本人电话：" + BookingBaseInfo?.Telephone + ",紧急联系人电话：" + BookingBaseInfo?.UrgentTelephone + "，职业：" + BookingBaseInfo?.HasJob);
                    //拼接已预约的业务类型
                    var strlst = result.ReturnValue.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.SQLB)));
                    //预约的业务
                    OwnerViewModel._hasYYXX = result.ReturnValue.Select(t => t.SQLB).ToList();

                    #region 太极  外地有预约且预约在办的 提示
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        Log.Instance.WriteInfo("申请类型为：" + OwnerViewModel._hasYYXX.Count + "----");
                        if (!OwnerViewModel.IsBeijingRegister && BookingBaseInfo.BookingSource == 0 && OwnerViewModel._hasYYXX.Count > 0 && OwnerViewModel?.zbywxx != null && OwnerViewModel?.zbywxx.Length > 0)
                        {
                            string strTypesDes = "";
                            List<string> zbyw = new List<string>(OwnerViewModel?.zbywxx);
                            if (zbyw != null && zbyw.Count > 0)
                            {
                                DictionaryType = new List<DictionaryType>();
                                //所有业务  
                                var carTypes = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
                                //预约业务列表 转字典
                                foreach (var item in OwnerViewModel._hasYYXX)
                                {
                                    DictionaryType.Add(carTypes.Where(t => t.Code == item).ToList()[0]);
                                }
                                //外地的可办业务为预约业务
                                //在办业务列表 转字典
                                List<DictionaryType> copeTypes = new List<DictionaryType>();
                                foreach (var item in zbyw)
                                {
                                    //预约业务==在办业务
                                    var yyyw = DictionaryType.Where(t => t.Code == item).ToList().FirstOrDefault();
                                    if (yyyw != null)
                                    {
                                        copeTypes.Add(yyyw);
                                    }
                                }
                                strTypesDes = string.Join("、", copeTypes.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.Code))));
                                msg = info.FullName + "系统查询到您预约业务【" + strTypesDes + "】与正在办理【" + strTypesDes.Substring(0, strTypesDes.Length) + "】业务冲突，不能重复办理，请您再次核对，若有疑问请咨询现场工作人员！";
                                return false;

                            }
                        }

                        //北京办证类别赋值有预约数据
                        if (result?.ReturnValue?.Length > 0)
                        {
                            OwnerViewModel._HasBZLB = new Dictionary<int, string>();
                            foreach (var bzlb in result.ReturnValue)
                            {
                                OwnerViewModel._HasBZLB.Add(bzlb.SQLB.ToInt(), bzlb.BZLB);
                            }
                        }
                    }

                    #endregion

                    if (OwnerViewModel._hasYYXX.Count > 0)
                    {
                        sqTypes = new List<DictionaryType>();
                        var carType = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
                        //预约业务列表
                        foreach (var item in OwnerViewModel._hasYYXX)
                        {
                            sqTypes.Add(carType.Where(t => t.Code == item).ToList()[0]);
                        }

                        BookingBaseInfo.SelectCardTypes = sqTypes;

                    }

                    BookingBaseInfo.StrCardTypes = string.Join("、", strlst);
                    if (!string.IsNullOrEmpty(BookingBaseInfo.StrCardTypes))
                    {
                        Log.Instance.WriteInfo("保存预约类型：" + BookingBaseInfo?.StrCardTypes);
                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询到预约类型为空！！！");
                    }

                    //保存平台证件信息

                    OwnerViewModel.PaperInfos = new List<PaperInfo>();
                    foreach (var item in result.ReturnValue)
                    {
                        if (item.XCZJHM != null && item.XCZJYXQZ != null)
                        {
                            PaperInfo paper = new PaperInfo
                            {
                                csrq = item.CSRQ,
                                sfzh = item.SFZH,
                                xb = item.XB,
                                zjhm = item.XCZJHM,
                                zjyxqz = item.XCZJYXQZ,
                                zjzl = item.XCZJZL
                            };
                            OwnerViewModel.PaperInfos.Add(paper);
                        }
                    }
                    return true;
                }
                else
                {
                    Log.Instance.WriteInfo("未查询到预约信息，默认构造预约数据");
                    msg = "Booking/BookingTarget";
                    BookingBaseInfo.BookingSource = 1;
                    return true;
                }

            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo(ex.Message);
                Log.Instance.WriteError(ex.Message);
                msg = ex.Message;
            }
            return false;

        }

        /// <summary>
        /// 下一步操作
        /// </summary>
        /// <param name="info"></param>
        private void NextStepOperaAsync(IdCardInfo info)
        {
            OwnerViewModel?.IsShowHiddenLoadingWait(TipMsgResource.IDCardQuueryTipMsg);
            QueryReadIDCardThread = new Thread(delegate ()
              {
                  try
                  {
                      Log.Instance.WriteInfo("=====开始获取身份证信息操作界面=====");

                      if (!OwnerViewModel.CheckServeStatus())
                      {
                          DoNextFunction("NotificationPage");
                      }

                      if (SaveIDCardNoAsync(info, out string msg))
                      {
                          info.IsOfficial = _isOfficial;
                          if (QJTConfig.QJTModel.IsOfficial)
                          {
                              Log.Instance.WriteInfo("开启核验【国家公务人员】选项，核查结果：");
                          }
                          Log.Instance.WriteInfo(info.IsOfficial ? "核查身份【国家公务人员】" : "核查身份【非国家公务人员】");
                          info.IsCheckSpecil = _isCheckSpecil;
                          if (QJTConfig.QJTModel.IsCheckSpecil)
                          {
                              Log.Instance.WriteInfo("开启核验【控制对象】选项，核查结果：");
                          }
                          Log.Instance.WriteInfo(info.IsCheckSpecil ? "核查身份【是控制对象】" : "核查身份【非控制对象】");
                          BookingBaseInfo.CardInfo = info;
                          Hz2Py.GetPinYin(BookingBaseInfo.CardInfo);

                          //未查询到预约信息
                          if (BookingBaseInfo.BookingSource == 1)
                          {
                              Log.Instance.WriteInfo("北京无预约,开始判断是否有在办业务...");
                              //北京地区分配取号信息跳转页面
                              if (OwnerViewModel?.IsTakePH_No == true && OwnerViewModel?.IsBeijing == true)
                              {
                                  HaveZBYW(info);
                                  //北京户籍没有预约信息可直接进行办理
                                  if (OwnerViewModel?.IsBeijingRegister == true)
                                  {
                                      OwnerViewModel?.ActionToGo("", (() =>
                                      {
                                          this.DoNext.Execute("Booking/BookingTarget");
                                      }), null, "取号");

                                  }
                                  else//非北京户籍没有预约信息不能办理
                                  {

                                      TTS.PlaySound("预约机-提示-北京无预约");
                                      //Log.Instance.WriteInfo("非北京户籍没有预约返回失败信息");
                                      OwnerViewModel?.MessageTips("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                                      {
                                          this.DoExit.Execute(null);
                                      }));
                                  }
                              }
                              //非北京地区正常流程跳转现场预约页面
                              else if (OwnerViewModel?.IsTakePH_No == true && OwnerViewModel?.IsBeijing == false)
                              {
                                  Log.Instance.WriteInfo("无预约信息，非北京地区，直接取号模式跳转选择预约时间页面");
                                  this.DoNext.Execute(msg);
                              }
                              else
                              {
                                  Log.Instance.WriteInfo("北京无预约,判断是否继续跳转....");
                                  if (OwnerViewModel?.IsTakePH_No == false && OwnerViewModel?.IsBeijing == true && OwnerViewModel?.IsBeijingRegister == true)
                                  {
                                      if (OwnerViewModel?.zbywxx != null)
                                      {
                                          HaveZBYW(info);
                                      }
                                      else
                                      {

                                          OwnerViewModel?.ActionToGo("", (() =>
                                          {
                                              this.DoNext.Execute("Booking/BookingTarget");
                                          }), null, "取号");
                                      }

                                  }
                                  else if (OwnerViewModel?.IsTakePH_No == false && OwnerViewModel?.IsBeijing == true && OwnerViewModel?.IsBeijingRegister == false)
                                  {
                                      OwnerViewModel?.MessageTips("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                                      {
                                          this.DoExit.Execute(null);
                                      }));
                                  }
                                  else
                                  {
                                      //正常跳转播放现场预约语音文件
                                      TTS.PlaySound("预约机-提示-现场预约");
                                      OwnerViewModel?.MessageTips("找不到您今日的有效预约数据，请您再次核对，若有疑问，请移步人工窗口咨询！如需现场预约请点击【前往】按钮。", (() =>
                                      {
                                          Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                                          this.DoExit.Execute(null);
                                      }), () =>
                                      {
                                          Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                                          this.DoNext.Execute("Booking/BookingTarget");
                                      });
                                  }

                              }
                          }
                          //已查询到当天的预约信息
                          else
                          {
                              //有预约播放重新预约语音文件
                              TTS.PlaySound("预约机-提示-重新预约");
                              string bookingDate = BookingBaseInfo?.BookingTarget?.BookingDt?.ToString("yyyy-MM-dd");
                              string bookingTime = BookingBaseInfo?.BookingTarget?.Title;

                              if (OwnerViewModel?.IsBeijing == false && OwnerViewModel?.IsTakePH_No == true)
                              {
                                  Log.Instance.WriteInfo($"{info.FullName}您已预约{bookingDate} {bookingTime}办理{BookingBaseInfo.StrCardTypes}业务");
                                  this.DoNext.Execute("Booking/BookingTarget");
                              }

                              if (OwnerViewModel?.IsBeijing == true)
                              {
                                  if (OwnerViewModel?.zbywxx != null)
                                  {
                                      HaveZBYW(info);
                                  }
                                  else
                                  {
                                      //您已预约{ bookingDate}{ bookingTime}办理{ BookingBaseInfo.StrCardTypes}业务,若您需要重新预约办理新业务,请点击【重新预约】按钮，上一次的预约办理业务将自动失效,若有疑问请咨询现场工作人员。

                                      OwnerViewModel?.MessageTips($"{info.FullName}您已预约{bookingDate} {bookingTime}办理{BookingBaseInfo.StrCardTypes}业务,请点击【取号】按钮取号。", (() =>
                                      {
                                          Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                                          this.DoExit.Execute(null);
                                      }), () =>
                                      {
                                          Log.Instance.WriteInfo("点击【取号】按钮，进入选择预约时间页面");
                                          //跳转填写基本资料页面
                                          this.DoNext.Execute("Booking/BookingTarget");
                                          //清空预约信息
                                          BookingBaseInfo.BookingTarget = null;
                                      });
                                  }

                              }
                              else//非北京地区有预约信息正常流程
                              {

                                  OwnerViewModel?.MessageTips($"{info.FullName}您已预约{bookingDate} {bookingTime}办理{BookingBaseInfo.StrCardTypes}业务,若您需要重新预约办理新业务,请点击【重新预约】按钮，上一次的预约办理业务将自动失效,若有疑问请咨询现场工作人员。", (() =>
                                  {
                                      Log.Instance.WriteInfo("点击【直接取号】按钮，进入照片回执页面");
                                      this.DoNext.Execute("ScanningPhotoReceipt");
                                  }), () =>
                                  {
                                      Log.Instance.WriteInfo("点击【重新预约】按钮，进入选择预约时间页面");
                                      //跳转填写基本资料页面
                                      this.DoNext.Execute("Booking/BookingTarget");
                                      //清空预约信息
                                      BookingBaseInfo.BookingTarget = null;
                                  }, 10, "直接取号");
                              }
                          }

                      }
                      else
                      {
                          OwnerViewModel?.MessageTips(msg, (() =>
                          {
                              this.DoExit.Execute(null);
                          }));
                      }
                  }
                  catch (Exception ex)
                  {
                      Log.Instance.WriteError("身份识读异常" + ex.Message);
                  }
                  finally
                  {
                      Log.Instance.WriteInfo("=====结束获取身份证信息操作界面=====");
                      OwnerViewModel?.IsShowHiddenLoadingWait();
                  }
              });
            QueryReadIDCardThread.IsBackground = true;
            QueryReadIDCardThread.Start();
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
        private bool DataVerification(string idCardNo, out string msg)
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
                    Log.Instance.WriteInfo("开始登录太极接口......");
                    //太极接口登录
                    Json_I_DY_login dyLogin = new Json_I_DY_login
                    {
                        loginName = TjConfig.TjModel.DYUserName,
                        password = TjConfig.TjModel.DYUserPwd
                    };

                    //登录返回实体类
                    var loginTaiji = new TaiJiHelper().DO_DY_Login(dyLogin);
                    //new Json_R_DY_login_Rec
                    //{
                    //    dwid = "BA1D16BAAAA84982BD6E844C730A0E58",
                    //    dwmc = "2",
                    //    dwdm = "3",
                    //    yhid = "9c8b442f3a8d423d85a74345e1593c3e"
                    //};

                    OwnerViewModel.DyLoginRec = loginTaiji;
                    Log.Instance.WriteInfo("太极导服接口登录返回成功：单位编号：" + loginTaiji?.dwid + "，单位代码：" + loginTaiji?.dwdm);

                    Log.Instance.WriteInfo("========" + info.IDCardNo + "开始核查太极人口信息========");
                    //登录接口
                    var logininfo = OwnerViewModel?.DyLoginRec;

                    if (logininfo != null)
                    {
                        Json_I_DY_check dymodel = new Json_I_DY_check
                        {
                            dwid = logininfo?.dwid,
                            loginName = TjConfig.TjModel.DYUserName,
                            sfzh = info?.IDCardNo
                        };
                        Log.Instance.WriteInfo("开始执行导服核查接口...");
                        //导服核查接口
                        Json_R_DY_check_Rec dyCheck = new TaiJiHelper().Do_DY_Check(dymodel);
                        if (dyCheck != null)
                        {

                            //导服核查返回结果
                            Log.Instance.WriteInfo(dyCheck?.zwx + dyCheck?.zwm + "导服查询结果返回：" + dyCheck?.ywid);
                            //核查和有预约的ywid是否一致
                            //1.有预约 有ywid 取之前ywid 不上传
                            //2.无预约 无ywid 赋值新ywid 上传
                            if (dyCheck?.zbywxxs != null && dyCheck?.zbywxxs.Length > 0)
                            {
                                //存在在办业务 报警上传
                                OwnerViewModel.zbywxx = dyCheck?.zbywxxs;
                                //Log.Instance.WriteInfo("在办业务：" + dyCheck?.zbywxxs);
                                DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                                DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000002);
                                DevAlarm.AlarmInfo = info.FullName + "核查到有正在办理且未完结的业务，身份证：" + info.IDCardNo + "，业务id：" + dyCheck?.ywid;
                                ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000002).ToString());

                                if (dyCheck?.zbywxxs.Length == 3)
                                {
                                    Log.Instance.WriteInfo("在办业务总数：" + dyCheck?.zbywxxs.Length);
                                    msg = info.FullName + "系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";

                                    return false;
                                }

                            }
                            if (!string.IsNullOrEmpty(OwnerViewModel?.YWID) && OwnerViewModel?.YWID == dyCheck?.ywid)
                            {
                                Log.Instance.WriteInfo("旧导引id：" + OwnerViewModel?.YWID + "，新导引id：" + dyCheck?.ywid);
                                if (dyCheck?.zbywxxs != null)
                                {
                                    if (dyCheck?.zbywxxs.Length > 0)
                                    {
                                        //OwnerViewModel.zbywxx = dyCheck?.zbywxxs;
                                        //核查到有正在办理且未完结的业务，不能派号
                                        Log.Instance.WriteInfo(msg);
                                        DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault21;
                                        DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_20000002);
                                        DevAlarm.AlarmInfo = info.IDCardNo + "核查到有正在办理且未完结的业务，身份证：" + info.IDCardNo + "，业务id：" + dyCheck?.ywid;
                                        ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_20000002).ToString());
                                        msg = "核查到您有正在办理且未完结的业务，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";
                                        //return false;
                                    }
                                }
                            }
                            else
                            {
                                OwnerViewModel.YWID = dyCheck?.ywid;
                                Log.Instance.WriteInfo("未上传列表:" + OwnerViewModel.YWID);
                                //业务id放到未上传列表，上传后删除
                                OwnerViewModel.UnOverYwList.Add(dyCheck.ywid);
                            }
                            //是否是国家工作人员

                            if (QJTConfig.QJTModel.IsOfficial && dyCheck?.sfgjgzry != null)
                            {
                                _isOfficial = true;
                                info.IsOfficial = true;
                                if (dyCheck?.sfgjgzry == "1")
                                {
                                    //info.IsOfficial = true;
                                    //国家工作人员获取人事主管单位 后面区域派号传
                                    if (dyCheck.gjgzryxxs.Length > 0)
                                    {
                                        OwnerViewModel.IsOfficial = "(国家工作人员)";
                                        info.RSZGDW = dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0].bsdwmc;
                                        _RSZGDW = dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0].bsdwmc;
                                        Log.Instance.WriteInfo("查询到国家工作人员，人事主管单位：" + dyCheck?.gjgzryxxs[0]?.zzjgdm + "+" + dyCheck?.gjgzryxxs[0].bsdwmc);
                                    }
                                    msg = OwnerViewModel?.IsShanXi == true ?
                                        "请前往人工拍照区域进行拍照" : dyCheck?.gjgzryxxs[0].bsdwmc + TipMsgResource.GJGWRYTipMsg;
                                }

                            }
                            else
                            {
                                _isOfficial = false;
                                info.IsOfficial = false;
                                Log.Instance.WriteInfo("查询国家工作人员返回：" + dyCheck?.sfgjgzry);
                            }


                            //是否是控制对象
                            if (QJTConfig.QJTModel.IsCheckSpecil && dyCheck?.sfkzdx == "1")
                            {
                                info.IsCheckSpecil = true;
                                _isOfficial = true;
                                msg = OwnerViewModel?.IsShanXi == true ?
                                    "请前往人工拍照区域进行拍照" : TipMsgResource.SKDXTipMsg;
                                return false;
                            }

                            if (dyCheck?.sfxjzdry == "1")
                            {
                                _issfxjzdry = true;
                                msg = "您不符合自助办理条件，请咨询工作人员！";
                                Log.Instance.WriteInfo("核查到为新疆重点人员");
                                return false;
                            }

                            if (dyCheck?.sfzddq == "1")
                            {
                                _issfzddq = true;
                                msg = "您不符合自助办理条件，请咨询工作人员！";
                                Log.Instance.WriteInfo("核查到为重点地区人员");
                                return false;
                            }


                            _isCheckSpecil = false;
                            Log.Instance.WriteInfo("查询控制对象返回：" + dyCheck?.sfkzdx);
                            //是否有预约信息
                            //Log.Instance.WriteInfo("查询预约信息返回：" + dyCheck?.yyxxs?.Length);
                            if (dyCheck?.yyxxs?.Length > 0 && dyCheck?.yyxxs[0].yybzlbs != null)
                            {
                                Log.Instance.WriteInfo("查询到预约信息，办理业务为：" + dyCheck?.yyxxs[0].yybzlbs);
                                BookingBaseInfo.BookingSource = 0;
                                //已预约的时间段信息
                                //太极无预约时间字段 有预约 北京地区默认当天办理
                                BookingBaseInfo.BookingTarget = new BookingTargetModel
                                {
                                    BookingDate = DateTime.Now.ToString("yyyyMMdd"),
                                    StartTime = "08:00",
                                    EndTime = "18:00"
                                };
                                Log.Instance.WriteInfo("预约信息时间：" + BookingBaseInfo?.BookingTarget?.BookingDate + "-" + BookingBaseInfo?.BookingTarget?.StartTime + "-" + BookingBaseInfo?.BookingTarget?.EndTime);
                                //预约电话和紧急联系人 、职业
                                BookingBaseInfo.UrgentName = dyCheck?.yyxxs[0]?.jjlxr;
                                BookingBaseInfo.Telephone = dyCheck?.yyxxs[0]?.brlxdh;
                                BookingBaseInfo.UrgentTelephone = dyCheck?.yyxxs[0]?.jjlxrdh;
                                //BookingBaseInfo.HasJob = result?.ReturnValue[0]?.ZY;
                                var job = DictionaryType.FirstOrDefault(t => t.Code == dyCheck?.yyxxs[0]?.zy);
                                BookingBaseInfo.HasJob = job?.Description;
                                BookingBaseInfo.Job = job;

                                Log.Instance.WriteInfo("紧急联系人：" + BookingBaseInfo?.UrgentName + "，本人电话：" + BookingBaseInfo?.Telephone + ",紧急联系人电话：" + BookingBaseInfo?.UrgentTelephone + "，职业：" + BookingBaseInfo?.HasJob);


                                //拼接已预约的业务类型
                                if (dyCheck?.yyxxs[0].yybzlbs != null)
                                {
                                    var strlst = dyCheck?.yyxxs?.Select(t => EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.yybzlbs[0].sqlb)));

                                    BookingBaseInfo.StrCardTypes = string.Join("、", strlst);
                                    if (!string.IsNullOrEmpty(BookingBaseInfo.StrCardTypes))
                                    {
                                        Log.Instance.WriteInfo("保存预约类型：" + BookingBaseInfo?.StrCardTypes);
                                    }
                                    else
                                    {
                                        Log.Instance.WriteInfo("查询到预约类型为空！！！");
                                    }
                                }

                            }
                            else
                            {
                                Log.Instance.WriteInfo(info.IDCardNo + "未查询到今天的预约信息！");
                            }
                            if (QJTConfig.QJTModel.IsCheckInformation)
                            {
                                //连接大集中 取大集中数据至预约数据
                                Log.Instance.WriteInfo("==" + info.IDCardNo + "开始核查太极接口人口信息==");
                                if (dyCheck?.rkxxs?.Length > 0)
                                {
                                    if (dyCheck?.rkxxs[0]?.mzdm.ToString().Contains("04") == true ||
                                        dyCheck?.rkxxs[0]?.mzdm.ToString().Contains("05") == true)
                                    {
                                        msg = "您不符合自助办理条件，请咨询工作人员！";
                                        Log.Instance.WriteInfo("核查到民族为维吾尔族或藏族！" + dyCheck?.rkxxs[0]?.mzdm);
                                        return false;
                                    }

                                    if (!string.IsNullOrEmpty(dyCheck?.rkxxs[0].zp))
                                    {
                                        //var photo = CommandTools.Base64StringToImage(dyCheck?.rkxxs[0].zp);

                                        //System.IO.FileStream fs =
                                        //    new System.IO.FileStream(filepath,
                                        //        System.IO.FileMode.Open, System.IO.FileAccess.Read);

                                        //OwnerViewModel.DjzPhoto = photo;
                                        //BitmapImage
                                    }



                                    //保存人口库户口所在地信息
                                    BookingBaseInfo.Address = new DictionaryType()
                                    {
                                        Code = dyCheck?.rkxxs[0]?.hkszddm?.ToString(),
                                        Description = dyCheck?.rkxxs[0]?.hkszdmc.ToString(),
                                    };
                                    //绑定到基本信息户口所在地输入框
                                    BookingBaseInfo.HasAddress = BookingBaseInfo.Address.Description;

                                    //保存人口库信息
                                    info.FullName = dyCheck?.rkxxs[0]?.xm?.ToString();
                                    //航信和太极性别返回值不同 需先判断
                                    if (dyCheck?.rkxxs[0]?.xb.ToString() != null)
                                    {
                                        info.Gender = dyCheck?.rkxxs[0]?.xb == "1" ? "1" : "2";
                                    }

                                    info.pNational = dyCheck?.rkxxs[0]?.mzdm.ToString();
                                    info.Address = dyCheck?.rkxxs[0]?.jtzz?.ToString();
                                    info.Birthday = dyCheck?.rkxxs[0]?.csrq?.Replace("-", "");
                                    Log.Instance.WriteInfo(dyCheck?.rkxxs[0]?.sfzh?.ToString() + "查询太极接口全国人口库信息：" + dyCheck?.rkxxs[0]?.xm?.ToString() + "来自" + dyCheck?.rkxxs[0]?.csdmc?.ToString() + "，户口所在地：" + dyCheck?.rkxxs[0]?.hkszdmc?.ToString() + "性别：" + info.Gender?.ToString() + "，民族：" + dyCheck?.rkxxs[0].mzdm + "，生日：" + info.Birthday);
                                    if (string.IsNullOrEmpty(info.Birthday) && !string.IsNullOrEmpty(info.IDCardNo))
                                    {
                                        info.Birthday = info.IDCardNo.Substring(6, 8);
                                    }
                                    if (string.IsNullOrWhiteSpace(info.FullName))
                                    {
                                        Log.Instance.WriteInfo(info.IDCardNo + "未核查到全国人口库姓名");
                                        msg = OwnerViewModel?.IsShanXi == true ?
                                            "请前往人工拍照区域进行拍照" : TipMsgResource.IdentityExceptionTipMsg;
                                        return false;
                                    }
                                    if (dyCheck != null && dyCheck.rkxxs == null)
                                    {
                                        Log.Instance.WriteInfo("全国人口库服务器响应失败，请重试！");
                                        DevAlarm.AlarmTypeID = (int)EnumTypeALARMTYPEID.Fault22;
                                        DevAlarm.AlarmTitle = EnumType.GetEnumDescription(EnumTypeALARMCODE.ALARMCODE_50000004);
                                        DevAlarm.AlarmInfo = "太极接口返回数据为空";//返回信息
                                        ZHWSBase.Instance.InsertDeviceAutoFault(QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(), DevAlarm.AlarmTypeID.ToString(), DevAlarm?.AlarmInfo.ToString(), DevAlarm.AlarmTitle, ((int)EnumTypeALARMCODE.ALARMCODE_50000004).ToString());
                                    }


                                    Log.Instance.WriteInfo("========" + info.IDCardNo + "结束核查人口信息========");
                                }
                                Log.Instance.WriteInfo("========" + info.IDCardNo + "开始核查证件信息========");
                                if (dyCheck?.zjxxs.Length > 0)
                                {
                                    Log.Instance.WriteInfo("核查到存在有效证件信息：" + dyCheck?.zjxxs.Length + "条");

                                    OwnerViewModel.PaperWork = dyCheck?.zjxxs;
                                    Log.Instance.WriteInfo("核查有效证件条数为：" + OwnerViewModel?.PaperWork.Length + "条，号码为" + OwnerViewModel?.PaperWork?.FirstOrDefault()?.zjhm + "");
                                }
                                Log.Instance.WriteInfo("========" + info.IDCardNo + "结束核查证件信息========");

                                //核查太极返回可办业务类型
                                Log.Instance.WriteInfo("========" + info.IDCardNo + "开始核查可办业务========");
                                if (dyCheck != null && dyCheck.kbywxx != null && dyCheck.kbywxx.Length > 0)
                                {
                                    Log.Instance.WriteInfo(info.IDCardNo + "核查可办业务为：" + dyCheck.kbywxx.Length + "条");
                                    OwnerViewModel.KbywInfos = dyCheck.kbywxx;

                                }
                                else
                                {
                                    Log.Instance.WriteInfo(info.IDCardNo + "核查可办业务为空");
                                    msg = info.FullName + "系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！";
                                    return false;
                                }
                                Log.Instance.WriteInfo("========" + info.IDCardNo + "结束核查可办业务========");



                            }


                        }
                        else
                        {
                            Log.Instance.WriteInfo("导服核查接口查询返回：" + dyCheck?.zwx);
                        }
                    }
                    else
                    {
                        //Log.Instance.WriteInfo("太极接口登录返回：" + JsonHelper.ToJson(loginTaiji));
                    }
                    Log.Instance.WriteInfo("========" + info.IDCardNo + "结束核查太极人口信息========");
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
                    strTypesDes = string.Join("、",
                        dicTypes.Select(t =>
                            EnumType.GetEnumDescription((EnumSqlType)int.Parse(t.Code))));

                    msg = info.FullName + "系统查询到您正在办理[" + strTypesDes.Substring(0, strTypesDes.Length) + "]业务，如需预约请点击【前往】按钮。";
                    OwnerViewModel?.MessageTips(msg, (() =>
                    {
                        Log.Instance.WriteInfo("点击【返回】按钮，进入主页面");
                        this.DoExit.Execute(null);
                    }), () =>
                    {
                        Log.Instance.WriteInfo("点击【前往】按钮，进入选择预约时间页面");
                        this.DoNext.Execute("Booking/BookingTarget");
                    });
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
            if (!string.IsNullOrEmpty(info.IDCardNo))
            {
                DateTime now = DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());
                if (!string.IsNullOrEmpty(now.ToString()))
                {
                    Log.Instance.WriteInfo("当前时间：" + now.ToString("yyyy年MM月dd日 HH:mm:ss"));
                    DateTime birth;
                    DateTime.TryParse(info.IDCardNo.Substring(6, 4) + "-" + info.IDCardNo.Substring(10, 2) + "-" +
                                      info.IDCardNo.Substring(12, 2), out birth);
                    int age = now.Year - birth.Year; //年龄
                    if (now.Month < birth.Month || (now.Month == birth.Month && now.Day < birth.Day))
                        age--;
                    Log.Instance.WriteInfo("计算出年龄为：" + age);
                    if (age < 16)
                    {
                        if (OwnerViewModel.IsShanXi)
                        {
                            strMsg = "16周岁以下未成年申请人，请前往人工拍照区域进行拍照！";
                            Log.Instance.WriteInfo(strMsg);
                            //OwnerViewModel.IsManual = true;
                            msg = strMsg;
                            return false;
                        }

                        if (OwnerViewModel?.IsBeijing == true)
                        {
                            strMsg = "16周岁以下未成年申请人，请前往人工窗口办理！";
                            Log.Instance.WriteInfo(strMsg);
                            OwnerViewModel.IsOfficial = strMsg;
                        }
                    }

                    if (age > 65 && OwnerViewModel.IsShanXi)
                    {
                        strMsg = "65周岁以上长者，请前往人工拍照区域进行拍照！";
                        Log.Instance.WriteInfo(strMsg);
                        //OwnerViewModel.IsManual = true;
                        msg = strMsg;
                        return false;

                    }
                }

            }

            return true;
        }

        /// <summary>
        /// (北京太极缓存)清除上个人办证缓存
        /// </summary>
        public void ClearCache()
        {
            //清空上个人的证件信息
            OwnerViewModel.PaperWork = null;
            //清空上个人在办业务
            OwnerViewModel.zbywxx = null;
            //清空上个人国家工作人员信息
            OwnerViewModel.IsOfficial = "";
            //清空上个人预约业务
            OwnerViewModel._hasYYXX = null;
            //清空上个人照片
            OwnerViewModel.DjzPhoto = null;
            //清空上个人办证类别
            OwnerViewModel.PaperInfos = null;
        }

        #endregion
    }
}
