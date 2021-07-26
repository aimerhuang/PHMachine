using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Freedom.BLL;
using Freedom.Common;
using Freedom.Config;
using Freedom.Models;
using Freedom.Models.TJJsonModels;

namespace Freedom.ZHPHMachine.Command
{
    public class TaiJiHelper
    {
        public CrjPreapplyManager crjManager = new CrjPreapplyManager();
        /// <summary>
        /// 获取单位可办理业务
        /// </summary>
        /// <param name="dwid"></param>
        /// <returns></returns>
        public bool Common_searchYwlb(string dwid)
        {

            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            var returnInfo = crjManager.Common_searchYwlb(dwid);
            if (returnInfo != null && returnInfo.IsSucceed)
            {
                Log.Instance.WriteInfo("查询单位可办业务返回【成功】");
                return returnInfo.IsSucceed;
            }
            Log.Instance.WriteInfo("查询单位可办业务返回【失败】");
            return false;

            //new YwlbInfo
            //{
            //    bzlb = "1",
            //    jzblbs = "123,123".Split(),
            //    qzblbs = "123".Split(),
            //};
            //return returnInfo;
            //string a =
            //    "{\"success\":\"1\",\"stateCode\":\"KTJ0000\",\"stateDesc\":\"处理成功\",\"moreinfo\":null,\"data\":\"{\\\"ywlbs\\\":[{\\\"bzlb\\\":\\\"91\\\",\\\"qzzls\\\":[\\\"22\\\",\\\"23\\\",\\\"26\\\",\\\"29\\\",\\\"2A\\\"],\\\"sqlb\\\":\\\"104\\\"},{\\\"bzlb\\\":\\\"92\\\",\\\"qzzls\\\":[\\\"19\\\",\\\"1G\\\"],\\\"sqlb\\\":\\\"102\\\"},{\\\"bzlb\\\":\\\"93\\\",\\\"jzzls\\\":[\\\"11\\\",\\\"18\\\",\\\"19\\\",\\\"1A\\\"],\\\"sqlb\\\":\\\"101\\\"},{\\\"bzlb\\\":\\\"31\\\",\\\"qzzls\\\":[\\\"19\\\",\\\"1G\\\"],\\\"sqlb\\\":\\\"102\\\"},{\\\"bzlb\\\":\\\"31\\\",\\\"jzzls\\\":[\\\"11\\\",\\\"14\\\",\\\"18\\\",\\\"19\\\",\\\"1A\\\"],\\\"sqlb\\\":\\\"101\\\"},{\\\"bzlb\\\":\\\"11\\\",\\\"qzzls\\\":[\\\"19\\\",\\\"1G\\\"],\\\"sqlb\\\":\\\"102\\\"},{\\\"bzlb\\\":\\\"31\\\",\\\"qzzls\\\":[\\\"22\\\",\\\"23\\\",\\\"26\\\",\\\"29\\\",\\\"2A\\\"],\\\"sqlb\\\":\\\"104\\\"},{\\\"bzlb\\\":\\\"11\\\",\\\"jzzls\\\":[\\\"11\\\",\\\"18\\\",\\\"19\\\"],\\\"sqlb\\\":\\\"101\\\"},{\\\"bzlb\\\":\\\"21\\\",\\\"qzzls\\\":[\\\"19\\\",\\\"1G\\\"],\\\"sqlb\\\":\\\"102\\\"},{\\\"bzlb\\\":\\\"13\\\",\\\"qzzls\\\":[\\\"19\\\",\\\"1G\\\"],\\\"sqlb\\\":\\\"102\\\"},{\\\"bzlb\\\":\\\"11\\\",\\\"qzzls\\\":[\\\"22\\\",\\\"23\\\",\\\"26\\\",\\\"29\\\",\\\"2A\\\"],\\\"sqlb\\\":\\\"104\\\"},{\\\"bzlb\\\":\\\"13\\\",\\\"jzzls\\\":[\\\"11\\\",\\\"18\\\",\\\"19\\\",\\\"1A\\\"],\\\"sqlb\\\":\\\"101\\\"},{\\\"bzlb\\\":\\\"21\\\",\\\"qzzls\\\":[\\\"22\\\",\\\"23\\\",\\\"26\\\",\\\"29\\\",\\\"2A\\\"],\\\"sqlb\\\":\\\"104\\\"},{\\\"bzlb\\\":\\\"13\\\",\\\"qzzls\\\":[\\\"22\\\",\\\"23\\\",\\\"26\\\",\\\"29\\\",\\\"2A\\\"],\\\"sqlb\\\":\\\"104\\\"}]}\"}";
            //Log.Instance.WriteInfo("查询单位可办业务 返回：" + returnInfo.ReturnValue);
            //TjReturnInfo tjInfo = JsonHelper.ConvertToObject<TjReturnInfo>(a);
            //var YwlbRec = JsonHelper.ConvertToObject<YwlbList>(tjInfo?.data);
            //return null;
        }

