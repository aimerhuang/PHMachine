using Freedom.Controls.Foundation;
using Freedom.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Freedom.ZHPHMachine.Command;

namespace Freedom.ZHPHMachine.ViewModels
{
    public class NumberKeyboardViewModels : ObservableObject
    {
        public NumberKeyboardViewModels(KeyboardType type, string title = "")
        {
            _type = type;
            switch ((int)type)
            {
                case 0:
                    Title = "请输入身份证号码";//不带正则验证
                    break;
                case 1:
                    Title = "请输入回执单号";
                    break;
                case 2:
                    Title = "请输入手机号码";
                    break;
                case 3:
                    Title = "请输入身份证号码";//带正则验证
                    break;
                case 4:
                    Title = "请输入邮政编码";
                    break;
            }
            if (!string.IsNullOrEmpty(title))
            {
                Title = title;
            }
        }
        #region 属性
        private KeyboardType _type;
        private string title;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged("Title"); }
        }

        private string tipsMsg;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipsMsg
        {
            get { return tipsMsg; }
            set { tipsMsg = value; RaisePropertyChanged("TipsMsg"); }
        }

        private string content;

        /// <summary>
        /// 输入内容
        /// </summary>
        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }
        public ICommand OKCommand
        {
            get
            {
                return new RelayCommand<System.Windows.Window>((win) =>
                {
                    if (!DateVerification(_type, content, out string msg))
                    {
                        TipsMsg = msg;
                        return;
                    }
                    else
                    {
                        win.DialogResult = true;
                    }
                });
            }
        }

        #endregion

        #region 校验
        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="content">验证数据</param>
        /// <param name="msg">返回提示信息</param>
        /// <returns></returns>
        private bool DateVerification(KeyboardType type, string content, out string msg)
        {
            bool result = true;
            msg = string.Empty;
            switch ((int)type)
            {
                case 0:
                    if (string.IsNullOrWhiteSpace(content))//|| !ValidationHelper.IsIdNumber(content)
                    {
                        msg = "请输入正确的身份证号码";
                        result = false;
                    }
                    break;
                case 1:
                    if (string.IsNullOrWhiteSpace(content) || content.Length > 15 || content.Length < 8)
                    {
                        msg = "请输入正确的回执单号";
                        result = false;
                    }
                    break;
                case 2:
                    if (string.IsNullOrWhiteSpace(content) || !Freedom.Common.Validate.IsValidMobile(content))
                    {
                        msg = "请输入正确的电话号码";
                        result = false;
                    }
                    break;
                case 3:
                    if (string.IsNullOrWhiteSpace(content) || !ValidationHelper.IsIdNumber(content))
                    {
                        msg = "请输入正确的身份证号码";
                        result = false;
                    }
                    break;
                case 4:
                    if (string.IsNullOrWhiteSpace(content)|| !ValidationHelper.IsYzbm(content))
                    {
                        msg = "请输入正确的邮政编码";
                        result = false;
                    }
                    break;
            }
            return result;
        }

        #endregion


    }

    public enum KeyboardType
    {
        /// <summary>
        /// 不带正则验证的身份证号码
        /// </summary>
        ID = 0,
        /// <summary>
        /// 照片回执
        /// </summary>
        Receipt = 1,
        /// <summary>
        /// 电话号码
        /// </summary>
        TelePhone = 2,
        /// <summary>
        /// 带正则验证的身份证号码
        /// </summary>
        IDCard = 3,
        /// <summary>
        /// 邮政编码
        /// </summary>
        EMSCode = 4
    }
}
