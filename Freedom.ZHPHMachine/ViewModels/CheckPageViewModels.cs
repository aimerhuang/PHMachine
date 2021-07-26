using Freedom.Common;
using Freedom.Config;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.ZHPHMachine;
using Freedom.ZHPHMachine.Command;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.ViewModels;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Freedom.BLL;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Models.CrjDataModels;
using Freedom.Models.HsZhPjhJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;

namespace SFreedom.ZHPHMachine.ViewModels
{
    public class CheckPageViewModels : ViewModelBase
    {
        #region 属性
        private CrjPreapplyManager crjManager = new CrjPreapplyManager();
        private string strCheckResult = string.Empty;
        private TaiJiHelper taiJiHelper = new TaiJiHelper();
        /// <summary>
        /// 办理业务信息
        /// </summary>
        List<YwlbInfo> ywlbList = new List<YwlbInfo>();
        YwlbInfo info = new YwlbInfo();

        /// <summary>
        /// 结果消息
        /// </summary>
        public string StrCheckResult
        {
            get { return this.strCheckResult; }

            set
            {
                this.strCheckResult = value;
                RaisePropertyChanged("StrCheckResult");
            }
        }

        private Visual printTemplate;
        public Visual PrintTemplate
        {
            get { return printTemplate; }
            set { printTemplate = value; this.RaisePropertyChanged("PrintTemplate"); }
        }

        private JsonPH_SendOut sendInfo;

        /// <summary>
        /// 派号信息
        /// </summary>
        public JsonPH_SendOut SendInfo
        {
            get { return sendInfo; }
            set { sendInfo = value; RaisePropertyChanged("SendInfo"); }
        }

        private ImageSource codeImg;

        /// <summary>
        /// 派号二维码
        /// </summary>
        public ImageSource CodeImg
        {
            get { return codeImg; }
            set { codeImg = value; RaisePropertyChanged("CodeImg"); }
        }

        private string bookingPerSonName;

        /// <summary>
        /// 预约人姓名
        /// </summary>
        public string BookingPerSonName
        {
            get { return bookingPerSonName; }
            set { bookingPerSonName = value; RaisePropertyChanged("BookingPerSonName"); }
        }

        private string bookingDateTime;

        /// <summary>
        /// 预约时间
        /// </summary>
        public string BookingDateTime
        {
            get { return bookingDateTime; }
            set { bookingDateTime = value; RaisePropertyChanged("BookingDateTime"); }
        }

        /// <summary>
        /// 倒计时取票时间
        /// </summary>
        public int CountDown { get; set; } = 15;
        #endregion

        #region 构造函数
        public CheckPageViewModels()
        {
            this.TipMessage = "派号完成!!!";
            PH();
        }
        #endregion

