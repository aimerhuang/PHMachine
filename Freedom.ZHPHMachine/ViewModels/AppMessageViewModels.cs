using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.ZHPHMachine.ViewModels
{
    using Freedom.Controls;
    using Freedom.Controls.Foundation;
    using Freedom.Common;
    using Freedom.Models;
    using System.Windows;
    using Freedom.Config;
    public partial class AppMessageViewModels : ObservableObject
    {
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
        private string _ButtonContent = "取证";
        /// <summary>
        /// 按钮是否显示
        /// </summary>
        private Visibility _ButtonVisibility = Visibility.Visible;

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


        public AppMessageViewModels()
        {

        }

        public override void DoInitFunction(object obj)
        {
            if (this.IsClose)
            {
                MainWindowViewModels.Instance.OpenTimeOut(10);
            }
        }

        public override void DoNextFunction(object obj)
        {
            if (this.IsClose)
                MainWindowViewModels.Instance.StopTimeOut();
            MainWindowViewModels.Instance.ReturnHome();
        }
    }
}
