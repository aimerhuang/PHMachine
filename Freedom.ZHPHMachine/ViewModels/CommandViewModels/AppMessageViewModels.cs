using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Freedom.Common;
using Freedom.Controls.Foundation;
using Freedom.Models;

namespace Freedom.ZHPHMachine.ViewModels.CommandViewModels
{
    public class AppMessageViewModels : ObservableObject
    {
        private readonly MainWindowViewModels OwnerViewModel = MainWindowViewModels.Instance;
        public static AppMessageViewModels Instance
        {
            get
            {
                return SingletonProvider<AppMessageViewModels>.Instance;
            }
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        private string _AppMessageInfo;
        /// <summary>
        /// 是否自动关闭
        /// </summary>
        private bool _IsClose = false;
        /// <summary>
        /// 按钮文本
        /// </summary>
        private string _ButtonContent = "确定";
        /// <summary>
        /// 按钮是否显示
        /// </summary>
        private Visibility _ButtonVisibility = Visibility.Visible;
        /// <summary>
        /// 不返回首页时要跳转的页面
        /// </summary>
        private string _FrameTraget = "";


        /// <summary>
        /// 是否自动关闭
        /// </summary>
        public bool IsClose
        {
            get { return _IsClose; }
            set { _IsClose = value; }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string AppMessageInfo
        {
            get { return _AppMessageInfo; }
            set { _AppMessageInfo = value; base.RaisePropertyChanged("AppMessageInfo"); }
        }

        /// <summary>
        /// 按钮文本
        /// </summary>
        public string ButtonContent
        {
            get { return _ButtonContent; }
            set { _ButtonContent = value; base.RaisePropertyChanged("ButtonContent"); }
        }

        /// <summary>
        /// 按钮是否显示
        /// </summary>
        public Visibility ButtonVisibility
        {
            get { return _ButtonVisibility; }
            set { _ButtonVisibility = value; base.RaisePropertyChanged("ButtonVisibility"); }
        }

        /// <summary>
        /// 不返回首页时要跳转的页面
        /// </summary>
        public string FrameTraget
        {
            get { return _FrameTraget; }
            set { _FrameTraget = value; base.RaisePropertyChanged("FrameTraget"); }
        }

        public AppMessageViewModels()
        {

        }

        public override void DoInitFunction(object obj)
        {
            if (this.IsClose)
            {
                if (this.FrameTraget.IsEmpty())
                    OwnerViewModel.OpenTimeOut(10);
                else
                {
                    OwnerViewModel.OpenTimeOut();
                    OwnerViewModel.TimeOutEventAction += Instance_TimeOutEventAction;
                }
            }
        }

        private void Instance_TimeOutEventAction()
        {
            if (this.FrameTraget.IsNotEmpty())
                OwnerViewModel.DoNextFunction(this.FrameTraget);
        }

        public override void DoNextFunction(object obj)
        {
            if (this.IsClose)
                OwnerViewModel.StopTimeOut();
            if (this.FrameTraget.IsEmpty())
                OwnerViewModel.DoExitFunction(null);
            else
                OwnerViewModel.DoNextFunction(this.FrameTraget);

        }

        protected override void OnDispose()
        {
            CommonHelper.OnDispose();
            base.OnDispose();
        }
    }
}
