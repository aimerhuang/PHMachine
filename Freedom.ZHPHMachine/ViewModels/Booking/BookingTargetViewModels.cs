using Freedom.Common;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using MachineCommandService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Freedom.Config;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class BookingTargetViewModels : ViewModelBase
    {

        #region 属性

        private List<BookingTargetModel> targetModels = new List<BookingTargetModel>();

        private List<BookingTargetModel> bookingTagetInfo;

        /// <summary>
        /// 受理单位当天预约指标
        /// </summary>
        public List<BookingTargetModel> BookingTagetInfo
        {
            get { return bookingTagetInfo; }
            set { bookingTagetInfo = value; RaisePropertyChanged("BookingTagetInfo"); }
        }

        private DateTime selectDate = DateTime.Now;
        /// <summary>
        /// 选中预约日期
        /// </summary>
        public DateTime SelectDate
        {
            get { return selectDate; }
            set { selectDate = value; RaisePropertyChanged("SelectDate"); }
        }

        public ICommand TextBoxGotFocusCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CommonHelper.PopupDate(OperaType.Date, SelectDate.ToString("yyyy年MM月dd日"), out DateTime str))
                    {
                        //判断是否是重复选择
                        var newDate = str.ToString("yyyy-MM-dd");
                        var date = selectDate.ToString("yyyy-MM-dd");
                        if (!newDate.Equals(date))
                        {
                            SelectDate = str;
                            GetBookingTarget(selectDate);
                        }
                    }
                });
            }
        }
        #endregion

        #region 方法 
        public async void GetBookingTarget(DateTime date)
        {
            try
            {
                OwnerViewModel?.IsShowHiddenLoadingWait("查询大厅预约时间段，请稍等.....");
                TTS.PlaySound("预约机-页面-完善预约时间");
                Log.Instance.WriteInfo("开始查询【" + DateTime.Today.ToString() + "】预约指标");
                var result = await ZHPHMachineWSHelper.ZHPHInstance.S_BookingZB(date);

                if (result?.Count <= 0)
                {
                    InitHardware(date);
                    if (targetModels?.Count <= 0)
                    {
                        Log.Instance.WriteInfo("预约时间段查询为空");
                        TTS.PlaySound("预约机-提示-无预约时段");
                        //未查询到预约信息
                        OwnerViewModel?.MessageTips($"{date.ToString("yyyy年MM月dd日")} 没有可预约时段,请您选择其它日期");
                    }

                }
                else
                {
                    Log.Instance.WriteInfo("预约时间段查询总数为：" + result?.Count + "");
                    BookingTagetInfo = result;
                }

                //北京房山流程简化，默认时间段为当天08:00-18:00
                if (OwnerViewModel?.IsBeijing == true || OwnerViewModel?.IsWuHan == true || OwnerViewModel?.IsShenZhen == true || OwnerViewModel?.IsTakePH_No == true || BookingBaseInfo?.Book_Type == "0" || QJTConfig.QJTModel.IsTodayPH)
                {
                    Log.Instance.WriteInfo("流程简化，默认填写预约时间段开始：");
                    if (result != null && result.Count > 0)
                    {
                        BookingBaseInfo.BookingTarget = result.First();
                    }
                    else
                    {
                        BookingBaseInfo.BookingTarget = new BookingTargetModel()
                        {
                            BookingDate = DateTime.Now.ToString("yyyyMMdd"),
                            StartTime = "08:00",
                            EndTime = "18:00"
                        };
                    }

                    if (OwnerViewModel?.IsBeijing == true)
                    {
                        DoNextFunction("Booking/BookingBaseInfoByBeijing");
                    }
                    else
                    {
                        this.DoNextFunction("Booking/BookingBaseInfo");
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Instance.WriteInfo("查询预约时间段异常");
                Log.Instance.WriteError("查询预约时间段异常" + ex.Message);
                OwnerViewModel?.MessageTips(ex.Message, (() =>
                {
                    this.DoExit.Execute(null);
                }));
            }
            finally
            {
                Log.Instance.WriteInfo("结束查询【" + DateTime.Today.ToString() + "】预约指标");
                Log.Instance.WriteInfo("\n*********************** 结束【选择预约时间】页面***********************");
                //Log.Instance.WriteInfo($"选择预约时间段{BookingBaseInfo.BookingTarget.BookingDt?.ToString("yyyy年MM月dd日")} {BookingBaseInfo.BookingTarget.Title}");
                OwnerViewModel?.IsShowHiddenLoadingWait();
            }

        }

        public override void DoInitFunction(object obj)
        {
            Log.Instance.WriteInfo("\n*********************** 进入【选择预约时间】页面***********************");
            if (OwnerViewModel?.CheckServeStatus() == false)
            {
                DoNextFunction("NotificationPage");
            }
            Log.Instance.WriteInfo("检测网络连接正常");

            if (BookingBaseInfo == null)
            {
                OwnerViewModel?.MessageTips("查询预约基础信息发生错误，请重试！", (() =>
                {
                    this.DoExit.Execute(null);
                }));
                return;
            }

            if (BookingBaseInfo?.BookingTarget?.BookingDate != null)
            {
                SelectDate = BookingBaseInfo?.BookingTarget?.BookingDt ?? DateTime.Now;
                //Log.Instance.WriteInfo($"查询到预约信息：预约时间段{BookingBaseInfo.BookingTarget.BookingDt?.ToString("yyyy年MM月dd日")} {BookingBaseInfo.BookingTarget.Title}");
                //DoNextFunction(null);
            }

            GetBookingTarget(selectDate);
            this.TipMessage = "请选择本单位的预约时间段";
        }

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }

        public override void DoNextFunction(object obj)
        {
            if (BookingBaseInfo?.BookingTarget == null || (!BookingBaseInfo.BookingTarget.IsEnable && OwnerViewModel?.IsBeijing == false))
            {
                Log.Instance.WriteInfo("选中预约时间段：" + BookingBaseInfo?.BookingTarget + "\n选中时间段状态：" + BookingBaseInfo?.BookingTarget?.IsEnable);
                TTS.PlaySound("完善预约时间");
                OwnerViewModel?.MessageTips(TipMsgResource.NonEmptySelectTipMsg);
                return;
            }
            Log.Instance.WriteInfo("预约时间：" + BookingBaseInfo?.BookingTarget?.BookingDate + "-" + BookingBaseInfo?.BookingTarget?.StartTime + "-" + BookingBaseInfo?.BookingTarget?.EndTime);
            //Log.Instance.WriteInfo("=====离开选择预约时间段页面=====");
            //Log.Instance.WriteInfo("下一步：进入基础信息页面");

            //关闭倒计时
            //OnDispose();
            if (OwnerViewModel?.IsBeijing == true)
            {
                base.DoNextFunction("Booking/BookingBaseInfoByBeijing");//ByBeijing
            }
            else if (OwnerViewModel?.IsWuHan == true)
            {
                base.DoNextFunction("Booking/BookingBaseInfoByWuHan");
            }
            else
            {
                base.DoNextFunction("Booking/BookingBaseInfo");

            }

        }

        /// <summary>
        /// 重新获取大厅预约时间段
        /// </summary>
        public async void InitHardware(DateTime date)
        {

            for (int i = 0; i <= 3; i++)
            {
                var result = await ZHPHMachineWSHelper.ZHPHInstance.S_BookingZB(date);
                if (result?.Count > 0)
                {
                    targetModels = result;
                    //Log.Instance.WriteInfo("重新获取大厅预约时间段成功！");
                    break;
                }
            }
            targetModels = new List<BookingTargetModel>();
        }

        #endregion
    }
}