        #region 方法 
        private async void PH()
        {
            try
            {
                OwnerViewModel?.IsShowHiddenLoadingWait("正在派号,请稍等...");
                //派号前预先关闭文档进程
                //PrintHelper.killWinWordProcess();
                //if (OwnerViewModel?.CheckServeStatus() == false)
                //{
                //    DoNext.Execute("NotificationPage");
                //}
                Log.Instance.WriteInfo("\n**********************************进入【派号成功】界面**********************************");

                Log.Instance.WriteInfo("正在检测打印机状态……");
                //检查打印机状态
                if (!PrintHelper.CheckPrinter(out string msg))
                {
                    OwnerViewModel?.MessageTips(msg, () => { DoExit.Execute(null); });
                    Log.Instance.WriteInfo("打印机初始化失败：" + msg);
                    return;
                }
                else
                    Log.Instance.WriteInfo("打印机正常！！！");

                //检测是否能连上服务器
                var sysTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                Log.Instance.WriteInfo("正在检测应用服务器连接：" + sysTime);
                if (string.IsNullOrEmpty(sysTime))
                {
                    //增加10秒测试服务连接是否正常 by 2021年7月7日11:02:38 wei.chen
                    for (int i = 1; i <= 10; i++)
                    {
                        sysTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                        Log.Instance.WriteInfo("重新【" + i + "】获取应用服务器连接返回：" + sysTime);
                        if (sysTime.IsNotEmpty())
                            break;
                        Thread.Sleep(1000);
                    }
                    if (string.IsNullOrEmpty(sysTime))
                    {
                        msg = "应用服务器连接出现异常，派号失败！请重试！";
                        OwnerViewModel?.MessageTips(msg, () =>
                        {
                            DoExit.Execute(null);
                        });
                        Log.Instance.WriteInfo("应用服务器连接出现异常，派号失败！请重试！：" + msg);
                        Log.Instance.WriteError("应用服务器连接出现异常，派号失败！请重试！：" + msg);
                        //切换到暂停页面
                        base.DoNextFunction("NotificationPage");
                    }
                }

                Log.Instance.WriteInfo("检测当前时间返回：" + sysTime);
                if (string.IsNullOrEmpty(sysTime))
                {
                    //切换到暂停页面
                    Log.Instance.WriteInfo("获取服务器时间失败，开始跳转机器故障页面");
                    base.DoNextFunction("NotificationPage");
                }

                Log.Instance.WriteInfo("检测当前时间返回：" + sysTime);
                if (string.IsNullOrEmpty(sysTime))
                {
                    //切换到暂停页面
                    base.DoNextFunction("NotificationPage");
                }


                Log.Instance.WriteInfo("开始【保存预约信息】，条数为：" + BookingBaseInfo.BookingInfo?.Count);
                if (BookingBaseInfo?.IsGetPHNO == "0")
                {
                    //判断是否网上预约 0 网上预约 1 现场预约
                    //if (BookingBaseInfo.BookingSource == 1)
                    //{
                    //Log.Instance.WriteInfo("====开始保存预约信息====");

                    if (!string.IsNullOrEmpty(BookingBaseInfo?.ReceiptsNo) && BookingBaseInfo?.BookingInfo != null)
                    {
                        //保存预约数据时，添加照片编号
                        foreach (var item in BookingBaseInfo?.BookingInfo)
                        {
                            item.ZPBM = BookingBaseInfo.ReceiptsNo;
                        }
                    }

                    //保存预约信息
                    var result = await ZHPHMachineWSHelper.ZHPHInstance.I_YYSQXX(BookingBaseInfo?.BookingInfo);
                    if (result != null && result.Item2.IsSucceed && result.Item1.Count > 0)
                    {
                        BookingBaseInfo.StrBookingNo = string.Join("、", result.Item1.Select(t => t.YWBH));
                        BookingBaseInfo.BookingTarget.Title = result?.Item1?.FirstOrDefault()?.WSYYSJ;
                    }
                    else
                    {
                        OwnerViewModel?.MessageTips("保存预约数据失败：" + result?.Item2?.MessageInfo, () =>
                        {
                            this.DoNextFunction("MainPage");
                        });
                        return;
                    }
                    Thread.Sleep(500);
                    Log.Instance.WriteInfo("结束【保存预约信息】");
                }
                //北京仅签注，上传默认制证照片
                if (OwnerViewModel?.IsBeijing == true && BookingBaseInfo.ReceiptsNo == "DY12345678")
                {
                    var result = UpLoadPhoto(1, "", out msg);
                }

                //武汉上传身份证照片
                if (OwnerViewModel?.IsWuHan == true)
                {
                    if (BookingBaseInfo?.CardInfo?.ImageUrl?.IsNotEmpty() == true)
                    {
                        UpLoadPhoto(2, BookingBaseInfo?.CardInfo?.ImageUrl, out msg);
                    }
                }
                //湛江上传默认空照片
                if (BookingBaseInfo.ReceiptsNo == "DY12345678")//OwnerViewModel?.IsXuwen == true && 
                {
                    var result = UpLoadPhoto(3, "", out msg);
                }

                OpenTimeOut(OwnerViewModel?.IsShenZhen == true ? 3 : CountDown);
                //判断是否预约当天否则不派号
                if (QJTConfig.QJTModel.IsTodayPH || BookingBaseInfo?.BookingSource == 1 && BookingBaseInfo?.BookingTarget?.BookingDate?.Equals(DateTime.Now.ToString("yyyyMMdd")) == false)
                {
                    Log.Instance.WriteInfo("查询到非当天预约，进入派号成功界面！");
                    this.DoNext.Execute("Booking/BookingComplete");
                    return;
                }
                //判断是否拍照 0否 1是
                int isPhoto = string.IsNullOrWhiteSpace(BookingBaseInfo?.ReceiptsNo) ? 0 : 1;
                if (OwnerViewModel?.RenGong == 2)
                {
                    isPhoto = OwnerViewModel.RenGong;
                }

                if (OwnerViewModel?.IsWuHan == true && isPhoto == 0)
                {
                    isPhoto = 1;
                    BookingBaseInfo.ReceiptsNo = "DY12345678";
                }

                if (TjConfig.TjModel.IsConnectionTj && !taiJiHelper.Common_searchYwlb(OwnerViewModel.DyLoginRec.dwid))
                {
                    if (!DoConnectingTest())
                    {
                        OwnerViewModel?.MessageTips("派号失败：" + "导服结果上传失败！请联系管理员或重新取号！", () =>
                        {
                            this.DoExitFunction("MainPage");
                        });
                        return;
                    }
                }

                SendInfo = await ZHPHMachineWSHelper.ZHPHInstance.PH_SendNo(BookingBaseInfo.CardInfo.IDCardNo, BookingBaseInfo.CardInfo.FullName, "", isPhoto, BookingBaseInfo?.ReceiptsNo, BookingBaseInfo?.AreaCode?.SERVICE_CODE, "0");
                if (SendInfo?.SFZH?.IsNotEmpty() == true)
                {

                    TTS.PlaySound("预约机-页面-派号成功，请在屏幕下方取走您的派号单");
                    Log.Instance.WriteInfo("派号结果：" + sendInfo.PREFIX + sendInfo.PH_NUMBER + "号-" + CommandTools.ReplaceWithSpecialChar(sendInfo.ZWXM) + "-" + sendInfo.SERVICE_CNDEC + "-" + sendInfo.YY_TIME + "-" + "等候人数：" + sendInfo.PH_WAITNUMS + "-" + sendInfo.PRINT_GUIDE);
                    OwnerViewModel?.IsShowHiddenLoadingWait("打印派号单,请稍等...");
                    //Log.Instance.WriteInfo("派号成功，开始打印派号单");

                    //导服上传和流程上报
                    var uploadResult = TjUpLoading();
                    if (!uploadResult)
                    {
                        if (!TjUpLoading())
                        {
                            OwnerViewModel?.MessageTips("派号失败：" + "导服结果上传失败！请联系管理员或重新取号！", () =>
                            {
                                this.DoExitFunction("MainPage");
                            });
                            return;
                        }
                    }
                    //上传太极设备状态为离线

                    //获取单位名称
                    string billDwName = await QueryPrintAsync(BookingBaseInfo?.AreaCode?.QYCODE);

                    if (OwnerViewModel?.IsShenZhen == true)
                    {
                        await PrintHelper.WordPrintNew($"{SendInfo.PREFIX}{SendInfo.PH_NUMBER}",
                            SendInfo.Id.ToString(), SendInfo.PRINT_GUIDE, billDwName, sendInfo.PH_WAITNUMS, BookingBaseInfo, SendInfo);
                    }
                    else
                    {
                        //创建新线程打印派号单
                        //thCommon = new System.Threading.Thread(() =>
                        //{
                        //    PrintHelper.BookingWordPrintNew($"{SendInfo.PREFIX}{SendInfo.PH_NUMBER}",
                        //        SendInfo.Id.ToString(), SendInfo.PRINT_GUIDE, billDwName, sendInfo.PH_WAITNUMS, BookingBaseInfo, SendInfo);
                        //});
                        //thCommon.IsBackground = true;
                        //thCommon.Start();

                        if (PrintHelper.BookingWordPrintNew($"{SendInfo.PREFIX}{SendInfo.PH_NUMBER}",
                    SendInfo.Id.ToString(), SendInfo.PRINT_GUIDE, billDwName, sendInfo.PH_WAITNUMS, BookingBaseInfo, SendInfo))
                        {
                            Log.Instance.WriteInfo("BookingWordPrintNew 操作结果：成功！！！");
                        };

                    }


                    //修改设备状态为离线
                    bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.FREE);

                }
                else
                {
                    //TTS.PlaySound(sendInfo?.MessageInfo?.Contains("制证照片") == true
                    //    ? "预约机-提示-获取制证照片失败，不派号"
                    //    : "预约机-提示-派号失败，请重新取号");
                    Log.Instance.WriteInfo(sendInfo?.MessageInfo);
                    OwnerViewModel?.MessageTips("派号失败：" + sendInfo?.MessageInfo, () =>
                     {
                         this.DoNextFunction("MainPage");
                     });
                }

            }
            catch (Exception ex)
            {

                Log.Instance.WriteInfo("派号异常" + ex.Message);
                Log.Instance.WriteError("派号异常:" + ex.Message);
                OwnerViewModel?.MessageTips(ex.Message, () =>
                {
                    DoExit.Execute(null);
                });
            }
            finally
            {
                //移到首页加载处理 by 2021年7月6日17:10:57 wei.chen
                ////清空上个人的信息
                //OwnerViewModel.PaperWork = null;
                //OwnerViewModel.YWID = null;
                //OwnerViewModel.KbywInfos = null;
                //OwnerViewModel.DyLoginRec = null;
                //OwnerViewModel.IsManual = false;
                //OwnerViewModel.ReceiptCardNo = "";
                ////OwnerViewModel._HasBZLB = null;

                Log.Instance.WriteInfo("\n**********************************结束【打印派号单】界面**********************************");
                Log.Instance.WriteInfo("\n**********************************离开【派号成功】界面**********************************");
                OwnerViewModel?.IsShowHiddenLoadingWait();
                //OnDispose();
            }
        }

