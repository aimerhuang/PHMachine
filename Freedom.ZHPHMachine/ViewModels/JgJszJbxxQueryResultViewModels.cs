using Freedom.Common;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.JgModels;
using Freedom.ZHPHMachine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineCommandService;
using Freedom.ZHPHMachine.Common;
using Freedom.BLL;
using Freedom.Models.CrjCreateJsonModels;

namespace SFreedom.ZHPHMachine.ViewModels
{
    public class JgJszJbxxQueryResultViewModels : ViewModelBase
    {
        private PH_YYSQXX_TB baseInfo;
        private CrjPreapplyManager crjManager;

        /// <summary>
        /// 预约基本信息
        /// </summary>
        public PH_YYSQXX_TB BaseInfo
        {
            get { return this.baseInfo; }
            set { this.baseInfo = value; this.RaisePropertyChanged("BaseInfo"); }
        }

        private string _NextButtonName;
        public string NextButtonName
        {
            get { return this._NextButtonName; }
            set { this._NextButtonName = value; this.RaisePropertyChanged("NextButtonName"); }
        }

        private string exceptionMsg;

        /// <summary>
        /// 异常信息提示
        /// </summary>
        public string ExceptionMsg
        {
            get { return exceptionMsg; }
            set
            {
                exceptionMsg = value;
                RaisePropertyChanged("ExceptionMsg");
            }
        }

        private bool isException;

        /// <summary>
        /// 是否异常
        /// </summary>
        public bool IsException
        {
            get { return isException; }
            set { isException = value; RaisePropertyChanged("IsException"); }
        }

        public override void DoInitFunction(object obj)
        { 
            Log.Instance.WriteInfo("进入查询预约信息界面");
            TTS.PlaySound("查询");
            //查询预约信息
            OwnerViewModel?.IsShowHiddenLoadingWait("正在查询预约信息,请稍等...");

            Task.Run(async () =>
            {
                try
                {
                    if (crjManager == null) { crjManager = new CrjPreapplyManager(); }

                    //获取身份证信息
                    var model = ServiceRegistry.Instance.Get<ElementManager>().Get<IdCardInfo>();
                    //身份证号码
                    string idCardNo = model.IDCardNo; 
                    //判断是否是国家公务人员
                    bool isOfficial = QueryOfficialAsync(model);
                    if (isOfficial)
                    {
                        Log.Instance.WriteInfo("公务人员，需要人工办理");
                        ExceptionMsg = "公务人员,请人工办理。";
                        IsException = true;
                        return;
                    }
                    //判断是否是控制对象
                    bool isCheckSpecil = CheckSpecialList(idCardNo);
                    if (isCheckSpecil)
                    {
                        Log.Instance.WriteInfo("控制对象，需要人工办理");
                        ExceptionMsg = "控制对象,请人工办理。";
                        IsException = true;
                        return;
                    }
                    //查询当前大厅当天此人预约信息
                    //BaseInfo = await ZHPHMachineWSHelper.ZHPHInstance.S_YYSQXX(idCardNo, "", DateTime.Now);
                    //未查询到预约信息
                    if (BaseInfo == null)
                    {
                        this.DoNext.Execute("Booking/BookingTarget");
                    }
                    else //查询到预约信息
                    {
                        //添加预约信息
                        ServiceRegistry.Instance.Get<ElementManager>().Set<PH_YYSQXX_TB>(BaseInfo);
                        this.DoNext.Execute("ScanningPhotoReceipt");
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.WriteInfo("预约信息查询异常");
                    Log.Instance.WriteError($"预约信息查询异常:{e.Message}");
                    IsException = true;
                }
                finally
                {
                    OwnerViewModel?.IsShowHiddenLoadingWait();
                }
            });
        }

        protected override void OnDispose()
        {
            TTS.StopSound(); 
            base.OnDispose();
        }

        public override void DoNextFunction(object obj)
        {
            //判断是否有照片(如果有照片直接派号否则跳转扫描回执)
            bool isImg = false;
            if (!isImg)
            {
                base.DoNextFunction(obj);
            }
            else
            {
                base.DoNextFunction("MainPage");
            }

        }

        #region 方法

        /// <summary>
        ///  核查是否国家工作人员
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool QueryOfficialAsync(IdCardInfo info)
        {
            bool isOfficial = false;
            try
            {
                Log.Instance.WriteInfo("核查国家公务人员"); 

                var result = crjManager.QueryOfficial(info.IDCardNo, info.FullName, info.Gender, info.Birthday);

                if (int.TryParse(result.ReturnValue?.ToString(), out int returnValue))
                {
                    isOfficial = returnValue == 1;
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteInfo("核查国家公务人员异常");
                Log.Instance.WriteError($"核查国家公务人员异常:{e.Message}");
                throw e;
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
            try
            {
                Log.Instance.WriteInfo("核查受控对象");
                var model = new CheckSpecialInfo()
                {
                    sfzh = idCardNo,
                };
                var result = crjManager.CheckSpecialList(model);
                if (int.TryParse(result.ReturnValue?.ToString(), out int returnValue))
                {
                    isCheckSpecil = returnValue == 1;
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteInfo("核查受控对象异常");
                Log.Instance.WriteError($"核查受控对象异常:{e.Message}");
                throw e;
            }
            return isCheckSpecil;
        }
        #endregion

    }
}
