using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Freedom.Hardware;
using Freedom.Models;
using Application = System.Windows.Forms.Application;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class MainPageViewModels : ViewModelBase
    {
        /// <summary>
        /// 是否显示现场预约
        /// </summary>
        public bool IsBooking
        {
            get
            {
                if (OwnerViewModel?.IsBeijing == true)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 读取身份证线程
        /// </summary>
        private Thread ReadIDCardThread = null;

        //private Thread QueryReadIDCardThread = null;

        private bool bln = false;

        public override void DoInitFunction(object obj)
        {
            //程序名称
            //string processName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
            ////程序版本
            //string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //Log.Instance.WriteInfo("当前版本：" + version);
            ////设备ID
            //string deviceId = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString();

            if (!OwnerViewModel.CheckServeStatus())
            {
                base.DoNextFunction("NotificationPage");
                return;
            }

            //更新程序
            //AutoUpdate.CheckAutoUpdate(deviceId, version, processName);

            //注释 by 2021年7月16日16:48:38 wei.chen
            //OnDispose();

            ////设置背景图   
            //var bgpath = Path.Combine(FileHelper.GetLocalPath(), @"ApplicationData/Skin/HsZhPjh/bg.jpg");
            //MainWindowViewModels.Instance.WinBrshImage = new BitmapImage(new Uri(bgpath, UriKind.RelativeOrAbsolute));

            Log.Instance.WriteInfo("\n***********************进入【首页】***********************");

            Log.Instance.WriteInfo("======清空上个人的信息=====");
            //清空上个人的信息
            OwnerViewModel.PaperWork = null;
            OwnerViewModel.YWID = null;
            OwnerViewModel.KbywInfos = null;
            OwnerViewModel.DyLoginRec = null;
            OwnerViewModel.IsManual = false;
            OwnerViewModel.ReceiptCardNo = "";
            //OwnerViewModel._HasBZLB = null;

            UpdateDeviceStatus();

            if (OwnerViewModel?.IsShenZhen == true && QJTConfig.QJTModel.IsMainCheckIdCard)
            {

                ReadIDCardThread = new Thread(new ThreadStart(ReadIDCard));
                ReadIDCardThread.IsBackground = true;
                ReadIDCardThread.Start();
            }
        }

        public async void UpdateDeviceStatus()
        {
            if (QJTConfig.QJTModel?.QJTDevInfo?.DEVICESTATUS != (EnumTypeSTATUS.FREE).ToString() &&
                QJTConfig.QJTModel?.QJTDevInfo?.DEVICESTATUS != (EnumTypeSTATUS.PASUE).ToString())
            {
                OwnerViewModel?.IsShowHiddenLoadingWait("更新设备状态,请稍等.....");
                await Task.Run(() =>
               {
                   try
                   {
                       //修改设备空闲状态
                       bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.FREE);
                       Log.Instance.WriteInfo(blnResult
                           ? $"修改设备状态【{EnumType.GetEnumDescription(EnumTypeSTATUS.FREE)}】：成功"
                           : $"修改设备状态【{EnumType.GetEnumDescription(EnumTypeSTATUS.FREE)}】：失败");
                       TopMostTool.SetTopWindow();
                       //if (blnResult)
                       //{
                       //    Log.Instance.WriteInfo($"首页修改设备状态[{EnumType.GetEnumDescription(EnumTypeSTATUS.FREE)}]成功");
                       //}

                   }
                   catch (Exception ex)
                   {
                       Log.Instance.WriteInfo("首页修改设备状态【失败】：" + ex.Message);
                       Log.Instance.WriteError(ex.Message);
                   }
                   finally
                   {
                       OwnerViewModel?.IsShowHiddenLoadingWait();
                       Log.Instance.WriteInfo("首页数据初始化：【成功】");
                   }
                   //北京地区跳过
                   if (OwnerViewModel?.IsBeijing == true)
                   {
                       this.DoNextFunction(0);
                   }
               });
            }
            else if (QJTConfig.QJTModel?.QJTDevInfo?.DEVICESTATUS == (EnumTypeSTATUS.PASUE).ToString())
            {
                //切换到暂停页面
                base.DoNextFunction("NotificationPage");
            }
            else
            {
                if (OwnerViewModel?.IsBeijing == true)
                {
                    this.DoNextFunction(0);
                }
            }
        }

        /// <summary>
        /// 读取身份证号码信息
        /// </summary>
        private void ReadIDCard()
        {
            try
            {
                //清空全局身份证对象
                OwnerViewModel.CardInfo = null;
                if (OwnerViewModel?.IsShenZhen == true)
                    TTS.PlaySound("预约机-页面-刷身份证");
                Log.Instance.WriteInfo("===========开始读取身份证===========");
                if (!OwnerViewModel.CheckServeStatus())
                {
                    DoNextFunction("NotificationPage");
                    return;
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
                    ReturnInfo info = ReadIDCardHelper.Instance.DoReadIDCardInfo(out IdCardInfo model, "GetImage");
                    if (info.IsSucceed)
                    {
                        bln = true;
                        Log.Instance.WriteInfo("读身份成功：" + model?.FullName?.ToString() + "，" + model?.IDCardNo?.ToString());
                        Log.Instance.WriteInfo("===========结束读取身份证===========");
                        OwnerViewModel.CardInfo = model;
                        //ReadIDCardHelper.Instance.DoCloseIDCard();
                        DoNextFunction(2);
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

        public override void DoNextFunction(object obj)
        {
            Log.Instance.WriteInfo("\n***********************结束【首页】”"+ obj?.ToString() + "“***********************");
            if (obj?.ToString() == "1")
            {
                ServiceRegistry.Instance.Get<ElementManager>().Set<string>("0", "ModelType");
                base.DoNextFunction("Booking/BookingReceipt");
            }
            else if (obj?.ToString() == "2")
            {
                ServiceRegistry.Instance.Get<ElementManager>().Set<string>("0", "ModelType");
                base.DoNextFunction("JgReadIDCard2");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(obj?.ToString()))
                {
                    ServiceRegistry.Instance.Get<ElementManager>().Set<string>(obj?.ToString(), "ModelType");
                    base.DoNextFunction("JgReadIDCard");
                }
            }

        }

        protected override void OnDispose()
        {
            try
            {
                Log.Instance.WriteInfo("MainPage OnDispose");
                if (ReadIDCardThread != null)
                {
                    ReadIDCardThread.Abort(0);
                    ReadIDCardThread = null;
                }
                CommonHelper.OnDispose();
                TTS.StopSound();
                base.OnDispose();
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo(" MainPage ex:"+ex.Message);
            }
        }
    }
}
