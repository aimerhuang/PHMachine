using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Freedom.Common;
using Freedom.Config;
using Freedom.Hardware;
using Freedom.ZHPHMachine.View;

namespace Freedom.ZHPHMachine.ViewModels.Booking
{
    public class BookingReceiptViewModels : ViewModelBase
    {
        public BookingReceiptViewModels(Page page)
        {
            this.ContentPage = page;
            this.TipMessage = "扫描本人智慧码";
        }
        public override void DoInitFunction(object obj)
        {
            //string b = "430981199512160031_199512160031_20200101";
            //int aOf = b.IndexOf("_", StringComparison.Ordinal);
            //var vv = b.Substring(0,
            //    aOf );
            Log.Instance.WriteInfo("\n====== 进入输回执页面======");
            TTS.PlaySound("扫描相片回执");
            if (BarCodeScanner.Instance.OpenBarCodeDev().IsSucceed)
            {
                this.timer = new System.Timers.Timer();
                this.timer.Interval = 500;
                this.timer.Elapsed += (sender, e) =>
                {
                    BarCodeScanner.Instance.OpenScan();
                    if (!string.IsNullOrEmpty(BarCodeScanner.Instance.ReadValue))
                    {
                        var xphz = BarCodeScanner.Instance.ReadValue.Replace("*", "").Trim().TrimEnd(); ;
                        BarCodeScanner.Instance.ReadValue = "";
                        if (xphz.IsNotEmpty())
                        {
                            if (string.IsNullOrWhiteSpace(xphz) || xphz.Length < 18)
                            {
                                OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptErrorTipMsg);
                            }
                            else
                            {
                                this.timer.Stop();
                                BarCodeScanner.Instance.CloseScan();

                                int len = xphz.IndexOf("_", StringComparison.Ordinal);
                                var substring = xphz.Substring(0, len);

                                OwnerViewModel.ReceiptCardNo = substring;
                                DoNextFunction(null);
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
                OwnerViewModel?.MessageTips(TipMsgResource.ScanningReceiptTipMsg, () =>
                {
                    this.DoExit.Execute(null);
                });
                return;
            }
        }


        public override void DoNextFunction(object obj)
        {
            OnDispose();
            Log.Instance.WriteInfo("扫描到的身份证号码为：" + OwnerViewModel.ReceiptCardNo);
            base.DoNextFunction("JgReadIDCard");
        }



        protected override void OnDispose()
        {
            //关闭串口
            BarCodeScanner.Instance.CloseBarCodeDev();
            TTS.StopSound();
            CommonHelper.OnDispose();
            base.OnDispose();
        }
    }
}
