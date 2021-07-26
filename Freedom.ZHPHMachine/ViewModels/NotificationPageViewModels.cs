using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Freedom.Common;
using Freedom.Config;
using Freedom.Hardware;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using MachineCommandService;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class NotificationPageViewModels : ViewModelBase
    {
        public NotificationPageViewModels()
        {
            Text = OwnerViewModel?.CheckServeStatus() == false ? "网络故障中" : "设备暂停中";
            if (Text == "网络故障中")
            {
                DoInitFunction(null);
                return;
            }
            var time = dbHelper.S_Sysdate();
            if (string.IsNullOrEmpty(time))
            {

                Text = "应用服务器故障中";
            }
            var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
            DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
            {
                DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                CZ_PASSWORD = OwnerViewModel?.CZPassWord,
                DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                LOG_TYPE = Log_Type.INFO.ToString(),
                LOG_STATUS = Log_Status.PAUSE.ToString(),
                QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                START_USER = "",
                START_DATE = Now.ToDate(),
                LOG_MESSAGE = "管理员认证-系统管理-暂停服务",
                REMARK = ""
            };
            //推送设备软件使用记录
            dbHelper.I_Dev_Watch_Log(watchLog);
            this.DoInitFunction(null);
        }

        private ZHPHMachineWSHelper dbHelper = ZHPHMachineWSHelper.ZHPHInstance;
        DispatcherTimer timerHeartBeat = new DispatcherTimer();
        DispatcherTimer timers = new DispatcherTimer();
        public string text;
        public string Text
        {
            get { return text; }
            set { text = value; RaisePropertyChanged("Text"); }
        }
        public override void DoInitFunction(object obj)
        {
            //OwnerViewModel.TimeOutNew();
            timerHeartBeat.Tick += SendHeartBeatToServer;
            timerHeartBeat.Interval = TimeSpan.FromSeconds(15);
            timerHeartBeat.Start();

        }

        public override void DoNextFunction(object obj)
        {
            OnDispose();
            DoExitFunction(null);
            //base.DoNextFunction("MainPage");

        }

        private void SendHeartBeatToServer(object sender, EventArgs e)
        {
            if (OwnerViewModel?.CheckServeStatus() == false)
            {
                Log.Instance.WriteInfo("暂停页面-检测到无网络连接...");
                Text = "网络故障中";
            }
            else
            {
                Log.Instance.WriteInfo("暂停页面-开始重新连接服务...");
                Text = "设备暂停中";
                var time = dbHelper.S_Sysdate();
                Log.Instance.WriteInfo("暂停页面-获取到服务时间：" + time);
                if (string.IsNullOrEmpty(time))
                {
                    Text = "应用服务器故障中";
                }
                timerHeartBeat.Stop();
                DoNextFunction(null);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void OnDispose()
        {
            Log.Instance.WriteInfo("暂停页面-关闭计时器");
            //关闭串口
            BarCodeScanner.Instance.CloseBarCodeDev();
            TTS.StopSound();
            CommonHelper.OnDispose();
            base.OnDispose();
        }
    }
}
