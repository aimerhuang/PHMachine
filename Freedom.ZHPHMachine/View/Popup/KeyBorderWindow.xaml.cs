using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Freedom.Common;
using Freedom.Config;
using Freedom.Controls;
using Freedom.WinAPI;
using Freedom.ZHPHMachine.Command;
using Freedom.ZHPHMachine.ViewModels.Popup;

namespace Freedom.ZHPHMachine.View.Popup
{
    /// <summary>
    /// KeyBorderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class KeyBorderWindow : Window
    {
        public event CommandDelegate.delegateStrFun ResultStrEvent;
        public string topTitle = string.Empty;
        public string strTextName = string.Empty;
        public string oldTextInfo = string.Empty;
        public EnumTypeStopTypewriting enumTypeStopTypewriting;
        private string selectCity = "";
        public List<string> _City = new List<string>() { "选择城市", "广州市", "深圳市", "珠海市", "汕头市", "韶关市", "佛山市", "江门市", "湛江市", "茂名市", "肇庆市", "惠州市", "梅州市", "汕尾市", "河源市", "阳江市", "清远市", "东莞市 ", "中山市", "潮州市", "揭阳市", "云浮市" };

        #region 属性
        public string KeyBorderContent
        {
            get
            {
                return this.txtInputValue.Text.Trim();
            }
        }
        //public readonly static DependencyProperty dependencyProperty
        #endregion
        public KeyBorderWindow()
        {
            InitializeComponent();
            this.DataContext = new KeyBorderWindowViewModels();
            //ssViewModel.OpenTimeOut(QJTConfig.QJTModel.TOutForWrittingSeconds);
            int x = SystemParameters.PrimaryScreenWidth.ToInt();
            int y = SystemParameters.PrimaryScreenHeight.ToInt();
            if (this.Width < x && y < this.Height)
            {
                this.Width = this.Width / 2;
                this.Height = this.Height / 2;
            }
            this.Loaded += KeyBorderWindow_Loaded;
            this.Closed += KeyBorderWindow_Closed;
        }

        public KeyBorderWindowViewModels ViewModel => this.DataContext as KeyBorderWindowViewModels;
        private void KeyBorderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HandInput.btnClick = new HandInputControl.BtnClickOver(BtnTextClick);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError(ex.Message);
            }
        }

        private void KeyBorderWindow_Closed(object sender, EventArgs e)
        {
            HandInput.Dispose();
        }

        public void InitKeyBorderWindow()
        {
            topTitleInfo.Text = topTitle;
            //txtName.Text = topTitle.Replace("请输入", "") + "：";
            txtName.Text = strTextName;
            txtInputValue.Text = oldTextInfo;
            SX.IsChecked = true;
            txtInputValue.Focus();
            txtInputValue.Select(txtInputValue.Text.Length, 0);
            txtInputValue.SelectionStart = txtInputValue.Text.Length;
            switch (this.enumTypeStopTypewriting)
            {
                case EnumTypeStopTypewriting.None:
                    break;
                case EnumTypeStopTypewriting.SX:
                    this.SX.Visibility = Visibility.Collapsed;
                    break;
                case EnumTypeStopTypewriting.EN:
                    this.EN.Visibility = Visibility.Collapsed;
                    break;
                case EnumTypeStopTypewriting.WB:
                    this.WB.Visibility = Visibility.Collapsed;
                    break;
                case EnumTypeStopTypewriting.ZW:
                    this.ZW.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }

            if (txtName.Text.Contains("姓名"))
            {
                txtInputValue.MaxLength = 15;
            }
        }

        private void BtnTextClick()
        {
            txtInputValue.Focus();
            txtInputValue.Text = txtInputValue.Text.Insert(txtInputValue.SelectionStart, this.HandInput.CurrentStr);
            txtInputValue.SelectionStart = txtInputValue.Text.Length;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button keybtn = sender as System.Windows.Controls.Button;
            #region//First Row
            if (keybtn.Name == "CmdTlide")
            {
                addNumkeyINput(0xc0);
            }
            else if (keybtn.Name == "cmd1" || keybtn.Name == "Scmd1")
            {
                addNumkeyINput(0x31);
            }
            else if (keybtn.Name == "cmd2" || keybtn.Name == "Scmd2")
            {
                addNumkeyINput(0x32);
            }
            else if (keybtn.Name == "cmd3" || keybtn.Name == "Scmd3")
            {
                addNumkeyINput(0x33);
            }
            else if (keybtn.Name == "cmd4" || keybtn.Name == "Scmd4")
            {
                addNumkeyINput(0x34);
            }
            else if (keybtn.Name == "cmd5" || keybtn.Name == "Scmd5")
            {
                addNumkeyINput(0x35);
            }
            else if (keybtn.Name == "cmd6" || keybtn.Name == "Scmd6")
            {
                addNumkeyINput(0x36);

            }
            else if (keybtn.Name == "cmd7" || keybtn.Name == "Scmd7")
            {
                addNumkeyINput(0x37);
            }
            else if (keybtn.Name == "cmd8" || keybtn.Name == "Scmd8")
            {
                addNumkeyINput(0x38);
            }
            else if (keybtn.Name == "cmd9" || keybtn.Name == "Scmd9")
            {
                addNumkeyINput(0x39);
            }
            else if (keybtn.Name == "cmd0" || keybtn.Name == "Scmd0")
            {
                addNumkeyINput(0x30);

            }
            else if (keybtn.Name == "cmdminus")//-_
            {
                addNumkeyINput(0xbd);
            }
            else if (keybtn.Name == "cmdplus")//+=
            {
                addNumkeyINput(0xbb);
            }
            else if (keybtn.Name == "cmdBackspace" || keybtn.Name == "btnBack")//backspace
            {
                AddKeyBoardINput(0x08);
            }
            #endregion
            #region//Second Row
            else if (keybtn.Name == "CmdTab")
            {
                AddKeyBoardINput(0x09);
            }
            else if (keybtn.Name == "CmdQ")
            {
                AddKeyBoardINput(0x51);
            }
            else if (keybtn.Name == "Cmdw")
            {
                AddKeyBoardINput(0x57);

            }
            else if (keybtn.Name == "CmdE")
            {
                AddKeyBoardINput(0X45);

            }
            else if (keybtn.Name == "CmdR")
            {
                AddKeyBoardINput(0X52);

            }
            else if (keybtn.Name == "CmdT")
            {
                AddKeyBoardINput(0X54);

            }
            else if (keybtn.Name == "CmdY")
            {
                AddKeyBoardINput(0X59);

            }
            else if (keybtn.Name == "CmdU")
            {
                AddKeyBoardINput(0X55);

            }
            else if (keybtn.Name == "CmdI")
            {
                AddKeyBoardINput(0X49);

            }
            else if (keybtn.Name == "CmdO")
            {
                AddKeyBoardINput(0X4F);
            }
            else if (keybtn.Name == "CmdP")
            {
                AddKeyBoardINput(0X50);
            }
            else if (keybtn.Name == "CmdOpenCrulyBrace")
            {
                addNumkeyINput(0xdb);
            }
            else if (keybtn.Name == "CmdEndCrultBrace")
            {
                addNumkeyINput(0xdd);
            }
            else if (keybtn.Name == "CmdOR")
            {
                addNumkeyINput(0xdc);
            }
            #endregion
            #region///Third ROw

            else if (keybtn.Name == "CmdCapsLock")
            {
                AddKeyBoardINput(0x14);
                if (checkImage.Visibility != Visibility.Visible)
                {
                    checkImage.Visibility = Visibility.Visible;
                }
                else
                {
                    checkImage.Visibility = Visibility.Hidden;
                }
            }
            else if (keybtn.Name == "CmdA")
            {
                AddKeyBoardINput(0x41);
            }
            else if (keybtn.Name == "CmdS")
            {
                AddKeyBoardINput(0x53);
            }
            else if (keybtn.Name == "CmdD")
            {
                AddKeyBoardINput(0x44);
            }
            else if (keybtn.Name == "CmdF")
            {
                AddKeyBoardINput(0x46);
            }
            else if (keybtn.Name == "CmdG")
            {
                AddKeyBoardINput(0x47);
            }
            else if (keybtn.Name == "CmdH")
            {
                AddKeyBoardINput(0x48);
            }
            else if (keybtn.Name == "CmdJ")
            {
                AddKeyBoardINput(0x4A);
            }
            else if (keybtn.Name == "CmdK")
            {
                AddKeyBoardINput(0X4B);
            }
            else if (keybtn.Name == "CmdL")
            {
                AddKeyBoardINput(0X4C);

            }
            else if (keybtn.Name == "CmdColon")//;:
            {
                addNumkeyINput(0xba);
            }
            else if (keybtn.Name == "CmdDoubleInvertedComma")//'"
            {
                addNumkeyINput(0xde);
            }
            else if (keybtn.Name == "CmdEnter")
            {
                AddKeyBoardINput(0x0d);
            }
            #endregion
            #region//Fourth Row
            else if (keybtn.Name == "CmdShift" || keybtn.Name == "CmdlShift")
            {
                if (CtrlFlag)
                {
                    CtrlFlag = false;
                    ShiftFlag = false;
                    changeInput();
                }
                else
                {
                    ShiftFlag = true;
                }
            }
            else if (keybtn.Name == "CmdZ")
            {

                AddKeyBoardINput(0X5A);

            }
            else if (keybtn.Name == "CmdX")
            {
                AddKeyBoardINput(0X58);

            }
            else if (keybtn.Name == "CmdC")
            {
                AddKeyBoardINput(0X43);

            }
            else if (keybtn.Name == "CmdV")
            {
                AddKeyBoardINput(0X56);

            }
            else if (keybtn.Name == "CmdB")
            {
                AddKeyBoardINput(0X42);

            }
            else if (keybtn.Name == "CmdN")
            {
                AddKeyBoardINput(0x4E);

            }
            else if (keybtn.Name == "CmdM")
            {
                AddKeyBoardINput(0x4D);
            }
            else if (keybtn.Name == "CmdLessThan")//<,
            {
                addNumkeyINput(0xbc);
            }
            else if (keybtn.Name == "CmdGreaterThan")//>.
            {
                addNumkeyINput(0xbe);
            }
            else if (keybtn.Name == "CmdQuestion")//?/
            {
                addNumkeyINput(0xbf);
            }

            else if (keybtn.Name == "CmdSpaceBar")
            {
                AddKeyBoardINput(0x20);
            }
            #endregion
            #region//Last row
            else if (keybtn.Name == "CmdCtrl" || keybtn.Name == "CmdlCtrl")//ctrl
            {
                if (ShiftFlag)
                {
                    ShiftFlag = false;
                    CtrlFlag = false;
                }
                else
                {
                    CtrlFlag = true;
                }
            }
            else if (keybtn.Name == "CmdpageUp")
            {
                AddKeyBoardINput(0x21);
            }
            else if (keybtn.Name == "CmdpageDown")
            {
                AddKeyBoardINput(0x22);
            }
            else if (keybtn.Name == "CmdClose")//关闭键盘
            {
                KeyBorderPanel.Visibility = Visibility.Hidden;
                //this.Close();
            }
            #endregion
        }

        private void changeInput()
        {
            Win32API.keybd_event(0x11, 0, 0, 0);
            Win32API.keybd_event(0x10, 0, 0, 0);
            Win32API.keybd_event(0x11, 0, 0x02, 0);
            Win32API.keybd_event(0x10, 0, 0x02, 0);
        }

        private static void addNumkeyINput(byte input)
        {
            if (CtrlFlag)
            {
                CtrlFlag = false;
                ShiftFlag = false;
                Win32API.keybd_event(input, 0, 0, 0);
                Win32API.keybd_event(input, 0, 0x02, 0);
            }
            else
            {
                if (!ShiftFlag)
                {
                    Win32API.keybd_event(input, 0, 0, 0);
                    Win32API.keybd_event(input, 0, 0x02, 0);
                }
                else
                {
                    Win32API.keybd_event(0x10, 0, 0, 0);//shift
                    Win32API.keybd_event(input, 0, 0, 0);
                    Win32API.keybd_event(input, 0, 0x02, 0);
                    Win32API.keybd_event(0x10, 0, 0x02, 0);
                    ShiftFlag = false;
                }
            }
        }
        private static void AddKeyBoardINput(byte input)
        {

            if (CtrlFlag)
            {
                CtrlFlag = false;
                ShiftFlag = false;
                Win32API.keybd_event(input, 0, 0, 0);
                Win32API.keybd_event(input, 0, 0x02, 0);
            }
            else
            {
                if (ShiftFlag)
                {
                    Win32API.keybd_event(input, 0, 0, 0);
                    Win32API.keybd_event(input, 0, 0x02, 0);
                    ShiftFlag = false;
                }
                else
                {
                    Win32API.keybd_event(input, 0, 0, 0);
                    Win32API.keybd_event(input, 0, 0x02, 0);
                }
            }

        }

        public bool CapsLockStatus
        {
            get
            {
                byte[] bs = new byte[256];
                Win32API.GetKeyboardState(bs);
                return (bs[0x14] == 1);
            }
        }

        #region Property & Variable & Constructor
        private static double _WidthTouchKeyboard = 605;

        public static double WidthTouchKeyboard
        {
            get { return _WidthTouchKeyboard; }
            set { _WidthTouchKeyboard = value; }

        }
        private static bool _ShiftFlag;

        protected static bool ShiftFlag
        {
            get { return _ShiftFlag; }
            set { _ShiftFlag = value; }
        }
        private static bool _CtrlFlag;
        protected static bool CtrlFlag
        {
            get { return _CtrlFlag; }
            set { _CtrlFlag = value; }
        }
        #endregion

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            txtInputValue.Focus();
            if (e.Source is RadioButton)
            {
                RadioButton rd = sender as RadioButton;
                switch (rd.Name)
                {
                    case "SX":
                        HandlerPanel.Visibility = System.Windows.Visibility.Visible;
                        KeyBorderPanel.Visibility = System.Windows.Visibility.Hidden;
                        setInput("美式键盘");
                        if (CapsLockStatus)
                        {
                            AddKeyBoardINput(0x14);
                        }
                        break;
                    case "EN":
                        HandlerPanel.Visibility = System.Windows.Visibility.Hidden;
                        KeyBorderPanel.Visibility = System.Windows.Visibility.Visible;
                        setInput("美式键盘");
                        if (!CapsLockStatus)
                        {
                            AddKeyBoardINput(0x14);
                        }
                        checkImage.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "ZW":
                        HandlerPanel.Visibility = System.Windows.Visibility.Hidden;
                        KeyBorderPanel.Visibility = System.Windows.Visibility.Visible;
                        setInput("搜狗拼音");
                        if (CapsLockStatus)
                        {
                            AddKeyBoardINput(0x14);
                        }
                        checkImage.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case "WB":
                        HandlerPanel.Visibility = System.Windows.Visibility.Hidden;
                        KeyBorderPanel.Visibility = System.Windows.Visibility.Visible;
                        setInput("万能五笔输入法");
                        if (CapsLockStatus)
                        {
                            Win32API.AddKeyBoardINput(0x14);
                        }
                        checkImage.Visibility = System.Windows.Visibility.Hidden;
                        break;
                }
                txtInputValue.SelectionStart = txtInputValue.Text.Length;
            }
        }

        private void setInput(string name)
        {
            int k = -1;
            for (int i = 0; i < System.Windows.Forms.InputLanguage.InstalledInputLanguages.Count; i++)
            {
                Log.Instance.WriteWarn("输入法名称：" + System.Windows.Forms.InputLanguage.InstalledInputLanguages[i].LayoutName.Trim());
                if (System.Windows.Forms.InputLanguage.InstalledInputLanguages[i].LayoutName.Trim().Contains(name))
                {
                    k = i;
                    System.Windows.Forms.InputLanguage.CurrentInputLanguage = System.Windows.Forms.InputLanguage.InstalledInputLanguages[i];
                    break;
                }
            }

            if (k == -1)
            {
                SX.IsChecked = true;
                MessageBox.Show("错误！系统未安装" + name);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (txtInputValue.Text != "")
            {
                txtInputValue.Text = string.Empty;
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            //topTitle = "";
            //oldTextInfo = "";
            //ResultStrEvent = null;
            //HandInput.Dispose();
            //this.Hide();
            this.DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if(topTitle.Contains("名"))
            //if (topTitle.Equals("紧急联系人姓名") || topTitle.Equals("曾用名") || topTitle.Equals("姓名") || topTitle.Equals("港澳亲属姓名"))//
            {
                //姓名正则验证
                var validate = ValidationHelper.IsName(txtInputValue.Text.Trim());
                if (!validate)
                {
                    Msg.Text = "请输入正确的中文姓名";
                    return;
                }
            }
            if (topTitle.Contains("地址"))
                //邮寄地址//
            {
                //邮寄地址正则验证
                var validate = ValidationHelper.IsPlace(txtInputValue.Text.Trim());
                if (!validate)
                {
                    Msg.Text = "请输入正确的中文地址";
                    return;
                }
            }

            
            //topTitle = "";
            //oldTextInfo = "";
            if (ResultStrEvent != null)
            {
                ResultStrEvent(this.txtInputValue.Text.Trim());
            }
            //selectCity = "";
            //ResultStrEvent = null;
            HandInput.Dispose();
            //this.Hide();
            this.DialogResult = true;
        }
        /// <summary>
        /// 禁用输入法
        /// </summary>
        public enum EnumTypeStopTypewriting
        {
            /// <summary>
            /// 无
            /// </summary>
            None = 0,
            /// <summary>
            /// 手写
            /// </summary>
            SX = 1,
            /// <summary>
            /// 英文
            /// </summary>
            EN = 2,
            /// <summary>
            /// 中文
            /// </summary>
            ZW = 3,
            /// <summary>
            /// 五笔
            /// </summary>
            WB = 4,
        }
    }
}
