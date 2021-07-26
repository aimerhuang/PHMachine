using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Config;
using Freedom.Hardware;
using Freedom.ZHPHMachine.Common;
using Freedom.ZHPHMachine.ViewModels;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Freedom.Models;
using Freedom.Models.TJJsonModels;
using Freedom.ZHPHMachine.Command;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Freedom.ZHPHMachine
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 启动事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //------注册全局对象
            ServiceRegistry.Instance.Register<ElementManager>(new ElementManager());


            //获取当前进程id
            Process proceMain = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)//获取所有同名进程id
            {
                if (process.ProcessName == proceMain.ProcessName)
                {
                    if (process.Id != proceMain.Id)//根据进程id删除所有除本进程外的所有相同进程
                        process.Kill();
                }
            }
            base.OnStartup(e);  
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.Instance.WriteInfo("正在准备退出程序……");
                //修改设备状态为离线
                bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.OFFLINE);
                if (TjConfig.TjModel.IsConnectionTj)
                {
                    UploadMachineCondition();
                    UploadRunningCondition();
                }
                //if (blnResult)
                //{
                //    Log.Instance.WriteInfo($"{QJTConfig.QJTModel.QJTDevInfo.DEV_NAME}[{QJTConfig.QJTModel.QJTDevInfo.DEV_NO}][{EnumType.GetEnumDescription(EnumTypeSTATUS.OFFLINE)}]<退出程序>");
                //} 
                //获取服务器时间
                var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                Log.Instance.WriteInfo("设备id：" + QJTConfig.QJTModel.ZhDeviceInfo?.DEV_ID);
                DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
                {
                    DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                    CZ_PASSWORD = "",
                    DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                    DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                    LOG_TYPE = "ERROR",
                    LOG_STATUS = Log_Status.ERROR.ToString(),
                    QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                    STOP_DATE = Now.ToDate(),
                    LOG_MESSAGE = "退出程序",
                    REMARK = ""
                };
                //推送设备软件使用记录
                ZHPHMachineWSHelper.ZHPHInstance.I_Dev_Watch_Log(watchLog);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("退出程序修改设备状态失败");
                Log.Instance.WriteError(ex.Message);
            }

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Log.Instance.WriteError("非UI线程异常：" + ex?.Message);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Instance.WriteError("Task线程异常：" + e.Exception.Message);
            e.SetObserved();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Instance.WriteError("UI线程异常：" + e.Exception.Message);
            //var viewmodel = App.Current.MainWindow.DataContext as MainWindowViewModels;
            //viewmodel?.MessageTips(TipMsgResource.GlobalExceptionTipMsg, () =>
            //{
            //    viewmodel?.DoExit.Execute(null); 
            //});
            e.Handled = true;
        }

        /// <summary>
        /// 太极上传设备状态
        /// </summary>
        /// <returns></returns>
        public static bool UploadMachineCondition()
        {
            var nowTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_MachineCondition sbMonitor = new Json_I_SB_Monitor_MachineCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    sbzt = "00",
                    sbsj = nowTime
                };
                var result = new TaiJiHelper().Do_SB_Monitor_MachineCondition(sbMonitor);
                Log.Instance.WriteInfo(result? "上传设备状态关机【成功】" : "上传设备状态关机【失败】");
            }
            return true;
        }

        /// <summary>
        /// 上传太极设备运行状态
        /// </summary>
        /// <returns></returns>
        public bool UploadRunningCondition()
        {
            var nowTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_RunningCondition sbMonitor = new Json_I_SB_Monitor_RunningCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    yxzt = "00",
                    sbsj = nowTime
                };
                var result=new TaiJiHelper().Do_SB_Monitor_RunningCondition(sbMonitor);
                Log.Instance.WriteInfo(result ? "上传设备运行状态【成功】" : "上传设备运行状态【失败】");
            }
            return true;
        }
    }
}
