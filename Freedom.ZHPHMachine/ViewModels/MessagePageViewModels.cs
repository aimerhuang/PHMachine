using Freedom.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class MessagePageViewModels : ViewModelBase
    {
        #region 构造函数
        public MessagePageViewModels(Page page)
        {
            this.ContentPage = page;
        }

        #endregion
        #region 属性
        private string title = "消息提示";
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title"); }
        }

        private string content;
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }

        private string btnContent;

        /// <summary>
        /// 按钮内容
        /// </summary>
        public string BtnContent
        {
            get { return btnContent; }
            set { btnContent = value; RaisePropertyChanged("BtnContent"); }
        }

        public override void DoInitFunction(object obj)
        {
            //设置背景图   
            var bgpath = Path.Combine(FileHelper.GetLocalPath(), @"ApplicationData/Skin/HsZhPjh/bg.jpg");
            OwnerViewModel.WinBrshImage = new BitmapImage(new Uri(bgpath, UriKind.RelativeOrAbsolute));
            //启用计时器
            this.OpenTimeOut(5);
        }
        #endregion


    }
}