        /// <summary>
        /// 根据区域编号取单位名称
        /// </summary>
        /// <param name="code">区域编号</param>
        /// <returns>单位名称</returns>
        private async Task<string> QueryPrintAsync(string code)
        {
            try
            {
                PARA_VALUE param = null;
                code = string.IsNullOrWhiteSpace(code) ? QJTConfig.QJTModel.QJTDevInfo.QYCODE : code;
                var config = ServiceRegistry.Instance.Get<ElementManager>();
                var lst = config?.Get<List<PH_SYSPARASETTING_TB>>();
                var model = lst?.FirstOrDefault(t => t.NAME == "PRINT_BILL_Title" && t.QYCODE == code);
                if (model == null)
                {
                    lst = await ZHPHMachineWSHelper.ZHPHInstance.S_SysParaSettingList(code);
                    if (lst != null && lst.Count > 0)
                    {
                        model = lst?.FirstOrDefault(t => t.NAME == "PRINT_BILL_Title");
                        config.Get<List<PH_SYSPARASETTING_TB>>()?.AddRange(lst);
                    }
                }
                param = JsonHelper.ConvertToObject<PARA_VALUE>(model?.PARA_VALUE);
                return param?.BillDwName;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("查询派号区域配置信息异常" + ex.Message);
            }
            return "";
        }

