using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Freedom.BLL;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Command;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;

namespace Freedom.ZHPHMachine.ViewModels.Booking
{
    public class BookingBaseInfoByBeijingViewModel : ViewModelBase
    {
        #region 字段

        private CrjPreapplyManager crjManager;
        private List<DictionaryType> cardTypes;
        private List<DictionaryType> applyTypes;

        /// <summary>
        /// 办证类型
        /// </summary>
        public List<DictionaryType> CardTypes
        {
            get { return cardTypes; }
            set
            {
                cardTypes = value;
                RaisePropertyChanged("CardTypes");
            }
        }

        private string tipsMsg;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipsMsg
        {
            get { return tipsMsg; }
            set
            {
                tipsMsg = value;
                RaisePropertyChanged("TipsMsg");
            }
        }

        public string imageStr;

        /// <summary>
        /// 太极人口照片路径
        /// </summary>
        public string ImageStr
        {
            get { return imageStr; }
            set
            {
                imageStr = value;
                RaisePropertyChanged("ImageStr");
            }
        }


        public BookingInfo hZBookingInfo;

        /// <summary>
        /// 护照预约信息
        /// </summary>
        public BookingInfo HZBookingInfo
        {
            get { return hZBookingInfo; }
            set
            {
                hZBookingInfo = value;
                RaisePropertyChanged("HZBookingInfo");
            }
        }

        public BookingInfo tWBookingInfo;

        /// <summary>
        /// 台湾预约信息
        /// </summary>
        public BookingInfo TWBookingInfo
        {
            get { return tWBookingInfo; }
            set
            {
                tWBookingInfo = value;
                RaisePropertyChanged("TWBookingInfo");
            }
        }

        public BookingInfo gABookingInfo;

        /// <summary>
        /// 港澳预约信息
        /// </summary>
        public BookingInfo GABookingInfo
        {
            get { return gABookingInfo; }
            set
            {
                gABookingInfo = value;
                RaisePropertyChanged("GABookingInfo");
            }
        }

        #endregion

        Page currentPage = null;

        public BookingBaseInfoByBeijingViewModel(Object obj)
        {
            if (obj.IsNotEmpty())
            {
                currentPage = ((obj as Page));
            }
            //System.Windows.Controls.Image imgRK = currentPage.FindName("ImgRK")) as System.Windows.Controls.Image);
            //if (imgRK.Resources.IsNotEmpty())
            //{
            //    imgRK.Resources = null;
            //}

            Log.Instance.WriteInfo("\n**********************************进入【填写申请信息】界面**********************************");
            //OwnerViewModel.IsOfficial = "查询到国家工作人员信息，请移步人工窗口办理！";
            if (crjManager == null)
            {
                crjManager = new CrjPreapplyManager();
            }


            var result = ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>();
            CardTypes = result?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code)
                .ToList();
            applyTypes = GetApplyTypes();

            //TTS.PlaySound("完善个人基本信息");
            //if (!OwnerViewModel.CheckServeStatus())
            //{
            //    DoNext.Execute("NotificationPage");
            //}
            OwnerViewModel.HomeShow = Visibility.Visible;
            OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);

