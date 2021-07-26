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
    public class AppMessageToNextViewModels : ObservableObject
    {
        private readonly MainWindowViewModels _mainViewModles = MainWindowViewModels.Instance;

        public static AppMessageToNextViewModels Instance
        {
            get
            {
                return SingletonProvider<AppMessageToNextViewModels>.Instance;
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

        private string _ButtonExitContent = "返回";

        /// <summary>
        /// 按钮是否显示
        /// </summary>
        private Visibility _ButtonVisibility = Visibility.Visible;

        private Visibility _ButtonExitVisibility = Visibility.Visible;


        /// <summary>
        /// 不返回首页时要跳转的页面
        /// </summary>
        private string _FrameTraget = "";


        private string _FrameExitTraget = "";

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

        
        public string ButtonExitContent
        {
            get { return _ButtonExitContent; }
            set { _ButtonExitContent = value; base.RaisePropertyChanged("ButtonExitContent"); }
        }
        /// <summary>
        /// 按钮是否显示
        /// </summary>
        public Visibility ButtonVisibility
        {
            get { return _ButtonVisibility; }
            set { _ButtonVisibility = value; base.RaisePropertyChanged("ButtonVisibility"); }
        }

        
        public Visibility ButtonExitVisibility
        {
            get { return _ButtonExitVisibility; }
            set { _ButtonExitVisibility = value; base.RaisePropertyChanged("ButtonExitVisibility"); }
        }

        /// <summary>
        /// 不返回首页时要跳转的页面
        /// </summary>
        public string FrameTraget
        {
            get { return _FrameTraget; }
            set { _FrameTraget = value; base.RaisePropertyChanged("FrameTraget"); }
        }

        
        public string FrameExitTraget
        {
            get { return _FrameExitTraget; }
            set { _FrameExitTraget = value; base.RaisePropertyChanged("FrameExitTraget"); }
        }

        public override void DoInitFunction(object obj)
        {
            if (!this.IsClose) return;


            if (this.FrameTraget.IsEmpty())
                _mainViewModles.OpenTimeOut(10);
            else
            {
                _mainViewModles.OpenTimeOut();
                _mainViewModles.TimeOutEventAction += Instance_TimeOutEventAction;
            }

            //if (this._FrameExitTraget.IsEmpty())
            //    _mainViewModles.OpenTimeOut(10);
            //else
            //{
            //    _mainViewModles.OpenTimeOut();
            //    _mainViewModles.TimeOutEventAction += Instance_TimeOutEventAction;
            //}
        }

        private void Instance_TimeOutEventAction()
        {
            if (this.FrameTraget.IsNotEmpty())
                _mainViewModles.ContentPageSetting(this.FrameTraget);

            //if (this._FrameExitTraget.IsNotEmpty())
            //    _mainViewModles.ContentPageSetting(this.FrameTraget);
        }

        public override void DoNextFunction(object obj)
        {
            if (this.IsClose)
                _mainViewModles.StopTimeOut();
            if (this.FrameTraget.IsEmpty())
                _mainViewModles.DoExitFunction(null);
            else
                _mainViewModles.ContentPageSetting(this.FrameTraget);




        }


    }
}