        /// <summary>
        /// 导服上传接口和流程上报
        /// </summary>
        public bool TjUpLoading()
        {
            if (TjConfig.TjModel.IsConnectionTj)
            {
                if (!string.IsNullOrEmpty(OwnerViewModel?.YWID))
                {
                    Log.Instance.WriteInfo("【太极】开始上传导服结果");
                    //通过PARENT_CODE 来判断是否是取的一站式的号还是一桌式的取号
                    OwnerViewModel.TaijiPhMode = BookingBaseInfo?.AreaCode?.PARENT_CODE != null ? BookingBaseInfo?.AreaCode?.PARENT_CODE.ToInt().ToString() : QJTConfig.QJTModel.TaijiPHMode;
                    //Log.Instance.WriteInfo("取号方式：" + OwnerViewModel.TaijiPhMode);
                    //太极接口导服数据上传回写进太极库
                    var result = Dy_Upload(SendInfo);
                    Log.Instance.WriteInfo(result ? "导服上传结果：【成功】" : "导服上传结果：【失败】");
                    var EndTime = DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());
                    //和排队叫号数据上报
                    Json_I_Common_saveQueue queueModel = new Json_I_Common_saveQueue
                    {
                        dwid = OwnerViewModel?.DyLoginRec?.dwid,
                        pdlb = OwnerViewModel?.TaijiPhMode == "0" ? ((int)TaiJiHelper.pdlb.OneZhan).ToString() : ((int)TaiJiHelper.pdlb.OneZhuo).ToString(),//106
                        pdh = sendInfo.PREFIX + sendInfo.PH_NUMBER.ToString(),
                        pdzt = "01",//太极文档长度1位入不了库，需前面补0 ,目前00取号状态传不了 先传01
                        jqbh = TjConfig.TjModel.TJMachineNo,
                        zjhm = BookingBaseInfo.CardInfo.IDCardNo
                    };
                    Log.Instance.WriteInfo("排队叫号数据上报参数：" + JsonHelper.ToJson(queueModel));
                    var queue = taiJiHelper.Common_saveQueue(queueModel);
                    Log.Instance.WriteInfo(queue ? "排队叫号上报：【成功】" : "排队叫号上报：【失败】");
                    //流程上报
                    Json_I_Common_saveFlow flowModel = new Json_I_Common_saveFlow
                    {
                        ywid = OwnerViewModel?.YWID,
                        hjdm = DM_HJB.DY02.ToString(),
                        clr = TjConfig.TjModel.TJMachineNo,
                        dwid = OwnerViewModel?.DyLoginRec?.dwid,
                        kssj = OwnerViewModel?.BeginTime.ToString("yyyyMMddHHmmss"),
                        wcsj = EndTime.ToString("yyyyMMddHHmmss")
                    };
                    Log.Instance.WriteInfo("流程上报参数：" + JsonHelper.ToJson(flowModel));
                    var flow = taiJiHelper.Common_saveFlow(flowModel);
                    Log.Instance.WriteInfo(flow ? "流程上报：【成功】" : "流程上报：【失败】");
                    Log.Instance.WriteInfo("【太极】结束上传导服结果");
                    if (result && queue && flow)
                        return true;
                    else
                        return false;
                }
                else
                {
                    Log.Instance.WriteInfo("获取导服登录业务编号失败！");
                    return false;
                }
            }

