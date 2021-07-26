using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Freedom.Models.TJJsonModels;
using Freedom.Models.ZHPHMachine;

namespace SFreedom.ZHPHMachine.ViewModels
{
    public class SystemManagementViewModels : ViewModelBase
    {
        private ZHPHMachineWSHelper dbHelper = ZHPHMachineWSHelper.ZHPHInstance;
        public SystemManagementViewModels()
        {
            this.TipMessage = "系统管理";

        }

        /// <summary>
        /// 退出主程序
        /// </summary>
        public ICommand ExitProgramComand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Instance.WriteInfo("系统管理-退出程序");
                    //获取服务器时间
                    //var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                    //Log.Instance.WriteInfo("设备id：" + QJTConfig.QJTModel.ZhDeviceInfo?.DEV_ID);
                    //DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
                    //{
                    //    DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                    //    CZ_PASSWORD = OwnerViewModel?.CZPassWord,
                    //    DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                    //    DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                    //    LOG_TYPE = "INFO",
                    //    LOG_STATUS = Log_Status.EXIT.ToString(),
                    //    QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                    //    STOP_DATE = Now.ToDate(),
                    //    LOG_MESSAGE = "管理员认证正常退出！",
                    //    REMARK = ""
                    //};
                    //推送设备软件使用记录
                    //dbHelper.I_Dev_Watch_Log(watchLog);
                    bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.OFFLINE);
                    //UploadMachineCondition();
                    //UploadRunningCondition();

                    this.ExitProgram();
                });
            }
        }

        /// <summary>
        /// 返回首页
        /// </summary>
        public ICommand BackHomeComand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Instance.WriteInfo("系统管理-返回首页");
                    this.DoExitFunction(true);
                });
            }
        }

        /// <summary>
        /// 重启软件
        /// </summary>
        public ICommand RestartComand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Instance.WriteInfo("系统管理-重启软件");
                    Application.Current.Shutdown();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);//重启软件
                });
            }
        }

        /// <summary>
        /// 暂停服务
        /// </summary>
        public ICommand StopDeviceCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Instance.WriteInfo("系统管理-暂停服务");
                    
                    UpdateDeviceStatus(EnumTypeSTATUS.PASUE);
                });
            }
        }

        /// <summary>
        /// 恢复服务
        /// </summary>
        public ICommand StartDeviceCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Instance.WriteInfo("系统管理-恢复服务");
                    //获取服务器时间
                    var Now = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
                    Log.Instance.WriteInfo("设备id：" + QJTConfig.QJTModel.ZhDeviceInfo?.DEV_ID);
                    DEV_WATCH_LOGS watchLog = new DEV_WATCH_LOGS
                    {
                        DEV_ID = QJTConfig.QJTModel.QJTDevInfo.DEV_ID.ToString(),
                        CZ_PASSWORD = OwnerViewModel?.CZPassWord,
                        DEV_OLDIP = QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString(),
                        DEV_NEWIP = Net.GetLanIp().Equals(QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString()) ? QJTConfig.QJTModel?.ZhDeviceInfo?.DEV_IP?.ToString() : Net.GetLanIp(),
                        LOG_TYPE = "INFO",
                        LOG_STATUS = Log_Status.ONLINE.ToString(),
                        QYCODE = QJTConfig.QJTModel?.ZhDeviceInfo?.QYCODE?.ToString(),
                        STOP_DATE = Now.ToDate(),
                        LOG_MESSAGE = "管理员认证-恢复服务",
                        REMARK = ""
                    };
                    //推送设备软件使用记录
                    dbHelper.I_Dev_Watch_Log(watchLog);

                    UpdateDeviceStatus(EnumTypeSTATUS.FREE);

                });
            }
        }

        #region 方法
        private void UpdateDeviceStatus(EnumTypeSTATUS status)
        {
            if (QJTConfig.QJTModel == null && QJTConfig.QJTModel.QJTDevInfo == null
                   && QJTConfig.QJTModel?.QJTDevInfo?.DEVICESTATUS == status.ToString())
            {
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    //修改设备状态为暂停
                    bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, status);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo($"管理员设置设备状态{status.ToString()}异常");
                    Log.Instance.WriteError($"管理员设置设备状态{status.ToString()}异常" + ex.Message);
                }

            });
            //不用等待服务器更新
            QJTConfig.QJTModel.QJTDevInfo.DEVICESTATUS = status.ToString();
            //返回首页
            this.DoExitFunction(true);
        }

        /// <summary>
        /// 太极上传设备状态
        /// </summary>
        /// <returns></returns>
        public static bool UploadMachineCondition()
        {
            Log.Instance.WriteInfo("======开始上传设备状态至太极=====");
            //如果接口获取失败，这样转空字符串会报异常  by 2021年7月7日10:25:32 wei.chen
            //var nowTime = Convert.ToDateTime().ToString("yyyyMMddHHmmss");
            string nowTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
            if (nowTime.IsNotEmpty())
            {
                nowTime = Convert.ToDateTime(nowTime).ToString("yyyyMMddHHmmss");
                Json_I_SB_Monitor_MachineCondition sbMonitor = new Json_I_SB_Monitor_MachineCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    sbzt = "00",
                    sbsj = nowTime
                };
                var result = new TaiJiHelper().Do_SB_Monitor_MachineCondition(sbMonitor);
            }

            Log.Instance.WriteInfo("======结束上传设备状态至太极=====");
            return true;
        }

        /// <summary>
        /// 上传太极设备运行状态
        /// </summary>
        /// <returns></returns>
        public bool UploadRunningCondition()
        {
            Log.Instance.WriteInfo("======开始上传设备运行状态至太极=====");
            //如果接口获取失败，这样转空字符串会报异常  by 2021年7月7日10:25:32 wei.chen
            //var nowTime = Convert.ToDateTime().ToString("yyyyMMddHHmmss");
            string nowTime = ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate();
            if (nowTime.IsNotEmpty())
            {
                nowTime = Convert.ToDateTime(nowTime).ToString("yyyyMMddHHmmss");
                Json_I_SB_Monitor_RunningCondition sbMonitor = new Json_I_SB_Monitor_RunningCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    yxzt = "00",
                    sbsj = nowTime
                };
                return new TaiJiHelper().Do_SB_Monitor_RunningCondition(sbMonitor);
            }

            Log.Instance.WriteInfo("======结束上传设备运行状态至太极=====");
            return true;
        }
        #endregion
    }
}
