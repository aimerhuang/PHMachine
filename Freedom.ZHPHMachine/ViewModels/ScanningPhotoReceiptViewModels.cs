using Freedom.Common;
using Freedom.Config;
using Freedom.Controls.Foundation;
using Freedom.Hardware;
using Freedom.Models;
using Freedom.WinAPI;
using Freedom.ZHPHMachine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Freedom.BLL;
using Freedom.Models.CrjCreateJsonModels;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Command;
using MachineCommandService;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class ScanningPhotoReceiptViewModels : ViewModelBase
    {
        #region 构造函数 
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="window"></param>
        public ScanningPhotoReceiptViewModels(Page page)
        {
            this.ContentPage = page;

            if (BookingBaseInfo?.BookingSource == 1 && OwnerViewModel?.IsShenZhen == true)
                this.TipMessage = BookingBaseInfo?.CardInfo?.FullName + "  扫描本人相片回执条码";
            else
                TipMessage = "扫描本人相片回执条码";
        }
        #endregion

        #region 属性
        private string bookingPerSonName;
        private CrjPreapplyManager crjManager = new CrjPreapplyManager();
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

        public ICommand NumberKeyboard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupNumberKeyboard(KeyboardType.Receipt, "", out string str))
                    {
                        Log.Instance.WriteInfo("KeyboardType.Receipt");
                        QueryRKZPInfo(str);
                    }
                });
            }
        }

        private string receiptNo;

        /// <summary>
        /// 手输回执单号
        /// </summary>
        public string ReceiptNo
        {
            get { return receiptNo; }
            set { receiptNo = value; RaisePropertyChanged("ReceiptNo"); }
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

        public ICommand KeyboardCommand
        {
            get
            {
                return new RelayCommand<string>((val) =>
                {
                    TipMsg = string.Empty;
                    Log.Instance.WriteInfo("点击【" + val + "】");
                    switch (val)
                    {
                        case "确定":
                            if (string.IsNullOrWhiteSpace(receiptNo) || receiptNo.Length > 15 || receiptNo.Length < 8)
                            {
                                TipMsg = TipMsgResource.ScanningReceiptErrorTipMsg;

                                return;
                            }
                            //if (receiptNo.StartsWith("ZZEM") && OwnerViewModel?.IsXuwen == true)
                            //{
                            //    //TipMsg = "系统未查询到您有效的制证照片，请重新操作！";
                            //    OwnerViewModel?.MessageTips("系统未查询到您有效的制证照片，请重新操作！");
                            //    return;

                            //}
                            QueryRKZPInfo(receiptNo);
                            break;
                        case "删除":
                            Win32API.AddKeyBoardINput(0x08);
                            break;
                        case "重填":
                            ReceiptNo = string.Empty;
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
                     (this.ContentPage.FindName("txt") as TextBox).Focus();
                });
            }
        }
        #endregion

        #region 方法

        /// <summary>
        /// 初始化数据
        /// </summary>

        public override void DoInitFunction(object obj)
        {
            Log.Instance.WriteInfo("\n**********************************进入【扫描相片回执】界面**********************************");
            //if (!OwnerViewModel.CheckServeStatus())
            //{
            //    DoNext.Execute("NotificationPage");
            //}
            try
            {
                if (BookingBaseInfo.IsEmpty())
                {
                    Log.Instance.WriteInfo("《《《《《阿o 预约数据弄丢了》》》》》");
                }
                if (!QJTConfig.QJTModel.IsScanReceipt)
                {
                    Log.Instance.WriteInfo("配置项跳过扫描回执界面");
                    DoNextFunction("CheckPage");
                    return;
                }

                Log.Instance.WriteInfo("进入扫描回执界面");
                Log.Instance.WriteInfo("IsBeijing标识：" + OwnerViewModel?.IsBeijing + " IsDirectNumber标识：" + OwnerViewModel?.IsDirectNumber.ToString() + " - IsGetPHNO标识：" + BookingBaseInfo.IsGetPHNO?.ToString());
                //北京仅签注给默认回执照片
                //选中仅签注默认给照片
                if (OwnerViewModel?.IsDirectNumber == true && BookingBaseInfo.ReceiptsNo.IsEmpty())
                {
                    Log.Instance.WriteInfo("仅签注默认默认回执照片 DY12345678 ");
                    BookingBaseInfo.ReceiptsNo = "DY12345678";
                    //OwnerViewModel?.MessageTips(msg, (() => { this.DoExit.Execute(null); }));
                }
                else
                {
                    if (BookingBaseInfo?.ReceiptsNo?.ToString() == "DY12345678")
                    {
                        if (BookingBaseInfo.IsGetPHNO?.ToString() != "1")
                        {
                            Log.Instance.WriteInfo("核查到非仅签注，不使用默认照片，进入扫描回执页");
                            BookingBaseInfo.ReceiptsNo = "";
                        }
                        else
                        {
                            Log.Instance.WriteInfo("直接取号，仅签注跳过扫描回执页面");
                        }
                    }
                }
                Log.Instance.WriteInfo("ReceiptsNo标识为：" + BookingBaseInfo.ReceiptsNo?.ToString());
                Log.Instance.WriteInfo("IsCheck_Pic标识为：" + QJTConfig.QJTModel.IsCheck_Pic + " -IsCheckNowPic标识为： " + QJTConfig.QJTModel.IsCheckNowPic + " -IsCheckYYXX_ZZZP标识为： " + QJTConfig.QJTModel.IsCheckYYXX_ZZZP);
                //根据配置项是否核查制证照片
                if (!string.IsNullOrEmpty(BookingBaseInfo.ReceiptsNo) && QJTConfig.QJTModel.IsCheck_Pic ||
                    !string.IsNullOrEmpty(BookingBaseInfo.ReceiptsNo) && QJTConfig.QJTModel.IsCheckNowPic ||
                    !string.IsNullOrEmpty(BookingBaseInfo.ReceiptsNo) && QJTConfig.QJTModel.IsCheckYYXX_ZZZP ||
                    OwnerViewModel?.IsBeijing == true && OwnerViewModel?.IsDirectNumber == true && BookingBaseInfo.ReceiptsNo.IsNotEmpty())
                {
                    Log.Instance.WriteInfo("制证照片回执编号：【" + BookingBaseInfo.ReceiptsNo?.ToString() + "】，自动进入核查");
                    //DoNextFunction("Booking/BookingArea");
                    this.DoNext.Execute("CheckPage");
                    return;
                }


                //全部办理签注，跳过扫描照片界面
                if (OwnerViewModel?.IsXuwen == true)
                {
                    Log.Instance.WriteInfo("徐闻地区……");
                    int? bookingCount = BookingBaseInfo.BookingInfo?.Count;
                    int? selCount = 0;

                    if (BookingBaseInfo?.BookingInfo != null)
                    {
                        foreach (var item in BookingBaseInfo?.BookingInfo)
                        {
                            if (item.ApplyType?.Code == "92")
                                selCount++;
                        }

                        Log.Instance.WriteInfo("办证总数：" + bookingCount + "|选择签注总数：" + selCount);
                        if (bookingCount == selCount && !QJTConfig.QJTModel.IsTodayPH)//区分徐闻县和遂溪县，签注规则
                        {
                            Log.Instance.WriteInfo("查询只办理签注，跳过扫描回执界面");
                            this.DoNext.Execute("CheckPage");
                            return;
                        }
                    }

                }
                Log.Instance.WriteInfo("===初始化硬件====");
                TTS.PlaySound("扫描相片回执");
                OpenBarCode();
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo(" 加载页面 - 操作发生异常：" + ex.Message);
                Log.Instance.WriteError(" 加载页面 - 操作发生异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 查询照片回执
        /// </summary>
        /// <param name="xphz">照片回执号</param>
        public async void QueryRKZPInfo(string xphz)
        {
            try
            {
                OwnerViewModel?.IsShowHiddenLoadingWait("正在查询照片请稍等......");
                //if (OwnerViewModel?.CheckServeStatus() == false)
                //{
                //    //ReadIDCardHelper.Instance.DoCloseIDCard();
                //    OwnerViewModel?.MessageTips("检查到网络不稳定，请重新取号！", (() =>
                //    {
                //        DoExitFunction(null);
                //    }));
                //    return;
                //    //DoNextFunction("NotificationPage");
                //}
                Log.Instance.WriteInfo("点击【确定】按钮，开始获取制证照片" + xphz);

                if (string.IsNullOrWhiteSpace(xphz) || xphz.Length > 15 || xphz.Length < 8)
                {
                    Log.Instance.WriteInfo("校验结果：" + TipMsgResource.ScanningReceiptErrorTipMsg);
                    OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptErrorTipMsg);
                    return;
                }

                if (!QJTConfig.QJTModel.IsUseXdPhoto && receiptNo.StartsWith("ZZEM"))
                {
                    TipMsg = "系统未查询到您有效的制证照片，请重新填写！";
                    Log.Instance.WriteInfo("校验结果：" + TipMsg);
                    return;
                }

                if (xphz == "DY12345678" || DjzConfig.DjzModel.IsConnectionDjz && xphz == "12345678")
                {
                    Log.Instance.WriteInfo("校验结果：DY12345678 系统未查询到您有效的制证照片，请重新操作！");
                    OwnerViewModel?.MessageTips("系统未查询到您有效的制证照片，请重新操作！");
                    return;
                }
                base.OnDispose();//停止计时器 by wei.chen

                //广东地区查询全国照片接口
                if (OwnerViewModel.IsGuangDong && QJTConfig.QJTModel.IsCheckYYXX_ZZZP)//&& QJTConfig.QJTModel.IsCheck_Pic
                {
                    Log.Instance.WriteInfo("广东地区开始查询全国照片接口...");
                    //Dictionary<string, object> args = new Dictionary<string, object>()
                    //{
                    //    ["SFZH"] = BookingBaseInfo.CardInfo.IDCardNo,
                    //    ["HZBH"] = xphz,
                    //};
                    ////先拿接口的照片
                    //var result = ZHPHMachineWSHelper.ZHPHInstance.QueryQgPhoto(JsonHelper.ToJson(args));
                    string msg = string.Empty;
                    //if (result == null)
                    //{

                    Json_I_cn_queryPhotoHyt photo = new Json_I_cn_queryPhotoHyt();
                    //photo.rylb = "R";
                    photo.zpbh = xphz;
                    photo.scode = DjzConfig.DjzModel.DzjZCXLH;
                    //非核验台查询制证照片
                    Json_R_cn_queryPhotoHytList photoInfo = crjManager.HYT_cn_queryLocalPhotoList(photo);
                    //Log.Instance.WriteInfo(JsonHelper.ToJson(photoInfo));
                    if (photoInfo.success == 1 && photoInfo?.data?.zzzpinfos != null)
                    {

                        //Log.Instance.WriteInfo("照片回执：" + photoInfo?.data?.zp);
                        if (!string.IsNullOrEmpty(photoInfo?.data?.zzzpinfos[0]?.zp))
                        {
                            var reinforce = I_UploadPhoto(xphz, photoInfo?.data?.zzzpinfos[0]?.zp, out msg);
                            if (reinforce)
                            {
                                //BookingBaseInfo.ReceiptsNo = xphz;
                                //Log.Instance.WriteInfo("上传制证照片成功!");
                                //获取到回执号后跳转至派号
                                this.DoNext.Execute("CheckPage");
                            }
                            else
                            {
                                //Log.Instance.WriteInfo("上传制证照片失败：" + zzzpinfo.MessageInfo);
                                OwnerViewModel?.MessageTips(msg,
                                    () => { DoNext.Execute("CheckPage"); });
                            }
                            ////全国制证照片上传到服务器
                            //var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                            //    BookingBaseInfo?.CardInfo?.IDCardNo, xphz,
                            //    QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                            //    BookingBaseInfo?.CardInfo?.FullName ?? BookingBaseInfo?.CardInfo?.IDCardNo + ".jpg",
                            //    photoInfo?.data?.zzzpinfos[0]?.zp, photoInfo?.data?.zzzpinfos[0]?.zp, null);
                            ////赋值制证照片，跳过照片回执界面
                            ////BookingBaseInfo.ReceiptsNo = xphz;
                            ////Log.Instance.WriteInfo("查询到全国制证照片回执单号：" + zzzpinfo?.ReturnValue[0]?.HZBH + ",跳过扫描回执界面！");
                            //if (zzzpinfo.IsSucceed)
                            //{
                            //    BookingBaseInfo.ReceiptsNo = xphz;
                            //    Log.Instance.WriteInfo("上传制证照片成功!");
                            //    //获取到回执号后跳转至派号
                            //    this.DoNext.Execute("CheckPage");
                            //}
                            //else
                            //{
                            //    Log.Instance.WriteInfo("上传制证照片失败：" + zzzpinfo.MessageInfo);
                            //    OwnerViewModel?.MessageTips(zzzpinfo.MessageInfo,
                            //        () => { DoNext.Execute("CheckPage"); });
                            //}
                        }
                        else
                        {
                            Log.Instance.WriteInfo("查询全国照片接口返回值：" + photoInfo?.message);
                        }
                    }
                    //}
                    //else
                    //{
                    //    var count = result.data.zzzpinfos[0].zp;
                    //    var reinforce = I_UploadPhoto(xphz, count, out msg);
                    //    if (reinforce)
                    //    {
                    //        //获取到回执号后跳转至派号
                    //        this.DoNext.Execute("CheckPage");
                    //    }
                    //    else
                    //    {
                    //        OwnerViewModel?.MessageTips(msg,
                    //            () => { DoNext.Execute("CheckPage"); });
                    //    }
                    //}

                }


                //太极接口获取历史制证照片
                if (TjConfig.TjModel.IsConnectionTj && QJTConfig.QJTModel.IsCheck_Pic)
                {
                    Log.Instance.WriteInfo("启用太极接口，开始核查制证照片");
                    Xpxx[] xpxx = new Xpxx[0];
                    //取历史制证照片
                    var result = crjManager.Do_Common_queryPaperPhoto(BookingBaseInfo?.CardInfo?.IDCardNo, out xpxx);
                    var photoInfo = result.ReturnValue as Xpxx;
                    if (!string.IsNullOrEmpty(photoInfo?.xp))
                    {
                        Log.Instance.WriteInfo("查询到太极制证照片，开始上传至服务器...");
                        //制证照片保存至服务器
                        var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                            BookingBaseInfo?.CardInfo?.IDCardNo, xphz,
                            QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() + "+" +
                            QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                            BookingBaseInfo?.CardInfo?.FullName ?? BookingBaseInfo?.CardInfo?.IDCardNo + ".jpg",
                            photoInfo?.xp, photoInfo?.xp, null);
                        //赋值制证照片，跳过照片回执界面
                        //BookingBaseInfo.ReceiptsNo = xphz;
                        if (zzzpinfo?.ReturnValue != null && zzzpinfo?.IsSucceed == true)
                        {
                            Log.Instance.WriteInfo("上传制证照片成功：" + zzzpinfo?.ReturnValue[0]?.HZBH + "");
                        }
                        else
                        {
                            Log.Instance.WriteInfo("上传制证照片失败：" + zzzpinfo?.MessageInfo + "");
                        }

                    }
                    else
                    {
                        Log.Instance.WriteInfo("查询全国照片接口返回值：" + photoInfo?.lrsj);
                    }
                }

                //PH_ZZZP_TBExtend ZZinfo = new PH_ZZZP_TBExtend();
                //ZZinfo = OwnerViewModel?.IsBeijing == true ? ZHPHMachineWSHelper.ZHPHInstance.S_ZZZP(BookingBaseInfo.CardInfo.IDCardNo, xphz) : ZHPHMachineWSHelper.ZHPHInstance.S_ZZZP(BookingBaseInfo.CardInfo.IDCardNo);

                var ZZinfo = await ZHPHMachineWSHelper.ZHPHInstance.T_S_ZZZP(BookingBaseInfo?.CardInfo?.IDCardNo, xphz);
                await Task.Delay(1000);
                if (ZZinfo == null)
                {
                    //换弹出页面 by 2021年7月8日16:55:07 wei.chen
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ReturnInfo showReturn = MainWindowViewModels.Instance.ShowMsgReturnDialog("系统未查询到您有效的制证照片，请重新操作！", 10, true);
                        if (showReturn.IsNotEmpty() && showReturn.IsSucceed)
                        {
                            (this.ContentPage.FindName("txt") as TextBox).Focus();
                            OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);//恢复计时器 by wei.chen
                        }
                    }));

                    //OwnerViewModel?.MessageTips("系统未查询到您有效的制证照片，请重新操作！");
                    return;
                }

                Log.Instance.WriteInfo("查询制证照片成功,赋值回执编号：" + xphz);
                BookingBaseInfo.ReceiptsNo = xphz;

                //获取到回执号后跳转至派号
                this.DoNext.Execute("BookingArea");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("查询制证照片异常");
                Log.Instance.WriteError("查询制证照片异常" + ex.Message);
                OwnerViewModel?.MessageTips(ex.Message, (() => { this.DoExit.Execute(null); }));
            }
            finally
            {
                OwnerViewModel?.IsShowHiddenLoadingWait();

            }
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <returns></returns>
        public bool I_UploadPhoto(string xphz, string zp, out string msg)
        {
            msg = string.Empty;
            var zzzpinfo = ZHPHMachineWSHelper.ZHPHInstance.I_UpLoadJPZInfoAsync(
                BookingBaseInfo?.CardInfo?.IDCardNo, xphz,
                QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToInt().ToString(),
                BookingBaseInfo?.CardInfo?.FullName ?? BookingBaseInfo?.CardInfo?.IDCardNo + ".jpg",
                zp, zp, null);
            if (zzzpinfo.IsSucceed)
            {
                BookingBaseInfo.ReceiptsNo = xphz;
                Log.Instance.WriteInfo("上传制证照片成功!");
                return true;
            }
            else
            {
                msg = zzzpinfo.MessageInfo;
                Log.Instance.WriteInfo("上传制证照片失败：" + zzzpinfo.MessageInfo);
                return false;
            }

        }

        public void OpenBarCode()
        {
            try
            {
                Log.Instance.WriteInfo("开始打开扫描枪扫描回执编码");
                if (BarCodeScanner.Instance.OpenBarCodeDev().IsSucceed)
                {
                    Log.Instance.WriteInfo("打开扫描枪【成功】");
                    this.timer = new System.Timers.Timer();
                    this.timer.Interval = 500;
                    this.timer.Elapsed += (sender, e) =>
                    {
                        //打开扫回执摄像头
                        BarCodeScanner.Instance.OpenScan();
                        if (!string.IsNullOrEmpty(BarCodeScanner.Instance.ReadValue))
                        {
                            var xphz = BarCodeScanner.Instance.ReadValue.Replace("*", "").Trim().TrimEnd();
                            BarCodeScanner.Instance.ReadValue = "";
                            if (xphz.IsNotEmpty())
                            {
                                base.Dispose();//扫码成功暂停倒计时 by wei.chen
                                if (string.IsNullOrWhiteSpace(xphz) || xphz.Length > 15 || xphz.Length < 8)
                                {
                                    //OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptErrorTipMsg);
                                    //换弹出页面 by 2021年7月8日16:55:07 wei.chen
                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        ReturnInfo showReturn = MainWindowViewModels.Instance.ShowMsgReturnDialog(TipMsgResource.ScanningReceiptErrorTipMsg, 10, true);
                                        if (showReturn.IsNotEmpty() && showReturn.IsSucceed)
                                        {
                                            (this.ContentPage.FindName("txt") as TextBox).Focus();
                                            OpenTimeOut(QJTConfig.QJTModel.TOutSeconds); //重新恢复 
                                        }
                                    }));
                                }
                                else
                                {
                                    this.timer.Stop();
                                    BarCodeScanner.Instance.CloseScan();
                                    //if (receiptNo.StartsWith("ZZEM") && OwnerViewModel?.IsXuwen == true)
                                    //{
                                    //    TipMsg = "系统未查询到您有效的制证照片，请重新填写！";
                                    //    return;
                                    //}
                                    Log.Instance.WriteInfo("扫描回执编号成功：" + xphz + "，开始查询相片回执。");
                                    QueryRKZPInfo(receiptNo);
                                }

                                //if (xphz.Length == QJTConfig.QJTModel.XphzBarCodeLen) 
                            }
                        }


                    };
                    this.timer.Start();
                    //界面超时
                    this.OpenTimeOut(QJTConfig.QJTModel.TOutSeconds);
                }
                else
                {
                    Log.Instance.WriteInfo("打开扫描枪【失败】");
                    OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptTipMsg,
                        () => { this.DoExit.Execute(null); });
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("扫描枪扫描发生错误：" + ex);
                Log.Instance.WriteError("扫描枪扫描发生错误：" + ex);
                OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptTipMsg,
                    () => { DoExitFunction(null); });
                //return;
                //Console.WriteLine(ex);
                //throw;
            }
            finally
            {
                Log.Instance.WriteInfo("结束打开扫描枪扫描回执编码");
            }

        }


        #endregion

        #region 重写方法

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void OnDispose()
        {
            //关闭串口
            BarCodeScanner.Instance.CloseBarCodeDev();
            TTS.StopSound();
            CommonHelper.OnDispose();
            base.OnDispose();
        }

        public override void DoNextFunction(object obj)
        {
            try
            {
                if (obj.ToString() == "CheckPage")
                {
                    Log.Instance.WriteInfo("点击【前往拍照区拍照】按钮");
                }
                else if (obj.ToString() == "2")
                {
                    OwnerViewModel.RenGong = 2;
                    Log.Instance.WriteInfo("点击【前往人工区拍照】按钮，拍照标识：" + obj?.ToString());
                }
                else
                {
                    Log.Instance.WriteInfo("点击【确认】按钮");
                }
                string msg = BookingBaseInfo.ReceiptsNo.IsNotEmpty() ? "回执编号:" + BookingBaseInfo?.ReceiptsNo + "" : "查询到未拍照";
                //Log.Instance.WriteInfo(msg);
                //Log.Instance.WriteInfo("=====离开扫描回执界面=====");

                //关闭倒计时
                //OnDispose(); by wei.chen

                //Log.Instance.WriteInfo("下一步：进入选择区域界面");
                Log.Instance.WriteInfo("\n**********************************结束【扫描相片回执】界面**********************************");
                base.DoNextFunction("Booking/BookingArea");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("获取制证照片出现异常：" + ex);
                OwnerViewModel?.MessageTips("获取制证照片出现异常，请重试！", () =>
                {
                    this.DoExit.Execute(null);
                });
                //return;
            }

        }

        #endregion
    }
}