            return true;
        }



        /// <summary>
        /// 太极接口导服上传
        /// </summary> 
        /// <param name="SendInfo"></param>
        public bool Dy_Upload(JsonPH_SendOut SendInfo)
        {
            //var infactqy = SendInfo.PREFIX.Contains("Q")|| SendInfo.PREFIX.Contains("S") ? "1" : "0";//实际派号区域

            //var infactqy = SendInfo.PREFIX.Contains("Q") ? "1" : "0";//实际派号区域
            //获取实际办证类别
            if (!string.IsNullOrEmpty(OwnerViewModel?.YWID))
            {
                //Log.Instance.WriteInfo("====开始获取是否能进自助大厅开始====");
                foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
                {
                    info = new YwlbInfo();
                    info.sqlb = item?.Code; //申请类型
                    var model = BookingBaseInfo.BookingInfo.Where(t => t.SQLB.Equals(item?.Code)).ToList()[0];
                    info.bzlb = model?.ApplyType?.Code; //办证类型
                    info.qzblbs = model?.ApplyType?.Status.ToString(); //签注办理标识 后面修改

                    string logo = model?.YYJZBLLIST.Count > 0 ? "1" : "0";
                    info.jzblbs = logo; //加注办理标识
                    Log.Instance.WriteInfo("申请类型：" + info.sqlb + "，办证类型：" + info.bzlb + "，签注办理标识：" + info?.qzblbs?.ToString());
                    //if (item.Code == "104")
                    //continue;
                    ywlbList.Add(info);
                }
            }
            //改为按区域划分不按业务前缀分区。A区为一站式，A区以外的都是一桌办  2021年7月12日10:38:06 by wei.chen
            var infactqy = sendInfo.QY_CODE.Contains("110111_0068") ? "0" : "1";
            OwnerViewModel.TaijiPhMode = infactqy;
            //注释  by wei.chen
            //通过PARENT_CODE 来判断是否是取的一站式的号还是一桌式的取号
            //OwnerViewModel.TaijiPhMode = BookingBaseInfo?.AreaCode?.PARENT_CODE != null ? BookingBaseInfo?.AreaCode?.PARENT_CODE.ToInt().ToString() : QJTConfig.QJTModel.TaijiPHMode;

            //导服上传model
            Json_I_DY_upload dyUpload = new Json_I_DY_upload
            {
                ywid = OwnerViewModel?.YWID,
                ydrysf = OwnerViewModel?.ydrysf ?? "",
                sjbzlbs = ywlbList.ToArray(),
                dyqy = infactqy,
                //OwnerViewModel?.TaijiPhMode == "0" ? ((int)TaiJiHelper.dyqy.OneZhan).ToString() : ((int)TaiJiHelper.dyqy.OneZhuo).ToString(),
                // dyqy = QJTConfig.QJTModel.TaijiPHMode == "0" ? "0" : "1",
                //dyqy = OwnerViewModel?.TaijiPhMode == "0" ? ((int)TaiJiHelper.dyqy.OneZhan).ToString() : ((int)TaiJiHelper.dyqy.OneZhuo).ToString(),
                // dyqy = QJTConfig.QJTModel.TaijiPHMode == "0" ? "0" : "1",
                dyzt = "0",
                pdlb = infactqy == "0" ? ((int)TaiJiHelper.pdlb.OneZhan).ToString() : ((int)TaiJiHelper.pdlb.OneZhuo).ToString(),
                pdh = sendInfo.PREFIX + sendInfo.PH_NUMBER.ToString(),
                jqbh = TjConfig.TjModel.TJMachineNo
            };
            if (dyUpload.pdlb.IsEmpty())
                dyUpload.pdlb = QJTConfig.QJTModel.TaijiPHMode == "0" ? ((int)TaiJiHelper.pdlb.OneZhan).ToString() : ((int)TaiJiHelper.pdlb.OneZhuo).ToString();//106
            if (dyUpload.dyqy.IsEmpty())
                dyUpload.dyqy = OwnerViewModel?.TaijiPhMode;
            Log.Instance.WriteInfo("导服上传参数：" + JsonHelper.ToJson(dyUpload));
            //导服上传
            var result = taiJiHelper.Do_DY_Upload(dyUpload);

            if (OwnerViewModel.UnOverYwList.Contains(OwnerViewModel?.YWID))
                OwnerViewModel.UnOverYwList.Remove(OwnerViewModel?.YWID);
            Log.Instance.WriteInfo("删除未上传导引id：" + OwnerViewModel?.YWID);
            return result;

        }

        /// <summary>
        /// 上传默认制证照片
        /// </summary>
        /// <returns></returns>
        public bool UpLoadPhoto(int placeType, string url, out string msg)
        {
            msg = string.Empty;
            //北京上传默认制证照片
            if (placeType == 1)
            {
                string photoName = "defaultzzzp.png";
                string photoPath = Path.Combine(FileHelper.GetLocalPath(), photoName);

                if (File.Exists(photoPath))
                {
                    Log.Instance.WriteInfo("仅签注，上传默认制证照片defaultzzzp.png");
                    var img = CommandTools.ImgToBase64(photoPath);
                    //根据配置是否使用默认照片来上传回执编号
                    var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                        BookingBaseInfo?.CardInfo?.IDCardNo, QJTConfig.QJTModel.IsUseTestZp == true ? "DY12345678" : "",
                        QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                        BookingBaseInfo?.CardInfo?.IDCardNo + ".png",
                        img, img, null);
                    if (zzzpinfo.IsSucceed)
                    {
                        return true;
                    }
                    else
                    {
                        Log.Instance.WriteInfo(zzzpinfo.MessageInfo);
                        msg = zzzpinfo.MessageInfo;
                        return false;
                    }
                }
            }
            //武汉上传身份证照片
            if (placeType == 2)
            {
                Log.Instance.WriteInfo("武汉开始【上传身份证照片】");
                if (File.Exists(url))
                {
                    var img = CommandTools.ImgToBase64(url);
                    if(img.IsNotEmpty())
                        Log.Instance.WriteInfo("武汉身份证照片");
                    var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                        BookingBaseInfo?.CardInfo?.IDCardNo, QJTConfig.QJTModel.IsUseTestZp == true ? "DY12345678" : "",
                        QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                        BookingBaseInfo?.CardInfo?.IDCardNo + ".png",
                        img, img, null, 1);
                    if (zzzpinfo.IsSucceed)
                    {
                        Log.Instance.WriteInfo("武汉【上传身份证照片】【成功】");
                        return true;
                    }
                    else
                    {
                        Log.Instance.WriteInfo(zzzpinfo.MessageInfo);
                        msg = zzzpinfo.MessageInfo;
                        return false;
                    }
                }
            }
            //上传默认制证照片
            if (placeType == 3)
            {
                string photoName = "默认无照片.png";
                string photoPath = Path.Combine(FileHelper.GetLocalPath(), photoName);

                if (File.Exists(photoPath))
                {
                    Log.Instance.WriteInfo("上传制证照片：" + photoName);
                    var img = CommandTools.ImgToBase64(photoPath);
                    //根据配置是否使用默认照片来上传回执编号
                    var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                        BookingBaseInfo?.CardInfo?.IDCardNo, "12345678",
                        QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                        BookingBaseInfo?.CardInfo?.IDCardNo + ".png",
                        img, img, null);
                    if (zzzpinfo.IsSucceed)
                    {
                        return true;
                    }
                    else
                    {
                        Log.Instance.WriteInfo(zzzpinfo.MessageInfo);
                        msg = zzzpinfo.MessageInfo;
                        return false;
                    }
                }
            }
            return false;
        }

        public bool DoConnectingTest()
        {
            var iTimes = 0;
            while (!taiJiHelper.Common_searchYwlb(OwnerViewModel.DyLoginRec.dwid))
            {
                if (++iTimes >= 3)
                {
                    ZHPHMachineWSHelper.ZHPHInstance.WriteDeviceAlarm(EnumTypeALARMTYPEID.Fault22,
                        EnumTypeALARMCODE.ALARMCODE_20000001, "获取太极服务数据失败！");
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// 提交数据至预受理接口 [武汉]
        /// </summary>
        /// <returns></returns>
        public bool CommitApplys()
        {
            //身份证信息
            BasicInfo basicInfo = new BasicInfo
            {

            };
            //港澳台受理信息
            HkMaTwApplyInfo applyInfo = new HkMaTwApplyInfo
            {

            };
            PrevApplyInfo prevApplyInfo = new PrevApplyInfo
            {
                Iswsyy = BookingBaseInfo.Book_Type.ToBool(),
                IsKP = false,
                Xphzh = BookingBaseInfo.ReceiptsNo ?? "",
                Basicinfo = basicInfo,

                //Wsyydw = BookingBaseInfo.
            };

            //prevApplyInfo.ApplyType=BookingBaseInfo.BookingInfo

            var result = CommitApply(prevApplyInfo);
            if (result?.IsSucceed == true)
                return true;
            else
                return false;
        }


        /// <summary>
        /// 提交数据至预受理接口 [武汉]
        /// </summary>
        /// <param name="applyInfo"></param>
        /// <returns></returns>
        public ReturnInfo CommitApply(PrevApplyInfo applyInfo)
        {
            if (DjzConfig.DjzModel.IsConnectionDjz)
            {
                var reinfo = crjManager.CommitApply(applyInfo);
                return reinfo;
                //if (reinfo?.IsSucceed == true && reinfo?.ReturnValue != null)
                //{
                //    return reinfo;
                //}
                //else
                //{
                //    return
                //}
            }

            return null;
        }



        public override void TimeOutCallBackExcuted()
        {
            this.DoExit?.Execute(null);
        }
        #endregion
    }
}