            //QueryAsync这里也是调用IsSkipBasicInfo，这个判断没意义。 by wei.chen
            Log.Instance.WriteInfo("预约来源：" + (BookingBaseInfo?.BookingSource == 1 ? "现场" : "网上"));
            //根据配置判断是否跳过基本信息页面
            IsSkipBasicInfo();
            //当没有信息时查询
            //if (BookingBaseInfo?.BookingSource == 1)
            //{
            //    QueryAsync();
            //}
            //else
            //{
            //    //根据配置判断是否跳过基本信息页面
            //    IsSkipBasicInfo();
            //}
        }

        /// <summary>
        /// 申请类型选择
        /// </summary>
        public ICommand TextBoxGotFocusCommand
        {
            get
            {
                return new RelayCommand<String>((param) =>
                {
                    if (int.TryParse(param, out int type))
                    {
                        string key = "";
                        switch (type)
                        {
                            case 1:
                                key = HZBookingInfo?.ApplyType?.Description;
                                break;
                            case 2:
                                key = GABookingInfo?.ApplyType?.Description;
                                break;
                            case 3:
                                key = TWBookingInfo?.ApplyType?.Description;
                                break;
                        }

                        if (CommonHelper.PopupDictionary(OperaType.ApplyType, key, out DictionaryType model, null,
                            type))
                        {
                            //OwnerViewModel?.KbywInfos
                            string msg = "";
                            switch (type)
                            {
                                case 1:
                                    HZBookingInfo.ApplyType = model;
                                    DataVerificate(EnumTypeSQLB.HZ, HZBookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        HZBookingInfo.ApplyType = null;
                                    }

                                    break;
                                case 2:
                                    GABookingInfo.ApplyType = model;
                                    DataVerificate(EnumTypeSQLB.HKGMAC, GABookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        GABookingInfo.ApplyType = null;
                                    }

                                    break;
                                case 3:
                                    TWBookingInfo.ApplyType = model;
                                    DataVerificate(EnumTypeSQLB.TWN, TWBookingInfo, out msg);
                                    if (TipMsg(msg))
                                    {
                                        TWBookingInfo.ApplyType = null;
                                    }

                                    break;
                            }
                        }
                    }

                });
            }
        }

        private void QueryAsync()
        {
            IsSkipBasicInfo();
            //OwnerViewModel?.IsShowHiddenLoadingWait("正在查询请稍等......");
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        if (!DjzConfig.DjzModel.IsConnectionDjz)
            //            return;

            //        IsSkipBasicInfo();
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Instance.WriteInfo($"[申请信息异常]");
            //        Log.Instance.WriteError($"[申请信息异常]{ex.Message}");
            //    }
            //    finally
            //    {
            //        //Log.Instance.WriteInfo("-----结束查询历史申请信息-----");

            //        //根据配置判断是否跳过基本信息页面
            //        OwnerViewModel?.IsShowHiddenLoadingWait();
            //    }
            //});

        }

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }


        public override void DoNextFunction(object obj)
        {

            Log.Instance.WriteInfo("点击【确认按钮】");
            //if (OwnerViewModel?.CheckServeStatus() == false)
            //{
            //    OwnerViewModel?.MessageTips("连接网络服务失败！请重试！", (() =>
            //    {
            //        DoNext.Execute("NotificationPage");
            //    }));

            //}
            //if (BookingBaseInfo.IsEmpty())
            //{
            //    OwnerViewModel?.MessageTips("证件信息查询发生错误！请重试！", (() =>
            //    {
            //        this.DoExit.Execute(null);
            //    }));
            //    return;
            //}
            var url = "ScanningPhotoReceipt";
            try
            {
                StopTimeOut();

                //数据校验处理
                if (!DataVerification(BookingBaseInfo, out string msg) && OwnerViewModel.IsTakePH_No == false) // && OwnerViewModel?.IsBeijing == false
                {
                    //Log.Instance.WriteInfo("数据校验结果：" + msg);
                    //OwnerViewModel?.MessageTips(msg);
                    //return;

                    //换弹出页面 by 2021年7月8日16:55:07 wei.chen
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ReturnInfo showReturn = MainWindowViewModels.Instance.ShowMsgReturnDialog("数据校验结果：" + msg, 30, true);
                        if (showReturn.IsNotEmpty() && showReturn.IsSucceed)
                        {
                            OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);//恢复计时器 by wei.chen
                        }
                    }));
                    return;
                }

                //北京户口默认值
                if (OwnerViewModel?.IsBeijing == true && BookingBaseInfo.Address == null ||
                    OwnerViewModel?.IsBeijing == true && string.IsNullOrWhiteSpace(BookingBaseInfo?.Address?.Code))
                {
                    BookingBaseInfo.Address = new DictionaryType()
                    {
                        Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                    };
                }

                //初始化预约数据
                if (!ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(BookingBaseInfo, out string msg1))
                {
                    Log.Instance.WriteInfo("初始化预约数据结果：" + msg1);
                    OwnerViewModel?.MessageTips(msg1);
                    return;
                }

                BookingBaseInfo.StrCardTypes = msg1.Substring(0, msg1.Length - 1);


                //北京存在制证照片，跳过扫描回执页面
                if (OwnerViewModel?.IsDirectNumber == true ||
                    BookingBaseInfo.ReceiptsNo.IsNotEmpty() &&
                    BookingBaseInfo.ReceiptsNo != "DY12345678")
                    url = "Booking/BookingArea";
                Log.Instance.WriteInfo("准备跳转到：" + url);
                base.DoNextFunction(url);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("操作确认无误发生异常：" + ex.Message);
                Log.Instance.WriteError("操作确认无误发生异常：" + ex.Message);

            }
            finally
            {
                //释放人口照片这样写没办法删除，放到了刷身份页核查后删除了 2021年7月12日13:21:20 by wei.chen
                //System.Windows.Controls.Image imgRK = (currentPage.FindName("ImgRK")) as System.Windows.Controls.Image;
                //if (imgRK.Source.IsNotEmpty())
                //{
                //    imgRK.Source = null;
                //    imgRK = null;
                //}

                Log.Instance.WriteInfo("\n**********************************离开【填写申请信息】界面**********************************");
            }

        }

        /// <summary>
        /// （下一步）数据校验
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool DataVerification(BookingModel model, out string msg)
        {
            msg = "";
            if (model?.SelectCardTypes == null || model?.SelectCardTypes?.Count <= 0)
            {
                TTS.PlaySound("预约机-提示-请选择办证类型");
                msg = TipMsgResource.NonEmptySelectsTipMsg;
                return false;
            }

            //选中总数
            var selCount = model.SelectCardTypes.Count;
            foreach (DictionaryType item in BookingBaseInfo.SelectCardTypes)
            {
                //加了trycatch 日志 by wei.chen
                try
                {
                    if (HZBookingInfo != null && item.Code.Contains(HZBookingInfo.SQLB))
                    {
                        //无证件，默认首次办理
                        if (HZBookingInfo.ApplyType == null)
                        {
                            //修改 申请类型为空默认可办第一个
                            //TTS.PlaySound("预约机-提示-请完善办证类别信息");
                            //msg = string.Format(TipMsgResource.NonEmptyTipMsg, "普通护照中申请类型");
                            //return false;
                            var hz = OwnerViewModel?.KbywInfos?.Where(t => t.sqlb == HZBookingInfo.SQLB)?.ToList();
                            if (hz?.Count > 0)
                            {
                                var Bzlb = hz.FirstOrDefault()?.bzlb;
                                HZBookingInfo.ApplyType = GetApplyTypes()?.FirstOrDefault(t => t.Code == Bzlb);
                            }
                        }

                        if (string.IsNullOrEmpty(HZBookingInfo?.CJSY))
                        {
                            HZBookingInfo.CJSY = "19";
                            HZBookingInfo.CJSYDescription = new DictionaryType() { Code = "19", Description = "旅游", KindType = "102" };
                        }

                        //护照申请类别描述
                        //msg += $" [护照] 申请类型:{HZBookingInfo?.ApplyType?.Description}";
                        //护照加注判断
                        if (HZBookingInfo.ApplyType != null && HZBookingInfo.ApplyType?.Code == "93")
                        {

                            string ywbh = "";

                            HZBookingInfo.YYJZBLLIST = new List<PassportNote>();
                            HZBookingInfo.YYJZBLLIST.Add(new PassportNote()
                            {
                                XH = 1,
                                YWBH = ywbh,
                                ZJZL = HZBookingInfo.XCZJZL?.ToString(),
                                ZJHM = HZBookingInfo.XCZJHM?.ToString(),
                                BGJZZL = "1A",
                                BGJZXM = HZBookingInfo.XCZJHM?.ToString(),
                                TBDWBH = QJTConfig.QJTModel?.QJTDevInfo?.TBDWBH
                            });
                            //}

                        }
                        else
                        {
                            //非持证加注清空加注信息
                            HZBookingInfo.YYJZBLLIST.Clear();
                        }
                        Log.Instance.WriteInfo("判断选中【护照】业务，证件号码：" + CommandTools.ReplaceWithSpecialChar(HZBookingInfo.XCZJHM) + "，是否有效：" + HZBookingInfo.DJSY + ",申请类型：" + HZBookingInfo?.ApplyType?.Description + "");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo("护照数据校验发生异常：" + ex.Message);
                    Log.Instance.WriteError("护照数据校验发生异常：" + ex.Message);
                }

                try
                {
                    if (GABookingInfo != null && item.Code.Contains(GABookingInfo?.SQLB))
                    {
                        if (GABookingInfo != null)
                        {
                            if (GABookingInfo.ApplyType == null)
                            {
                                //TTS.PlaySound("预约机-提示-请完善办证类别信息");
                                //msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来港澳通行证中办证类别");
                                //return false;
                                var ga = OwnerViewModel?.KbywInfos?.Where(t => t.sqlb == GABookingInfo?.SQLB)?.ToList();
                                if (ga?.Count > 0)
                                {
                                    //2021-5-11 13:46:33 修改：【遗失补发】默认可办业务为【到期换发】
                                    var Bzlb = ga.FirstOrDefault(t => t.bzlb.Contains("31"))?.bzlb;
                                    //到期换发不在可办业务，默认第一条可办
                                    if (Bzlb.Length <= 0)
                                        Bzlb = ga.FirstOrDefault()?.bzlb;

                                    GABookingInfo.ApplyType = GetApplyTypes()?.FirstOrDefault(t => t.Code == Bzlb);
                                }
                            }
                        }

                        //港澳通行证申请类别描述
                        //msg += $" [香港+澳门] 办证类别:{GABookingInfo?.ApplyType?.Description}";
                        Log.Instance.WriteInfo("选中【港澳】业务，证件号码：" + CommandTools.ReplaceWithSpecialChar(GABookingInfo.XCZJHM) + "，是否有效：" + GABookingInfo.DJSY + ",申请类型：" + GABookingInfo?.ApplyType?.Description + "");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo("港澳数据校验发生异常：" + ex.Message);
                    Log.Instance.WriteError("港澳数据校验发生异常：" + ex.Message);
                }

                try
                {
                    if (TWBookingInfo != null && item.Code.Contains(TWBookingInfo.SQLB))
                    {
                        if (TWBookingInfo.ApplyType == null)
                        {
                            //TTS.PlaySound("预约机-提示-请完善办证类别信息");
                            //msg = string.Format(TipMsgResource.NonEmptyTipMsg, "往来台湾通行证中申请类型");
                            //return false;
                            var tw = OwnerViewModel.KbywInfos?.Where(t => t.sqlb == TWBookingInfo?.SQLB)?.ToList();
                            if (tw?.Count > 0)
                            {
                                //2021-5-11 13:46:33 修改：【遗失补发】默认可办业务为【到期换发】
                                var Bzlb = tw.FirstOrDefault(t => t.bzlb.Contains("31"))?.bzlb;
                                //到期换发不在可办业务，默认第一条可办
                                if (Bzlb.Length <= 0)
                                    Bzlb = tw.FirstOrDefault()?.bzlb;
                                //var Bzlb = tw.FirstOrDefault()?.bzlb;
                                TWBookingInfo.ApplyType = GetApplyTypes()?.FirstOrDefault(t => t.Code == Bzlb);
                            }
                        }

                        //台湾申请类别描述
                        //var t = TWBookingInfo.YYQZLIST[0];
                        //msg += $" [台湾] 办证类别:{TWBookingInfo?.ApplyType?.Description}";
                        Log.Instance.WriteInfo("选中【赴台】业务，证件号码：" + CommandTools.ReplaceWithSpecialChar(TWBookingInfo.XCZJHM) + "，是否有效：" + TWBookingInfo.DJSY + ",申请类型：" + TWBookingInfo?.ApplyType?.Description + "");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo("赴台数据校验发生异常：" + ex.Message);
                    Log.Instance.WriteError("赴台数据校验发生异常：" + ex.Message);
                }

            }

            Log.Instance.WriteInfo("仅签注业务总数为：" + OwnerViewModel?.IsDqzType);
            //所有选择的都是首次 人工号标识为1
            int firstCount = 0;

            //跳过回执页标识 
            //仅签注：仅持证申请签注 显示仅签注按钮
            //首次 换发 补发 隐藏按钮
            int DirectCount = 0;

            foreach (DictionaryType item in model.SelectCardTypes)
            {
                //首次办理业务
                var firstVerification = model.BookingInfo?.Where(t => t.SQLB == item?.Code && t.ApplyType?.Code == "11")?.ToList();
                //仅签注业务
                var directVerification = model.BookingInfo?.Where(t =>
                    t.SQLB == item?.Code && t.SQLB != ((int)EnumSqlType.HZ).ToString())?.ToList();
                //首次业务总数
                if (firstVerification?.Count > 0)
                    firstCount++;
                //仅签注业务总数
                if (directVerification?.Count > 0)
                    DirectCount++;
            }
            //全部为首次办理
            if (selCount == firstCount)
            {
                OwnerViewModel.isVALIDXCYY = "1";
                Log.Instance.WriteInfo("系统查询到选择都为首次办理，区域参数isVALIDXCYY修改为1");
            }
            //全部为仅签注业务
            if (selCount == DirectCount && DirectCount == OwnerViewModel.IsDqzType)
            {
                OwnerViewModel.IsDirectNumber = true;
                BookingBaseInfo.ReceiptsNo = "DY12345678";
                Log.Instance.WriteInfo("系统查询办证类型与仅签注业务对应，跳过扫描回执页。");
                Log.Instance.WriteInfo("仅签注【上传默认制证照片】默认回执编号：【" + BookingBaseInfo.ReceiptsNo + "】");

            }

            return true;
        }
        public BitmapImage GetImage(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            if (File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            return bitmap;
        }
        /// <summary>
        /// 根据区域和取号模式显示跳转页面
        /// </summary>
        public void IsSkipBasicInfo()
        {
            Log.Instance.WriteInfo(OwnerViewModel?.IsTakePH_No == true ? "取号模式：直接取号" : "取号模式：现场预约+取号");
            if (!string.IsNullOrEmpty(OwnerViewModel?.IsOfficial))
            {
                TipsMsg = OwnerViewModel?.IsOfficial;
            }

            //TipsMsg = "普通群众 (国家工作人员)";
            //显示照片
            if (!string.IsNullOrWhiteSpace(OwnerViewModel?.djzPhoto))
            {
                // ImageStr = OwnerViewModel?.djzPhoto;
                System.Windows.Controls.Image imgRK = (currentPage.FindName("ImgRK")) as System.Windows.Controls.Image;
                if (imgRK.IsNotEmpty())
                {
                    //使用时直接通过调用此方法获得Image后立马释放掉资源
                    imgRK.Source = GetImage(OwnerViewModel?.djzPhoto);     // path为图片路径
                }
            }
            Log.Instance.WriteInfo("《《《《《《默认预约指标数据》》》》》》》");
            //北京地区默认预约当天指标
            if (GetTargetTime())
            {
                Log.Instance.WriteInfo(BookingBaseInfo.BookingTarget?.BookingDate + " " + BookingBaseInfo.BookingTarget?.StartTime?.ToString() + "   " + BookingBaseInfo.BookingTarget?.EndTime?.ToString());

            }

            Log.Instance.WriteInfo("筛选在办业务【" + OwnerViewModel?.zbywxx?.Length + "】");
            //在办业务 在办理类型里删除
            if (OwnerViewModel?.zbywxx != null)
            {
                CardTypes = GetSQTypes();
                //Log.Instance.WriteInfo("核查在办业务不为空：");
                List<string> zbyw = new List<string>(OwnerViewModel?.zbywxx);
                if (OwnerViewModel?.zbywxx != null && zbyw.Count > 0)
                {
                    foreach (var item in zbyw)
                    {
                        Log.Instance.WriteInfo("核查在办业务：" + item);
                        var UnOver = CardTypes.Where(t => t.Code == item).ToList()[0];
                        CardTypes.Remove(UnOver);
                        //如果预约在办业务 删除预约业务
                        if (BookingBaseInfo.SelectCardTypes != null && BookingBaseInfo.SelectCardTypes.Count > 0 &&
                            BookingBaseInfo.SelectCardTypes.Contains(UnOver))
                        {
                            Log.Instance.WriteInfo("核查到预约存在在办业务：" + item);
                            BookingBaseInfo.SelectCardTypes.Remove(UnOver);
                        }

                    }

                    if (CardTypes.Count == 0)
                    {
                        //OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！");
                        //改为弹出直接退出回首页 by 2021年7月7日15:35:24 wei.chen
                        Log.Instance.WriteInfo("IsSkipBasicInfo");
                        OwnerViewModel?.MessageTips("系统查询到您已办理业务【护照、港澳、赴台】，不能重复办理，请您再次核对，若有疑问，请移步人工窗口咨询！", (() =>
                        {
                            //提示框内的倒计时卡死 by 2021年7月10日18:37:30
                            this.DoNextFunction(null);
                        }), null, 10, "返回");
                    }
                }
            }

            //修改北京外网预约 删除非预约业务
            if (BookingBaseInfo?.BookingSource == 0 && OwnerViewModel?._hasYYXX.Count > 0 && BookingBaseInfo?.Book_Type == "0") //OwnerViewModel?.IsBeijingRegister == false && OwnerViewModel?.IsBeijingRegister == false
            {
                List<DictionaryType> carTypesCope = new List<DictionaryType>();
                //if (OwnerViewModel?._hasYYXX.Count > 0 && BookingBaseInfo?.Book_Type == "0")//修改 非网上预约的
                //{
                var cardtype = CardTypes?.Select(s => s.Code).ToList(); //可办业务
                var hasyyxx = OwnerViewModel?._hasYYXX.Select(t => t).ToList(); //预约业务
                foreach (var item in cardtype)
                {
                    //var a = unyyTypes.Where(t => t.Code != item).ToList();
                    if (hasyyxx.Contains(item))
                    {
                        var yyTypes = CardTypes.Where(t => t.Code == item).ToList()[0];
                        //预约业务
                        carTypesCope.Add(yyTypes);
                    }
                }

                //北京本地。预约业务已经办理，则可以办理非预约业务
                if (OwnerViewModel?.zbywxx != null && hasyyxx?.Count > 0)
                {
                    foreach (var zbyw in OwnerViewModel?.zbywxx)
                    {
                        var bjyw = hasyyxx.Where(t => t == zbyw).ToList().FirstOrDefault();//办结业务  预约业务=在办业务
                        if (!string.IsNullOrEmpty(bjyw))
                        {
                            if (carTypesCope.Count == 0)
                            {
                                BookingBaseInfo.Book_Type = "1";
                                BookingBaseInfo.BookingSource = 1;
                                BookingBaseInfo.IsExpress = 0;
                                BookingBaseInfo.RecipientName = null;
                                BookingBaseInfo.RecipientTelephone = null;
                                BookingBaseInfo.EMSCode = null;
                                BookingBaseInfo.EMSAddress = null;
                                OwnerViewModel.Xxly = "00";//00现场预约 99移民局  15公安
                                //预约业务非在办 ，则显示所有可办业务
                                carTypesCope = CardTypes;
                            }

                        }
                        //var UnOver = CardTypes.Where(t => t.Code == bjyw).ToList()[0];
                        //CardTypes.Remove(UnOver);
                    }
                }
                CardTypes = carTypesCope;
            }

            try
            {
                Log.Instance.WriteInfo("《《《《《《默认初始化数据》》》》》》》");
                StateInitializationAsync();
                //从配置项获取：取号模式为直接取号
                if (!QJTConfig.QJTModel.IsBasicInfo && OwnerViewModel?.IsTakePH_No == true)
                {
                    if (BookingBaseInfo.Address == null || string.IsNullOrWhiteSpace(BookingBaseInfo.Address?.Code))
                    {
                        BookingBaseInfo.Address = new DictionaryType()
                        {
                            Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                        };
                    }

                    if (BookingBaseInfo?.SelectCardTypes == null || BookingBaseInfo?.SelectCardTypes.Count <= 0)
                    {
                        BookingBaseInfo.SelectCardTypes = CardTypes;
                    }

                    Log.Instance.WriteInfo("直接取号模式跳过基本信息填写");
                    this.DoNextFunction(null);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("StateInitializationAsync发生异常：" + ex.Message);
                Log.Instance.WriteError("StateInitializationAsync发生异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void StateInitializationAsync()
        {
            //加try+日志 by wei.chen
            if (BookingBaseInfo.SelectCardTypes == null)
            {
                BookingBaseInfo.SelectCardTypes = BookingBaseInfo?.BookingSource == 1 ? null : CardTypes;
                Log.Instance.WriteInfo(BookingBaseInfo?.BookingSource + " 来源申请类型有：" + CardTypes?.Count + " 笔 ");
            }

            //Log.Instance.WriteInfo("初始化办证类型成功！！！" + BookingBaseInfo.SelectCardTypes?.Count);
            try
            {
                if (OwnerViewModel?.IsBeijing == true && BookingBaseInfo.Address == null ||
                    OwnerViewModel?.IsBeijing == true && string.IsNullOrWhiteSpace(BookingBaseInfo?.Address?.Code))
                {
                    BookingBaseInfo.Address = new DictionaryType()
                    {
                        Code = BookingBaseInfo.CardInfo.IDCardNo.Substring(0, 6)
                    };
                }

                //申请类型初始化
                string msgstr = "";
                if (BookingBaseInfo.SelectCardTypes.IsNotEmpty())
                    ZHPHMachineWSHelper.ZHPHInstance.CreateBookingInfo(BookingBaseInfo, out msgstr);

                //Log.Instance.WriteInfo("初始化办证类型成功！！！" + BookingBaseInfo.SelectCardTypes?.Count);
                //初始化证件
                //初始化状态
                HZBookingInfo = TWBookingInfo = GABookingInfo = null;
                if (BookingBaseInfo?.BookingInfo != null && BookingBaseInfo.BookingInfo?.Count > 0)
                {
                    //有预约信息且在使用状态中
                    HZBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.HZ).ToString()) && t.IsUse);
                    TWBookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.TWN).ToString()) && t.IsUse);
                    GABookingInfo = BookingBaseInfo.BookingInfo.FirstOrDefault(t =>
                        t.SQLB.Equals(((int)EnumTypeSQLB.HKGMAC).ToString()) && t.IsUse);
                    //Log.Instance.WriteInfo("初始化证件状态成功！！！");
                }

                if (HZBookingInfo != null && HZBookingInfo.ApplyType == null)
                {
                    Log.Instance.WriteInfo("开始初始化【护照】数据：");
                    //是否启用太极接口查询
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.HZ);
                        if (paperWorkTask != null)
                        {
                            HZBookingInfo.XCZJZL = paperWorkTask.zjzl;
                            HZBookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //HZBookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            HZBookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            HZBookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            HZBookingInfo.DJSY = paperWorkTask?.zjzt;
                            HZBookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;
                            //更新护照加注信息
                            foreach (var item in HZBookingInfo.YYJZBLLIST)
                            {
                                item.ZJHM = paperWorkTask.zjhm;
                            }
                        }
                        else
                        {
                            if (OwnerViewModel?.PaperInfos != null && OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var HZPaperInfo = OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.HZ.GetHashCode().ToString()).ToList();
                                if (HZPaperInfo != null && HZPaperInfo.Count > 0)
                                {
                                    HZBookingInfo.XCZJYXQZ = HZPaperInfo.FirstOrDefault()?.zjyxqz;
                                    HZBookingInfo.XCZJHM = HZPaperInfo.FirstOrDefault()?.zjhm;
                                    HZBookingInfo.XCZJZL = HZPaperInfo.FirstOrDefault()?.zjzl;
                                    HZBookingInfo.DJSY = HZPaperInfo.FirstOrDefault()?.zjzt;

                                    foreach (var i in HZBookingInfo.YYJZBLLIST)
                                    {
                                        i.ZJHM = HZPaperInfo.FirstOrDefault()?.zjhm;
                                    }
                                }

                            }
                        }
                    }
                    //无预约默认第一个可办业务办证类别
                    if (OwnerViewModel?.KbywInfos != null && OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Hz = OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.HZ).ToString()).ToList();
                        string Bzlb = string.Empty;
                        //外网预约存在预约信息
                        //1.预约办证类别重新赋值  
                        //2.预约加注信息赋值
                        if (BookingBaseInfo.Book_Type == "0" &&
                            OwnerViewModel.YyywInfo != null &&
                            OwnerViewModel.YyywInfo?.Length > 0)
                        {
                            foreach (var item in OwnerViewModel.YyywInfo)
                            {
                                if (item.sqlb == ((int)EnumTypeSQLB.HZ).ToString())
                                {
                                    Bzlb = item?.bzlb;
                                    Log.Instance.WriteInfo("预约办理：" + item?.bzlb);
                                    //预约信息不在可办业务范围里，默认可以办业务第一条
                                    if (Hz.Where(t => t.bzlb.Contains(Bzlb))?.ToList()?.Count == 0)
                                    {
                                        Bzlb = Hz.FirstOrDefault().bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                    }
                                    foreach (var i in HZBookingInfo?.YYJZBLLIST)
                                    {
                                        i.YWBH = HZBookingInfo?.YWBH;
                                        i.BGJZZL = item?.jzxxs?.FirstOrDefault()?.jzzl;
                                        i.BGJZXM = item?.jzxxs?.FirstOrDefault()?.jznr;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Bzlb = Hz.FirstOrDefault().bzlb;
                            Log.Instance.WriteInfo("默认【护照】办证类型：" + Bzlb);
                        }
                        HZBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);

                    }

                    //北京地区有预约默认办证类别
                    //if (OwnerViewModel?._HasBZLB.Count > 0 && OwnerViewModel._HasBZLB.ContainsKey(101))
                    //{
                    //    //HZBookingInfo.ApplyType.Code = OwnerViewModel._HasBZLB[101];
                    //    if (OwnerViewModel._HasBZLB.ContainsKey(101))
                    //    {
                    //        var HZtypes = GetApplyTypes().Where(t => t.Code == OwnerViewModel._HasBZLB[101]).ToList()[0];
                    //        HZBookingInfo.ApplyType = HZtypes;

                    //    }

                    //}
                    //根据护照信息，给护照签注默认值
                    DataVerificate(EnumTypeSQLB.HZ, HZBookingInfo, out string msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                        HZBookingInfo.ApplyType = null;


                }

                if (GABookingInfo != null && GABookingInfo.ApplyType == null)
                {
                    //存在港澳信息
                    //获取港澳加注信息
                    //var itemHK = GABookingInfo.YYQZLIST[0];
                    //var itemMAC = GABookingInfo.YYQZLIST[1];

                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        Log.Instance.WriteInfo("开始初始化【港澳】数据：");
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.GA);
                        if (paperWorkTask != null)
                        {
                            GABookingInfo.XCZJZL = paperWorkTask.zjzl;
                            GABookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //GABookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            GABookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            GABookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            GABookingInfo.DJSY = paperWorkTask.zjzt;
                            GABookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;
                        }
                        else
                        {
                            if (OwnerViewModel?.PaperInfos != null && OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var GAPaperInfo = OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.GA.GetHashCode().ToString()).ToList();
                                if (GAPaperInfo != null && GAPaperInfo.Count > 0)
                                {
                                    GABookingInfo.XCZJYXQZ = GAPaperInfo.FirstOrDefault().zjyxqz;
                                    GABookingInfo.XCZJHM = GAPaperInfo.FirstOrDefault().zjhm;
                                    GABookingInfo.XCZJZL = GAPaperInfo.FirstOrDefault().zjzl;
                                    GABookingInfo.DJSY = GAPaperInfo.FirstOrDefault().zjzt;
                                }

                            }
                        }
                    }

                    //无预约默认第一个可办业务办证类别
                    if (OwnerViewModel?.KbywInfos != null && OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Ga = OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.HKGMAC).ToString()).ToList();
                        if (Ga.Count > 0)
                        {
                            string Bzlb = string.Empty;
                            //外网预约存在预约信息
                            //1.预约办证类别重新赋值  
                            //2.预约加注信息赋值
                            if (BookingBaseInfo.Book_Type == "0" &&
                                OwnerViewModel.YyywInfo != null &&
                                OwnerViewModel.YyywInfo?.Length > 0)
                            {
                                foreach (var item in OwnerViewModel.YyywInfo)
                                {

                                    //仅香港签注或澳门签注
                                    if (item.qzxxs.Length == 1 && GABookingInfo?.YYQZLIST.Count > 1)
                                    {
                                        GABookingInfo?.YYQZLIST.Remove(GABookingInfo?.YYQZLIST.Where(t => t.XH == 2)
                                            .ToList().FirstOrDefault());
                                    }

                                    if (item.sqlb == ((int)EnumTypeSQLB.HKGMAC).ToString())
                                    {
                                        Bzlb = item?.bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb);
                                        //预约信息不在可办业务范围里，默认可以办业务第一条
                                        if (Ga.Where(t => t.bzlb.Contains(Bzlb))?.ToList().Count == 0)
                                        {
                                            Bzlb = Ga.FirstOrDefault().bzlb;
                                            Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                        }
                                        if (item.qzxxs.Length > 0)
                                        {
                                            foreach (var t in item.qzxxs)
                                            {
                                                if (t.yxq.IsEmpty())
                                                    t.yxq = "Y03";

                                                foreach (var i in GABookingInfo?.YYQZLIST)
                                                {
                                                    if (i.QWD == t.qwd || item.qzxxs.Length == 1)
                                                    {
                                                        i.YWBH = GABookingInfo?.YWBH;
                                                        i.QZZL = t?.qzzl.IsEmpty() == true ? "1B" : t?.qzzl;
                                                        i.QZType = GetQZTypes(t?.qzzl).FirstOrDefault();
                                                        i.QWD = t?.qwd;
                                                        i.QZYXCS = t?.yxcs;
                                                        i.QZYXQ = ConverYxq(t?.yxq);//太极有效期转换
                                                        i.QZYXQDW = GetYxqDw(t?.yxq);//年；月；周；日
                                                                                     //i.QZType = GetQZTypes(item?.qzxxs?.FirstOrDefault()?.qzzl).FirstOrDefault();
                                                                                     //i.QWD = item?.qzxxs?.FirstOrDefault()?.qwd;
                                                                                     //i.QZYXCS = item?.qzxxs?.FirstOrDefault()?.yxcs;
                                                                                     //i.QZZL = item?.qzxxs?.FirstOrDefault()?.qzzl;
                                                                                     //i.QZYXQ = ConverYxq(item?.qzxxs?.FirstOrDefault()?.yxq);//太极有效期转换
                                                                                     //i.QZYXQDW = GetYxqDw(item?.qzxxs?.FirstOrDefault()?.yxq);//年；月；周；日
                                                        i.ZJHM = GABookingInfo.XCZJHM;
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            GABookingInfo.YYQZLIST = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Bzlb = Ga.FirstOrDefault().bzlb;
                                Log.Instance.WriteInfo("默认【港澳】办证类型：" + Bzlb);
                            }

                            GABookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);
                        }
                    }
                    //北京地区默认办证类别
                    //if (OwnerViewModel?._HasBZLB.Count > 0 && OwnerViewModel._HasBZLB.ContainsKey(102))
                    //{
                    //    //HZBookingInfo.ApplyType.Code = OwnerViewModel._HasBZLB[101];
                    //    if (OwnerViewModel._HasBZLB.ContainsKey(102))
                    //    {
                    //        var HZtypes = GetApplyTypes().Where(t => t.Code == OwnerViewModel._HasBZLB[102]).ToList()[0];
                    //        GABookingInfo.ApplyType = HZtypes;
                    //    }

                    //}
                    //根据港澳信息，给港澳签注默认值
                    DataVerificate(EnumTypeSQLB.HKGMAC, GABookingInfo, out string msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                        GABookingInfo.ApplyType = null;
                }

                if (TWBookingInfo != null && TWBookingInfo.ApplyType == null)
                {
                    //存在台湾预约信息
                    //获取加注信息
                    //var item = TWBookingInfo.YYQZLIST[0];
                    //是否启用太极接口查询
                    if (TjConfig.TjModel.IsConnectionTj)
                    {
                        Log.Instance.WriteInfo("开始初始化【赴台】数据：");
                        var paperWorkTask = QueryPaperWorkTask(DocumentType.TW);
                        if (paperWorkTask != null)
                        {
                            TWBookingInfo.XCZJZL = paperWorkTask.zjzl;
                            TWBookingInfo.XCZJHM = paperWorkTask.zjhm;
                            //TWBookingInfo.XCZJQFRQ = paperWorkTask.zjyxqz;
                            TWBookingInfo.XCZJYXQZ = paperWorkTask.zjyxqz;
                            TWBookingInfo.XCZJQFRQ = paperWorkTask?.zjqfrq;
                            TWBookingInfo.DJSY = paperWorkTask?.zjzt;
                            TWBookingInfo.ZJSXYY = paperWorkTask?.zjsxyy;
                            //更新护照加注信息
                            //foreach (var items in TWBookingInfo.YYJZBLLIST)
                            //{
                            //    items.ZJHM = paperWorkTask.zjhm;
                            //}
                        }
                        else
                        {   //平台证件信息
                            if (OwnerViewModel?.PaperInfos != null && OwnerViewModel?.PaperInfos?.Count > 0)
                            {
                                var TWPaperInfo = OwnerViewModel?.PaperInfos.Where(t => t.zjzl == DocumentType.TW.GetHashCode().ToString()).ToList();
                                if (TWPaperInfo != null && TWPaperInfo.Count > 0)
                                {
                                    TWBookingInfo.XCZJYXQZ = TWPaperInfo.FirstOrDefault().zjyxqz;
                                    TWBookingInfo.XCZJHM = TWPaperInfo.FirstOrDefault().zjhm;
                                    TWBookingInfo.XCZJZL = TWPaperInfo.FirstOrDefault().zjzl;
                                    TWBookingInfo.DJSY = TWPaperInfo.FirstOrDefault()?.zjzt;
                                    //foreach (var i in TWBookingInfo.YYJZBLLIST)
                                    //{
                                    //    i.ZJHM = TWPaperInfo.FirstOrDefault().zjhm;
                                    //}
                                }

                            }
                        }
                    }

                    //无预约默认第一个可办业务办证类别
                    if (OwnerViewModel?.KbywInfos != null && OwnerViewModel?.KbywInfos.Length > 0)
                    {
                        var Hz = OwnerViewModel.KbywInfos.Where(t => t.sqlb == ((int)EnumTypeSQLB.TWN).ToString()).ToList();
                        if (Hz.Count > 0)
                        {
                            string Bzlb = string.Empty;
                            //外网预约存在预约信息
                            //1.预约办证类别重新赋值  
                            //2.预约加注信息赋值
                            if (BookingBaseInfo.Book_Type == "0" &&
                                OwnerViewModel.YyywInfo != null &&
                                OwnerViewModel.YyywInfo?.Length > 0)
                            {
                                foreach (var item in OwnerViewModel.YyywInfo)
                                {
                                    if (item.sqlb == ((int)EnumTypeSQLB.TWN).ToString())
                                    {

                                        Bzlb = item?.bzlb;
                                        Log.Instance.WriteInfo("预约办理：" + item?.bzlb);
                                        //预约信息不在可办业务范围里，默认可以办业务第一条
                                        if (Hz.Where(t => t.bzlb.Contains(Bzlb))?.ToList()?.Count == 0)
                                        {
                                            Bzlb = Hz.FirstOrDefault().bzlb;
                                            Log.Instance.WriteInfo("预约办理：" + item?.bzlb + "，不在可办业务范围内。默认修改为：" + Bzlb);
                                        }
                                        if (item.qzxxs.Length > 0)
                                        {
                                            if (item?.qzxxs?.FirstOrDefault()?.yxq?.IsEmpty() == true)
                                                item.qzxxs.FirstOrDefault().yxq = "Y06";//无有效期默认6月一次
                                            foreach (var i in TWBookingInfo?.YYQZLIST)
                                            {
                                                i.YWBH = TWBookingInfo?.YWBH;
                                                i.QZZL = item?.qzxxs?.FirstOrDefault()?.qzzl?.IsEmpty() == true ? "25" : item?.qzxxs?.FirstOrDefault()?.qzzl;
                                                i.QZType = GetQZTypes(item?.qzxxs?.FirstOrDefault()?.qzzl).FirstOrDefault();
                                                i.QWD = item?.qzxxs?.FirstOrDefault()?.qwd;
                                                i.QZYXCS = item?.qzxxs?.FirstOrDefault()?.yxcs;

                                                i.QZYXQ = ConverYxq(item?.qzxxs?.FirstOrDefault()?.yxq);//太极有效期转换
                                                i.QZYXQDW = GetYxqDw(item?.qzxxs?.FirstOrDefault()?.yxq);//年；月；周；日
                                                i.ZJHM = TWBookingInfo?.XCZJHM;
                                            }
                                        }
                                        else
                                        {
                                            TWBookingInfo.YYQZLIST = null;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                Bzlb = Hz.FirstOrDefault().bzlb;
                                Log.Instance.WriteInfo("默认【港澳】办证类型：" + Bzlb);
                            }

                            TWBookingInfo.ApplyType = GetApplyTypes().FirstOrDefault(t => t.Code == Bzlb);
                        }
                    }
                    //北京地区默认办证类别
                    //if (OwnerViewModel?._HasBZLB.Count > 0 && OwnerViewModel._HasBZLB.ContainsKey(104))
                    //{
                    //    //HZBookingInfo.ApplyType.Code = OwnerViewModel._HasBZLB[101];
                    //    if (OwnerViewModel._HasBZLB.ContainsKey(104))
                    //    {
                    //        var TWtypes = GetApplyTypes().Where(t => t.Code == OwnerViewModel._HasBZLB[104]).ToList()[0];
                    //        TWBookingInfo.ApplyType = TWtypes;
                    //    }

                    //}
                    //根据台湾证件信息，给台湾签注默认值
                    DataVerificate(EnumTypeSQLB.TWN, TWBookingInfo, out string msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                        TWBookingInfo.ApplyType = null;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("默认初始化数据发生异常：" + ex.Message);
                Log.Instance.WriteError("默认初始化数据发生异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="type">办理类别</param>
        /// <param name="info"></param>
        /// <param name="msg"></param>
        /// <param name="isApply"></param>
        private void DataVerificate(EnumTypeSQLB type, BookingInfo info, out string msg, bool isApply = true)
        {
            msg = "";
            var PaperInfo = OwnerViewModel?.PaperInfos;
            //获取间隔时间
            int days = DaysInterval(info.XCZJYXQZ);

            if (type == EnumTypeSQLB.HZ && HZBookingInfo?.ApplyType != null)
            {
                //核查无证件 是否选择首次
                if (string.IsNullOrEmpty(HZBookingInfo.XCZJHM) && string.IsNullOrEmpty(HZBookingInfo.XCZJYXQZ))
                {
                    if (info.ApplyType.Code != "11")
                    {
                        msg = TipMsgResource.FirstApplyTipMsg;
                    }
                }
                else
                {
                    //核查有证件 是否剔除首次
                    if (!string.IsNullOrEmpty(HZBookingInfo.XCZJHM) && days >= 0 && (info.ApplyType?.Code == "13" || info.ApplyType?.Code == "11"))
                    {
                        msg = TipMsgResource.DocumentValidTipMsg;
                    }
                    //核查证件是否已经过期
                    if (days < 0 && info.ApplyType?.Code != "13")
                    {
                        msg = TipMsgResource.DocumentExpirationTipMsg;
                    }
                }
            }

            if (type == EnumTypeSQLB.HKGMAC && GABookingInfo?.ApplyType != null)
            {
                //核查无证件 是否选择首次
                if (string.IsNullOrEmpty(GABookingInfo.XCZJHM) && string.IsNullOrEmpty(GABookingInfo.XCZJYXQZ))
                {
                    if (info.ApplyType != null && info.ApplyType.Code != "11")
                    {
                        msg = TipMsgResource.FirstApplyTipMsg;
                    }
                }
                else
                {
                    //核查有证件 是否剔除首次
                    if (!string.IsNullOrEmpty(GABookingInfo.XCZJHM) && days >= 0 && (info.ApplyType?.Code == "13" || info.ApplyType?.Code == "11"))
                    {
                        msg = TipMsgResource.DocumentValidTipMsg;
                    }
                    //核查证件是否已经过期
                    if (days < 0 && info.ApplyType?.Code != "13")
                    {
                        msg = TipMsgResource.DocumentExpirationTipMsg;
                    }

                }
            }

            if (type == EnumTypeSQLB.TWN && TWBookingInfo?.ApplyType != null)
            {
                //核查无证件 是否选择首次
                if (string.IsNullOrEmpty(TWBookingInfo.XCZJHM) && string.IsNullOrEmpty(TWBookingInfo.XCZJYXQZ))
                {
                    if (info.ApplyType != null && info.ApplyType.Code != "11")
                    {
                        msg = TipMsgResource.FirstApplyTipMsg;

                    }
                }
                else
                {
                    //核查有证件 是否剔除首次
                    if (!string.IsNullOrEmpty(TWBookingInfo.XCZJHM) && days >= 0 && (info.ApplyType?.Code == "13" || info.ApplyType?.Code == "11"))
                    {
                        msg = TipMsgResource.DocumentValidTipMsg;
                    }
                    //核查证件是否已经过期
                    if (days < 0 && info.ApplyType?.Code != "13")
                    {
                        msg = TipMsgResource.DocumentExpirationTipMsg;
                    }

                }
            }

        }

        /// <summary>
        /// 北京地区默认预约时间段
        /// </summary>
        public bool GetTargetTime() //async
        {
            var result = new List<BookingTargetModel>();
            try
            {
                Log.Instance.WriteInfo("北京地区，开始查询预约指标接口……");
                //result =  ZHPHMachineWSHelper.ZHPHInstance.S_BookingZB(DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate()));
                //不用异步调用，往下执行后都还没返回值导致卡死 by wei.chen 2021年7月16日14:12:10 
                //var a = DateTime.Parse(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate());
                //result = ZHPHMachineWSHelper.ZHPHInstance.S_BookingZB(a);
                //if (result != null && result.Count > 0)
                //{
                //    BookingBaseInfo.BookingTarget = result.FirstOrDefault();
                //}
                //else
                //{
                //    BookingBaseInfo.BookingTarget = new BookingTargetModel()
                //    {
                //        BookingDate = DateTime.Now.ToString("yyyyMMdd"),
                //        StartTime = "",
                //        EndTime = ""
                //    };
                //}
                return true;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("北京地区，查询预约指标接口发生异常：" + ex.Message);
            }
            finally
            {
                Log.Instance.WriteInfo("北京地区，结束查询预约指标接口……" + result?.Count);
            }
            return false;
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

                if (OwnerViewModel?.PaperWork != null && OwnerViewModel?.PaperWork?.Length > 0)
                {

                    //Log.Instance.WriteInfo("查询到证件：" + OwnerViewModel?.PaperWork.Length + "====" + type.GetHashCode().ToString() + "=-----" + OwnerViewModel?.PaperWork?.FirstOrDefault()?.zjzl);
                    var paperWork = OwnerViewModel?.PaperWork?.Where(t => t.zjzl == type.GetHashCode().ToString())
                        ?.ToList();
                    //Log.Instance.WriteInfo("查询到证件：" + paperWork?.Count + "条");
                    // Log.Instance.WriteInfo("证件种类：" + paperWork?.FirstOrDefault()?.zjzl + "，号码：" + paperWork?.FirstOrDefault()?.zjhm);
                    if (paperWork?.Count > 0)
                    {
                        model = OwnerViewModel.PaperWork.Where(t => t.zjzl == type.GetHashCode().ToString())
                            ?.OrderByDescending(t => t?.zjyxqz?.FirstOrDefault())?.ToList()?[0];
                        //Log.Instance.WriteInfo(type.ToString() + "查询到证件类型为：" + model?.zjzl + "，证件号码为：" + model?.zjhm);
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


        private bool TipMsg(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                OwnerViewModel?.MessageTips(msg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 取申请类型
        /// </summary>
        /// <returns></returns>
        private List<DictionaryType> GetSQTypes()
        {
            return ServiceRegistry.Instance.Get<ElementManager>()?.Get<List<DictionaryType>>()
                ?.Where(t => t.KindType == ((int)KindType.ApplyType).ToString()).OrderBy(t => t.Code).ToList();
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
        /// 证件有效期
        /// </summary>
        /// <param name="yxq"></param>
        /// <returns></returns>
        public string ConverYxq(string yxq)
        {
            if (yxq.IsEmpty())
                return "1";

            if (yxq.ToUpper().Contains("N"))
            {
                yxq = yxq?.Replace('N', '0').ToInt().ToString();
                return yxq;
            }
            else if (yxq.ToUpper().Contains("Y"))
            {
                yxq = yxq?.Replace('Y', '0').ToInt().ToString();
                return yxq;
            }
            else if (yxq.ToUpper().Contains("Z"))
            {
                yxq = yxq?.Replace('Z', '0').ToInt().ToString();
                return yxq;
            }
            else
            {
                return "1";
            }
        }

        /// <summary>
        /// 根据太极的签注有效期
        /// 返回有效期单位
        /// </summary>
        /// <param name="yxq"></param>
        /// <returns></returns>
        public string GetYxqDw(string yxq)
        {
            if (yxq.IsEmpty())
                return "";
            if (yxq.ToUpper().Contains("N"))
            {
                return "年";
            }
            else if (yxq.ToUpper().Contains("Y"))
            {
                return "月";
            }
            else if (yxq.ToUpper().Contains("Z"))
            {
                return "周";
            }
            else
            {
                return "日";
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
    }
}
