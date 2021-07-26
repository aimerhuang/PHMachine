using Freedom.Common;
using Freedom.Controls.Foundation;
using Freedom.Models;
using Freedom.Models.ZHPHMachine;
using Freedom.ZHPHMachine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Freedom.ZHPHMachine.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        #region 成员 

        /// <summary>
        /// 取消任务
        /// </summary>
        public CancellationTokenSource cts;

        /// <summary>
        /// 所有者
        /// </summary>
        public Window Owner = App.Current.MainWindow;

        /// <summary>
        /// 获取主程序控制类
        /// </summary>
        public MainWindowViewModels OwnerViewModel = null;

        /// <summary>
        /// 内容页
        /// </summary>
        public Page ContentPage;

        /// <summary>
        /// 超时回调
        /// </summary>
        public Action TimeOutCallBack = null;

        /// <summary>
        /// 计时器
        /// </summary>
        public System.Timers.Timer timer;

        /// <summary>
        /// 常用线程
        /// </summary>
        public System.Threading.Thread thCommon;

        /// <summary>
        /// 倒计时
        /// </summary>
        public System.Threading.Thread thCountdown;

        /// <summary>
        /// 监控网络是否断开线程
        /// </summary>
        public Thread thNetWork;

        #endregion

        #region 属性

        /// <summary>
        /// 是否暂停倒计时
        /// </summary>
        public bool IsStop { get; set; } = false;

        /// <summary>
        /// 预约基础信息
        /// </summary>
        public BookingModel BookingBaseInfo
        {
            get
            {
                return ServiceRegistry.Instance.Get<ElementManager>().Get<BookingModel>();
            }
        }

        private string strCountdownValue;
        /// <summary>
        /// 倒计时
        /// </summary>
        public string StrCountdownValue
        {
            get { return this.strCountdownValue; }
            set
            {
                this.strCountdownValue = value;
                RaisePropertyChanged("StrCountdownValue");
            }

        }

        /// <summary>
        /// 设置页面提示语句
        /// </summary>
        public string TipMessage
        {
            set
            {
                if (OwnerViewModel != null)
                {
                    OwnerViewModel.TipMessage = value;
                }
            }
        }

        /// <summary>
        /// 设置是否显示上一页
        /// </summary>
        public Visibility PreviousPageShow
        {
            set
            {
                if (OwnerViewModel != null)
                {
                    OwnerViewModel.PreviousPageShow = value;
                }
            }
        }

        /// <summary>
        /// 设置是否显示下一页
        /// </summary>
        public Visibility NextPageShow
        {
            set
            {
                if (OwnerViewModel != null)
                {
                    OwnerViewModel.NextPageShow = value;
                }
            }
        }

        #endregion

        #region 命令

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public ViewModelBase()
        {
            OwnerViewModel = Owner?.DataContext as MainWindowViewModels;

            this.DoInit?.Execute(null);
        }

        #endregion

        #region 方法  

        private void ClearData()
        {
            this.TipMessage = string.Empty;
            this.NextPageShow = Visibility.Collapsed;
            this.PreviousPageShow = Visibility.Collapsed;
        }

        /// <summary>
        /// 返回入口
        /// </summary>
        /// <param name="obj"></param>
        public override void DoExitFunction(object obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                //允许不再系统页面 或者 在系统页面点击返回首页 
                bool isAllow = obj != null || this.OwnerViewModel?.ContentPage?.Title.Equals("系统") == false;
                if (!isAllow)
                {
                    return;
                }
                this.Dispose();
                //清空页面缓存对象
                ServiceRegistry.Instance.Get<ElementManager>().Remove<BookingModel>();
                ClearData();
                OwnerViewModel?.ContentPageSetting("MainPage", isAllow);
            });
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="obj"></param>
        public override void DoBackFunction(object obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Dispose();
                ClearData();
                if (obj is Page)
                {
                    Page page = (Page)obj;
                    if (page.NavigationService != null && page.NavigationService.CanGoBack)
                    {
                        page.NavigationService.GoBack();
                    }
                }
            });
        }

        /// <summary>
        /// 下一步
        /// </summary>
        /// <param name="obj"></param>
        public override void DoNextFunction(object obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (obj.ToString() == "JgReadIDCard2")//首页刷身份证
                    obj = obj.ToString().Substring(0, obj.ToString().Length - 1);
                else
                    this.Dispose();
                ClearData();

                //if(obj is object)
                if (OwnerViewModel != null)
                {
                    //关闭消息提示
                    OwnerViewModel.IsMessage = false;
                    //OwnerViewModel.Dispose();
                    OwnerViewModel.ContentPageSetting(obj.ToString());
                }
            });
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        public virtual void ExitProgram()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Owner?.Close();
            });
        }
        /// <summary>
        /// 界面超时
        /// </summary>
        /// <param name="second">秒</param>
        public virtual void OpenTimeOut(int second = 60)
        {
            //默认将计时器归位 by wei.chen
            IsStop = false;

            //注册超时回调
            this.TimeOutCallBack = this.TimeOutCallBackExcuted;
            //Log.Instance.WriteInfo(this.ContentPage?.Name+":"+second.ToString());
            //倒计时
            this.thCountdown = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                try
                {
                    while (!IsStop)
                    {
                        Log.Instance.WriteInfo(second.ToString());
                        this.StrCountdownValue = second.ToString();
                        System.Threading.Thread.Sleep(1000);
                        second--;
                        if (second <= 0)
                        {
                            this.StrCountdownValue = string.Empty;
                            this.TimeOutCallBack?.Invoke();
                            if (this.thCountdown?.ThreadState != System.Threading.ThreadState.Stopped)
                            {
                                this.thCountdown?.Abort();
                            }
                            break;
                        }
                    }
                }
                catch (ThreadAbortException ex)
                {
                    this.TimeOutCallBack -= this.TimeOutCallBackExcuted;
                    Log.Instance.WriteInfo("倒计时线程异常：" + ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteInfo("倒计时异常：" + ex.Message);
                }

            }));
            this.thCountdown.IsBackground = true;
            this.thCountdown.Start();
        }

        /// <summary>
        /// 界面超时
        /// </summary>
        /// <param name="second">秒</param>
        public virtual void OpenTimeOut2(double second = 60)
        {
            OwnerViewModel?.IsShowHiddenLoadingWait(TipMsgResource.IDCardQuueryTipMsg);
            //注册超时回调
            this.TimeOutCallBack = this.TimeOutCallBackExcuted;

            //倒计时
            this.thCountdown = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (true)
                {
                    if (!IsStop)
                    {
                        this.StrCountdownValue = second.ToString();
                        System.Threading.Thread.Sleep(10);
                        second--;
                        if (second <= 0)
                        {
                            this.StrCountdownValue = string.Empty;
                            this.TimeOutCallBack?.Invoke();
                            if (this.thCountdown.ThreadState != System.Threading.ThreadState.Stopped)
                            {
                                this.thCountdown.Abort();
                            }
                            break;
                        }
                    }
                }
            }));
            this.thCountdown.IsBackground = true;
            this.thCountdown.Start();
            OwnerViewModel?.IsShowHiddenLoadingWait();
        }

        /// <summary>
        /// 停止界面超时
        /// </summary>
        /// <param name="nflag"></param>
        public void StopTimeOut(bool nflag = true)
        {
            if (thCountdown != null && nflag)
            {
                TimeOutCallBack = null;
                IsStop = true;
                //thCountdown.Abort();
                //thCountdown = null;
            }
        }


        /// <summary>
        /// 执行超时
        /// </summary>
        public virtual void TimeOutCallBackExcuted()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.Dispose();
                //清空页面缓存对象
                ServiceRegistry.Instance.Get<ElementManager>().Remove<BookingModel>();
                ClearData();
                Log.Instance.WriteInfo(TipMsgResource.TimeOutTipMsg);
                OwnerViewModel?.MessageTips(TipMsgResource.TimeOutTipMsg, () =>
                {
                    OwnerViewModel?.ContentPageSetting("MainPage");
                });
            });
            //this.Owner?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    this.Dispose();
            //    OwnerViewModel?.MessageTips("操作超时，现在返回首页请重新操作！", () =>
            //    {
            //        this.DoExit?.Execute(null);
            //    });
            //    //this.DoBack?.Execute(this.ContentPage);
            //}));
        }

        /// <summary>
        /// 关闭超时进程
        /// </summary>
        public virtual void CancelTimeOut()
        {
            //timer.Dispose();
            thCountdown?.Abort();
        }



        /// <summary>
        /// 释放资源
        /// </summary>

        protected override void OnDispose()
        {
            try
            {
                if (cts != null)
                    cts.Cancel();

                Log.Instance.WriteInfo("******OnDispose停止计时器****** ");
                //this.TimeOutCallBack -= this.TimeOutCallBackExcuted;

                if (timer.IsNotEmpty())
                    timer?.Dispose();
                try
                {
                    if (!IsStop && this.thCountdown.IsNotEmpty() && this.thCountdown?.ThreadState != System.Threading.ThreadState.Stopped)
                        this.thCountdown?.Abort();
                }
                catch (ThreadAbortException ex)
                {
                    Log.Instance.WriteInfo("thCountdown发生异常：" + ex.Message);
                }
                if (this.thCommon.IsNotEmpty() && this.thCommon?.ThreadState != System.Threading.ThreadState.Stopped)
                    thCommon?.Abort();

                //关闭页面清空 消息提示框关闭不清空
                //if (!closeMsg)
                //{
                //this.TipMessage = string.Empty;
                //    this.NextPageShow = Visibility.Collapsed;
                //    this.PreviousPageShow = Visibility.Collapsed;
                //}
                GC.Collect();
                GC.SuppressFinalize(this);
                base.OnDispose();
            }

            catch (Exception ex)
            {
                Log.Instance.WriteInfo("基类的释放发生异常：" + ex.Message);
                Log.Instance.WriteError("基类的释放发生异常：" + ex.Message);
            }

        }

        #endregion
    }
}