        /// <summary>
        /// 查询太极导服上传
        /// </summary>
        /// <param name="dyUpload"></param>
        /// <returns></returns>
        public bool Do_DY_Upload(Json_I_DY_upload dyUpload)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            ReturnInfo returnInfo = crjManager.Do_DY_Upload(dyUpload);
            //TjReturnInfo tjInfo = JsonHelper.ConvertToObject<TjReturnInfo>(JsonHelper.ToJson(returnInfo));
            //Log.Instance.WriteInfo("太极返回：" + JsonHelper.ToJson(returnInfo.ReturnValue));
            return returnInfo.IsSucceed;
            //return false;
        }

        /// <summary>
        /// 流程上报
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public bool Common_saveFlow(Json_I_Common_saveFlow flow)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            //Log.Instance.WriteInfo("流程上报：" + JsonHelper.ToJson(flow));
            ReturnInfo returnInfo = crjManager.Common_saveFlow(flow);
            //TjReturnInfo tjInfo = JsonHelper.ConvertToObject<TjReturnInfo>(JsonHelper.ToJson(returnInfo));
            if (returnInfo.IsSucceed)
            {
                Log.Instance.WriteInfo("导服流程上报成功！");
                return true;
            }
            else
            {
                Log.Instance.WriteInfo("导服流程上报失败！");
                return false;
            }
        }

        /// <summary>
        /// 排队叫号数据上报
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public bool Common_saveQueue(Json_I_Common_saveQueue queue)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            //Log.Instance.WriteInfo("排队叫号数据上报：" + JsonHelper.ToJson(queue));
            ReturnInfo returnInfo = crjManager.Common_saveQueue(queue);
            //TjReturnInfo tjInfo = JsonHelper.ConvertToObject<TjReturnInfo>(JsonHelper.ToJson(returnInfo));
            if (returnInfo.IsSucceed)
            {
                Log.Instance.WriteInfo("排队叫号数据上报成功！");
                return true;
            }
            else
            {
                Log.Instance.WriteInfo("排队叫号数据上报失败");
                return false;
            }

        }

        /// <summary>
        /// 获取导服能否进自助大厅
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public DY_result_Rec DY_result(Json_I_DY_Result result)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                string path = Path.Combine(FileHelper.GetLocalPath(), "ViewModels\\Test\\DY_Result.json");
                if (File.Exists(path))
                {
                    string str = FileHelper.ReadFileContent(path);
                    return JsonHelper.ConvertToObject<DY_result_Rec>(str);
                }
                else
                {
                    return null;
                }
            }
            ReturnInfo returnInfo = crjManager.Do_DY_Result(result);
            if (returnInfo?.ReturnValue != null && returnInfo?.IsSucceed == true)
            {
                var resultRec = JsonHelper.ConvertToObject<DY_result_Rec>(returnInfo?.ReturnValue?.ToString());
                return resultRec;
            }
            else
            {
                Log.Instance.WriteInfo("导服核查是否可进自助大厅返回失败！");
            }
            return null;
        }

        /// <summary>
        /// 太极登录
        /// </summary>
        /// <returns>授权码</returns>
        public ReturnInfo login()
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return null;
            }
            Json_I_DY_login dyLogin = new Json_I_DY_login
            {
                loginName = TjConfig.TjModel.TjUserName,
                password = TjConfig.TjModel.TjUserPwd,
                jqbh = TjConfig.TjModel.TJMachineNo
            };
            Log.Instance.WriteInfo("开始太极登录：" + TjConfig.TjModel.TjUserName + TjConfig.TjModel.TjUserPwd + TjConfig.TjModel.TJMachineNo);
            ReturnInfo returnInfo = crjManager.DoLoginByTJ(dyLogin);
            if (returnInfo.ReturnValue != null && returnInfo.IsSucceed)
            {
                Log.Instance.WriteInfo("太极登录成功！");
            }
            else
            {
                Log.Instance.WriteInfo("太极登录失败！");
            }
            return returnInfo;
        }


        /// <summary>
        /// 太极导服登录
        /// </summary>
        /// <param name="dyLogin"></param>
        /// <returns></returns>
        public Json_R_DY_login_Rec DO_DY_Login(Json_I_DY_login dyLogin)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                string path = Path.Combine(FileHelper.GetLocalPath(), "ViewModels\\Test\\DY_Login.json");
                //Log.Instance.WriteInfo("开始测试模式登录太极....");
                if (File.Exists(path))
                {
                    string str = FileHelper.ReadFileContent(path);
                    return JsonHelper.ConvertToObject<Json_R_DY_login_Rec>(str);
                }
                else
                {
                    return null;
                }
            }
            ReturnInfo login = new TaiJiHelper().login();
            if (login?.ReturnValue != null && login?.IsSucceed == true)
            {
                ReturnInfo returnInfo = crjManager.Do_DY_Login(dyLogin);
                if (returnInfo.ReturnValue != null && returnInfo.IsSucceed)
                {
                    var loginRec = JsonHelper.ConvertToObject<Json_R_DY_login_Rec>(returnInfo.ReturnValue.ToString());
                    Log.Instance.WriteInfo("导服登录接口查询返回：【成功】");
                    return loginRec;
                }
                else
                {
                    Log.Instance.WriteInfo("导服登录接口查询返回：【失败】");//+returnInfo?.IsSucceed
                }
                return null;
            }
            else
            {
                Log.Instance.WriteInfo("太极登录失败：" + login?.MessageInfo);
                return null;
            }
        }

        /// <summary>
        /// 查询太极导服网上预约数据
        /// </summary>
        /// <param name="sfzh"></param>
        /// <returns></returns>
        public ReturnInfo Common_queryAppointment(string sfzh)
        {
            ReturnInfo returnInfo = crjManager.Common_queryAppointment(sfzh);
            if (returnInfo.ReturnValue != null && returnInfo.IsSucceed)
            {
                Log.Instance.WriteInfo("导服核查接口查询成功！");
            }
            else
            {
                Log.Instance.WriteInfo("导服核查接口查询失败！");
            }
            return returnInfo;
        }

        /// <summary>
        /// 查询太极导服核查
        /// </summary>
        /// <param name="dyCheck"></param>
        /// <returns></returns>
        public Json_R_DY_check_Rec Do_DY_Check(Json_I_DY_check dyCheck)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                string path = Path.Combine(FileHelper.GetLocalPath(), "ViewModels\\Test\\DY_Check.json");
                if (File.Exists(path))
                {
                    string str = FileHelper.ReadFileContent(path);
                    return JsonHelper.ConvertToObject<Json_R_DY_check_Rec>(str);
                }
                else
                {
                    return null;
                }
            }
            ReturnInfo returnInfo = crjManager.Do_DY_Check(dyCheck);
            if (returnInfo?.ReturnValue != null && returnInfo?.IsSucceed == true)
            {
                var checkRec = JsonHelper.ConvertToObject<Json_R_DY_check_Rec>(returnInfo?.ReturnValue.ToString());
                return checkRec;

            }
            else
            {
                Log.Instance.WriteInfo("导服核查接口查询失败！");
            }
            return null;
        }

        /// <summary>
        /// 特殊人员导服核查接口
        /// </summary>
        /// <param name="dycheck"></param>
        /// <returns></returns>
        public Json_R_DY_check_Rec DY_tszhCheck(Json_I_DY_check dycheck)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                string path = Path.Combine(FileHelper.GetLocalPath(), "ViewModels\\Test\\DY_Check.json");
                if (File.Exists(path))
                {
                    string str = FileHelper.ReadFileContent(path);
                    return JsonHelper.ConvertToObject<Json_R_DY_check_Rec>(str);
                }
                else
                {
                    return null;
                }
            }
            ReturnInfo returnInfo = crjManager.Do_DY_Check(dycheck);
            if (returnInfo?.ReturnValue != null && returnInfo?.IsSucceed == true)
            {
                var checkRec = JsonHelper.ConvertToObject<Json_R_DY_check_Rec>(returnInfo?.ReturnValue.ToString());
                return checkRec;

            }
            else
            {
                Log.Instance.WriteInfo("导服核查接口查询失败！");
            }
            return null;
        }

        public Json_R_DY_check_Rec PH_CheckZHPH(Json_I_DY_check dycheck)
        {
            if (!QJTConfig.QJTModel.IsConnDBServer)
                return null;

            return null;
        }


        /// <summary>
        /// 5.10	生成业务编号
        /// </summary>
        /// <param name="dwid">行政区划（字典DM_XZQHB）</param>
        /// <returns>业务编号</returns>
        public Json_R_Common_makeBarcode Do_Common_MakeBarcode(string dwid, string xxly)
        {
            //if (!QJTConfig.QJTModel.IsConnDBServer)
            //{
            //    return new Json_R_Common_makeBarcode { ywbh = "KFS150000005615" };
            //}
            if (string.IsNullOrEmpty(xxly))
                xxly = "00";
            ReturnInfo returnInfo = crjManager.Do_Common_makeBarcode(dwid, xxly);//前台受理
            if (returnInfo?.ReturnValue != null && returnInfo?.IsSucceed == true)
            {
                var barcode = JsonHelper.ConvertToObject<Json_R_Common_makeBarcode>(returnInfo?.ReturnValue.ToString());
                return barcode;
            }
            else
            {
                Log.Instance.WriteInfo("获取业务编号失败！");
                return null;
            }
        }

        /// <summary>
        /// 上传设备开机状态
        /// </summary>
        /// <param name="sbMonitor"></param>
        /// <returns></returns>
        public bool Do_SB_Monitor_MachineCondition(Json_I_SB_Monitor_MachineCondition sbMonitor)
        {
            //Log.Instance.WriteInfo("上传设备状态参数：" + JsonHelper.ToJson(sbMonitor));
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            ReturnInfo returnInfo = crjManager.Do_SB_Monitor_MachineCondition(sbMonitor);//前台受理
            if (returnInfo.IsSucceed)
            {
                //Log.Instance.WriteInfo("上传设备状态成功！");
                return true;
            }
            else
            {
                Log.Instance.WriteInfo("上传设备状态失败");
                return false;
            }
        }

        /// <summary>
        /// 上传设备开机状态
        /// </summary>
        /// <param name="sbMonitor"></param>
        /// <returns></returns>
        public bool Do_SB_Monitor_RunningCondition(Json_I_SB_Monitor_RunningCondition sbMonitor)
        {
            //Log.Instance.WriteInfo("上传设备运行状态参数：" + JsonHelper.ToJson(sbMonitor));
            if (!QJTConfig.QJTModel.IsConnDBServer)
            {
                return true;
            }
            ReturnInfo returnInfo = crjManager.Do_SB_Monitor_RunningCondition(sbMonitor);//前台受理
            if (returnInfo.IsSucceed)
            {
                //Log.Instance.WriteInfo("上传设备状态成功！");
                return true;
            }
            else
            {
                Log.Instance.WriteInfo("上传设备状态失败");
                return false;
            }
        }


        /// <summary>
        /// 导引区域
        /// </summary>
        public enum dyqy
        {

            [Description("一站式")]
            OneZhan = 0,
            [Description("一桌式")]
            OneZhuo = 1,
        }

        /// <summary>
        /// 排队类别
        /// </summary>
        public enum pdlb
        {
            [Description("一桌式")]
            OneZhuo = 106,
            [Description("一站式")]
            OneZhan = 104,
        }
    }
}
