using Freedom.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Freedom.Common;
using Freedom.Common.HsZhPjh.Enums;
using Freedom.Models.TJJsonModels;
using Freedom.ZHPHMachine.Command;
using Freedom.ZHPHMachine.ViewModels;
using MachineCommandService;

namespace Freedom.ZHPHMachine
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainWindowViewModels.Instance;
            ViewModel.OwnerViewModel = ViewModel;

            //this.Topmost = true;
            //this.WindowState = WindowState.Maximized;
            if (QJTConfig.QJTModel.MainTopmost)
            {
                //this.Topmost = true;
                //this.WindowState = WindowState.Maximized;
            }
            this.Closed += MainWindow_Closed;
            this.KeyDown += WinMain_KeyDown;

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ViewModel.OwnerViewModel.Showtask();
            bool blnResult = ZHPHMachineWSHelper.ZHPHInstance.U_DeviceStatus(QJTConfig.QJTModel.QJTDevInfo.DEV_ID, EnumTypeSTATUS.OFFLINE);
            if (TjConfig.TjModel.IsConnectionTj && blnResult)
            {
                UploadMachineCondition();
                UploadRunningCondition();
            }
            Process processes = Process.GetCurrentProcess();
            Log.Instance.WriteInfo("===================结束【主程序】【" + processes.ProcessName + "】====================");
            //Log.Instance.WriteInfo("================== 结束【主窗口】==================");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            System.Diagnostics.Process.GetCurrentProcess().Dispose();

        }

        private void WinMain_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.F4) && (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)) //ALT+F4
            {

                Log.Instance.WriteInfo("================== 按ALT+F4结束【主窗口】==================");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                System.Diagnostics.Process.GetCurrentProcess().Dispose();
                Application.Current.Shutdown();
            }
        }

        public MainWindowViewModels ViewModel => this.DataContext as MainWindowViewModels;
        private void Frame_ContentRendered(object sender, EventArgs e)
        {
            //获取Frame当前Page
            var currentPage = (Page)((Frame)sender).Content;
            //设置Frame内容页的背景颜色
            var bgColor = Frame.Background;
            Brush contentBg = currentPage is View.MainPage ? null : this.FindResource("ContentBackground") as SolidColorBrush;
            if (bgColor != contentBg)
            {
                Frame.Background = contentBg;
            }
            //设置Frame当前页面
            ViewModel.ContentPage = currentPage;

            ViewModel.ContentViewModel = currentPage?.DataContext as ViewModelBase;


        }
        int count = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (count == 0)
            {
                count++;
            }
            if (count == 2)
            {
                ViewModel.IsShowHiddenLoadingWait();
                ViewModel.ContentViewModel?.Dispose();
                //ViewModel.TipMessage = string.Empty;
                ViewModel.NextPageShow = Visibility.Collapsed;
                ViewModel.PreviousPageShow = Visibility.Collapsed;
                ViewModel.IsMessage = false;
                ViewModel.ContentPageSetting("PasswordKeyboardPage");
                count = 0;
            }

        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (!isBack)
                {
                    e.Cancel = true;
                }
                else
                {
                    isBack = false;
                }
            }
        }

        /// <summary>
        /// 定义的计时器
        /// </summary>
        private static DispatcherTimer myClickWaitTimer =
            new DispatcherTimer(
                new TimeSpan(0, 0, 0, 1),
                DispatcherPriority.Background,
                mouseWaitTimer_Tick,
                Dispatcher.CurrentDispatcher);

        private static void mouseWaitTimer_Tick(object sender, EventArgs e)
        {
            myClickWaitTimer.Stop();
            // Handle Single Click Actions
            //Trace.WriteLine("Single Click");
        }



        bool isBack = false;
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            isBack = true;
        }


        DateTime lastClick = DateTime.Now;
        object obj = new object();
        int i = 0;
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            //myClickWaitTimer.Start();
            myClickWaitTimer.Stop();

            e.Handled = true;

            this.IsEnabled = false;
            DispatcherHelper.DoEvents();
            var t = (DateTime.Now - lastClick).TotalMilliseconds;
            i++;
            lastClick = DateTime.Now;
            System.Diagnostics.Debug.Print(t + "," + i + ";" + DateTime.Now);
            Thread.Sleep(100);
            this.IsEnabled = true;
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (count == 1)
            {
                count++;
            }
        }

        public static class DispatcherHelper
        {
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
                try { Dispatcher.PushFrame(frame); }
                catch (InvalidOperationException) { }
            }
            private static object ExitFrames(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }

        private void BtnNext_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            myClickWaitTimer.Stop();

            e.Handled = true;

            this.IsEnabled = false;
            DispatcherHelper.DoEvents();
            var t = (DateTime.Now - lastClick).TotalMilliseconds;
            i++;
            lastClick = DateTime.Now;
            System.Diagnostics.Debug.Print(t + "," + i + ";" + DateTime.Now);
            Thread.Sleep(500);
            this.IsEnabled = true;

        }

        /// <summary>
        /// 太极上传设备状态
        /// </summary>
        /// <returns></returns>
        public static bool UploadMachineCondition()
        {
            var nowTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate()).ToString("yyyyMMddHHmmss");
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_MachineCondition sbMonitor = new Json_I_SB_Monitor_MachineCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    sbzt = "00",
                    sbsj = nowTime
                };
                var result = new TaiJiHelper().Do_SB_Monitor_MachineCondition(sbMonitor);
                Log.Instance.WriteInfo(result ? "上传设备状态关机【成功】" : "上传设备状态关机【失败】");
            }

            return true;
        }

        /// <summary>
        /// 上传太极设备运行状态
        /// </summary>
        /// <returns></returns>
        public bool UploadRunningCondition()
        {
            var nowTime = Convert.ToDateTime(ZHPHMachineWSHelper.ZHPHInstance.S_Sysdate()).ToString("yyyyMMddHHmmss");
            if (nowTime.IsNotEmpty())
            {
                Json_I_SB_Monitor_RunningCondition sbMonitor = new Json_I_SB_Monitor_RunningCondition
                {
                    sbid = TjConfig.TjModel.TJMachineNo,
                    sbzl = ((int)DM_SBZL.ZL17).ToString(),
                    yxzt = "00",
                    sbsj = nowTime
                };
                var result=new TaiJiHelper().Do_SB_Monitor_RunningCondition(sbMonitor);
                Log.Instance.WriteInfo(result ? "上传设备运行状态【成功】" : "上传设备运行状态【失败】");
            }

            return true;
        }

        /// <summary>
        /// 全屏
        /// </summary>
        public void ToFullscreen()
        {
            //存储窗体信息
            //m_WindowState = this.WindowState;
            //m_WindowStyle = this.WindowStyle;
            //m_WindowTopMost = this.Topmost;
            //m_WindowResizeMode = this.ResizeMode;
            //m_WindowRect.X = this.Left;
            //m_WindowRect.Y = this.Top;
            //m_WindowRect.Width = this.Width;
            //m_WindowRect.Height = this.Height;

            //变成无边窗体
            this.WindowState = WindowState.Normal;//假如已经是Maximized，就不能进入全屏，所以这里先调整状态
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;//最大化后总是在最上面

            // 调整窗口最大化。
            //this.Width = m_DisplayBounds.width;
            //this.Height = m_DisplayBounds.height;
            this.WindowState = WindowState.Maximized;

        }


        private void win_Loaded(object sender, RoutedEventArgs e)
        {
            //TopMostTool.GoFullscreen(this);
            // 设置全屏  
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            //this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }
    }
}
